using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zhucai.LambdaParser.ObjectDynamicExtension
{
    static public class ObjectDynamicExtension
    {
        /// <summary>
        /// 以instance为上下文，执行代码
        /// </summary>
        /// <typeparam name="T">返回的结果类型</typeparam>
        /// <param name="instance">执行代码以此对象为上下文(在code中用$0表示，$0可省略)</param>
        /// <param name="code">被执行代码</param>
        /// <param name="namespaces">引入命名空间</param>
        /// <param name="objects">参数对象(在code中用$1表示第一个对象；$2表示第二个对象....)</param>
        /// <returns></returns>
        static public T E<T>(this object instance, string code, string[] namespaces, params object[] objects)
            where T : class
        {
            return ExpressionParser.Exec<T>(instance,code,namespaces,objects);
        }

        /// <summary>
        /// 以instance为上下文，执行代码
        /// </summary>
        /// <typeparam name="T">返回的结果类型</typeparam>
        /// <param name="instance">执行代码以此对象为上下文(在code中用$0表示，$0可省略)</param>
        /// <param name="code">被执行代码</param>
        /// <param name="objects">参数对象(在code中用$1表示第一个对象；$2表示第二个对象....)</param>
        /// <returns></returns>
        static public T E<T>(this object instance, string code, params object[] objects)
        {
            return ExpressionParser.Exec<T>(instance, code, null, objects);
        }

        /// <summary>
        /// 以instance为上下文，执行代码
        /// </summary>
        /// <param name="instance">执行代码以此对象为上下文(在code中用$0表示，$0可省略)</param>
        /// <param name="code">被执行代码</param>
        /// <param name="namespaces">引入命名空间</param>
        /// <param name="objects">参数对象(在code中用$1表示第一个对象；$2表示第二个对象....)</param>
        /// <returns></returns>
        static public object E(this object instance, string code, string[] namespaces, params object[] objects)
        {
            return ExpressionParser.Exec(instance, code, namespaces, objects);
        }

        /// <summary>
        /// 以instance为上下文，执行代码
        /// </summary>
        /// <param name="instance">执行代码以此对象为上下文(在code中用$0表示，$0可省略)</param>
        /// <param name="code">被执行代码</param>
        /// <param name="objects">参数对象(在code中用$1表示第一个对象；$2表示第二个对象....)</param>
        /// <returns></returns>
        static public object E(this object instance, string code, params object[] objects)
        {
            return ExpressionParser.Exec(instance, code, null, objects);
        }
    }
}
