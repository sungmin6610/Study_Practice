using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;


namespace AdressBook
{
    public partial class Form1 : Form
    {
        int selected = -1;
        string connStr = "Server=localhost;Database=library;Uid=root;Pwd=root;";
        public Form1()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData() {
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string sql = "select * from contacts";
                    MySqlDataAdapter result = new MySqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    result.Fill(dt);
                    dataView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataView.DataSource = dt;
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "" || textBox2.Text == "")
            {
                MessageBox.Show("이름과 연락처를 입력해야합니다.");
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = @"INSERT INTO contacts (name, phone, email, address) VALUES (@name, @phone, @email, @address)";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", textBox1.Text);
                cmd.Parameters.AddWithValue("@phone", textBox2.Text);
                cmd.Parameters.AddWithValue("@email", textBox3.Text);
                cmd.Parameters.AddWithValue("@address", textBox4.Text);
                cmd.ExecuteNonQuery();
                MessageBox.Show("추가 완료");
                LoadData();
                Clear();
            }
        }

        private void Clear()
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
        }
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "" || textBox2.Text == "")
            {
                MessageBox.Show("이름과 연락처를 입력해야합니다.");
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = @"update contacts set name=@name, phone=@phone, email=@email, address=@address where id = @id";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", textBox1.Text);
                cmd.Parameters.AddWithValue("@phone", textBox2.Text);
                cmd.Parameters.AddWithValue("@email", textBox3.Text);
                cmd.Parameters.AddWithValue("@address", textBox4.Text);
                cmd.Parameters.AddWithValue("@id", selected);
                cmd.ExecuteNonQuery();
                MessageBox.Show("수정 완료");
                LoadData();
                Clear();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selected == -1)
            {
                MessageBox.Show("삭제할 행을 선택해주세요.");
                return;

            }
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = @"delete from contacts where id = @id";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", textBox1.Text);
                cmd.Parameters.AddWithValue("@phone", textBox2.Text);
                cmd.Parameters.AddWithValue("@email", textBox3.Text);
                cmd.Parameters.AddWithValue("@address", textBox4.Text);
                cmd.Parameters.AddWithValue("@id", selected);
                cmd.ExecuteNonQuery();
                MessageBox.Show("삭제 완료");
                LoadData();
                Clear();
            }
        }

        private void dataView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void dataView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            {
                if (!dataView.Rows[e.RowIndex].IsNewRow)
                {
                    DataGridViewRow row = dataView.Rows[e.RowIndex];
                    selected = Convert.ToInt32(row.Cells["id"].Value);
                    textBox1.Text = row.Cells["name"].Value.ToString();
                    textBox2.Text = row.Cells["Phone"].Value.ToString();
                    textBox3.Text = row.Cells["email"].Value.ToString();
                    textBox4.Text = row.Cells["address"].Value.ToString();
                }
            }
                        
        }

        private void 정보추가ToolStripMenuItem_Click(object sender, EventArgs e)
        {
           btnInsert_Click(sender, e);
        }

        private void 정보수정ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnUpdate_Click(sender, e);
        }

        private void 정보삭제ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnDelete_Click(sender, e);
        }

        private void 계산기ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Calculator cal = new Calculator();
            cal.ShowDialog(); //모달 창

        }

        private void 숫자맞추기ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FindNumber fn = new FindNumber();
            fn.Show();
        }
    }
}
