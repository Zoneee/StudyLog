using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignalRApp.Business
{
    public static class AppGlobals
    {
        static AppGlobals()
        {
            TaskList = new List<string>();
            WsClient = new ClientWebSocket();
            CancellationTokenSource = new CancellationTokenSource();
        }

        public static AppInfo AppInfo;
        public readonly static List<string> TaskList;
        public readonly static ClientWebSocket WsClient;
        public readonly static CancellationTokenSource CancellationTokenSource;

        public static async Task SendAppInfoAsync()
        {
            while (true)
            {
                CancellationTokenSource.Token.ThrowIfCancellationRequested();

                var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(AppInfo));
                await WsClient.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, false, CancellationTokenSource.Token);
                await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            }
        }
    }

    public class AppInfo
    {
        public AppInfo()
        {
            StartTime = DateTime.Now;
        }
        public AppInfo(double version) : this()
        {
            Version = version;
        }
        public string IP { get => Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault().AddressFamily.ToString(); }
        public bool Online { get; set; }
        public int TaskCount { get => AppGlobals.TaskList.Count; }
        /// <summary>
        /// 从配置文件中读取
        /// </summary>
        public double Version { get; }
        public DateTime StartTime { get; }
    }
}
