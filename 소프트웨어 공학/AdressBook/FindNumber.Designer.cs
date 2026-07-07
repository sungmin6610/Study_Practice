namespace AdressBook
{
    partial class FindNumber
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.btnStart = new System.Windows.Forms.Button();
            this.display = new System.Windows.Forms.Label();
            this.display2 = new System.Windows.Forms.Label();
            this.textBoxNum = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnStart.Location = new System.Drawing.Point(135, 30);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(120, 40);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "게임 시작";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // display
            // 
            this.display.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.display.Location = new System.Drawing.Point(12, 100);
            this.display.Name = "display";
            this.display.Size = new System.Drawing.Size(360, 30);
            this.display.TabIndex = 1;
            this.display.Text = "게임을 시작하려면 버튼을 누르세요";
            this.display.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // display2
            // 
            this.display2.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.display2.Location = new System.Drawing.Point(12, 150);
            this.display2.Name = "display2";
            this.display2.Size = new System.Drawing.Size(360, 30);
            this.display2.TabIndex = 2;
            this.display2.Text = "";
            this.display2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.display2.Click += new System.EventHandler(this.display2_Click);
            // 
            // textBoxNum
            // 
            this.textBoxNum.Enabled = false;
            this.textBoxNum.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.textBoxNum.Location = new System.Drawing.Point(135, 210);
            this.textBoxNum.Name = "textBoxNum";
            this.textBoxNum.Size = new System.Drawing.Size(120, 33);
            this.textBoxNum.TabIndex = 3;
            this.textBoxNum.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBoxNum.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxNum_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(340, 280);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "target";
            this.label1.Visible = false;
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // FindNumber
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 311);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxNum);
            this.Controls.Add(this.display2);
            this.Controls.Add(this.display);
            this.Controls.Add(this.btnStart);
            this.Name = "FindNumber";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "숫자 맞추기 게임";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Label display;
        private System.Windows.Forms.Label display2;
        private System.Windows.Forms.TextBox textBoxNum;
        private System.Windows.Forms.Label label1;
    }
}