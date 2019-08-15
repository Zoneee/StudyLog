using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;
using SignalRMaster.Business;
using SignalRMaster.Hubs;

namespace SignalRMaster.Controllers
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
        public ActionResult<string> GetHeartBeat()
        {
            return $"Master：{AppGlobals.Version}";
        }

        [HttpPost]
        public void GitDevelopHook()
        {
        }
    }
}