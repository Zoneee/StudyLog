using Microsoft.AspNetCore.SignalR;
using SignalRApp.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRApp.Hubs
{
    public class ClientHub : Hub
    {
        public void Online()
        {
            AppGlobals.AppInfo.Online = true;
        }

        public void Offline()
        {
            AppGlobals.AppInfo.Online = false;
        }
    }
}
