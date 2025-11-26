using System;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Client
{
    public partial class FormRegister : Form
    {
        private TcpClient client;
        private StreamReader sr;
        private StreamWriter sw;

        public FormRegister()
        {
            InitializeComponent();

            cbb_securityq.Items.Clear();
            cbb_securityq.Items.Add("Tên trường tiểu học đầu tiên của bạn?");
            cbb_securityq.Items.Add("Tên thú cưng đầu tiên của bạn?");
            cbb_securityq.Items.Add("Người bạn thân nhất thời thơ ấu tên là gì?");
            cbb_securityq.Items.Add("Thành phố nơi cha mẹ bạn gặp nhau?");
            cbb_securityq.Items.Add("Món ăn yêu thích nhất của bạn?");
            cbb_securityq.Items.Add("Tên chiếc xe đầu tiên bạn sở hữu?");
            cbb_securityq.Items.Add("Biệt danh ở nhà của bạn là gì?");

            if (cbb_securityq.Items.Count > 0)
                cbb_securityq.SelectedIndex = 0;
        }

        private void btn_register_Click(object sender, EventArgs e)
        {
            string fullname = textBox1.Text.Trim();
            string username = tb_username.Text.Trim();
            string password = tb_pword.Text.Trim();
            string securityQuestion = cbb_securityq.SelectedItem != null ? cbb_securityq.SelectedItem.ToString() : cbb_securityq.Text.Trim();
            string securityAnswer = tb_securitya.Text.Trim();
            string confirmAnswer = textBox2.Text.Trim();

            if (string.IsNullOrEmpty(fullname) || string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(securityAnswer))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!securityAnswer.Equals(confirmAnswer, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Câu trả lời bảo mật xác nhận không khớp!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                client = new TcpClient("127.0.0.1", 51888);
                NetworkStream ns = client.GetStream();
                sr = new StreamReader(ns, Encoding.UTF8);
                sw = new StreamWriter(ns, Encoding.UTF8) { AutoFlush = true };

                string safeName = fullname.Replace(",", ".");
                string safeUser = username.Replace(",", "");
                string safePass = password.Replace(",", "");
                string safeQuestion = securityQuestion.Replace(",", ".");
                string safeAnswer = securityAnswer.Replace(",", ".");

                string registerCommand = string.Format("Register,{0},{1},{2},{3},{4}",
                    safeUser, safePass, safeName, safeQuestion, safeAnswer);

                sw.WriteLine(registerCommand);

                string response = sr.ReadLine();

                if (response != null)
                {
                    string[] splitRes = response.Split(',');
                    if (splitRes[0] == "RegisterSuccess")
                    {
                        MessageBox.Show("Đăng ký thành công! Bạn có thể đăng nhập ngay bây giờ.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                    else if (splitRes[0] == "RegisterFail")
                    {
                        string reason = splitRes.Length > 1 ? splitRes[1] : "Lỗi không xác định";
                        MessageBox.Show("Đăng ký thất bại: " + reason, "Lỗi Máy Chủ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối tới máy chủ: " + ex.Message, "Lỗi Kết Nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (client != null) client.Close();
            }
        }
    }
}