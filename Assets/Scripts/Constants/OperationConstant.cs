namespace PSInterpreter.Constants
{
    public class OperationConstant : Constant
    {
        public Operation Value { get; set; }

        public OperationConstant(Operation value)
        {
            ConstantType = typeof(Operation);
            Value = value;
        }
    }
}