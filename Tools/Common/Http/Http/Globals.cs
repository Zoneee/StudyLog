using Jxl.Http;
using Jxl.Logging;
using Monitor.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jxl
{
    public static class Globals
    {

        /// <summary>
        /// 封装一个Task
        /// 休眠间隔：1秒
        /// </summary>
        /// <typeparam name="T">接收类型</typeparam>
        /// <param name="func">需要执行的动作 第一个是与T类型相同的return结果。第二个参数是是否完成标识，true将return，false将继续执行</param>
        /// <param name="cancellationToken">TaskToken</param>
        public static Task GetTask(Func<bool> func, CancellationToken cancellationToken)
        {
            return GetTask(func, cancellationToken, 1);
        }

        /// <summary>
        /// 封装一个Task
        /// </summary>
        /// <typeparam name="T">接收类型</typeparam>
        /// <param name="func">需要执行的动作 第一个是与T类型相同的return结果。第二个参数是是否完成标识，true将return，false将继续执行</param>
        /// <param name="cancellationToken">TaskToken</param>
        /// <param name="delayTime">休眠间隔时间：秒</param>
        public static Task GetTask(Func<bool> func, CancellationToken cancellationToken, int delayTime)
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    await Task.Delay(TimeSpan.FromSeconds(delayTime), cancellationToken);

                    if (!func())
                    {
                        continue;
                    }
                    return;
                }
            }, cancellationToken);
        }

        /// <summary>
        /// 封装一个Task
        /// </summary>
        /// 休眠间隔：1秒
        /// <typeparam name="T">接收类型</typeparam>
        /// <param name="func">需要执行的动作 第一个是与T类型相同的return结果。第二个参数是是否完成标识，true将return，false将继续执行</param>
        /// <param name="cancellationToken">TaskToken</param>
        public static Task<T> GetTask<T>(Func<Tuple<T, bool>> func, CancellationToken cancellationToken)
        {
            return GetTask(func, cancellationToken, 1);
        }

        /// <summary>
        /// 封装一个Task
        /// </summary>
        /// <typeparam name="T">接收类型</typeparam>
        /// <param name="func">需要执行的动作 第一个是与T类型相同的return结果。第二个参数是是否完成标识，true将return，false将继续执行</param>
        /// <param name="cancellationToken">TaskToken</param>
        /// <param name="delayTime">休眠间隔时间：秒</param>
        public static Task<T> GetTask<T>(Func<Tuple<T, bool>> func, CancellationToken cancellationToken, int delayTime)
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    await Task.Delay(TimeSpan.FromSeconds(delayTime), cancellationToken);

                    var t = func();
                    if (!t.Item2)
                    {
                        continue;
                    }
                    return t.Item1;
                }
            }, cancellationToken);
        }
    }

    
}
