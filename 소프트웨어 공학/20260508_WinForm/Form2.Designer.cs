namespace _20260508_WinForm
{
    partial class Form2
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
            display = new Label();
            textBoxNum = new TextBox();
            btnStart = new Button();
            display2 = new Label();
            label1 = new Label();
            SuspendLayout();
            // 
            // display
            // 
            display.Font = new Font("맑은 고딕", 15F);
            display.Location = new Point(132, 21);
            display.Name = "display";
            display.Size = new Size(351, 36);
            display.TabIndex = 0;
            display.Text = "게임시작버튼을 누르세요";
            display.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // textBoxNum
            // 
            textBoxNum.BackColor = Color.White;
            textBoxNum.Enabled = false;
            textBoxNum.Font = new Font("맑은 고딕", 15F);
            textBoxNum.Location = new Point(151, 122);
            textBoxNum.Name = "textBoxNum";
            textBoxNum.Size = new Size(309, 34);
            textBoxNum.TabIndex = 1;
            textBoxNum.KeyDown += textBoxNum_KeyDown;
            // 
            // btnStart
            // 
            btnStart.BackColor = Color.FromArgb(255, 128, 128);
            btnStart.Dock = DockStyle.Bottom;
            btnStart.Font = new Font("맑은 고딕", 15F);
            btnStart.Location = new Point(0, 277);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(629, 34);
            btnStart.TabIndex = 2;
            btnStart.Text = "게임시작";
            btnStart.UseVisualStyleBackColor = false;
            btnStart.Click += btnStart_Click;
            // 
            // display2
            // 
            display2.Font = new Font("맑은 고딕", 15F);
            display2.Location = new Point(132, 73);
            display2.Name = "display2";
            display2.Size = new Size(351, 33);
            display2.TabIndex = 3;
            display2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            label1.Location = new Point(479, 179);
            label1.Name = "label1";
            label1.Size = new Size(100, 23);
            label1.TabIndex = 4;
            label1.Text = "label1";
            label1.Click += label1_Click;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(629, 311);
            Controls.Add(label1);
            Controls.Add(display2);
            Controls.Add(btnStart);
            Controls.Add(textBoxNum);
            Controls.Add(display);
            Name = "Form2";
            Text = "Form2";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label display;
        private TextBox textBoxNum;
        private Button btnStart;
        private Label display2;
        private Label label1;
    }
}