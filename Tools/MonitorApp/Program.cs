using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace MonitorApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            SemaphoreSlim slim = new SemaphoreSlim(1);
            var allProcesses = Process.GetProcesses();
            Queue<ProcessInfo> processes = new Queue<ProcessInfo>();

            for (int i = 0; processes.Count < 2; i++)
            {
                var item = allProcesses[i];
                try
                {
                    var info = new ProcessInfo();
                    info.RootPath = item.MainModule.FileName;
                    info.Name = item.MainModule.ModuleName;
                    info.PID = item.Id;
                    processes.Enqueue(info);
                    Console.WriteLine($"{info.Name}  >>  {info.RootPath}");

                }
                catch { }
            }

            var config = new ConfigInfo() { Processes = processes.ToArray() };
            XmlSerializer serializer = new XmlSerializer(typeof(ConfigInfo));
            using (TextWriter writer = new StreamWriter("config.xml"))
            {
                serializer.Serialize(writer, config);
            }

            CancellationTokenSource deployCts = new CancellationTokenSource();
            CancellationTokenSource guardCts = new CancellationTokenSource();

            var guardTask = Task.Run(async () =>
            {
                try
                {
                    await slim.WaitAsync();
                    await Task.Delay(TimeSpan.FromMinutes(3), guardCts.Token);
                    if (!guardCts.Token.IsCancellationRequested)
                    {
                        guardCts.Token.ThrowIfCancellationRequested();
                    }
                    processes.Clear();
                    foreach (var p in config.Processes)
                    {
                        if (!guardCts.Token.IsCancellationRequested)
                        {
                            guardCts.Token.ThrowIfCancellationRequested();
                        }

                        Console.WriteLine($"进程无响应  >>  {p.Name}");
                        var process = Process.GetProcessById(p.PID);
                        while (!process.HasExited)
                        {
                            Console.WriteLine($"正在关闭 >> {p.Name}");
                            process.Kill();
                            await Task.Delay(TimeSpan.FromSeconds(3), guardCts.Token);
                        }
                        await Task.Delay(TimeSpan.FromSeconds(10), guardCts.Token);
                        var newP = Process.Start(p.RootPath);
                        Console.WriteLine($"完成进程重启  >>  {p.Name}");
                        p.PID = newP.Id;
                        processes.Enqueue(p);
                    }
                }
                finally
                {
                    slim.Release();
                }
            }, guardCts.Token);


            var hotTask = Task.Run(async () =>
           {
               try
               {
                   await slim.WaitAsync();
                   foreach (var p in config.Processes)
                   {
                       await Task.Delay(TimeSpan.FromSeconds(10), deployCts.Token);
                       Console.WriteLine($"进行热部署  >>  {p.Name}");
                       await Task.Delay(TimeSpan.FromSeconds(3), deployCts.Token);
                       Console.WriteLine($"热部署完成  >>  {p.Name}");
                   }
               }
               finally
               {
                   slim.Release();
               }
           }, deployCts.Token);

            try
            {
                await Task.WhenAll(hotTask, guardTask);
            }
            catch (Exception e)
            {
                Console.WriteLine($"热部署异常：{e.ToString()}");
            }

            var coldTask = Task.Run(async () =>
           {
               guardCts.Cancel();
               processes.Clear();
               foreach (var p in config.Processes)
               {
                   Console.WriteLine($"进行冷部署  >>  {p.Name}");
                   var process = Process.GetProcessById(p.PID);
                   while (!process.HasExited)
                   {
                       Console.WriteLine($"正在关闭 >> {p.Name}");
                       process.Kill();
                       await Task.Delay(TimeSpan.FromSeconds(3), deployCts.Token);
                   }
                   Console.WriteLine($"成功关闭  >>  {p.Name}");
                   await Task.Delay(TimeSpan.FromSeconds(10), deployCts.Token);
                   Console.WriteLine($"完成文件部署  >>  {p.Name}");
                   await Task.Delay(TimeSpan.FromSeconds(5), deployCts.Token);
                   var newP = Process.Start(p.RootPath);
                   Console.WriteLine($"完成进程重启  >>  {p.Name}");
                   p.PID = newP.Id;
                   processes.Enqueue(p);
                   Console.WriteLine($"完成冷部署  >>  {p.Name}");
               }
           }, deployCts.Token);

            try
            {
                await slim.WaitAsync();
                await coldTask;
            }
            catch (Exception e)
            {
                Console.WriteLine($"冷部署异常：{e.ToString()}");
            }
            finally
            {
                slim.Release();
            }

            await Task.Delay(TimeSpan.FromSeconds(5));

            try
            {
                await slim.WaitAsync();
                await guardTask;
            }
            catch (Exception e)
            {
                Console.WriteLine($"守护异常：{e.ToString()}");
            }
            finally
            {
                slim.Release();
            }
            deployCts?.Cancel();
            guardCts?.Cancel();
            Console.ReadLine();
        }
    }

    [XmlRoot("root")]
    public class ConfigInfo
    {
        [XmlArray]
        public ProcessInfo[] Processes { get; set; }
    }

    public class ProcessInfo
    {
        public int PID { get; set; }
        public string Name { get; set; }
        public string RootPath { get; set; }
    }

    internal static class Win32API
    {
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);
    }
}
