using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Zhucai.LambdaParser
{
    static public class FastReflection
    {
        //static private Dictionary<MemberInfo, Delegate> _memberDelegateCache = new Dictionary<MemberInfo, Delegate>();
        //static private object FastGetValueCore(MemberInfo memberInfo, object target)
        //{
        //    if (target != null)
        //    {
        //        Func<object, object> func;
        //        Delegate dele;

        //        // 先从缓存里找委托
        //        if (_memberDelegateCache.TryGetValue(memberInfo, out dele))
        //        {
        //            func = (Func<object, object>)dele;
        //        }
        //        // 缓存里没有，则创建委托
        //        else
        //        {
        //            string code = "m=>(object)((({0})m).{1})";
        //            code = string.Format(code, memberInfo.DeclaringType, memberInfo.Name);

        //            func = ExpressionParserManager.ParseDelegate<Func<object, object>>(code);

        //            lock (_memberDelegateCache)
        //            {
        //                _memberDelegateCache[memberInfo] = func;
        //            }
        //        }
        //        return func(target);
        //    }
        //    else
        //    {
        //        string code = "m=>(object)({0}.{1})";
        //        code = string.Format(code, memberInfo.DeclaringType, memberInfo.Name);

        //        Func<object> func = ExpressionParserManager.ParseDelegate<Func<object>>(code);
        //        return func();
        //    }
        //}

        //static private void FastSetValueCore(MemberInfo memberInfo, object target, object val)
        //{
        //    //if (target != null)
        //    //{
        //    //    string code = "(m,n)=>(({0})m).{1} = n";
        //    //    code = string.Format(code, memberInfo.DeclaringType, memberInfo.Name);

        //    //    Action<object, object> func = ExpressionParserManager.ParseDelegate<Action<object, object>>(code);
        //    //    func(target, val);
        //    //}
        //    //else
        //    //{
        //    //    string code = "(m,n)=>{0}.{1} = n";
        //    //    code = string.Format(code, memberInfo.DeclaringType, memberInfo.Name);

        //    //    Action<object> func = ExpressionParserManager.ParseDelegate<Action<object>>(code);
        //    //    func(val);
        //    //}
        //}

        ///// <summary>
        ///// 获取属性的值。
        ///// </summary>
        ///// <param name="propertyInfo">属性</param>
        ///// <param name="target">在此对象上获取。静态字段传null。</param>
        ///// <returns></returns>
        //static public object FastGetValue(this PropertyInfo propertyInfo, object target)
        //{
        //    return FastGetValueCore(propertyInfo, target);
        //}

        ///// <summary>
        ///// 获取字段的值
        ///// </summary>
        ///// <param name="fieldInfo">字段</param>
        ///// <param name="target">在此对象上获取。静态字段传null。</param>
        ///// <returns></returns>
        //public static object FastGetValue(this FieldInfo fieldInfo, object target)
        //{
        //    return FastGetValueCore(fieldInfo, target);
        //}

        ///// <summary>
        ///// 设置属性的值
        ///// </summary>
        ///// <param name="propertyInfo">属性</param>
        ///// <param name="target">在此对象上获取。静态字段传null。</param>
        ///// <param name="val">设置的值</param>
        //public static void FastSetValue(this PropertyInfo propertyInfo, object target, object val)
        //{
        //    FastSetValueCore(propertyInfo, target, val);
        //}

        ///// <summary>
        ///// 设置字段的值
        ///// </summary>
        ///// <param name="fieldInfo">字段</param>
        ///// <param name="target">在此对象上获取。静态字段传null。</param>
        ///// <param name="val">设置的值</param>
        //public static void FastSetValue(this FieldInfo fieldInfo, object target, object val)
        //{
        //    FastSetValueCore(fieldInfo, target, val);
        //}

        ////static public object FastInvoke(this MethodInfo methodInfo,object target, params object[] methodParams)
        ////{
        ////    if (target != null)
        ////    {
        ////        switch (methodParams.Length)
        ////        {
        ////            case 0:
        ////                {
        ////                    string code = "m=>(object)((({0})m).{1}({2}))";
        ////                    code = string.Format(code, methodInfo.DeclaringType, methodInfo.Name, "");

        ////                    Func<object, object> func = ExpressionParserManager.ParseDelegate<Func<object, object>>(code);
        ////                    return func(target);
        ////                }

        ////            default:
        ////                //todo:
        ////                return null;
        ////        }
        ////    }
        ////    else
        ////    {
        ////        switch (methodParams.Length)
        ////        {
        ////            case 0:
        ////                {
        ////                    string code = "m=>(object)({0}.{1}({2}))";
        ////                    code = string.Format(code, methodInfo.DeclaringType, methodInfo.Name, "");

        ////                    Func<object> func = ExpressionParserManager.ParseDelegate<Func<object>>(code);
        ////                    return func();
        ////                }

        ////            default:
        ////                //todo:
        ////                return null;
        ////        }
        ////    }
        ////}

        public static object FastEval(this System.Web.UI.TemplateControl control, string propertyExpression)
        {
            object dataItem = control.Page.GetDataItem();

            string code = string.Format("m=>(object)((({0})m).{1})", dataItem.GetType().FullName, propertyExpression);
            Func<object, object> func = new ExpressionParserCore<Func<object, object>>(code).ToExpression().Compile();

            return func(dataItem);
        }
    }
}
