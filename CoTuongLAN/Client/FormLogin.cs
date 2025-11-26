using System;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Client
{
    public partial class FormLogin : Form
    {
        private TcpClient client;
        private StreamReader sr;
        private StreamWriter sw;

        public FormLogin()
        {
            InitializeComponent();
        }

        private void linklb_register_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormRegister f = new FormRegister();
            f.ShowDialog();
        }

        // Nếu bạn có FormAuthentication riêng, giữ nguyên logic này
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // FormAuthentication f = new FormAuthentication();
            // f.Show();
        }

        private void btn_Login_Click(object sender, EventArgs e)
        {
            string username = tb_username.Text.Trim();
            string password = tb_pword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                client = new TcpClient("127.0.0.1", 51888);
                NetworkStream ns = client.GetStream();
                sr = new StreamReader(ns, Encoding.UTF8);
                sw = new StreamWriter(ns, Encoding.UTF8) { AutoFlush = true };

                // Xử lý chuỗi (Sanitize input)
                // QUAN TRỌNG: Loại bỏ dấu phẩy để tránh lỗi giao thức
                string safeUser = username.Replace(",", "");
                string safePass = password.Replace(",", "");

                string loginCommand = string.Format("Login,{0},{1}", safeUser, safePass);
                sw.WriteLine(loginCommand);

                string response = sr.ReadLine();

                if (response == null)
                {
                    MessageBox.Show("Không nhận được phản hồi từ Server.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    client.Close();
                    return;
                }

                string[] splitResponse = response.Split(',');
                string command = splitResponse[0];

                if (command == "Tables")
                {
                    MessageBox.Show("Đăng nhập thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    FormRoom room = new FormRoom(client, safeUser, response);
                    room.Show();

                    this.Hide();
                }
                else if (command == "LoginFailed")
                {
                    string reason = splitResponse.Length > 1 ? splitResponse[1] : "Sai tài khoản hoặc mật khẩu";
                    MessageBox.Show("Đăng nhập thất bại: " + reason, "Lỗi đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    client.Close(); // Đóng kết nối để người dùng thử lại
                }
                else
                {
                    MessageBox.Show("Phản hồi không xác định từ Server: " + response);
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối Server: " + ex.Message, "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (client != null) client.Close();
            }
        }
    }
}