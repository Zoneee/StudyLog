using ExNameChanger;
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
            Queue<ProcessInfo> processes = new Queue<ProcessInfo>();
            List<Monitor> monitors = new List<Monitor>();
            ConfigInfo config = new ConfigInfo();

            WriteHelper writeHelper = new WriteHelper();
            Console.SetOut(writeHelper);

            for (int i = 0; processes.Count < 2; i++)
            {
                using (Process process = ProcessHelper.StartProcess(@"C:\Users\Zoneee\Desktop\other\Changer\ExNameChanger.exe"))
                {
                    processes.Enqueue(new ProcessInfo(process));
                }
            }

            config.Processes = processes.ToArray();
            XmlSerializer serializer = new XmlSerializer(typeof(ConfigInfo));
            using (TextWriter writer = new StreamWriter("config.xml"))
            {
                serializer.Serialize(writer, config);
            }

            foreach (var item in config.Processes)
            {
                var m = new Monitor(item);
                var open = m.Open();
                monitors.Add(m);
            }

            await Task.Delay(TimeSpan.FromSeconds(60));


            foreach (var item in monitors)
            {
                await item.Close();
            }

            using (TextWriter writer = new StreamWriter("config.xml"))
            {
                serializer.Serialize(writer, config);
            }

            Console.ReadLine();
        }


    }

    public class ProcessHelper
    {
        public static Process StartProcess(string startPath)
        {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(startPath);
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.WorkingDirectory = startPath;
            process.StartInfo.FileName = startPath;
            process.EnableRaisingEvents = true;
            process.Start();
            return process;
        }

        public static void KillProcess(int pid)
        {
            using (Process process = Process.GetProcessById(pid))
            {
                process.Kill();
                process.WaitForExit();
            }
        }
    }

    public class Monitor
    {
        protected SemaphoreSlim slim = new SemaphoreSlim(1);
        public ProcessInfo ProcessInfo { get; private set; }
        public Monitor(ProcessInfo processInfo)
        {
            ProcessInfo = processInfo;
            cancellationTokenSource = new CancellationTokenSource();
        }

        protected Task monitorTask;
        protected CancellationTokenSource cancellationTokenSource;

        public Task Open()
        {
            if (monitorTask != null && !@monitorTask.IsCompleted)
            {
                return monitorTask;
            }

            monitorTask = Task.Run(async () =>
             {
                 while (true)
                 {
                     if (cancellationTokenSource.Token.IsCancellationRequested)
                     {
                         cancellationTokenSource.Token.ThrowIfCancellationRequested();
                     }

                     await Guard(cancellationTokenSource.Token);

                     await Deploy(cancellationTokenSource.Token);
                 }

             }, cancellationTokenSource.Token);
            return monitorTask;
        }

        public Task Close()
        {
            cancellationTokenSource?.Cancel(true);
            return Task.FromResult(true);
        }

        Task Deploy(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                var _deployType = string.Empty;
                if (string.IsNullOrWhiteSpace(_deployType))
                {
                    Console.WriteLine($"{ProcessInfo.Name}无部署任务");
                }
                else
                {
                    try
                    {
                        await slim.WaitAsync();
                        using (CancellationTokenSource deployCancellation = new CancellationTokenSource(TimeSpan.FromMinutes(30)))
                        {
                            using (CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(deployCancellation.Token, cancellationToken))
                            {
                                switch (_deployType)
                                {
                                    case "热部署":
                                        await HotDeploy(cts.Token);
                                        break;
                                    case "冷部署":
                                        await ColdDeploy(cts.Token);
                                        break;
                                }
                            }
                        }
                    }
                    finally
                    {
                        slim.Release();
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            });
        }

        Task HotDeploy(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                Console.WriteLine($"进行热部署  >>  {ProcessInfo.Name}");
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                Console.WriteLine($"热部署完成  >>  {ProcessInfo.Name}");
            }, cancellationToken);
        }

        Task ColdDeploy(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                Console.WriteLine($"进行冷部署  >>  {ProcessInfo.Name}");
                Console.WriteLine($"正在关闭 >> {ProcessInfo.Name}");
                ProcessHelper.KillProcess(ProcessInfo.PID);
                Console.WriteLine($"成功关闭  >>  {ProcessInfo.Name}");
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                Console.WriteLine($"完成文件部署  >>  {ProcessInfo.Name}");
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                Console.WriteLine($"正在重启进程 >> {ProcessInfo.Name}");
                using (var process = ProcessHelper.StartProcess(ProcessInfo.RootPath))
                {
                    ProcessInfo = new ProcessInfo(process);
                }
                Console.WriteLine($"完成重启进程  >>  {ProcessInfo.Name}");
                Console.WriteLine($"完成冷部署  >>  {ProcessInfo.Name}");
            }, cancellationToken);
        }

        Task Guard(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                var status = string.Empty;
                if (string.IsNullOrWhiteSpace(status))
                {
                    Console.WriteLine($"{ProcessInfo.Name}心跳正常");
                }
                else
                {
                    try
                    {
                        await slim.WaitAsync();
                        Console.WriteLine($"进程无响应  >>  {ProcessInfo.Name}");
                        ProcessHelper.KillProcess(ProcessInfo.PID);
                        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                        using (var process = ProcessHelper.StartProcess(ProcessInfo.RootPath))
                        {
                            ProcessInfo = new ProcessInfo(process);
                        }
                        Console.WriteLine($"完成进程重启  >>  {ProcessInfo.Name}");
                    }
                    catch { }
                    finally
                    {
                        slim.Release();
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }, cancellationToken);

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
        public ProcessInfo() { }
        public ProcessInfo(Process process)
        {
            RootPath = process.MainModule.FileName;
            Name = process.MainModule.ModuleName;
            PID = process.Id;
        }
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
