namespace WebSocketApp
{
    partial class Win10Form
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
            this.LinkBtn = new System.Windows.Forms.Button();
            this.WsAddressTBox = new System.Windows.Forms.TextBox();
            this.UnLinkBtn = new System.Windows.Forms.Button();
            this.SendBtn = new System.Windows.Forms.Button();
            this.ReveiceBtn = new System.Windows.Forms.Button();
            this.LogTBox = new System.Windows.Forms.TextBox();
            this.MessageTBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // LinkBtn
            // 
            this.LinkBtn.Location = new System.Drawing.Point(12, 72);
            this.LinkBtn.Name = "LinkBtn";
            this.LinkBtn.Size = new System.Drawing.Size(304, 23);
            this.LinkBtn.TabIndex = 0;
            this.LinkBtn.Text = "建立链接";
            this.LinkBtn.UseVisualStyleBackColor = true;
            this.LinkBtn.Click += new System.EventHandler(this.LinkBtn_Click);
            // 
            // WsAddressTBox
            // 
            this.WsAddressTBox.Location = new System.Drawing.Point(12, 45);
            this.WsAddressTBox.Name = "WsAddressTBox";
            this.WsAddressTBox.Size = new System.Drawing.Size(304, 21);
            this.WsAddressTBox.TabIndex = 1;
            // 
            // UnLinkBtn
            // 
            this.UnLinkBtn.Location = new System.Drawing.Point(12, 101);
            this.UnLinkBtn.Name = "UnLinkBtn";
            this.UnLinkBtn.Size = new System.Drawing.Size(304, 23);
            this.UnLinkBtn.TabIndex = 2;
            this.UnLinkBtn.Text = "断开链接";
            this.UnLinkBtn.UseVisualStyleBackColor = true;
            this.UnLinkBtn.Click += new System.EventHandler(this.UnLinkBtn_Click);
            // 
            // SendBtn
            // 
            this.SendBtn.Location = new System.Drawing.Point(12, 386);
            this.SendBtn.Name = "SendBtn";
            this.SendBtn.Size = new System.Drawing.Size(304, 23);
            this.SendBtn.TabIndex = 3;
            this.SendBtn.Text = "发送消息";
            this.SendBtn.UseVisualStyleBackColor = true;
            this.SendBtn.Click += new System.EventHandler(this.SendBtn_Click);
            // 
            // ReveiceBtn
            // 
            this.ReveiceBtn.Location = new System.Drawing.Point(12, 415);
            this.ReveiceBtn.Name = "ReveiceBtn";
            this.ReveiceBtn.Size = new System.Drawing.Size(304, 23);
            this.ReveiceBtn.TabIndex = 4;
            this.ReveiceBtn.Text = "接收消息";
            this.ReveiceBtn.UseVisualStyleBackColor = true;
            this.ReveiceBtn.Click += new System.EventHandler(this.ReveiceBtn_Click);
            // 
            // LogTBox
            // 
            this.LogTBox.Location = new System.Drawing.Point(322, 45);
            this.LogTBox.Multiline = true;
            this.LogTBox.Name = "LogTBox";
            this.LogTBox.Size = new System.Drawing.Size(274, 393);
            this.LogTBox.TabIndex = 5;
            // 
            // MessageTBox
            // 
            this.MessageTBox.Location = new System.Drawing.Point(12, 189);
            this.MessageTBox.Multiline = true;
            this.MessageTBox.Name = "MessageTBox";
            this.MessageTBox.Size = new System.Drawing.Size(304, 191);
            this.MessageTBox.TabIndex = 6;
            // 
            // Win10Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(615, 450);
            this.Controls.Add(this.MessageTBox);
            this.Controls.Add(this.LogTBox);
            this.Controls.Add(this.ReveiceBtn);
            this.Controls.Add(this.SendBtn);
            this.Controls.Add(this.UnLinkBtn);
            this.Controls.Add(this.WsAddressTBox);
            this.Controls.Add(this.LinkBtn);
            this.Name = "Win10Form";
            this.Text = "Win10Form";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button LinkBtn;
        private System.Windows.Forms.TextBox WsAddressTBox;
        private System.Windows.Forms.Button UnLinkBtn;
        private System.Windows.Forms.Button SendBtn;
        private System.Windows.Forms.Button ReveiceBtn;
        private System.Windows.Forms.TextBox LogTBox;
        private System.Windows.Forms.TextBox MessageTBox;
    }
}

