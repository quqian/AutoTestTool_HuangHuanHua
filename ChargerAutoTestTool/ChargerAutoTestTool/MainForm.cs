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
using System.Collections;
using System.Threading;

namespace ChargerAutoTestTool
{
    public partial class MainForm : Skin_VS
    {
        public MainForm()
        {
            InitializeComponent();
        }

        enum Command
        {
            KEY = 0x01,
            TapCard,
            LCD,
            Sim2G,
            Trumpet,
            Relay,
            SetPcbCode,
            SetCID,
            BLE,
            FwVersion,
            GetPcbCode,
            SetRegisterCode,
            SetDevType,
            Set2_4G_Gw_addr,
            SetTerminalInfo,
            SetServerAddr,
            SetServerPort,
            SetPrintSwitch,
            Reboot,
            SetUUID,
            TestMode = 0x99
        };

        string reportPath = @".\智能报表";

        //ArrayList arraybuffer = new ArrayList(byte);
        List<byte> arraybuffer = new List<byte> { };

        public static bool AllowTest = false;
        public static bool startTestting = false;

        struct GetResult
        {
            public int testMode;
            public int testModeAllow;
            public int key;
            public int[] keyValue;
            public int tapCard;
            public string cardNum;
            public int lcd;
            public int _2G;
            public int _2gCSQ;
            public string _2G_Iccid;
            public int trumpet;
            public int relay;
            public int measurementChip;
            public int[] getPower;
            public int SetCID;
            public int SetPcbCode;
            public int SetRegisterCode;
            public string MainBoardCode;
            public string InterfaceBoardCode;
            public int BLE;
            public string FwVersion;
            public UInt32 UsedTime_interface;
            public UInt32 UsedTime_main;
            public UInt32 UsedTime_Charger;
        };

        struct CountDownTime
        {
            public int testMode;

            public int key;

            public int tapCard;

            public int lcd;
            public int _2G;

            public int trumpet;
            public int relay;

            public int PowerSource;

            public int SetCID;

            public int SetPcbCode;
            public int BLE;

        };

        GetResult GetResultObj = new GetResult { testMode = -1, testModeAllow = -1, key = -1, keyValue = new int[12], tapCard = -1, lcd = -1, _2G = -1, _2gCSQ = -1, _2G_Iccid = "", trumpet = -1, relay = -1, measurementChip = -1, SetPcbCode = -1, BLE = -1, SetRegisterCode = -1, cardNum = "", getPower = new int[12], FwVersion = "", UsedTime_interface = 0, UsedTime_main = 0, UsedTime_Charger = 0, MainBoardCode = "", InterfaceBoardCode = "" };
        CountDownTime countDownTimeMainBorad = new CountDownTime { testMode = 0, key = 0, tapCard = 0, lcd = 0, _2G = 0, PowerSource = 0, trumpet = 0, relay = 0, SetCID = 0, SetPcbCode = 0, BLE = 0 };
        CountDownTime countDownTimeInterfaceBorad = new CountDownTime { testMode = 0, key = 0, tapCard = 0, lcd = 0, _2G = 0, PowerSource = 0, trumpet = 0, relay = 0, SetCID = 0, SetPcbCode = 0, BLE = 0 };
        CountDownTime countDownTimeCharger = new CountDownTime { testMode = 0, key = 0, tapCard = 0, lcd = 0, _2G = 0, PowerSource = 0, trumpet = 0, relay = 0, SetCID = 0, SetPcbCode = 0, BLE = 0 };

        Dictionary<string, string> mainboardTestData = new Dictionary<string, string>();
        Dictionary<string, string> interfaceboardTestData = new Dictionary<string, string>();
        Dictionary<string, string> chargerTestData = new Dictionary<string, string>();

        Dictionary<string, object> TestSettingInfo = new Dictionary<string, object>
        {
            {"ChargerModel","X10" },
            {"CountDown",30 },
            {"CardNum", "A1000000" },
            {"CsqLowerLimit",20 },
            {"CsqUpperLimit",60 },
            {"PowerLowerLimit",100 },
            {"PowerUpperLimit",1000 },
            {"AllFuncAgingTest",false}
        };

        void resetCountDownTime(out CountDownTime countDownTime)
        {
            countDownTime.testMode = 0;
            countDownTime.key = 0;
            countDownTime.tapCard = 0;
            countDownTime.lcd = 0;
            countDownTime._2G = 0;
            countDownTime.PowerSource = 0;
            countDownTime.trumpet = 0;
            countDownTime.relay = 0;
            countDownTime.SetCID = 0;
            countDownTime.SetPcbCode = 0;
            countDownTime.BLE = 0;
        }




        private void MainForm_Load(object sender, EventArgs e)
        {
            skinTabControlTestMenu.SelectTab(skinTabPageloginName);
            //skinTabControlTestMenu.SelectedTab.Text =
            skinTabPageloginName.Text = "当前登录用户:" + DataProgram.PresentAccount;
            if (DataProgram.PresentAccount == "Admin")
            {
                skinButtonAccountSetting.Visible = true;
            }
            else
            {
                skinButtonAccountSetting.Visible = false;

            }
            TestSettingInfo = DataProgram.ReadConfig(DataProgram.testConfigFile, TestSettingInfo);

            comboBoxChargerModel.SelectedItem = TestSettingInfo["ChargerModel"];
            numericUpDownTestWaittime.Value = Convert.ToDecimal(TestSettingInfo["CountDown"]);
            textBoxTestCardNum.Text = TestSettingInfo["CardNum"].ToString();
            numericUpDownCsqLowerLimit.Value = Convert.ToDecimal(TestSettingInfo["CsqLowerLimit"]);
            numericUpDownCsqUpperLimit.Value = Convert.ToDecimal(TestSettingInfo["CsqUpperLimit"]);
            numericUpDownPowerLowerLimit.Value = Convert.ToDecimal(TestSettingInfo["PowerLowerLimit"]);
            numericUpDownPowerUpperLimit.Value = Convert.ToDecimal(TestSettingInfo["PowerUpperLimit"]);
            string agingtest = TestSettingInfo["AllFuncAgingTest"].ToString();
            checkBoxHoleAgingTest.Checked = Convert.ToBoolean(agingtest == "" ? "false" : agingtest);
            //DataProgram.WriteConfig(DataProgram.testConfigFile,TestSettingInfo);
            timer1.Enabled = true;
            timer1.Start();

            try
            {
                if (Directory.Exists(reportPath) == false)
                {
                    Directory.CreateDirectory(reportPath);
                }


                //添加串口项目  
                foreach (string s in System.IO.Ports.SerialPort.GetPortNames())
                {//获取有多少个COM口  
                    skinComboBoxPortSelect.Items.Add(s);
                }
                if (skinComboBoxPortSelect.Items.Count > 0)
                {
                    skinComboBoxPortSelect.SelectedIndex = 0;
                    skinComboBoxBandRate.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            DataProgram.DealBackUpData(DataProgram.backupMysqlCmdFile);

        }

        public bool SendSerialData(byte[] data)
        {
            bool state = false;
            try
            {
                if (this.serialPort1 != null)
                {

                    serialPort1.Write(data, 0, data.Length);

                    state = true;
                }
            }
            catch (Exception ex)
            {
                updateText(ex.Message);
                state = false;
            }
            return state;

        }
        static byte sequence = 0;
        public static byte[] MakeSendArray(byte cmd, byte[] data)
        {
            List<byte> list = new List<byte> { };
            list.Add(0xAA);
            list.Add(0x55);

            byte[] srtDes = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            list.AddRange(srtDes);
            byte ver = 0x01;
            sequence++;
            UInt16 length;
            if (data != null)
            {
                length = (UInt16)(1 + 1 + 1 + data.Length + 1);
            }
            else
            {
                length = 2;
            }

            list.Add((byte)(length));
            list.Add((byte)(length >> 8));
            list.Add(ver);
            list.Add(sequence);
            list.Add(cmd);
            if (data != null)
            {
                list.AddRange(data);
            }

            list.Add(DataProgram.caculatedCRC(list.ToArray(), list.Count));

            return list.ToArray();
        }

        private void SendSetDevType(byte type)
        {
            byte[] data = { type };
            SendSerialData(MakeSendArray((byte)Command.SetDevType, data));
        }

        private void SendDevReboot()
        {
            SendSerialData(MakeSendArray((byte)Command.Reboot, null));
        }

        private void SendSetUUID(string uuid)
        {
            string str = DataProgram.fillString(uuid, 12, '0', 0);
            byte[] data = DataProgram.stringToBCD(str);
            SendSerialData(MakeSendArray((byte)Command.SetUUID, data));
        }

        private void SendSetServerAddr(string addr)
        {
            byte[] data = Encoding.Default.GetBytes(addr+'\0');
            SendSerialData(MakeSendArray((byte)Command.SetServerAddr, data));
        }

        private void SendSetServerPort(string portStr)
        {
            UInt16 port16 = Convert.ToUInt16(portStr);

            byte[] portByte = { (byte)(port16 >> 8), (byte)port16};

            SendSerialData(MakeSendArray((byte)Command.SetServerPort, portByte));
        }

        private void SendSetPrintSwitch(byte sw)
        {
            byte[] data = {sw};
            SendSerialData(MakeSendArray((byte)Command.SetPrintSwitch, data));
        }

        private void SendSetGwAddr(string addrStr)
        {
            try
            {
                //string[] addr = addrStr.Split('.');
                //byte[] data = new byte[addr.Length];

                //for (int i = 0; i < data.Length; i++)
                //{
                //    data[i] = Convert.ToByte(addr[i]);
                //}

                string str = DataProgram.fillString(addrStr, 10, '0', 0);
                byte[] data = DataProgram.stringToBCD(str);
                //byte[] data = Encoding.Default.GetBytes(addr);
                SendSerialData(MakeSendArray((byte)Command.Set2_4G_Gw_addr, data));
            }
            catch (Exception ex)
            {

                updateText("异常："+ex.Message);
            }
           
        }

        private void SendSetTerminalInfo(List<string> info)
        {
            try
            {
                List<byte> list = new List<byte> { };

                list.Add((byte)info.Count);
                string temp = "";
                foreach (var item in info)
                {
                    temp = String.Copy(item);
                    DataProgram.fillString(temp, 10, '0', 0);
                    list.AddRange(DataProgram.stringToBCD(temp));
                }
                SendSerialData(MakeSendArray((byte)Command.SetTerminalInfo, list.ToArray()));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }


        private void SendSetRegisterCode(string registerCode)
        {
            byte[] data = Encoding.Default.GetBytes(registerCode);
            GetResultObj.SetRegisterCode = -1;
            SendSerialData(MakeSendArray((byte)Command.SetRegisterCode, data));
            int wait = 0, n = 0;
            while (GetResultObj.SetRegisterCode == -1)
            {
                Thread.Sleep(30);
                if (wait++ > 10)
                {
                    wait = 0;
                    SendSerialData(MakeSendArray((byte)Command.SetRegisterCode, data));
                    n++;
                }
                if (n > 10)
                {
                    break;
                }
            }

            if (n > 10 || GetResultObj.SetRegisterCode == 1)
            {
                if (MessageBox.Show("注册码设置失败！\r\n是否重试", "提示", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
                {
                    SendSetRegisterCode(registerCode);
                }
            }


        }

        private void SendSetID(string id)
        {
            string str = DataProgram.fillString(id, 16, '0', 0);
            byte[] data = DataProgram.stringToBCD(str);
            GetResultObj.SetCID = -1;
            SendSerialData(MakeSendArray((byte)Command.SetCID, data));
            int wait = 0, n = 0;
            while (GetResultObj.SetCID == -1)
            {
                Thread.Sleep(30);
                if (wait++ > 10)
                {
                    wait = 0;
                    SendSerialData(MakeSendArray((byte)Command.SetCID, data));
                    n++;
                }
                if (n > 10)
                {
                    break;
                }
            }

            if (n > 10)
            {
                if (MessageBox.Show("桩号设置失败！\r\n是否重试", "提示", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
                {
                    SendSetID(id);
                }
            }


        }
        private void SendTestMode(byte operate)
        {
            byte[] data= { operate };

            GetResultObj.testMode = -1;
            GetResultObj.testModeAllow = -1;
            SendSerialData(MakeSendArray((byte)Command.TestMode, data));
            int wait = 0, n = 0;
           
            while (GetResultObj.testMode == -1)
            {
                Thread.Sleep(30);
                if (wait++ > 10)
                {
                    wait = 0;
                    n++;
                    SendSerialData(MakeSendArray((byte)Command.TestMode, data));
                   
                }
                if (n > 10)
                {
                    break;
                }
                
            }

            if (n > 10)
            {
                if (MessageBox.Show((operate==0)?"请求开始失败！\r\n是否重试": "请求结束失败！\r\n是否重试", "提示", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
                {
                    SendTestMode(operate);
                }
            }
        }

        private void SendKeyTest()
        {
            SendSerialData( MakeSendArray((byte)Command.KEY,null));
        }
        private void SendBleTest()
        {
            GetResultObj.BLE = -1;
            SendSerialData(MakeSendArray((byte)Command.BLE, null));
            int wait = 0, n = 0;

            while (GetResultObj.BLE == -1)
            {
                Thread.Sleep(30);
                if (wait++ > 10)
                {
                    wait = 0;
                    n++;
                    SendSerialData(MakeSendArray((byte)Command.BLE, null));

                }
                if (n > 10)
                {
                    break;
                }

            }
        }
        private void SendTapCard()
        {
            GetResultObj.tapCard = -1;
            SendSerialData(MakeSendArray((byte)Command.TapCard, null));
            //int wait = 0, n = 0;

            //while (GetResultObj.tapCard == -1)
            //{
            //    Thread.Sleep(30);
            //    if (wait++ > 10)
            //    {
            //        wait = 0;
            //        n++;
            //        SendSerialData(MakeSendArray((byte)Command.TapCard, null));

            //    }
            //    if (n > 10)
            //    {
            //        break;
            //    }

            //}
        }
        private void SendLCD()
        {
            SendSerialData(MakeSendArray((byte)Command.LCD, null));


        }
        private void Send2GModule()
        {
            GetResultObj._2G = -1;
            SendSerialData(MakeSendArray((byte)Command.Sim2G, null));
            int wait = 0, n = 0;

            while (GetResultObj.tapCard == -1)
            {
                Thread.Sleep(30);
                if (wait++ > 10)
                {
                    wait = 0;
                    n++;
                    SendSerialData(MakeSendArray((byte)Command.Sim2G, null));
                }
                if (n > 10)
                {
                    break;
                }

            }
        }
        private void SendTrumpet()
        {
            SendSerialData(MakeSendArray((byte)Command.Trumpet, null));
        }
        private void SendRelay(byte operate,byte ch)
        {

            byte[] data = {operate,ch };
            GetResultObj.relay = -1;
            SendSerialData(MakeSendArray((byte)Command.Relay, data));
            //int wait = 0, n = 0;
            //while (GetResultObj.relay == -1)
            //{
            //    Thread.Sleep(30);
            //    if (wait++ > 10)
            //    {
            //        wait = 0;
            //        n++;
            //        SendSerialData(MakeSendArray((byte)Command.Relay, null));
            //    }
            //    if (n > 10)
            //    {
            //        break;
            //    }

            //}


        }
        private void SendSetPcbCode(byte type, string code)
        {
            List<byte> data = new List<byte>();
            data.Add(type);
            string str = DataProgram.fillString(code, 16, '0', 0);
            data.AddRange(DataProgram.stringToBCD(str));
            GetResultObj.SetPcbCode = -1;
            SendSerialData(MakeSendArray((byte)Command.SetPcbCode, data.ToArray()));           
            int wait=0, n = 0;
            while (GetResultObj.SetPcbCode == -1)
            {
                Thread.Sleep(20);
                if (wait++ > 10)
                {
                    wait = 0;
                    n++;
                    SendSerialData(MakeSendArray((byte)Command.SetPcbCode, data.ToArray()));
                   
                }
                if (n > 10)
                {
                    break;
                }
               
            }

            if (n > 10)
            {
                if (MessageBox.Show("PCB编号设置失败！\r\n是否重试", "提示", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
                {
                    SendSetPcbCode(type,code);
                }
            }
        }
        private void SendFwVersion(byte operate)
        {
            byte[] data = { operate };
            GetResultObj.FwVersion = "";
            SendSerialData(MakeSendArray((byte)Command.FwVersion, data));
            int waittime = 0, n = 0;
            while (GetResultObj.FwVersion == "")
            {
                Thread.Sleep(20);
                waittime++;
                if (waittime > 10)
                {
                    n++;
                    waittime = 0;
                    SendSerialData(MakeSendArray((byte)Command.FwVersion, data));
                }
                if (n > 10)
                {
                    break;
                }
            }
            if (n > 10)
            {
                if (MessageBox.Show("获取PCB软件版本失败！\r\n是否重试", "提示", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
                {
                    SendFwVersion(operate);
                }
            }
        }

        private void SendGetPcdCode(byte operate)
        {
            byte[] data = {operate };
            SendSerialData(MakeSendArray((byte)Command.GetPcbCode, data));
            int waittime = 0, n = 0;

            if (operate == 0)
            {
                GetResultObj.MainBoardCode = "";
                SendSerialData(MakeSendArray((byte)Command.GetPcbCode, data));
                while (GetResultObj.MainBoardCode == "")
                {
                    Thread.Sleep(30);
                    waittime++;
                    if (waittime > 10)
                    {
                        n++;
                        waittime = 0;
                        SendSerialData(MakeSendArray((byte)Command.GetPcbCode, data));
                    }
                    if (n > 10)
                    {
                        break;
                    }
                }
            }
            else if (operate == 1)
            {
                GetResultObj.InterfaceBoardCode = "";
                SendSerialData(MakeSendArray((byte)Command.GetPcbCode, data));
                while ((GetResultObj.InterfaceBoardCode == ""))
                {
                    Thread.Sleep(30);
                    waittime++;
                    if (waittime > 10)
                    {
                        n++;
                        waittime = 0;
                        SendSerialData(MakeSendArray((byte)Command.GetPcbCode, data));
                    }
                    if (n > 10)
                    {
                        break;
                    }
                }
            }

            if (n > 10)
            {
                if (MessageBox.Show("获取PCB编号失败！\r\n是否重试", "提示", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
                {
                    SendGetPcdCode(operate);
                }
            }


        }

       




        private void skinButtonSerialCtrl_Click(object sender, EventArgs e)
        {
            try
            {
                if (!serialPort1.IsOpen)
                {
                    serialPort1.BaudRate = int.Parse(skinComboBoxBandRate.SelectedItem.ToString());
                    serialPort1.PortName = skinComboBoxPortSelect.SelectedItem.ToString();
                    serialPort1.Open();
                    if (serialPort1.IsOpen)
                    {
                        skinButtonSerialCtrl.Text = "关闭串口";
                       
                        //sendTestRequestCmd((byte)Command._485);
                    }
                }
                else if (serialPort1.IsOpen)
                {
                    serialPort1.Close();
                    if (!serialPort1.IsOpen)
                    {
                        skinButtonSerialCtrl.Text = "打开串口";
                       
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "异常提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void updateText(String text)
        {
            try
            {
                this.textBoxDebug.Invoke(
                 new MethodInvoker(delegate {

                     this.textBoxDebug.AppendText(text + "\r\n");
                 }
               )
            );
                this.textBoxDebuginfo.Invoke(
                 new MethodInvoker(delegate {

                     this.textBoxDebuginfo.AppendText(text + "\r\n");
                 }
               )
            );
                this.textBoxConfigPrint.Invoke(
                new MethodInvoker(delegate {

                    this.textBoxConfigPrint.AppendText(text + "\r\n");
                }
              )
           );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }


        private void updateControlText(Control control,string text)
        {
            try
            {
                 control.Invoke(
                 new MethodInvoker(delegate {

                     control.Text = text;
                 }
               )
            );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void updateControlText(Control control, string text, Color color)
        {
            try
            {
                control.Invoke(
                new MethodInvoker(delegate {

                    control.Text = text;
                    control.ForeColor = color;
                }
              )
           );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void skinButtonReportDir_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", reportPath);
            }
            catch (Exception ex)
            {
                updateText(ex.Message);
            }
        }

        private void skinButtonClear_Click(object sender, EventArgs e)
        {
            textBoxDebug.Text = "";
        }

        

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            int n = serialPort1.BytesToRead;
            byte[] buf = new byte[n];
            serialPort1.Read(buf, 0, n);

            //MemoryStream ms = new MemoryStream();
            //ms.Write(buf, 0, n);
            //buf = ms.ToArray();
            //for (int i = 0; i < buf.Length; i++)
            //{
            //    arraybuffer.Add(buf[i]);
            //}
            arraybuffer.AddRange(buf);
            FormatData(arraybuffer.ToArray());

        }

        private void updateTableSelectedIndex(TabControl tabControl,int index)
        {
            try
            {
                 tabControl.Invoke(
                 new MethodInvoker(delegate {

                     tabControl.SelectedIndex = index;
                 }
               )
            );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private TabPage getPresentTabPage(TabControl tabControl)
        {
            TabPage tabPage = null;
            try
            {
                tabControl.Invoke(
                new MethodInvoker(delegate {
                    tabPage = tabControl.SelectedTab;
                })    );
            }
            catch (Exception ex)
            {

                updateText(ex.Message);
            }
            return tabPage;
        }

        private void showKeyValue(byte value,bool reset)
        {
            if (reset == true)
            {
                updateControlText(skinLabelKey0, "");
                updateControlText(skinLabelKey1, "");
                updateControlText(skinLabelKey2, "");
                updateControlText(skinLabelKey3, "");
                updateControlText(skinLabelKey4, "");
                updateControlText(skinLabelKey5, "");
                updateControlText(skinLabelKey6, "");
                updateControlText(skinLabelKey7, "");
                updateControlText(skinLabelKey8, "");
                updateControlText(skinLabelKey9, "");
                updateControlText(skinLabelKeyComfirm, "");
                updateControlText(skinLabelKeyReturn, "");

                updateControlText(skinLabelInterfaceKey0, "");
                updateControlText(skinLabelInterfaceKey1, "");
                updateControlText(skinLabelInterfaceKey2, "");
                updateControlText(skinLabelInterfaceKey3, "");
                updateControlText(skinLabelInterfaceKey4, "");
                updateControlText(skinLabelInterfaceKey5, "");
                updateControlText(skinLabelInterfaceKey6, "");
                updateControlText(skinLabelInterfaceKey7, "");
                updateControlText(skinLabelInterfaceKey8, "");
                updateControlText(skinLabelInterfaceKey9, "");
                updateControlText(skinLabelInterfaceKeyComfirm, "");
                updateControlText(skinLabelInterfaceKeyReturn, "");


                updateControlText(skinLabelChargerKey0, "");
                updateControlText(skinLabelChargerKey1, "");
                updateControlText(skinLabelChargerKey2, "");
                updateControlText(skinLabelChargerKey3, "");
                updateControlText(skinLabelChargerKey4, "");
                updateControlText(skinLabelChargerKey5, "");
                updateControlText(skinLabelChargerKey6, "");
                updateControlText(skinLabelChargerKey7, "");
                updateControlText(skinLabelChargerKey8, "");
                updateControlText(skinLabelChargerKey9, "");
                updateControlText(skinLabelChargerKeyComfirm, "");
                updateControlText(skinLabelChargerKeyReturn, "");

            }
            else
            {
                switch (value)
                {
                    case 0:
                        updateControlText(skinLabelKey0, "0");
                        updateControlText(skinLabelInterfaceKey0, "0");
                        updateControlText(skinLabelChargerKey0, "0");
                        break;
                    case 1:
                        updateControlText(skinLabelKey1, "1");
                        updateControlText(skinLabelInterfaceKey1, "1");
                        updateControlText(skinLabelChargerKey1, "1");
                        break;
                    case 2:
                        updateControlText(skinLabelKey2, "2");
                        updateControlText(skinLabelInterfaceKey2, "2");
                        updateControlText(skinLabelChargerKey2, "2");
                        break;
                    case 3:
                        updateControlText(skinLabelKey3, "3");
                        updateControlText(skinLabelInterfaceKey3, "3");
                        updateControlText(skinLabelChargerKey3, "3");
                        break;
                    case 4:
                        updateControlText(skinLabelKey4, "4");
                        updateControlText(skinLabelInterfaceKey4, "4");
                        updateControlText(skinLabelChargerKey4, "4");
                        break;
                    case 5:
                        updateControlText(skinLabelKey5, "5");
                        updateControlText(skinLabelInterfaceKey5, "5");
                        updateControlText(skinLabelChargerKey5, "5");
                        break;
                    case 6:
                        updateControlText(skinLabelKey6, "6");
                        updateControlText(skinLabelInterfaceKey6, "6");
                        updateControlText(skinLabelChargerKey6, "6");
                        break;
                    case 7:
                        updateControlText(skinLabelKey7, "7");
                        updateControlText(skinLabelInterfaceKey7, "7");
                        updateControlText(skinLabelChargerKey7, "7");
                        break;
                    case 8:
                        updateControlText(skinLabelKey8, "8");
                        updateControlText(skinLabelInterfaceKey8, "8");
                        updateControlText(skinLabelChargerKey8, "8");
                        break;
                    case 9:
                        updateControlText(skinLabelKey9, "9");
                        updateControlText(skinLabelInterfaceKey9, "9");
                        updateControlText(skinLabelChargerKey9, "9");
                        break;

                    case 10:
                        updateControlText(skinLabelKeyReturn, "返回");
                        updateControlText(skinLabelInterfaceKeyReturn, "返回");
                        updateControlText(skinLabelChargerKeyReturn, "返回");
                        break;
                    case 11:
                        updateControlText(skinLabelKeyComfirm, "确认");
                        updateControlText(skinLabelInterfaceKeyComfirm, "确认");
                        updateControlText(skinLabelChargerKeyComfirm, "确认");
                        break;

                    default:
                        break;

                }
            }
         

        }

        private void FormatData(byte[] buffer)
        {

            byte[] buf = new byte[buffer.Length];
            Array.Copy(buffer,buf,buffer.Length);
            int[] ZeroPower= new int[12];
            try
            {
                if (buf.Length>17)
                {
                    for (int i = 0; i < buf.Length; i++)
                    {
                        if (buf[i] == 0xAA && buf[i + 1] == 0x55)
                        {
                            //startindex = i;
                            int length = (buf[i + 12])+(buf[i + 13]<<8) + 14;
                            if (buf.Length >= length + i)
                            {
                                byte getCRC = buf[length + i - 1];
                                byte[] validFrame = new byte[length];
                                
                                Array.Copy(buf, i, validFrame, 0, validFrame.Length);

                                byte calcCRC = DataProgram.caculatedCRC(validFrame, validFrame.Length - 1);

                                if (getCRC == calcCRC)
                                {
                                    arraybuffer.Clear();
                                    //endindex = length + i;
                                    //string receive = "";
                                    //for (int j = 0; j < validFrame.Length; j++)
                                    //{
                                    //    receive += validFrame[j].ToString("X2") + " ";
                                    //}
                                    //updateText("Receive: " + receive);
                                    byte frameCmd = validFrame[16];

                                    switch (frameCmd)
                                    {
                                        case (byte)Command.TestMode:
                                            GetResultObj.testMode = validFrame[17];
                                            GetResultObj.testModeAllow = validFrame[18];
                                            if (GetResultObj.testMode == 0x00)
                                            {
                                                if (GetResultObj.testModeAllow == 0x00)
                                                {

                                                    updateControlText(skinButtonStandraTest, "结束测试");
                                                    startTestting = true;
                                                    AllowTest = true;
                                                    Thread.Sleep(200);
                                                    //DataProgram. setArrayValue(DataProgram.testReportData,"",0, DataProgram.testReportData.Length);
                                                    DateTime now = DateTime.Now;
                                                    // if (getPresentTabPage(skinTabControlPCBTestItem) == skinTabPageMainboard)
                                                    if (mainBoardTestingflag)
                                                    {


                                                        mainboardTestData.Clear();
                                                        mainboardTestData.Add("PCB编号", textBoxMainboardCode.Text.Trim());
                                                        mainboardTestData.Add("测试员", DataProgram.PresentAccount);
                                                        mainboardTestData.Add("软件版本", "");
                                                        mainboardTestData.Add("测试结果", "");
                                                        mainboardTestData.Add("刷卡", "");
                                                        mainboardTestData.Add("卡号", "00000000");
                                                        mainboardTestData.Add("2G模块", "");
                                                        mainboardTestData.Add("信号值", "0");
                                                        mainboardTestData.Add("功率", "");
                                                        mainboardTestData.Add("按键", "");
                                                        mainboardTestData.Add("LCD", "");
                                                        mainboardTestData.Add("喇叭", "");
                                                        mainboardTestData.Add("电源", "");
                                                        mainboardTestData.Add("测试时间", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                                        mainboardTestData.Add("测试用时", "0");

                                                        GetResultObj.UsedTime_main = GetCurrentTimeStamp();

                                                        mainBoardTabSelectIndex = 1;
                                                        updateTableSelectedIndex(skinTabControlPCBAMainBoardTest, mainBoardTabSelectIndex);
                                                    }
                                                    else if (interfaceBoardTestingflag)
                                                    {
                                                        interfaceboardTestData.Clear();
                                                        interfaceboardTestData.Add("PCB编号", textBoxInterfaceCode.Text.Trim());
                                                        interfaceboardTestData.Add("测试员", DataProgram.PresentAccount);
                                                        interfaceboardTestData.Add("软件版本", "");
                                                        interfaceboardTestData.Add("测试结果", "");
                                                        interfaceboardTestData.Add("刷卡", "");
                                                        interfaceboardTestData.Add("卡号", "00000000");
                                                        interfaceboardTestData.Add("按键", "");
                                                        interfaceboardTestData.Add("蓝牙", "");
                                                        interfaceboardTestData.Add("测试时间", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                                        interfaceboardTestData.Add("测试用时", "0");

                                                        GetResultObj.UsedTime_interface = GetCurrentTimeStamp();

                                                        interfaceTabSelectIndex = 1;
                                                        updateTableSelectedIndex(skinTabControlInterfaceTest, interfaceTabSelectIndex);
                                                    }
                                                    else if (chargerTestingflag)
                                                    {
                                                        updateControlText(skinButtonStartTestCharger, "结束测试");
                                                        chargerTestData.Clear();
                                                        chargerTestData.Add("电桩号", textBoxChargerQrCode.Text.Trim());
                                                        chargerTestData.Add("主板编号", "");
                                                        chargerTestData.Add("按键板编号", "");
                                                        chargerTestData.Add("测试员", DataProgram.PresentAccount);
                                                        chargerTestData.Add("软件版本", "");
                                                        chargerTestData.Add("按键板软件版本", "");
                                                        chargerTestData.Add("测试结果", "");
                                                        chargerTestData.Add("刷卡", "");
                                                        chargerTestData.Add("卡号", "00000000");
                                                        chargerTestData.Add("按键", "");
                                                        chargerTestData.Add("LCD", "");
                                                        chargerTestData.Add("喇叭", "");
                                                        chargerTestData.Add("功率", "");
                                                        chargerTestData.Add("2G模块", "");
                                                        chargerTestData.Add("信号值", "0");
                                                        chargerTestData.Add("ICCID", "0000000000");
                                                        chargerTestData.Add("蓝牙", "0000000000");

                                                        chargerTestData.Add("测试时间", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                                        chargerTestData.Add("测试用时", "0");

                                                        GetResultObj.UsedTime_Charger = GetCurrentTimeStamp();

                                                        chargerTestSelectIndex = 1;
                                                        updateTableSelectedIndex(skinTabControlChargerTest, chargerTestSelectIndex);
                                                    }



                                                }
                                                else
                                                {
                                                    updateControlText(skinButtonStandraTest, "开始测试");
                                                    updateControlText(skinButtonStartTestCharger, "开始测试");
                                                    startTestting = false;
                                                }
                                            }
                                            else if (GetResultObj.testMode == 0x01)
                                            {
                                                if (GetResultObj.testModeAllow == 0x00)
                                                {

                                                    //updateText("允许测试");
                                                    updateControlText(skinButtonStandraTest, "开始测试");
                                                    updateControlText(skinButtonStartTestCharger, "开始测试");
                                                    Thread.Sleep(200);
                                                    startTestting = false;
                                                    AllowTest = false;

                                                }
                                                else
                                                {
                                                    updateControlText(skinButtonStartTestCharger, "结束测试");
                                                    updateControlText(skinButtonStandraTest, "结束测试");
                                                    startTestting = true;
                                                }
                                            }
                                            break;


                                        case (byte)Command.KEY:
                                            GetResultObj.key = validFrame[17];
                                            int count = 0;
                                            if (GetResultObj.key == 0x01)
                                            {
                                                GetResultObj.keyValue[validFrame[18]] = 1;
                                                updateText("Key Value:" + validFrame[18]);

                                                for (int m = 0; m < 12; m++)
                                                {
                                                    if (GetResultObj.keyValue[m] == 1)
                                                    {
                                                        count++;
                                                    }


                                                }
                                                showKeyValue(validFrame[18], false);
                                                if (mainBoardTestingflag)
                                                {
                                                    if (count >= 2)
                                                    {
                                                        mainboardTestData["按键"] = "通过";
                                                        updateText("所有按键正常");
                                                        updateControlText(skinLabelKeyResult, "测试通过", Color.Green);
                                                        if (getPresentTabPage(skinTabControlPCBAMainBoardTest) == skinTabPageKey)
                                                        {
                                                            updateTableSelectedIndex(skinTabControlPCBAMainBoardTest, ++mainBoardTabSelectIndex);
                                                        }

                                                    }
                                                    else
                                                    {
                                                        mainboardTestData["按键"] = "不通过";
                                                        updateControlText(skinLabelKeyResult, "部分按键未按下!", Color.Red);
                                                    }

                                                }
                                                else if (interfaceBoardTestingflag)
                                                {
                                                    if (count >= 12)
                                                    {
                                                        interfaceboardTestData["按键"] = "通过";
                                                        updateText("所有按键正常");
                                                        updateControlText(skinLabelInterfaceKeyResult, "测试通过", Color.Green);
                                                        if (getPresentTabPage(skinTabControlInterfaceTest) == skinTabPageKeyTest)
                                                            updateTableSelectedIndex(skinTabControlInterfaceTest, ++interfaceTabSelectIndex);
                                                    }
                                                    else
                                                    {
                                                        interfaceboardTestData["按键"] = "不通过";
                                                        updateControlText(skinLabelInterfaceKeyResult, "部分按键未按下！", Color.Red);
                                                    }
                                                }
                                                else if (chargerTestingflag)
                                                {
                                                    if (count >= 12)
                                                    {
                                                        chargerTestData["按键"] = "通过";
                                                        updateText("所有按键正常");
                                                        updateControlText(skinLabelChargerKeyResult, "测试通过", Color.Green);
                                                        if (getPresentTabPage(skinTabControlChargerTest) == skinTabPageChargerKey)
                                                            updateTableSelectedIndex(skinTabControlChargerTest, ++chargerTestSelectIndex);
                                                    }
                                                    else
                                                    {
                                                        chargerTestData["按键"] = "不通过";
                                                        updateControlText(skinLabelChargerKeyResult, "部分按键未按下！", Color.Red);
                                                    }
                                                }


                                            }
                                            else
                                            {
                                                for (int l = 0; l < 12; l++)
                                                {
                                                    GetResultObj.keyValue[l] = 0;
                                                }
                                                showKeyValue(0, true);
                                            }

                                            break;


                                        case (byte)Command.TapCard:
                                            //GetResultObj.tapCard = validFrame[4];
                                            GetResultObj.cardNum = Encoding.ASCII.GetString(validFrame, 17, 16).ToUpper();
                                            GetResultObj.cardNum = GetResultObj.cardNum.Remove(GetResultObj.cardNum.IndexOf('\0'));
                                            updateText("卡号:" + GetResultObj.cardNum + "\r\n");

                                            if (mainBoardTestingflag)
                                            {

                                                //加入卡号判断
                                                if (TestSettingInfo["CardNum"].ToString() == GetResultObj.cardNum)
                                                {
                                                    mainboardTestData["刷卡"] = "通过";
                                                    mainboardTestData["卡号"] = GetResultObj.cardNum;
                                                    updateControlText(skinLabelTapCardResult, "通过\r\n卡号:" + GetResultObj.cardNum.ToUpper(), Color.Green);
                                                    if (getPresentTabPage(skinTabControlPCBAMainBoardTest) == skinTabPageTapCard)
                                                    {
                                                        updateTableSelectedIndex(skinTabControlPCBAMainBoardTest, ++mainBoardTabSelectIndex);
                                                    }
                                                }
                                                else
                                                {
                                                    mainboardTestData["刷卡"] = "不通过";
                                                    mainboardTestData["卡号"] = GetResultObj.cardNum;
                                                    updateControlText(skinLabelTapCardResult, "不通过\r\n当前卡号:" + GetResultObj.cardNum + "\r\n正确卡号:" + TestSettingInfo["CardNum"] + "\r\n", Color.Red);
                                                }



                                            }
                                            else if (interfaceBoardTestingflag)
                                            {

                                                if (TestSettingInfo["CardNum"].ToString() == GetResultObj.cardNum)
                                                {
                                                    updateControlText(skinLabelInterfaceTapCardResult, "通过\r\n卡号:" + GetResultObj.cardNum, Color.Green);
                                                    //加入卡号判断
                                                    interfaceboardTestData["刷卡"] = "通过";
                                                    interfaceboardTestData["卡号"] = GetResultObj.cardNum;
                                                    if (getPresentTabPage(skinTabControlInterfaceTest) == skinTabPageTapCardTest)
                                                    {
                                                        updateTableSelectedIndex(skinTabControlInterfaceTest, ++interfaceTabSelectIndex);
                                                    }
                                                }
                                                else
                                                {
                                                    updateControlText(skinLabelInterfaceTapCardResult, "不通过\r\n当前卡号:" + GetResultObj.cardNum + "\r\n正确卡号:" + TestSettingInfo["CardNum"], Color.Red);
                                                    //加入卡号判断
                                                    interfaceboardTestData["刷卡"] = "通过";
                                                    interfaceboardTestData["卡号"] = GetResultObj.cardNum;
                                                }

                                            }
                                            else if (chargerTestingflag)
                                            {
                                                if (TestSettingInfo["CardNum"].ToString() == GetResultObj.cardNum)
                                                {
                                                    updateControlText(skinLabelChargerTapCardResult, "通过\r\n卡号:" + GetResultObj.cardNum, Color.Green);
                                                    //加入卡号判断
                                                    chargerTestData["刷卡"] = "通过";
                                                    chargerTestData["卡号"] = GetResultObj.cardNum;
                                                    if (getPresentTabPage(skinTabControlChargerTest) == skinTabPageChargerTapCard)
                                                    {
                                                        updateTableSelectedIndex(skinTabControlChargerTest, ++chargerTestSelectIndex);
                                                    }
                                                }
                                                else
                                                {
                                                    updateControlText(skinLabelChargerTapCardResult, "不通过\r\n当前卡号:" + GetResultObj.cardNum + "\r\n正确卡号:" + TestSettingInfo["CardNum"], Color.Red);
                                                    //加入卡号判断
                                                    chargerTestData["刷卡"] = "通过";
                                                    chargerTestData["卡号"] = GetResultObj.cardNum;
                                                }
                                            }

                                            break;



                                        case (byte)Command.LCD:
                                            GetResultObj.lcd = validFrame[17];
                                            break;


                                        case (byte)Command.Sim2G:
                                            GetResultObj._2G = validFrame[17];
                                            GetResultObj._2gCSQ = validFrame[18];
                                        
                                            GetResultObj._2G_Iccid =Encoding.ASCII.GetString(validFrame, 19, 20);

                                            if(GetResultObj._2G_Iccid.IndexOf('\0')>=0)
                                            {
                                                GetResultObj._2G_Iccid = GetResultObj._2G_Iccid.Remove(GetResultObj._2G_Iccid.IndexOf('\0'));
                                            }

                                            if (GetResultObj._2G == 0x00)
                                            {
                                                //加入信号值判断
                                                if ((GetResultObj._2gCSQ >= Convert.ToByte(TestSettingInfo["CsqLowerLimit"]) && GetResultObj._2gCSQ <= Convert.ToByte(TestSettingInfo["CsqUpperLimit"]))&&(GetResultObj._2G_Iccid!=null))
                                                {
                                                    if (mainBoardTestingflag)
                                                    {
                                                        updateControlText(skinLabel2GResult, "通过\r\n信号值:" + GetResultObj._2gCSQ, Color.Green);
                                                        mainboardTestData["2G模块"] = "通过";
                                                        mainboardTestData["信号值"] = GetResultObj._2gCSQ.ToString();
                                                        if (getPresentTabPage(skinTabControlPCBAMainBoardTest) == skinTabPage2GModule)
                                                        {
                                                            updateTableSelectedIndex(skinTabControlPCBAMainBoardTest, ++mainBoardTabSelectIndex);
                                                        }
                                                    }
                                                    else if (chargerTestingflag)
                                                    {
                                                        updateControlText(skinLabelCharger2GResult, "通过\r\n信号值:" + GetResultObj._2gCSQ+"\r\nICCID:"+ GetResultObj._2G_Iccid, Color.Green);

                                                        chargerTestData["2G模块"] = "通过";
                                                        chargerTestData["信号值"] = GetResultObj._2gCSQ.ToString();
                                                        chargerTestData["ICCID"] = GetResultObj._2G_Iccid;
                                                        if (getPresentTabPage(skinTabControlChargerTest) == skinTabPageCharger2GModule)
                                                        {
                                                            updateTableSelectedIndex(skinTabControlChargerTest, ++chargerTestSelectIndex);
                                                        }
                                                    }
                                                  
                                                }
                                                else
                                                {
                                                    if (mainboardTestRunningFlag)
                                                    {
                                                        updateControlText(skinLabel2GResult, "不通过\r\n信号值:" + GetResultObj._2gCSQ, Color.Red);
                                                        mainboardTestData["2G模块"] = "不通过";
                                                        mainboardTestData["信号值"] = GetResultObj._2gCSQ.ToString();
                                                    }
                                                    else if (chargerTestingflag)
                                                    {
                                                        updateControlText(skinLabelCharger2GResult, "不通过\r\n信号值:" + GetResultObj._2gCSQ + "\r\nICCID:" + GetResultObj._2G_Iccid, Color.Red);
                                                        chargerTestData["2G模块"] = "不通过";
                                                        chargerTestData["信号值"] = GetResultObj._2gCSQ.ToString();
                                                        chargerTestData["ICCID"] = "00000000000";
                                                    }

                                                }

                                            }
                                            else
                                            {
                                                if (mainboardTestRunningFlag)
                                                {
                                                    mainboardTestData["2G模块"] = "不通过";
                                                    updateControlText(skinLabel2GResult, "不通过", Color.Red);
                                                }
                                                else
                                                {
                                                    chargerTestData["2G模块"] = "不通过";
                                                    updateControlText(skinLabelCharger2GResult, "不通过",Color.Red);                                                   
                                                }
                                            }
                                            break;


                                        case (byte)Command.Trumpet:
                                            GetResultObj.trumpet = validFrame[17];
                                            break;


                                        case (byte)Command.Relay:
                                            GetResultObj.relay = validFrame[17];
                                            if ((mainBoardTabSelectIndex != 3 && chargerTestSelectIndex != 5))
                                            {
                                                break;
                                            }
                                            string resultStr = "";
                                            string power1 = "";
                                            string power2 = "";
                                            int operate = validFrame[17];
                                            byte ch = validFrame[18];
                                            int power_1 = ((validFrame[19] << 8) | (validFrame[20])) / 10;
                                            int power_2 = ((validFrame[21] << 8) | +(validFrame[22])) / 10;
                                            int power_3 = ((validFrame[23] << 8) + (validFrame[24])) / 10;

                                            if (operate == 0x00)
                                            {

                                                updateText("第" + (ch + 1) + "," + (ch + 2) + "," + (ch + 3) + "路继电器开");
                                                GetResultObj.getPower[ch] = power_1;
                                                GetResultObj.getPower[ch + 1] = power_2;
                                                GetResultObj.getPower[ch + 2] = power_3;

                                                for (int j = 0; j < 12; j++)
                                                {
                                                    if (j < 6)
                                                    {
                                                        power1 += "第" + (j + 1) + "路功率:" + GetResultObj.getPower[j] + " W\r\n";
                                                    }
                                                    else
                                                    {
                                                        power2 += "第" + (j + 1) + "路功率:" + GetResultObj.getPower[j] + " W\r\n";
                                                    }
                                                }
                                                updateControlText(skinLabelRelayResult, "第" + (ch + 1) + "," + (ch + 2) + "," + (ch + 3) + "路测试中，请等待", Color.Black);
                                                updateControlText(skinLabelChargeResult, "第" + (ch + 1) + "," + (ch + 2) + "," + (ch + 3) + "路测试中，请等待", Color.Black);

                                                updateControlText(skinLabelRelayResult1, power1);
                                                updateControlText(skinLabelRelayResult2, power2);

                                                updateControlText(skinLabelChargeResult1, power1);
                                                updateControlText(skinLabelChargeResult2, power2);

                                                SendRelay(0x01, ch);

                                            }
                                            else if (operate == 0x01)
                                            {

                                                updateText("第" + (ch + 1) + "," + (ch + 2) + "," + (ch + 3) + "路继电器关 ");
                                                ZeroPower[ch] = power_1;
                                                ZeroPower[ch + 1] = power_2;
                                                ZeroPower[ch + 2] = power_3;
                                                if (ch < 9 && startTestting == true && (getPresentTabPage(skinTabControlPCBAMainBoardTest) == skinTabPageRelay || getPresentTabPage(skinTabControlChargerTest) == skinTabPageCharge))
                                                {
                                                    SendRelay(0x00, (byte)(ch + 3));
                                                }
                                                else if (ch >= 9)
                                                {
                                                    bool pass = true;
                                                    resultStr = "第";
                                                    for (int j = 0; j < 12; j++)
                                                    {
                                                        if (GetResultObj.getPower[j] < Convert.ToInt32(TestSettingInfo["PowerLowerLimit"]) || GetResultObj.getPower[j] > Convert.ToInt32(TestSettingInfo["PowerUpperLimit"]))
                                                        {
                                                            resultStr += (j + 1) + " ";
                                                            pass = false;
                                                        }
                                                    }
                                                    resultStr += "路功率不正常";

                                                    if (pass)
                                                    {
                                                        resultStr = "测试通过";
                                                        if (mainBoardTestingflag) mainboardTestData["功率"] = "通过";
                                                        else if (chargerTestingflag) chargerTestData["功率"] = "通过";

                                                        updateControlText(skinLabelRelayResult, resultStr, Color.Green);
                                                        if (getPresentTabPage(skinTabControlPCBAMainBoardTest) == skinTabPageRelay)
                                                        {
                                                            updateTableSelectedIndex(skinTabControlPCBAMainBoardTest, ++mainBoardTabSelectIndex);
                                                        }

                                                        updateControlText(skinLabelChargeResult, resultStr, Color.Green);
                                                        if (getPresentTabPage(skinTabControlChargerTest) == skinTabPageCharge)
                                                        {
                                                            updateTableSelectedIndex(skinTabControlChargerTest, ++chargerTestSelectIndex);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (mainBoardTestingflag) mainboardTestData["功率"] = "不通过";
                                                        else if (chargerTestingflag) chargerTestData["功率"] = "不通过";
                                                        updateControlText(skinLabelRelayResult, resultStr, Color.Red);
                                                        updateControlText(skinLabelChargeResult, resultStr, Color.Red);
                                                    }
                                                }


                                            }

                                            break;


                                        case (byte)Command.SetPcbCode:
                                            GetResultObj.SetPcbCode = validFrame[18];
                                            if (validFrame[17] == 0x00)
                                            {
                                                if (GetResultObj.SetPcbCode == 0x00)
                                                {

                                                    updateText("主板编码设置成功");
                                                }
                                                else
                                                {

                                                    updateText("主板编码设置失败");
                                                }
                                            }
                                            else if (validFrame[17] == 0x01)
                                            {
                                                if (GetResultObj.SetPcbCode == 0x00)
                                                {

                                                    updateText("按键板编码设置成功");
                                                }
                                                else
                                                {

                                                    updateText("按键板编码设置失败");
                                                }
                                            }

                                            break;


                                        case (byte)Command.SetCID:
                                            GetResultObj.SetCID = validFrame[17];

                                            break;

                                        case (byte)Command.BLE:
                                            GetResultObj.BLE = validFrame[17];
                                            if (interfaceBoardTestingflag)
                                            {
                                                switch (GetResultObj.BLE)
                                                {
                                                    case 0x01:
                                                        interfaceboardTestData["蓝牙"] = "通过";
                                                        updateControlText(skinLabelInterfaceBleResult, "测试通过", Color.Green);
                                                        if (getPresentTabPage(skinTabControlInterfaceTest) == skinTabPageBleTest)
                                                        {
                                                            updateTableSelectedIndex(skinTabControlInterfaceTest, ++interfaceTabSelectIndex);
                                                        }
                                                        break;
                                                    case 0x00:
                                                        updateControlText(skinLabelInterfaceBleResult, "测试不通过", Color.Red);
                                                        interfaceboardTestData["蓝牙"] = "不通过";
                                                        break;

                                                    case 0x02:
                                                        interfaceboardTestData["蓝牙"] = "无";
                                                        updateControlText(skinLabelInterfaceBleResult, "此PCB不带蓝牙模块", Color.Black);
                                                        if (getPresentTabPage(skinTabControlInterfaceTest) == skinTabPageBleTest)
                                                        {
                                                            updateTableSelectedIndex(skinTabControlInterfaceTest, ++interfaceTabSelectIndex);
                                                        }
                                                        break;
                                                        //updateControlText(skinLabelInterfaceBleResult, "测试不通过", Color.Red);
                                                        //interfaceboardTestData["蓝牙"] = "不通过";
                                                        //break;
                                                }
                                            }
                                            else if (chargerTestingflag)
                                            {
                                                switch (GetResultObj.BLE)
                                                {
                                                    case 0x01:
                                                        chargerTestData["蓝牙"] = "通过";
                                                        updateControlText(skinLabelChargerBleResult, "测试通过", Color.Green);
                                                        if (getPresentTabPage(skinTabControlChargerTest) == skinTabPageChargerBLE)
                                                        {
                                                            updateTableSelectedIndex(skinTabControlChargerTest, ++chargerTestSelectIndex);
                                                        }
                                                        break;

                                                    case 0x00:
                                                        updateControlText(skinLabelChargerBleResult, "测试不通过", Color.Red);
                                                        chargerTestData["蓝牙"] = "不通过";
                                                        break;

                                                    case 0x02:
                                                        chargerTestData["蓝牙"] = "无";
                                                        updateControlText(skinLabelChargerBleResult, "此PCB不带蓝牙模块", Color.Black);
                                                        if (getPresentTabPage(skinTabControlChargerTest) == skinTabPageChargerBLE)
                                                        {
                                                            updateTableSelectedIndex(skinTabControlChargerTest, ++chargerTestSelectIndex);
                                                        }
                                                        break;
                                                        //updateControlText(skinLabelChargerBleResult, "测试不通过", Color.Red);
                                                        //chargerTestData["蓝牙"] = "不通过";
                                                       // break;
                                                }
                                            }

                                            break;

                                        case (byte)Command.FwVersion:
                                            if (validFrame[17] == 0x00)
                                            {
                                                int fwVer = (int)((validFrame[18] << 8) | (validFrame[19]));
                                                int subver = validFrame[20];
                                                GetResultObj.FwVersion = fwVer + "." + subver;
                                            }
                                            else if(validFrame[17] ==0x01)
                                            {
                                                GetResultObj.FwVersion = validFrame[18].ToString("D3");
                                            }                                         
                                           
                                            if (mainBoardTestingflag) mainboardTestData["软件版本"] = GetResultObj.FwVersion;
                                            else if (interfaceBoardTestingflag) interfaceboardTestData["软件版本"] = GetResultObj.FwVersion;
                                            else if (chargerTestingflag)
                                            {
                                                if (validFrame[17] == 0)
                                                {
                                                    chargerTestData["软件版本"] = GetResultObj.FwVersion;
                                                }
                                                else
                                                {
                                                    chargerTestData["按键板软件版本"] = GetResultObj.FwVersion;
                                                }
                                             
                                            }
                                            updateText("软件版本"+GetResultObj.FwVersion);
                                            break;

                                        case (byte)Command.GetPcbCode:
                                            string str = "";
                                            bool isZero = true;
                                            for (int m = 0; m < 8; m++)
                                            {    
                                                if (validFrame[18 + m] != 0x00 && isZero == true)
                                                {
                                                    isZero = false;
                                                }

                                                if(isZero == false)
                                                {
                                                    str += validFrame[18 + m].ToString("X2");
                                                }
                                            
                                            }
                                            if (validFrame[17] == 0)
                                            {
                                                
                                                GetResultObj.MainBoardCode = str;
                                                chargerTestData["主板编号"] = GetResultObj.MainBoardCode;
                                            }
                                            else if (validFrame[17] == 1)
                                            {
                                                GetResultObj.InterfaceBoardCode = str; 
                                                chargerTestData["按键板编号"] = GetResultObj.InterfaceBoardCode;
                                            }
                                            break;

                                        case (byte)Command.SetRegisterCode:
                                            GetResultObj.SetRegisterCode = validFrame[17];
                                            break;


                                        case (byte)Command.SetDevType:
                                            if (validFrame[17] == 0x00)
                                            {
                                                //设置成功     
                                                updateText("设备类型设置成功");
                                            }
                                            else
                                            {
                                                //设置失败
                                                updateText("设备类型设置失败");
                                            }
                                            break;

                                        case (byte)Command.Set2_4G_Gw_addr:
                                            if (validFrame[17] == 0x00)
                                            {
                                                //设置成功     
                                                updateText("设置2.4G网关地址成功");
                                            }
                                            else
                                            {
                                                //设置失败
                                                updateText("设置2.4G网关地址失败");
                                            }
                                            break;

                                        case (byte)Command.SetTerminalInfo:
                                            if (validFrame[17] == 0x00)
                                            {
                                                //设置成功     
                                                updateText("设置终端信息成功");
                                            }
                                            else
                                            {
                                                //设置失败
                                                updateText("设置终端信息失败");
                                            }
                                            break;

                                        case (byte)Command.SetServerAddr:
                                            if (validFrame[17] == 0x00)
                                            {
                                                //设置成功     
                                                updateText("设置服务器地址成功");
                                            }
                                            else
                                            {
                                                //设置失败
                                                updateText("设置服务器地址失败");
                                            }
                                            break;

                                        case (byte)Command.SetServerPort:
                                            if (validFrame[17] == 0x00)
                                            {
                                                //设置成功     
                                                updateText("设置服务器端口成功");
                                            }
                                            else
                                            {
                                                //设置失败
                                                updateText("设置服务器端口失败");
                                            }
                                            break;

                                        case (byte)Command.SetPrintSwitch:
                                            if (validFrame[17] == 0x00)
                                            {
                                                //设置成功     
                                                updateText("设置日志打印开关成功");
                                            }
                                            else
                                            {
                                                updateText("设置日志打印开关失败");
                                            }
                                            break;

                                        case (byte)Command.Reboot:
                                            if (validFrame[17] == 0x00)
                                            {
                                                //设置成功     
                                                updateText("设备重启成功");
                                            }
                                            else
                                            {
                                                updateText("设备重启失败");
                                            }
                                            break;

                                        case (byte)Command.SetUUID:
                                            if (validFrame[17] == 0x00)
                                            {
                                                //设置成功     
                                                updateText("设置识别码成功");
                                            }
                                            else
                                            {
                                                updateText("设置识别码失败");
                                            }
                                            break;


                                        default:
                                            updateText("不支持此命令:" + frameCmd.ToString("X2"));
                                            break;


                                    }
                                    //arraybuffer.RemoveRange(startindex, endindex);
                                    //i += (endindex - startindex)-1;
                                    
                                }
                                else
                                {
                                    updateText("CRC Error");
                                }

                            }
                            else
                            {
                               // updateText("Length Error");
                            }
                        }
                    }
                }
                if (arraybuffer.Count>3096)
                {
                    arraybuffer.Clear();
                }
            }
            catch (Exception ex)
            {
                updateText(ex.Message);
            }
        }

     

        Thread mainboardTestThread;
        Thread interfaceboardTestThread;
        static int mainBoardTabSelectIndex;
        static int interfaceTabSelectIndex;
        static int chargerTestSelectIndex;

        public static UInt32 GetCurrentTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToUInt32(ts.TotalSeconds);
        }


        private void skinTabControlPCBATest_SelectedIndexChanged(object sender, EventArgs e)
        {
            mainBoardTabSelectIndex = skinTabControlPCBAMainBoardTest.SelectedIndex;
            if (mainBoardTabSelectIndex == 0)
            {
                textBoxMainboardCode.Focus();
            }

            if (mainboardTestThread != null)
            {
                mainboardTestThread.Abort();
                mainboardTestThread = null;
            }

            mainboardTestThread = new Thread(mainboardTestProcess);
            mainboardTestThread.Start();
        }

        bool interfaceboardTestRunningFlag;
        bool mainboardTestRunningFlag;
        bool chargerTestRunningFlag;

        bool interfaceBoardTestingflag = false;
        bool mainBoardTestingflag = false;
        bool chargerTestingflag = false;

        private void chargerTestProcess()
        {
            chargerTestRunningFlag = true;
            int countdonwTime = Convert.ToInt32(TestSettingInfo["CountDown"]);
            if (startTestting)
            {
                resetCountDownTime(out countDownTimeCharger);
                switch (chargerTestSelectIndex)
                {
                    case 0x00:
                        //textBoxChargerQrCode.Focus();
                        updateText("Scan QR Code");
                        break;

                    case 0x01:
                        SendTapCard();
                        updateControlText(skinLabelChargerTapCardResult, "");
                        countDownTimeCharger.tapCard = countdonwTime;
                       
                        break;

                    case 0x02:
                        SendKeyTest();
                        updateControlText(skinLabelChargerKeyResult, "");
                        countDownTimeCharger.key = countdonwTime;
                        break;

                    case 0x03:
                        SendLCD();
                        updateControlText(skinLabelChargerLcdResult,"");
                        countDownTimeCharger.lcd = countdonwTime;
                        break;

                    case 0x04:
                        SendTrumpet();
                        updateControlText(skinLabelChargerTrumpetResult, "");
                        countDownTimeCharger.trumpet = countdonwTime;
                        break;

                    case 0x05:
                        //updateControlText(skinLabelChargeResult, "");
                        updateControlText(skinLabelChargeResult1, "");
                        updateControlText(skinLabelChargeResult2, "");
                        DataProgram.setArrayValue(GetResultObj.getPower, 0, 0, 12);
                   
                        updateControlText(skinLabelChargeResult, "第1,2,3 路测试中，请等待...", Color.Black);
                   
                        countDownTimeCharger.relay = countdonwTime + 20 ;
                        SendRelay(0x00, 0x00);
                        break;

                    case 0x06:
                        updateControlText(skinLabelCharger2GResult,"");
                        countDownTimeCharger._2G = countdonwTime;
                        Send2GModule();
                        break;

                    case 0x07:
                        updateControlText(skinLabelChargerBleResult, "");
                        countDownTimeCharger.BLE = countdonwTime;
                        SendBleTest();
                        break;

                    case 0x08:
                        ShowChargerTestResult(false);
                        //lock (this)
                        
                        SendFwVersion(0);
                        Thread.Sleep(10);
                        SendFwVersion(1);
                        Thread.Sleep(10);
                        SendGetPcdCode(0);
                        Thread.Sleep(10);
                        SendGetPcdCode(1);
                        Thread.Sleep(10);




                        GetResultObj.UsedTime_Charger = GetCurrentTimeStamp() - GetResultObj.UsedTime_Charger;
                        chargerTestData["测试用时"] = (GetResultObj.UsedTime_Charger / 60) + "分 " + ((GetResultObj.UsedTime_Charger) % 60) + "秒";

                        chargerTestData = ModifyResultData(chargerTestData);
                        updateText("测试结束\r\n测试用时:" + chargerTestData["测试用时"]);
                        //ShowInterfaceboardResult();
                        ShowChargerTestResult(true);
                        //Thread.Sleep(100);
                      

                        if (chargerTestData["测试结果"]=="通过")
                        {
                            if(Convert.ToBoolean( TestSettingInfo["AllFuncAgingTest"])==false)
                            {
                                
                                 SendSetID(textBoxChargerQrCode.Text.Trim());
                                
                               
                            }
                            
                        }
                        //lock (this)
                        {
                            SendTestMode(0x01);
                        }
                        
                        string cmd;
                        if (Convert.ToBoolean(TestSettingInfo["AllFuncAgingTest"]) == false)
                        {
                            DataProgram.WriteReport(TestSettingInfo["ChargerModel"] + "_整机测试.xlsx", TestSettingInfo["ChargerModel"] + "_整机测试", chargerTestData);


                            cmd = DataProgram.ChargerTestMysqlCommand(TestSettingInfo["ChargerModel"].ToString(), chargerTestData["电桩号"], chargerTestData["测试员"], chargerTestData["软件版本"], chargerTestData["软件版本"], chargerTestData["按键板软件版本"],
                            chargerTestData["主板编号"], chargerTestData["按键板编号"], chargerTestData["测试结果"] == "通过" ? "Pass" : "Fail", chargerTestData["刷卡"] == "通过" ? "Pass" : "Fail", chargerTestData["卡号"], chargerTestData["LCD"] == "通过" ? "Pass" : "Fail",
                            chargerTestData["喇叭"] == "通过" ? "Pass" : "Fail", chargerTestData["功率"] == "通过" ? "Pass" : "Fail", chargerTestData["2G模块"]=="通过"?"Pass" : "Fail", chargerTestData["信号值"], chargerTestData["ICCID"], chargerTestData["蓝牙"]=="通过"? "Pass" : (chargerTestData["蓝牙"]=="无"?"Without":"Fail"),
                            chargerTestData["测试时间"], GetResultObj.UsedTime_Charger);
                        }
                        else
                        {
                            DataProgram.WriteReport(TestSettingInfo["ChargerModel"] + "_整机老化测试.xlsx", TestSettingInfo["ChargerModel"] + "_整机老化测试", chargerTestData);

                            cmd = DataProgram.ChargerAgingTestMysqlCommand(TestSettingInfo["ChargerModel"].ToString(), chargerTestData["电桩号"], chargerTestData["测试员"], chargerTestData["软件版本"], chargerTestData["软件版本"], chargerTestData["按键板软件版本"],
                            chargerTestData["主板编号"], chargerTestData["按键板编号"], chargerTestData["测试结果"] == "通过" ? "Pass" : "Fail", chargerTestData["刷卡"] == "通过" ? "Pass" : "Fail", chargerTestData["卡号"], chargerTestData["LCD"] == "通过" ? "Pass" : "Fail",
                            chargerTestData["喇叭"] == "通过" ? "Pass" : "Fail", chargerTestData["功率"] == "通过" ? "Pass" : "Fail", chargerTestData["2G模块"] == "通过" ? "Pass" : "Fail", chargerTestData["信号值"], chargerTestData["ICCID"], chargerTestData["蓝牙"] == "通过" ? "Pass" : (chargerTestData["蓝牙"] == "无" ? "Without" : "Fail"),
                            chargerTestData["测试时间"], GetResultObj.UsedTime_Charger);
                        }
                    
                        if(DataProgram.SendMysqlCommand(cmd,true)==true)
                        {
                            DataProgram.DealBackUpData(DataProgram.backupMysqlCmdFile);
                        }
                        
                        updateControlText(textBoxChargerQrCode, "");
                        break;

                    default:
                        break;

                }
            }



            chargerTestRunningFlag = false;
        }
        private void interfaceBoardTestProcess()
        {
            interfaceboardTestRunningFlag = true;
            int countdonwTime = Convert.ToInt32(TestSettingInfo["CountDown"]);
            if (startTestting == true)
            {
                resetCountDownTime(out countDownTimeInterfaceBorad);
                switch (interfaceTabSelectIndex)
                {
                    case 0x00:
                        //textBoxInterfaceCode.Focus();
                        updateText("Scan QR Code");

                        break;

                    case 0x01:
                        updateControlText(skinLabelInterfaceTapCardResult, "");
                        countDownTimeInterfaceBorad.tapCard = countdonwTime;
                        SendTapCard();
                        break;

                    case 0x03:
                        updateControlText(skinLabelInterfaceKeyResult, "");
                        countDownTimeInterfaceBorad.key = countdonwTime;
                        SendKeyTest();
                        break;

                    case 0x02:
                        updateControlText(skinLabelInterfaceBleResult, "");
                        countDownTimeInterfaceBorad.BLE = countdonwTime;
                        SendBleTest();
                        break;

                    case 0x04:
                        SendFwVersion(1);
                        Thread.Sleep(100);
                        SendSetPcbCode(0x01, textBoxInterfaceCode.Text.Trim());
                        Thread.Sleep(100);
                        GetResultObj.UsedTime_interface = GetCurrentTimeStamp() - GetResultObj.UsedTime_interface;
                        interfaceboardTestData["测试用时"] = (GetResultObj.UsedTime_interface / 60) + "分 " + ((GetResultObj.UsedTime_interface) % 60) + "秒";

                        interfaceboardTestData = ModifyResultData(interfaceboardTestData);
                        updateText("测试结束\r\n测试用时:" + interfaceboardTestData["测试用时"]);
                        ShowInterfaceboardResult();
                        DataProgram. WriteReport(TestSettingInfo["ChargerModel"]+"_PCBA_按键板.xlsx", TestSettingInfo["ChargerModel"]+"_PCBA_按键板", interfaceboardTestData);

                        SendTestMode(0x01);

                        string mysqlCmd = DataProgram.InterfaceboardTestMysqlCommand(TestSettingInfo["ChargerModel"].ToString(), textBoxInterfaceCode.Text.Trim(), interfaceboardTestData["测试员"], interfaceboardTestData["软件版本"],
                            interfaceboardTestData["测试结果"] == "通过" ? "Pass" : "Fail", interfaceboardTestData["刷卡"] == "通过" ? "Pass" : "Fail", interfaceboardTestData["卡号"], interfaceboardTestData["按键"] == "通过" ? "Pass" : "Fail" ,
                            interfaceboardTestData["蓝牙"] == "通过" ? "Pass" : (interfaceboardTestData["蓝牙"] == "无"?"Without":"Fail"), interfaceboardTestData["测试时间"], GetResultObj.UsedTime_interface);

                        if (DataProgram.SendMysqlCommand(mysqlCmd,true) == true)
                        {
                            DataProgram.DealBackUpData(DataProgram.backupMysqlCmdFile);
                        }
                        updateControlText(textBoxInterfaceCode,"");
                      
                        break;

                    default:
                        break;
                }
            }
            interfaceboardTestRunningFlag = false;
        }

        private void mainboardTestProcess()
        {
            mainboardTestRunningFlag = true;
            int countdownTime = Convert.ToInt32( TestSettingInfo["CountDown"]);
            if (startTestting == true)
            {
                resetCountDownTime(out countDownTimeMainBorad);
                switch (mainBoardTabSelectIndex)
                {
                    case 0x00:
                        //textBoxMainboardCode.Focus();

                        updateText("Scan QR Code");

                        break;

                    case 0x01:
                        updateControlText(skinLabelPwrResult, "");
                        countDownTimeMainBorad.PowerSource = countdownTime;
                        updateText("检测电源是否正常");
                        break;

                    case 0x02:
                        updateControlText(skinLabelTapCardResult,"");
                        countDownTimeMainBorad.tapCard = countdownTime;
                        SendTapCard();
                        break;

                    case 0x03:
                         updateControlText(skinLabelRelayResult, "");
                        updateControlText(skinLabelRelayResult1, "");
                        updateControlText(skinLabelRelayResult2, "");
                        DataProgram.setArrayValue(GetResultObj.getPower,0,0,12);
                        countDownTimeMainBorad.relay = countdownTime+30;
                        updateControlText(skinLabelRelayResult, "第1,2,3 路测试中，请等待...",Color.Black);
                        SendRelay(0x00,0x00);
                        break;
                    case 0x04:
                        updateControlText(skinLabelKeyResult, "");
                        countDownTimeMainBorad.key = countdownTime;
                        SendKeyTest();
                        break;

                    case 0x05:
                      
                        updateControlText(skinLabel2GResult, "");
                        countDownTimeMainBorad._2G = countdownTime;
                        Send2GModule();
                        break;
                    case 0x06:
                        updateControlText(skinLabelLcdResult, "");
                        countDownTimeMainBorad.lcd = countdownTime;
                        SendLCD();
                        break;

                    case 0x07:
                        updateControlText(skinLabelTrumpetResult, "");
                        countDownTimeMainBorad.trumpet = countdownTime;
                        SendTrumpet();
                        break;
                  

                    case 0x08:
                        SendFwVersion(0);
                        Thread.Sleep(100);
                        SendSetPcbCode(0x00, textBoxMainboardCode.Text.Trim());
                        GetResultObj.UsedTime_main = GetCurrentTimeStamp() - GetResultObj.UsedTime_main;
                        mainboardTestData["测试用时"] = (GetResultObj.UsedTime_main / 60) + "分 " + ((GetResultObj.UsedTime_main) % 60) + "秒";
                        mainboardTestData = ModifyResultData(mainboardTestData);
                        updateText("结束测试\r\n用时:" + mainboardTestData["测试用时"]);
                     
                     
                        ShowMainboardResult();
                        DataProgram.WriteReport(TestSettingInfo["ChargerModel"]+"_PCBA_主板.xlsx", TestSettingInfo["ChargerModel"] +"_PCBA_主板", mainboardTestData);
                        SendTestMode(0x01);


                        string mysqlCmd = DataProgram.MainboardTestMysqlCommand(TestSettingInfo["ChargerModel"].ToString(),textBoxMainboardCode.Text.Trim(),mainboardTestData["测试员"], mainboardTestData["软件版本"],
                            mainboardTestData["测试结果"] == "通过" ? "Pass" : "Fail", mainboardTestData["刷卡"] == "通过" ? "Pass" : "Fail", mainboardTestData["卡号"], mainboardTestData["2G模块"] == "通过" ? "Pass" : "Fail", Convert.ToByte(mainboardTestData["信号值"]), 
                            mainboardTestData["功率"] == "通过" ? "Pass" : "Fail", mainboardTestData["按键"] == "通过" ? "Pass" : "Fail", mainboardTestData["LCD"] == "通过" ? "Pass" : "Fail", mainboardTestData["喇叭"] == "通过" ? "Pass" : "Fail", mainboardTestData["电源"] == "通过" ? "Pass" : "Fail", mainboardTestData["测试时间"], GetResultObj.UsedTime_main);

                        if (DataProgram.SendMysqlCommand(mysqlCmd,true) == true)
                        {
                            DataProgram.DealBackUpData(DataProgram.backupMysqlCmdFile);
                        }
                        updateControlText(textBoxMainboardCode, "");
                     
                        //skinButtonStandraTest_Click(sender,e);
                        break;
                }
            }
            mainboardTestRunningFlag = false;
        }

        private Dictionary<string, string> ModifyResultData(Dictionary<string, string> inputDic)
        {


            string resKey = "", resValue = "";


            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            foreach (var item in inputDic)
            {
                dictionary.Add(item.Key, item.Value);
            }

            foreach (var item in dictionary)
            {
                if (item.Value == "" && item.Key != "测试结果")
                {
                    inputDic[item.Key] = "未测试";
                }
                else if (item.Value == "不通过" && item.Value != "无")
                {
                    inputDic["测试结果"] = "不通过";
                }

                if (inputDic[item.Key] == "未测试")
                {
                    inputDic["测试结果"] = "不通过";
                }
            }
            if (inputDic["测试结果"] == "")
            {
                inputDic["测试结果"] = "通过";
            }

            foreach (var item in inputDic)
            {
                resKey += item.Key + " :\r\n";
                resValue += item.Value + "\r\n";
            }
            //updateControlText(key, resKey);
            //updateControlText(value, resValue);

            return inputDic;
            //string resKey = "", resValue = "";


            //Dictionary<string, string> dictionary = new Dictionary<string, string>();

            //foreach (var item in mainboardTestData)
            //{
            //    dictionary.Add(item.Key, item.Value);
            //}

            //foreach (var item in dictionary)
            //{
            //    if (item.Value == "" && item.Key != "测试结果")
            //    {
            //        mainboardTestData[item.Key] = "未测试";
            //    }
            //    else if (item.Value == "不通过")
            //    {
            //        mainboardTestData["测试结果"] = "不通过";
            //    }
            //}

            //foreach (var item in mainboardTestData)
            //{
            //    resKey += item.Key + " :\r\n";
            //    resValue += item.Value + "\r\n";
            //}
            //updateControlText(skinLabelMainboardResultItem, resKey);
            //updateControlText(skinLabelMainboardResultValue, resValue);
        }

        private Color decideColor(string text)
        {
            
            switch (text)
            {
                case "通过":
                    return Color.Green;                    

                case "不通过":
                    return Color.Red;               

                default:
                    return Color.Black;
           
            }
        }

        private void ShowMainboardResult()
        {
            updateControlText(lbMianPcbCode, "PCB编号：", Color.Black);
            updateControlText(lbMainTestor, "测试员：", Color.Black);
            updateControlText(lbMianfwVer, "软件版本：",Color.Black);
            updateControlText(lbMianAllResult, "测试结果：",decideColor(mainboardTestData["测试结果"]));          
            updateControlText(lbMianTapCard, "刷卡：", decideColor(mainboardTestData["刷卡"]));
            updateControlText(lbMian2G, "2G模块：", decideColor(mainboardTestData["2G模块"]));
            updateControlText(lbMianRelay, "功率：", decideColor(mainboardTestData["功率"]));
            updateControlText(lbMianKey, "按键：", decideColor(mainboardTestData["按键"]));
            updateControlText(lbMianLcd, "LCD：", decideColor(mainboardTestData["LCD"]));
            updateControlText(lbMianTrumpet, "喇叭：", decideColor(mainboardTestData["喇叭"]));
            updateControlText(lbMianPowerSource, "电源：", decideColor(mainboardTestData["电源"]));
            updateControlText(lbMainTestTime, "测试时间：", Color.Black);
            updateControlText(lbMainUsedTime, "测试用时：", Color.Black);

            updateControlText(lbMainPcbCodeValue, mainboardTestData["PCB编号"], Color.Black);
            updateControlText(lbMainTestorValue, mainboardTestData["测试员"], Color.Black);
            updateControlText(lbMainfwVer, mainboardTestData["软件版本"], Color.Black);
            updateControlText(lbMainResultValue, mainboardTestData["测试结果"], decideColor(mainboardTestData["测试结果"]));
            updateControlText(lbMainTapCardValue, mainboardTestData["刷卡"]+"  卡号:"+mainboardTestData["卡号"], decideColor(mainboardTestData["刷卡"]));
            updateControlText(lbMain2GValue, mainboardTestData["2G模块"]+"  信号值:"+mainboardTestData["信号值"], decideColor(mainboardTestData["2G模块"]));
            updateControlText(lbMainRelayValue, mainboardTestData["功率"], decideColor(mainboardTestData["功率"]));
            updateControlText(lbMainKeyValue, mainboardTestData["按键"], decideColor(mainboardTestData["按键"]));
            updateControlText(lbMainLcdValue, mainboardTestData["LCD"], decideColor(mainboardTestData["LCD"]));
            updateControlText(lbMainTrumpetValue, mainboardTestData["喇叭"], decideColor(mainboardTestData["喇叭"]));
            updateControlText(lbMainPwrValue, mainboardTestData["电源"], decideColor(mainboardTestData["电源"]));
            updateControlText(lbMainTestTimeValue, mainboardTestData["测试时间"]+" ", Color.Black);
            updateControlText(lbMainUsedTimeValue, mainboardTestData["测试用时"], Color.Black);

        }

        private void ShowInterfaceboardResult()
        {
            updateControlText(lbInterfacePcbCode, "PCB编号：", Color.Black);
            updateControlText(lbInterfaceTestor, "测试员：", Color.Black);
            updateControlText(lbInterfaceFwVer, "软件版本：", Color.Black);
            updateControlText(lbInterfaceAllResult, "测试结果：", decideColor(interfaceboardTestData["测试结果"]));
            updateControlText(lbInterfaceTapCard, "刷卡：", decideColor(interfaceboardTestData["刷卡"]));
            
            updateControlText(lbInterfaceKey, "按键：", decideColor(interfaceboardTestData["按键"]));
           
            updateControlText(lbInterfaceBle, "蓝牙：", decideColor(interfaceboardTestData["蓝牙"]));
            updateControlText(lbInterfaceTestTime, "测试时间：", Color.Black);
            updateControlText(lbInterfaceUsedTime, "测试用时：", Color.Black);

            updateControlText(lbInterfacePcbCodeValue, interfaceboardTestData["PCB编号"], Color.Black);
            updateControlText(lbInterfaceTestorValue, interfaceboardTestData["测试员"], Color.Black);
            updateControlText(lbInterfaceFwVerValue, interfaceboardTestData["软件版本"], Color.Black);
            updateControlText(lbInterfaceAllResultValue, interfaceboardTestData["测试结果"], decideColor(interfaceboardTestData["测试结果"]));
            updateControlText(lbInterfaceTapCardValue, interfaceboardTestData["刷卡"]+"  卡号:" + interfaceboardTestData["卡号"], decideColor(interfaceboardTestData["刷卡"]));
            updateControlText(lbInterfaceKeyValue, interfaceboardTestData["按键"], decideColor(interfaceboardTestData["按键"]));

            updateControlText(lbInterfaceBleValue, interfaceboardTestData["蓝牙"], decideColor(interfaceboardTestData["蓝牙"]));
            updateControlText(lbInterfaceTestTimeValue, interfaceboardTestData["测试时间"]+" ", Color.Black);
            updateControlText(lbInterfaceUsedTimeValue, interfaceboardTestData["测试用时"], Color.Black);
        }

        private void ShowChargerTestResult(bool state)
        {
         
            updateControlText(lbChargerCode, "电桩号：", Color.Black);
            updateControlText(skinLabelChargeMainboardCode, "主板编号：", Color.Black);
            updateControlText(skinLabelChargeInterfaceboardCode, "按键板编号：", Color.Black);
            updateControlText(lbChargerTestor, "测试员：", Color.Black);
            updateControlText(lbChargerFwVer, "软件版本：", Color.Black);
            updateControlText(lbChargerAllResult, "测试结果：", decideColor(chargerTestData["测试结果"]));
            updateControlText(lbChargerTapCard, "刷卡：", decideColor(chargerTestData["刷卡"]));

            updateControlText(lbChargerKey, "按键：", decideColor(chargerTestData["按键"]));
            updateControlText(lbChargerLCD, "LCD：", decideColor(chargerTestData["LCD"]));
            updateControlText(lbChargerCharge, "功率：", decideColor(chargerTestData["功率"]));
            updateControlText(lbChargerTrumpet, "喇叭：", decideColor(chargerTestData["喇叭"]));
            updateControlText(lbCharger2g, "2G模块：", decideColor(chargerTestData["2G模块"]));
            updateControlText(lbChargerble, "蓝牙：", decideColor(chargerTestData["蓝牙"]));

            updateControlText(lbChargerTestTime, "测试时间：", Color.Black);
            updateControlText(lbChargerTestUsedTime, "测试用时：", Color.Black);

            if (state == true)
            {
                updateControlText(lbChargerCodeValue, chargerTestData["电桩号"], Color.Black);

                updateControlText(skinLabelChargeMainboardCodeValue, chargerTestData["主板编号"], Color.Black);
                updateControlText(skinLabelChargeInterfaceboardCodeValue, chargerTestData["按键板编号"], Color.Black);

                updateControlText(lbChargerTestorValue, chargerTestData["测试员"], Color.Black);
                updateControlText(lbChargerFwVerValue, "主板 : "+chargerTestData["软件版本"]+"  按键板 : "+chargerTestData["按键板软件版本"] +"  ", Color.Black);
                updateControlText(lbChargerAllResultValue, chargerTestData["测试结果"], decideColor(chargerTestData["测试结果"]));
                updateControlText(lbChargerTapCardValue, chargerTestData["刷卡"] + "  卡号:" + chargerTestData["卡号"], decideColor(chargerTestData["刷卡"]));
                updateControlText(lbChargerKeyValue, chargerTestData["按键"], decideColor(chargerTestData["按键"]));

                updateControlText(lbChargerLCDValue, chargerTestData["LCD"], decideColor(chargerTestData["LCD"]));
                updateControlText(lbChargerTrumpetValue, chargerTestData["喇叭"], decideColor(chargerTestData["喇叭"]));
                updateControlText(lbChargerChargeValue, chargerTestData["功率"], decideColor(chargerTestData["功率"]));
                //updateControlText(lbChargerPwrValue, chargerTestData["电源"], decideColor(chargerTestData["电源"]));

                updateControlText(lbCharger2gValue, chargerTestData["2G模块"]+"  信号值：" + chargerTestData["信号值"] +"  ICCID:"+ chargerTestData["ICCID"]+"  ", decideColor(chargerTestData["2G模块"]));
                updateControlText(lbChargerBleValue, chargerTestData["蓝牙"], decideColor(chargerTestData["蓝牙"]));


                updateControlText(lbChargerTestTimeValue, chargerTestData["测试时间"]+" ", Color.Black);
                updateControlText(lbChargerTestUsedTimeValue, chargerTestData["测试用时"], Color.Black);
            }
            else
            {
                updateControlText(lbChargerCodeValue, "", Color.Black);

                updateControlText(skinLabelChargeMainboardCodeValue, "", Color.Black);
                updateControlText(skinLabelChargeInterfaceboardCodeValue,"", Color.Black);

                updateControlText(lbChargerTestorValue, "", Color.Black);
                updateControlText(lbChargerFwVerValue, "", Color.Black);
                updateControlText(lbChargerAllResultValue, "", Color.Black);
                updateControlText(lbChargerTapCardValue, "", Color.Black);
                updateControlText(lbChargerKeyValue, "", Color.Black);

                updateControlText(lbChargerLCDValue, "", Color.Black);
                updateControlText(lbChargerChargeValue, "", Color.Black);
                //updateControlText(lbChargerPwrValue, "", Color.Black);
                updateControlText(lbChargerTrumpetValue, "", Color.Black);
                updateControlText(lbChargerTestTimeValue, "", Color.Black);
                updateControlText(lbChargerTestUsedTimeValue, "", Color.Black);
                updateControlText(lbCharger2gValue,"",Color.Black);
                updateControlText(lbChargerBleValue, "", Color.Black);
            }
        }

        //private bool waittingResult(out int result,int waittime)
        //{
        //    while (waittime-->0)
        //    {
        //        if (result != -1)
        //        {

        //        }
        //    }

        //}

        private void skinButtonStandraTest_Click(object sender, EventArgs e)
        {          

            if (startTestting == false)
            {
                if ((textBoxMainboardCode.Text == ""&&skinTabControlPCBTestItem.SelectedTab== skinTabPageMainboard) ||(textBoxInterfaceCode.Text==""&& skinTabControlPCBTestItem.SelectedTab ==skinTabPageInterface))
                {
                    MessageBox.Show("PCB编码不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxMainboardCode.Text = "";
                    textBoxInterfaceCode.Text = "";
                }
                else
                {

                  
                    SendTestMode(0x00);
                }
             
               // skinTabControlPCBAMainBoardTest.SelectedIndex++;
            }
            else
            {
                SendTestMode(0x01);
               
            }
            
        }

        private void skinButtonTapCardSkin_Click(object sender, EventArgs e)
        {

           

                skinTabControlPCBAMainBoardTest.SelectedIndex++;
            
            
        }

        int ItemCountDown(int time, Label label,TabControl tabControl,TabPage tabPage)
        {
            if (time > 0)
            {
                time--;
                updateControlText(label, time.ToString("D2"));
                if (time == 0)
                {
                    if (tabControl.SelectedTab == tabPage)
                    {
                        tabControl.SelectedIndex++;
                    }
                   
                }
            }
            return time;
        }

        int t = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {

            if (t++ >= 10)
            {
                t = 0;
                //if (skinTabControlPCBTestItem.SelectedTab == skinTabPageMainboard)
                if (mainBoardTestingflag)
                {
                    countDownTimeMainBorad.key = ItemCountDown(countDownTimeMainBorad.key, skinLabelKeyCountDown, skinTabControlPCBAMainBoardTest, skinTabPageKey);
                    countDownTimeMainBorad.lcd = ItemCountDown(countDownTimeMainBorad.lcd, skinLabelLCDCountDown, skinTabControlPCBAMainBoardTest, skinTabPageLCD);
                    countDownTimeMainBorad.relay = ItemCountDown(countDownTimeMainBorad.relay, skinLabelRelayCountDown, skinTabControlPCBAMainBoardTest, skinTabPageRelay);
                    countDownTimeMainBorad.tapCard = ItemCountDown(countDownTimeMainBorad.tapCard, skinLabelCountdownTapCard, skinTabControlPCBAMainBoardTest, skinTabPageTapCard);
                    countDownTimeMainBorad.trumpet = ItemCountDown(countDownTimeMainBorad.trumpet, skinLabelTrumpetCountDown, skinTabControlPCBAMainBoardTest, skinTabPageTrumpet);
                    countDownTimeMainBorad._2G = ItemCountDown(countDownTimeMainBorad._2G, skinLabel2GCountDown, skinTabControlPCBAMainBoardTest, skinTabPage2GModule);
                    countDownTimeMainBorad.PowerSource = ItemCountDown(countDownTimeMainBorad.PowerSource, skinLabelPwrCountdown, skinTabControlPCBAMainBoardTest, skinTabPagePowerSource);
                }
                else if (interfaceBoardTestingflag)
                {
                    countDownTimeInterfaceBorad.BLE = ItemCountDown(countDownTimeInterfaceBorad.BLE, skinLabelInterfaceBleCountdown, skinTabControlInterfaceTest, skinTabPageBleTest);
                    countDownTimeInterfaceBorad.key = ItemCountDown(countDownTimeInterfaceBorad.key, skinLabelInterfaceKeyCountdown, skinTabControlInterfaceTest, skinTabPageKeyTest);
                    countDownTimeInterfaceBorad.tapCard = ItemCountDown(countDownTimeInterfaceBorad.tapCard, skinLabelInterfaceTapCardCountdown, skinTabControlInterfaceTest, skinTabPageTapCardTest);

                }
                else if (chargerTestingflag)
                {
                    countDownTimeCharger.key = ItemCountDown(countDownTimeCharger.key, skinLabelChargerKeyCountdown, skinTabControlChargerTest, skinTabPageChargerKey);
                    countDownTimeCharger.lcd = ItemCountDown(countDownTimeCharger.lcd, skinLabelChargerLcdCountdown, skinTabControlChargerTest, skinTabPageChargerLCD);
                    countDownTimeCharger.tapCard = ItemCountDown(countDownTimeCharger.tapCard, skinLabelChargerTapCardCountdown, skinTabControlChargerTest, skinTabPageChargerTapCard);
                    countDownTimeCharger.relay = ItemCountDown(countDownTimeCharger.relay, skinLabelChargeCountdown, skinTabControlChargerTest, skinTabPageCharge);
                    countDownTimeCharger.trumpet = ItemCountDown(countDownTimeCharger.trumpet, skinLabelChargeCountdown, skinTabControlChargerTest, skinTabPageChargerTrumpet);
                    countDownTimeCharger._2G = ItemCountDown(countDownTimeCharger._2G, skinLabelCharger2GCountdown, skinTabControlChargerTest, skinTabPageCharger2GModule);
                    countDownTimeCharger.BLE = ItemCountDown(countDownTimeCharger.BLE, skinLabelChargerBleCountdown, skinTabControlChargerTest, skinTabPageChargerBLE);
                    //countDownTimeCharger.PowerSource = ItemCountDown(countDownTimeCharger.PowerSource, skinLabelChargerPowerSrcCountdown, skinTabControlChargerTest, skinTabPageChargerPower);
                }

            }
        }

        private void reTestMainBoard()
        {
            int t = 0;
            while (mainboardTestRunningFlag == true)
            {
                Thread.Sleep(10);
                if (t++ > 1000)
                {
                    break;
                }
            }

            mainboardTestProcess();
        }

        private void reTestCharger()
        {
            int t = 0;
            while (chargerTestRunningFlag == true)
            {
                Thread.Sleep(10);
                if (t++ > 1000)
                {
                    break;
                }
            }

            chargerTestProcess();
        }

        

        private void reTestInterfaceBoard()
        {
            int t = 0;
            while (interfaceboardTestRunningFlag == true)
            {
                Thread.Sleep(10);
                if (t++ > 1000)
                {
                    break;
                }
            }

            interfaceBoardTestProcess();
        }

        private void skinButtonTapCardReTest_Click(object sender, EventArgs e)
        {

            reTestMainBoard();
        }

        private void textBoxMainboardCode_TextChanged(object sender, EventArgs e)
        {
         
            //else
            //{
            //    MessageBox.Show("编号不能为空,请重新扫描！","提示",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            //    textBoxMainboardCode.Text = "";
            //    textBoxMainboardCode.Focus();
            //}
        }

        private void skinTabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            switch (skinTabControlPCBTestItem.SelectedIndex)
            {
                case 0:
                    skinTabControlPCBAMainBoardTest.SelectedIndex = 0;
                    textBoxMainboardCode.Focus();
                    break;
                case 1:
                    skinTabControlInterfaceTest.SelectedIndex = 0;
                    textBoxInterfaceCode.Focus();
                    break;

            }
        }

        private void skinTabControlTestMenu_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (skinTabControlTestMenu.SelectedIndex)
            {
                case 0:                   
                    switch (skinTabControlPCBTestItem.SelectedIndex)
                    {
                        case 0:
                            textBoxMainboardCode.Focus();
                        break;

                        case 1:
                            textBoxInterfaceCode.Focus();
                        break;
                        
                    }
                    break;
                case 1:
                    skinTabControlChargerTest.SelectedIndex = 0;
                    textBoxChargerQrCode.Focus();

                    break;
                case 2:
                    
                    textBoxAgingTestCode.Focus();

                    break;
                case 5:
                    textBoxChargerIdQrCode.Focus();
                    break;

            }

            //skinTabControl1_SelectedIndexChanged(sender,e);
        }

        private void CurrentTestItem(int TesttingItem)
        {
         
            switch (TesttingItem)
            {
                case 0:
                    interfaceBoardTestingflag = false;
                    mainBoardTestingflag = true;
                    chargerTestingflag = false; 
                 
                    break;
                case 1:
                      interfaceBoardTestingflag = true;
                     mainBoardTestingflag = false;
                     chargerTestingflag = false;
                    break;
                case 2:
                    interfaceBoardTestingflag = false;
                    mainBoardTestingflag = false;
                    chargerTestingflag = true;
                    break;

                default:
                    break;
            }
        }

        private void skinButtonConfirmMainboard_Click(object sender, EventArgs e)
        {
            CurrentTestItem(0);
            //skinTabControlPCBAMainBoardTest.SelectedIndex++;
            skinButtonStandraTest_Click(sender,e);
        }

        private void skinButton2GSkip_Click(object sender, EventArgs e)
        {
            if (DataProgram.testReportData[5] == "")
            {
                DataProgram.testReportData[5] = "未测试";
            }
            updateTableSelectedIndex(skinTabControlPCBAMainBoardTest, ++mainBoardTabSelectIndex);
        }

        private void skinButton2GReTest_Click(object sender, EventArgs e)
        {
            reTestMainBoard();
        }

        private void skinButtonKeySkip_Click(object sender, EventArgs e)
        {
            updateTableSelectedIndex(skinTabControlPCBAMainBoardTest, ++mainBoardTabSelectIndex);
        }

        private void skinButtonKeyReTest_Click(object sender, EventArgs e)
        {
            reTestMainBoard();
        }

        private void skinButtonLcdSuccess_Click(object sender, EventArgs e)
        {

        }

        private void skinButtonLcdFail_Click(object sender, EventArgs e)
        {

        }

        private void skinButtonLcdSkip_Click(object sender, EventArgs e)
        {

        }

        private void skinButtonLcdReTest_Click(object sender, EventArgs e)
        {
            reTestMainBoard();
        }

        private void skinButtonTrumpetSuccess_Click(object sender, EventArgs e)
        {
            mainboardTestData["喇叭"] = "通过";
            updateControlText(skinLabelTrumpetResult, "通过",Color.Green);
            updateTableSelectedIndex(skinTabControlPCBAMainBoardTest, ++mainBoardTabSelectIndex);
        }

        private void skinButtonTrumpetFail_Click(object sender, EventArgs e)
        {
            mainboardTestData["喇叭"] = "不通过";
            updateControlText(skinLabelTrumpetResult, "不通过",Color.Red);
        }

        private void skinButtonTrumpetSkip_Click(object sender, EventArgs e)
        {

            updateTableSelectedIndex(skinTabControlPCBAMainBoardTest, ++mainBoardTabSelectIndex);
        }

        private void skinButtonTrumpetReTest_Click(object sender, EventArgs e)
        {
            reTestMainBoard();
        }

        private void skinButtonPwrSuccess_Click(object sender, EventArgs e)
        {
            mainboardTestData["电源"] = "通过";
            updateControlText(skinLabelPwrResult, "通过",Color.Green);
            updateTableSelectedIndex(skinTabControlPCBAMainBoardTest, ++mainBoardTabSelectIndex);
        }

        private void skinButtonPwrFail_Click(object sender, EventArgs e)
        {
            mainboardTestData["电源"] = "不通过";
            updateControlText(skinLabelPwrResult, "不通过",Color.Red);
        }

        private void skinButtonPwrSkip_Click(object sender, EventArgs e)
        {
            updateTableSelectedIndex(skinTabControlPCBAMainBoardTest, ++mainBoardTabSelectIndex);
        }

        private void skinButtonReTest_Click(object sender, EventArgs e)
        {
            reTestMainBoard();
        }

        private void skinButtonRelayReTest_Click(object sender, EventArgs e)
        {
            reTestMainBoard();
        }

        private void skinTabControlInterfaceTest_SelectedIndexChanged(object sender, EventArgs e)
        {
            interfaceTabSelectIndex = skinTabControlInterfaceTest.SelectedIndex;

            if (interfaceTabSelectIndex == 0)
            {
                textBoxInterfaceCode.Focus();
            }

            if (interfaceboardTestThread != null)
            {
                interfaceboardTestThread.Abort();
                interfaceboardTestThread = null;
            }
            interfaceboardTestThread = new Thread(interfaceBoardTestProcess);
            interfaceboardTestThread.Start();
        }

        private void skinButtonInterFaceTapCardSkip_Click(object sender, EventArgs e)
        {
            if(getPresentTabPage(skinTabControlInterfaceTest)==skinTabPageTapCardTest)
            {
                updateTableSelectedIndex(skinTabControlInterfaceTest, ++interfaceTabSelectIndex);
            }
         
        }

        private void skinButtonInterfaceTapCardReTest_Click(object sender, EventArgs e)
        {
            reTestInterfaceBoard();
        }

        private void skinButtonComfirmInterface_Click(object sender, EventArgs e)
        {
            CurrentTestItem(1);
            skinButtonStandraTest_Click(sender, e);
        }

        private void textBoxInterfaceCode_KeyPress(object sender, KeyPressEventArgs e)
        {
            //int index = textBoxInterfaceCode.Text.IndexOf(' ');
            //if (index > 0)
            //{
            if(e.KeyChar == '\r')
            {
               
                    //DataProgram.testReportData[0] = textBoxInterfaceCode.Text.Trim();
                    skinButtonComfirmInterface_Click(sender, e);
                
              
            }
         
            //    //updateTableSelectedIndex(skinTabControlPCBAMainBoardTest, 1);
            //}
        }

        private void textBoxMainboardCode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
              
                    //DataProgram.testReportData[0] = textBoxMainboardCode.Text.Trim();
                    skinButtonConfirmMainboard_Click(sender, e);
                

            }
        }

        private void skinButtonRelaySkip_Click(object sender, EventArgs e)
        {
            updateTableSelectedIndex(skinTabControlPCBAMainBoardTest,++mainBoardTabSelectIndex);
        }

        private void skinButtonInterfaceKeySkip_Click(object sender, EventArgs e)
        {
            if (getPresentTabPage(skinTabControlInterfaceTest) == skinTabPageKeyTest)
            {
               
              
                updateTableSelectedIndex(skinTabControlInterfaceTest, ++interfaceTabSelectIndex);
            }
        }

        private void skinButtonInterfaceBleSkip_Click(object sender, EventArgs e)
        {
            updateTableSelectedIndex(skinTabControlInterfaceTest, ++interfaceTabSelectIndex);
        }

        private void skinButtonInterfaceBleReTest_Click(object sender, EventArgs e)
        {
            reTestInterfaceBoard();
        }

        private void skinTabControlInterfaceTest_Selecting(object sender, TabControlCancelEventArgs e)
        {
            //if (startTestting == true)
            //{
            //    e.Cancel=true;
            //}
            
        }

        private void skinTabControlTestMenu_Selecting(object sender, TabControlCancelEventArgs e)
        {


            if ((getPresentTabPage(skinTabControlTestMenu) == skinTabPagePCBA) || (getPresentTabPage(skinTabControlTestMenu) == skinTabPageChargerTest) || (getPresentTabPage(skinTabControlTestMenu) == skinTabPageConfig))
            {
                if (serialPort1.IsOpen == false)
                {

                    MessageBox.Show("请先打开串口", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                }


            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                this.Dispose();
                this.Close();

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message,"提示",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
            finally
            {
                Application.Exit();
            }
         
        }

        private void skinButtonSerialCtrl_Click_1(object sender, EventArgs e)
        {
            try
            {

                if (serialPort1 != null)
                {

                    if (!serialPort1.IsOpen)
                    {
                        serialPort1.BaudRate = int.Parse(skinComboBoxBandRate.SelectedItem.ToString());
                        serialPort1.PortName = skinComboBoxPortSelect.SelectedItem.ToString();
                        serialPort1.Open();
                        if (serialPort1.IsOpen)
                        {
                            skinButtonSerialCtrl.Text = "关闭串口";

                            //sendTestRequestCmd((byte)Command._485);
                        }
                    }
                    else if (serialPort1.IsOpen)
                    {
                       
                        serialPort1.Close();
                        if (!serialPort1.IsOpen)
                        {
                            skinButtonSerialCtrl.Text = "打开串口";

                        }
                    }

                    else
                    {
                        skinButtonSerialCtrl.Text = "打开串口";
                    }
                }
                else
                {
                    skinButtonSerialCtrl.Text = "打开串口";
                    return;
                }
            
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "异常提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                skinButtonSerialCtrl.Text = "打开串口";
            }
        }

        private void skinButtonSaveConfig_Click(object sender, EventArgs e)
        {
            try
            {
                TestSettingInfo["ChargerModel"] = comboBoxChargerModel.SelectedItem;
                TestSettingInfo["CountDown"] = numericUpDownTestWaittime.Value;
                TestSettingInfo["CardNum"] = textBoxTestCardNum.Text;
                TestSettingInfo["CsqLowerLimit"] = numericUpDownCsqLowerLimit.Value;
                TestSettingInfo["CsqUpperLimit"] = numericUpDownCsqUpperLimit.Value;
                TestSettingInfo["PowerLowerLimit"] = numericUpDownPowerLowerLimit.Value;
                TestSettingInfo["PowerUpperLimit"] = numericUpDownPowerUpperLimit.Value;
                TestSettingInfo["AllFuncAgingTest"] = checkBoxHoleAgingTest.Checked;
                DataProgram.WriteConfig(DataProgram.testConfigFile, TestSettingInfo);
                MessageBox.Show("保存成功","温馨提示",MessageBoxButtons.OK,MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message,"异常提示",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }

        }

        private void skinSplitContainer18_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }


        protected bool isNumberic(string message, out int result)
        {
            System.Text.RegularExpressions.Regex rex =
            new System.Text.RegularExpressions.Regex(@"^\d+$");
            result = -1;
            if (rex.IsMatch(message))
            {
                result = int.Parse(message);
                return true;
            }
            else
                return false;
        }
        
    
        private void textBoxChargerQrCode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {

                skinButtonChargerTestConfirm_Click(sender, e);
               
                //DataProgram.testReportData[0] = textBoxInterfaceCode.Text.Trim();
                
            }
        }

        private void skinButtonStartTestCharger_Click(object sender, EventArgs e)
        {
            if (startTestting == false)
            {

                if (textBoxChargerQrCode.Text.IndexOf(DataProgram.X10QrCodeUrl) == 0)
                {
                    textBoxChargerQrCode.Text = textBoxChargerQrCode.Text.Remove(0, DataProgram.X10QrCodeUrl.Length);
                    System.Text.RegularExpressions.Regex rex =
                    new System.Text.RegularExpressions.Regex(@"^\d+$");
                    if (rex.IsMatch(textBoxChargerQrCode.Text)==false)
                    {

                        MessageBox.Show("桩号包含非数字！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBoxChargerQrCode.Text = "";
                        return;
                    }


                }
                else
                {
                    MessageBox.Show("二维码不正确！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxChargerQrCode.Text = "";
                    return;


                }


                //if ((textBoxMainboardCode.Text == "" && skinTabControlPCBTestItem.SelectedTab == skinTabPageMainboard) || (textBoxInterfaceCode.Text == "" && skinTabControlPCBTestItem.SelectedTab == skinTabPageInterface))
                if (textBoxChargerQrCode.Text=="" && skinTabControlChargerTest.SelectedTab==skinTabPageChargerID)
                {
                    MessageBox.Show("桩号不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxChargerQrCode.Text = "";
                }
                else
                {
                  
                    SendTestMode(0x00);

                }

                // skinTabControlPCBAMainBoardTest.SelectedIndex++;
            }
            else
            {
                SendTestMode(0x01);

            }
        }

        private void skinButtonChargerTestConfirm_Click(object sender, EventArgs e)
        {
            //if (textBoxChargerQrCode.Text.IndexOf(DataProgram.X10QrCodeUrl) == 0)
            //{
            //    textBoxChargerQrCode.Text = textBoxChargerQrCode.Text.Remove(0, DataProgram.X10QrCodeUrl.Length);
            //    System.Text.RegularExpressions.Regex rex =
            //    new System.Text.RegularExpressions.Regex(@"^\d+$");
            //    if (rex.IsMatch(textBoxChargerQrCode.Text))
            //    {
                    CurrentTestItem(2);
                    skinButtonStartTestCharger_Click(sender, e);
            //    }
            //    else
            //    {
            //        MessageBox.Show("桩号包含非数字！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //        textBoxChargerQrCode.Text = "";
            //    }


            //}
            //else
            //{
            //    MessageBox.Show("二维码不正确！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    textBoxChargerQrCode.Text = "";


            //}

        }

        Thread chargerTestThread;

        private void skinTabControlChargerTest_SelectedIndexChanged(object sender, EventArgs e)
        {
            chargerTestSelectIndex = skinTabControlChargerTest.SelectedIndex;
            if (chargerTestSelectIndex == 0)
            {
                textBoxChargerQrCode.Focus();
            }

            if(chargerTestThread != null)
            {
                chargerTestThread.Abort();
                chargerTestThread = null;
            }
            chargerTestThread = new Thread(chargerTestProcess);
            chargerTestThread.Start();

        }

        private void skinButtonClearDebug_Click(object sender, EventArgs e)
        {
            textBoxDebuginfo.Text = "";
        }

        private void skinButtonReport_Click(object sender, EventArgs e)
        {
            skinButtonReportDir_Click(sender,e);
        }

        private void skinButtonChargerTapCardSkip_Click(object sender, EventArgs e)
        {
            if (getPresentTabPage(skinTabControlChargerTest) == skinTabPageChargerTapCard)
            {
                updateTableSelectedIndex(skinTabControlChargerTest,++chargerTestSelectIndex);
            }
        }

        private void skinButtonChargerTapCardReTest_Click(object sender, EventArgs e)
        {
            reTestCharger();
        }

        private void skinButtonChargerKeySkip_Click(object sender, EventArgs e)
        {

            if (getPresentTabPage(skinTabControlChargerTest) == skinTabPageChargerKey)
            {
                updateTableSelectedIndex(skinTabControlChargerTest, ++chargerTestSelectIndex);
            }
        }

        private void skinButtonChargerKeyReTest_Click(object sender, EventArgs e)
        {
            reTestCharger();
        }

        private void skinButtonChargerLcdSuccess_Click(object sender, EventArgs e)
        {
            updateControlText(skinLabelChargerLcdResult, "通过", Color.Green);
            chargerTestData["LCD"] = "通过";
            updateTableSelectedIndex(skinTabControlChargerTest, ++chargerTestSelectIndex);
        }

        private void skinButtonChargerLcdFail_Click(object sender, EventArgs e)
        {
            updateControlText(skinLabelChargerLcdResult, "不通过", Color.Red);
            chargerTestData["LCD"] = "不通过";
        }

        private void skinButtonChargerLcdSkip_Click(object sender, EventArgs e)
        {
            updateTableSelectedIndex(skinTabControlChargerTest, ++chargerTestSelectIndex);
        }

        private void skinButtonChargerLcdReTest_Click(object sender, EventArgs e)
        {
            reTestCharger();
        }

        private void skinButtonChargerPowerSrcSuccess_Click(object sender, EventArgs e)
        {
            //updateControlText(skinLabelChargerPowerSrcResult, "通过", Color.Green);
            //chargerTestData["电源"] = "通过";
            //updateTableSelectedIndex(skinTabControlChargerTest, ++chargerTestSelectIndex);
        }

        private void skinButtonChargerPowerSrcFail_Click(object sender, EventArgs e)
        {
            //updateControlText(skinLabelChargerPowerSrcResult, "不通过", Color.Red);
            //chargerTestData["电源"] = "不通过";
        }

        private void skinButtonChargerPowerSrcSkip_Click(object sender, EventArgs e)
        {
            updateTableSelectedIndex(skinTabControlChargerTest, ++chargerTestSelectIndex);
        }

        private void skinButtonChargerPowerSrcReTest_Click(object sender, EventArgs e)
        {
            reTestCharger();
        }

        private void skinButtonChargeSkip_Click(object sender, EventArgs e)
        {
            updateTableSelectedIndex(skinTabControlChargerTest, ++chargerTestSelectIndex);
        }

        private void skinButtonChargeReTest_Click(object sender, EventArgs e)
        {
            reTestCharger();
        }

        private void skinLabel63_Click(object sender, EventArgs e)
        {

        }

        private void skinButtonAccountSetting_Click(object sender, EventArgs e)
        {
            AccountSettingForm accountSettingForm = new AccountSettingForm();
            accountSettingForm.ShowDialog();
        }

        private void skinButtonSubmitAgingTest_Click(object sender, EventArgs e)
        {
            if (textBoxAgingTestCode.Text == "")
            {
                MessageBox.Show("电桩号不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxAgingTestCode.Text = "";
                return;
            }
            else if (dateTimePickerAgingTestStartTime.Value > dateTimePickerAgingTestEndTime.Value)
            {
                MessageBox.Show("开始时间不能大于结束时间！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (textBoxAgingTestCode.Text.IndexOf(DataProgram.X10QrCodeUrl) == 0)
            {
                textBoxAgingTestCode.Text = textBoxAgingTestCode.Text.Remove(0, DataProgram.X10QrCodeUrl.Length);
            }

            Dictionary<string, string> dictionary = new Dictionary<string, string> {
                {"电桩号",textBoxAgingTestCode.Text.Trim() },
                {"测试员",DataProgram.PresentAccount },
                {"测试结果",(radioButtonPass.Checked==true)?"通过":"不通过"},
                {"测试功率",numericUpDownPower.Value.ToString() },
                {"开始时间", dateTimePickerAgingTestStartTime.Value.ToString("yyyy-MM-dd HH:mm:ss")},
                {"结束时间", dateTimePickerAgingTestEndTime.Value.ToString("yyyy-MM-dd HH:mm:ss")},
                {"测试用时", (int)((dateTimePickerAgingTestEndTime.Value - dateTimePickerAgingTestStartTime.Value).TotalSeconds/60) + "分 "+(int)((dateTimePickerAgingTestEndTime.Value - dateTimePickerAgingTestStartTime.Value).TotalSeconds%60) +"秒" }
            };

            DataProgram.WriteReport(TestSettingInfo["ChargerModel"] + "_老化测试.xlsx", TestSettingInfo["ChargerModel"] + "_老化测试", dictionary);


            string mysqlCmd = DataProgram.AgingTestMysqlCommand(TestSettingInfo["ChargerModel"].ToString(),textBoxAgingTestCode.Text.Trim(),DataProgram.PresentAccount,numericUpDownPower.Value,dateTimePickerAgingTestStartTime.Value.ToString("yyyy-MM-dd HH:mm:ss"), dateTimePickerAgingTestEndTime.Value.ToString("yyyy-MM-dd HH:mm:ss"), (radioButtonPass.Checked==true)?"Pass":"Fail");
            if (DataProgram.SendMysqlCommand(mysqlCmd,true))
            {
                DataProgram.DealBackUpData(DataProgram.backupMysqlCmdFile);
            }

            MessageBox.Show("提交成功！","温馨提示",MessageBoxButtons.OK,MessageBoxIcon.Information);
            textBoxAgingTestCode.Text = "";
        }

        private void skinButtonChargerTrumpetPass_Click(object sender, EventArgs e)
        {
            updateControlText(skinLabelChargerTrumpetResult, "通过", Color.Green);
            chargerTestData["喇叭"] = "通过";
            updateTableSelectedIndex(skinTabControlChargerTest, ++chargerTestSelectIndex);
        }

        private void skinButtonChargerTrumpetFail_Click(object sender, EventArgs e)
        {
            updateControlText(skinLabelChargerTrumpetResult, "不通过", Color.Red);
            chargerTestData["喇叭"] = "不通过";
        }

        private void skinButtonChargerTrumpetSkip_Click(object sender, EventArgs e)
        {
            updateTableSelectedIndex(skinTabControlChargerTest, ++chargerTestSelectIndex);
        }

        private void skinButtonChargerTrumpetRetest_Click(object sender, EventArgs e)
        {
            reTestCharger();
        }

        private void checkBoxHoleAgingTest_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBoxHoleAgingTest.Checked==true)
            MessageBox.Show("整机测试保存为整机老化测试！","注意",MessageBoxButtons.OK,MessageBoxIcon.Asterisk);
        }

        private void skinComboBoxPortSelect_DropDown(object sender, EventArgs e)
        {
            try
            {
                skinComboBoxPortSelect.Items.Clear();
                //添加串口项目  
                foreach (string s in System.IO.Ports.SerialPort.GetPortNames())
                {//获取有多少个COM口  
                    skinComboBoxPortSelect.Items.Add(s);
                }
                if (skinComboBoxPortSelect.Items.Count > 0)
                {
                    skinComboBoxPortSelect.SelectedIndex = 0;
                    skinComboBoxBandRate.SelectedIndex = 0;
                }
                else
                {
                    skinComboBoxPortSelect.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示");
            }
        }

        private void textBoxAgingTestCode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                if (textBoxAgingTestCode.Text.IndexOf(DataProgram.X10QrCodeUrl) == 0)
                {
                    textBoxAgingTestCode.Text = textBoxAgingTestCode.Text.Remove(0, DataProgram.X10QrCodeUrl.Length);
                    System.Text.RegularExpressions.Regex rex =
                    new System.Text.RegularExpressions.Regex(@"^\d+$");
                    if (rex.IsMatch(textBoxAgingTestCode.Text)==false)
                    {
                        MessageBox.Show("桩号包含非数字！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBoxAgingTestCode.Text = "";
                    }
                


                }
                else
                {
                    MessageBox.Show("二维码不正确！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxAgingTestCode.Text = "";

                }
            }
        }

        private void skinButtonCharger2GSkip_Click(object sender, EventArgs e)
        {
            updateTableSelectedIndex(skinTabControlChargerTest, ++chargerTestSelectIndex);
        }

        private void skinButtonCharger2GRetest_Click(object sender, EventArgs e)
        {
            reTestCharger();
        }

        private void skinButtonChargerBleSkip_Click(object sender, EventArgs e)
        {
            updateTableSelectedIndex(skinTabControlChargerTest, ++chargerTestSelectIndex);
        }

        private void skinButtonChargerBleReTest_Click(object sender, EventArgs e)
        {
            reTestCharger();
        }

        private void serialPort1_ErrorReceived(object sender, System.IO.Ports.SerialErrorReceivedEventArgs e)
        {
            MessageBox.Show("serialPort1_ErrorReceived");
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void skinButtonRegisterCode_Click(object sender, EventArgs e)
        {
            SendSetRegisterCode(textBoxRegisterCode.Text.Trim());
        }

        private void skinButtonDevType_Click(object sender, EventArgs e)
        {
            byte type = (byte)(radioButtonGateWay.Checked == true ? 0x01 : 0x00);

            SendSetDevType(type);
        }

        private void skinButtonGateWayAddr_Click(object sender, EventArgs e)
        {
            SendSetGwAddr(textBoxGateWayAddr.Text.Trim());
        }

        private void skinButtonSerAddr_Click(object sender, EventArgs e)
        {
            SendSetServerAddr(textBoxServerAddr.Text.Trim());
        }

        private void skinButtonSerPort_Click(object sender, EventArgs e)
        {
            SendSetServerPort(textBoxServerPort.Text.Trim());
        }

        private void skinButtonLogPrintSw_Click(object sender, EventArgs e)
        {
            byte sw = (byte)(radioButtonPrintOn.Checked==true?1:0);
            SendSetPrintSwitch(sw);

        }

        List<string> terminalInfoList = new List<string> { };

        private void skinButtonAddTerminalInfo_Click(object sender, EventArgs e)
        {
            if (textBoxTerminalInfo.Text != "")
            {
                terminalInfoList.Add(textBoxTerminalInfo.Text);
                string str = "";

                foreach (var item in terminalInfoList)
                {
                    str += item+"\r\n";
                }
                labelTermialCount.Text = "数量："+ terminalInfoList.Count;
                richTextBoxTotalTerminalInfo.Text = str;
                textBoxTerminalInfo.Text = "";
            }
            else
            {
                MessageBox.Show("数据不能为空","提示",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
        }

        private void textBoxTerminalInfo_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                textBoxTerminalInfo.Text = textBoxTerminalInfo.Text.Trim();
                skinButtonAddTerminalInfo_Click(sender, e);
            }
        }

        private void skinButtonDeleteTerminalInfo_Click(object sender, EventArgs e)
        {
            if (terminalInfoList.Count > 0)
            {
                terminalInfoList.RemoveAt(terminalInfoList.Count-1);

                string str = "";

                foreach (var item in terminalInfoList)
                {
                    str += item + "\r\n";
                }
                labelTermialCount.Text = "数量：" + terminalInfoList.Count;
                richTextBoxTotalTerminalInfo.Text = str;
            }
        }

        private void skinButtonTerminalInfo_Click(object sender, EventArgs e)
        {
            SendSetTerminalInfo(terminalInfoList);
        }

        private void skinButtonCleanConfigLog_Click(object sender, EventArgs e)
        {
            textBoxConfigPrint.Text = "";
        }

        private void skinButtonSetChargerID_Click(object sender, EventArgs e)
        {
            SendSetID(textBoxChargerID.Text.Trim());
        }

        private void skinButtonDevReboot_Click(object sender, EventArgs e)
        {
            SendDevReboot();
        }

        private void skinButtonUniqueCode_Click(object sender, EventArgs e)
        {
            SendSetUUID(textBoxUniqueCode.Text.Trim());
        }

        private void skinButtonLcdSuccess_Click_1(object sender, EventArgs e)
        {
            updateControlText(skinLabelLcdResult, "通过", Color.Green);
            mainboardTestData["LCD"] = "通过";
            updateTableSelectedIndex(skinTabControlPCBAMainBoardTest, ++mainBoardTabSelectIndex);
        }

        private void skinButtonLcdFail_Click_1(object sender, EventArgs e)
        {
            updateControlText(skinLabelLcdResult, "失败", Color.Red);
            mainboardTestData["LCD"] = "失败";
        }

        private void skinButtonLcdSkip_Click_1(object sender, EventArgs e)
        {
            updateTableSelectedIndex(skinTabControlPCBAMainBoardTest, ++mainBoardTabSelectIndex);
        }

        private void skinButtonLcdReTest_Click_1(object sender, EventArgs e)
        {
            reTestMainBoard();
        }

        private void textBoxChargerIdQrCode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                if (textBoxChargerIdQrCode.Text.IndexOf(DataProgram.X10QrCodeUrl) == 0)
                {
                    textBoxChargerIdQrCode.Text = textBoxChargerIdQrCode.Text.Remove(0, DataProgram.X10QrCodeUrl.Length);
                    System.Text.RegularExpressions.Regex rex =
                    new System.Text.RegularExpressions.Regex(@"^\d+$");
                    if (rex.IsMatch(textBoxChargerIdQrCode.Text) == true)
                    {
                        labelQrCodeChargerId.Text = textBoxChargerIdQrCode.Text;

                        bool res = DataProgram.DuplicateChecking(labelQrCodeChargerId.Text);


                        textBoxChargerIdQrCode.Text = "";
                        if (res == false)
                        {

                            updateControlText(labelCheckResult, "桩号重复", Color.Red);
                        }
                        else
                        {
                            updateControlText(labelCheckResult, "通过", Color.Green);
                            string mysqlCmd = "INSERT INTO  product_charger_id_tbl(charger_id,testor) VALUES( '"+ labelQrCodeChargerId.Text + "','" + DataProgram.PresentAccount + "');";

                            DataProgram.SendMysqlCommand(mysqlCmd,false);
                        }
                        //updateControlText(labelCheckResult, "", Color.Black);

                    }
                    else
                    {
                        MessageBox.Show("桩号包含非数字！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBoxChargerIdQrCode.Text = "";
                    }


                }
                else
                {
                    MessageBox.Show("二维码不正确！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxChargerIdQrCode.Text = "";

                }
                textBoxChargerIdQrCode.Focus();
            }
        }

        private void skinButtonCleanRecord_Click(object sender, EventArgs e)
        {
            if (DataProgram.PresentAccount == "Admin")
            {
                if (MessageBox.Show("是否确认清空已保存的桩号列表？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    if (MessageBox.Show("此操作不可逆！\r\n确认删除？", "注意", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                    {
                        string mysqldeletecmd = "DELETE FROM product_charger_id_tbl";
                        DataProgram.SendMysqlCommand(mysqldeletecmd, false);
                    }
                }
            }
            else
            {
                MessageBox.Show("此操作权限仅限管理员！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
           
        }
    }
}
