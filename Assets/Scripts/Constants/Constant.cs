using System;

namespace Interpreter.Constants
{
    public abstract class Constant
    {
        public static Type ConstantType { get; protected set; }
    }
}