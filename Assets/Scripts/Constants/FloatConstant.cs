namespace Interpreter.Constants
{
    public class FloatConstant : Constant
    {
        public static float Value { get; set; }

        public FloatConstant(float value)
        {
            ConstantType = typeof(float);
            Value = value;
        }
    }
}