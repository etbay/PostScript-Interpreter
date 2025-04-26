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
            dictStack[0]["add"] = new OperationConstant(AdditionOperation);
            dictStack[0]["sub"] = new OperationConstant(SubtractionOperation);
            dictStack[0]["mul"] = new OperationConstant(MultiplicationOperation);
            dictStack[0]["div"] = new OperationConstant(DivisionOperation);
            dictStack[0]["mod"] = new OperationConstant(ModularDivisionOperation);
            dictStack[0]["="] = new OperationConstant(PopAndDisplay);
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
                    DisplayToConsole.Invoke(ex.Message);
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
                        throw new Exception("Type is not supported in add operation");
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
                        throw new Exception("Type is not supported in add operation");
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
                        opStack.Push(ProcessNumericFromFloat((val1Float.Value / val2Int.Value)));
                        break;

                    case (IntegerConstant val1Int, FloatConstant val2Float):
                        opStack.Push(ProcessNumericFromFloat((val1Int.Value / val2Float.Value)));
                        break;

                    case (IntegerConstant val1Int, IntegerConstant val2Int):
                        opStack.Push(ProcessNumericFromFloat((val1Int.Value / val2Int.Value)));
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
                        throw new Exception("Type is not supported in add operation");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "=" pop and display operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void PopAndDisplay()
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
        }
    }
}
