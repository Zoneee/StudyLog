using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRMaster.Business
{
    public static class AppGlobals
    {
        /// <summary>
        /// 启动时从配置文件中读取
        /// 修改时同时修改配置文件
        /// </summary>
        public static double Version;
        /// <summary>
        /// 从配置文件中读取
        /// </summary>
        public static List<string> IPLists;
    }
}
