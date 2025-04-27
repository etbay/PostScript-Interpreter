using CodiceApp;
using System.Collections.Generic;

namespace PSInterpreter.Constants
{
    public class DictionaryConstant : Constant
    {
        public Dictionary<string, Constant> Value { get; set; }
        public int MaxSize { get; private set; }

        public DictionaryConstant(int maxVal)
        {
            MaxSize = maxVal;
            ConstantType = typeof(Dictionary<string, Constant>);
            Value = new Dictionary<string, Constant>();
        }
    }
}