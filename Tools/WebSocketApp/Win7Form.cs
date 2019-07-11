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
using forestpeas.WebSocketClient;

namespace WebSocketApp
{
    public partial class Win7Form : Form
    {
        WsClient wsClient;
        public Win7Form()
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
            wsClient?.Dispose();
            wsClient = await WsClient.ConnectAsync(new Uri(wsAddress));
            Console.WriteLine("建立链接！");
        }

        void UnLinkWs()
        {
            wsClient?.Dispose();
            Console.WriteLine("断开链接！");
        }

        async Task SendMessage()
        {
            var message = MessageTBox.Text;
            await wsClient.SendStringAsync(message);
            Console.WriteLine($"发送消息：“{message}”");
        }

        async Task ReceiveMessage()
        {
            var message = await wsClient.ReceiveStringAsync();
            Console.WriteLine($"接收消息：“{message}”");
        }

        private async void LinkBtn_Click(object sender, EventArgs e)
        {
            await LinkWs();
        }

        private void UnLinkBtn_Click(object sender, EventArgs e)
        {
            UnLinkWs();
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
}
