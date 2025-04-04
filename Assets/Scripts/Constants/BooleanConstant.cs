namespace PSInterpreter.Constants
{
    public class BooleanConstant : Constant
    {
        public bool Value { get; set; }

        public BooleanConstant(bool value)
        {
            ConstantType = typeof(bool);
            Value = value;
        }
    }
}