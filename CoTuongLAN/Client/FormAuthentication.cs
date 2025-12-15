using System;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Client
{
    public partial class FormAuthentication : Form
    {
        private string targetUser = "";

        public FormAuthentication()
        {
            InitializeComponent();
        }
        public FormAuthentication(string username)
        {
            InitializeComponent();
            this.targetUser = username;
        }

        private void FormAuthentication_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(targetUser))
            {
                MessageBox.Show("Lỗi: Không nhận được thông tin tài khoản.", "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            GetSecurityQuestion();
        }

        private void GetSecurityQuestion()
        {
            TcpClient client = null;
            try
            {
                client = new TcpClient(Program.ServerIP, 51888);
                NetworkStream ns = client.GetStream();
                StreamReader sr = new StreamReader(ns, Encoding.UTF8);
                StreamWriter sw = new StreamWriter(ns, Encoding.UTF8) { AutoFlush = true };

                sw.WriteLine("GetSecurityQuestion," + targetUser);

                string response = sr.ReadLine();

                if (response != null && response.StartsWith("SecurityQuestion,"))
                {
                    string prefix = "SecurityQuestion,";
                    string question = response.Substring(prefix.Length);
                    tb_securityq.Text = question;
                    tb_securityq.ReadOnly = true;
                }
                else
                {
                    MessageBox.Show("Không tìm thấy câu hỏi bảo mật cho user: " + targetUser, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối khi lấy câu hỏi: " + ex.Message);
                this.Close();
            }
            finally
            {
                if (client != null) client.Close();
            }
        }

        private void buttonConfirm_Click(object sender, EventArgs e)
        {
            string answer = tb_securitya.Text.Trim();
            if (string.IsNullOrEmpty(answer))
            {
                MessageBox.Show("Vui lòng nhập câu trả lời!");
                return;
            }

            TcpClient client = null;
            try
            {
                client = new TcpClient(Program.ServerIP, 51888);
                NetworkStream ns = client.GetStream();
                StreamReader sr = new StreamReader(ns, Encoding.UTF8);
                StreamWriter sw = new StreamWriter(ns, Encoding.UTF8) { AutoFlush = true };

                string safeAnswer = answer.Replace(",", ".");

                sw.WriteLine(string.Format("VerifyAnswer,{0},{1}", targetUser, safeAnswer));

                string response = sr.ReadLine();

                if (response == "AnswerCorrect")
                {
                    MessageBox.Show("Xác thực thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    FormReset f = new FormReset(targetUser);
                    this.Hide();
                    f.ShowDialog();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Câu trả lời sai!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message);
            }
            finally
            {
                if (client != null) client.Close();
            }
        }
    }
}