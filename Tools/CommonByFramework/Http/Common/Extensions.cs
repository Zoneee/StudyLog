using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Jxl
{
    /// <summary>
    /// 扩展方法
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// 根据指定的key从一个dictionary中获取对应的value，如果key不存在则返回value的类型的默认值
        /// </summary>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            return dictionary.TryGetValue(key, out TValue value) ? value : default(TValue);
        }

        /// <summary>
        /// 获取异常的完整描述消息（包括InnerException）
        /// </summary>
        public static string GetFullMessage(this Exception ex)
        {
            if (ex.InnerException == null)
            {
                return ex.Message;
            }

            return $"{ex.Message} ---> {ex.InnerException.GetFullMessage()}";
        }

        /// <summary>
        /// 获取异常的完整描述消息（包括InnerException），包括异常类型，包括Data中消息
        /// </summary>
        public static string GetFullMessageWithTypeName(this Exception ex)
        {
            if (ex.InnerException == null)
            {
                return $"{ex.GetType().FullName}: {ex.Message}";
            }

            return $"{ex.GetExceptionData()} ---> {ex.GetType().FullName}: {ex.Message} ---> {ex.InnerException.GetFullMessageWithTypeName()}";
        }

        /// <summary>
        /// 获取异常的<see cref="Exception.Data"/>（包括InnerException）
        /// </summary>
        public static string GetExceptionData(this Exception ex)
        {
            var sb = new StringBuilder();
            do
            {
                if (ex.Data.Count != 0)
                {
                    sb.Append($"{ex.Message} ---> ");
                }

                foreach (DictionaryEntry item in ex.Data)
                {
                    sb.Append($"{item.Key}:{item.Value} ");
                }

                ex = ex.InnerException;

            } while (ex != null);

            return sb.ToString();
        }
    }
}
