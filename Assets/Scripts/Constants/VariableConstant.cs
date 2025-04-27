namespace PSInterpreter.Constants
{
    public class VariableConstant : Constant
    {
        public string Value { get; set; }

        public VariableConstant(string value)
        {
            ConstantType = typeof(string);
            Value = value;
        }
    }
}