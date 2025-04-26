namespace PSInterpreter
{
    using PSInterpreter.Constants;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net.NetworkInformation;
    using Unity.VisualScripting;
    using UnityEngine;
    using UnityEngine.Windows;

    public static class Interpreter
    {
        public delegate Constant Parser(string input);

        public static event Action<string> DisplayToConsole;

        private static Stack<Constant> opStack = new Stack<Constant>();

        private static List<Dictionary<string, Constant>> dictStack = new List<Dictionary<string, Constant>>();

        private static List<Parser> parsers = new List<Parser>();

        static Interpreter()
        {
            parsers.Add(ProcessNumeric);
            parsers.Add(ProcessBoolean);
            InitializeDict();
        }

        /// <summary>
        /// Creates system dictionary and populates with operators.
        /// </summary>
        private static void InitializeDict()
        {
            // initializes primary dictionary to be populated in program
            dictStack.Add(new Dictionary<string, Constant>());

            // initializes the system dictionary with operators
            dictStack[0]["exch"] = new OperationConstant(ExchangeOperation);
            dictStack[0]["pop"] = new OperationConstant(PopOperation);
            dictStack[0]["copy"] = new OperationConstant(CopyOperation);
            dictStack[0]["dup"] = new OperationConstant(DuplicateOperation);
            dictStack[0]["clear"] = new OperationConstant(ClearOperation);
            dictStack[0]["count"] = new OperationConstant(CountOperation);

            dictStack[0]["add"] = new OperationConstant(AdditionOperation);
            dictStack[0]["sub"] = new OperationConstant(SubtractionOperation);
            dictStack[0]["mul"] = new OperationConstant(MultiplicationOperation);
            dictStack[0]["div"] = new OperationConstant(DivisionOperation);
            dictStack[0]["mod"] = new OperationConstant(ModularDivisionOperation);
            dictStack[0]["idiv"] = new OperationConstant(IntegerDivisionOperation);

            dictStack[0]["="] = new OperationConstant(PopAndDisplayOperation);
        }

        public static void Reset()
        {
            opStack.Clear();
            dictStack.Clear();
            InitializeDict();
        }

        public static int StackCount()
        {
            return opStack.Count;
        }

        public static Constant PeekStack()
        {
            return opStack.Peek();
        }

        /// <summary>
        /// Attempts to process input into a constant or operation.
        /// </summary>
        /// <param name="input">String to process.</param>
        public static void ProcessInput(string input)
        {
            try
            {
                Debug.Log($"Processing '{input}'");
                ProcessConstants(input);
            }
            catch
            {
                Debug.Log($"Could not process '{input}' as a constant");
                try
                {
                    LookupInDict(input);
                }
                catch (Exception ex)
                {
                    DisplayToConsole?.Invoke(ex.Message);
                }
            }
        }

        /// <summary>
        /// Attempts to parse input using each constant parser.
        /// </summary>
        /// <param name="input">String to parse.</param>
        /// <exception cref="Exception">Input could not be parsed as a constant.</exception>
        private static void ProcessConstants(string input)
        {
            // attempts to parse input using each parser
            foreach (Parser parser in parsers)
            {
                try
                {
                    Constant res = parser(input);
                    opStack.Push(res);
                    return;
                }
                catch   // if parsing fails, try next parser
                {
                    continue;
                }
            }
            throw new Exception("Could not process as constant");
        }

        private static void LookupInDict(string input)
        {
            Debug.Log("Looking up " + input + " in dictStack(" + dictStack.Count + ")");
            foreach (Dictionary<string, Constant> dict in dictStack)
            {
                foreach (KeyValuePair<string, Constant> variable in dict)
                {
                    Debug.Log("Key: " + variable.Key + ", Value: " + variable.Value);
                    if (input == variable.Key && variable.Value is OperationConstant)
                    {
                        Debug.Log("Found " + input + " in dictStack. Executing");
                        OperationConstant op = (OperationConstant) variable.Value;
                        op.Value();
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Input could not be parsed as a numeric constant.</exception>
        private static Constant ProcessNumeric(string input)
        {
            Debug.Log($"Attempting to parse '{input}' into numeric");

            if (float.TryParse(input, out float floatResult))
            {
                return ProcessNumericFromFloat(floatResult);
            }
            throw new Exception("Could not parse " + input + " into numeric");
        }

        private static Constant ProcessNumericFromFloat(float input)
        {
            if (input == (float)(int)input) // if float is actually an integer
            {
                Debug.Log($"Parsed '{input}' into integer");
                return new IntegerConstant((int)input);
            }
            else
            {
                Debug.Log($"Parsed '{input}' into float");
                return new FloatConstant(input);
            }
        }

        private static Constant ProcessBoolean(string input)
        {
            Debug.Log($"Attempting to parse '{input}' into boolean");

            if (input == "true")
            {
                Debug.Log($"Parsed '{input}' into boolean");
                return new BooleanConstant(true);
            }
            else if (input == "false")
            {
                Debug.Log($"Parsed '{input}' into boolean");
                return new BooleanConstant(false);
            }
            throw new Exception("Could not parse " + input + " into boolean");
        }

        #region STACK_MANIPULATION_OPERATIONS

        /// <summary>
        /// Defines "exch" exchange top two elements operation.
        /// </summary>
        private static void ExchangeOperation()
        {
            if (StackCount() >= 2)
            {
                Constant val1 = opStack.Pop();
                Constant val2 = opStack.Pop();

                opStack.Push(val1);
                opStack.Push(val2);
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "pop" pop stack operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void PopOperation()
        {
            if (StackCount() >= 1)
            {
                opStack.Pop();
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "copy" operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void CopyOperation()
        {
            if (StackCount() >= 1)
            {
                List<Constant> constantsCopied = new List<Constant>();
                Constant numCopy = opStack.Pop();
                if (numCopy is IntegerConstant integerConstant)
                {
                    // don't attempt to copy more than the stack
                    int numToCopy = integerConstant.Value < StackCount() ? integerConstant.Value : StackCount();
                    for (int i = 0; i < numToCopy; i++)
                    {
                        constantsCopied.Add(opStack.Pop());
                    }

                    constantsCopied.Reverse();

                    // add constants back that were removed
                    foreach (Constant constant in constantsCopied)
                    {
                        opStack.Push(constant);
                    }

                    // add copies to top of the stack
                    foreach (Constant constant in constantsCopied)
                    {
                        opStack.Push(constant);
                    }
                }
                else
                {
                    throw new Exception("Type is not supported in copy operation");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "dup" duplication operation
        /// </summary>
        private static void DuplicateOperation()
        {
            if (StackCount() >= 1)
            {
                opStack.Push(opStack.Peek());
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "clear" operation.
        /// </summary>
        private static void ClearOperation()
        {
            opStack.Clear();
        }

        /// <summary>
        /// Defines "count" operation.
        /// </summary>
        private static void CountOperation()
        {
            opStack.Push(new IntegerConstant(StackCount()));
        }

        #endregion

        #region ARITHMETIC_OPERATIONS

        /// <summary>
        /// Defines "add" addition operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void AdditionOperation()
        {
            if (StackCount() >= 2)
            {
                Constant val2 = opStack.Pop();
                Constant val1 = opStack.Pop();

                switch ((val1, val2))
                {
                    case (FloatConstant val1Float, FloatConstant val2Float):
                        opStack.Push(ProcessNumericFromFloat((val1Float.Value + val2Float.Value)));
                        break;

                    case (FloatConstant val1Float, IntegerConstant val2Int):
                        opStack.Push(ProcessNumericFromFloat((val1Float.Value + val2Int.Value)));
                        break;

                    case (IntegerConstant val1Int, FloatConstant val2Float):
                        opStack.Push(ProcessNumericFromFloat((val1Int.Value + val2Float.Value)));
                        break;

                    case (IntegerConstant val1Int, IntegerConstant val2Int):
                        opStack.Push(ProcessNumericFromFloat((val1Int.Value + val2Int.Value)));
                        break;

                    default:
                        opStack.Push(val1);
                        opStack.Push(val2);
                        throw new Exception("Type is not supported in add operation");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "sub" subtraction operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void SubtractionOperation()
        {
            if (StackCount() >= 2)
            {
                Constant val2 = opStack.Pop();
                Constant val1 = opStack.Pop();

                switch ((val1, val2))
                {
                    case (FloatConstant val1Float, FloatConstant val2Float):
                        opStack.Push(ProcessNumericFromFloat((val1Float.Value - val2Float.Value)));
                        break;

                    case (FloatConstant val1Float, IntegerConstant val2Int):
                        opStack.Push(ProcessNumericFromFloat((val1Float.Value - val2Int.Value)));
                        break;

                    case (IntegerConstant val1Int, FloatConstant val2Float):
                        opStack.Push(ProcessNumericFromFloat((val1Int.Value - val2Float.Value)));
                        break;

                    case (IntegerConstant val1Int, IntegerConstant val2Int):
                        opStack.Push(ProcessNumericFromFloat((val1Int.Value - val2Int.Value)));
                        break;

                    default:
                        opStack.Push(val1);
                        opStack.Push(val2);
                        throw new Exception("Type is not supported in sub operation");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "mul" multiplication operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void MultiplicationOperation()
        {
            if (StackCount() >= 2)
            {
                Constant val2 = opStack.Pop();
                Constant val1 = opStack.Pop();

                switch ((val1, val2))
                {
                    case (FloatConstant val1Float, FloatConstant val2Float):
                        opStack.Push(ProcessNumericFromFloat((val1Float.Value * val2Float.Value)));
                        break;

                    case (FloatConstant val1Float, IntegerConstant val2Int):
                        opStack.Push(ProcessNumericFromFloat((val1Float.Value * val2Int.Value)));
                        break;

                    case (IntegerConstant val1Int, FloatConstant val2Float):
                        opStack.Push(ProcessNumericFromFloat((val1Int.Value * val2Float.Value)));
                        break;

                    case (IntegerConstant val1Int, IntegerConstant val2Int):
                        opStack.Push(ProcessNumericFromFloat((val1Int.Value * val2Int.Value)));
                        break;

                    default:
                        opStack.Push(val1);
                        opStack.Push(val2);
                        throw new Exception("Type is not supported in mul operation");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "div" division operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void DivisionOperation()
        {
            if (StackCount() >= 2)
            {
                Constant val2 = opStack.Pop();
                Constant val1 = opStack.Pop();

                switch ((val1, val2))
                {
                    case (FloatConstant val1Float, FloatConstant val2Float):
                        opStack.Push(ProcessNumericFromFloat((val1Float.Value / val2Float.Value)));
                        break;

                    case (FloatConstant val1Float, IntegerConstant val2Int):
                        if (val2Int.Value == 0)
                        {
                            opStack.Push(val1Float);
                            opStack.Push(val2Int);
                            throw new Exception("Cannot divide by zero");
                        }
                        else
                        {
                            opStack.Push(ProcessNumericFromFloat((val1Float.Value / val2Int.Value)));
                        }
                        break;

                    case (IntegerConstant val1Int, FloatConstant val2Float):
                        opStack.Push(ProcessNumericFromFloat((val1Int.Value / val2Float.Value)));
                        break;

                    case (IntegerConstant val1Int, IntegerConstant val2Int):
                        if (val2Int.Value == 0)
                        {
                            opStack.Push(val1Int);
                            opStack.Push(val2Int);
                            throw new Exception("Cannot divide by zero");
                        }
                        else
                        {
                            opStack.Push(ProcessNumericFromFloat(((float)val1Int.Value / val2Int.Value)));
                        }
                        break;

                    default:
                        opStack.Push(val1);
                        opStack.Push(val2);
                        throw new Exception("Type is not supported in div operation");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "mod" modular division operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void ModularDivisionOperation()
        {
            if (StackCount() >= 2)
            {
                Constant val2 = opStack.Pop();
                Constant val1 = opStack.Pop();

                switch ((val1, val2))
                {
                    case (IntegerConstant val1Int, IntegerConstant val2Int):
                        opStack.Push(ProcessNumericFromFloat((val1Int.Value % val2Int.Value)));
                        break;

                    default:
                        opStack.Push(val1);
                        opStack.Push(val2);
                        throw new Exception("Type is not supported in mod operation");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        private static void IntegerDivisionOperation()
        {
            if (StackCount() >= 2)
            {
                Constant val2 = opStack.Pop();
                Constant val1 = opStack.Pop();

                switch ((val1, val2))
                {
                    case (IntegerConstant val1Int, IntegerConstant val2Int):
                        opStack.Push(ProcessNumericFromFloat((int)(val1Int.Value / val2Int.Value)));
                        break;

                    default:
                        opStack.Push(val1);
                        opStack.Push(val2);
                        throw new Exception("Type is not supported in idiv operation");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        #endregion

        /// <summary>
        /// Defines "=" pop and display operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void PopAndDisplayOperation()
        {
            if (StackCount() >= 1)
            {
                Constant constant = opStack.Pop();

                if (constant is BooleanConstant bc)
                    DisplayToConsole(bc.Value.ToString());
                else if (constant is IntegerConstant ic)
                    DisplayToConsole(ic.Value.ToString());
                else if (constant is FloatConstant fc)
                    DisplayToConsole(fc.Value.ToString());
                else
                    throw new Exception("Unable to display constant type");
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }
    }
}
