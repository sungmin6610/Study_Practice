namespace _20260508_WinForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnIn_Click(object sender, EventArgs e)
        {

        }

        private void lblOut_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                lblOut.Text = textBoxID.Text;
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void logIn_Click(object sender, EventArgs e)
        {
            lblOut.Text = $"ID : {textBoxID.Text}\n PW: {textBoxPW.Text}";
        }
    }
}
