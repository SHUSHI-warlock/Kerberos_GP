//#define TEST

using Client.core;
using Client.core.Services;
using Client.MsgTrans;
using Client.Utils.LogHelper;
using Client.Utils.UIHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Client
{
    public enum GameState
    {
        Exit = 0,
        Enter = 1,
        Start = 2,
        Prepared = 3,
        UnPrepared = 4,
        Move = 5,
        Choose = 6,
        Moving = 7,

        Submit = 11,    //提交过后的等待

        Waiting = 8,
        ShowMove = 9,
        Ending = 10,
    }

    /// <summary>
    /// GameRoom.xaml 的交互逻辑
    /// </summary>
    public partial class GameRoom : Window
    {
        private static Logger logger = Logger.GetLogger();  //日志记录
        private DispatcherTimer ShowTimer;                  //时间刷新

        private static MessageShow messageShowWin = MessageShow.GetInstance();

        private MyTcpClient client;
        
        /// <summary>
        /// 游戏步数
        /// </summary>
        private int steps { get; set; }

        private GameState roomState;
        private RoomInfo roomInfo;
        private Player _player;
        private static CheckerCore gameCore = new CheckerCore();

        /// <summary>
        /// UI显示帮助
        /// </summary>
        private Utils.UIHelper.GameUIhelper UIHelper;

        private BindingList<ChatMsg> roomMsgs;

        public GameRoom(MyTcpClient client)
        {
            InitializeComponent();
            //gameCore = new CheckerCore(new Utils.UIHelper.GameUIhelper(this));
            UIHelper = new Utils.UIHelper.GameUIhelper(this);
            gameCore.SetUIHelper(UIHelper);
            roomState = GameState.Exit;
            this.client = client;
            roomMsgs = new BindingList<ChatMsg>();
            RoomChat.ItemsSource = roomMsgs;

            //显示时间
            ShowTime();    //在这里窗体加载的时候不执行文本框赋值，窗体上不会及时的把时间显示出来，而是等待了片刻才显示了出来
            ShowTimer = new System.Windows.Threading.DispatcherTimer();
            ShowTimer.Tick += new EventHandler(ShowCurTimer);//起个Timer一直获取当前时间
            ShowTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            ShowTimer.Start();

            //开一个读线程读取数据
            Thread thread = new Thread(new ParameterizedThreadStart(ReciveHandle));
            thread.IsBackground = true; //主线程退出时自动结束
            thread.Start(client);

        }

        /// <summary>
        /// 打开房间界面
        /// </summary>
        /// <param name="room">房间详细信息</param>
        public void ShowRoom(RoomInfo room)
        {
            ChangedState(GameState.Enter);
            SetRoomInfo(room);
            logger.Debug("房间加载完毕！即将显示！");
            ChangedState(GameState.UnPrepared);

            this.ShowDialog();
        }

        /// <summary>
        /// 加载界面、全屏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Normal;//还原窗口（非最小化和最大化）
            this.WindowStyle = System.Windows.WindowStyle.None; //仅工作区可见，不显示标题栏和边框
            this.ResizeMode = System.Windows.ResizeMode.NoResize;//不显示最大化和最小化按钮
            //this.Topmost = true;    //窗口在最前

            this.Left = 0.0;
            this.Top = 0.0;
            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;

            //显示
            messageShowWin.Show();
        }

        /// <summary>
        /// 玩家进来
        /// </summary>
        /// <param name="player"></param>
        public void SetPlayer(Player player)
        {
            this._player = player;
        }


        /// <summary>
        /// 循环接收消息
        /// </summary>
        /// <param name="Client"></param>
        public void ReciveHandle(object Client)
        {
            MyTcpClient tcpClient = Client as MyTcpClient;

            while (true)
            {
                Message message = tcpClient.Recive();
                switch (message.MessageType)
                {
                    //case 5:
                    //    MsgHandler(message);
                    //    break;
                    case 6:
                        PlayerStateHandler(message);
                        break;
                    case 7:
                        GameMsgHandler(message);
                        break;
                    case 8:
                        PlayerMoveHandler(message);
                        break;
                    default:
                        //
                        break;
                }
            }
        }


        #region 消息处理

        /// <summary>
        /// 用户状态改变
        /// </summary>
        public void PlayerStateHandler(Message message)
        {
            Player p = JsonConvert.DeserializeObject<Player>(message.bodyToString());

            if (_player.roomId == p.roomId)
            {//在同一个房间内
                logger.Debug("收到玩家状态改变报文！");
                Application.Current.Dispatcher.Invoke(() => {
                    PlayerStateChanged(p);
                    //显示
                });
               
            }
            else
            {
                logger.Error("玩家和用户根本不在一个房间内！！！");
                return;
            }
        }

        /// <summary>
        /// 游戏内通知
        /// </summary>
        /// <param name="message"></param>
        public void GameMsgHandler(Message message)
        {
            GameMsg msg = JsonConvert.DeserializeObject<GameMsg>(message.bodyToString());
            
            if (msg.gameState == 0)
            {//开始游戏
                logger.Info("游戏开始！");
                Application.Current.Dispatcher.Invoke(() => {
                    AddRoomNews(Brushes.Green, string.Format("游戏开始！"));
                    ChangedState(GameState.Start);
                    //显示
                });

                msg.gameState = 2;
            }
            //游戏开始后还会指定第一个玩家

            if (msg.gameState == 1)
            {//游戏结束
                logger.Info("游戏结束！");
                Application.Current.Dispatcher.Invoke(() => {
                    AddRoomNews(Brushes.Green, string.Format("游戏结束！"));
                    ChangedState(GameState.Ending);
                    //显示
                });
            }
            else if (msg.gameState == 2)
            {//设置当前移动的玩家
                if(roomInfo.players[msg.pos]==null)
                {
                    logger.Warn("下一个移动的玩家为空！");
                    return;
                }
                logger.Info(string.Format("当前移动的是{0}号玩家{1}", msg.pos, roomInfo.players[msg.pos].userId));
                Application.Current.Dispatcher.Invoke(() => {
                    roomInfo.players[msg.pos].userState = UserState.move;
                    BindPlayer(roomInfo.players[msg.pos]);

                    if (gameCore.getCurPlayer() > msg.pos)
                        steps++;

                    gameCore.setCurPlayer(msg.pos);

                    if (msg.pos == _player.pos)
                    {
                        ChangedState(GameState.Move);
                        AddRoomNews(Brushes.Green, string.Format("轮到你移动了！"));
                    }
                    else
                    {
                        AddRoomNews(Brushes.Green, string.Format("轮到{0}号玩家移动了！", msg.pos));
                        //开一个计时器！？
                    }
                    //显示

                });
            }
            else
            {//接收其他玩家移动结果
                if (msg.pos != gameCore.getCurPlayer())
                {
                    logger.Error(string.Format("当前不属于{}号玩家的移动回合！", msg.pos));
                    return;
                }
                logger.Info(string.Format("接收到{0}号玩家{1}的移动结果：{2}-->{3}",
                    msg.pos, roomInfo.players[msg.pos].userId, msg.s.ToString(), msg.e.ToString()));

                Application.Current.Dispatcher.Invoke(() =>
                {
                    gameCore.ShowMove(msg.pos, msg.s, msg.e);
                    roomInfo.players[gameCore.getCurPlayer()].userState = UserState.wait;
                    BindPlayer(roomInfo.players[gameCore.getCurPlayer()]);

                });
            }
        }

        /// <summary>
        /// 返回的移动结果
        /// </summary>
        /// <param name="message"></param>
        public void PlayerMoveHandler(Message message)
        {
            if (message.StateCode == 0)
            {
                logger.Info(string.Format("返回：提交的移动有效"));
                Application.Current.Dispatcher.Invoke(() => {
                    gameCore.SubmitMove();
                    ChangedState(GameState.Waiting);
                    //显示
                    AddRoomNews(Brushes.Green, string.Format("玩家移动有效！"));
                });
            }
            else if (message.StateCode == 1)
            {
                logger.Info(string.Format("返回：当前不是你的移动回合"));
                Application.Current.Dispatcher.Invoke(() => {
                    gameCore.ClearMove();
                    ChangedState(GameState.Waiting);
                    //显示
                    AddRoomNews(Brushes.Green, string.Format("当期不是你的移动回合！"));
                });
            }
            else if (message.StateCode == 2)
            {
                logger.Info(string.Format("返回：提交移动无效！"));
                Application.Current.Dispatcher.Invoke(() => {
                    gameCore.ClearMove();
                    ChangedState(GameState.Move);
                    //显示
                    AddRoomNews(Brushes.Red, string.Format("玩家移动无效！"));
                });
            }
            else if (message.StateCode == 3)
            {
                logger.Info(string.Format("返回：移动超时"));
                Application.Current.Dispatcher.Invoke(() => {
                    gameCore.ClearMove();
                    ChangedState(GameState.Waiting);
                    //显示
                    AddRoomNews(Brushes.Red, string.Format("玩家移动超时！"));
                });
            }
        }

        public void PlayerStateChanged(Player p)
        {
            string msg;
            if (p.userState == UserState.enter_room)//玩家进入房间
            {logger.Debug(string.Format("玩家{0}进入房间！", p.userId));
                roomInfo.addPlayer(p);
                BindPlayerNum();
                p.userState = UserState.unprepared;
                msg = string.Format("玩家 {0} 进入房间！", p.userId);
            }
            else if (p.userState == UserState.exit_room)//玩家离开房间
            {logger.Debug(string.Format("玩家{0}离开房间！", p.userId));
                roomInfo.removePlayer(p);
                BindPlayerNum();
                msg = string.Format("玩家 {0} 离开房间！", p.userId);
            }
            else if (p.userState == UserState.prepared)//玩家准备
            {logger.Debug(string.Format("玩家 {0} 准备！", p.userId));
                roomInfo.players[p.pos].userState = UserState.prepared;
                msg = string.Format("玩家 {0} 准备！", p.userId);
            }
            else if (p.userState == UserState.unprepared)//玩家取消准备
            {logger.Debug(string.Format("玩家 {0} 取消准备！", p.userId));
                roomInfo.players[p.pos].userState = UserState.prepared;
                msg = string.Format("玩家 {0} 取消准备！", p.userId);
            }
            else
            {logger.Error("传回了异常玩家信息！");
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                BindPlayer(p);
                //显示
                AddRoomNews(Brushes.Red, msg);
            });

        }

        public void SendChatMsg(ChatMsg msg)
        {
            Message message = new Message(1, 8, 0);
            message.SetBody(msg);
            client.Send(message);
        }

        public void SubmitPlayerMove(CheckerPoint s,CheckerPoint e)
        {
            Message message = new Message(1, 9, 0);
            GameMsg msg = new GameMsg(3, _player.pos, s, e);
            message.SetBody(msg);
            client.Send(message);
        }

        #endregion


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

        private void BindRoomInfo()
        {
            if(roomInfo.HasPsw==-1)
                Room_lock.Visibility = Visibility.Hidden;
            else
                Room_lock.Visibility = Visibility.Visible;
            Text_RoomName.Text = roomInfo.RoomName;
            Text_RoomId.Text = roomInfo.RoomId.ToString();
            Text_Pos.Text = _player.pos.ToString();
            BindPlayerNum();
        }
        private void BindPlayerNum()
        {
            Text_Player.Text = string.Format("{0}/{1}", roomInfo.PlayerNum, roomInfo.RoomMaxPlayer);
        }

        /// <summary>
        /// 进入房间之前先要设置房间信息
        /// </summary>
        /// <param name="room"></param>
        public void SetRoomInfo(RoomInfo room)
        {
            roomInfo = room;

            //显示人员位置
            for (int i = 1; i <= 6; i++)
            {
                Border playerBorder = (this.FindName("Player_" + i)) as Border;
                playerBorder.Visibility = Visibility.Hidden;
            }

            if (room.RoomMaxPlayer == 6)
            {
                Player_3.Visibility = Visibility.Visible;
                Player_6.Visibility = Visibility.Visible;
            }
            if (room.RoomMaxPlayer == 4 || room.RoomMaxPlayer == 6)
            {
                Player_2.Visibility = Visibility.Visible;
                Player_5.Visibility = Visibility.Visible;
            }
            if (room.RoomMaxPlayer == 2 || room.RoomMaxPlayer == 4 || room.RoomMaxPlayer == 6)
            {
                Player_1.Visibility = Visibility.Visible;
                Player_4.Visibility = Visibility.Visible;
            }
            else
            {
                logger.Fatal("房间传入的RoomInfo中MaxPlayer有误！");
                return;
            }

            //绑定人员信息
            if (room.players != null)
                for (int i = 1; i < 7; i++)
                    if (room.players[i] != null)
                        BindPlayer(room.players[i]);
                    else
                        BindPlayerDefault(i);

            //显示
            BindRoomInfo();

            AddRoomNews(Brushes.Red, "房间初始化成功！");

            //PlayerStateChanged(client);
        }

        /// <summary>
        /// 显示玩家信息
        /// </summary>
        /// <param name="player"></param>
        public void BindPlayer(Player player)
        {
            if (player == null)
                return;

            Label temp;
            temp = FindName(String.Format("Player{0}_Id", player.pos)) as Label;
            temp.Content = player.userId;
            temp = FindName(String.Format("Player{0}_State", player.pos)) as Label;

            if (player.userState == UserState.prepared) temp.Content = "已准备";
            else if (player.userState == UserState.unprepared) temp.Content = "未准备";
            else if (player.userState == UserState.complete) temp.Content = "完成";
            else if (player.userState == UserState.move) temp.Content = "走棋中";
            else if (player.userState == UserState.wait) temp.Content = "等待中";
            else logger.Error("BindPlayer:玩家信息错误！");

        }
        public void BindPlayerDefault(int i)
        {
            Label temp;
            temp = FindName(String.Format("Player{0}_Id", i)) as Label;
            temp.Content = "玩家" + i;
            temp = FindName(String.Format("Player{0}_State", i)) as Label;
            temp.Content = "等待加入";
        }

        /// <summary>
        /// 添加一条房间消息
        /// </summary>
        public void AddRoomNews(Brush color,string msg)
        {
            DateTime date = DateTime.Now;
            Run time = new Run(String.Format("{0}:{1}:{2} ", date.Hour, date.Minute, date.Second));
            time.Foreground = Brushes.Black;
            RoomNews.Inlines.Add(time);
            Run r = new Run(string.Format("{0}\n", msg));
            r.Foreground = color;
            RoomNews.Inlines.Add(r);
        }
        public void ClearRoomNews()
        {
            RoomNews.Inlines.Clear();
        }
        
        
        /// <summary>
        /// 判断游戏是否能开始
        /// </summary>
        /// <returns></returns>
        private bool CanStart()
        {
            if (roomInfo.PlayerNum == roomInfo.RoomMaxPlayer)
            {
                foreach (Player p in roomInfo.players)
                    if (p != null && p.userState == UserState.unprepared)
                        return false;
                return true;
            }
            return false;
        }

        public void ShowGameOver()
        {
            //显示成绩结果
            StringBuilder sb = new StringBuilder();
            sb.Append("对局结果：\n");
            for(int i=1;i<=roomInfo.PlayerNum;i++)
            {
                sb.Append(string.Format("{0,20}|{1,4}", roomInfo.players[i].userId, roomInfo.players[i].steps));
            }
            AddRoomNews(Brushes.Yellow, sb.ToString());

            ChangedState(GameState.UnPrepared);
        }

        /// <summary>
        /// 聊天发送按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            if (SendMag.Text == "")
            {
                logger.Error(String.Format("消息为空！"));
                return;
            }

            ChatMsg msg = new ChatMsg(_player.userId, DateTime.Now, SendMag.Text);
            msg.Type = 2;   //房间消息

            //发送出去
            logger.Debug("发送消息！");
            SendChatMsg(msg);

            //清空
            SendMag.Text = "";

            //请求
            roomMsgs.Add(msg);
        }

        /// <summary>
        /// 准备按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_prepare_Click(object sender, RoutedEventArgs e)
        {
            string msg;
            if (roomState == GameState.Prepared)
            {
                ChangedState(GameState.UnPrepared);

                Message message = new Message(1, 7, 0);
                message.SetBody(_player);
                client.Send(message);

                //显示
                msg = string.Format("玩家 {0} 取消准备！", _player.userId);
            }
            else
            {
                ChangedState(GameState.Prepared);

                Message message = new Message(1, 6, 0);
                message.SetBody(_player);
                client.Send(message);

                //显示
                msg = string.Format("玩家 {0} 取消准备！", _player.userId);
            }
            BindPlayer(_player);
            //显示
            AddRoomNews(Brushes.Red, msg);
        }
        /// <summary>
        /// 退出按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_exit_Click(object sender, RoutedEventArgs e)
        {
            messageShowWin.Hide();
            this.Close();
        }

        /// <summary>
        /// 禁止运行期间关闭
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //TODO 退出时发送报文
            //_player.userState = UserState.exit_room;
            ChangedState(GameState.Exit);
            //PlayerStateChanged(_player);
            Message message = new Message(1, 5, 0);
            client.Send(message);

           

        }

        /// <summary>
        /// 棋盘左键点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Board_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point point = e.GetPosition((Canvas)sender);
            CheckerPoint checkerPoint = GameUIhelper.GetLogicPoint(point);
            if (checkerPoint == null)
            {
                logger.Info("点击界面：" + point.ToString() + " 逻辑坐标:无");
                return;
            }
            logger.Info("点击界面：" + point.ToString() + " 逻辑坐标:" + checkerPoint.ToString());

            //点击逻辑
            switch (roomState)
            {
                case GameState.Move:
                    if (gameCore.ChooseChecker(checkerPoint, _player.pos))
                    {
                        gameCore.SetStartPoint(checkerPoint);


                        //显示可走的路径

                        ChangedState(GameState.Choose);
                    }
                    break;
                case GameState.Choose:
                    if (gameCore.CanMoveNext(checkerPoint))
                    {
                        //当前的棋子显示
                        //UIHelper.MovePostion(gameCore.GetCurrentPoint(), checkerPoint);
                        gameCore.MoveNext(checkerPoint);

                        //上一步路径显示
                        //UIHelper.ShowLastRoad();

                        //显示当前可走的路径

                        ChangedState(GameState.Moving);
                    }
                    else if (gameCore.ChooseChecker(checkerPoint, _player.pos))
                    {//换一个点
                        //取消路径显示

                        //取消选择
                        //UIHelper.MovePostion(gameCore.GetCurrentPoint(), gameCore.GetStartPoint());
                        //重新选择
                        gameCore.SetStartPoint(checkerPoint);

                        //选中的棋子显示
                        //UIHelper.ShowMark(checkerPoint);

                        //显示可走的路径


                        ChangedState(GameState.Choose);
                    }
                    break;
                case GameState.Moving:
                    if (checkerPoint.Equals(gameCore.GetCurrentPoint()))
                    {//再次点击确定
                        //发送报文
                        SubmitPlayerMove(gameCore.GetStartPoint(), gameCore.GetCurrentPoint());
                        ChangedState(GameState.Submit);
                    }
                    else if (gameCore.CanMoveNext(checkerPoint))
                    {
                        //当前的棋子显示
                        //UIHelper.AddRoad(checkerPoint);

                        //UIHelper.MovePostion(gameCore.GetCurrentPoint(), checkerPoint);
                        gameCore.MoveNext(checkerPoint);

                        //上一步路径显示
                        //UIHelper.ShowLastRoad();


                        //显示当前可走的路径


                    }
                    else if (gameCore.ChooseChecker(checkerPoint, _player.pos))
                    {//换一个点

                        //重新选择
                        gameCore.SetStartPoint(checkerPoint);

                        //显示可走的路径

                        ChangedState(GameState.Choose);
                    }
                    break;
                default:
                    logger.Info("当期状态点击无效！");
                    break;
            }

        }
        /// <summary>
        /// 棋盘右键点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Board_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //右键撤回
            if (gameCore.GetCurrentPoint().Equals(gameCore.GetStartPoint()))
            {
                //取消选择
                gameCore.ClearMove();
                ChangedState(GameState.Move);
            }
            else
            {
                gameCore.ReturnMove();
                if (gameCore.GetCurrentPoint().Equals(gameCore.GetStartPoint()))
                    ChangedState(GameState.Choose);
            }
        }

        /// <summary>
        /// 确定，提交按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_submit_Click(object sender, RoutedEventArgs e)
        {
            //获取起点终点
            //发送报文
            SubmitPlayerMove(gameCore.GetStartPoint(), gameCore.GetCurrentPoint());
            ChangedState(GameState.Submit);
#if TEST
            gameCore.SubmitMove();

            ChangedState(GameState.Move);
#endif
        }
        /// <summary>
        /// 撤回按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_return_Click(object sender, RoutedEventArgs e)
        {
            //
            if (gameCore.GetCurrentPoint().Equals(gameCore.GetStartPoint()))
            {
                //取消选择
                //
                gameCore.ClearMove();
                ChangedState(GameState.Move);
            }
            else
            {
                gameCore.ReturnMove();
                if (gameCore.GetCurrentPoint().Equals(gameCore.GetStartPoint()))
                    ChangedState(GameState.Choose);
            }
        }
        /// <summary>
        /// 取消选择按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_cancel_Click(object sender, RoutedEventArgs e)
        {
            if(roomState == GameState.Choose|| roomState == GameState.Moving)
            {
                logger.Info("用户取消选择！");
                gameCore.ClearMove();
                ChangedState(GameState.Move);
            }
        }


        /// <summary>
        /// 状态转换
        /// </summary>
        /// <param name="newState"></param>
        public void ChangedState(GameState newState)
        {
            //判断现在的状态
            switch (roomState)
            {
                case GameState.Exit:
                    {
                        if (newState != GameState.Enter)
                        {
                            logger.Error("状态错误！从" + roomState.ToString() + "到" + newState.ToString());
                            return;
                        }
                        else
                        {
                            logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开Exit状态要做的事

                            //进入Enter状态要做的事
                        }
                        break;
                    }
                case GameState.Enter:
                    {
                        if (newState != GameState.UnPrepared)
                        {
                            logger.Error("状态错误！从" + roomState.ToString() + "到" + newState.ToString());
                            return;
                        }
                        else
                        {
                            logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开Enter状态要做的事
                            gameControl.Visibility = Visibility.Hidden;
                            roomControl.Visibility = Visibility.Visible;

                        }
                        break;
                    }
                case GameState.UnPrepared:
                    {
                        if (newState == GameState.Exit)
                        {
                            logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开UnPrepared状态要做的事

                        }
                        else if (newState == GameState.Prepared)
                        {
                            logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开UnPrepared状态要做的事

                        }
                        else
                        {
                            logger.Error("状态错误！从" + roomState.ToString() + "到" + newState.ToString());
                            return;
                        }
                        break;
                    }
                case GameState.Prepared:
                    {
                        if (newState == GameState.UnPrepared)
                        {
                            logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开Prepared状态要做的事

                        }
                        else if (newState == GameState.Start)
                        {
                            logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开Prepared状态要做的事
                            roomControl.Visibility = Visibility.Hidden;
                            gameControl.Visibility = Visibility.Visible;

                        }
                        else
                        {
                            logger.Error("状态错误！从" + roomState.ToString() + "到" + newState.ToString());
                            return;
                        }
                        break;
                    }
                case GameState.Start:
                    {
                        if (newState == GameState.Waiting)
                        {
                            //离开Start状态要做的事
                            gameControl.IsEnabled = true;

                        }
                        else if (newState == GameState.Move)
                        {
                            logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开Start状态要做的事
                            gameControl.IsEnabled = true;

                        }
                        else
                        {
                            logger.Error("状态错误！从" + roomState.ToString() + "到" + newState.ToString());
                            return;
                        }
                        break;
                    }
                case GameState.Move:
                    {
                        if (newState == GameState.Choose)
                        {
                            logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开旧状态要做的事

                        }
                        else if (newState == GameState.Waiting)
                        {
                            logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());

                        }
                        else
                        {
                            logger.Error("状态错误！从" + roomState.ToString() + "到" + newState.ToString());
                            return;
                        }
                        break;
                    }
                case GameState.Choose:
                    {
                        if (newState == GameState.Move)
                        {
                            logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开旧状态要做的事

                            //进入新状态要做的事
                            Button_submit.IsEnabled = false;
                            Button_cancel.IsEnabled = false;
                            Button_return.IsEnabled = false;
                        }
                        if (newState == GameState.Moving)
                        {
                            logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开旧状态要做的事

                        }
                        else if (newState == GameState.Waiting)
                        {
                            logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //中途超时

                        }
                        else
                        {
                            logger.Error("状态错误！从" + roomState.ToString() + "到" + newState.ToString());
                            return;
                        }
                        break;
                    }
                case GameState.Moving:
                    {
                        if (newState == GameState.Choose)
                        {
                            logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开旧状态要做的事


                        }
                        else if (newState == GameState.Move)
                        {
                            logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开旧状态要做的事


                        }
                        else if (newState == GameState.Waiting)
                        {
                            logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //中途超时

                        }
                        else if (newState == GameState.Submit)
                        {
                            logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //提交了

                        }
                        else
                        {
                            logger.Error("状态错误！从" + roomState.ToString() + "到" + newState.ToString());
                            return;
                        }
                        break;
                    }
                case GameState.ShowMove:
                    {
                        if (newState == GameState.Waiting)
                        {
                            logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开旧状态要做的事


                        }
                        else
                        {
                            logger.Error("状态错误！从" + roomState.ToString() + "到" + newState.ToString());
                            return;
                        }
                        break;
                    }
                case GameState.Waiting:
                    {
                        if (newState == GameState.Move)
                        {
                            logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开旧状态要做的事
                            _player.userState = UserState.move;
                            BindPlayer(_player);
                        }
                        else if (newState == GameState.ShowMove)
                        {
                            logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开旧状态要做的事

                            //进入新状态要做的事

                        }
                        else if (newState == GameState.Ending)
                        {
                            logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开旧状态要做的事

                        }
                        else
                        {
                            logger.Error("状态错误！从" + roomState.ToString() + "到" + newState.ToString());
                            return;
                        }
                        break;
                    }
                case GameState.Ending:
                    {
                        if (newState == GameState.UnPrepared)
                        {
                            logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开旧状态要做的事
                            gameControl.Visibility = Visibility.Hidden;
                            roomControl.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            logger.Error("状态错误！从" + roomState.ToString() + "到" + newState.ToString());
                            return;
                        }
                        break;
                    }
                default:
                    break;
            }
            //状态变化
            roomState = newState;
            switch (roomState)
            {
                case GameState.Exit:
                    {
                        //清除消息
                        ClearRoomNews();

                        //解除绑定
                        UIHelper.BreakCheckerBind();

                        //进入Exit状态要做的事
                        _player.userState = UserState.online;
                        //BindPlayer(client);
                        roomInfo = null;
                        _player = null;
                        break;
                    }
                case GameState.Enter:
                    {
                        _player.userState = UserState.enter_room;
                        break;
                    }
                case GameState.UnPrepared:
                    {
                        //进入UnPrePaerd状态要做的事
                        _player.userState = UserState.unprepared;
                        BindPlayer(_player);

                        prepare_Text.Text = "准备";
                        Button_exit.IsEnabled = true;
                        break;
                    }
                case GameState.Prepared:
                    {
                        //进入Prepared状态要做的事
                        _player.userState = UserState.prepared;
                        BindPlayer(_player);

                        prepare_Text.Text = "取消准备";
                        Button_exit.IsEnabled = false;
                        break;
                    }
                case GameState.Start:
                    {
                        //进入Start状态要做的事
                        gameCore.startGame(roomInfo.PlayerNum);
                        steps = 0;
                        break;
                    }
                case GameState.Move:
                    {
                        Button_submit.IsEnabled = false;
                        Button_continue.IsEnabled = true;
                        Button_cancel.IsEnabled = false;
                        Button_return.IsEnabled = false;
                        break;
                    }
                case GameState.Choose:
                    {
                        //进入新状态要做的事
                        Button_submit.IsEnabled = false;
                        Button_cancel.IsEnabled = true;
                        Button_return.IsEnabled = false;
                        break;
                    }
                case GameState.Moving:
                    {
                        //进入新状态要做的事
                        Button_submit.IsEnabled = true;
                        Button_cancel.IsEnabled = true;
                        Button_return.IsEnabled = true;
                        break;
                    }
                case GameState.Submit:
                    {
                        Button_submit.IsEnabled = false;
                        Button_continue.IsEnabled = false;
                        Button_cancel.IsEnabled = false;
                        Button_return.IsEnabled = false;
                        break;
                    }
                case GameState.ShowMove:
                    {

                        break;
                    }
                case GameState.Waiting:
                    {
                        //进入Waiting状态要做的事
                        _player.userState = UserState.wait;
                        BindPlayer(_player);

                        Button_submit.IsEnabled = false;
                        Button_continue.IsEnabled = false;
                        Button_cancel.IsEnabled = false;
                        Button_return.IsEnabled = false;
                        break;
                    }
                case GameState.Ending:
                    {
                        //进入新状态要做的事
                        _player.userState = UserState.in_room;
                        ShowGameOver();
                        break;
                    }
                default:
                    break;
            }

        }

        
    }
}

