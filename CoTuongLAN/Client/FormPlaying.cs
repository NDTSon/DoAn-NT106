using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class FormPlaying : Form
    {
        private int counter = 60;
        private int tableIndex;
        private int side;
        private bool order;
        private int[,] grid = new int[10, 9];
        private int r;
        int baseX, baseY, interval, binterval;
        private Dictionary<int, string> chess = new Dictionary<int, string>();
        private int oriX, oriY;
        private int endX, endY;
        int pick;
        Bitmap bm = new Bitmap(700, 700);
        private bool isReceiveCommand = false;
        private Service service;
        delegate void LabelDelegate(Label label, string str);
        delegate void ButtonDelegate(Button button, bool flag);
        LabelDelegate labelDelegate;

        private void FormPlaying_Load(object sender, EventArgs e)
        {
            initChess();
            initGrid();
            labelSide0.Text = "";
            labelSide1.Text = "";
            labelOrder.Text = "";
            label1.Text = "";
            pictureBox1.Image = bm;
        }

        ButtonDelegate buttonDelegate;
        public FormPlaying(int TableIndex, int Side, StreamWriter sw)
        {
            InitializeComponent();
            this.tableIndex = TableIndex;
            this.side = Side;
            order = false;
            baseX = 50;
            baseY = 50;
            interval = 60;
            binterval = 30;
            r = 25;
            oriX = -1;
            oriY = -1;
            endX = -1;
            endY = -1;
            pick = -1;
            labelDelegate = new LabelDelegate(SetLabel);
            buttonDelegate = new ButtonDelegate(SetButton);
            service = new Client.Service(listBox1, sw);
        }
        public void SetLabel(Label label, string str)
        {
            if (label.InvokeRequired)
            {
                this.Invoke(labelDelegate, label, str);
            }
            else
            {
                label.Text = str;
            }
        }
        private void SetButton(Button button, bool flag)
        {
            if (button.InvokeRequired)
            {
                this.Invoke(buttonDelegate, button, flag);
            }
            else
            {
                button.Enabled = flag;
            }
        }


        private void initChess()
        {
            chess.Add(0, "红,車,1");
            chess.Add(1, "红,马,1");
            chess.Add(2, "红,象,1");
            chess.Add(3, "红,士,1");
            chess.Add(4, "红,帅");
            chess.Add(5, "红,士,2");
            chess.Add(6, "红,象,2");
            chess.Add(7, "红,马,2");
            chess.Add(8, "红,車,2");
            chess.Add(9, "红,炮,1");
            chess.Add(10, "红,炮,2");
            chess.Add(11, "红,兵,1");
            chess.Add(12, "红,兵,2");
            chess.Add(13, "红,兵,3");
            chess.Add(14, "红,兵,4");
            chess.Add(15, "红,兵,5");
            chess.Add(16, "黑,車,1");
            chess.Add(17, "黑,马,1");
            chess.Add(18, "黑,相,1");
            chess.Add(19, "黑,士,1");
            chess.Add(20, "黑,将,1");
            chess.Add(21, "黑,士,2");
            chess.Add(22, "黑,相,2");
            chess.Add(23, "黑,马,2");
            chess.Add(24, "黑,車,2");
            chess.Add(25, "黑,炮,1");
            chess.Add(26, "黑,炮,2");
            chess.Add(27, "黑,卒,1");
            chess.Add(28, "黑,卒,2");
            chess.Add(29, "黑,卒,3");
            chess.Add(30, "黑,卒,4");
            chess.Add(31, "黑,卒,5");

        }
        public void initGrid()
        {
            int i, j;
            for (i = 0; i <= grid.GetUpperBound(0); i++)
            {
                for (j = 0; j <= grid.GetUpperBound(1); j++)
                {
                    grid[i, j] = -1;
                }
            }
            if (side == 0)
            {
                for (i = 0; i < 9; i++)
                {
                    grid[0, i] = i;
                }
                grid[2, 1] = 9;
                grid[2, 7] = 10;
                grid[3, 0] = 11;
                grid[3, 2] = 12;
                grid[3, 4] = 13;
                grid[3, 6] = 14;
                grid[3, 8] = 15;
                for (j = 0, i = 16; i <= 24; i++, j++)
                {
                    grid[9, j] = i;
                }
                grid[7, 1] = 25;
                grid[7, 7] = 26;
                grid[6, 0] = 27;
                grid[6, 2] = 28;
                grid[6, 4] = 29;
                grid[6, 6] = 30;
                grid[6, 8] = 31;
            }
            else
            {
                for (i = 0; i < 9; i++)
                {
                    grid[9, i] = i;
                }
                grid[7, 1] = 9;
                grid[7, 7] = 10;
                grid[6, 0] = 11;
                grid[6, 2] = 12;
                grid[6, 4] = 13;
                grid[6, 6] = 14;
                grid[6, 8] = 15;
                for (j = 0, i = 16; i <= 24; i++, j++)
                {
                    grid[0, j] = i;
                }
                grid[2, 1] = 25;
                grid[2, 7] = 26;
                grid[3, 0] = 27;
                grid[3, 2] = 28;
                grid[3, 4] = 29;
                grid[3, 6] = 30;
                grid[3, 8] = 31;
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            service.SendToServer(string.Format("Talk,{0},{1}", tableIndex, textBox1.Text));
            textBox1.Text = "";
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                service.SendToServer(string.Format("Talk,{0},{1}", tableIndex, textBox1.Text));
                textBox1.Text = "";
            }
        }
        public void Restart(string str)
        {
            MessageBox.Show(str, "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            service.AddItemToListBox(str);
            initGrid();
            order = false;
            SetButton(buttonStart, true);
        }
        private void buttonStart_Click(object sender, EventArgs e)
        {
            service.SendToServer(string.Format("Start,{0},{1}", tableIndex, side));
            this.buttonStart.Enabled = false;
            initGrid();
            RePaint();
        }

        private void FormPlaying_FormClosing(object sender, FormClosingEventArgs e)
        {
            service.SendToServer(string.Format("GetUp,{0},{1}", tableIndex, side));
        }

        public void RePaint()
        {


            Graphics g = Graphics.FromImage(bm);
            Pen pen = new Pen(Color.DimGray, 2); // Đổi màu và kích thước của viền bàn cờ
            SolidBrush boardBrush = new SolidBrush(Color.AntiqueWhite); // Đổi màu nền của bàn cờ
            Font boardFont = new Font("Arial", 14, FontStyle.Regular); // Đổi font chữ và kích thước

            // Vẽ viền bàn cờ
            g.DrawRectangle(pen, baseX - binterval, baseY - binterval, 8 * interval + 2 * binterval, 9 * interval + 2 * binterval);
            g.FillRectangle(boardBrush, baseX - binterval, baseY - binterval, 8 * interval + 2 * binterval, 9 * interval + 2 * binterval);

            // Vẽ các đường kẻ bàn cờ
            for (int i = 0; i < 10; i++)
            {
                g.DrawLine(pen, baseX, baseY + interval * i, baseX + 8 * interval, baseY + interval * i);
            }
            for (int i = 0; i < 9; i++)
            {
                g.DrawLine(pen, baseX + interval * i, baseY, baseX + interval * i, baseY + 9 * interval);
            }

            // Vẽ chữ "楚河" và "汉界"
            g.DrawString("楚河", boardFont, Brushes.DarkRed, baseX + 1 * interval, baseY + 4 * interval);
            g.DrawString("汉界", boardFont, Brushes.DarkRed, baseX + 5 * interval, baseY + 4 * interval);

            // Vẽ lưới và chữ trên bàn cờ
            int hz = 5;
            for (int i = 0; i < 5; i++)
            {
                int px = baseX + 2 * i * interval;
                int py = baseY + 3 * interval;
                DrawCross(g, pen, px, py, hz);
                px = baseX + 2 * i * interval;
                py = baseY + 6 * interval;
                DrawCross(g, pen, px, py, hz);
            }
            for (int i = 0; i < 2; i++)
            {
                int px = baseX + interval + 6 * i * interval;
                int py = baseY + 2 * interval;
                DrawCross(g, pen, px, py, hz);
                px = baseX + interval + 6 * i * interval;
                py = baseY + 7 * interval;
                DrawCross(g, pen, px, py, hz);
            }

            // Vẽ các quân cờ
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (grid[i, j] != -1)
                    {
                        int y = baseY + i * interval;
                        int x = baseX + j * interval;
                        string[] splitString = chess[grid[i, j]].Split(',');

                        SolidBrush pieceBrush = splitString[0] == "红" ? new SolidBrush(Color.Red) : new SolidBrush(Color.Black); // Chọn màu sắc cho quân cờ
                        Font pieceFont = new Font("Arial", 20, FontStyle.Bold); // Đổi font chữ và kích thước cho quân cờ

                        Rectangle rt = new Rectangle(x - r, y - r, 2 * r, 2 * r);
                        g.FillEllipse(pieceBrush, rt);
                        g.DrawString(splitString[1], pieceFont, Brushes.White, x - r + 10, y - r + 10);
                    }
                }
            }

            g.Save();
            pictureBox1.Image = bm;
        }

        private void FormPlaying_MouseDown(object sender, MouseEventArgs e)
        {
            //If it is one's own move
            if (order == true)
            {
                double xt = (e.X - baseX) / (double)interval;
                double yt = (e.Y - baseY) / (double)interval;
                //Convert drawing coordinates to grid coordinates
                int y = (int)Math.Round(xt);
                int x = (int)Math.Round(yt);
                // in the chessboard range
                if (!(x < 0 || x > 9 || y < 0 || y > 8))
                {
                    //The selected piece is your own chess piece
                    if ((grid[x, y] != -1) && ((side == 1 && grid[x, y] < 16) || (side == 0 && grid[x, y] > 15)))
                    {
                        oriX = x;
                        oriY = y;
                        pick = grid[x, y];
                        RePaint();
                        drawFrame("green", x, y);
                    }
                    //Selected is the vacancy or the opponent's pawn
                    else
                    {
                        //if the original position has been determined
                        if (oriX != -1 && oriY != -1)
                        {
                            endX = x;
                            endY = y;
                            //if the rules are met
                            if (CheckRule(pick, oriX, oriY, endX, endY) == true)
                            {
                                service.SendToServer(string.Format("ChessInfo,{0},{1},{2},{3},{4},{5},{6}", tableIndex, side, pick,
                                    oriX, oriY, endX, endY));
                                oriX = -1;
                                oriY = -1;
                                endX = -1;
                                endY = -1;
                                pick = -1;
                                order = false;
                                return;
                            }
                            //incompatible
                            else
                            {
                                endX = -1;
                                endY = -1;
                            }
                        }
                    }
                }
            }
        }

        private void DrawCross(Graphics g, Pen pen, int px, int py, int hz)
        {
            g.DrawLine(pen, px - hz, py - hz, px - 2 * hz, py - hz);
            g.DrawLine(pen, px + hz, py - hz, px + 2 * hz, py - hz);
            g.DrawLine(pen, px - hz, py + hz, px - 2 * hz, py + hz);
            g.DrawLine(pen, px + hz, py + hz, px + 2 * hz, py + hz);
            g.DrawLine(pen, px - hz, py - hz, px - hz, py - 2 * hz);
            g.DrawLine(pen, px + hz, py - hz, px + hz, py - 2 * hz);
            g.DrawLine(pen, px - hz, py + hz, px - hz, py + 2 * hz);
            g.DrawLine(pen, px + hz, py + hz, px + hz, py + 2 * hz);
        }
        public void SetTableSideText(string sideString, string labelSideString, string listBoxString)
        {
            string s = "Red";
            if (sideString == "0")
            {
                s = "Black";
            }
            //Determine whether you are black or red
            if (sideString == side.ToString())
            {
                SetLabel(labelSide1, s + labelSideString);
            }
            else
            {
                SetLabel(labelSide0, s + labelSideString);
            }
            service.AddItemToListBox(listBoxString);
        }
        public void ShowTalk(string talkMan, string str)
        {
            service.AddItemToListBox(string.Format("{0} says: {1}", talkMan, str));
        }
        //Display information
        public void ShowMessage(string str)
        {
            service.AddItemToListBox(str);
        }
        private void StopFormPlaying()
        {
            Application.Exit();
        }
        public void CheckWin()
        {
            bool win = true;
            for (int i = 0; i <= 2; i++)
            {
                for (int j = 3; j <= 5; j++)
                {
                    if (grid[i, j] == 4 || grid[i, j] == 20)
                    {
                        win = false;
                    }
                }
            }
            if (win == true)
            {
                service.SendToServer(string.Format("win,{0},{1}", tableIndex, side));
            }
        }
        public void ShowForm(int sideWin)
        {
            if(this.side==sideWin)
            {
                FormWin formWin = new FormWin();
                formWin.ShowDialog();
            }
            else
            {
                FormLose formLose = new FormLose();
                formLose.ShowDialog();
            }
        }

        public void drawFrame(string color, int x, int y)
        {
                // x = row, y = column
                int px = baseX + y * interval; // pixel X coordinate
                int py = baseY + x * interval; // pixel Y coordinate
                Graphics g = Graphics.FromImage(bm);
                if (color == "green")
                {
                    Pen pen = new Pen(Color.Lime, 3);
                    g.DrawRectangle(pen, px - r, py - r, 2 * r, 2 * r);
                }
                else if (color == "blue")
                {
                    Pen pen = new Pen(Color.DeepSkyBlue, 3);
                    g.DrawRectangle(pen, px - r, py - r, 2 * r, 2 * r);
                }
                g.Save();
                pictureBox1.Image = bm;
        }
        public void ChangeChess(int cno, int x, int y)
        {
            grid[x, y] = cno;
        }
        public void Ready(int i)
        {
            if (i == 1)
            {
                order = true;
                SetLabel(labelOrder, "Our side moves");

            }
            else
            {
                order = false;
                SetLabel(labelOrder, "The opponent moves");

            }
        }
        //Update Order
        public void ChangeOrder(int i)
        {
            //It's time for the opponent to move
            if (i == side)
            {
                order = false;
                SetLabel(labelOrder, "The opponent moves");

            }
            else
            {
                order = true;
                SetLabel(labelOrder, "Our side moves");

            }
        }
        // check if the rules are met
        private bool CheckRule(int c, int x0, int y0, int x1, int y1)
        {
            int i;
            int miny, maxy;
            int minx, maxx;
            switch (c)
            {
                //car
                case 0:
                case 8:
                case 16:
                case 24:
                    //same line
                    if (x0 == x1)
                    {
                        // Determine if there is a chess piece between the two points
                        miny = y0 < y1 ? y0 : y1;
                        maxy = y0 > y1 ? y0 : y1;
                        for (i = miny + 1; i < maxy; i++)
                        {
                            //If there is a chess piece, return false directly
                            if (grid[x0, i] != -1)
                            {
                                return false;
                            }
                        }
                        if (i == maxy)
                        {
                            return true;
                        }
                    }
                    //same vertical line
                    else if (y0 == y1)
                    {
                        minx = x0 < x1 ? x0 : x1;
                        maxx = x0 > x1 ? x0 : x1;
                        for (i = minx + 1; i < maxx; i++)
                        {
                            if (grid[i, y0] != -1)
                            {
                                return false;
                            }
                        }
                        if (i == maxx)
                        {
                            return true;
                        }
                    }
                    return false;
                //horse
                case 1:
                case 7:
                case 17:
                case 23:
                    // down day
                    if (Math.Abs(y1 - y0) == 2 && Math.Abs(x1 - x0) == 1)
                    {
                        // don't lame
                        if (grid[x0, (y0 + y1) / 2] == -1)
                        {
                            return true;
                        }
                    }
                    // Immediately
                    else if (Math.Abs(x1 - x0) == 2 && Math.Abs(y1 - y0) == 1)
                    {
                        if (grid[(x0 + x1) / 2, y0] == -1)
                        {
                            return true;
                        }
                    }
                    return false;
                //image, phase
                case 2:
                case 6:
                case 18:
                case 22:
                    //The elephant cannot cross the river
                    if (x0 == 5 && x1 == 3)
                    {
                        return false;
                    }
                    else
                    {
                        //walk field word
                        if (Math.Abs(x1 - x0) == 2 && Math.Abs(y1 - y0) == 2)
                        {
                            //Do not block elephant eyes
                            if (grid[(x0 + x1) / 2, (y0 + y1) / 2] == -1)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                //scholar
                case 3:
                case 5:
                case 19:
                case 21:
                    //Scholar not out of field character grid
                    if (y1 < 3 || y1 > 5 || x1 < 7)
                    {
                        return false;
                    }
                    else
                    {
                        // move diagonally
                        if (Math.Abs(x1 - x0) == 1 && Math.Abs(y1 - y0) == 1)
                        {
                            return true;
                        }
                    }
                    return false;
                // will, handsome
                case 4:
                case 20:
                    //The general cannot go out of the field
                    if (y1 < 3 || y1 > 5 || x1 < 7)
                    {
                        return false;
                    }
                    else
                    {
                        // go straight
                        if ((Math.Abs(x1 - x0) == 1 && y0 == y1) || (Math.Abs(y1 - y0) == 1 && x0 == x1))
                        {
                            return true;
                        }
                    }
                    return false;
                //gun
                case 9:
                case 10:
                case 25:
                case 26:
                    //same line
                    if (x0 == x1)
                    {
                        miny = y0 < y1 ? y0 : y1;
                        maxy = y0 > y1 ? y0 : y1;
                        //move
                        if (grid[x1, y1] == -1)
                        {
                            // Determine if there is a chess piece between the two points
                            for (i = miny + 1; i < maxy; i++)
                            {
                                //If there is a chess piece, return false directly
                                if (grid[x0, i] != -1)
                                {
                                    return false;
                                }
                            }
                            if (i == maxy)
                            {
                                return true;
                            }
                        }
                        // eat child
                        else
                        {
                            int n = 0;
                            for (i = miny + 1; i < maxy; i++)
                            {
                                //There are pieces n++
                                if (grid[x0, i] != -1)
                                {
                                    n++;
                                }
                            }
                            if (n == 1)
                            {
                                return true;
                            }
                        }
                    }
                    //same vertical line
                    else if (y0 == y1)
                    {
                        minx = x0 < x1 ? x0 : x1;
                        maxx = x0 > x1 ? x0 : x1;
                        //move
                        if (grid[x1, y1] == -1)
                        {
                            // Determine if there is a chess piece between the two points
                            for (i = minx + 1; i < maxx; i++)
                            {
                                //If there is a chess piece, return false directly
                                if (grid[i, y0] != -1)
                                {
                                    return false;
                                }
                            }
                            if (i == maxx)
                            {
                                return true;
                            }
                        }
                        // eat child
                        else
                        {
                            int n = 0;
                            for (i = minx + 1; i < maxx; i++)
                            {
                                //There are pieces n++
                                if (grid[i, y0] != -1)
                                {
                                    n++;
                                }
                            }
                            if (n == 1)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                // soldier, pawn
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 27:
                case 28:
                case 29:
                case 30:
                case 31:
                    // Soldiers cannot retreat
                    if (x1 > x0)
                    {
                        return false;
                    }
                    else
                    {
                        // If you haven't crossed the river, you can only move forward one space
                        if (x0 >= 5)
                        {
                            if (Math.Abs(x1 - x0) == 1 && y0 == y1)
                            {
                                return true;
                            }
                        }
                        //Crossing the river, you can move forward, left, right
                        else
                        {
                            if ((Math.Abs(x1 - x0) == 1 && y0 == y1) || (Math.Abs(y1 - y0) == 1 && x0 == x1))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                default:
                    return false;
            }
        }
    }
}
