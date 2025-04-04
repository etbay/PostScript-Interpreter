namespace PSInterpreter.Constants
{
    public class IntegerConstant : Constant
    {
        public static int Value { get; set; }

        public IntegerConstant(int value)
        {
            ConstantType = typeof(int);
            Value = value;
        }
    }
}