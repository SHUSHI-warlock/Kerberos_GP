using Client.core.Services;
using Client.Ker;
using Client.MsgTrans;
using Client.Utils;
using Client.Utils.DesUtil;
using Client.Utils.LogHelper;
using Client.Utils.RSAUtil;
using Newtonsoft.Json;
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
        private bool Registing;

        private DESUtils registDes;

        private MyTcpClient client;

        private static String ASServer_Ip = System.Configuration.ConfigurationManager.AppSettings["ASIp"];
        private static int ASServer_Port = int.Parse(System.Configuration.ConfigurationManager.AppSettings["ASPort"]);
        private static String TGSServer_Ip = System.Configuration.ConfigurationManager.AppSettings["TGSIp"];
        private static int TGSServer_Port = int.Parse(System.Configuration.ConfigurationManager.AppSettings["TGSPort"]);
        private static String Server_Ip = System.Configuration.ConfigurationManager.AppSettings["ServerIp"];
        private static int Server_Port = int.Parse(System.Configuration.ConfigurationManager.AppSettings["ServerPort"]);

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
            client = new MyTcpClient(ASServer_Ip, ASServer_Port);

            if (!client.Connect())
            {
                logger.Info("连接AS失败！");
                //加弹窗
                return;
            }

            mclient.id = id;
            mclient.password = pa;
            //this.client.generateKey();
            Console.WriteLine(pa);

            //生成秘钥

            //mclient.generateKey(pa);
            ///测试
            mclient.asKey = new DesKey(new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 });

            mclient.IDtgs = "1";

            //生成发送给AS的信息
            String mes = mclient.ASconfirm(id, "1");
            byte[] ba = Encoding.Default.GetBytes(mes);
            Console.WriteLine(mes);
            //生成消息报文
            Message message1 = new Message(0, 0, 0);
            message1.SetBody(ba);

            client.Send(message1);

            //等待AS回复
            int sta = enterAS(client.Recive(), mclient);

            if (sta == 1)
            {
                MessageBox.Show("登陆失败");
                userPassword.Clear();
            }
            else if (sta == 0)
            {
                client.Close();

                client = new MyTcpClient(TGSServer_Ip, TGSServer_Port);

                if (!client.Connect())
                {
                    logger.Info("连接TGS失败！");
                    //加弹窗
                    return;
                }

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
                
                int stb = enterTGS(client.Recive(), mclient);
                if (stb == 1)
                {
                    MessageBox.Show("登陆失败");
                    userPassword.Clear();
                }
                else if (stb == 0)
                {
                    client.Close();
                    //连接服务器
                    client = new MyTcpClient(Server_Ip, Server_Port);

                    if (!client.Connect())
                    {
                        logger.Info("连接Server失败！");
                        //加弹窗
                        return;
                    }
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
                    }
                }
            }

            client.Close();
        }

        public int enterAS(Message message, MyClient mclient)
        {
            if (message.StateCode != 0)
            {
                //出错！
                logger.Info(String.Format("登录应用服务器失败！ 状态码：{0}", message.StateCode));
                MessageBox.Show(String.Format("登录应用服务器失败！ 状态码：{0}", message.StateCode));
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
                return 1;
            }

            //String mes=message.bodyToString();
            byte[] mess = message.GetBody();

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
                return 0;
            }
            else
            {
                logger.Info("验证失败");
                MessageBox.Show("验证失败！");
                return 1;
            }
        }

        public int enterTGS(Message message, MyClient mclient)
        {
            if (message.StateCode != 0)
            {
                logger.Info(String.Format("登录应用服务器失败！ 状态码：{0}", message.StateCode));
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
                return 1;

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
                return 0;
            }
            else
            {
                logger.Info("验证失败");
                MessageBox.Show("验证失败！");
                return 1;
            }
        }

        public int enterV(Message message, MyClient mclient)
        {
            if (message.StateCode != 0)
            {
                logger.Info(String.Format("登录应用服务器失败！ 状态码：{0}", message.StateCode));
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
        
        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pa"></param>
        public bool TryRegist(string id,string pa)
        {
            Message message = new Message(0, 3, 0);

            if (id.Length < 20)
            {
                StringBuilder sb = new StringBuilder(id);
                for (int i = id.Length; i < 20; i++)
                    sb.Append(" ");
                id = sb.ToString();
            }
            if (pa.Length < 20)
            {
                StringBuilder sb = new StringBuilder(pa);
                for (int i = pa.Length; i < 20; i++)
                    sb.Append(" ");
                pa = sb.ToString();
            }

            message.SetBody(byteManage.concat(Encoding.UTF8.GetBytes(id), Encoding.UTF8.GetBytes(pa)));

            client.Send(message);

            Message res = client.Recive();

            if(res.StateCode==0)
            {
                logger.Info("注册成功！");
                return true;
            }
            else
            {//注册失败！
                logger.Error("注册失败！");
                return false;
            }
        }

        /// <summary>
        /// 进入大厅
        /// </summary>
        public void EnterLobby(User user)
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
            client = new MyTcpClient(ASServer_Ip, ASServer_Port);
            if (!client.Connect())
            {
                logger.Info("AS连接失败！");
                return;
            }
            //请求建立连接
            Message message = new Message(0, 1, 0);

            client.Send(message);

            Message res = client.Recive();

            PublicKey pk = JsonConvert.DeserializeObject<PublicKey>(res.bodyToString());
            if(pk==null)
            {
                logger.Info("AS建立连接失败！");
                return;
            }

            //第二次握手
            //DesKey desKey = new DesKey(new byte[]{ 221, 212, 1, 157, 1, 174, 1, 14 });
            DesKey desKey = new DesKey();
            desKey.GenKey();
            
            registDes = new DESUtils(desKey);

            Message message2 = new Message(0, 2, 0);


            byte[] temp = desKey.getKeyBytes();
            message2.SetBody(RSAUtils.Encryption(pk, temp));

            client.Send(message2);

            Message res2 = client.Recive();
            //解密服务器返回消息
            string msg = Encoding.UTF8.GetString(registDes.Decryption( res2.GetBody()));

            if(msg.Equals("connection formed"))
            {
                logger.Info("进入注册界面！");
                loginPage.Visibility = Visibility.Hidden;
                registPage.Visibility = Visibility.Visible;
                registId.Text = "";
                registPassword.Password = "";
                rePassword.Password = "";
            }
            else
            {
                logger.Info("服务器连接2建立失败！");
                MessageBox.Show("服务器连接2建立失败！");
                client.Close();
            }
        }

        /// <summary>
        /// 注册界面返回登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void returnBtn_Click(object sender, RoutedEventArgs e)
        {
            Message message = new Message(0, 4, 0);
            client.Send(message);
            client.Close();

            logger.Info("返回登录界面！");
            registPage.Visibility = Visibility.Hidden;
            loginPage.Visibility = Visibility.Visible;
            Registing = false;

        }

        /// <summary>
        /// 注册提交
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void registBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!Registing)
            {
                if (registId.Text == "" || registPassword.Password == "")
                {
                    logger.Info("账号密码不能为空！");
                    MessageBox.Show("账号密码不能为空！");

                    return;
                }
                if(!registPassword.Password.Equals(rePassword.Password))
                {
                    logger.Info("前后两次密码输入不一致！");
                    MessageBox.Show("前后两次密码输入不一致！");

                    return;
                }
            
                if(TryRegist(registId.Text,registPassword.Password))
                {
                    MessageBox.Show("注册成功！");

                    client.Close();

                    logger.Info("返回登录界面！");
                    registPage.Visibility = Visibility.Hidden;
                    loginPage.Visibility = Visibility.Visible;
                }
                else
                {
                    registId.Text = "";
                    registPassword.Password = "";
                    rePassword.Password = "";
                    MessageBox.Show("注册失败：用户已存在！");
                }
            }

            Registing = false;

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
