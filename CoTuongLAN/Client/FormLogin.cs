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
            //Lấy IP người dùng đang nhập ở ô textbox Login
            string ipInput = tb_ServerIP.Text.Trim();

            //Kiểm tra xem người dùng đã nhập IP chưa
            if (string.IsNullOrEmpty(ipInput))
            {
                MessageBox.Show("Vui lòng nhập IP Server trước khi đăng ký!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Dừng lại, không mở Form đăng ký
            }

            //Lưu IP vào biến toàn cục
            Program.ServerIP = ipInput;
            FormRegister f = new FormRegister();
            f.ShowDialog();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormPreAuthentication f = new FormPreAuthentication();
            f.Show();
        }

        private void btn_Login_Click(object sender, EventArgs e)
        {
            string username = tb_username.Text.Trim();
            string password = tb_pword.Text.Trim();
            string ipInput = tb_ServerIP.Text.Trim();

            //Kiểm tra rỗng
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(ipInput))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin (Tài khoản, Mật khẩu, IP Server)!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //Kiểm tra định dạng IP
            System.Net.IPAddress ipAddress;
            if (!System.Net.IPAddress.TryParse(ipInput, out ipAddress))
            {
                MessageBox.Show("Địa chỉ IP Server không hợp lệ! Vui lòng kiểm tra lại.", "Lỗi IP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Program.ServerIP = ipInput;

            try
            {
                client = new TcpClient(Program.ServerIP, 51888);
                NetworkStream ns = client.GetStream();
                sr = new StreamReader(ns, Encoding.UTF8);
                sw = new StreamWriter(ns, Encoding.UTF8) { AutoFlush = true };

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
                    client.Close();
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