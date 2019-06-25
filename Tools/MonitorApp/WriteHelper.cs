using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExNameChanger
{
    public class WriteHelper : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
        public override void WriteLine(string value)
        {
            string _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", DateTime.Now.ToString("yyyy-MM-dd"), "log.log");
            if (!Directory.Exists(Path.GetDirectoryName(_path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path));
            }
            using (FileStream stream = new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                var buffer = Encoding.GetBytes($"{value}{Environment.NewLine}");
                stream.Write(buffer, 0, buffer.Length);
            }
            base.WriteLine(value);
        }
        public override async Task WriteAsync(string value)
        {
            string _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", DateTime.Now.ToString("yyyy-MM-dd"), "log.log");
            if (!Directory.Exists(Path.GetDirectoryName(_path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path));
            }
            using (FileStream stream = new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                var buffer = Encoding.GetBytes($"{value}{Environment.NewLine}");
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
            await base.WriteAsync(value);
        }


    }
}
