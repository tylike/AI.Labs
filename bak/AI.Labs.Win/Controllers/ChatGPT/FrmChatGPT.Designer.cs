namespace Browser
{
    partial class FrmChatGPT
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            txtURL = new TextBox();
            btnGO = new Button();
            panel1 = new Panel();
            textBox1 = new TextBox();
            btnSend = new Button();
            btnWatch = new Button();
            ((System.ComponentModel.ISupportInitialize)webView).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // webView
            // 
            webView.AllowExternalDrop = true;
            webView.CreationProperties = null;
            webView.DefaultBackgroundColor = Color.White;
            webView.Dock = DockStyle.Fill;
            webView.Location = new Point(0, 57);
            webView.Name = "webView";
            webView.Size = new Size(2152, 1075);
            webView.TabIndex = 0;
            webView.ZoomFactor = 1D;
            // 
            // txtURL
            // 
            txtURL.Location = new Point(15, 12);
            txtURL.Name = "txtURL";
            txtURL.Size = new Size(592, 30);
            txtURL.TabIndex = 1;
            // 
            // btnGO
            // 
            btnGO.Location = new Point(641, 10);
            btnGO.Name = "btnGO";
            btnGO.Size = new Size(112, 34);
            btnGO.TabIndex = 2;
            btnGO.Text = "GO";
            btnGO.UseVisualStyleBackColor = true;
            btnGO.Click += btnGO_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(textBox1);
            panel1.Controls.Add(txtURL);
            panel1.Controls.Add(btnWatch);
            panel1.Controls.Add(btnSend);
            panel1.Controls.Add(btnGO);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(2152, 57);
            panel1.TabIndex = 3;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(771, 10);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(643, 30);
            textBox1.TabIndex = 3;
            // 
            // btnSend
            // 
            btnSend.Location = new Point(1432, 8);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(112, 34);
            btnSend.TabIndex = 2;
            btnSend.Text = "Send";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // btnWatch
            // 
            btnWatch.Location = new Point(2028, 10);
            btnWatch.Name = "btnWatch";
            btnWatch.Size = new Size(112, 34);
            btnWatch.TabIndex = 2;
            btnWatch.Text = "监控";
            btnWatch.UseVisualStyleBackColor = true;
            btnWatch.Click += btnWatch_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2152, 1132);
            Controls.Add(webView);
            Controls.Add(panel1);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)webView).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webView;
        private TextBox txtURL;
        private Button btnGO;
        private Panel panel1;
        private TextBox textBox1;
        private Button btnSend;
        private Button btnWatch;
    }
}
