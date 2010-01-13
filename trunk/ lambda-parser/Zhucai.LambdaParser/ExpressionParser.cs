using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

namespace Zhucai.LambdaParser
{
    /// <summary>
    /// Lambda表达式的解析器
    /// </summary>
    static public class ExpressionParser
    {
        /// <summary>
        /// 解析Lambda表达式代码
        /// </summary>
        /// <param name="lambdaCode">lambda表达式代码。如：m=>m.ToString()</param>
        /// <param name="namespaces">命名空间集</param>
        static public LambdaExpression Parse(string lambdaCode, params string[] namespaces)
        {
            return ParseCore<Delegate>(lambdaCode, null, namespaces);
        }

        /// <summary>
        /// 解析Lambda表达式代码
        /// </summary>
        /// <param name="lambdaCode">lambda表达式代码。如：m=>m.ToString()</param>
        /// <param name="delegateType">委托类型</param>
        /// <param name="namespaces">命名空间集</param>
        static public LambdaExpression Parse(string lambdaCode,Type delegateType, params string[] namespaces)
        {
            return ParseCore<Delegate>(lambdaCode,delegateType, namespaces);
        }

        /// <summary>
        /// 解析Lambda表达式代码
        /// </summary>
        /// <param name="lambdaCode">lambda表达式代码。如：m=>m.ToString()</param>
        /// <param name="namespaces">命名空间集</param>
        static public Expression<TDelegate> Parse<TDelegate>(string lambdaCode, params string[] namespaces)
        {
            return (Expression<TDelegate>)ParseCore<TDelegate>(lambdaCode,null, namespaces);
        }

        /// <summary>
        /// 解析Lambda表达式代码并编译成委托
        /// </summary>
        /// <param name="lambdaCode">lambda表达式代码。如：m=>m.ToString()</param>
        /// <param name="namespaces">命名空间集</param>
        static public Delegate Compile(string lambdaCode, params string[] namespaces)
        {
            return Parse(lambdaCode, namespaces).Compile();
        }

        /// <summary>
        /// 解析Lambda表达式代码并编译成委托
        /// </summary>
        /// <param name="lambdaCode">lambda表达式代码。如：m=>m.ToString()</param>
        /// <param name="delegateType">委托类型</param>
        /// <param name="namespaces">命名空间集</param>
        static public Delegate Compile(string lambdaCode, Type delegateType, params string[] namespaces)
        {
            return Parse(lambdaCode,delegateType, namespaces).Compile();
        }

        /// <summary>
        /// 解析Lambda表达式代码并编译成委托
        /// </summary>
        /// <param name="lambdaCode">lambda表达式代码。如：m=>m.ToString()</param>
        /// <param name="namespaces">命名空间集</param>
        static public TDelegate Compile<TDelegate>(string lambdaCode, params string[] namespaces)
        {
            return Parse<TDelegate>(lambdaCode, namespaces).Compile();
        }

        #region private method.内部方法

        /// <summary>
        /// 解析Lambda表达式代码
        /// </summary>
        /// <param name="lambdaCode">lambda表达式代码。如：m=>m.ToString()</param>
        /// <param name="namespaces">命名空间集</param>
        static private LambdaExpression ParseCore<TDelegate>(string lambdaCode, Type delegateType, params string[] namespaces)
        {
            ExpressionParserCore<TDelegate> parser = new ExpressionParserCore<TDelegate>(lambdaCode, delegateType);
            if (namespaces != null && namespaces.Length > 0)
            {
                parser.Namespaces.AddRange(namespaces);
            }
            return parser.ToLambdaExpression();
        }

        #endregion
    }
}
