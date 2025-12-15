using System;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Client
{
    public partial class FormPreAuthentication : Form
    {
        public FormPreAuthentication()
        {
            InitializeComponent();
        }

        private void buttonConfirm_Click(object sender, EventArgs e)
        {
            string username = tb_username.Text.Trim();

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Vui lòng nhập tên tài khoản!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            TcpClient client = null;
            try
            {
                Debug.WriteLine(Program.ServerIP);
                client = new TcpClient(Program.ServerIP, 51888);
                NetworkStream ns = client.GetStream();
                StreamReader sr = new StreamReader(ns, Encoding.UTF8);
                StreamWriter sw = new StreamWriter(ns, Encoding.UTF8) { AutoFlush = true };

                string safeUser = username.Replace(",", "");
                sw.WriteLine("CheckUser," + safeUser);

                string response = sr.ReadLine();

                if (response == "UserExist")
                {
                    FormAuthentication f = new FormAuthentication(safeUser);

                    this.Hide();
                    f.ShowDialog();
                    this.Close();
                }
                else if (response == "UserNotExist")
                {
                    MessageBox.Show("Tài khoản không tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Lỗi Server: " + response);
                }
            }
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Lỗi kết nối: " + ex.Message);
            //}
            finally
            {
                if (client != null) client.Close();
            }
        }
    }
}