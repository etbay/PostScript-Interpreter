namespace PSInterpreter.Constants
{
    public class FloatConstant : Constant
    {
        public float Value { get; set; }

        public FloatConstant(float value)
        {
            ConstantType = typeof(float);
            Value = value;
        }
    }
}