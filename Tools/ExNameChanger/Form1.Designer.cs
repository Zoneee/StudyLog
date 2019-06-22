using System.Threading.Tasks;

namespace ExNameChanger
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.LogTBox = new System.Windows.Forms.TextBox();
            this.SelectBtn = new System.Windows.Forms.Button();
            this.ChangeCBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // LogTBox
            // 
            this.LogTBox.Enabled = false;
            this.LogTBox.Location = new System.Drawing.Point(12, 61);
            this.LogTBox.Multiline = true;
            this.LogTBox.Name = "LogTBox";
            this.LogTBox.Size = new System.Drawing.Size(282, 311);
            this.LogTBox.TabIndex = 3;
            // 
            // SelectBtn
            // 
            this.SelectBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.SelectBtn.Location = new System.Drawing.Point(12, 12);
            this.SelectBtn.Name = "SelectBtn";
            this.SelectBtn.Size = new System.Drawing.Size(159, 23);
            this.SelectBtn.TabIndex = 4;
            this.SelectBtn.Text = "选择目录";
            this.SelectBtn.UseVisualStyleBackColor = true;
            this.SelectBtn.Click += new System.EventHandler(this.SelectBtn_Click);
            // 
            // ChangeCBox
            // 
            this.ChangeCBox.FormattingEnabled = true;
            this.ChangeCBox.Location = new System.Drawing.Point(177, 15);
            this.ChangeCBox.Name = "ChangeCBox";
            this.ChangeCBox.Size = new System.Drawing.Size(121, 20);
            this.ChangeCBox.TabIndex = 5;
            this.ChangeCBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ChangeCBox_KeyDown);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(310, 403);
            this.Controls.Add(this.ChangeCBox);
            this.Controls.Add(this.SelectBtn);
            this.Controls.Add(this.LogTBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "修改文件后缀名";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox LogTBox;
        private System.Windows.Forms.Button SelectBtn;
        private System.Windows.Forms.ComboBox ChangeCBox;
    }
}

