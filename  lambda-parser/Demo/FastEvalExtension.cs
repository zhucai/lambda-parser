using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Zhucai.LambdaParser
{
    static public class FastEvalExtension
    {
        public static object FastEval(this System.Web.UI.TemplateControl control, string propertyExpression)
        {
            Func<object,object> func = GetFunc(control, propertyExpression);
            return func(control);
        }

        // 根据类型和属性名缓存
        static Dictionary<Type, Dictionary<string, Func<object, object>>> _dicCache = new Dictionary<Type, Dictionary<string, Func<object, object>>>();

        /// <summary>
        /// 从缓存中取Func，缓存没有则创建后放到缓存
        /// </summary>
        static private Func<object, object> GetFunc(System.Web.UI.TemplateControl control, string propertyExpression)
        {
            Type dataItemType = control.Page.GetDataItem().GetType();
            if (!_dicCache.ContainsKey(dataItemType))
            {
                lock (_dicCache)
                {
                    if (!_dicCache.ContainsKey(dataItemType))
                    {
                        _dicCache.Add(dataItemType,new Dictionary<string,Func<object,object>>());
                    }
                }
            }
            Dictionary<string, Func<object, object>> dic2 = _dicCache[dataItemType];

            if (!dic2.ContainsKey(propertyExpression))
            {
                lock (_dicCache)
                {
                    if (!dic2.ContainsKey(propertyExpression))
                    {
                        dic2.Add(propertyExpression, CreateFunc(dataItemType, propertyExpression));
                    }
                }
            }
            return dic2[propertyExpression];
        }

        /// <summary>
        /// 创建Func
        /// </summary>
        static private Func<object, object> CreateFunc(Type dataItemType, string propertyExpression)
        {
            string code = string.Format("m=>(object)((({0})m).{1})", dataItemType.FullName, propertyExpression);
            return ExpressionParser.Compile<Func<object,object>>(code);
        }
    }
}
