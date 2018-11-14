using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CCWin;
using System.IO;
using MySql.Data.MySqlClient;

namespace ChargerAutoTestTool
{
    public partial class LoginForm : Skin_VS
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void skinButtonLogin_Click(object sender, EventArgs e)
        {
            // int index = -1;
            if (textBoxAccount.Text == "")
            {
                MessageBox.Show("用户名不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else if ((textBoxPassword.Text == ""))
            {
                MessageBox.Show("密码不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            for (int i = 0; i < DataProgram.Account.Count; i++)
            {
                if (textBoxAccount.Text == DataProgram.Account[i])
                {
                    if (textBoxPassword.Text == DataProgram.Password[i])
                    {
                        //MessageBox.Show("登陆成功");
                        DataProgram.PresentAccount = DataProgram.Account[i];
                        DataProgram.WriteLastUserName(DataProgram.lastLoginUserFile, DataProgram.PresentAccount);
                        //skinLabelLoginedName.Visible = true;
                        //this.tableLayoutPanelSerialSetting.Hide();

                       

                        this.Hide();

                        if (DataProgram.PresentAccount == "林培俊")
                        {
                            if (MessageBox.Show("擦！怎么又是你这个傻逼！\r\n承认傻逼点击“是”，否则不允许登陆！", "傻逼登陆中", MessageBoxButtons.YesNo,MessageBoxIcon.Error) == DialogResult.No)
                            {
                                this.Dispose();
                                return;
                            }
                        }

                        MainForm fm = new MainForm();
                        fm.Show();
                        return;
                    }
                }
            }

            MessageBox.Show("请检查用户名或密码是否正确", "登陆失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            textBoxAccount.Text = "";
            textBoxPassword.Text = "";
        }



        private void LoginForm_Load(object sender, EventArgs e)
        {
            try
            {

               // DataProgram.ModifyUserPassword("Testor","123456");

                DataProgram.Account = DataProgram.GetMysqlUserInfo("username");
                DataProgram.Password = DataProgram.GetMysqlUserInfo("password");

                if (DataProgram.Account == null)
                {
                    MessageBox.Show("用户信息读取失败","异常提示",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    Application.Exit();
                }
               // DataProgram.ReadUserInfo(DataProgram.encryptAccountFile);

                textBoxAccount.Text = DataProgram.ReadLastUserName(DataProgram.lastLoginUserFile);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message,"异常提示",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                
            }
         


         

        


        }

        private void textBoxPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if (e.KeyChar == '\r')
            //{
            //    skinButtonLogin_Click(sender,e);
            //}
        }

        private void textBoxAccount_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if (e.KeyChar == '\r')
            //{
            //    skinButtonLogin_Click(sender, e);
            //}
        }

        private void LoginForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                skinButtonLogin_Click(sender, e);
            }
        }
    }
}
