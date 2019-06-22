using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExNameChanger
{
    public class WriteHelper : TextWriter
    {
        public WriteHelper(TextBox control,Action<string> action)
        {
            Action = action;
            Control = control;
        }

        public Action<string> Action { get; set; }
        public TextBox Control { get; set; }
        public override Encoding Encoding => Encoding.UTF8;
        public override void WriteLine(string value)
        {
            if (Control.InvokeRequired)
            {
                Control.Invoke(Action,value);
            }
            else
            {
                Action(value);
            }
            base.WriteLine(value);
        }
        public override async Task WriteAsync(string value)
        {
            await Task.Run(() =>
            {
                WriteLine(value);
            });
            await base.WriteAsync(value);
        }
    }
}
