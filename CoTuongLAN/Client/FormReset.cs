using System;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Client
{
    public partial class FormReset : Form
    {
        private string targetUser; // Username cần reset mật khẩu

        public FormReset()
        {
            InitializeComponent();
        }

        public FormReset(string username)
        {
            InitializeComponent();
            this.targetUser = username;
        }

        private void buttonConfirm_Click(object sender, EventArgs e)
        {
            string newPass = tb_NewPassword.Text.Trim();
            string confirmPass = tb_Confirmpassword.Text.Trim();

            // 1. Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(newPass) || string.IsNullOrEmpty(confirmPass))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPass != confirmPass)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 2. Kết nối Server để cập nhật
            try
            {
                TcpClient client = new TcpClient("127.0.0.1", 51888);
                NetworkStream ns = client.GetStream();
                StreamReader sr = new StreamReader(ns, Encoding.UTF8);
                StreamWriter sw = new StreamWriter(ns, Encoding.UTF8) { AutoFlush = true };

                // Xử lý chuỗi an toàn (loại bỏ dấu phẩy)
                string safePass = newPass.Replace(",", "");

                // Gửi lệnh: ResetPassword,username,newpassword
                string cmd = string.Format("ResetPassword,{0},{1}", targetUser, safePass);
                sw.WriteLine(cmd);

                // 3. Nhận phản hồi
                string response = sr.ReadLine();
                client.Close();

                if (response == "ResetSuccess")
                {
                    MessageBox.Show("Đổi mật khẩu thành công! Hãy đăng nhập lại.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close(); // Đóng Form Reset -> Quay về màn hình Login hoặc đóng app
                }
                else
                {
                    MessageBox.Show("Đổi mật khẩu thất bại. Lỗi hệ thống.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối Server: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}