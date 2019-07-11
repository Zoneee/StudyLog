using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebSocketApp
{
    public partial class Form1 : Form
    {
        ClientWebSocket client = new ClientWebSocket();
        CancellationTokenSource cancellation = new CancellationTokenSource();
        public Form1()
        {
            InitializeComponent();
            var writer = new ConsoleLogWriter(LogTBox);
            Console.SetOut(writer);
        }

        /**
         * 建立链接
         * 发送消息
         * 接受消息
         * 打印日志
         */

        async Task LinkWs()
        {
            var wsAddress = WsAddressTBox.Text.Trim();
            client?.Dispose();
            client = new ClientWebSocket();
            await client.ConnectAsync(new Uri(wsAddress), cancellation.Token);
            Console.WriteLine("建立链接！");
        }

        async Task UnLinkWs()
        {
            await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "手动关闭！", cancellation.Token);
            Console.WriteLine("断开链接！");
        }

        async Task SendMessage()
        {
            var message = MessageTBox.Text;
            var buffer = Encoding.UTF8.GetBytes(message);
            await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancellation.Token);
            Console.WriteLine($"发送消息：“{message}”");
        }

        async Task ReceiveMessage()
        {
            var bytes = new List<byte>();
            bool flag;
            do
            {
                var buffer = new byte[1024];
                var array = new ArraySegment<byte>(buffer);
                var result = await client.ReceiveAsync(array, cancellation.Token);
                bytes.AddRange(array.Array);
                flag = result.EndOfMessage;
            } while (!flag);
            Console.WriteLine($"接收消息：“{Encoding.UTF8.GetString(bytes.ToArray())}”");
        }

        private async void LinkBtn_Click(object sender, EventArgs e)
        {
            await LinkWs();
        }

        private async void UnLinkBtn_Click(object sender, EventArgs e)
        {
            await UnLinkWs();
        }

        private async void ReveiceBtn_Click(object sender, EventArgs e)
        {
            await ReceiveMessage();
        }

        private async void SendBtn_Click(object sender, EventArgs e)
        {
            await SendMessage();
        }
    }

    class ConsoleLogWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
        public TextBox TextBox { get; private set; }
        public LoggerHelper Logger { get; private set; }
        public ConsoleLogWriter(TextBox textBox)
        {
            TextBox = textBox;
            Logger = LoggerHelper.CreateLoggerHelper();
        }

        public override void WriteLine(string value)
        {
            var m = $"{DateTime.Now.ToString("hh:mm:ss")}  {value}{Environment.NewLine}";
            if (TextBox.InvokeRequired)
            {
                TextBox.Invoke(new Action(() =>
                {
                    TextBox.AppendText(m);
                }));
            }
            else
            {
                TextBox.AppendText(m);
            }
            //base.WriteLine(value);
            Logger.Log(LogLevel.Info, value);
        }
    }

    class LoggerHelper
    {
        public Logger Logger;
        public string Path;
        private LoggerHelper() { }

        public static LoggerHelper CreateLoggerHelper()
        {
            var logger = new LoggerHelper();
            logger.Logger = LogManager.GetCurrentClassLogger();
            logger.Path = System.IO.Path.Combine(DateTime.Now.ToString("yyyy-MM-dd"), "log.log");
            return logger;
        }

        public static LoggerHelper CreateLoggerHelper(string path)
        {
            var logger = new LoggerHelper();
            logger.Logger = LogManager.GetCurrentClassLogger();
            logger.Path = path;
            return logger;
        }

        public void Log(LogLevel level, string message)
        {
            LogEventInfo logEventInfo = new LogEventInfo()
            {
                Level = level,
                Message = message,
            };
            logEventInfo.Properties["path"] = Path;
            Logger.Log(logEventInfo);
        }
    }
}
