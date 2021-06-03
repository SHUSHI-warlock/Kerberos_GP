using Client.core.Services;
using Client.Ker;
using Client.MsgTrans;
using Client.Utils;
using Client.Utils.DesUtil;
using Client.Utils.LogHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {
        private static Logger logger = Logger.GetLogger();  //日志记录
        
        private static MessageShow messageShowWin = MessageShow.GetInstance();

        private bool Logining;

        private MyTcpClient client;

        private User user;

        public Login()
        {
            InitializeComponent();
            Logining = false;
            messageShowWin.Show();
        }
        /// <summary>
        /// 登录
        /// </summary>
        public void TryLogin(String id, String pa)
        {
            Logining = true;
            MyClient mclient = new MyClient();
            //连接AS
            String ASServer_Ip = System.Configuration.ConfigurationManager.AppSettings["ASIp"];
            int ASServer_Port = int.Parse(System.Configuration.ConfigurationManager.AppSettings["ASPort"]);
            String TGSServer_Ip = System.Configuration.ConfigurationManager.AppSettings["TGSIp"];
            int TGSServer_Port = int.Parse(System.Configuration.ConfigurationManager.AppSettings["TGSPort"]);
            String Server_Ip = System.Configuration.ConfigurationManager.AppSettings["ServerIp"];
            int Server_Port = int.Parse(System.Configuration.ConfigurationManager.AppSettings["ServerPort"]);

            client = new MyTcpClient(ASServer_Ip, ASServer_Port);

            client.Connect();

            mclient.id = id;
            mclient.password = pa;
            //this.client.generateKey();
            Console.WriteLine(pa);
            mclient.generateKey(pa);
            mclient.IDtgs = "1";

            string s = string.Empty;
            byte[] kee = mclient.asKey.getKeyBytes();
            Console.WriteLine(String.Join(",", ByteConverter.UbyteToSbyte(kee)));
            sbyte[] keee = ByteConverter.UbyteToSbyte(kee);
            /*
            foreach (sbyte bb in keee)
            {
                s += bb.ToString();
                s += ",";
            }
            s = s.TrimEnd(',');
            Console.WriteLine(s);
            */



            //Console.WriteLine(Encoding.Default.GetString(mclient.asKey.getKeyBytes()));

            String mes = mclient.ASconfirm(id, "1");
            byte[] ba = Encoding.Default.GetBytes(mes);
            Console.WriteLine(mes);
            Message message1 = new Message(0, 0, 0);
            message1.SetBody(ba);
            //Console.WriteLine(message1.MessageP2P +"," +message1.MessageType + ","+ message1.StateCode);

            client.Send(message1);

            int sta = 0;
            enterAS(client.Recive(), mclient, sta);
            if (sta == 1)
            {
                MessageBox.Show("登陆失败");
                userPassword.Clear();
            }
            else if (sta == 0)
            {
                client.Close();
                client = new MyTcpClient(TGSServer_Ip, TGSServer_Port);

                client.Connect();

                mclient.IDv = "1";
                String ADc = "127.0.0.1";
                if (ADc.Length < 16)
                {
                    for (int i = ADc.Length; i < 16; i++)
                    {
                        ADc += " ";
                    }
                }

                byte[] mess = mclient.TGSconfirm(mclient.IDv, mclient.tgsTicket, mclient.id, ADc, mclient.tgsKey);
                Message message2 = new Message(2, 0, 0);
                message2.SetBody(mess);
                client.Send(message2);
                sta = 0;
                enterTGS(client.Recive(), mclient, sta);
                if (sta == 1)
                {
                    MessageBox.Show("登陆失败");
                    userPassword.Clear();
                }
                else if (sta == 0)
                {
                    client.Close();
                    //连接服务器
                    client = new MyTcpClient(Server_Ip, Server_Port);

                    client.Connect();

                    Message messageV = new Message(4, 0, 0);
                    messageV.SetBody(mclient.Vconfirm(mclient.vTicket, mclient.id, ADc, mclient.vKey));

                    client.Send(messageV);
                    //开启接收

                    //服务器认证
                    int stc = enterV(client.Recive(), mclient);

                    if (stc == 0)
                    {
                        //验证成功！
                        //client.OnReceive -= ServerAuth;
                        ///后续消息加密传输
                        ///假设全为1
                        //DESUtils des = new DESUtils(new DesKey(new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 }));
                        client.DesOpen(new DESUtils(mclient.vKey));



                        EnterLobby(new User(id));
                    }
                    else
                    {
                        logger.Debug("TryLogin:服务器验证失败！");
                        client.Close();
                    }
                }
            }
        }

        public void enterAS(Message message, MyClient mclient, int sta)
        {
            sta = 0;
            if (message.StateCode != 0)
            {
                //出错！
                logger.Info(String.Format("登录应用服务器失败！ 状态码：%d", message.StateCode));
                MessageBox.Show(String.Format("登录应用服务器失败！ 状态码：%d", message.StateCode));
                switch (message.StateCode)
                {
                    case 1:
                        //Console.WriteLine("IDc不在数据库中");
                        MessageBox.Show("IDc不在数据库中");
                        break;
                    case 2:
                        //Console.WriteLine("IDtgs不存在");
                        MessageBox.Show("IDtgs不存在");
                        break;
                    case 3:
                        //Console.WriteLine("时钟不同步");
                        MessageBox.Show("时钟不同步");
                        break;
                }
                //重新回到登录
                //loginInput(ctx);
                return;
            }

            //String mes=message.bodyToString();
            byte[] mess = message.GetBody();

            sbyte[] messa = ByteConverter.UbyteToSbyte(mess);
            //System.out.println(Arrays.toString(mess));
            AuthenticationMessage au = new AuthenticationMessage();
            //Console.WriteLine("a");
            au.ASCMessage(mess, mclient.asKey);
            mclient.manageAS(au);
            //Console.WriteLine(Arrays.toString(client.tgsKey.getKeyBytes()));
            if (mclient.VerifyAS(au))
            {
                logger.Info("用户AS验证成功!");
                MessageBox.Show("用户AS验证成功!");
            }
            else
            {
                sta = 1;
                logger.Info("验证失败");
                MessageBox.Show("验证失败！");
            }
        }

        public void enterTGS(Message message, MyClient mclient, int sta)
        {
            sta = 0;
            if (message.StateCode != 0)
            {
                logger.Info(String.Format("登录应用服务器失败！ 状态码：%d", message.StateCode));
                switch (message.StateCode)
                {
                    case 1:
                        //Console.WriteLine("IDv不存在");
                        MessageBox.Show("IDv不存在");
                        break;
                    case 2:
                        //Console.WriteLine("票据错误");
                        MessageBox.Show("票据错误");
                        break;
                    case 3:
                        //Console.WriteLine("票据过期");
                        MessageBox.Show("票据过期");
                        break;
                    case 4:
                        //Console.WriteLine("身份验证失败");
                        MessageBox.Show("身份验证失败");
                        break;
                    default:
                        break;
                }
                //重新回到登录
                //loginInput(ctx);
                return;
            }
            //String mes = message.bodyToString();
            byte[] mess = message.GetBody();

            AuthenticationMessage au = new AuthenticationMessage();
            au.TGSCMessage(mess, mclient.tgsKey);
            mclient.manageTGS(au);
            if (mclient.VerifyTGS(au))
            {
                logger.Info("用户TGS验证成功!");
                MessageBox.Show("用户TGS验证成功!");
            }
            else
            {
                sta = 1;
                logger.Info("验证失败");
                MessageBox.Show("验证失败！");
            }
        }

        public int enterV(Message message, MyClient mclient)
        {
            if (message.StateCode != 0)
            {
                logger.Info(String.Format("登录应用服务器失败！ 状态码：%d", message.StateCode));
                switch (message.StateCode)
                {
                    case 1:
                        logger.Debug("V返回:票据错误");
                        MessageBox.Show("票据错误");
                        break;
                    case 2:
                        logger.Debug("V返回:票据过期");
                        MessageBox.Show("票据过期");
                        break;
                    case 3:
                        logger.Debug("V返回:身份验证失败");
                        MessageBox.Show("身份验证失败");
                        break;
                    case 4:
                        logger.Debug("V返回:用户已在其他地方登录");
                        MessageBox.Show("用户已在其他地方登录");
                        break;
                    default:
                        break;
                }
                return 1;
            }
            byte[] ts = message.GetBody();
            AuthenticationMessage au = new AuthenticationMessage();
            au.VCMessage(ts, mclient.vKey);
            //判断返回时间是否加一
            if (au.TS.Equals(mclient.VTime))
            {//时间相同
                logger.Debug("V返回:服务器认证成功");
                MessageBox.Show("服务器认证成功");
                return 0;
            }
            else
            {//服务器认证失败
                logger.Debug("V返回:认证服务器失败");
                MessageBox.Show("认证服务器失败");
                return 1;
            }
        }

        //服务器验证登录
        public void ServerAuth(Message msg)
        {
            Logining = false;
            if (msg==null)
            {
                logger.Debug("登录失败！");
                client.Close();
                return;
            }
            
            
            if (msg.MessageP2P==5&&msg.MessageType==0)
            {
                if (msg.StateCode == 0)
                {
                    //验证成功！
                    //client.OnReceive -= ServerAuth;
                    ///后续消息加密传输
                    ///假设全为1
                    DESUtils des = new DESUtils(new DesKey(new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 }));
                    client.DesOpen(des);

                    EnterLobby();
                    //
                }
                else if(msg.StateCode==1)
                {
                    logger.Debug("用户已在其他地方登录！");
                    client.Close();
                }
            }
            else
            {
                logger.Debug("登录失败！");

                client.Close();
            }
        }

        /// <summary>
        /// 进入大厅
        /// </summary>
        public void EnterLobby()
        {
            //进入大厅界面
            logger.Debug("登录成功！进入大厅！");
            this.Hide();
            messageShowWin.Hide();
            //大厅界面
            Lobby lobby = new Lobby(client,user);

            App.Current.MainWindow = lobby;

            lobby.ShowDialog();
            messageShowWin.Show();
            this.Show();
        }

        /// <summary>
        /// 转到注册
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Regist_MouseDown(object sender, MouseButtonEventArgs e)
        {
            logger.Info("进入注册界面！");
            loginPage.Visibility = Visibility.Hidden;
            registPage.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 注册界面返回登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void returnBtn_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("返回登录界面！");
            registPage.Visibility = Visibility.Hidden;
            loginPage.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 注册提交
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void registBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("注册成功！");
        }

        /// <summary>
        /// 登录提交
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loginBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!Logining)
            {
                if (userId.Text == "" || userPassword.Password == "")
                    return;
                TryLogin(userId.Text, userPassword.Password);
            }

            //MessageBox.Show("登录成功！");


        }

        /// <summary>
        /// 退出时，把消息也退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            //messageShowWin.Close();
            Environment.Exit(0);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            Console.WriteLine("按钮已禁止");
        }
    }
}
