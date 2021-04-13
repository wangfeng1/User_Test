using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Windows.Forms;
using System.Xml;


namespace User_Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            lbLocalIP.Text = GetLocalIp();//获取本机IP
        }



        private UdpClient udpcSend;
        private UdpClient udpcRecv;
        private bool isLocalConneted = false;//本地是否已连接
        private Thread thrRecv;//本地监听接收线程

        private void label1_Click(object sender, EventArgs e)
        {

        }
        public string GetLocalIp()
        {
            ///获取本地的IP地址
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                }
            }
            return AddressIP;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbRemoteIP.Text) || string.IsNullOrWhiteSpace(tbRemotePort.Text))
            {
                MessageBox.Show("远程IP和端口不能为空！", "系统提示");
                return;
            }
            if (!isLocalConneted)
            {
                MessageBox.Show("请先建立本地连接！", "系统提示");
                return;
            }

            string[] param = new string[] {"123", tbRemoteIP.Text, tbRemotePort.Text };

            SendMessage(param);


        }

        private void btnConnect_Click(object sender, EventArgs e)//连接按钮
        {
           
            if (string.IsNullOrWhiteSpace(tbLocalPort.Text))
            {
                MessageBox.Show("本地IP和端口不能为空！", "系统提示");
                return;
            }

            if (!isLocalConneted)//构建本地连接
            {
                IPEndPoint localIpep = new IPEndPoint(IPAddress.Parse(lbLocalIP.Text), Convert.ToInt32(tbLocalPort.Text));  //本机IP 及端口
                udpcSend = new UdpClient(localIpep);  

                btnConnect.Text = "断开";
                isLocalConneted = true;

                //启动监听接收信息
                BeginListenRecv();
            }
            else
            {
                thrRecv.Abort();
                udpcRecv.Close();

                isLocalConneted = false;
                btnConnect.Text = "连接";

            }
        }
        /// 【开始监听】
        private void BeginListenRecv()
        {
            udpcRecv = udpcSend;//接收与发送使用同一个实例
 
            thrRecv = new Thread(ReceiveMessage);
            thrRecv.Start();
            //           ShowMessage(tbRecvMsg, "[OS]:Local UDP 监听已成功启动");

        }
        /// 【接收信息】
        private void ReceiveMessage()
        {
            IPEndPoint remoteIpep = new IPEndPoint(IPAddress.Any, 0);//接收任何来源的信息
            while (true)
            {
                byte[] byteRecv = udpcRecv.Receive(ref remoteIpep);
                string message = ByteArrayToHexStringNoBlank(byteRecv);

                Console.WriteLine(message);//
                //ShowMessage(tbRecvMsg, "[Local Recv]:" + remoteIpep.Address + ":" + remoteIpep.Port + "=" + message);

            }
        }
       /// 【发送消息】
        /// <param name="obj"></param>
        private void SendMessage(object obj)
        {
            string[] data = (string[])obj;

            //字符串转16进制
            //byte[] sendBytes = HexStringToByteArray(data[0]);
            byte[] sendBytes = {0x30,0x31,0x32};

            //发送到远程IP和端口
            IPEndPoint remoteIpep = new IPEndPoint(IPAddress.Parse(data[1]), Convert.ToInt32(data[2]));

            udpcSend.Send(sendBytes, sendBytes.Length, remoteIpep);

            //ShowMessage(tbRecvMsg, "[Local Send]:" + data[0]);
            //ResetTextBox(tbSendMsg);
        }
    /// <summary>
    /// 【字节数组转换成十六进制字符串(不带空格)】
    ///  20181021
    /// </summary>
    /// <param name="data">要转换的字节数组</param>
    /// <returns>转换后的字符串</returns>
        public string ByteArrayToHexStringNoBlank(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0') + " ");
            return sb.ToString().ToUpper();
        }   
       /// <summary>
        /// 【字符串转16进制】
        ///  20181021
       /// </summary>
        /// <param name="s"></param>
       /// <returns></returns>
        public byte[] HexStringToByteArray(string s)
        {
            if (s.Length == 0)
                throw new Exception("字符串长度为0");
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i<s.Length; i += 2)
                buffer[i / 2] = Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }

        public string localFilePath = "", fileNameExt = "", newFileName = "", FilePath = "";

        private void btn_Load_Click(object sender, EventArgs e)
        {
            XmlRW myXml = new XmlRW();
            myXml.ReadXML(file_read_path);//读取 
            panel1_load();

            panel2_load();

            // TabControl1_load();
            TabControl_load();
        }

        public string file_read_path;

        private void btn_ReadFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.ShowDialog();
            //获得文件路径
            localFilePath = file.FileName.ToString();
            lb_CurrenFiile.Font = new Font("宋体", 14, FontStyle.Regular);
            lb_CurrenFiile.Text = localFilePath;
            file_read_path = localFilePath;
        }

        /*********** panel1相关操作 *******************/
        TableLayoutPanel table1 = new TableLayoutPanel();
        
        private void panel1_load()
        {
            table1.Controls.Clear();
            table1.RowCount = 0;
            table1.ColumnCount = 0;

            // 默认添加一行数据
            table1.Dock = DockStyle.Top;     //顶部填充
            table1.ColumnCount = 4;          //
            table1.Height = table1.RowCount * 30; //1行table的整体高
            table1.Width = table1.ColumnCount * 150;//1行table的整体宽
            panel1.Controls.Add(table1);



            table1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, table1.Width * 0.20f));    //利用百分比计算，0.20f表示占用本行长度的25%  
            table1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, table1.Width * 0.20f));
            table1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, table1.Width * 0.20f));
            table1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, table1.Width * 0.20f));

            //根据process_num对GlobalConstants.Xml_list去重
            List<string>  List_Distinct = GlobalConstants.Xml_list.Select(t => t.process_num).Distinct().ToList();

            for (int i = 0; i < List_Distinct.Count; i++)  //显示
            {
                table1_AddRow(List_Distinct[i].ToString());
                //有问题！！  添加进度条
                panel1.AutoScroll = false;
                panel1.HorizontalScroll.Enabled = false;
                panel1.HorizontalScroll.Visible = false;
                panel1.HorizontalScroll.Maximum = 0;
                panel1.AutoScroll = true;
            }
        }
        private void table1_AddRow(string process_num)
        {
            try
            {
                // 动态添加一行
                table1.RowCount++;//table里面有多少行
                //设置高度,边框线也算高度，所以将40修改大一点
                table1.Height = table1.RowCount * 30;
                // 行高
                table1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30));
                // 设置cell样式，增加线条
                //table.CellBorderStyle = TableLayoutPanelCellBorderStyle.OutsetPartial;

                int i = table1.RowCount - 1;

                ////流程编号  第一列
                Label lb_process_name = new Label();
                lb_process_name.Text = "流程" + process_num;
                lb_process_name.Font = new Font("宋体", 11, FontStyle.Regular);
                table1.Controls.Add(lb_process_name, 0, i);


                //开始按钮  第二列
                Button btn_sta = new Button();
                btn_sta.Text = "开始";
                btn_sta.Font = new Font("宋体", 11, FontStyle.Regular);
                btn_sta.Click += Btn_Sta_Click;
                table1.Controls.Add(btn_sta, 1, i);

                //下一个按钮  第三列
                Button btn_next = new Button();
                btn_next.Text = "下一个";
                btn_next.Font = new Font("宋体", 11, FontStyle.Regular);
                btn_next.Click += Btn_Next_Click;
                table1.Controls.Add(btn_next, 2, i);

                //重置按钮  第四列
                Button btn_reload = new Button();
                btn_reload.Text = "重置";
                btn_reload.Font = new Font("宋体", 11, FontStyle.Regular);
                btn_reload.Click += Btn_Reload_Click;
                table1.Controls.Add(btn_reload, 2, i);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.PadRight(30, ' '), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void Btn_Sta_Click(object sender, EventArgs e)  //开始按钮
        {
            Button Btn = sender as Button;
            tabControl2.SelectedIndex = table1.Controls.IndexOf(Btn) / 4 ;
        }
        private void Btn_Next_Click(object sender, EventArgs e)  //下一个按钮
        {
            Button Btn = sender as Button;
            tabControl2.SelectedIndex = table1.Controls.IndexOf(Btn) / 4 + 1;

        }
        private void Btn_Reload_Click(object sender, EventArgs e)  //重置按钮
        {
            Button Btn = sender as Button;
            tabControl2.SelectedIndex = table1.Controls.IndexOf(Btn) / 4 ;
        }
        /******************************************/


        /*********** panel2相关操作 *******************/
        TableLayoutPanel table2 = new TableLayoutPanel();
        private void panel2_load()
        {
            table2.Controls.Clear();
            table2.RowCount = 0;
            table2.ColumnCount = 0;

            // 默认添加一行数据
            table2.Dock = DockStyle.Top;     //顶部填充
            panel2.Controls.Add(table2);
            table2.ColumnCount = 1;          //1列  设置1行为5列     
            table2.Height = table2.RowCount * 30; //1行table的整体高
            table2.Width = table2.ColumnCount * 150;//1行table的整体宽

            table2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, table1.Width * 1f));    

            List<string> List_Distinct = GlobalConstants.Xml_list.Select(t => t.process_num).Distinct().ToList();

            for (int i = 0; i < List_Distinct.Count; i++)  //显示
            {
                table2_AddRow(List_Distinct[i].ToString());
                //有问题！！  添加进度条
                panel2.AutoScroll = false;
                panel2.HorizontalScroll.Enabled = false;
                panel2.HorizontalScroll.Visible = false;
                panel2.HorizontalScroll.Maximum = 0;
                panel2.AutoScroll = true;
            }


        }
        private void table2_AddRow(string process_num)
        {
            try
            {
                // 动态添加一行
                table2.RowCount++;//table里面有多少行
                //设置高度,边框线也算高度，所以将40修改大一点
                table2.Height = table1.RowCount * 30;
                // 行高
                table2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30));
                // 设置cell样式，增加线条
                //table.CellBorderStyle = TableLayoutPanelCellBorderStyle.OutsetPartial;

                int i = table2.RowCount - 1;

                
                Button btn_process_num = new Button();
                btn_process_num.Text = "流程" + process_num;
                btn_process_num.Font = new Font("宋体", 11, FontStyle.Regular);
                btn_process_num.Click += Btn_Process_Click;
                table2.Controls.Add(btn_process_num, 0, i);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.PadRight(30, ' '), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void Btn_Process_Click(object sender, EventArgs e)
        {
            int Current_Row_Num;
            Button Btn_add = sender as Button;
            Current_Row_Num = table2.Controls.IndexOf(Btn_add);//当前的button 在Table中第几行     0-流程1   1-流程2  2-流程3   以此类推  

            tabControl2.SelectedIndex = Current_Row_Num;

        }
        /**********************************************/


        /*********** TabControl1 *******************/

        private void TabControl_load()
        {
            tabControl2.SelectedIndex = 1;  //当前页的位置

            List<string> List_Distinct = GlobalConstants.Xml_list.Select(t => t.process_num).Distinct().ToList();

            //根据process_num对GlobalConstants.Xml_list进行分类
            var List_process = GlobalConstants.Xml_list.GroupBy(x => x.process_num);
            int List_process_num = 0;

            for (int i = 0; i < List_Distinct.Count; i++)  //有几个流程  进几次
            {
                TabPage Page = new TabPage();
                Page.Name = "Page" + i.ToString();
                Page.Text = "流程" + (i + 1).ToString();
                Page.TabIndex = i + 1;
                tabControl2.Controls.Add(Page);

                foreach (IGrouping<string, data_struct> item in List_process)
                {
                    if (List_process_num== i)
                    {
                        TabControl_page_load(Page,item.ToList());//有几个流程就返回几个List
                    }
                    List_process_num++;
                }

                List_process_num = 0;
            }
        }
        private void TabControl_page_load(TabPage Page, List<data_struct> curren_list)
        {
            //1.对curren_list去重
            var List_new = curren_list.Select(m => new {m.ctrl_box_num,m.device_kind,m.device_num }).Distinct().ToList();

            for (int i = 0; i < List_new.Count; i++)
            {
                Label lb_name = new Label();
                lb_name.Text = "控制盒" + List_new[i].ctrl_box_num + List_new[i].device_kind + List_new[i].device_num;
                lb_name.Location = new Point(20, 20 + 40 * i);
                Page.Controls.Add(lb_name);

                if (List_new[i].device_kind == "电灯" || List_new[i].device_kind == "射灯" || List_new[i].device_kind == "电磁门")
                {
                    Button btn_on = new Button();
                    btn_on.Text = "开";
                    btn_on.Tag = Page.TabIndex.ToString() + "-" + List_new[i].ctrl_box_num + "-" + List_new[i].device_kind + "-" + List_new[i].device_num+ "-" + "开";
                    btn_on.Location = new Point(130, 15 + 40 * i);
                    btn_on.Width = 80;
                    btn_on.Click += Btn_On_Click;
                    Page.Controls.Add(btn_on);
                    
                    Button btn_off = new Button();
                    btn_off.Text = "关";
                    btn_off.Tag = Page.TabIndex.ToString() + "-" + List_new[i].ctrl_box_num + "-" + List_new[i].device_kind + "-" + List_new[i].device_num + "-" + "关";
                    btn_off.Location = new Point(250, 15 + 40 * i);
                    btn_off.Width = 80;
                    btn_off.Click += Btn_Off_Click;
                    Page.Controls.Add(btn_off);
                }
                else if (List_new[i].device_kind == "播放器")
                {
                    Button start_play = new Button();
                    start_play.Text = "开始播放";
                    start_play.Tag = Page.TabIndex.ToString() + "-" + List_new[i].ctrl_box_num + "-" + List_new[i].device_kind + "-" + List_new[i].device_num + "-" + "开始播放";
                    start_play.Location = new Point(130, 15 + 40 * i);
                    start_play.Width = 80;
                    start_play.Click += Start_Play_Click;
                    Page.Controls.Add(start_play);

                    Button end_play = new Button();
                    end_play.Text = "停止播放";
                    end_play.Tag = Page.TabIndex.ToString() + "-" + List_new[i].ctrl_box_num + "-" + List_new[i].device_kind + "-" + List_new[i].device_num + "-" + "停止播放";
                    end_play.Location = new Point(250, 15 + 40 * i);
                    end_play.Width = 80;
                    end_play.Click += End_Play_Click;
                    Page.Controls.Add(end_play);
                }
                else if (List_new[i].device_kind == "延时")
                {
                    Button start_delay = new Button();
                    start_delay.Text = "开始";
                    start_delay.Tag = Page.TabIndex.ToString() + "-" + List_new[i].ctrl_box_num + "-" + List_new[i].device_kind + "-" + List_new[i].device_num + "-" + "开始";
                    start_delay.Location = new Point(130, 15 + 40 * i);
                    start_delay.Width = 80;
                    start_delay.Click += Start_Delay_Click;
                    Page.Controls.Add(start_delay);

                    Button end_delay = new Button();
                    end_delay.Text = "停止";
                    end_delay.Tag = Page.TabIndex.ToString() + "-" + List_new[i].ctrl_box_num + "-" + List_new[i].device_kind + "-" + List_new[i].device_num + "-" + "停止";
                    end_delay.Location = new Point(250, 15 + 40 * i);
                    end_delay.Width = 80;
                    end_delay.Click += End_Delay_Click;
                    Page.Controls.Add(end_delay);
                }
            }
        }
        private void Btn_On_Click(object sender, EventArgs e)
        {
            Button Btn = sender as Button;
            Console.WriteLine(Btn.Tag.ToString());
        }
        private void Btn_Off_Click(object sender, EventArgs e)
        {
            Button Btn = sender as Button;
            Console.WriteLine(Btn.Tag.ToString());
        }
        private void Start_Play_Click(object sender, EventArgs e)
        {
            Button Btn = sender as Button;
            Console.WriteLine(Btn.Tag.ToString());
        }
        private void End_Play_Click(object sender, EventArgs e)
        {
            Button Btn = sender as Button;
            Console.WriteLine(Btn.Tag.ToString());
        }
        private void Start_Delay_Click(object sender, EventArgs e)
        {
            Button Btn = sender as Button;
            Console.WriteLine(Btn.Tag.ToString());
        }
        private void End_Delay_Click(object sender, EventArgs e)
        {
            Button Btn = sender as Button;
            Console.WriteLine(Btn.Tag.ToString());
        }
        /**********************************************/
    }

    public class XmlRW
    {
        //WriteXml 完成对User的添加操作
        //FileName 当前xml文件的存放位置
        //step- 
        //kind1-流程编号
        //kind2-控制盒编号
        //kind3-器件类型
        //kind4-器件编号
        //kind5-器件动作

        XmlDocument myDoc = new XmlDocument();//初始化XML文档操作类

        public void WriteXML_init(string FileName)
        {
            myDoc.Load(FileName); //加载XML文件       
        }
        public void WriteXML_write(string step, string kind1, string kind2, string kind3, string kind4, string kind5)
        {
            XmlNode newElem = myDoc.CreateNode("element", step, ""); //添加节点 User要对应我们xml文件中的节点名字

            XmlElement ele1 = myDoc.CreateElement("流程编号");//添加属性名称
            XmlText text1 = myDoc.CreateTextNode(kind1);//添加属性的值
            newElem.AppendChild(ele1);            //在节点中添加元素
            newElem.LastChild.AppendChild(text1);


            XmlElement ele2 = myDoc.CreateElement("控制盒编号");//添加属性名称
            XmlText text2 = myDoc.CreateTextNode(kind2);//添加属性的值
            newElem.AppendChild(ele2);
            newElem.LastChild.AppendChild(text2);


            XmlElement ele3 = myDoc.CreateElement("器件类型");//添加属性名称
            XmlText text3 = myDoc.CreateTextNode(kind3);//添加属性的值
            newElem.AppendChild(ele3);
            newElem.LastChild.AppendChild(text3);

            XmlElement ele4 = myDoc.CreateElement("器件编号");//添加属性名称
            XmlText text4 = myDoc.CreateTextNode(kind4);//添加属性的值
            newElem.AppendChild(ele4);
            newElem.LastChild.AppendChild(text4);

            XmlElement ele5 = myDoc.CreateElement("器件动作");//添加属性名称
            XmlText text5 = myDoc.CreateTextNode(kind5);//添加属性的值
            newElem.AppendChild(ele5);
            newElem.LastChild.AppendChild(text5);

            //将节点添加到文档中
            XmlElement root = myDoc.DocumentElement;
            root.AppendChild(newElem);
        }
        public void WriteXML_save(string FileName)
        {
            //保存
            myDoc.Save(FileName);
        }
        public void ReadXML(string FileName) //返回读到了几组  及每组 的内容
        {
            string process_num, ctrl_box_num, device_kind, device_num, device_action;

            GlobalConstants.Xml_list.Clear();

            XmlDocument doc = new XmlDocument();
            doc.Load(FileName);

            XmlNode root_node = doc.SelectSingleNode("测试");//读取根节点
            XmlNodeList xnl0 = root_node.ChildNodes; //

            foreach (XmlNode xn1 in xnl0)
            {
                //// 将节点转换为元素，便于得到节点的属性值
                XmlElement xe = (XmlElement)xn1;

                //// 得到属性值
                process_num = xe.ChildNodes[0].InnerText.ToString();
                ctrl_box_num = xe.ChildNodes[1].InnerText.ToString();
                device_kind = xe.ChildNodes[2].InnerText.ToString();
                device_num = xe.ChildNodes[3].InnerText.ToString();
                device_action = xe.ChildNodes[4].InnerText.ToString();

                GlobalConstants.Xml_list.Add(new data_struct(process_num, ctrl_box_num, device_kind, device_num, device_action));


            }
        }
    }
    public class data_struct
    {
        public data_struct(string process_num, string ctrl_box_num, string device_kind, string device_num, string device_action)
        {
            this.process_num = process_num;
            this.ctrl_box_num = ctrl_box_num;
            this.device_kind = device_kind;
            this.device_num = device_num;
            this.device_action = device_action;
        }
        public string process_num { get; set; }
        public string ctrl_box_num { get; set; }
        public string device_kind { get; set; }
        public string device_num { get; set; }
        public string device_action { get; set; }
    }
    public class GlobalConstants  //全局变量都在这儿
    {
        public static List<data_struct> Xml_list = new List<data_struct>();

        public Dictionary<int, data_struct> Xml_Dictionary = new Dictionary<int, data_struct>();
    }
}
