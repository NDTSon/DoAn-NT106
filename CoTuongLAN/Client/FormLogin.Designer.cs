namespace Client
{
    partial class FormLogin
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label4 = new System.Windows.Forms.Label();
            this.linklb_register = new System.Windows.Forms.LinkLabel();
            this.tb_pword = new System.Windows.Forms.TextBox();
            this.tb_username = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_Login = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.tb_ServerIP = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Font = new System.Drawing.Font("Cascadia Code SemiBold", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabel1.Location = new System.Drawing.Point(318, 223);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(241, 24);
            this.linkLabel1.TabIndex = 18;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Forgot your password?";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Cascadia Code SemiBold", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(269, 308);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(142, 24);
            this.label4.TabIndex = 17;
            this.label4.Text = "Are you new?";
            // 
            // linklb_register
            // 
            this.linklb_register.AutoSize = true;
            this.linklb_register.Font = new System.Drawing.Font("Cascadia Code SemiBold", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linklb_register.Location = new System.Drawing.Point(445, 308);
            this.linklb_register.Name = "linklb_register";
            this.linklb_register.Size = new System.Drawing.Size(142, 24);
            this.linklb_register.TabIndex = 16;
            this.linklb_register.TabStop = true;
            this.linklb_register.Text = "Register now";
            this.linklb_register.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linklb_register_LinkClicked);
            // 
            // tb_pword
            // 
            this.tb_pword.Font = new System.Drawing.Font("Cascadia Code SemiBold", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tb_pword.Location = new System.Drawing.Point(215, 192);
            this.tb_pword.Name = "tb_pword";
            this.tb_pword.Size = new System.Drawing.Size(344, 28);
            this.tb_pword.TabIndex = 15;
            // 
            // tb_username
            // 
            this.tb_username.Font = new System.Drawing.Font("Cascadia Code SemiBold", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tb_username.Location = new System.Drawing.Point(215, 118);
            this.tb_username.Name = "tb_username";
            this.tb_username.Size = new System.Drawing.Size(344, 28);
            this.tb_username.TabIndex = 14;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Cascadia Code SemiBold", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(53, 195);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(120, 24);
            this.label2.TabIndex = 13;
            this.label2.Text = "Password :";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Cascadia Code SemiBold", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(53, 121);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(120, 24);
            this.label1.TabIndex = 12;
            this.label1.Text = "Username :";
            // 
            // btn_Login
            // 
            this.btn_Login.Font = new System.Drawing.Font("Cascadia Code SemiBold", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Login.Location = new System.Drawing.Point(609, 118);
            this.btn_Login.Name = "btn_Login";
            this.btn_Login.Size = new System.Drawing.Size(138, 102);
            this.btn_Login.TabIndex = 11;
            this.btn_Login.Text = "LOGIN";
            this.btn_Login.UseVisualStyleBackColor = true;
            this.btn_Login.Click += new System.EventHandler(this.btn_Login_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Cascadia Code SemiBold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(469, 31);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 22);
            this.label3.TabIndex = 19;
            this.label3.Text = "Nhập IP:";
            // 
            // tb_ServerIP
            // 
            this.tb_ServerIP.Font = new System.Drawing.Font("Cascadia Code SemiBold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tb_ServerIP.Location = new System.Drawing.Point(565, 33);
            this.tb_ServerIP.Name = "tb_ServerIP";
            this.tb_ServerIP.Size = new System.Drawing.Size(182, 27);
            this.tb_ServerIP.TabIndex = 20;
            // 
            // FormLogin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tb_ServerIP);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.linklb_register);
            this.Controls.Add(this.tb_pword);
            this.Controls.Add(this.tb_username);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_Login);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormLogin";
            this.Text = "LOGIN";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.LinkLabel linklb_register;
        private System.Windows.Forms.TextBox tb_pword;
        private System.Windows.Forms.TextBox tb_username;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_Login;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tb_ServerIP;
    }
}