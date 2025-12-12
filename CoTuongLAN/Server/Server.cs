using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Server;
using System.IO;

namespace Server
{
    public partial class FormServer : Form
    {
        DatabaseHelper dbHelper;

        private int maxUsers;

        System.Collections.Generic.List<User> userList = new List<User>();
        private int maxTables;

        private GameTable[] gameTable;

        IPAddress localAddress;

        private int port = 51888;
        private TcpListener myListener;
        private Service service;
        private static readonly object _lockObject = new object();
        public FormServer()
        {
            InitializeComponent();
            service = new Service(listBox1);
        }

        private void FormServer_Load(object sender, EventArgs e)
        {
            listBox1.HorizontalScrollbar = true;
            localAddress = IPAddress.Any;
            buttonStop.Enabled = false;
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            dbHelper = new DatabaseHelper();
            if (int.TryParse(textBoxMaxTables.Text, out maxTables) == false
                || int.TryParse(textBoxMaxUsers.Text, out maxUsers) == false)
            {
                MessageBox.Show("Vui lòng nhập số nguyên dương hợp lệ!!!");
                return;
            }
            if (maxUsers < 1 || maxUsers > 300)
            {
                MessageBox.Show("Số lượng người cho phép là 1-300!!!");
                return;
            }
            if (maxTables < 1 || maxTables > 100)
            {
                MessageBox.Show("Số lượng bàn cho phép là 1-100!!!");
                return;
            }
            textBoxMaxTables.Enabled = false;
            textBoxMaxUsers.Enabled = false;
            gameTable = new GameTable[maxTables];
            for (int i = 0; i < maxTables; i++)
            {
                gameTable[i] = new GameTable(listBox1);
            }
            myListener = new TcpListener(localAddress, port);
            myListener.Start();
            service.AddItem(string.Format("Bắt đầu lắng nghe kết nối tại {0}:{1}", localAddress, port));
            Thread myThread = new Thread(new ThreadStart(ListenClientConnect));
            myThread.Start();
            buttonStart.Enabled = false;
            buttonStop.Enabled = true;
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            service.AddItem(string.Format("Số người dùng đang kết nối: {0}", userList.Count));
            service.AddItem(string.Format("Dừng dịch vụ, ngắt kết nối tất cả người dùng"));
            for (int i = 0; i < userList.Count; i++)
            {
                userList[i].client.Close();
            }
            myListener.Stop();
            buttonStart.Enabled = true;
            buttonStop.Enabled = false;
            textBoxMaxUsers.Enabled = true;
            textBoxMaxTables.Enabled = true;
        }
        private void ListenClientConnect()
        {
            while (true)
            {
                TcpClient newClient = null;
                try
                {
                    newClient = myListener.AcceptTcpClient();
                }
                catch
                {
                    break;
                }
                Thread threadReceive = new Thread(ReceiveData);
                User user = new User(newClient);
                threadReceive.Start(user);
                userList.Add(user);
                service.AddItem(string.Format("{0} đã kết nối", newClient.Client.RemoteEndPoint));
                service.AddItem(string.Format("Số người dùng hiện tại: {0}", userList.Count));
            }
        }
        private void ReceiveData(object obj)
        {
            User user = (User)obj;
            TcpClient client = user.client;
            bool normalExit = false;
            bool exitWhile = false;
            while (exitWhile == false)
            {
                string receiveString = null;
                try
                {
                    receiveString = user.sr.ReadLine();
                }
                catch
                {
                    service.AddItem("Nhận dữ liệu thất bại");
                }
                if (receiveString == null)
                {
                    if (normalExit == false)
                    {
                        if (client.Connected == true)
                        {
                            service.AddItem(string.Format("Mất kết nối với {0}, đã dừng nhận thông tin", client.Client.RemoteEndPoint));
                        }
                        RemoveClientfromPlayer(user);
                    }
                    break;
                }
                service.AddItem(string.Format("Từ {0}:{1}", user.userName, receiveString));
                string[] splitString = receiveString.Split(',');
                int tableIndex = -1;
                int side = -1;
                int anotherSide = -1;
                string sendString = "";
                string command = splitString[0].ToLower();
                switch (command)
                {
                    case "login":
                        if (splitString.Length < 3)
                        {
                            service.SendToOne(user, "LoginFailed,Lỗi giao thức");
                            exitWhile = true;
                            break;
                        }

                        string loginUser = splitString[1].Trim();
                        string loginPass = splitString[2].Trim();
                        bool isAlreadyOnline = false;
                        string userTagToCheck = string.Format("[{0}]", loginUser);

                        foreach (User u in userList)
                        {
                            if (u.userName == userTagToCheck)
                            {
                                isAlreadyOnline = true;
                                break;
                            }
                        }

                        if (isAlreadyOnline)
                        {
                            service.SendToOne(user, "LoginFailed,Tài khoản này đang online ở nơi khác!");
                            service.AddItem(string.Format("Từ chối đăng nhập {0} vì đã online.", loginUser));
                            exitWhile = true;
                            break;
                        }
                        if (userList.Count > maxUsers)
                        {
                            service.SendToOne(user, "LoginFailed,Phòng đã đầy");
                            service.AddItem("Phòng đầy, từ chối " + loginUser);
                            exitWhile = true;
                        }
                        else
                        {
                            bool isValid = dbHelper.ValidateUser(loginUser, loginPass);

                            if (isValid)
                            {
                                user.userName = string.Format("[{0}]", loginUser);

                                sendString = "Tables," + this.GetOnlineString();
                                service.SendToOne(user, sendString);
                                service.AddItem(string.Format("{0} đăng nhập thành công", loginUser));
                            }
                            else
                            {
                                service.SendToOne(user, "LoginFailed,Sai tài khoản hoặc mật khẩu");
                                service.AddItem(string.Format("{0} đăng nhập thất bại (sai mật khẩu)", loginUser));
                                exitWhile = true;
                            }
                        }
                        break;
                    case "getsecurityquestion":
                        string uAsk = splitString[1];
                        string question = dbHelper.GetSecurityQuestion(uAsk);
                        if (!string.IsNullOrEmpty(question))
                        {
                            service.SendToOne(user, "SecurityQuestion," + question);
                        }
                        else
                        {
                            service.SendToOne(user, "SecurityQuestionFail");
                        }
                        break;

                    case "verifyanswer":
                        string uVerify = splitString[1];
                        string ansVerify = splitString[2];
                        bool isCorrect = dbHelper.VerifySecurityAnswer(uVerify, ansVerify);

                        if (isCorrect)
                        {
                            service.SendToOne(user, "AnswerCorrect");
                        }
                        else
                        {
                            service.SendToOne(user, "AnswerIncorrect");
                        }
                        break;
                    case "checkuser":
                        string uCheck = splitString[1];
                        bool uExists = dbHelper.CheckUserExists(uCheck);

                        if (uExists)
                        {
                            service.SendToOne(user, "UserExist");
                        }
                        else
                        {
                            service.SendToOne(user, "UserNotExist");
                        }
                        break;
                    case "logout":
                        service.AddItem(string.Format("{0} thoát khỏi phòng game", user.userName));
                        normalExit = true;
                        exitWhile = true;
                        break;
                    case "sitdown":
                        tableIndex = int.Parse(splitString[1]);
                        side = int.Parse(splitString[2]);

                        lock (_lockObject)
                        {
                            if (gameTable[tableIndex].gamePlayer[side].someone == true)
                            {
                                service.SendToOne(user, "Tables," + this.GetOnlineString());
                                service.SendToOne(user, "Message,Ghế này vừa có người nhanh tay hơn ngồi rồi!");
                                break;
                            }
                            gameTable[tableIndex].gamePlayer[side].user = user;
                            gameTable[tableIndex].gamePlayer[side].someone = true;
                            service.AddItem(string.Format("{0} ngồi vào bàn {1}, ghế {2}", user.userName, tableIndex + 1, side));

                            anotherSide = (side + 1) % 2;
                            if (gameTable[tableIndex].gamePlayer[anotherSide].someone == true)
                            {
                                sendString = string.Format("SitDown,{0},{1}", anotherSide,
                                                gameTable[tableIndex].gamePlayer[anotherSide].user.userName);
                                service.SendToOne(user, sendString);
                            }

                            sendString = string.Format("SitDown,{0},{1}", side, user.userName);
                            service.SendToBoth(gameTable[tableIndex], sendString);
                            service.SendToAll(userList, "Tables," + this.GetOnlineString());
                        }
                        break;
                    case "getup":
                        tableIndex = int.Parse(splitString[1]);
                        side = int.Parse(splitString[2]);
                        service.AddItem(string.Format("{0} rời ghế quay lại phòng chờ", user.userName));
                        service.SendToBoth(gameTable[tableIndex], string.Format("GetUp,{0},{1}", side, user.userName));
                        gameTable[tableIndex].gamePlayer[side].someone = false;
                        gameTable[tableIndex].gamePlayer[side].started = false;
                        anotherSide = (side + 1) % 2;
                        if (gameTable[tableIndex].gamePlayer[anotherSide].someone == true)
                        {
                            gameTable[tableIndex].gamePlayer[anotherSide].started = false;
                        }
                        service.SendToAll(userList, "Tables," + this.GetOnlineString());
                        break;
                    case "talk":
                        tableIndex = int.Parse(splitString[1]);
                        sendString = string.Format("Talk,{0},{1}", user.userName,
                                    receiveString.Substring(splitString[0].Length + splitString[1].Length + 2));
                        service.SendToBoth(gameTable[tableIndex], sendString);
                        break;
                    case "start":
                        tableIndex = int.Parse(splitString[1]);
                        side = int.Parse(splitString[2]);
                        gameTable[tableIndex].gamePlayer[side].started = true;
                        if (side == 0)
                        {
                            anotherSide = 1;
                            sendString = "Message, Phe Đen đã sẵn sàng";
                        }
                        else
                        {
                            anotherSide = 0;
                            sendString = "Message, Phe Đỏ đã sẵn sàng";
                        }
                        service.SendToBoth(gameTable[tableIndex], sendString);
                        if (gameTable[tableIndex].gamePlayer[anotherSide].started == true)
                        {
                            sendString = "AllReady";
                            service.SendToBoth(gameTable[tableIndex], sendString);
                        }
                        break;
                    case "chessinfo":
                        tableIndex = int.Parse(splitString[1]);
                        side = int.Parse(splitString[2]);
                        anotherSide = (side + 1) % 2;
                        int cno;
                        int x0, y0;
                        int x1, y1;
                        cno = int.Parse(splitString[3]);
                        x0 = ChangeX(int.Parse(splitString[4]));
                        y0 = ChangeY(int.Parse(splitString[5]));
                        x1 = ChangeX(int.Parse(splitString[6]));
                        y1 = ChangeY(int.Parse(splitString[7]));
                        sendString = string.Format("ChessInfo,{0},{1},{2},{3},{4},{5}", side, int.Parse(splitString[3]),
                             int.Parse(splitString[4]), int.Parse(splitString[5]), int.Parse(splitString[6]), int.Parse(splitString[7]));
                        service.SendToOne(gameTable[tableIndex].gamePlayer[side].user, sendString);
                        service.AddItem(string.Format("{0}:{1}:Từ ({2},{3}) -> ({4},{5})", gameTable[tableIndex].gamePlayer[side].user.userName,
                            int.Parse(splitString[3]), int.Parse(splitString[4]), int.Parse(splitString[5]),
                            int.Parse(splitString[6]), int.Parse(splitString[7])));
                        sendString = string.Format("ChessInfo,{0},{1},{2},{3},{4},{5}", side, cno, x0, y0, x1, y1);
                        service.SendToOne(gameTable[tableIndex].gamePlayer[anotherSide].user, sendString);
                        service.AddItem(string.Format("{0}:{1}:Từ ({2},{3}) -> ({4},{5})", gameTable[tableIndex].gamePlayer[anotherSide].user.userName,
                            cno, x0, y0, x1, y1));
                        break;
                    case "win":
                        tableIndex = int.Parse(splitString[1]);
                        side = int.Parse(splitString[2]);
                        anotherSide = (side + 1) % 2;
                        sendString = string.Format("win,{0}", side);
                        service.SendToBoth(gameTable[tableIndex], sendString);
                        gameTable[tableIndex].gamePlayer[side].started = false;
                        gameTable[tableIndex].gamePlayer[anotherSide].started = false;
                        break;
                    case "resetpassword":
                        string rUser = splitString[1];
                        string rPass = splitString[2];

                        bool resetOk = dbHelper.UpdatePassword(rUser, rPass);

                        if (resetOk)
                        {
                            service.SendToOne(user, "ResetSuccess");
                            service.AddItem(string.Format("Người dùng {0} đã đổi mật khẩu thành công", rUser));
                        }
                        else
                        {
                            service.SendToOne(user, "ResetFail");
                        }
                        break;
                    case "register":
                        try
                        {
                            if (splitString.Length < 6)
                            {
                                service.SendToOne(user, "RegisterFail,Thiếu thông tin");
                                break;
                            }

                            string regUser = splitString[1].Trim();
                            string regPass = splitString[2].Trim();
                            string regName = splitString[3].Trim();
                            string regQuest = splitString[4].Trim();
                            string regAns = splitString[5].Trim();

                            service.AddItem(string.Format("Đang đăng ký người dùng: {0}", regUser));

                            bool isRegistered = dbHelper.RegisterUser(regUser, regPass, regName, regQuest, regAns);

                            if (isRegistered)
                            {
                                service.SendToOne(user, "RegisterSuccess");
                                service.AddItem(string.Format("Đăng ký thành công cho: {0}", regUser));
                            }
                            else
                            {
                                service.SendToOne(user, "RegisterFail,Tài khoản đã tồn tại hoặc lỗi DB");
                                service.AddItem(string.Format("Đăng ký thất bại cho: {0}", regUser));
                            }
                        }
                        catch (Exception ex)
                        {
                            service.SendToOne(user, "RegisterFail,Lỗi xử lý server");
                            service.AddItem("Lỗi Đăng ký: " + ex.Message);
                        }
                        break;
                }
            }
            userList.Remove(user);
            client.Close();
            service.AddItem(string.Format("Một người dùng đã thoát, số người dùng còn lại: {0}", userList.Count));
        }
        private int ChangeY(int x)
        {
            return x + 2 * (4 - x);
        }
        private int ChangeX(int y)
        {
            return y + 2 * (4 - y) + 1;
        }
        private void RemoveClientfromPlayer(User user)
        {
            for (int i = 0; i < gameTable.Length; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (gameTable[i].gamePlayer[j].user != null)
                    {
                        if (gameTable[i].gamePlayer[j].user == user)
                        {
                            StopPlayer(i, j);
                            return;
                        }
                    }
                }
            }
        }
        private void StopPlayer(int i, int j)
        {
            gameTable[i].gamePlayer[j].someone = false;
            gameTable[i].gamePlayer[j].started = false;

            int otherSide = (j + 1) % 2;

            if (gameTable[i].gamePlayer[otherSide].someone == true)
            {
                string msg = string.Format("GetUp,{0},{1}", j,
                    gameTable[i].gamePlayer[j].user != null ? gameTable[i].gamePlayer[j].user.userName : "Opponent");
                service.SendToOne(gameTable[i].gamePlayer[otherSide].user, msg);

                gameTable[i].gamePlayer[otherSide].started = false;
            }

            service.SendToAll(userList, "Tables," + this.GetOnlineString());
        }
        private string GetOnlineString()
        {
            string str = "";
            for (int i = 0; i < gameTable.Length; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    str += gameTable[i].gamePlayer[j].someone == true ? "1" : "0";
                }
            }
            return str;
        }

        private void FormServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (myListener != null)
            {
                buttonStop_Click(null, null);
            }
        }
    }
}