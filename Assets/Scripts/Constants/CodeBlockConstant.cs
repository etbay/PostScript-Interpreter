namespace PSInterpreter.Constants
{
    public class CodeBlockConstant : Constant
    {
        public string Value { get; set; } = string.Empty;

        public CodeBlockConstant(string value)
        {
            ConstantType = typeof(string);
            Value = value;
        }

        public CodeBlockConstant()
        {
            ConstantType = typeof(string);
        }
    }
}