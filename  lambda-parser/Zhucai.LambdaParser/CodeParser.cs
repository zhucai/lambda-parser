using System;
using System.Collections.Generic;
using System.Text;

namespace Zhucai.LambdaParser
{
    /// <summary>
    /// 解析代码
    /// </summary>
    public class CodeParser
    {
        /// <summary>
        /// 当前读取的索引位置
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// 当前读取的字符长度
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// 整个传入的代码内容
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// 分析".."或@".."所定义的字符串
        /// </summary>
        public string DefineString { get; private set; }

        public CodeParser(string content)
        {
            this.Content = content;
        }

        /// <summary>
        /// 往下读取字符串。(此方法是Read()方法的封装)
        /// </summary>
        /// <returns></returns>
        public string ReadString()
        {
            return ReadString(true);
        }

        /// <summary>
        /// 往下读取字符串。(此方法是Read()方法的封装)
        /// </summary>
        /// <param name="isIgnoreWhiteSpace">是否忽略空格</param>
        /// <returns></returns>
        public string ReadString(bool isIgnoreWhiteSpace)
        {
            if (Read(true, isIgnoreWhiteSpace))
            {
                return this.Content.Substring(this.Index, this.Length);
            }
            return null;
        }

        /// <summary>
        /// 读取接下来的符号
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public bool ReadSymbol(string symbol)
        {
            return ReadSymbol(symbol, true);
        }

        /// <summary>
        /// 读取接下来的符号
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="throwExceptionIfError"></param>
        /// <returns></returns>
        public bool ReadSymbol(string symbol, bool throwExceptionIfError)
        {
            // 跳过空格
            while (char.IsWhiteSpace(this.Content[this.Index+this.Length]))
            {
                this.Length++;
            }

            if (throwExceptionIfError)
            {
                ParseException.Assert(this.Content.Substring(this.Index + this.Length, symbol.Length), symbol, this.Index);
            }
            else if (this.Content.Substring(this.Index + this.Length, symbol.Length) != symbol)
            {
                return false;
            }
            this.Index += this.Length;
            this.Length = symbol.Length;
            return true;
        }

        /// <summary>
        /// 获取下一个字符串而不改变当前位置
        /// </summary>
        /// <returns></returns>
        public string PeekString()
        {
            int index = this.Index;
            int length = this.Length;

            string str = ReadString(true);

            this.Index = index;
            this.Length = length;

            return str;
        }

        #region private 方法

        /// <summary>
        /// 往下读取。通过Index和Length指示当前位置。
        /// </summary>
        /// <param name="isBuildDefineString">遇到代码中的字符串常量时是否将字符串常量解析到DefineString成员。</param>
        /// <param name="isIgnoreWhiteSpace">是否忽略空格</param>
        /// <returns></returns>
        private bool Read(bool isBuildDefineString, bool isIgnoreWhiteSpace)
        {
            this.Index += this.Length;
            this.Length = 1;

            // 超过了末尾，则返回0
            if (this.Index == this.Content.Length)
            {
                this.Index = 0;
                return false;
            }

            // 检查到空白字符，则跳过，继续
            if (isIgnoreWhiteSpace && char.IsWhiteSpace(this.Content, this.Index))
            {
                return Read(isBuildDefineString, isIgnoreWhiteSpace);
            }

            // 获取当前字母
            char c = this.Content[this.Index];

            // 字母或下划线开头
            #region if (char.IsLetter(c) || c == '_' || c == '$')
            if (char.IsLetter(c) || c == '_' || c == '$')
            {
                // 找下去
                for (this.Length = 1; (this.Length + this.Index) < this.Content.Length; this.Length++)
                {
                    char cInner = this.Content[this.Index + this.Length];

                    // 当char不是字母、数字、下划线时返回
                    if ((!char.IsLetterOrDigit(cInner)) && cInner != '_')
                    {
                        return true;
                    }
                }

                return true;
            }
            #endregion

            // 数字开头
            #region if (char.IsDigit(c))
            if (char.IsDigit(c))
            {
                // 找下去
                for (this.Length = 1; (this.Length + this.Index) < this.Content.Length; this.Length++)
                {
                    char cInner = this.Content[this.Index + this.Length];

                    // 当char是点时，判断其后面的字符是否数字，若不是数字，则返回
                    if (cInner == '.')
                    {
                        char nextChar = this.Content[this.Index + this.Length + 1];
                        if (!char.IsDigit(nextChar))
                        {
                            return true;
                        }
                    }

                    // 当char不是数字、mdflx(表示各种数字类型如l表示long)时返回
                    if ((!char.IsDigit(cInner)) && cInner != '.'
                        && cInner != 'M' && cInner != 'm'
                        && cInner != 'D' && cInner != 'd'
                        && cInner != 'F' && cInner != 'f'
                        && cInner != 'L' && cInner != 'l'
                        && cInner != 'X' && cInner != 'x')
                    {
                        return true;
                    }
                }

                return true;
            }
            #endregion

            // 获取下一个char
            char nextInner;
            if (!TryGetNextChar(false, out nextInner))
            {
                // 到尾了，直接返回
                return true;
            }

            // 是否已知符号，某些做处理
            switch (c)
            {
                #region case .....
                case '.':
                case '(':
                case ')':
                case '[':
                case ']':
                case '+':
                case '-':
                case '!':
                case '~':
                case '*':
                case '%':
                case '^':
                case ':':

                case '{':
                case '}':
                case ',':
                case ';':
                case ' ':
                case '	':
                    break;

                case '=':
                    if (nextInner == '>') // =>
                    {
                        this.Length++;
                        return true;
                    }
                    break;

                case '>':
                case '<':
                    if (nextInner == c) // >>,<<
                    {
                        this.Length++;
                    }
                    break;
                case '&':
                case '|':
                case '?':
                    if (nextInner == c) // &&,||, ??
                    {
                        this.Length++;
                        return true;
                    }
                    break;
                #endregion

                #region case '/':
                case '/':
                    if (nextInner == c) // 注释符://
                    {
                        const string SampleCommitEnd = "\n";
                        this.Length++;
                        int endIndex = GetStringIndex(SampleCommitEnd, this.Index + this.Length);
                        if (endIndex == -1) // 到达最后
                        {
                            this.Length = this.Content.Length - this.Index;
                        }
                        else
                        {
                            this.Length = endIndex - this.Index + SampleCommitEnd.Length;
                        }

                        return true;
                    }
                    else if (nextInner == '*') // 注释符:/**/
                    {
                        const string MultiLineEnd = "*/";
                        this.Length++;
                        int endIndex = GetStringIndex(MultiLineEnd, this.Index + this.Length);
                        if (endIndex == -1) // 到达最后
                        {
                            throw new ParseNoEndException("/*", this.Index);
                        }
                        else
                        {
                            this.Length = endIndex - this.Index + MultiLineEnd.Length;
                        }

                        return true;
                    }
                    break;
                #endregion

                #region case '\'':
                case '\'':
                    for (int i = this.Index + this.Length; i < this.Content.Length; i++)
                    {
                        // 找到\，则忽略下一个
                        if (this.Content[i] == '\\')
                        {
                            i++;
                            continue;
                        }

                        // 找到'
                        if (this.Content[i] == '\'')
                        {
                            this.Length = i - this.Index + 1;

                            if (isBuildDefineString)
                            {
                                if (this.Length == 3)
                                {
                                    this.DefineString = this.Content.Substring(this.Index + 1, 1);
                                }
                                else if (this.Length == 4 && this.Content[this.Index + 1] == '\\')
                                {
                                    this.DefineString = GetTransformMeanChar(this.Content[this.Index + 2]).ToString();
                                }
                            }
                            return true;
                        }
                    }
                    throw new ParseNoEndException("\'", this.Index);
                #endregion

                #region case '\"':
                case '\"':
                    StringBuilder sb = null;
                    int prevIndex = this.Index + this.Length;
                    if (isBuildDefineString)
                    {
                        sb = new StringBuilder();
                    }
                    for (int i = this.Index + this.Length; i < this.Content.Length; i++)
                    {
                        // 发现\，则忽略下一个
                        if (this.Content[i] == '\\')
                        {
                            i++;
                            if (isBuildDefineString)
                            {
                                sb.Append(this.Content, prevIndex, i - prevIndex - 1);
                                prevIndex = i + 1;

                                char chOriginal = this.Content[i];
                                char ch = GetTransformMeanChar(chOriginal);
                                sb.Append(ch);
                            }
                            continue;
                        }

                        // 发现"
                        if (this.Content[i] == '\"')
                        {
                            this.Length = i - this.Index + 1;
                            if (isBuildDefineString)
                            {
                                sb.Append(this.Content, prevIndex, i - prevIndex);
                                this.DefineString = sb.ToString();
                            }
                            return true;
                        }
                    }
                    throw new ParseNoEndException("\"", this.Index);
                #endregion

                #region case '@':
                case '@':
                    if (nextInner == '\"') // @""
                    {
                        this.Length++;
                        for (int i = this.Index + this.Length; i < this.Content.Length; i++)
                        {
                            // 找到"
                            if (this.Content[i] == '\"')
                            {
                                // 是否到达结尾
                                if ((i + 1) < this.Content.Length)
                                {
                                    // 是否跟着一个"
                                    if (this.Content[i + 1] == '\"')
                                    {
                                        i++;
                                        continue;
                                    }
                                }

                                this.Length = i - this.Index + 1;

                                if (isBuildDefineString)
                                {
                                    // 目前先用替换，有空了可以做优化
                                    this.DefineString = this.Content.Substring(this.Index + 2, this.Length - 3).Replace("\"\"", "\"");
                                }

                                return true;
                            }
                        }
                    }
                    break;
                #endregion

                default:
                    throw new ParseUnknownException(c.ToString(), this.Index);
            }

            // 处理后面可能跟等号(=)的
            #region switch (c)
            switch (c)
            {
                case '&':
                case '|':
                case '+':
                case '-':
                case '*':
                case '/':
                case '%':
                case '^':
                case '<':
                case '>':
                case '!':
                case '=':
                    if (!TryGetNextChar(false, out nextInner))
                    {
                        return true;
                    }
                    if (nextInner == '=')
                    {
                        this.Length++;
                    }
                    break;
            }
            #endregion

            return true;
        }

        /// <summary>
        /// 在this.Content获取指定字符串，返回-1表示没找到
        /// </summary>
        private int GetStringIndex(string str, int startIndex)
        {
            for (int i = startIndex; i < this.Content.Length; i++)
            {
                if (string.Compare(this.Content, i, str, 0, str.Length, StringComparison.Ordinal)
                    == 0)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 尝试获取下个字符，若已到结尾没有下个字符则返回false
        /// </summary>
        private bool TryGetNextChar(bool ignoreWhiteSpace, out char cNext)
        {
            cNext = '\0';
            for (int i = 0; i < int.MaxValue; i++)
            {
                if (this.Index + this.Length + i >= this.Content.Length)
                {
                    return false;
                }
                cNext = this.Content[this.Index + this.Length];
                if ((!ignoreWhiteSpace) || (!char.IsWhiteSpace(cNext)))
                {
                    break;
                }
            }
            return true;
        }

        private char GetTransformMeanChar(char chOriginal)
        {
            switch (chOriginal)
            {
                case '\\':
                    return '\\';

                case '\'':
                    return '\'';

                case '"':
                    return '"';

                case 'n':
                    return '\n';

                case 'r':
                    return '\r';

                case 't':
                    return '\t';

                case 'v':
                    return '\v';

                case 'b':
                    return '\b';

                case 'f':
                    return '\f';

                case 'a':
                    return '\a';

                //todo:
                //case 'x':
                //case 'X':
                //    sb.Append(Convert.ToChar();

                default:
                    throw new ParseUnknownException("\\" + chOriginal, Index);
                //return '\0';
            }
        }

        #endregion


        #region CodeParserPosition的操作

        /// <summary>
        /// 保存当前位置
        /// </summary>
        /// <returns></returns>
        public CodeParserPosition SavePosition()
        {
            return new MyCodeParserPosition()
            {
                Index = this.Index,
                Length = this.Length
            };
        }

        /// <summary>
        /// 恢复指定的位置
        /// </summary>
        /// <param name="position"></param>
        public void RevertPosition(CodeParserPosition position)
        {
            MyCodeParserPosition myPosition = (MyCodeParserPosition)position;
            this.Index = myPosition.Index;
            this.Length = myPosition.Length;
        }

        /// <summary>
        /// 恢复到初始状态
        /// </summary>
        public void RevertPosition()
        {
            RevertPosition(new MyCodeParserPosition());
        }

        private class MyCodeParserPosition : CodeParserPosition
        {
            public int Index { get; set; }
            public int Length { get; set; }
        }

        #endregion
    }

    /// <summary>
    /// CodeParser保存的位置点，用来还原
    /// </summary>
    abstract public class CodeParserPosition
    {
    }
}
