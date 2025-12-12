using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class FormRoom : Form
    {
        private int maxPlayingTables;
        private CheckBox[,] checkBoxGameTables;
        private TcpClient client = null;
        private StreamWriter sw;
        private StreamReader sr;
        private Service service;
        private FormPlaying formPlaying;

        private string myUserName;
        private string _initialTableData;

        private bool normalExit = false;
        private bool isReceiveCommand = false;

        private int side = -1;
        private int tableIndex = -1;

        public FormRoom(TcpClient client, string userName, string strTables)
        {
            InitializeComponent();
            this.client = client;
            this.myUserName = userName;
            this._initialTableData = strTables;

            NetworkStream netStream = client.GetStream();
            sr = new StreamReader(netStream, System.Text.Encoding.UTF8);
            sw = new StreamWriter(netStream, System.Text.Encoding.UTF8) { AutoFlush = true };

            service = new Service(listBox1, sw);
        }

        private void FormRoom_Load(object sender, EventArgs e)
        {
            textBoxName.Text = this.myUserName;
            textBoxName.ReadOnly = true;

            if (buttonReady != null)
            {
                buttonReady.Visible = true;
                buttonReady.Enabled = false;
                buttonReady.Text = "Sẵn sàng";
            }

            maxPlayingTables = 0;

            if (client != null && client.Connected)
            {
                textBoxLocal.Text = client.Client.LocalEndPoint.ToString();
                textBoxServer.Text = client.Client.RemoteEndPoint.ToString();

                if (!string.IsNullOrEmpty(_initialTableData))
                {
                    string[] parts = _initialTableData.Split(',');
                    if (parts.Length > 1 && parts[0] == "Tables")
                    {
                        ProcessTableData(parts[1]);
                    }
                }

                Thread threadReceive = new Thread(new ThreadStart(ReceiveData));
                threadReceive.IsBackground = true;
                threadReceive.Start();
            }
        }

        private void buttonReady_Click(object sender, EventArgs e)
        {
            if (side != -1 && tableIndex != -1)
            {
                service.SendToServer(string.Format("Start,{0},{1}", tableIndex, side));

                buttonReady.Enabled = false;
                buttonReady.Text = "Đang chờ đối thủ...";
                service.AddItemToListBox("Đã sẵn sàng, vui lòng chờ đối thủ...");
            }
        }

        private void ReceiveData()
        {
            bool exitWhile = false;
            while (exitWhile == false)
            {
                string receiveString = null;
                try
                {
                    receiveString = sr.ReadLine();
                }
                catch
                {
                    service.AddItemToListBox("Nhận dữ liệu thất bại");
                }

                if (receiveString == null)
                {
                    if (normalExit == false)
                    {
                        MessageBox.Show("Mất kết nối với máy chủ!");
                    }
                    if (side != -1) ExitFormPlaying();
                    side = -1;
                    normalExit = true;
                    break;
                }

                string[] splitString = receiveString.Split(',');
                string command = splitString[0].ToLower();

                switch (command)
                {
                    case "sorry":
                        MessageBox.Show("Phòng đã đầy!");
                        exitWhile = true;
                        break;

                    case "tables":
                        string s = splitString[1];
                        ProcessTableData(s);
                        break;

                    case "sitdown":
                        if (formPlaying != null)
                        {
                            formPlaying.SetTableSideText(splitString[1], splitString[2],
                                            string.Format("{0} đã vào bàn", splitString[2]));
                        }
                        else
                        {
                            service.AddItemToListBox(string.Format("Bàn {0}: {1} đã ngồi vào ghế {2}",
                                int.Parse(splitString[1]) + 1, splitString[2], splitString[1]));
                        }
                        break;

                    case "getup":
                        int getupSide = int.Parse(splitString[1]);
                        if (side == getupSide)
                        {
                            side = -1;
                            tableIndex = -1;
                            ExitFormPlaying();
                            formPlaying = null;

                            Invoke(new Action(() => {
                                buttonReady.Enabled = false;
                                buttonReady.Text = "Sẵn sàng";
                            }));
                        }
                        else
                        {
                            if (formPlaying != null)
                            {
                                Invoke(new Action(() => formPlaying.Restart("Đối thủ đã thoát, bạn thắng!")));
                            }
                        }
                        break;

                    case "allready":
                        Invoke(new Action(() => {
                            if (formPlaying == null || formPlaying.IsDisposed)
                            {
                                formPlaying = new FormPlaying(tableIndex, side, sw);
                                formPlaying.Text = string.Format("Bàn {0} - Phe {1} - {2}",
                                    tableIndex + 1, side == 0 ? "Đen" : "Đỏ", myUserName);
                                formPlaying.Show();
                            }
                            formPlaying.ShowMessage("Hai bên đã sẵn sàng, Ván đấu bắt đầu!");
                            formPlaying.Ready(side);
                        }));
                        break;

                    case "talk":
                        if (formPlaying != null)
                            formPlaying.ShowTalk(splitString[1], receiveString.Substring(splitString[0].Length + splitString[1].Length + 2));
                        break;

                    case "message":
                        string msg = splitString[1];
                        service.AddItemToListBox(msg);
                        if (formPlaying != null) formPlaying.ShowMessage(msg);
                        break;

                    case "chessinfo":
                        if (formPlaying != null)
                        {
                            int tside = int.Parse(splitString[1]);
                            int cno = int.Parse(splitString[2]);
                            int oriX = int.Parse(splitString[3]);
                            int oriY = int.Parse(splitString[4]);
                            int endX = int.Parse(splitString[5]);
                            int endY = int.Parse(splitString[6]);

                            if (formPlaying.InvokeRequired)
                            {
                                formPlaying.Invoke(new Action(() =>
                                {
                                    formPlaying.ChangeChess(-1, oriX, oriY);
                                    formPlaying.ChangeChess(cno, endX, endY);
                                    formPlaying.RePaint();
                                    formPlaying.drawFrame("blue", endX, endY);
                                    formPlaying.ChangeOrder(tside);
                                    formPlaying.CheckWin();
                                }));
                            }
                        }
                        break;

                    case "win":
                        if (formPlaying != null)
                        {
                            int winner = int.Parse(splitString[1]);
                            formPlaying.ShowForm(winner);
                        }
                        break;
                }
            }
            Application.Exit();
        }

        private void ProcessTableData(string s)
        {
            if (maxPlayingTables == 0)
            {
                maxPlayingTables = s.Length / 2;
                checkBoxGameTables = new CheckBox[maxPlayingTables, 2];
                isReceiveCommand = true;
                for (int i = 0; i < maxPlayingTables; i++)
                {
                    AddCheckBoxToPanel(s, i);
                }
                isReceiveCommand = false;
            }
            else
            {
                isReceiveCommand = true;
                for (int i = 0; i < maxPlayingTables; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        if (s[2 * i + j] == '0')
                            UpdateCheckBox(checkBoxGameTables[i, j], false);
                        else
                            UpdateCheckBox(checkBoxGameTables[i, j], true);
                    }
                }
                isReceiveCommand = false;
            }
        }

        delegate void ExitFormPlayingDelegate();
        private void ExitFormPlaying()
        {
            if (formPlaying != null)
            {
                if (formPlaying.InvokeRequired)
                {
                    Invoke(new Action(ExitFormPlaying));
                }
                else
                {
                    formPlaying.Close();
                }
            }
        }

        delegate void Paneldelegate(string s, int i);
        private void AddCheckBoxToPanel(string s, int i)
        {
            if (panel1.InvokeRequired)
            {
                this.Invoke(new Paneldelegate(AddCheckBoxToPanel), s, i);
            }
            else
            {
                Label label = new Label();
                label.Location = new Point(10, 15 + i * 30);
                label.Text = string.Format("Bàn {0}: ", i + 1);
                label.Width = 70;
                this.panel1.Controls.Add(label);

                CreateCheckBox(i, 1, s, "Đỏ");
                CreateCheckBox(i, 0, s, "Đen");
            }
        }

        delegate void CheckBoxDelegate(CheckBox checkbox, bool isChecked);
        private void UpdateCheckBox(CheckBox checkbox, bool isChecked)
        {
            if (checkbox.InvokeRequired)
            {
                this.Invoke(new CheckBoxDelegate(UpdateCheckBox), checkbox, isChecked);
            }
            else
            {
                if (side == -1)
                {
                    checkbox.Enabled = !isChecked;
                }
                else
                {
                    checkbox.Enabled = false;
                }

                checkbox.Checked = isChecked;

                if (isChecked) checkbox.BackColor = Color.Transparent;
                else checkbox.BackColor = Color.Transparent;
            }
        }

        private void CreateCheckBox(int i, int j, string s, string text)
        {
            int x = j == 0 ? 100 : 200;
            checkBoxGameTables[i, j] = new CheckBox();
            checkBoxGameTables[i, j].Name = string.Format("check{0:0000}{1:0000}", i, j);
            checkBoxGameTables[i, j].Width = 60;
            checkBoxGameTables[i, j].Location = new Point(x, 10 + i * 30);
            checkBoxGameTables[i, j].Text = text;
            checkBoxGameTables[i, j].TextAlign = ContentAlignment.MiddleLeft;

            if (s[2 * i + j] == '1')
            {
                checkBoxGameTables[i, j].Enabled = false;
                checkBoxGameTables[i, j].Checked = true;
            }
            else
            {
                checkBoxGameTables[i, j].Enabled = true;
                checkBoxGameTables[i, j].Checked = false;
            }

            this.panel1.Controls.Add(checkBoxGameTables[i, j]);
            checkBoxGameTables[i, j].CheckedChanged += new EventHandler(checkBox_CheckedChanged);
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (isReceiveCommand == true) return;

            CheckBox checkbox = (CheckBox)sender;
            if (checkbox.Checked == true)
            {
                int i = int.Parse(checkbox.Name.Substring(5, 4));
                int j = int.Parse(checkbox.Name.Substring(9, 4));

                side = j;
                tableIndex = i;

                service.SendToServer(string.Format("SitDown,{0},{1}", i, j));

                buttonReady.Enabled = true;
                buttonReady.Text = "Bắt đầu";
                service.AddItemToListBox(string.Format("Bạn đã chọn bàn {0}, phe {1}. Hãy bấm Sẵn sàng!", i + 1, j == 0 ? "Đen" : "Đỏ"));
            }
        }
    }
}