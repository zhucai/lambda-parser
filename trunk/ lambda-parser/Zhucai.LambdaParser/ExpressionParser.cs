using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Zhucai.LambdaParser
{
    /// <summary>
    /// 解释成Lambda表达式
    /// </summary>
    /// <typeparam name="TDelegate"></typeparam>
    public class ExpressionParser<TDelegate>
    {
        private CodeParser _parser;

        private List<ParameterExpression> _params = new List<ParameterExpression>();

        #region static member
        /// <summary>
        /// 存放操作符的优先级
        /// </summary>
        static private Dictionary<string, int> _operatorPriorityLevel = new Dictionary<string, int>();

        /// <summary>
        /// 存放数字类型的隐式转换级别
        /// </summary>
        static private Dictionary<Type, int> _numberTypeLevel = new Dictionary<Type, int>();

        static ExpressionParser()
        {
            // 初始化_operatorPriorityLevel
            _operatorPriorityLevel.Add("(", 100);
            _operatorPriorityLevel.Add(")", 100);
            _operatorPriorityLevel.Add("[", 100);
            _operatorPriorityLevel.Add("]", 100);

            _operatorPriorityLevel.Add(".", 13);
            _operatorPriorityLevel.Add("function()", 13);
            _operatorPriorityLevel.Add("index[]", 13);
            _operatorPriorityLevel.Add("++behind", 13);
            _operatorPriorityLevel.Add("--behind", 13);
            _operatorPriorityLevel.Add("new", 13);
            _operatorPriorityLevel.Add("typeof", 13);
            _operatorPriorityLevel.Add("checked", 13);
            _operatorPriorityLevel.Add("unchecked", 13);
            _operatorPriorityLevel.Add("->", 13);

            _operatorPriorityLevel.Add("++before", 12);
            _operatorPriorityLevel.Add("--before", 12);
            _operatorPriorityLevel.Add("+before", 12);
            _operatorPriorityLevel.Add("-before", 12);
            _operatorPriorityLevel.Add("!", 12);
            _operatorPriorityLevel.Add("~", 12);
            _operatorPriorityLevel.Add("convert()", 12);
            _operatorPriorityLevel.Add("sizeof", 12);

            _operatorPriorityLevel.Add("*", 11);
            _operatorPriorityLevel.Add("/", 11);
            _operatorPriorityLevel.Add("%", 11);
            _operatorPriorityLevel.Add("+", 10);
            _operatorPriorityLevel.Add("-", 10);
            _operatorPriorityLevel.Add("<<", 9);
            _operatorPriorityLevel.Add(">>", 9);
            _operatorPriorityLevel.Add(">", 8);
            _operatorPriorityLevel.Add("<", 8);
            _operatorPriorityLevel.Add(">=", 8);
            _operatorPriorityLevel.Add("<=", 8);
            _operatorPriorityLevel.Add("is", 8);
            _operatorPriorityLevel.Add("as", 8);
            _operatorPriorityLevel.Add("==", 7);
            _operatorPriorityLevel.Add("!=", 7);
            _operatorPriorityLevel.Add("&", 6);
            _operatorPriorityLevel.Add("^", 6);
            _operatorPriorityLevel.Add("|", 6);
            _operatorPriorityLevel.Add("&&", 5);
            _operatorPriorityLevel.Add("||", 5);
            _operatorPriorityLevel.Add("?", 5);
            _operatorPriorityLevel.Add("??", 4);
            _operatorPriorityLevel.Add("=", 4);
            _operatorPriorityLevel.Add("+=", 4);
            _operatorPriorityLevel.Add("-=", 4);
            _operatorPriorityLevel.Add("*=", 4);
            _operatorPriorityLevel.Add("/=", 4);
            _operatorPriorityLevel.Add("%=", 4);
            _operatorPriorityLevel.Add("&=", 4);
            _operatorPriorityLevel.Add("|=", 4);
            _operatorPriorityLevel.Add("^=", 4);
            _operatorPriorityLevel.Add(">>=", 4);
            _operatorPriorityLevel.Add("<<=", 4);

            // 初始化_numberTypeLevel
            _numberTypeLevel.Add(typeof(byte), 1);
            _numberTypeLevel.Add(typeof(short), 2);
            _numberTypeLevel.Add(typeof(ushort), 3);
            _numberTypeLevel.Add(typeof(int), 4);
            _numberTypeLevel.Add(typeof(uint), 5);
            _numberTypeLevel.Add(typeof(long), 6);
            _numberTypeLevel.Add(typeof(ulong), 7);
            _numberTypeLevel.Add(typeof(float), 8);
            _numberTypeLevel.Add(typeof(double), 9);
            _numberTypeLevel.Add(typeof(decimal), 10);
        }

        /// <summary>
        /// 获取操作符的优先级，越大优先级越高
        /// </summary>
        /// <param name="operatorSymbol">操作符</param>
        /// <param name="isBefore">是否前置操作符(一元)</param>
        /// <returns>优先级</returns>
        static private int GetOperatorLevel(string operatorSymbol, bool isBefore)
        {
            switch (operatorSymbol)
            {
                case "++":
                case "--":
                    operatorSymbol += isBefore ? "before" : "behind";
                    break;

                case "+":
                case "-":
                    operatorSymbol += isBefore ? "before" : null;
                    break;
            }
            return _operatorPriorityLevel[operatorSymbol];
        }

        /// <summary>
        /// 根据类型名称获取类型对象
        /// </summary>
        /// <param name="typeName">类型名称。可以是简写：如int、string</param>
        /// <returns></returns>
        static private Type GetType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            switch (typeName)
            {
                case "bool":
                    return typeof(bool);

                case "byte":
                    return typeof(byte);

                case "sbyte":
                    return typeof(sbyte);

                case "char":
                    return typeof(char);

                case "decimal":
                    return typeof(decimal);

                case "double":
                    return typeof(double);

                case "float":
                    return typeof(float);

                case "int":
                    return typeof(int);

                case "uint":
                    return typeof(uint);

                case "long":
                    return typeof(long);

                case "ulong":
                    return typeof(ulong);

                case "object":
                    return typeof(object);

                case "short":
                    return typeof(short);

                case "ushort":
                    return typeof(ushort);

                case "string":
                    return typeof(string);

                default:
                    break;
            }

            Type type = Type.GetType(typeName, false, false);

            Assembly[] listAssembly = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in listAssembly)
            {
                if (assembly != Assembly.GetExecutingAssembly())
                {
                    type = assembly.GetType(typeName, false, false);
                    if (type != null)
                    {
                        return type;
                    }
                }
            }
            return type;
        }

        #endregion

        public ExpressionParser(string code)
        {
            this._parser = new CodeParser(code);
        }

        /// <summary>
        /// 输出Expression
        /// </summary>
        /// <returns></returns>
        public Expression<TDelegate> ToExpression()
        {
            // 获取委托的参数类型
            Type type = typeof(TDelegate);
            MethodInfo methodInfo = type.GetMethod("Invoke");
            List<Type> listType = methodInfo.GetParameters().ToList().ConvertAll(m => m.ParameterType);

            // 检查是否有lambda前置符(如:m=>)
            string val = _parser.ReadString();
            bool hasLambdaPre = false;
            if (val == "(")
            {
                string bracketContent = GetBracketString(true);
                if (bracketContent != null)
                {
                    string lambdaOperator = _parser.ReadString();
                    if (lambdaOperator == "=>")
                    {
                        hasLambdaPre = true;
                        string[] paramsName = bracketContent.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < paramsName.Length; i++)
                        {
                            this._params.Add(Expression.Parameter(listType[i], paramsName[i]));
                        }
                    }
                }
            }
            else if (char.IsLetter(val[0]) || val[0] == '_')
            {
                string lambdaOperator = _parser.ReadString();
                if (lambdaOperator == "=>")
                {
                    hasLambdaPre = true;
                    this._params.Add(Expression.Parameter(listType[0], val));
                }
            }

            // 若没有lambda前置符(如:m=>)，则恢复_parser到初始化
            if (!hasLambdaPre)
            {
                _parser.RevertPosition();
            }

            bool isCloseWrap;
            Expression expression = ReadExpression(0, null, out isCloseWrap);

            return Expression.Lambda<TDelegate>(expression, this._params);
        }

        #region private method

        /// <summary>
        /// 读取Expression。可能会引发递归。
        /// </summary>
        /// <param name="priorityLevel">当前操作的优先级</param>
        /// <param name="wrapStart">括号开始符(如果有)</param>
        /// <param name="isClosedWrap">是否遇到符号结束符</param>
        /// <returns></returns>
        private Expression ReadExpression(int priorityLevel, string wrapStart, out bool isClosedWrap)
        {
            // 初始化
            isClosedWrap = false;
            string val = this._parser.ReadString();
            if (val == null)
            {
                return null;
            }
            char firstChar = val[0];
            Expression currentExpression = null;

            /********************** 第一次读取，一元操作或一个对象(Start) **************************/
            // 数字
            if (char.IsDigit(firstChar))
            {
                // 数字解析
                object constVal = ParseNumber(val);
                currentExpression = Expression.Constant(constVal);
            }
            // 非数字
            else
            {
                // 字母或字符
                switch (val)
                {
                    case "null":
                        currentExpression = Expression.Constant(null);
                        break;

                    case "true":
                        currentExpression = Expression.Constant(true);
                        break;

                    case "false":
                        currentExpression = Expression.Constant(false);
                        break;

                    //case "void":
                    //    currentExpression = Expression.Constant(typeof(System.Void));
                    //    break;

                    #region case "sizeof":
                    case "sizeof":
                        {
                            string str = GetBracketString(false);
                            Type type = GetType(str);
                            currentExpression = Expression.Constant(System.Runtime.InteropServices.Marshal.SizeOf(type));
                        }
                        break;
                    #endregion

                    #region case "typeof":
                    case "typeof":
                        {
                            string str = GetBracketString(false);
                            Type type = GetType(str);
                            currentExpression = Expression.Constant(type);
                        }
                        break;
                    #endregion

                    #region case "new":
                    case "new":
                        {
                            // 获取类型
                            StringBuilder sb = new StringBuilder();
                            sb.Append(_parser.ReadString());
                            while (_parser.Peek() == ".")
                            {
                                sb.Append(_parser.ReadString());
                                sb.Append(_parser.ReadString());
                            }
                            Type type = GetType(sb.ToString());

                            // 是否数组
                            string bracketStart = _parser.ReadString();
                            if (bracketStart == "(")
                            {
                                // 获取参数
                                List<Expression> listParam = ReadParams("(", true);

                                // 获取构造函数
                                ConstructorInfo constructor = type.GetConstructor(listParam.ConvertAll<Type>(m => m.Type).ToArray());
                                currentExpression = Expression.New(constructor, listParam);

                                // 成员初始化
                                if (_parser.Peek() == "{")
                                {
                                    _parser.ReadString();

                                    List<MemberBinding> listMemberBinding = new List<MemberBinding>();
                                    string memberName;
                                    while ((memberName = _parser.ReadString()) != "}")
                                    {
                                        ParseException.Assert(_parser.ReadString(), "=", _parser.Index);

                                        MemberInfo memberInfo = type.GetMember(memberName)[0];
                                        MemberBinding memberBinding = Expression.Bind(memberInfo, ReadExpression(0, wrapStart, out isClosedWrap));
                                        listMemberBinding.Add(memberBinding);

                                        // 逗号
                                        string comma = _parser.ReadString();
                                        if (comma == "}")
                                        {
                                            break;
                                        }
                                        ParseException.Assert(comma, ",", _parser.Index);
                                    }
                                    currentExpression = Expression.MemberInit((NewExpression)currentExpression, listMemberBinding);
                                }
                            }
                            else if (bracketStart == "[")
                            {
                                string nextStr = _parser.Peek();

                                // 读[]里的长度
                                List<Expression> listLen = new List<Expression>();
                                if (nextStr == "]")
                                {
                                    _parser.ReadString();
                                }
                                else
                                {
                                    listLen = ReadParams("[", true);
                                }

                                // 读{}里的数组初始化
                                string start = _parser.Peek();
                                if (start == "{")
                                {
                                    List<Expression> listParams = ReadParams("{", false);
                                    currentExpression = Expression.NewArrayInit(type, listParams);
                                }
                                else
                                {
                                    currentExpression = Expression.NewArrayBounds(type, listLen);
                                }
                            }
                            else
                            {
                                throw new ParseUnknownException(bracketStart, _parser.Index);
                            }
                        }
                        break;
                    #endregion

                    case "+":
                        // 忽略前置+
                        return ReadExpression(priorityLevel, wrapStart, out isClosedWrap);

                    case "-":
                        currentExpression = Expression.Negate(ReadExpression(GetOperatorLevel(val, true), wrapStart, out isClosedWrap));
                        break;

                    case "!":
                        currentExpression = Expression.Not(ReadExpression(GetOperatorLevel(val, true), wrapStart, out isClosedWrap));
                        break;

                    case "~":
                        currentExpression = Expression.Not(ReadExpression(GetOperatorLevel(val, true), wrapStart, out isClosedWrap));
                        break;

                    #region case "(":
                    case "(":
                        {
                            CodeParserPosition position = _parser.SavePosition();
                            string str = GetBracketString(true);
                            Type type = GetType(str);

                            // 找到类型，作为类型转换处理
                            if (type != null)
                            {
                                currentExpression = Expression.Convert(ReadExpression(GetOperatorLevel("convert()", true), wrapStart, out isClosedWrap), type);
                            }
                            // 未找到类型，作为仅用来优先处理
                            else
                            {
                                _parser.RevertPosition(position);

                                // 分配一个新的isClosedWrap变量
                                bool newIsClosedWrap;
                                currentExpression = ReadExpression(0, val, out newIsClosedWrap);
                            }
                        }
                        break;
                    #endregion

                    #region case ")":
                    case ")":
                        {
                            // 结束一个isClosedWrap变量
                            isClosedWrap = true;
                            return null;
                        }
                    #endregion

                    #region case "]":
                    case "]":
                        {
                            // 结束一个isClosedWrap变量
                            isClosedWrap = true;
                            return null;
                        }
                    #endregion

                    #region case "}":
                    case "}":
                        {
                            // 结束一个isClosedWrap变量
                            isClosedWrap = true;
                            return null;
                        }
                    #endregion

                    case ".":
                        {
                            //todo:?
                            //return null;
                        }
                        break;

                    case ",":
                        {
                            return ReadExpression(priorityLevel, wrapStart, out isClosedWrap);
                        }

                    default:
                        {
                            // 头Char是字母或下划线
                            if (char.IsLetter(firstChar) || firstChar == '_')
                            {
                                ParameterExpression parameter;
                                // 先看是否参数
                                if ((parameter = this._params.SingleOrDefault(m => m.Name == val))
                                    != null)
                                {
                                    currentExpression = parameter;
                                }
                                // 不是参数则当作类型
                                else
                                {
                                    #region 静态属性或方法
                                    Type type = null;
                                    string strVal = val;
                                    while (true)
                                    {
                                        type = GetType(strVal);
                                        if (type == null)
                                        {
                                            string str = _parser.ReadString();
                                            ParseException.Assert(str, ".", _parser.Index);
                                            strVal += str + _parser.ReadString();
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }

                                    string strPoint = _parser.ReadString();
                                    ParseException.Assert(strPoint, ".", _parser.Index);
                                    string strMember = _parser.ReadString();
                                    string strOperator = _parser.Peek();

                                    // 静态方法
                                    if (strOperator == "(")
                                    {
                                        // 获取参数
                                        List<Expression> listParam = ReadParams(strOperator, false);

                                        if (parameter != null)
                                        {
                                            MethodInfo methodInfo = parameter.Type.GetMethod(strMember, listParam.ConvertAll<Type>(m => m.Type).ToArray());
                                            currentExpression = Expression.Call(parameter, methodInfo, listParam.ToArray());
                                        }
                                        else
                                        {
                                            MethodInfo methodInfo = type.GetMethod(strMember, listParam.ConvertAll<Type>(m => m.Type).ToArray());
                                            currentExpression = Expression.Call(methodInfo, listParam.ToArray());
                                        }
                                    }
                                    // 静态成员(PropertyOrField)
                                    else
                                    {
                                        if (parameter != null)
                                        {
                                            currentExpression = Expression.PropertyOrField(Expression.Constant(parameter), strMember);
                                        }
                                        else
                                        {
                                            // 先找属性
                                            PropertyInfo propertyInfo = type.GetProperty(strMember, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                                            if (propertyInfo != null)
                                            {
                                                currentExpression = Expression.Property(null, propertyInfo);
                                            }
                                            // 没找到属性则找字段
                                            else
                                            {
                                                FieldInfo fieldInfo = type.GetField(strMember, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                                                if (fieldInfo == null)
                                                {
                                                    throw new ParseUnknownException(strMember, _parser.Index);
                                                }
                                                currentExpression = Expression.Field(null, fieldInfo);
                                            }
                                        }
                                    }

                                    #endregion
                                }
                            }
                            // 头Char不是字母或下划线
                            else
                            {
                                switch (firstChar)
                                {
                                    #region case '\"':
                                    case '\"':
                                        {
                                            string str = _parser.DefineString;
                                            currentExpression = Expression.Constant(str);
                                        }
                                        break;
                                    #endregion

                                    #region case '@':
                                    case '@':
                                        {
                                            string str = _parser.DefineString;
                                            currentExpression = Expression.Constant(str);
                                        }
                                        break;
                                    #endregion

                                    #region case '\'':
                                    case '\'':
                                        {
                                            string str = _parser.DefineString;
                                            currentExpression = Expression.Constant(str[0]);
                                        }
                                        break;
                                    #endregion

                                    #region default:
                                    default:
                                        {
                                            throw new ParseUnknownException(val, _parser.Index);
                                        }
                                    #endregion
                                }
                            }
                        }
                        break;
                }
            }
            /********************** 第一次读取，一元操作或一个对象(End) **************************/


            /********************** 第二(N)次读取，都将是二元或三元操作(Start) **********************/
            int nextLevel = 0;
            // 若isCloseWrap为false(遇到反括号则直接返回)，且下一个操作符的优先级大于当前优先级，则计算下一个
            while ((isClosedWrap == false) && (nextLevel = TryGetNextPriorityLevel()) > priorityLevel)
            {
                string nextVal = _parser.ReadString();

                switch (nextVal)
                {
                    #region case "[":
                    case "[":
                        {
                            // 索引器访问
                            bool newIsClosedWrap;
                            if (currentExpression.Type.IsArray)
                            {
                                currentExpression = Expression.ArrayIndex(currentExpression, ReadExpression(0, "[", out newIsClosedWrap));
                            }
                            else
                            {
                                string indexerName = "Item";

                                object[] atts = currentExpression.Type.GetCustomAttributes(typeof(DefaultMemberAttribute), true);
                                DefaultMemberAttribute indexerNameAtt = (DefaultMemberAttribute)atts.SingleOrDefault();
                                if (indexerNameAtt != null)
                                {
                                    indexerName = indexerNameAtt.MemberName;

                                    PropertyInfo propertyInfo = currentExpression.Type.GetProperty(indexerName);
                                    MethodInfo methodInfo = propertyInfo.GetGetMethod();

                                    // 获取参数
                                    List<Expression> listParam = ReadParams(nextVal, true);

                                    currentExpression = Expression.Call(currentExpression, methodInfo, listParam);
                                }
                            }
                        }
                        break;
                    #endregion

                    #region case "]":
                    case "]":
                        {
                            if (wrapStart != "[")
                            {
                                throw new ParseUnmatchException(wrapStart, nextVal, _parser.Index);
                            }
                            isClosedWrap = true;
                            return currentExpression;
                        }
                    #endregion

                    #region case ")":
                    case ")":
                        {
                            if (wrapStart != "(")
                            {
                                throw new ParseUnmatchException(wrapStart, nextVal, _parser.Index);
                            }
                            isClosedWrap = true;
                            return currentExpression;
                        }
                    #endregion

                    #region case "}":
                    case "}":
                        {
                            if (wrapStart != "{")
                            {
                                throw new ParseUnmatchException(wrapStart, nextVal, _parser.Index);
                            }
                            isClosedWrap = true;
                            return currentExpression;
                        }
                    #endregion

                    #region case "+":
                    case "+":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);

                            // 其中某一个是string类型
                            if (currentExpression.Type == typeof(string) || right.Type == typeof(string))
                            {
                                // 调用string.Concat方法
                                currentExpression = Expression.Call(typeof(string).GetMethod("Concat", new Type[] { typeof(object), typeof(object) }),
                                    Expression.Convert(currentExpression, typeof(object)), Expression.Convert(right, typeof(object)));
                            }
                            else
                            {
                                AdjustNumberType(ref currentExpression, ref right);
                                currentExpression = Expression.Add(currentExpression, right);
                            }
                        }
                        break;
                    #endregion

                    #region case "-":
                    case "-":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            AdjustNumberType(ref currentExpression, ref right);
                            currentExpression = Expression.Subtract(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "*":
                    case "*":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            AdjustNumberType(ref currentExpression, ref right);
                            currentExpression = Expression.Multiply(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "/":
                    case "/":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            AdjustNumberType(ref currentExpression, ref right);
                            currentExpression = Expression.Divide(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "%":
                    case "%":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            AdjustNumberType(ref currentExpression, ref right);
                            currentExpression = Expression.Modulo(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "<<":
                    case "<<":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.LeftShift(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case ">>":
                    case ">>":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.RightShift(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case ">":
                    case ">":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.GreaterThan(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "<":
                    case "<":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.LessThan(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case ">=":
                    case ">=":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.GreaterThanOrEqual(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "<=":
                    case "<=":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.LessThanOrEqual(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "==":
                    case "==":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.Equal(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "!=":
                    case "!=":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.NotEqual(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case ".":
                    case ".":
                        {
                            string strMember = _parser.ReadString();
                            string strOperator = _parser.Peek();
                            // 方法
                            if (strOperator == "(")
                            {
                                // 获取参数
                                List<Expression> listParam = ReadParams("(", false);

                                MethodInfo methodInfo = currentExpression.Type.GetMethod(strMember, listParam.ConvertAll<Type>(m => m.Type).ToArray());
                                currentExpression = Expression.Call(currentExpression, methodInfo, listParam.ToArray());
                            }
                            // 成员(PropertyOrField)
                            else
                            {
                                currentExpression = Expression.PropertyOrField(currentExpression, strMember);
                            }
                        }
                        break;
                    #endregion

                    #region case "is":
                    case "is":
                        {
                            string str = ReadTypeString();
                            currentExpression = Expression.TypeIs(currentExpression, GetType(str));
                        }
                        break;
                    #endregion

                    #region case "as":
                    case "as":
                        {
                            string str = ReadTypeString();
                            currentExpression = Expression.TypeAs(currentExpression, GetType(str));
                        }
                        break;
                    #endregion

                    #region case "^":
                    case "^":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.ExclusiveOr(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "&":
                    case "&":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.And(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "|":
                    case "|":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.Or(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "&&":
                    case "&&":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.AndAlso(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "||":
                    case "||":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.OrElse(currentExpression, right);
                        }
                        break;
                    #endregion

                    #region case "?":
                    case "?":
                        {
                            Expression first = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            ParseException.Assert(_parser.ReadString(), ":", _parser.Index);
                            Expression second = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            currentExpression = Expression.Condition(currentExpression, first, second);
                        }
                        break;
                    #endregion

                    #region case "??":
                    case "??":
                        {
                            Expression right = ReadExpression(nextLevel, wrapStart, out isClosedWrap);
                            Expression test = Expression.Equal(currentExpression, Expression.Constant(null, currentExpression.Type));
                            currentExpression = Expression.Condition(test, right, currentExpression);
                        }
                        break;
                    #endregion

                    default:
                        throw new ParseUnknownException(nextVal, _parser.Index);
                }
            }
            /********************** 第二(N)次读取，都将是二元或三元操作(End) **********************/

            return currentExpression;
        }

        /// <summary>
        /// 解析数字
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private static object ParseNumber(string val)
        {
            object constVal;
            switch (val[val.Length - 1])
            {
                case 'l':
                case 'L':
                    constVal = long.Parse(val.Substring(0, val.Length - 1));
                    break;

                case 'm':
                case 'M':
                    constVal = decimal.Parse(val.Substring(0, val.Length - 1));
                    break;

                case 'f':
                case 'F':
                    constVal = float.Parse(val.Substring(0, val.Length - 1));
                    break;

                case 'd':
                case 'D':
                    constVal = double.Parse(val.Substring(0, val.Length - 1));
                    break;

                default:
                    if (val.IndexOf('.') >= 0)
                    {
                        constVal = double.Parse(val);
                    }
                    else
                    {
                        constVal = long.Parse(val);
                        if ((long)constVal <= (long)int.MaxValue && (long)constVal >= (long)int.MinValue)
                        {
                            constVal = (int)(long)constVal;
                        }
                    }
                    break;
            }
            return constVal;
        }

        /// <summary>
        /// 调整数字运算的类型
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        private void AdjustNumberType(ref Expression left, ref Expression right)
        {
            if (left.Type == right.Type)
            {
                return;
            }

            int leftLevel = _numberTypeLevel[left.Type];
            int rightLevel = _numberTypeLevel[right.Type];

            if (leftLevel > rightLevel)
            {
                right = Expression.Convert(right, left.Type);
            }
            else
            {
                left = Expression.Convert(left, right.Type);
            }
        }

        /// <summary>
        /// 读取方法调用中的参数
        /// </summary>
        /// <param name="priorityLevel">当前操作的优先级</param>
        /// <returns></returns>
        private List<Expression> ReadParams(string startSymbol, bool hasReadPre)
        {
            // 读前置括号
            if (!hasReadPre)
            {
                string startBracket = _parser.ReadString();
                ParseException.Assert(startBracket, startSymbol, _parser.Index);
            }

            // 读参数
            List<Expression> listParam = new List<Expression>();
            bool newIsClosedWrap = false;
            while (!newIsClosedWrap)
            {
                Expression expression = ReadExpression(0, startSymbol, out newIsClosedWrap);
                if (expression != null)
                {
                    listParam.Add(expression);
                }
            }
            return listParam;
        }

        /// <summary>
        /// 读取圆括号中的字符串
        /// </summary>
        /// <param name="hasReadPre">是否已经读取了前置括号</param>
        /// <returns></returns>
        private string GetBracketString(bool hasReadPre)
        {
            // 保存还原点
            CodeParserPosition position = _parser.SavePosition();

            // 读(
            if (!hasReadPre)
            {
                string strStart = this._parser.ReadString();
                ParseException.Assert(strStart, "(", _parser.Index);
            }

            // 读中间内容
            StringBuilder sb = new StringBuilder();
            string str = null;
            while ((str = this._parser.ReadString()) != ")")
            {
                // 读到(则表示括号有嵌套，还原，返回null
                if (str == "(")
                {
                    _parser.RevertPosition(position);
                    return null;
                }

                sb.Append(str);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 读类型字符串
        /// </summary>
        /// <returns></returns>
        private string ReadTypeString()
        {
            StringBuilder sb = new StringBuilder();
            string str = this._parser.ReadString();
            while (true)
            {
                sb.Append(str);
                string point = this._parser.Peek();
                if (point == ".")
                {
                    this._parser.ReadString();
                    sb.Append(point);
                }
                else
                {
                    break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取下一个操作的优先级。-1表示没有操作。
        /// </summary>
        /// <returns></returns>
        private int TryGetNextPriorityLevel()
        {
            string nextString = _parser.Peek();
            if (string.IsNullOrEmpty(nextString) || nextString == ";" || nextString == "}" || nextString == "," || nextString == ":")
            {
                return -1;
            }

            return GetOperatorLevel(nextString, false);
        } 
        #endregion
    }
}
