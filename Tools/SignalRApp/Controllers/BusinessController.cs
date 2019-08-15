using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;
using SignalRApp.Business;

namespace SignalRApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessController : ControllerBase
    {
        ClientWebSocket wsClient { get; }
        public BusinessController(ClientWebSocket client)
        {
            wsClient = client;
        }

        /// <summary>
        /// 心跳
        /// </summary>
        /// <returns></returns>
        public ActionResult<AppInfo> GetHeartBeat()
        {
            return AppGlobals.AppInfo;
        }

        [HttpPost]
        public void GitDevelopHook()
        {
        }
    }
}