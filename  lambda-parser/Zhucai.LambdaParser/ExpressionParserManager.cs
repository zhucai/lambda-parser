using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Zhucai.LambdaParser
{
    public static class ExpressionParserManager
    {

        private Dictionary<CacheKey, Delegate> _dicCache = new Dictionary<CacheKey, Delegate>();
        /// <summary>
        /// 将字符串形式的Lambda表达式解析，返回一个强类型委托。
        /// 没有参数的情况下，可以省略()=>，但不建议省略。
        /// </summary>
        /// <typeparam name="TDelegate">委托类型</typeparam>
        /// <param name="code">Lambda表达式代码。</param>
        /// <param name="cache">是否缓存</param>
        /// <returns></returns>
        static public TDelegate ParseDelegate<TDelegate>(string code, bool cache)
        {
            CacheKey key = null;
            if (cache)
            {
                // 从缓存里取
                Delegate dele;
                Type type = typeof(TDelegate);
                key = new CacheKey(type, code);
                if (_dicCache.TryGetValue(key, out dele))
                {
                    return (TDelegate)(object)dele;
                }
            }

            // 缓存没有则创建
            ExpressionParser<TDelegate> parser = new ExpressionParser<TDelegate>(code);
            Expression<TDelegate> expressionDele = parser.ToExpression();
            TDelegate tDele = expressionDele.Compile();

            if (cache)
            {
                // 并加入缓存
                lock (_dicCache)
                {
                    // 由于这里lock得不严格，所以可能会重复添加，若用Add方法则可能出异常
                    _dicCache[key] = tDele as Delegate;
                }
            }
            return tDele;
        }
        /// <summary>
        /// 将字符串形式的Lambda表达式解析，返回一个强类型委托。
        /// 没有参数的情况下，可以省略()=>
        /// <typeparam name="TDelegate">委托类型</typeparam>
        /// <param name="code">Lambda表达式代码。</param>
        /// <returns></returns>
        static public TDelegate ParseDelegate<TDelegate>(string code)
        {
            return ParseDelegate<TDelegate>(code, false);
        }

        /// <summary>
        /// 用来缓存时的Key
        /// </summary>
        private class CacheKey
        {
            public Type TDelegateType { get; private set; }
            public string Code { get; set; }
            public CacheKey(Type type, string code)
            {
                this.TDelegateType = type;
                this.Code = code;
            }
            public override int GetHashCode()
            {
                return this.Code.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }

                CacheKey cacheKey2 = (CacheKey)obj;

                if (this.TDelegateType != cacheKey2.TDelegateType)
                {
                    return false;
                }

                if (this.Code != cacheKey2.Code)
                {
                    return false;
                }

                return true;
            }
        }
    }
}
