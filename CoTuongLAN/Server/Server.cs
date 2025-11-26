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

namespace Server
{
    public partial class Server : Form
    {
        DatabaseHelper dbHelper = new DatabaseHelper();

        private int maxUsers;

        System.Collections.Generic.List<User> userList = new List<User>();
        private int maxTables;

        private GameTable[] gameTable;

        IPAddress localAddress;

        private int port = 51888;
        private TcpListener myListener;
        private Service service;
        public Server()
        {
            InitializeComponent();
            service = new Service(listBox1);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listBox1.HorizontalScrollbar = true;
            localAddress = IPAddress.Parse("127.0.0.1");
            buttonStop.Enabled = false;
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBoxMaxTables.Text, out maxTables) == false
                || int.TryParse(textBoxMaxUsers.Text, out maxUsers) == false)
            {
                MessageBox.Show("Please enter a positive integer in the range!!!");
                return;
            }
            if (maxUsers < 1 || maxUsers > 300)
            {
                MessageBox.Show("The number of people allowed is 1-300!!!");
                return;
            }
            if (maxTables < 1 || maxTables > 100)
            {
                MessageBox.Show("The number of tables allowed is 1-100!!!");
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
            service.AddItem(string.Format("Start listening for client connections at {0}:{1}", localAddress, port));
            Thread myThread = new Thread(new ThreadStart(ListenClientConnect));
            myThread.Start();
            buttonStart.Enabled = false;
            buttonStop.Enabled = true;
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            service.AddItem(string.Format("Number of currently connected users:{0}", userList.Count));
            service.AddItem(string.Format("Stop the service immediately, the user will exit in sequence"));
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
                // create a thread for each client
                Thread threadReceive = new Thread(ReceiveData);
                User user = new User(newClient);
                threadReceive.Start(user);
                userList.Add(user);
                service.AddItem(string.Format("{0}Enter", newClient.Client.RemoteEndPoint));
                service.AddItem(string.Format("Number of currently connected users: {0}", userList.Count));
            }
        }
        private void ReceiveData(object obj)
        {
            User user = (User)obj;
            TcpClient client = user.client;
            //Whether to exit the receiving thread normally
            bool normalExit = false;
            //Used to control whether to exit the loop
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
                    service.AddItem("Failed to receive data");
                }
                if (receiveString == null)
                {
                    if (normalExit == false)
                    {
                        if (client.Connected == true)
                        {
                            service.AddItem(string.Format("lost contact with {0}, has terminated receiving the user information", client.Client.RemoteEndPoint));
                        }
                        RemoveClientfromPlayer(user);
                    }
                    break;
                }
                service.AddItem(string.Format("from {0}:{1}", user.userName, receiveString));
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

                        string loginUser = splitString[1];
                        string loginPass = splitString[2];

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
                                service.AddItem(string.Format("{0} đăng nhập thất bại (sai pass)", loginUser));
                                exitWhile = true;
                            }
                        }
                        break;
                    //Exit, format: Logout
                    case "logout":
                        service.AddItem(string.Format("{0} exit the game room", user.userName));
                        normalExit = true;
                        exitWhile = true;
                        break;
                    //Sit down, format: SitDown, table number, seat number
                    case "sitdown":
                        tableIndex = int.Parse(splitString[1]);
                        side = int.Parse(splitString[2]);
                        gameTable[tableIndex].gamePlayer[side].user = user;
                        gameTable[tableIndex].gamePlayer[side].someone = true;
                        service.AddItem(string.Format("{0} ngồi vào bàn {1}, ghế {2}", user.userName, tableIndex + 1, side));

                        //Get the seat number of the other party
                        anotherSide = (side + 1) % 2;
                        // Determine if the other party is someone
                        if (gameTable[tableIndex].gamePlayer[anotherSide].someone == true)
                        {
                            // Tell the user that the other party is seated
                            //Format: SitDown, seat number, username
                            sendString = string.Format("SitDown,{0},{1}", anotherSide,
                                            gameTable[tableIndex].gamePlayer[anotherSide].user.userName);
                            service.SendToOne(user, sendString);
                        }
                        //Tell both users that the user is seated
                        //Format: SitDown, seat number, username
                        sendString = string.Format("SitDown,{0},{1}", side, user.userName);
                        service.SendToBoth(gameTable[tableIndex], sendString);
                        //Send the status of each table in the game room to all users
                        service.SendToAll(userList, "Tables," + this.GetOnlineString());
                        break;
                    //Leave seat, format: GetUp, table number, seat number
                    case "getup":
                        tableIndex = int.Parse(splitString[1]);
                        side = int.Parse(splitString[2]);
                        service.AddItem(string.Format("{0} leave seat and return to the game room", user.userName));
                        //Send the departure information to two users in the format: GetUp, seat number, user name
                        service.SendToBoth(gameTable[tableIndex], string.Format("GetUp,{0},{1}", side, user.userName));
                        gameTable[tableIndex].gamePlayer[side].someone = false;
                        gameTable[tableIndex].gamePlayer[side].started = false;
                        anotherSide = (side + 1) % 2;
                        if (gameTable[tableIndex].gamePlayer[anotherSide].someone == true)
                        {
                            gameTable[tableIndex].gamePlayer[anotherSide].started = false;
                        }
                        //Send the status of each table in the game room to all users
                        service.SendToAll(userList, "Tables," + this.GetOnlineString());
                        break;
                    //chat, format: Talk, username, conversation content
                    case "talk":
                        tableIndex = int.Parse(splitString[1]);
                        //special handling of commas
                        sendString = string.Format("Talk,{0},{1}", user.userName,
                                    receiveString.Substring(splitString[0].Length + splitString[1].Length + 2));
                        service.SendToBoth(gameTable[tableIndex], sendString);
                        break;
                    //Prepare, format: Start, table number, seat number
                    case "start":
                        tableIndex = int.Parse(splitString[1]);
                        side = int.Parse(splitString[2]);
                        gameTable[tableIndex].gamePlayer[side].started = true;
                        if (side == 0)
                        {
                            anotherSide = 1;
                            sendString = "Message, Black is ready";
                        }
                        else
                        {
                            anotherSide = 0;
                            sendString = "Message, the red team is ready";
                        }
                        service.SendToBoth(gameTable[tableIndex], sendString);
                        if (gameTable[tableIndex].gamePlayer[anotherSide].started == true)
                        {
                            sendString = "AllReady";
                            service.SendToBoth(gameTable[tableIndex], sendString);
                        }
                        break;
                    //chess piece movement information, format: ChessInfo, table number, seat number, piece number, original x, original y, purpose x, purpose y
                    case "chessinfo":
                        tableIndex = int.Parse(splitString[1]);
                        side = int.Parse(splitString[2]);
                        anotherSide = (side + 1) % 2;
                        int cno;//Pawn number
                        int x0, y0;//Original coordinates
                        int x1, y1;//Destination coordinates
                        cno = int.Parse(splitString[3]);
                        x0 = ChangeX(int.Parse(splitString[4]));
                        y0 = ChangeY(int.Parse(splitString[5]));
                        x1 = ChangeX(int.Parse(splitString[6]));
                        y1 = ChangeY(int.Parse(splitString[7]));
                        sendString = string.Format("ChessInfo,{0},{1},{2},{3},{4},{5}", side, int.Parse(splitString[3]),
                             int.Parse(splitString[4]), int.Parse(splitString[5]), int.Parse(splitString[6]), int.Parse(splitString[7]));
                        service.SendToOne(gameTable[tableIndex].gamePlayer[side].user, sendString);
                        service.AddItem(string.Format("{0}:{1}:From ({2},{3}) -> ({4},{5})", gameTable[tableIndex].gamePlayer[side].user.userName,
                            int.Parse(splitString[3]), int.Parse(splitString[4]), int.Parse(splitString[5]),
                            int.Parse(splitString[6]), int.Parse(splitString[7])));
                        sendString = string.Format("ChessInfo,{0},{1},{2},{3},{4},{5}", side, cno, x0, y0, x1, y1);
                        service.SendToOne(gameTable[tableIndex].gamePlayer[anotherSide].user, sendString);
                        service.AddItem(string.Format("{0}:{1}:From ({2},{3}) -> ({4},{5})", gameTable[tableIndex].gamePlayer[anotherSide].user.userName,
                            cno, x0, y0, x1, y1));
                        break;
                    //Victory, format: Win, table number, seat number
                    case "win":
                        tableIndex = int.Parse(splitString[1]);
                        side = int.Parse(splitString[2]);
                        anotherSide = (side + 1) % 2;
                        sendString = string.Format("win,{0}", side);
                        service.SendToBoth(gameTable[tableIndex], sendString);
                        gameTable[tableIndex].gamePlayer[side].started = false;
                        gameTable[tableIndex].gamePlayer[anotherSide].started = false;
                        break;

                    case "register":
                        try
                        {
                            // Client gửi: Register,username,password,fullname,question,answer
                            // splitString[0] là lệnh "Register"
                            if (splitString.Length < 6)
                            {
                                service.SendToOne(user, "RegisterFail,Thiếu thông tin");
                                break;
                            }

                            string regUser = splitString[1];
                            string regPass = splitString[2];
                            string regName = splitString[3];
                            string regQuest = splitString[4];
                            string regAns = splitString[5];

                            service.AddItem(string.Format("Đang đăng ký user: {0}", regUser));

                            // Gọi hàm RegisterUser đã sửa ở DatabaseHelper
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
                            service.AddItem("Lỗi Register: " + ex.Message);
                        }
                        break;
                }
            }
            userList.Remove(user);
            client.Close();
            service.AddItem(string.Format("There is one exit, remaining connected users: {0}", userList.Count));
        }
        private int ChangeY(int x)
        {
            return x + 2 * (4 - x);
        }
        //Transform the vertical coordinates of the pieces
        private int ChangeX(int y)
        {
            return y + 2 * (4 - y) + 1;
        }
        //Detect if the user is sitting on the game table, if so, remove it and terminate the table game
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
            if (gameTable[i].gamePlayer[otherSide].started == true)
            {
                gameTable[i].gamePlayer[otherSide].started = false;
                if (gameTable[i].gamePlayer[otherSide].user.client.Connected == true)
                {
                    service.SendToOne(gameTable[i].gamePlayer[otherSide].user,
                                        string.Format("Lost,{0},{1}",
                                        j, gameTable[i].gamePlayer[j].user.userName));
                }
            }
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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (myListener != null)
            {
                buttonStop_Click(null, null);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBoxMaxUsers_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
