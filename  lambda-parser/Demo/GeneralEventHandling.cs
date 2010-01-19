using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using Zhucai.LambdaParser;

// 来自装配脑袋的blog：http://www.cnblogs.com/Ninputer/archive/2009/09/08/expression_tree3.html
namespace Demo
{

    public static class GeneralEventHandling
    {
        static object GeneralHandler(params object[] args)
        {
            Console.WriteLine("您的事件发生了说");
            System.Windows.Forms.MessageBox.Show("您的事件发生了说");
            return null;
        }

        // 原函数
        public static void AttachGeneralHandler(object target, EventInfo targetEvent)
        {
            //获得事件响应程序的委托类型
            var delegateType = targetEvent.EventHandlerType;

            //这个委托的Invoke方法有我们所需的签名信息
            MethodInfo invokeMethod = delegateType.GetMethod("Invoke");

            //按照这个委托制作所需要的参数
            ParameterInfo[] parameters = invokeMethod.GetParameters();
            ParameterExpression[] paramsExp = new ParameterExpression[parameters.Length];
            Expression[] argsArrayExp = new Expression[parameters.Length];

            //参数一个个转成object类型。有些本身即是object，管他呢……
            for (int i = 0; i < parameters.Length; i++)
            {
                paramsExp[i] = Expression.Parameter(parameters[i].ParameterType, parameters[i].Name);
                argsArrayExp[i] = Expression.Convert(paramsExp[i], typeof(Object));
            }

            //调用我们的GeneralHandler
            MethodInfo executeMethod = typeof(GeneralEventHandling).GetMethod(
                "GeneralHandler", BindingFlags.Static | BindingFlags.NonPublic);

            Expression lambdaBodyExp =
                Expression.Call(null, executeMethod, Expression.NewArrayInit(typeof(Object), argsArrayExp));

            //如果有返回值，那么将返回值转换成委托要求的类型
            //如果没有返回值就这样搁那里就成了
            if (!invokeMethod.ReturnType.Equals(typeof(void)))
            {
                //这是有返回值的情况
                lambdaBodyExp = Expression.Convert(lambdaBodyExp, invokeMethod.ReturnType);
            }

            //组装到一起
            LambdaExpression dynamicDelegateExp = Expression.Lambda(delegateType, lambdaBodyExp, paramsExp);

            //我们创建的Expression是这样的一个函数：
            //(委托的参数们) => GeneralHandler(new object[] { 委托的参数们 })

            //编译
            Delegate dynamiceDelegate = dynamicDelegateExp.Compile();

            //完成!
            targetEvent.AddEventHandler(target, dynamiceDelegate);
        }

        // 新函数
        public static void NewAttachGeneralHandler(object target, EventInfo targetEvent)
        {
            //获得事件响应程序的委托类型
            var delegateType = targetEvent.EventHandlerType;

            //这个委托的Invoke方法有我们所需的签名信息
            MethodInfo invokeMethod = delegateType.GetMethod("Invoke");
            ParameterInfo[] parameters = invokeMethod.GetParameters();

            //我们创建的Expression是这样的一个函数：
            //(委托的参数们) => (返回值类型)GeneralHandler(new object[] { 委托的参数们 })
            string lambdaCode = string.Format("({0})=>{1}GeneralEventHandling.GeneralHandler(new object[]{{{2}}})",
                string.Join(",", parameters.Select(m => m.Name).ToArray()),
                invokeMethod.ReturnType.Equals(typeof(void))?"":"("+invokeMethod.ReturnType.FullName+")",
                string.Join(",", parameters.Select(m => m.Name).ToArray()));

            Delegate dynamiceDelegate = ExpressionParser.Compile(delegateType, lambdaCode, "Demo"); // 最后一个参数Demo是命名空间

            //完成!
            targetEvent.AddEventHandler(target, dynamiceDelegate);
        }
    }
}
