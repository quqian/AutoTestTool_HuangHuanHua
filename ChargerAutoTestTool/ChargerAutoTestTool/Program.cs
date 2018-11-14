using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChargerAutoTestTool
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //if (DataProgram.PingIpOrDomainName(DataProgram.mysqlDomain) == false)
            //{
            //    int t = 0;
            //    while (DataProgram.PingIpOrDomainName(DataProgram.mysqlDomain) == false && t++ < 3);
            //    if (t >= 10)
            //    {
            //        MessageBox.Show("网络未连接!\r\n请确认连接后重试", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //        return;
            //    }
            //}
            if (DataProgram.IsConnectInternet()==false)
            {
                MessageBox.Show("网络未连接!\r\n请确认连接后重试", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Application.Run(new LoginForm());
            //Application.Run(new MainForm());
        }
    }
}
