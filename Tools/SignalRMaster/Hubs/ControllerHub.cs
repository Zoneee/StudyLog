using Microsoft.AspNetCore.SignalR;
using SignalRMaster.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRMaster.Hubs
{
    public class ControllerHub : Hub
    {
        static List<ClientInfo> _clients = new List<ClientInfo>();

        static ControllerHub()
        {
            _clients = AppGlobals.IPLists.Select(s => new ClientInfo()
            {
                IP = s,
            }).ToList();
        }

        /// <summary>
        /// 推送App信息
        /// 前端使用
        /// </summary>
        /// <returns></returns>
        public async Task GetAppInfoAsync()
        {
            while (true)
            {
                await Clients.All.SendAsync("GetClientInfo", _clients);
                await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 接收客户端任务
        /// App 端使用
        /// </summary>
        public async Task SendAppInfoAsync(ClientInfo client)
        {
            //更新信息
            var item = _clients.FirstOrDefault(s => s.IP == client.IP);
            item.Online = client.Online;
            item.TaskCount = client.TaskCount;
            item.LastTime = DateTime.Now;

            //升级版本管理
            if (client.Version < AppGlobals.Version)
            {
                if (client.Online)
                {
                    await Clients.Caller.SendAsync("Offline");
                }

                if (!client.Online && client.TaskCount < 0)
                {
                    //使用SSH
                    //执行 Docker Deploy 流程
                }
            }
        }
    }

    public class ClientInfo
    {
        public string IP { get; set; }
        public bool Online { get; set; }
        public int TaskCount { get; set; }
        public double Version { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime LastTime { get; set; }
        public string SafeLevel => GetSafeLevel().ToString();

        private SafeLevelEnum GetSafeLevel()
        {
            var t = DateTime.Now.Subtract(LastTime).TotalMinutes;
            if (t > 5)
            {
                return SafeLevelEnum.Error;
            }
            else if (t > 1)
            {
                return SafeLevelEnum.Warn;
            }
            return SafeLevelEnum.Normal;
        }
    }

    public enum SafeLevelEnum
    {
        Normal,
        Warn,
        Error,
    }
}
