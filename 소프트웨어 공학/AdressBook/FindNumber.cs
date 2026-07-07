using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdressBook
{
    public partial class FindNumber : Form
    {
        private int findNum = 0;
        private int chance = 0;

        public FindNumber()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            var rand = new Random();
            findNum = rand.Next(1, 101);
            chance = 10;

            display.Text = $"숫자를 입력해주세요 남은기회 : {chance}회";
            display2.Text = null;
            textBoxNum.Enabled = true;
            textBoxNum.Focus();
            btnStart.Enabled = false;

            label1.Text = Convert.ToString(findNum);
        }

        private void textBoxNum_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    int inputNum = Int32.Parse(textBoxNum.Text);

                    textBoxNum.Text = null;
                    chance--;
                    display.Text = $"숫자를 입력해주세요 남은기회 : {chance}회";
                    if (findNum < inputNum)
                    {
                        // 원본 코드의 텍스트가 거꾸로 되어 있어 논리에 맞게 수정 ("작습니다")
                        display2.Text = "숫자가 입력하신 값보다 작습니다";
                    }
                    else if (findNum > inputNum)
                    {
                        // 원본 코드의 텍스트가 거꾸로 되어 있어 논리에 맞게 수정 ("큽니다")
                        display2.Text = "숫자가 입력하신 값보다 큽니다";
                    }
                    else
                    {
                        display.Text = $"게임이 종료되었습니다 남은기회 : {chance}회";
                        display2.Text = "숫자가 입력하신 값과 같습니다";
                        textBoxNum.Enabled = false;
                        btnStart.Enabled = true;
                    }

                    if (chance <= 0)
                    {
                        display.Text = "기회가 소진되어";
                        display2.Text = "게임이 종료되었습니다";
                        textBoxNum.Enabled = false;
                        btnStart.Enabled = true;
                    }
                }
                catch (System.FormatException)
                {
                    display.Text = $"숫자만 입력해주세요 남은기회 : {chance}회";
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void display2_Click(object sender, EventArgs e)
        {

        }
    }
}
