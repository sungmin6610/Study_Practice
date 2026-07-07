using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _26_06_24_winform상속과다형성
{
    public partial class Form1: Form
    {
        class CustomForm:Form
        {
            public CustomForm()
            {
                Text = "커스텀 폼";
            }
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("내용");
            MessageBox.Show("내용", "제목");

            DialogResult result;
            do
            {
                result = MessageBox.Show("내용", "제목", MessageBoxButtons.RetryCancel);
            } while (result == DialogResult.Retry);
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CustomForm form = new CustomForm();
            form.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CustomForm form = new CustomForm();
        }
    }
}
