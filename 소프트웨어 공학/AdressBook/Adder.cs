using Google.Protobuf.WellKnownTypes;
using Org.BouncyCastle.Asn1.Cmp;
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
    public partial class Adder : Form
    {

        double currentNum = 0;              //입력받은 수를 저장
        double resultNum = 0;               //현재까지 계산된 결과
        string operation = "";              //눌린 연산자 저장, + - ...
        bool isOperationClicked = false;    // 연산자가 선택되어 있는 상태인지...
        string expressText = "";            // 디스플레이 내용
        string resultText = "";
        double beforeResult = 0;          // 계산 직전의 결과 임시저장
        string state = "시작";
        bool hasDot = false;      // 현재 입력 중인 숫자에 소수점이 있는지
        int decimalPlace = 1;     // 소수 자릿수
        public Adder()
        {
            InitializeComponent();
            updateDisplay();
        }

        private void updateDisplay()
        {
            txtAnswer.Text = expressText + Environment.NewLine + resultText + Environment.NewLine + "currentNum : " + currentNum + Environment.NewLine + "resultNum : " + resultNum + Environment.NewLine + "beforeResult : " + beforeResult + Environment.NewLine + state;
            state = "출력중";
        }

        private void Number_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            state = "수입력";

            if (!hasDot)
            {
                if (currentNum == 0)
                    currentNum = Convert.ToDouble(btn.Text);

                else
                {
                    currentNum = Convert.ToDouble(currentNum.ToString() + btn.Text);
                    resultNum = beforeResult;
                }
            }
            else
            {
                currentNum += Convert.ToDouble(btn.Text) / Math.Pow(10, decimalPlace);
                decimalPlace++;
                resultNum = beforeResult;
            }

            expressText += btn.Text;

            if (isOperationClicked)
            {
                state = "수입력 후 calculate 호츌";
                Calculate(currentNum.ToString());
            }
            updateDisplay();
        }

        private void Calculate(String currentStr)
        {
            state = "calculate 진입";
            beforeResult = resultNum;
            double num = Convert.ToDouble(currentStr);
            switch (operation)
            {
                case "+": resultNum += num; break;
                case "-": resultNum -= num; break;
                case "×": resultNum *= num; break;
                case "÷":
                    if (num == 0)
                    {
                        MessageBox.Show("0으로 나눌수 없어~");
                        return;
                    }
                    resultNum /= num;
                    break;
            }
            resultText = resultNum.ToString();

        }
        private void Operator_Click(object sender, EventArgs e)
        {
            state = "연산자 클릭";
            Button btn = (Button)sender;
            operation = btn.Text;
            if (resultNum == 0 && currentNum != 0)
            {
                resultNum = currentNum;

            }
            currentNum = 0;
            hasDot = false;
            decimalPlace = 1;
            isOperationClicked = true;
            expressText += btn.Text;
            resultText = "";
            updateDisplay();
        }

        private void Equal_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            expressText += btn.Text;
            currentNum = 0;
            operation = "";
            isOperationClicked = false;
            updateDisplay();
            updateDisplay();
            expressText = resultText;
        }

        private void btnAC_Click(object sender, EventArgs e)
        {
            txtAnswer.Text = "0";


            currentNum = 0;
            resultNum = 0;
            operation = "";
            expressText = "";
            resultText = "";
            isOperationClicked = false;
            beforeResult = 0;
            hasDot = false;
            decimalPlace = 1;
            state = "시작";
            updateDisplay();


        }

        private void btnPM_Click(object sender, EventArgs e)
        {
            resultNum = beforeResult;

            if (currentNum != 0)
            {
                currentNum *= -1;
                Calculate(currentNum.ToString());
            }

            string currentNumStr = currentNum.ToString();
            if (currentNum < 0)
            {
                expressText = expressText.Substring(0, expressText.Length - currentNumStr.Length);
                expressText += currentNum.ToString();
            }
            else
            {
                expressText = expressText.Substring(0, expressText.Length - currentNumStr.Length - 1);
                expressText += "+" + currentNum.ToString();
            }

            updateDisplay();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            state = "back 클릭";
            if (expressText.Length == 0) return;

            char del = expressText[expressText.Length - 1];
            expressText = expressText.Substring(0, expressText.Length - 1);

            if (char.IsDigit(del))
            {
                resultNum = beforeResult;
                string currentStr = currentNum.ToString();

                if (hasDot)
                {
                    decimalPlace--;

                    // 소수점 첫 자리까지 지워진 경우
                    if (decimalPlace < 1)
                    {
                        hasDot = false;
                        decimalPlace = 1;
                    }
                }

                if (currentStr.Length > 1)
                {
                    currentStr = currentStr.Substring(0, currentStr.Length - 1);
                    currentNum = Convert.ToDouble(currentStr);

                    if (currentStr.EndsWith("."))
                    {
                        currentStr = currentStr.Substring(0, currentStr.Length - 1);
                        hasDot = false;
                        decimalPlace = 1;
                    }
                }
                else
                {
                    currentNum = 0;
                    resultText = resultNum.ToString();
                    hasDot = false;
                    decimalPlace = 1;
                }

                //if (isOperationClicked)
                //{
                //    Calculate(currentNum.ToString());
                //}
            }
            else if ("+-×÷".Contains(del))
            {
                isOperationClicked = false;
                resultText = "";
            }
            updateDisplay();

        }

        private void btnDot_Click(object sender, EventArgs e)
        {
            state = "소수점 입력";

            if (hasDot)
                return;

            hasDot = true;

            if (currentNum == 0)
                expressText += "0.";
            else
                expressText += ".";

            updateDisplay();
        }
    }
}
