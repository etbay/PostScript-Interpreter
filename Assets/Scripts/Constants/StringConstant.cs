namespace PSInterpreter.Constants
{
    public class StringConstant : Constant
    {
        public string Value { get; set; }

        public StringConstant(string value)
        {
            ConstantType = typeof(string);
            Value = value;
        }
    }
}