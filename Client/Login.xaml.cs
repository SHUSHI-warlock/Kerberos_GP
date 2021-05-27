using Client.core.Services;
using Client.MsgTrans;
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

            //连接AS

            //连接TGS

            //连接服务器
            String Server_Ip = System.Configuration.ConfigurationManager.AppSettings["ServerIp"];
            int Server_Port = int.Parse(System.Configuration.ConfigurationManager.AppSettings["ServerPort"]);
            client = new MyTcpClient(Server_Ip, Server_Port);
            

            client.Connect();

            user = new Player(id);
            Message message = new Message(1,1,0);
            message.SetBody(user);

            client.Send(message);
            //开启接收

            ServerAuth(client.Recive());
            
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
            Message message = msg as Message;
            
            if (message.MessageP2P==2&&message.MessageType==1&&message.StateCode==0)
            {
                //验证成功！
                //client.OnReceive -= ServerAuth;

                EnterLobby();
                //
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
    }
}
