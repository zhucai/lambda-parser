using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Zhucai.LambdaParser
{
    [Serializable]
    abstract public class ParseException : Exception
    {
        public ParseException(string message, int errorIndex) : this(message, errorIndex, null) { }
        public ParseException(string message, int errorIndex, Exception inner)
            : base(string.Format("位置{0}附近：{1}", errorIndex, message), inner) { }

        static public void Assert(string strInput,string strNeed,int index)
        {
            if (strInput != strNeed)
            {
                throw new ParseWrongSymbolException(strNeed, strInput,index);
            }
        }
    }

    [Serializable]
    abstract public class CompileException : Exception
    {
        public CompileException(string message, int errorIndex) : this(message, errorIndex, null) { }
        public CompileException(string message, int errorIndex, Exception inner)
            : base(string.Format("位置{0}附近：{1}", errorIndex, message), inner) { }
    }

    [Serializable]
    public class ParseNoEndException : ParseException
    {
        public ParseNoEndException(string symbol, int errorIndex)
            : base(string.Format("未结束的符号：“{0}”", symbol), errorIndex)
        {
        }
    }

    [Serializable]
    public class ParseUnknownException : ParseException
    {
        public ParseUnknownException(string symbol, int errorIndex)
            : base(string.Format("未知的符号：“{0}”", symbol), errorIndex)
        {
        }
    }

    [Serializable]
    public class ParseUnmatchException : ParseException
    {
        public ParseUnmatchException(string startSymbol, string endSymbol, int errorIndex)
            : base(string.Format("未匹配的符号。开始符“{0}”VS结束符“{1}”", startSymbol, endSymbol), errorIndex)
        {
        }
    }

    [Serializable]
    public class ParseWrongSymbolException : ParseException
    {
        public ParseWrongSymbolException(string rightSymbol, string wrongSymbol, int errorIndex)
            : base(string.Format("不正确的符号。应该是“{0}”；这里是“{1}”", rightSymbol, wrongSymbol), errorIndex)
        {
        }
    }

    [Serializable]
    public class ParseUnfindTypeException : ParseException
    {
        public ParseUnfindTypeException(string typeName, int errorIndex)
            : base(string.Format("未找到类型：“{0}”", typeName), errorIndex)
        {
        }
    }
}
