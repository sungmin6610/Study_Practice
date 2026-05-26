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
    public partial class Calculator : Form
    {
        private double operand1 = 0;
        private string activeOperation = "";
        private bool isOperationPerformed = false;

        public Calculator()
        {
            InitializeComponent();
            InitializeCalculatorEvents();
        }

        private void InitializeCalculatorEvents()
        {
            
            num0.Click += NumButton_Click;
            num1.Click += NumButton_Click;
            num3.Click += NumButton_Click;
            num4.Click += NumButton_Click;
            num5.Click += NumButton_Click;
            num6.Click += NumButton_Click;
            num7.Click += NumButton_Click;
            num8.Click += NumButton_Click;
            num9.Click += NumButton_Click;

            
            plus.Click += OpButton_Click;
            minus.Click += OpButton_Click;
            x.Click += OpButton_Click;
            divide.Click += OpButton_Click;

            
            clear.Click += ClearButton_Click;
            result.Click += ResultButton_Click;
            dot.Click += DotButton_Click;
        }

        
        private void NumButton_Click(object sender, EventArgs e)
        {
            if (label1.Text == "0" || isOperationPerformed)
            {
                label1.Text = "";
            }

            isOperationPerformed = false;
            Button button = (Button)sender;
            label1.Text += button.Text;
        }

        
        private void DotButton_Click(object sender, EventArgs e)
        {
            if (isOperationPerformed)
            {
                label1.Text = "0";
                isOperationPerformed = false;
            }

            if (!label1.Text.Contains("."))
            {
                label1.Text += ".";
            }
        }

        
        private void OpButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            try
            {
                operand1 = double.Parse(label1.Text);
                activeOperation = button.Text;
                isOperationPerformed = true;
            }
            catch (Exception)
            {
                label1.Text = "Error";
            }
        }

        
        private void ClearButton_Click(object sender, EventArgs e)
        {
            label1.Text = "0";
            operand1 = 0;
            activeOperation = "";
            isOperationPerformed = false;
        }

        
        private void ResultButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(activeOperation))
                return;

            try
            {
                double operand2 = double.Parse(label1.Text);
                double resultValue = 0;

                switch (activeOperation)
                {
                    case "+":
                        resultValue = operand1 + operand2;
                        break;
                    case "-":
                        resultValue = operand1 - operand2;
                        break;
                    case "x":
                        resultValue = operand1 * operand2;
                        break;
                    case "÷":
                        if (operand2 != 0)
                        {
                            resultValue = operand1 / operand2;
                        }
                        else
                        {
                            label1.Text = "Error (0으로 나눔)";
                            activeOperation = "";
                            isOperationPerformed = true;
                            return;
                        }
                        break;
                    default:
                        return;
                }

                label1.Text = resultValue.ToString();
                operand1 = resultValue; 
                activeOperation = "";
                isOperationPerformed = true;
            }
            catch (Exception)
            {
                label1.Text = "Error";
            }
        }

        
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (label1.Text.Length > 0 && label1.Text != "Error" && !label1.Text.StartsWith("Error"))
            {
                label1.Text = label1.Text.Substring(0, label1.Text.Length - 1);
                if (label1.Text.Length == 0)
                {
                    label1.Text = "0";
                }
            }
            else
            {
                label1.Text = "0";
            }
        }

        
        private void button4_Click(object sender, EventArgs e)
        {
            NumButton_Click(sender, e);
        }

        
        private void button15_Click(object sender, EventArgs e)
        {
            DeleteButton_Click(sender, e);
        }
    }
}