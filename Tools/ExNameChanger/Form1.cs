using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExNameChanger
{
    public partial class Form1 : Form
    {
        List<string> items = new List<string>();
        string changes = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Changes.txt");
        public Form1()
        {
            InitializeComponent();
            Console.SetOut(new WriteHelper(LogTBox, s => LogTBox.Text += $"{ DateTime.Now.ToString("hh:mm:ss")}>> {s}{Environment.NewLine}"));
            if (File.Exists(changes))
            {
                items = File.ReadAllLines(changes).ToList();
                ChangeCBox.DataSource = items;
            }
        }

        private async void SelectBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ChangeCBox.Text))
            {
                MessageBox.Show("修改扩展名不可为空！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ChangeCBox.Focus();
                return;
            }
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Title = "请选择文件";
            dialog.Filter = "所有文件(*.*)|*.*";

            var dialogResult = dialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                Name = "执行中...";
                SelectBtn.Enabled = false;
                ChangeCBox.Enabled = false;
                List<Task> tasks = new List<Task>();
                foreach (var item in dialog.FileNames)
                {
                    var f = Path.GetFileName(Path.ChangeExtension(item, ChangeCBox.Text));
                    var d = Path.Combine(Path.GetDirectoryName(item), "Changes");
                    if (!Directory.Exists(d))
                    {
                        Directory.CreateDirectory(d);
                    }
                    d = Path.Combine(d, f);
                    tasks.Add(Task.Run(() =>
                    {
                        File.Copy(item, d, true);
                        Console.WriteLine($"已完成：{d}");
                    }));
                }
                await Task.WhenAll(tasks).ConfigureAwait(false);
                ControlHelper.SetControl(SelectBtn, () => SelectBtn.Enabled = true);
                ControlHelper.SetControl(ChangeCBox, () => ChangeCBox.Enabled = true);
            }
        }

        private void ChangeCBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //保存文件
                if (items.Contains(ChangeCBox.Text))
                {
                    Console.WriteLine($"已存在：\"{ChangeCBox.Text}\"选项");
                    return;
                }
                items.Add(ChangeCBox.Text);
                File.WriteAllLines(changes, items, Encoding.UTF8);
                //var items = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, changes));
                ChangeCBox.DataSource = items;
                Console.WriteLine($"已新增：\"{ChangeCBox.Text}\"选项");
            }
        }
    }
}
