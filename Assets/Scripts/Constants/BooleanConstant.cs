namespace Interpreter.Constants
{
    public class BooleanConstant : Constant
    {
        public static bool Value { get; set; }

        public BooleanConstant(bool value)
        {
            ConstantType = typeof(bool);
            Value = value;
        }
    }
}