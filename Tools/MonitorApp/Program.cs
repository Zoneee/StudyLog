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
        static SemaphoreSlim slim = new SemaphoreSlim(1);
        static Queue<ProcessInfo> processes = new Queue<ProcessInfo>();
        static ConfigInfo config = new ConfigInfo();
        static CancellationTokenSource deployCts = new CancellationTokenSource();
        static CancellationTokenSource guardCts = new CancellationTokenSource();
        static async Task Main(string[] args)
        {
            WriteHelper writeHelper = new WriteHelper();
            Console.SetOut(writeHelper);

            for (int i = 0; processes.Count < 2; i++)
            {
                using (Process process = StartProcess(@"C:\Users\Zoneee\Desktop\other\Changer\ExNameChanger.exe"))
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

            _deployType = "热部署";
            var guardTask = OpenGuard(guardCts.Token);
            var deployTask = OpenDeploy(deployCts.Token);

            var t = Task.Run(async () =>
              {
                  for (int i = 0; ; i++)
                  {
                      switch (_deployType)
                      {
                          case "热部署":
                              _deployType = "冷部署";
                              break;
                          case "冷部署":
                              _deployType = "热部署";
                              break;
                      }
                      await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                  }
              });

            try
            {
                await deployTask;
            }
            catch (Exception e)
            {
                Console.WriteLine($"冷部署异常：{e.ToString()}");
            }



            await Task.Delay(TimeSpan.FromSeconds(5));

            try
            {
                await guardTask;
            }
            catch (Exception e)
            {
                Console.WriteLine($"守护异常：{e.ToString()}");
            }

            deployCts?.Cancel();
            guardCts?.Cancel();
            Console.ReadLine();
        }

        static Task OpenGuard(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    try
                    {
                        await slim.WaitAsync();
                        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

                        processes.Clear();
                        foreach (var p in config.Processes)
                        {

                            Console.WriteLine($"进程无响应  >>  {p.Name}");
                            KillProcess(p.PID);
                            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                            using (var newP = StartProcess(p.RootPath)) { p.PID = newP.Id; }
                            Console.WriteLine($"完成进程重启  >>  {p.Name}");
                            processes.Enqueue(p);
                        }
                    }
                    finally
                    {
                        slim.Release();
                    }
                }
            }, cancellationToken);

        }

        static Task CloseGuard(CancellationTokenSource cancellationTokenSource)
        {
            cancellationTokenSource.Cancel(true);
            return Task.FromResult(true);
        }

        static string _deployType;
        static Task OpenDeploy(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
             {
                 while (true)
                 {
                     if (cancellationToken.IsCancellationRequested)
                     {
                         cancellationToken.ThrowIfCancellationRequested();
                     }

                     await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);

                     try
                     {
                         await slim.WaitAsync();
                         switch (_deployType)
                         {
                             case "热部署":
                                 await HotDeploy(cancellationToken);
                                 break;
                             case "冷部署":
                                 guardCts.Cancel(true);
                                 await ColdDeploy(cancellationToken);
                                 guardCts.Dispose();
                                 guardCts = new CancellationTokenSource();
                                 break;
                             default:
                                 return;
                         }
                     }
                     finally
                     {
                         slim.Release();
                     }
                 }
             });
        }

        static Task HotDeploy(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                foreach (var p in config.Processes)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                    Console.WriteLine($"进行热部署  >>  {p.Name}");
                    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
                    Console.WriteLine($"热部署完成  >>  {p.Name}");
                }
            }, cancellationToken);
        }

        static Task ColdDeploy(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                processes.Clear();
                foreach (var p in config.Processes)
                {
                    Console.WriteLine($"进行冷部署  >>  {p.Name}");
                    Console.WriteLine($"正在关闭 >> {p.Name}");
                    KillProcess(p.PID);
                    Console.WriteLine($"成功关闭  >>  {p.Name}");
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                    Console.WriteLine($"完成文件部署  >>  {p.Name}");
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                    Console.WriteLine($"正在重启进程 >> {p.Name}");
                    using (var newP = StartProcess(p.RootPath)) { p.PID = newP.Id; }
                    Console.WriteLine($"完成重启进程  >>  {p.Name}");
                    processes.Enqueue(p);
                    Console.WriteLine($"完成冷部署  >>  {p.Name}");
                }
            }, cancellationToken);
        }

        static Process StartProcess(string startPath)
        {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(startPath);
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.WorkingDirectory = startPath;
            process.EnableRaisingEvents = true;
            process.StartInfo.FileName = startPath;
            process.Start();
            return process;
        }

        static void KillProcess(int pid)
        {
            using (Process process = Process.GetProcessById(pid))
            {
                process.Kill();
                process.WaitForExit();
            }
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
