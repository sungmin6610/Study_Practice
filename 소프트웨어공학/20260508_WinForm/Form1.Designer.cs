namespace _20260508_WinForm
{
    partial class Form1
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
            Label label1;
            Label label2;
            lblOut = new Label();
            textBoxID = new TextBox();
            textBoxPW = new TextBox();
            logIn = new Button();
            label1 = new Label();
            label2 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Font = new Font("맑은 고딕", 15F);
            label1.Location = new Point(159, 33);
            label1.Name = "label1";
            label1.Size = new Size(96, 34);
            label1.TabIndex = 4;
            label1.Text = "아이디";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            label2.Font = new Font("맑은 고딕", 15F);
            label2.Location = new Point(159, 89);
            label2.Name = "label2";
            label2.Size = new Size(96, 31);
            label2.TabIndex = 5;
            label2.Text = "패스워드";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            label2.Click += label2_Click;
            // 
            // lblOut
            // 
            lblOut.BackColor = Color.FromArgb(255, 192, 128);
            lblOut.Font = new Font("맑은 고딕", 15F);
            lblOut.ImageAlign = ContentAlignment.TopLeft;
            lblOut.Location = new Point(159, 160);
            lblOut.Name = "lblOut";
            lblOut.Size = new Size(513, 233);
            lblOut.TabIndex = 1;
            lblOut.TextAlign = ContentAlignment.MiddleCenter;
            lblOut.Click += lblOut_Click;
            // 
            // textBoxID
            // 
            textBoxID.Font = new Font("맑은 고딕", 15F);
            textBoxID.Location = new Point(261, 33);
            textBoxID.Name = "textBoxID";
            textBoxID.Size = new Size(313, 34);
            textBoxID.TabIndex = 2;
            textBoxID.KeyDown += textBox1_KeyDown;
            // 
            // textBoxPW
            // 
            textBoxPW.Font = new Font("맑은 고딕", 15F);
            textBoxPW.Location = new Point(261, 86);
            textBoxPW.Name = "textBoxPW";
            textBoxPW.Size = new Size(313, 34);
            textBoxPW.TabIndex = 3;
            textBoxPW.UseSystemPasswordChar = true;
            // 
            // logIn
            // 
            logIn.Font = new Font("맑은 고딕", 15F);
            logIn.Location = new Point(591, 33);
            logIn.Name = "logIn";
            logIn.Size = new Size(81, 87);
            logIn.TabIndex = 6;
            logIn.Text = "로그인";
            logIn.UseVisualStyleBackColor = true;
            logIn.Click += logIn_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(logIn);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(textBoxPW);
            Controls.Add(textBoxID);
            Controls.Add(lblOut);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label lblOut;
        private TextBox textBoxID;
        private TextBox textBoxPW;
        private Button logIn;
    }
}
