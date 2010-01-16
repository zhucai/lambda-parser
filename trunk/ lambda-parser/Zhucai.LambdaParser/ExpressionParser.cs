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
        #region all Parse()

        /// <summary>
        /// 解析Lambda表达式代码
        /// </summary>
        /// <param name="lambdaCode">lambda表达式代码。如：m=>m.ToString()</param>
        /// <param name="namespaces">命名空间集</param>
        static public LambdaExpression Parse(string lambdaCode, params string[] namespaces)
        {
            return ParseCore<Delegate>(null, lambdaCode, null, false, namespaces);
        }

        /// <summary>
        /// 解析Lambda表达式代码
        /// </summary>
        /// <param name="lambdaCode">lambda表达式代码。如：m=>m.ToString()</param>
        /// <param name="namespaces">命名空间集</param>
        static public LambdaExpression Parse(string lambdaCode, Type defaultInstance, params string[] namespaces)
        {
            return ParseCore<Delegate>(null, lambdaCode, defaultInstance, false, namespaces);
        }

        /// <summary>
        /// 解析Lambda表达式代码
        /// </summary>
        /// <param name="lambdaCode">lambda表达式代码。如：m=>m.ToString()</param>
        /// <param name="delegateType">委托类型</param>
        /// <param name="firstTypeIsDefaultInstance">是否第一个类型是默认实例</param>
        /// <param name="namespaces">命名空间集</param>
        static public LambdaExpression Parse(Type delegateType, string lambdaCode, params string[] namespaces)
        {
            return ParseCore<Delegate>(delegateType, lambdaCode, null, false, namespaces);
        }

        /// <summary>
        /// 解析Lambda表达式代码
        /// </summary>
        /// <param name="lambdaCode">lambda表达式代码。如：m=>m.ToString()</param>
        /// <param name="delegateType">委托类型</param>
        /// <param name="firstTypeIsDefaultInstance">是否第一个类型是默认实例</param>
        /// <param name="namespaces">命名空间集</param>
        static public LambdaExpression Parse(Type delegateType, string lambdaCode, bool firstTypeIsDefaultInstance, params string[] namespaces)
        {
            return ParseCore<Delegate>(delegateType, lambdaCode, null, firstTypeIsDefaultInstance, namespaces);
        }

        /// <summary>
        /// 解析Lambda表达式代码
        /// </summary>
        /// <param name="lambdaCode">lambda表达式代码。如：m=>m.ToString()</param>
        /// <param name="namespaces">命名空间集</param>
        static public Expression<TDelegate> Parse<TDelegate>(string lambdaCode, params string[] namespaces)
        {
            return (Expression<TDelegate>)ParseCore<TDelegate>(null, lambdaCode, null, false, namespaces);
        }

        /// <summary>
        /// 解析Lambda表达式代码
        /// </summary>
        /// <param name="lambdaCode">lambda表达式代码。如：m=>m.ToString()</param>
        /// <param name="namespaces">命名空间集</param>
        static public Expression<TDelegate> Parse<TDelegate>(string lambdaCode, bool firstTypeIsDefaultInstance, params string[] namespaces)
        {
            return (Expression<TDelegate>)ParseCore<TDelegate>(null, lambdaCode, null, firstTypeIsDefaultInstance, namespaces);
        }

        #endregion


        #region all Compile()

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
        /// <param name="namespaces">命名空间集</param>
        static public Delegate Compile(string lambdaCode, Type defaultInstance, params string[] namespaces)
        {
            return Parse(lambdaCode, defaultInstance, namespaces).Compile();
        }

        /// <summary>
        /// 解析Lambda表达式代码并编译成委托
        /// </summary>
        /// <param name="lambdaCode">lambda表达式代码。如：m=>m.ToString()</param>
        /// <param name="delegateType">委托类型</param>
        /// <param name="namespaces">命名空间集</param>
        static public Delegate Compile(Type delegateType, string lambdaCode, params string[] namespaces)
        {
            return Parse(delegateType, lambdaCode, namespaces).Compile();
        }
        
        /// <summary>
        /// 解析Lambda表达式代码并编译成委托
        /// </summary>
        /// <param name="lambdaCode">lambda表达式代码。如：m=>m.ToString()</param>
        /// <param name="delegateType">委托类型</param>
        /// <param name="firstTypeIsDefaultInstance">是否第一个类型是默认实例</param>
        /// <param name="namespaces">命名空间集</param>
        static public Delegate Compile(Type delegateType, string lambdaCode, bool firstTypeIsDefaultInstance, params string[] namespaces)
        {
            return Parse(delegateType, lambdaCode, firstTypeIsDefaultInstance, namespaces).Compile();
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
        
        /// <summary>
        /// 解析Lambda表达式代码
        /// </summary>
        /// <param name="lambdaCode">lambda表达式代码。如：m=>m.ToString()</param>
        /// <param name="namespaces">命名空间集</param>
        static public TDelegate Compile<TDelegate>(string lambdaCode, bool firstTypeIsDefaultInstance, params string[] namespaces)
        {
            return Parse<TDelegate>(lambdaCode, firstTypeIsDefaultInstance, namespaces).Compile();
        }

        #endregion


        #region private method.内部方法

        /// <summary>
        /// 解析Lambda表达式代码
        /// </summary>
        /// <param name="lambdaCode">lambda表达式代码。如：m=>m.ToString()</param>
        /// <param name="namespaces">命名空间集</param>
        static private LambdaExpression ParseCore<TDelegate>(Type delegateType, string lambdaCode, Type defaultInstanceType, bool firstTypeIsDefaultInstance, params string[] namespaces)
        {
            ExpressionParserCore<TDelegate> parser = new ExpressionParserCore<TDelegate>(delegateType, lambdaCode, defaultInstanceType, firstTypeIsDefaultInstance);
            if (namespaces != null && namespaces.Length > 0)
            {
                parser.Namespaces.AddRange(namespaces);
            }
            return parser.ToLambdaExpression();
        }

        #endregion
    }
}
