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
            return ParseCore<Delegate>(null, lambdaCode, null, false,null, namespaces);
        }

        /// <summary>
        /// 解析Lambda表达式代码
        /// </summary>
        /// <param name="lambdaCode">lambda表达式代码。如：m=>m.ToString()</param>
        /// <param name="namespaces">命名空间集</param>
        static public LambdaExpression Parse(string lambdaCode, Type defaultInstance, params string[] namespaces)
        {
            return ParseCore<Delegate>(null, lambdaCode, defaultInstance, false,null, namespaces);
        }

        /// <summary>
        /// 解析Lambda表达式代码
        /// </summary>
        /// <param name="lambdaCode">lambda表达式代码。如：m=>m.ToString()</param>
        /// <param name="namespaces">命名空间集</param>
        static public LambdaExpression Parse(string lambdaCode, Type defaultInstance,Type[] paramTypes, params string[] namespaces)
        {
            return ParseCore<Delegate>(null, lambdaCode, defaultInstance, false, paramTypes, namespaces);
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
            return ParseCore<Delegate>(delegateType, lambdaCode, null, false,null, namespaces);
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
            return ParseCore<Delegate>(delegateType, lambdaCode, null, firstTypeIsDefaultInstance, null, namespaces);
        }

        /// <summary>
        /// 解析Lambda表达式代码
        /// </summary>
        /// <param name="lambdaCode">lambda表达式代码。如：m=>m.ToString()</param>
        /// <param name="namespaces">命名空间集</param>
        static public Expression<TDelegate> Parse<TDelegate>(string lambdaCode, params string[] namespaces)
        {
            return (Expression<TDelegate>)ParseCore<TDelegate>(null, lambdaCode, null, false, null, namespaces);
        }

        /// <summary>
        /// 解析Lambda表达式代码
        /// </summary>
        /// <param name="lambdaCode">lambda表达式代码。如：m=>m.ToString()</param>
        /// <param name="namespaces">命名空间集</param>
        static public Expression<TDelegate> Parse<TDelegate>(string lambdaCode, bool firstTypeIsDefaultInstance, params string[] namespaces)
        {
            return (Expression<TDelegate>)ParseCore<TDelegate>(null, lambdaCode, null, firstTypeIsDefaultInstance, null, namespaces);
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

        #region all Exec()

        /// <summary>
        /// 以instance为上下文，执行代码
        /// ($0表示instance，(可省略$0)；$1表示objects的第一个对象；$2表示objects的第二个对象....)
        /// </summary>
        /// <typeparam name="T">返回的结果类型</typeparam>
        /// <param name="instance">执行代码以此对象为上下文(在code中用$0表示，$0可省略)</param>
        /// <param name="code">被执行代码</param>
        /// <param name="namespaces">引入命名空间</param>
        /// <param name="objects">参数对象</param>
        /// <returns></returns>
        static public T Exec<T>(object instance, string code, string[] namespaces, params object[] objects)
        {
            object[] allObjs = new object[objects.Length + 1];
            allObjs[0] = instance;
            Array.Copy(objects, 0, allObjs, 1, objects.Length);

            object[] inputObjs = new object[objects.Length + 2];
            inputObjs[1] = inputObjs[0] = instance;
            Array.Copy(objects, 0, inputObjs, 2, objects.Length);

            // 从allObjs得到：[objectTypeName] $1,[objectTypeNameTypeName] $2...
            string lambdaParams = string.Join(",", allObjs.Select((m, i) => "$" + i).ToArray());
            Type[] paramTypes = inputObjs.Select(m => m.GetType()).ToArray();

            string newCode = string.Format("({0})=>{1}",lambdaParams,code);

            return (T)Parse(newCode, instance.GetType(), paramTypes, namespaces).Compile().DynamicInvoke(inputObjs);
        }
        
        /// <summary>
        /// 以instance为上下文，执行代码
        /// ($0表示instance，(可省略$0)；$1表示objects的第一个对象；$2表示objects的第二个对象....)
        /// </summary>
        /// <param name="instance">执行代码以此对象为上下文(在code中用$0表示，$0可省略)</param>
        /// <param name="code">被执行代码</param>
        /// <param name="namespaces">引入命名空间</param>
        /// <param name="objects">参数对象</param>
        /// <returns></returns>
        static public object Exec(object instance, string code, string[] namespaces, params object[] objects)
        {
            return Exec<object>(instance, code, namespaces, objects);
        }

        #endregion

        #region private method.内部方法

        /// <summary>
        /// 解析Lambda表达式代码
        /// </summary>
        /// <param name="lambdaCode">lambda表达式代码。如：m=>m.ToString()</param>
        /// <param name="namespaces">命名空间集</param>
        static private LambdaExpression ParseCore<TDelegate>(Type delegateType, string lambdaCode, Type defaultInstanceType, bool firstTypeIsDefaultInstance,Type[] paramTypes, params string[] namespaces)
        {
            ExpressionParserCore<TDelegate> parser = new ExpressionParserCore<TDelegate>(delegateType, lambdaCode, defaultInstanceType,paramTypes, firstTypeIsDefaultInstance);
            if (namespaces != null && namespaces.Length > 0)
            {
                parser.Namespaces.AddRange(namespaces);
            }
            return parser.ToLambdaExpression();
        }

        #endregion
    }
}
