namespace PSInterpreter
{
    using PSInterpreter.Constants;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net.NetworkInformation;
    using System.Text;
    using Unity.VisualScripting;
    using UnityEngine;
    using UnityEngine.UIElements;
    using UnityEngine.Windows;

    public static class Interpreter
    {
        public delegate Constant Parser(string input);

        public static event Action<string> DisplayToConsole;

        private static Stack<Constant> opStack = new Stack<Constant>();

        private static List<Dictionary<string, Constant>> dictStack = new List<Dictionary<string, Constant>>();

        private static List<Parser> parsers = new List<Parser>();

        private static bool dynamicScoping = true;

        static Interpreter()
        {
            parsers.Add(ProcessNumeric);
            parsers.Add(ProcessBoolean);
            parsers.Add(ProcessString);
            parsers.Add(ProcessCodeBlock);
            parsers.Add(ProcessVariable);
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
            dictStack[0]["changescoping"] = new OperationConstant(ChangeScopingOperation);

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
            dictStack[0]["abs"] = new OperationConstant(AbsoluteValueOperation);
            dictStack[0]["neg"] = new OperationConstant(NegateOperation);
            dictStack[0]["ceil"] = new OperationConstant(CeilingOperation);
            dictStack[0]["floor"] = new OperationConstant(FloorOperation);
            dictStack[0]["round"] = new OperationConstant(RoundOperation);
            dictStack[0]["sqrt"] = new OperationConstant(SquareRootOperation);

            dictStack[0]["dict"] = new OperationConstant(DictionaryOperation);
            dictStack[0]["length"] = new OperationConstant(LengthOperation);
            dictStack[0]["maxlength"] = new OperationConstant(MaxLengthOperation);
            dictStack[0]["begin"] = new OperationConstant(BeginOperation);
            dictStack[0]["end"] = new OperationConstant(EndOperation);
            dictStack[0]["def"] = new OperationConstant(DefinitionOperation);

            dictStack[0]["length"] = new OperationConstant(LengthOperation);
            dictStack[0]["get"] = new OperationConstant(GetOperation);
            dictStack[0]["getinterval"] = new OperationConstant(GetIntervalOperation);
            dictStack[0]["puinterval"] = new OperationConstant(PutIntervalOperation);

            dictStack[0]["eq"] = new OperationConstant(EqualToOperation);
            dictStack[0]["ne"] = new OperationConstant(NotEqualToOperation);
            dictStack[0]["gt"] = new OperationConstant(GreaterThanOperation);
            dictStack[0]["lt"] = new OperationConstant(LessThanOperation);
            dictStack[0]["and"] = new OperationConstant(AndOperation);
            dictStack[0]["or"] = new OperationConstant(OrOperation);
            dictStack[0]["not"] = new OperationConstant(NotOperation);

            dictStack[0]["if"] = new OperationConstant(IfOperation);
            dictStack[0]["ifelse"] = new OperationConstant(IfElseOperation);
            dictStack[0]["for"] = new OperationConstant(ForOperation);
            dictStack[0]["repeat"] = new OperationConstant(RepeatOperation);
            dictStack[0]["quit"] = new OperationConstant(QuitOperation);

            dictStack[0]["print"] = new OperationConstant(PopAndDisplayOperation);
            dictStack[0]["="] = new OperationConstant(PopAndDisplayOperation);
            dictStack[0]["=="] = new OperationConstant(PopAndDisplayOperation);
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
            if (hasQuit)
                return;

            List<string> inputs = SplitString(input);

            foreach (string i in inputs)
            {
                Debug.Log($"Processing '{i}'");
                try
                {
                    ProcessConstants(i);
                }
                catch
                {
                    Debug.Log($"Could not process '{i}' as a constant");
                    try
                    {
                        LookupInDict(i);
                    }
                    catch (Exception ex)
                    {
                        DisplayToConsole?.Invoke(ex.Message);
                    }
                }
                if (hasQuit)
                    return;
            }
        }

        /// <summary>
        /// Processes input for lexical scoping.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="dict"></param>
        public static void ProcessInput(string input, Dictionary<string, Constant> dict)
        {
            if (hasQuit)
                return;

            List<string> inputs = SplitString(input);

            foreach (string i in inputs)
            {
                Debug.Log($"Processing '{i}'");
                try
                {
                    ProcessConstants(i);
                }
                catch
                {
                    Debug.Log($"Could not process '{i}' as a constant");
                    try
                    {
                        LookupInDict(i, dict);
                    }
                    catch (Exception ex)
                    {
                        DisplayToConsole?.Invoke(ex.Message);
                    }
                }
                if (hasQuit)
                    return;
            }
        }

        /// <summary>
        /// Splits string into tokens while recognizing code blocks.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static List<string> SplitString(string input)
        {
            List<string> splitInput = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> newInput = new List<string>();
            StringBuilder codeBlock = new StringBuilder();
            StringBuilder str = new StringBuilder();
            bool inCodeBlock = false;
            bool inString = false;
            int numBraceOpen = 0;

            foreach (string i in splitInput)
            {
                Debug.Log($"Processing split {i}");
                if (i.StartsWith("{") && !inString)
                {
                    numBraceOpen++;
                    inCodeBlock = true;
                    codeBlock.Append(i + " ");
                }
                else if (i.EndsWith("}") && !inString)
                {
                    numBraceOpen--;
                    if (numBraceOpen == 0)
                    {
                        inCodeBlock = false;
                        codeBlock.Append(i);
                        newInput.Add(codeBlock.ToString());
                        codeBlock.Clear();
                    }
                    else
                    {
                        codeBlock.Append(i + " ");
                    }
                }
                else if (inCodeBlock && !inString)
                {
                    codeBlock.Append(i + " ");
                }
                else if (i.StartsWith("(") && !i.EndsWith(")"))
                {
                    Debug.Log("Started string");
                    inString = true;
                    str.Append(i + " ");
                }
                else if (i.EndsWith(")") && !i.StartsWith("("))
                {
                    Debug.Log("Ended string");
                    inString = false;
                    str.Append(i);
                    newInput.Add(str.ToString());
                    str.Clear();
                }
                else if (inString && !inCodeBlock)
                {
                    str.Append(i + " ");
                }
                else
                {
                    newInput.Add(i);
                }
            }

            if (inCodeBlock)
            {
                codeBlock.Append('}');
                newInput.Add(codeBlock.ToString() + " ");
            }
            else if (inString)
            {
                str.Append(')');
                newInput.Add(str.ToString() + " ");
            }

            foreach (string i in newInput)
            {
                Debug.Log("New input " + i);
            }

            return newInput;
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
            Debug.Log($"Looking up '{input}' in dictStack({dictStack.Count})");
            dictStack.Reverse();
            foreach (Dictionary<string, Constant> dict in dictStack)
            {
                foreach (KeyValuePair<string, Constant> variable in dict)
                {
                    if (input == variable.Key)
                    {
                        if (variable.Value is OperationConstant op)
                        {
                            Debug.Log($"Found '{input}' in dictStack. Executing");
                            op.Value();
                            dictStack.Reverse();
                            return;
                        }
                        else if (variable.Value is CodeBlockConstant cbc)
                        {
                            if (dynamicScoping)
                            {
                                ProcessInput(cbc.Value);
                                dictStack.Reverse();
                                return;
                            }
                            else
                            {
                                ProcessInput(cbc.Value, dict);
                                dictStack.Reverse();
                                return;
                            }
                        }
                        else
                        {
                            opStack.Push(variable.Value);
                            dictStack.Reverse();
                            return;
                        }
                    }
                }
            }
            dictStack.Reverse();
            throw new Exception($"Operation '{input}' not found");
        }

        private static void LookupInDict(string input, Dictionary<string, Constant> dict)
        {
            Debug.Log($"Looking up '{input}' in dictStack({dictStack.Count})");
            dictStack.Reverse();
            foreach (KeyValuePair<string, Constant> variable in dict)
            {
                if (input == variable.Key)
                {
                    if (variable.Value is OperationConstant op)
                    {
                        Debug.Log($"Found '{input}' in dictStack. Executing");
                        op.Value();
                        dictStack.Reverse();
                        return;
                    }
                    else if (variable.Value is CodeBlockConstant cbc)
                    {
                        if (dynamicScoping)
                        {
                            ProcessInput(cbc.Value);
                            dictStack.Reverse();
                            return;
                        }
                        else
                        {
                            ProcessInput(cbc.Value, dict);
                            dictStack.Reverse();
                            return;
                        }
                    }
                    else
                    {
                        opStack.Push(variable.Value);
                        dictStack.Reverse();
                        return;
                    }
                }
            }
            dictStack.Reverse();
            throw new Exception($"Operation '{input}' not found");
        }

        /// <summary>
        /// Defines "changescoping" operation.
        /// Changes from dynamic to lexical and lexical to dynamic scoping.
        /// </summary>
        private static void ChangeScopingOperation()
        {
            dynamicScoping = !dynamicScoping;
            Debug.Log("Dynamic scoping is: " + dynamicScoping.ToString());
        }

        #region PARSERS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Input could not be parsed as a numeric constant.</exception>
        private static Constant ProcessNumeric(string input)
        {
            Debug.Log($"Attempting to parse '{input}' into numeric");
            if (float.TryParse(input, out float floatResult) || int.TryParse(input, out int intResult))
            {

                if (input.Contains('.'))
                {
                    Debug.Log($"Parsed '{input}' into float");
                    return new FloatConstant(floatResult);
                }
                else
                {
                    Debug.Log($"Parsed '{input}' into integer");
                    return new IntegerConstant((int)floatResult);
                }
            }
            throw new Exception("Could not parse " + input + " into numeric");
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

        private static Constant ProcessString(string input)
        {
            Debug.Log($"Attempting to parse '{input}' into string");
            if (input.StartsWith('(') && input.EndsWith(')'))
            {
                Debug.Log($"Parsed '{input}' into string");
                return new StringConstant(input.Substring(1, input.Length - 2));
            }
            throw new Exception("Could not parse " + input + " into string");
        }

        private static Constant ProcessCodeBlock(string input)
        {
            Debug.Log($"Attempting to parse '{input}' into code block");
            if (input.StartsWith('{') && input.EndsWith('}'))
            {
                Debug.Log($"Parsed '{input}' into code block");
                return new CodeBlockConstant(input.Substring(1, input.Length - 2).Trim());
            }
            throw new Exception("Could not parse " + input + " into code block");
        }

        private static Constant ProcessVariable(string input)
        {
            Debug.Log($"Attempting to parse '{input}' into variable");
            if (input.StartsWith('/'))
            {
                Debug.Log($"Parsed '{input}' into variable");
                return new VariableConstant(input.Substring(1));
            }
            throw new Exception("Could not parse " + input + " into variable");
        }

        #endregion

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
                        opStack.Push(new FloatConstant((val1Float.Value + val2Float.Value)));
                        break;

                    case (FloatConstant val1Float, IntegerConstant val2Int):
                        opStack.Push(new FloatConstant((val1Float.Value + val2Int.Value)));
                        break;

                    case (IntegerConstant val1Int, FloatConstant val2Float):
                        opStack.Push(new FloatConstant((val1Int.Value + val2Float.Value)));
                        break;

                    case (IntegerConstant val1Int, IntegerConstant val2Int):
                        opStack.Push(new IntegerConstant((val1Int.Value + val2Int.Value)));
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
                        opStack.Push(new FloatConstant((val1Float.Value - val2Float.Value)));
                        break;

                    case (FloatConstant val1Float, IntegerConstant val2Int):
                        opStack.Push(new FloatConstant((val1Float.Value - val2Int.Value)));
                        break;

                    case (IntegerConstant val1Int, FloatConstant val2Float):
                        opStack.Push(new FloatConstant((val1Int.Value - val2Float.Value)));
                        break;

                    case (IntegerConstant val1Int, IntegerConstant val2Int):
                        opStack.Push(new IntegerConstant((val1Int.Value - val2Int.Value)));
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
                        opStack.Push(new FloatConstant((val1Float.Value * val2Float.Value)));
                        break;

                    case (FloatConstant val1Float, IntegerConstant val2Int):
                        opStack.Push(new FloatConstant((val1Float.Value * val2Int.Value)));
                        break;

                    case (IntegerConstant val1Int, FloatConstant val2Float):
                        opStack.Push(new FloatConstant((val1Int.Value * val2Float.Value)));
                        break;

                    case (IntegerConstant val1Int, IntegerConstant val2Int):
                        opStack.Push(new IntegerConstant((val1Int.Value * val2Int.Value)));
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
                        opStack.Push(new FloatConstant((val1Float.Value / val2Float.Value)));
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
                            opStack.Push(new FloatConstant((val1Float.Value / val2Int.Value)));
                        }
                        break;

                    case (IntegerConstant val1Int, FloatConstant val2Float):
                        opStack.Push(new FloatConstant((val1Int.Value / val2Float.Value)));
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
                            opStack.Push(new FloatConstant(((float)val1Int.Value / val2Int.Value)));
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
                        opStack.Push(new IntegerConstant((val1Int.Value % val2Int.Value)));
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

        /// <summary>
        /// Defines "idiv" integer division operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void IntegerDivisionOperation()
        {
            if (StackCount() >= 2)
            {
                Constant val2 = opStack.Pop();
                Constant val1 = opStack.Pop();

                switch ((val1, val2))
                {
                    case (IntegerConstant val1Int, IntegerConstant val2Int):
                        opStack.Push(new IntegerConstant((int)(val1Int.Value / val2Int.Value)));
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

        /// <summary>
        /// Defines "abs" absolute value operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void AbsoluteValueOperation()
        {
            if (StackCount() >= 1)
            {
                Constant val = opStack.Pop();

                switch (val)
                {
                    case (IntegerConstant valInt):
                        opStack.Push(new IntegerConstant(Math.Abs(valInt.Value)));
                        break;

                    case (FloatConstant valFloat):
                        opStack.Push(new FloatConstant(Math.Abs(valFloat.Value)));
                        break;

                    default:
                        opStack.Push(val);
                        throw new Exception("Type is not supported in abs operation");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "neg" negate operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void NegateOperation()
        {
            if (StackCount() >= 1)
            {
                Constant val = opStack.Pop();

                switch (val)
                {
                    case (IntegerConstant valInt):
                        opStack.Push(new IntegerConstant(-valInt.Value));
                        break;

                    case (FloatConstant valFloat):
                        opStack.Push(new FloatConstant(-valFloat.Value));
                        break;

                    default:
                        opStack.Push(val);
                        throw new Exception("Type is not supported in negate operation");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "ceiling" operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void CeilingOperation()
        {
            if (StackCount() >= 1)
            {
                Constant val = opStack.Pop();

                switch (val)
                {
                    case (IntegerConstant valInt):
                        opStack.Push(valInt);
                        break;

                    case (FloatConstant valFloat):
                        opStack.Push(new FloatConstant((float)Math.Ceiling(valFloat.Value)));
                        break;

                    default:
                        opStack.Push(val);
                        throw new Exception("Type is not supported in ceiling operation");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "floor" operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void FloorOperation()
        {
            if (StackCount() >= 1)
            {
                Constant val = opStack.Pop();

                switch (val)
                {
                    case (IntegerConstant valInt):
                        opStack.Push(valInt);
                        break;

                    case (FloatConstant valFloat):
                        opStack.Push(new FloatConstant((float)Math.Floor(valFloat.Value)));
                        break;

                    default:
                        opStack.Push(val);
                        throw new Exception("Type is not supported in floor operation");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "round" operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void RoundOperation()
        {
            if (StackCount() >= 1)
            {
                Constant val = opStack.Pop();

                switch (val)
                {
                    case (IntegerConstant valInt):
                        opStack.Push(valInt);
                        break;

                    case (FloatConstant valFloat):
                        opStack.Push(new FloatConstant((float)Math.Round(valFloat.Value)));
                        break;

                    default:
                        opStack.Push(val);
                        throw new Exception("Type is not supported in round operation");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "sqrt" square root operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void SquareRootOperation()
        {
            if (StackCount() >= 1)
            {
                Constant val = opStack.Pop();

                switch (val)
                {
                    case (IntegerConstant valInt):
                        opStack.Push(new IntegerConstant((int)Math.Sqrt(valInt.Value)));
                        break;

                    case (FloatConstant valFloat):
                        opStack.Push(new FloatConstant((float)Math.Sqrt(valFloat.Value)));
                        break;

                    default:
                        opStack.Push(val);
                        throw new Exception("Type is not supported in square root operation");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        #endregion

        #region DICTIONARY_OPERATIONS

        /// <summary>
        /// Defines "dict" operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void DictionaryOperation()
        {
            if (StackCount() >= 1)
            {
                Constant val = opStack.Pop();

                switch (val)
                {
                    case (IntegerConstant valInt):
                        opStack.Push(new DictionaryConstant(valInt.Value));
                        break;

                    default:
                        opStack.Push(val);
                        throw new Exception("Type is not supported in dict operation");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "length" string operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void LengthOperation()
        {
            if (StackCount() >= 1)
            {
                Constant val = opStack.Pop();

                switch (val)
                {
                    case (StringConstant valString):
                        opStack.Push(new IntegerConstant(valString.Value.Length));
                        break;

                    case (DictionaryConstant valDict):
                        opStack.Push(new IntegerConstant(valDict.Value.Count));
                        break;

                    default:
                        opStack.Push(val);
                        throw new Exception($"Unsupported type for length operation: {val.GetType()}");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "maxlength" dictionary operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void MaxLengthOperation()
        {
            if (StackCount() >= 1)
            {
                Constant val = opStack.Pop();

                switch (val)
                {
                    case (DictionaryConstant valDict):
                        opStack.Push(new IntegerConstant(valDict.MaxSize));
                        break;

                    default:
                        opStack.Push(val);
                        throw new Exception("Type is not supported in begin operation");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "begin" dictionary operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void BeginOperation()
        {
            if (StackCount() >= 1)
            {
                Constant val = opStack.Pop();

                switch (val)
                {
                    case (DictionaryConstant valDict):
                        dictStack.Add(valDict.Value);
                        break;

                    default:
                        opStack.Push(val);
                        throw new Exception("Type is not supported in begin operation");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "end" dictionary operation.
        /// </summary>
        private static void EndOperation()
        {
            if (dictStack.Count >= 2)
            {
                dictStack.Remove(dictStack[dictStack.Count - 1]);
            }
        }

        /// <summary>
        /// Defines "def" definition operation.
        /// </summary>
        private static void DefinitionOperation()
        {
            if (StackCount() >= 2)
            {
                Constant val2 = opStack.Pop();
                Constant val1 = opStack.Pop();

                switch ((val1, val2))
                {
                    case (VariableConstant val1Variable, BooleanConstant val2Bool):
                        dictStack[dictStack.Count - 1][val1Variable.Value] = val2Bool;
                        break;

                    case (VariableConstant val1Variable, CodeBlockConstant val2CodeBlock):
                        dictStack[dictStack.Count - 1][val1Variable.Value] = val2CodeBlock;
                        break;

                    case (VariableConstant val1Variable, FloatConstant val2Float):
                        dictStack[dictStack.Count - 1][val1Variable.Value] = val2Float;
                        break;

                    case (VariableConstant val1Variable, IntegerConstant val2Int):
                        dictStack[dictStack.Count - 1][val1Variable.Value] = val2Int;
                        break;

                    case (VariableConstant val1Variable, StringConstant val2String):
                        dictStack[dictStack.Count - 1][val1Variable.Value] = val2String;
                        break;

                    case (VariableConstant val1Variable, DictionaryConstant val2Dictionary):
                        dictStack[dictStack.Count - 1][val1Variable.Value] = val2Dictionary;
                        break;

                    default:
                        opStack.Push(val1);
                        opStack.Push(val2);
                        throw new Exception($"Unsupported types for definition operation: {val1.GetType()} and {val2.GetType()}");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        #endregion

        #region STRING_OPERATIONS

        /// <summary>
        /// Defines "get" string operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void GetOperation()
        {
            if (StackCount() >= 2)
            {
                Constant val2 = opStack.Pop();
                Constant val1 = opStack.Pop();

                switch ((val1, val2))
                {
                    case (StringConstant val1String, IntegerConstant val2Int):
                        opStack.Push(new IntegerConstant(val1String.Value[val2Int.Value]));
                        break;

                    default:
                        opStack.Push(val1);
                        opStack.Push(val2);
                        throw new Exception($"Unsupported types for get operation: {val1.GetType()} and {val2.GetType()}");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "getinterval" string operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void GetIntervalOperation()
        {
            if (StackCount() >= 3)
            {
                Constant val3 = opStack.Pop();
                Constant val2 = opStack.Pop();
                Constant val1 = opStack.Pop();

                switch ((val1, val2, val3))
                {
                    case (StringConstant val1String, IntegerConstant val2Int, IntegerConstant val3Int):
                        opStack.Push(new StringConstant(val1String.Value.Substring(val2Int.Value, val3Int.Value)));
                        break;

                    default:
                        opStack.Push(val1);
                        opStack.Push(val2);
                        opStack.Push(val3);
                        throw new Exception($"Unsupported types for getinterval operation: {val1.GetType()}, {val2.GetType()}, and {val3.GetType()}");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "putinterval" string operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void PutIntervalOperation()
        {
            if (StackCount() >= 3)
            {
                Constant val3 = opStack.Pop();
                Constant val2 = opStack.Pop();
                Constant val1 = opStack.Pop();

                switch ((val1, val2, val3))
                {
                    case (StringConstant val1String, IntegerConstant val2Int, StringConstant val3String):
                        if (val2Int.Value < 0 || val2Int.Value + val3String.Value.Length > val1String.Value.Length)
                            throw new Exception("Putinterval failed: source string too large for destination");
                        opStack.Push(new StringConstant(val1String.Value.Substring(0, val2Int.Value) 
                                                        + val3String.Value 
                                                        + val1String.Value.Substring(val2Int.Value + val3String.Value.Length)));
                        break;

                    default:
                        opStack.Push(val1);
                        opStack.Push(val2);
                        opStack.Push(val3);
                        throw new Exception($"Unsupported types for putinterval operation: {val1.GetType()}, {val2.GetType()}, and {val3.GetType()}");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        #endregion

        #region BOOLEAN_OPERATIONS

        /// <summary>
        /// Defines "eq" equal to operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void EqualToOperation()
        {
            if (StackCount() >= 2)
            {
                Constant val2 = opStack.Pop();
                Constant val1 = opStack.Pop();

                switch ((val1, val2))
                {
                    case (StringConstant val1String, StringConstant val2String):
                        opStack.Push(new BooleanConstant((val1String.Value == val2String.Value)));
                        break;

                    case (FloatConstant val1Float, IntegerConstant val2Int):
                        opStack.Push(new BooleanConstant(Math.Abs(val1Float.Value - val2Int.Value) < 0.0001f));
                        break;

                    case (IntegerConstant val1Int, FloatConstant val2Float):
                        opStack.Push(new BooleanConstant(Math.Abs(val1Int.Value - val2Float.Value) < 0.0001f));
                        break;

                    case (IntegerConstant val1Int, IntegerConstant val2Int):
                        opStack.Push(new BooleanConstant((val1Int.Value == val2Int.Value)));
                        break;

                    case (BooleanConstant val1Bool, BooleanConstant val2Bool):
                        opStack.Push(new BooleanConstant(val1Bool.Value == val2Bool.Value));
                        break;

                    default:
                        opStack.Push(val1);
                        opStack.Push(val2);
                        throw new Exception($"Unsupported types for equality comparison: {val1.GetType()} and {val2.GetType()}");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "ne" not equal to operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void NotEqualToOperation()
        {
            if (StackCount() >= 2)
            {
                Constant val2 = opStack.Pop();
                Constant val1 = opStack.Pop();

                switch ((val1, val2))
                {
                    case (StringConstant val1String, StringConstant val2String):
                        opStack.Push(new BooleanConstant((val1String.Value != val2String.Value)));
                        break;

                    case (FloatConstant val1Float, IntegerConstant val2Int):
                        opStack.Push(new BooleanConstant(Math.Abs(val1Float.Value - val2Int.Value) > 0.0001f));
                        break;

                    case (IntegerConstant val1Int, FloatConstant val2Float):
                        opStack.Push(new BooleanConstant(Math.Abs(val1Int.Value - val2Float.Value) > 0.0001f));
                        break;

                    case (IntegerConstant val1Int, IntegerConstant val2Int):
                        opStack.Push(new BooleanConstant((val1Int.Value != val2Int.Value)));
                        break;

                    case (BooleanConstant val1Bool, BooleanConstant val2Bool):
                        opStack.Push(new BooleanConstant(val1Bool.Value != val2Bool.Value));
                        break;

                    default:
                        opStack.Push(val1);
                        opStack.Push(val2);
                        throw new Exception($"Unsupported types for inequality comparison: {val1.GetType()} and {val2.GetType()}");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "gt" greater than operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void GreaterThanOperation()
        {
            if (StackCount() >= 2)
            {
                Constant val2 = opStack.Pop();
                Constant val1 = opStack.Pop();

                switch ((val1, val2))
                {
                    case (FloatConstant val1Float, IntegerConstant val2Int):
                        opStack.Push(new BooleanConstant(val1Float.Value > val2Int.Value));
                        break;

                    case (IntegerConstant val1Int, FloatConstant val2Float):
                        opStack.Push(new BooleanConstant(val1Int.Value > val2Float.Value));
                        break;

                    case (IntegerConstant val1Int, IntegerConstant val2Int):
                        opStack.Push(new BooleanConstant((val1Int.Value > val2Int.Value)));
                        break;

                    default:
                        opStack.Push(val1);
                        opStack.Push(val2);
                        throw new Exception($"Unsupported types for greater than comparison: {val1.GetType()} and {val2.GetType()}");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "lt" less than operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void LessThanOperation()
        {
            if (StackCount() >= 2)
            {
                Constant val2 = opStack.Pop();
                Constant val1 = opStack.Pop();

                switch ((val1, val2))
                {
                    case (FloatConstant val1Float, IntegerConstant val2Int):
                        opStack.Push(new BooleanConstant(val1Float.Value < val2Int.Value));
                        break;

                    case (IntegerConstant val1Int, FloatConstant val2Float):
                        opStack.Push(new BooleanConstant(val1Int.Value < val2Float.Value));
                        break;

                    case (IntegerConstant val1Int, IntegerConstant val2Int):
                        opStack.Push(new BooleanConstant((val1Int.Value < val2Int.Value)));
                        break;

                    default:
                        opStack.Push(val1);
                        opStack.Push(val2);
                        throw new Exception($"Unsupported types for less than comparison: {val1.GetType()} and {val2.GetType()}");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "and" operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void AndOperation()
        {
            if (StackCount() >= 2)
            {
                Constant val2 = opStack.Pop();
                Constant val1 = opStack.Pop();

                switch ((val1, val2))
                {
                    case (BooleanConstant val1Bool, BooleanConstant val2Bool):
                        opStack.Push(new BooleanConstant(val1Bool.Value && val2Bool.Value));
                        break;

                    case (IntegerConstant val1Int, IntegerConstant val2Int):
                        opStack.Push(new IntegerConstant((val1Int.Value & val2Int.Value)));
                        break;

                    default:
                        opStack.Push(val1);
                        opStack.Push(val2);
                        throw new Exception($"Unsupported types for and comparison: {val1.GetType()} and {val2.GetType()}");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "or" operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void OrOperation()
        {
            if (StackCount() >= 2)
            {
                Constant val2 = opStack.Pop();
                Constant val1 = opStack.Pop();

                switch ((val1, val2))
                {
                    case (BooleanConstant val1Bool, BooleanConstant val2Bool):
                        opStack.Push(new BooleanConstant(val1Bool.Value || val2Bool.Value));
                        break;

                    case (IntegerConstant val1Int, IntegerConstant val2Int):
                        opStack.Push(new IntegerConstant((val1Int.Value | val2Int.Value)));
                        break;

                    default:
                        opStack.Push(val1);
                        opStack.Push(val2);
                        throw new Exception($"Unsupported types for or comparison: {val1.GetType()} and {val2.GetType()}");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "not" operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void NotOperation()
        {
            if (StackCount() >= 1)
            {
                Constant val = opStack.Pop();

                switch (val)
                {
                    case (BooleanConstant valBool):
                        opStack.Push(new BooleanConstant(!valBool.Value));
                        break;

                    case (IntegerConstant valInt):
                        opStack.Push(new IntegerConstant(~valInt.Value));
                        break;

                    default:
                        opStack.Push(val);
                        throw new Exception($"Unsupported type for not comparison: {val.GetType()}");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        #endregion

        #region FLOW_CONTROL_OPERATIONS

        /// <summary>
        /// Defines "if" conditional operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void IfOperation()
        {
            if (StackCount() >= 2)
            {
                Constant val2 = opStack.Pop();
                Constant val1 = opStack.Pop();

                switch ((val1, val2))
                {
                    case (BooleanConstant val1Bool, CodeBlockConstant val2CodeBlock):
                        if (val1Bool.Value)
                            ProcessInput(val2CodeBlock.Value);
                        break;

                    default:
                        opStack.Push(val1);
                        opStack.Push(val2);
                        throw new Exception($"Unsupported types for if conditional: {val1.GetType()} and {val2.GetType()}");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "ifelse" conditional operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void IfElseOperation()
        {
            if (StackCount() >= 3)
            {
                Constant val3 = opStack.Pop();
                Constant val2 = opStack.Pop();
                Constant val1 = opStack.Pop();

                switch ((val1, val2, val3))
                {
                    case (BooleanConstant val1Bool, CodeBlockConstant val2CodeBlock, CodeBlockConstant val3CodeBlock):
                        if (val1Bool.Value)
                            ProcessInput(val2CodeBlock.Value);
                        else
                            ProcessInput(val3CodeBlock.Value);
                        break;

                    default:
                        opStack.Push(val1);
                        opStack.Push(val2);
                        opStack.Push(val3);
                        throw new Exception($"Unsupported types for if else conditional: {val1.GetType()}, {val2.GetType()}, and {val3.GetType()}");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "for" loop operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void ForOperation()
        {
            if (StackCount() >= 4)
            {
                Constant val4 = opStack.Pop();
                Constant val3 = opStack.Pop();
                Constant val2 = opStack.Pop();
                Constant val1 = opStack.Pop();

                switch ((val1, val2, val3, val4))
                {
                    case (IntegerConstant val1Int, IntegerConstant val2Int, IntegerConstant val3Int, CodeBlockConstant val4CodeBlock):
                        for (int i = val1Int.Value; i <= val2Int.Value; i += val3Int.Value)
                        {
                            ProcessInput(val4CodeBlock.Value);
                        }
                        break;

                    default:
                        opStack.Push(val1);
                        opStack.Push(val2);
                        opStack.Push(val3);
                        opStack.Push(val4);
                        throw new Exception($"Unsupported types for for loop: {val1.GetType()}, {val2.GetType()}, {val3.GetType()}, and {val4.GetType()}");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "repeat" operation.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void RepeatOperation()
        {
            if (StackCount() >= 2)
            {
                Constant val2 = opStack.Pop();
                Constant val1 = opStack.Pop();

                switch ((val1, val2))
                {
                    case (IntegerConstant val1Bool, CodeBlockConstant val2CodeBlock):
                        for (int i = 0; i < val1Bool.Value; i++)
                        {
                            ProcessInput(val2CodeBlock.Value);
                        }
                        break;

                    default:
                        opStack.Push(val1);
                        opStack.Push(val2);
                        throw new Exception($"Unsupported types for repeat operation: {val1.GetType()} and {val2.GetType()}");
                }
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        /// <summary>
        /// Defines "quit" operation.
        /// </summary>
        private static void QuitOperation()
        {
            Application.Quit();
        }

        #endregion

        #region INPUT/OUTPUT_OPERATIONS

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
                    DisplayToConsole(fc.Value.ToString("0.0####"));
                else if (constant is StringConstant sc)
                    DisplayToConsole(sc.Value);
                else if (constant is CodeBlockConstant cbc)
                    DisplayToConsole(cbc.Value);
                else if (constant is VariableConstant vc)
                    DisplayToConsole(vc.Value);
                else if (constant is DictionaryConstant dc)
                    DisplayToConsole(dc.Value.ToString());
                else
                    throw new Exception("Unable to display constant type");
            }
            else
            {
                throw new Exception("Not enough constants in stack");
            }
        }

        #endregion
    }
}
