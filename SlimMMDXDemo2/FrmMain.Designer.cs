namespace SlimMMDXDemo2
{
    partial class FrmMain
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlDraw = new System.Windows.Forms.Panel();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnCapture = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // pnlDraw
            // 
            this.pnlDraw.Location = new System.Drawing.Point(12, 12);
            this.pnlDraw.Name = "pnlDraw";
            this.pnlDraw.Size = new System.Drawing.Size(501, 420);
            this.pnlDraw.TabIndex = 0;
            // 
            // btnPlay
            // 
            this.btnPlay.Location = new System.Drawing.Point(12, 438);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(87, 48);
            this.btnPlay.TabIndex = 1;
            this.btnPlay.Text = "再生";
            this.btnPlay.UseVisualStyleBackColor = true;
            // 
            // btnCapture
            // 
            this.btnCapture.Location = new System.Drawing.Point(105, 438);
            this.btnCapture.Name = "btnCapture";
            this.btnCapture.Size = new System.Drawing.Size(87, 48);
            this.btnCapture.TabIndex = 2;
            this.btnCapture.Text = "キャプチャ";
            this.btnCapture.UseVisualStyleBackColor = true;
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(525, 496);
            this.Controls.Add(this.btnCapture);
            this.Controls.Add(this.btnPlay);
            this.Controls.Add(this.pnlDraw);
            this.Name = "FrmMain";
            this.Text = "SlimMMDX Demo2 コントロール描画とキャプチャ";
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Panel pnlDraw;
        public System.Windows.Forms.Button btnPlay;
        public System.Windows.Forms.Button btnCapture;
    }
}

