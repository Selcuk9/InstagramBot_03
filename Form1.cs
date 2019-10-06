using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InstagramBot_03
{
    public partial class Form1 : Form
    {
        User user;
        public Form1()
        {
            InitializeComponent();
        }

        private void BtnSign_Click(object sender, EventArgs e)
        {
            user.InitializeUserSessionData(username.Text, password.Text);
            user.InitializeInstaApi();
            user.LoginUser();
        }

        private void BtnSendCode_Click(object sender, EventArgs e)
        {
            user.SendVerificationCode();
        }

        private void Form1_Load(object sender, EventArgs e)
        {            
            user = new User(info);
        }

        private void BtnVerify_Click(object sender, EventArgs e)
        {
            user.VerifyCode(code.Text);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            user.Log("hello");
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            user.GetInboxAndSendMessage();
        }
    }
}
