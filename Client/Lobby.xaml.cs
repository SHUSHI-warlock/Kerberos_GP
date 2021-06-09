using Client.core.Services;
using Client.MsgTrans;
using Client.Utils.LogHelper;
using Newtonsoft.Json;
using Panuon.UI.Silver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;


namespace Client
{
    /// <summary>
    /// Lobby.xaml 的交互逻辑
    /// </summary>
    public partial class Lobby : Window
    {
        private static Logger logger = Logger.GetLogger();  //日志记录
        private static MessageShow messageShowWin = MessageShow.GetInstance();

        private MyTcpClient client;

        private List<RoomInfo> allRoomInfos;
        private BindingList<RoomInfo> roomInfos;
        private BindingList<User> users;
        private BindingList<ChatMsg> lobbyMsgs;
        public Player player;
        //private GameRoom gameRoom;


        /// <summary>
        /// 房间对象
        /// </summary>
        public GameRoom gameRoom;

        /// <summary>
        /// 创建房间的信息
        /// </summary>
        public static RoomInfo createRoom;

        /// <summary>
        /// 消息读取器的开关
        /// </summary>
        private volatile bool canStop = false;

        private DispatcherTimer ShowTimer;  //时间刷新
        public Lobby(MyTcpClient client, User user)
        {
            InitializeComponent();

            this.client = client;
            player = new Player(user.userId);
            player.userState = UserState.online;

            allRoomInfos = new List<RoomInfo>();
            roomInfos = new BindingList<RoomInfo>();
            users = new BindingList<User>();
            lobbyMsgs = new BindingList<ChatMsg>();
            RoomList.ItemsSource = roomInfos;
            UserList.ItemsSource = users;
            LobbyChat.ItemsSource = lobbyMsgs;
            //player = new Player("lzh");
            //gameRoom = new GameRoom();


        }

        /// <summary>
        /// 刷新时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ShowCurTimer(object sender, EventArgs e)
        {
            ShowTime();
        }
        private void ShowTime()
        {
            //获得年月日获得时分秒
            this.NowTime.Content = DateTime.Now.ToString("yyyy/MM/dd ") + DateTime.Now.ToString(" HH:mm:ss");   //yyyy/MM/dd
        }

        /// <summary>
        /// 加载完
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Init();

            messageShowWin.Show();

            //显示欢迎用户
            Welcome_user.Content = player.userId;

            //显示时间
            ShowTime();    //在这里窗体加载的时候不执行文本框赋值，窗体上不会及时的把时间显示出来，而是等待了片刻才显示了出来
            ShowTimer = new System.Windows.Threading.DispatcherTimer();
            ShowTimer.Tick += new EventHandler(ShowCurTimer);//起个Timer一直获取当前时间
            ShowTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            ShowTimer.Start();

            //开启读取线程
            StartRead();

            //messageShowWin.SenderText.Text = "你大爷";
        }

        /// <summary>
        /// 测试用！
        /// </summary>
        public void Init()
        {
            //player = new Player("lzh");

            users.Add(player);
            users.Add(new User("wbc", UserState.prepared));
            users.Add(new User("ljh", UserState.online));
            users.Add(new User("ltl", UserState.online));
            users.Add(new User("lyx", UserState.offline));

            allRoomInfos.Add(new RoomInfo(0, "1", 4, 2, -1, 0));
            allRoomInfos.Add(new RoomInfo(1, "我带你们打asdafasf", 6, 2, -1, 1));
            allRoomInfos.Add(new RoomInfo(2, "5", 4, 2, -1, 0));
            allRoomInfos.Add(new RoomInfo(3, "2", 4, 0, 1, 1));
            allRoomInfos.Add(new RoomInfo(4, "4", 4, 2, -1, 0));
            allRoomInfos.Add(new RoomInfo(5, "进这个房间可以游戏", 2, 0, 1, 0));
            allRoomInfos.Add(new RoomInfo(6, "3", 4, 2, -1, 1));
            allRoomInfos.Add(new RoomInfo(7, "7", 4, 2, -1, 0));

            allRoomInfos[0].addPlayer(new Player("wl", 1, 233));
            allRoomInfos[0].addPlayer(new Player("wbc", 4, 233));


            Player temp = new Player("wbc", 1, 5);
            temp.userState = UserState.prepared;
            allRoomInfos[5].addPlayer(temp);


            lobbyMsgs.Add(new ChatMsg(1,"lzh", new DateTime(2021, 5, 20, 13, 36, 24), "喂喂喂！"));
            lobbyMsgs.Add(new ChatMsg(1,"lzh", new DateTime(2021, 5, 20, 13, 36, 24), "呦呦呦，这不是摇摆羊吗?几天不见这么拉了？"));
            lobbyMsgs.Add(new ChatMsg(1,"lzh", new DateTime(2021, 5, 20, 13, 36, 24), "喂喂喂！"));

            ShowRoomInfo();
        }

        public void StartRead()
        {
            //开一个读线程读取数据
            Thread thread = new Thread(new ParameterizedThreadStart(ReciveHandle));
            canStop = false;
            thread.IsBackground = true; //主线程退出时自动结束
            thread.Start(client);
        }
        
        public void StopRead()
        {
            canStop = true;
        }

        /// <summary>
        /// 循环接收消息
        /// </summary>
        /// <param name="Client"></param>
        public void ReciveHandle(object Client)
        {
            MyTcpClient tcpClient = Client as MyTcpClient;

            while (!canStop)
            {
                Message message = tcpClient.Recive();
                if(message==null)
                {
                    logger.Warn("message为空！");
                    continue;
                }
                switch (message.MessageType)
                {
                    case 2://服务器返回请求房间信息
                        RefreshRoomsHandler(message);
                        break;
                    case 3:
                        CreateRoomHandler(message);
                        break;
                    case 4:
                        EnterRoomHandler(message);
                        break;
                    case 5:
                        MsgHandler(message);
                        break;
                    default:
                        //
                        break;
                }
            }
        }

        #region 客户端处理服务器消息函数

        /// <summary>
        /// 处理服务器返回的服务器消息
        /// </summary>
        /// <param name="message"></param>
        public void RefreshRoomsHandler(Message message)
        {
            if (message.StateCode == 0)
            {
                logger.Info("大厅信息已收到，准备刷新！");
                string temp = message.bodyToString();
                LobbyMsg lobbyMsg = JsonConvert.DeserializeObject<LobbyMsg>(temp);
                if (lobbyMsg == null)
                {
                    logger.Info("大厅信息解析失败！");
                    return;
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    allRoomInfos = lobbyMsg.roomInfos;
                    ShowRoomInfo();
                    ShowUserInfo(lobbyMsg.users);
                    //通知
                    Notice.Show(string.Format("刷新成功！\n当前：房间数：{0} 大厅人数：{1}", roomInfos.Count, users.Count), "通知",3,MessageBoxIcon.Info);

                });
}
            else
            {
                logger.Error(string.Format("接收大厅信息出错！ 状态码:{0}", message.StateCode));
            }

        }
        /// <summary>
        /// 创建房间回复处理
        /// </summary>
        /// <param name="message"></param>
        public void CreateRoomHandler(Message message)
        {
            if (message.StateCode == 0)
            {//创建房间成功
                logger.Debug("创建房间成功！");
                //MessageBox.Show("创建房间成功！");
                //进入一个房间加载的界面

            }
            else
            {
                logger.Error("房间创建失败!");
                //MessageBox.Show("创建房间失败！");
            }

        }
        /// <summary>
        /// 加入房间回复处理
        /// </summary>
        /// <param name="message"></param>
        public void EnterRoomHandler(Message message)
        {
            if (message.StateCode == 0)
            {//加入房间成功！
                logger.Debug("加入房间成功，即将进入房间！");
                //提示框显示
                RoomInfo room = JsonConvert.DeserializeObject<RoomInfo>(message.bodyToString());

                ///新开线程执行进入房间操作
                ///因为如果不开线程的话，GameRoom的ShowDialog会阻塞大厅线程进而阻塞调用大厅函数的本线程
                ///导致进入房间后的所有消息（房间消息，游戏消息）都将堆积
                ///然后退出后，大厅的每一次操作，其实读到的是之前大厅读取线程积累的房间消息

                ///修复后
                ///新开线程，调用进入房间函数
                ///同时在本线程 StopRead() 等return后就会结束读取线程
                ///最后再在EnterRoom退出房间，ShowDiaLog阻塞结束后调用StartRead()重新开一个线程进行大厅消息处理
                Thread thread = new Thread(new ParameterizedThreadStart(EnterRoom));
                thread.IsBackground = true;    //后台线程不会阻止程序终止
                thread.Start(room);

                StopRead();

                logger.Debug("EnterRoomHandler 执行完毕！");
                
                return;
            }
            else if (message.StateCode == 1)
            {
                logger.Error("房间已经不存在！ error:501");
                //提示框显示
                MessageBox.Show("房间已经不存在了！\n error:501");
            }
            else if (message.StateCode == 2)
            {
                logger.Error("房间满人了！error:502");
                //提示框显示
                MessageBox.Show("房间满人了！\n error:502");
            }
            else if (message.StateCode == 3)
            {
                logger.Error("房间已经开始了！ error:503");
                //提示框显示
                MessageBox.Show("房间已经开始了！\n error:503");
            }
            else
            {
                logger.Error("房间加入失败！ error:未知错误");
                //提示框显示
                MessageBox.Show("房间加入失败！\n error:未知错误");
            }

            //结束之后都请求刷新一次
            QueryLobbyInfo();
        }
        /// <summary>
        /// 聊天消息处理
        /// </summary>
        /// <param name="message"></param>
        public void MsgHandler(Message message)
        {
            if (message.StateCode == 0)
            {//接收消息成功
                ChatMsg msg = JsonConvert.DeserializeObject<ChatMsg>(message.bodyToString());

                if (msg.Type == 1)
                {
                    logger.Debug("收到一条大厅消息！");
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        //请求
                        lobbyMsgs.Add(msg);
                    });
                }
                else
                {
                    logger.Error("消息类型错误！");
                }

            }
            else
            {
                logger.Error("报文状态错误！");
            }
        }
       
        #endregion

        #region 客户端请求函数

        /// <summary>
        /// 请求大厅信息
        /// </summary>
        public void QueryLobbyInfo()
        {
            Message message = new Message(4, 2, 0);
            client.Send(message);
        }
        /// <summary>
        /// 请求创建房间
        /// </summary>
        /// <param name="room"></param>
        public void QueryCreateRoom(RoomInfo room)
        {
            Message message = new Message(4, 3, 0);
            message.SetBody(room);
            client.Send(message);
        }
        /// <summary>
        /// 请求进入房间
        /// </summary>
        /// <param name="room"></param>
        public void QueryEnterRoom(RoomInfo room)
        {
            Message message = new Message(4, 4, 0);
            message.SetBody(room);
            client.Send(message);
        }
        /// <summary>
        /// 发送聊天消息
        /// </summary>
        /// <param name="msg"></param>
        public void SendChatMsg(ChatMsg msg)
        {
            Message message = new Message(4, 8, 0);
            message.SetBody(msg);
            client.Send(message);
        }

        #endregion

        /// <summary>
        /// 刷新房间页面
        /// </summary>
        private void ShowRoomInfo()
        {
            roomInfos.Clear();
            int filterNum = 0;
            foreach (RoomInfo room in allRoomInfos)
            {
                if (RoomFilter(room))
                {
                    filterNum++;
                    roomInfos.Add(room);
                }
            }
            RoomNum.Content = String.Format("{0}/{1}", filterNum, allRoomInfos.Count);
        }
        /// <summary>
        /// 过滤房间
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        private bool RoomFilter(RoomInfo room)
        {
            if ((bool)IsFull.IsChecked && IsRoomFull(room))
                return false;
            if ((bool)IsStart.IsChecked && IsRoomStart(room))
                return false;
            if ((bool)HasPassword.IsChecked && IsRoomHasPsw(room))
                return false;
            if (MaxPlayer.SelectedItem != null &&
                !MaxPlayer.Text.Equals("all") &&
                !MaxPlayer.Text.Equals(room.RoomMaxPlayer.ToString()))
                return false;


            if (KeyWords.Text == "" ||
                room.RoomId.ToString().Contains(KeyWords.Text) ||
                room.RoomName.Contains(KeyWords.Text))
                //搜索
                return true;

            return false;


        }
        private bool IsRoomFull(RoomInfo room)
        {
            return room.isFull();
        }
        private bool IsRoomStart(RoomInfo room)
        {
            return room.RoomState != 1;
        }
        private bool IsRoomHasPsw(RoomInfo room)
        {
            return room.HasPsw == 1;
        }
        /// <summary>
        /// 刷新用户页面
        /// </summary>
        private void ShowUserInfo(List<User> list)
        {
            users.Clear();
            foreach (User u in list)
                users.Add(u);
            playerNum.Content = string.Format("服务器人数:{0}人", users.Count);
        }

        /// <summary>
        /// 进入房间
        /// </summary>
        /// <param name="room"></param>
        private void EnterRoom(object o)
        {
            RoomInfo room = o as RoomInfo; 
            bool flag = false;
            foreach (Player p in room.players)
            {
                if (p == null) continue;
                if (p.userId == player.userId && p.userState == UserState.enter_room)
                {
                    player = p;
                    p.userState = UserState.unprepared;
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                logger.Error("传回的房间信息中没有自己!!!");
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                //进入前
                this.IsEnabled = false;
                messageShowWin.Hide();
                //StopRead();
                gameRoom = new GameRoom(client);
                gameRoom.SetPlayer(player);

                //进入
                gameRoom.ShowRoom(room);

                //退出后
                //gameRoom.ShowRoom(room,player);
                messageShowWin.Show();
                StartRead();
                this.IsEnabled = true;
            });
        }


        /// <summary>
        /// 窗口改变时算一下宽度，只变房间名的长度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RoomList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //logger.Info(sender.GetType().ToString());
            //ListView list = sender as ListView; 

            logger.Info("PreviousSize:" + e.PreviousSize.ToString());
            //logger.Info("NewSize:"+e.NewSize.ToString());

            foreach (GridViewColumn g in RoomGridView.Columns)
            {
                switch (g.Header)
                {
                    case "": g.Width = 30; break;
                    case "房间号": g.Width = 45; break;
                    case "房间名": g.Width = e.NewSize.Width - 290; break;
                    case "最大人数": g.Width = 60; break;
                    case "当前人数": g.Width = 60; break;
                    case "状态": g.Width = 80; break;
                }
            }

        }

        /// <summary>
        /// 房间过滤器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckedChanged(object sender, RoutedEventArgs e)
        {
            logger.Info(String.Format("过滤条件:房间未满：{0} 房间未开始：{1} 房间无密码：{2} ",
                (bool)IsFull.IsChecked ? "True" : "False", (bool)IsStart.IsChecked ? "True" : "False", (bool)HasPassword.IsChecked ? "True" : "False"));

            ShowRoomInfo();
        }
        private void KeyWords_TextChanged(object sender, TextChangedEventArgs e)
        {
            logger.Info("关键字过滤:" + ((TextBox)sender).Text);
            ShowRoomInfo();
        }
        private void MaxPlayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MaxPlayer.Text = ((ComboBoxItem)MaxPlayer.SelectedItem).Content.ToString();
            logger.Info("最大人数过滤:" + MaxPlayer.Text);

            ShowRoomInfo();
        }


        /// <summary>
        /// 禁止运行期间关闭
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //TODO 退出时发送报文
            player.userState = UserState.offline;
            //PlayerStateChanged(_player);
            Message message = new Message(4, 10, 0);
            client.Send(message);

            logger.Debug("退出大厅!");
            messageShowWin.Hide();
        }


        /// <summary>
        /// 退出点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            this.Close();
        }



        /// <summary>
        /// 刷新点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Refresh_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            //请求刷新
            logger.Debug("请求刷新!");

            QueryLobbyInfo();
            //showRoomInfo();
        }

        /// <summary>
        /// 弹出层对象
        /// </summary>
        public static NavigationWindow window = null;

        /// <summary>
        /// 使用NavigationWindow弹出页面
        /// </summary>
        private void ShowNavigationWindow(string title, string uri, int width, int height)
        {
            window = new NavigationWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Width = width;
            window.Height = height;
            window.Title = title;
            window.ResizeMode = ResizeMode.NoResize;
            window.Source = new Uri(uri, UriKind.Relative);
            window.ShowsNavigationUI = false;
            window.ShowDialog();
        }

        /// <summary>
        /// 进入房间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            //选中房间？
            if (RoomList.SelectedValue == null)
            {
                logger.Error("进入房间:未选中房间！");
                //提示框显示
                return;
            }


            RoomInfo room = (RoomInfo)RoomList.SelectedValue;

            if (room.PlayerNum == room.RoomMaxPlayer)
            {
                logger.Debug("选中的房间人已满！");
                //提示框显示

                return;
            }
            if (room.RoomState == 1)
            {
                logger.Debug("选中的房间已经开始游戏！");
                //提示框显示

                return;
            }

            //有密码？有则弹出密码框
            if (room.HasPsw == 1)
            {//弹出创建房间页面
                ShowNavigationWindow("输入房间密码", "PswInput.xaml", 350, 250);
                if (createRoom == null)
                {
                    logger.Debug(String.Format("用户取消进入房间！"));
                    return;
                }
                createRoom.RoomId = room.RoomId;
            }
            else
            {
                createRoom = new RoomInfo(room.RoomId, "");
            }

            //发送消息
            logger.Debug(String.Format("请求进入房间{0}!", room.RoomId));

            QueryEnterRoom(room);
        }

        /// <summary>
        /// 创建房间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            //弹出创建房间页面
            ShowNavigationWindow("创建房间", "CreateRoom.xaml", 600, 350);

            if (createRoom == null)
            {
                logger.Info("取消创建");
            }
            else
            {
                //RoomInfo room = window.
                logger.Debug(String.Format("创建房间:房间名：{0} 最大人数：{1} 密码：{2}", createRoom.RoomName, createRoom.RoomMaxPlayer, createRoom.RoomPsw));
                QueryCreateRoom(createRoom);
            }
        }

        private void SendMag_ButtonDown(object sender, RoutedEventArgs e)
        {
            if (SendMag.Text == "")
            {
                logger.Error(String.Format("消息为空！"));
                return;
            }

            ChatMsg msg = new ChatMsg(1,player.userId, DateTime.Now, SendMag.Text);
            //msg.Type = 1;   //大厅消息

            //发送出去
            logger.Debug("发送消息！");
            SendChatMsg(msg);

            //清空
            SendMag.Text = "";

            //请求
            lobbyMsgs.Add(msg);

        }

        
        
    }
}
