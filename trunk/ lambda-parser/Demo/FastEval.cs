using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using System.Web.UI;
using Zhucai.LambdaParser;

namespace Demo
{
    // 原类来自老赵的blog：http://www.cnblogs.com/jeffreyzhao/archive/2009/01/09/dynamicpropertyaccessor-and-fasteval.html

    // 老类
    public class DynamicPropertyAccessor
    {
        private Func<object, object> m_getter;

        public DynamicPropertyAccessor(Type type, string propertyName)
            : this(type.GetProperty(propertyName))
        { }

        public DynamicPropertyAccessor(PropertyInfo propertyInfo)
        {
            // target: (object)((({TargetType})instance).{Property})

            // preparing parameter, object type
            ParameterExpression instance = Expression.Parameter(
                typeof(object), "instance");

            // ({TargetType})instance
            Expression instanceCast = Expression.Convert(
                instance, propertyInfo.ReflectedType);

            // (({TargetType})instance).{Property}
            Expression propertyAccess = Expression.Property(
                instanceCast, propertyInfo);

            // (object)((({TargetType})instance).{Property})
            UnaryExpression castPropertyValue = Expression.Convert(
                propertyAccess, typeof(object));

            // Lambda expression
            Expression<Func<object, object>> lambda =
                Expression.Lambda<Func<object, object>>(
                    castPropertyValue, instance);

            this.m_getter = lambda.Compile();
        }

        public object GetValue(object o)
        {
            return this.m_getter(o);
        }
    }

    // 新类
    public class NewDynamicPropertyAccessor
    {
        private Func<object, object> m_getter;

        public NewDynamicPropertyAccessor(Type type, string propertyName)
            : this(type.GetProperty(propertyName))
        { }

        public NewDynamicPropertyAccessor(PropertyInfo propertyInfo)
        {
            // target: (object)((({TargetType})instance).{Property})
            string code = string.Format("m=>(object)((({0})m).{1})", propertyInfo.ReflectedType.FullName,propertyInfo.Name);
            this.m_getter = ExpressionParser.Compile<Func<object, object>>(code);
        }

        public object GetValue(object o)
        {
            return this.m_getter(o);
        }
    }

    public class DynamicPropertyAccessorCache
    {
        private object m_mutex = new object();
        private Dictionary<Type, Dictionary<string, NewDynamicPropertyAccessor>> m_cache =
            new Dictionary<Type, Dictionary<string, NewDynamicPropertyAccessor>>();

        public NewDynamicPropertyAccessor GetAccessor(Type type, string propertyName)
        {
            NewDynamicPropertyAccessor accessor;
            Dictionary<string, NewDynamicPropertyAccessor> typeCache;

            if (this.m_cache.TryGetValue(type, out typeCache))
            {
                if (typeCache.TryGetValue(propertyName, out accessor))
                {
                    return accessor;
                }
            }

            lock (m_mutex)
            {
                if (!this.m_cache.ContainsKey(type))
                {
                    this.m_cache[type] = new Dictionary<string, NewDynamicPropertyAccessor>();
                }

                accessor = new NewDynamicPropertyAccessor(type, propertyName);
                this.m_cache[type][propertyName] = accessor;

                return accessor;
            }
        }
    }

    public static class FastEvalExtensions
    {
        private static DynamicPropertyAccessorCache s_cache =
            new DynamicPropertyAccessorCache();

        public static object FastEval(this Control control, object o, string propertyName)
        {
            return s_cache.GetAccessor(o.GetType(), propertyName).GetValue(o);
        }

        public static object FastEval(this TemplateControl control, string propertyName)
        {
            return control.FastEval(control.Page.GetDataItem(), propertyName);
        }
    }
}
