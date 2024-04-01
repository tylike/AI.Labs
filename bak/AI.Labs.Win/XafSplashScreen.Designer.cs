namespace AI.Labs.Win
{
    partial class XafSplashScreen
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XafSplashScreen));
            progressBarControl = new DevExpress.XtraEditors.MarqueeProgressBarControl();
            labelCopyright = new DevExpress.XtraEditors.LabelControl();
            labelStatus = new DevExpress.XtraEditors.LabelControl();
            peImage = new DevExpress.XtraEditors.PictureEdit();
            peLogo = new DevExpress.XtraEditors.PictureEdit();
            pcApplicationName = new DevExpress.XtraEditors.PanelControl();
            labelSubtitle = new DevExpress.XtraEditors.LabelControl();
            labelApplicationName = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)progressBarControl.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)peImage.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)peLogo.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pcApplicationName).BeginInit();
            pcApplicationName.SuspendLayout();
            SuspendLayout();
            // 
            // progressBarControl
            // 
            progressBarControl.EditValue = 0;
            progressBarControl.Location = new Point(123, 459);
            progressBarControl.Margin = new Padding(5);
            progressBarControl.Name = "progressBarControl";
            progressBarControl.Properties.Appearance.BorderColor = Color.FromArgb(195, 194, 194);
            progressBarControl.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            progressBarControl.Properties.EndColor = Color.FromArgb(255, 114, 0);
            progressBarControl.Properties.LookAndFeel.SkinName = "Visual Studio 2013 Blue";
            progressBarControl.Properties.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.UltraFlat;
            progressBarControl.Properties.LookAndFeel.UseDefaultLookAndFeel = false;
            progressBarControl.Properties.ProgressViewStyle = DevExpress.XtraEditors.Controls.ProgressViewStyle.Solid;
            progressBarControl.Properties.StartColor = Color.FromArgb(255, 144, 0);
            progressBarControl.Size = new Size(583, 27);
            progressBarControl.TabIndex = 5;
            // 
            // labelCopyright
            // 
            labelCopyright.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            labelCopyright.Location = new Point(40, 548);
            labelCopyright.Margin = new Padding(5);
            labelCopyright.Name = "labelCopyright";
            labelCopyright.Size = new Size(76, 22);
            labelCopyright.TabIndex = 6;
            labelCopyright.Text = "Copyright";
            // 
            // labelStatus
            // 
            labelStatus.Location = new Point(125, 428);
            labelStatus.Margin = new Padding(5);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(76, 22);
            labelStatus.TabIndex = 7;
            labelStatus.Text = "Starting...";
            // 
            // peImage
            // 
            peImage.EditValue = resources.GetObject("peImage.EditValue");
            peImage.Location = new Point(20, 20);
            peImage.Margin = new Padding(5);
            peImage.Name = "peImage";
            peImage.Properties.AllowFocused = false;
            peImage.Properties.Appearance.BackColor = Color.Transparent;
            peImage.Properties.Appearance.Options.UseBackColor = true;
            peImage.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            peImage.Properties.ShowMenu = false;
            peImage.Size = new Size(710, 305);
            peImage.TabIndex = 9;
            peImage.Visible = false;
            // 
            // peLogo
            // 
            peLogo.Location = new Point(667, 555);
            peLogo.Margin = new Padding(5);
            peLogo.Name = "peLogo";
            peLogo.Properties.AllowFocused = false;
            peLogo.Properties.Appearance.BackColor = Color.Transparent;
            peLogo.Properties.Appearance.Options.UseBackColor = true;
            peLogo.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            peLogo.Properties.ShowMenu = false;
            peLogo.Size = new Size(117, 34);
            peLogo.TabIndex = 8;
            // 
            // pcApplicationName
            // 
            pcApplicationName.Appearance.BackColor = Color.FromArgb(255, 114, 0);
            pcApplicationName.Appearance.Options.UseBackColor = true;
            pcApplicationName.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pcApplicationName.Controls.Add(labelSubtitle);
            pcApplicationName.Controls.Add(labelApplicationName);
            pcApplicationName.Dock = DockStyle.Top;
            pcApplicationName.Location = new Point(2, 2);
            pcApplicationName.LookAndFeel.UseDefaultLookAndFeel = false;
            pcApplicationName.Margin = new Padding(5);
            pcApplicationName.Name = "pcApplicationName";
            pcApplicationName.Size = new Size(795, 394);
            pcApplicationName.TabIndex = 10;
            // 
            // labelSubtitle
            // 
            labelSubtitle.Appearance.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            labelSubtitle.Appearance.ForeColor = Color.FromArgb(255, 216, 188);
            labelSubtitle.Appearance.Options.UseFont = true;
            labelSubtitle.Appearance.Options.UseForeColor = true;
            labelSubtitle.Location = new Point(488, 283);
            labelSubtitle.Margin = new Padding(5);
            labelSubtitle.Name = "labelSubtitle";
            labelSubtitle.Size = new Size(240, 40);
            labelSubtitle.TabIndex = 1;
            labelSubtitle.Text = "调包大侠荣誉出品";
            // 
            // labelApplicationName
            // 
            labelApplicationName.Appearance.Font = new Font("Segoe UI", 26.25F, FontStyle.Regular, GraphicsUnit.Point);
            labelApplicationName.Appearance.ForeColor = SystemColors.Window;
            labelApplicationName.Appearance.Options.UseFont = true;
            labelApplicationName.Appearance.Options.UseForeColor = true;
            labelApplicationName.Location = new Point(123, 92);
            labelApplicationName.Margin = new Padding(5);
            labelApplicationName.Name = "labelApplicationName";
            labelApplicationName.Size = new Size(605, 71);
            labelApplicationName.TabIndex = 0;
            labelApplicationName.Text = "云梯人工智能融合实验室";
            // 
            // XafSplashScreen
            // 
            AutoScaleDimensions = new SizeF(10F, 22F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(799, 651);
            Controls.Add(peImage);
            Controls.Add(peLogo);
            Controls.Add(labelStatus);
            Controls.Add(labelCopyright);
            Controls.Add(progressBarControl);
            Controls.Add(pcApplicationName);
            Margin = new Padding(5);
            Name = "XafSplashScreen";
            Padding = new Padding(2);
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)progressBarControl.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)peImage.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)peLogo.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pcApplicationName).EndInit();
            pcApplicationName.ResumeLayout(false);
            pcApplicationName.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DevExpress.XtraEditors.MarqueeProgressBarControl progressBarControl;
        private DevExpress.XtraEditors.LabelControl labelCopyright;
        private DevExpress.XtraEditors.LabelControl labelStatus;
        private DevExpress.XtraEditors.PictureEdit peLogo;
        private DevExpress.XtraEditors.PictureEdit peImage;
        private DevExpress.XtraEditors.PanelControl pcApplicationName;
        private DevExpress.XtraEditors.LabelControl labelSubtitle;
        private DevExpress.XtraEditors.LabelControl labelApplicationName;
    }
}
