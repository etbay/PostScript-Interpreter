using PSInterpreter.Constants;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Net.NetworkInformation;

namespace PSInterpreter
{
    public static class Interpreter
    {
        public delegate Constant Parser(string input);

        public delegate void Operation();

        private static Stack<Constant> opStack = new Stack<Constant>();

        private static Stack<Dictionary<string, Constant>> dictStack = new Stack<Dictionary<string, Constant>>();

        private static List<Parser> parsers = new List<Parser>();

        static Interpreter()
        {
            parsers.Add(ProcessNumeric);
            parsers.Add(ProcessBoolean);
        }

        public static void ProcessInput(string input)
        {
            try
            {
                Debug.WriteLine("processing " + input);
                ProcessConstants(input);
            }
            catch
            {
                try
                {
                    LookupInDict(input);
                }
                catch
                {

                }
            }
            Debug.WriteLine(opStack.ToString());
        }

        public static int StackCount()
        {
            return opStack.Count;
        }

        public static Constant PeekStack()
        {
            return opStack.Peek();
        }

        public static void Reset()
        {
            opStack.Clear();
            dictStack.Clear();
            InitializeDict();
        }

        private static void InitializeDict()
        {
            //stuff
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
        }

        private static void LookupInDict(string input)
        {

        }

        // may create a factory later for parsers

        private static Constant ProcessNumeric(string input)
        {
            Debug.WriteLine("Attempting to parse " + input + " into numeric");

            if (float.TryParse(input, out float floatResult))
            {
                if (floatResult == (float)(int)floatResult)
                {
                    return new IntegerConstant((int)floatResult);
                }
                else
                {
                    return new FloatConstant(floatResult);
                }
            }

            throw new Exception("Could not parse " + input + " into numeric");
        }

        private static Constant ProcessBoolean(string input)
        {
            Debug.WriteLine("Attempting to parse " + input + " into boolean");

            if (input == "true")
            {
                return new BooleanConstant(true);
            }
            else if (input == "false")
            {
                return new BooleanConstant(false);
            }

            throw new Exception("Could not parse " + input + " into boolean");
        }
    }
}
