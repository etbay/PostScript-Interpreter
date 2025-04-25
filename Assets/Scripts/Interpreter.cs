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

    public static class Interpreter
    {
        public delegate Constant Parser(string input);

        private static Stack<Constant> opStack = new Stack<Constant>();

        private static List<Dictionary<string, Constant>> dictStack = new List<Dictionary<string, Constant>>();

        private static List<Parser> parsers = new List<Parser>();

        static Interpreter()
        {
            parsers.Add(ProcessNumeric);
            parsers.Add(ProcessBoolean);
            InitializeDict();
        }

        private static void InitializeDict()
        {
            dictStack.Add(new Dictionary<string, Constant>());
            // adds operations to first (and only) dictionary
            dictStack[0]["add"] = new OperationConstant(AddOperation);
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

        public static void ProcessInput(string input)
        {
            try
            {
                Debug.Log("Processing " + input);
                ProcessConstants(input);
            }
            catch
            {
                Debug.Log("Could not process " + input + " as a constant");
                try
                {
                    LookupInDict(input);
                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// Defines "add" operation.
        /// </summary>
        private static void AddOperation()
        {
            if (StackCount() >= 2)
            {
                Constant val1 = opStack.Pop();
                Constant val2 = opStack.Pop();

                
            }
        }

        private static void ProcessConstants(string input)
        {
            foreach (Parser parser in parsers)
            {
                try
                {
                    Constant res = parser(input);
                    opStack.Push(res);
                    return;
                }
                catch
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

        // may create a factory later for Constant types
        private static Constant ProcessNumeric(string input)
        {
            Debug.Log("Attempting to parse " + input + " into numeric");

            if (float.TryParse(input, out float floatResult))
            {
                if (floatResult == (float)(int)floatResult)
                {
                    Debug.Log("Parsed " + input + " into integer");
                    return new IntegerConstant((int)floatResult);
                }
                else
                {
                    Debug.Log("Parsed " + input + " into float");
                    return new FloatConstant(floatResult);
                }
            }
            throw new Exception("Could not parse " + input + " into numeric");
        }

        private static Constant ProcessBoolean(string input)
        {
            Debug.Log("Attempting to parse " + input + " into boolean");

            if (input == "true")
            {
                Debug.Log("Parsed " + input + " into boolean");
                return new BooleanConstant(true);
            }
            else if (input == "false")
            {
                Debug.Log("Parsed " + input + " into boolean");
                return new BooleanConstant(false);
            }
            throw new Exception("Could not parse " + input + " into boolean");
        }
    }
}
