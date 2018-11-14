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

namespace ChargerAutoTestTool
{
    public partial class AccountSettingForm : Skin_VS
    {
        public AccountSettingForm()
        {
            InitializeComponent();
        }

        private void AccountSettingForm_Load(object sender, EventArgs e)
        {
           
            comboBoxAllUsers.Items.AddRange(DataProgram.Account.ToArray());
            
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void skinButtonAddUser_Click(object sender, EventArgs e)
        {
            if (textBoxUserName.Text != "" && textBoxPassword.Text != "")
            {
                if (DataProgram.Account.Contains(textBoxUserName.Text) == false)
                {
                    DataProgram.Account.Add(textBoxUserName.Text);
                    DataProgram.Password.Add(textBoxPassword.Text);
     
                    DataProgram.AddUserAndPassword(textBoxUserName.Text, textBoxPassword.Text);
                    MessageBox.Show("已添加"+ textBoxUserName.Text, "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    comboBoxAllUsers.Items.Add(textBoxUserName.Text);
                }
                else
                {
                    MessageBox.Show("账号已存在", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxUserName.Text = "";
                }
               
            }
            else
            {
                MessageBox.Show("账号或密码不能为空","提示",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
        }

        private void skinButtonComfirmModifyPwd_Click(object sender, EventArgs e)
        {
            if (textBoxModifiedPassword.Text == "")
            {
                MessageBox.Show("密码不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DataProgram.Password[comboBoxAllUsers.SelectedIndex] = textBoxModifiedPassword.Text;
            //DataProgram.SaveUserInfo(DataProgram.encryptAccountFile,DataProgram.Account,DataProgram.Password);

            DataProgram.ModifyUserPassword(DataProgram.Account[comboBoxAllUsers.SelectedIndex], DataProgram.Password[comboBoxAllUsers.SelectedIndex]);
            MessageBox.Show(comboBoxAllUsers.SelectedItem.ToString()+" 密码已设置成功", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void comboBoxAllUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxModifiedPassword.Text = DataProgram.Password[comboBoxAllUsers.SelectedIndex];
        }

        private void skinButtonDeleteUser_Click(object sender, EventArgs e)
        {

            if (comboBoxAllUsers.SelectedItem.ToString() == "Admin")
            {
                MessageBox.Show("此用户为管理用户\r\n不可删除!","警告",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }
            if (MessageBox.Show("确认删除用户:" + comboBoxAllUsers.SelectedItem.ToString() + "\r\n此操作不能撤回！", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                DataProgram.DeleteUser(comboBoxAllUsers.SelectedItem.ToString());
                DataProgram.Account.RemoveAt(comboBoxAllUsers.SelectedIndex);
                DataProgram.Password.RemoveAt(comboBoxAllUsers.SelectedIndex);
                comboBoxAllUsers.Items.RemoveAt(comboBoxAllUsers.SelectedIndex);
                textBoxModifiedPassword.Text = "";
            }

        }
    }
}
