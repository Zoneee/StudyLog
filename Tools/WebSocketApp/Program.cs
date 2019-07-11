using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebSocketApp
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            Application.ApplicationExit += Application_ApplicationExit;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var logger = LoggerHelper.CreateLoggerHelper(System.IO.Path.Combine(DateTime.Now.ToString("yyyy-MM-dd"), "exception.log"));
            logger.Log(NLog.LogLevel.Fatal, (e.ExceptionObject as Exception).ToString());
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
        }
    }
}
