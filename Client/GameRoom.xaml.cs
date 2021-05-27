#define TEST

using Client.core;
using Client.core.Services;
using Client.Utils.LogHelper;
using Client.Utils.UIHelper;
using System;
using System.Collections.Generic;
using System.Linq;
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

        Waiting = 8,
        ShowMove = 9,
        Ending =10,
        

    }

    /// <summary>
    /// GameRoom.xaml 的交互逻辑
    /// </summary>
    public partial class GameRoom : Window
    {
        private static Logger logger = Logger.GetLogger();  //日志记录
        private DispatcherTimer ShowTimer;  //时间刷新

        private static MessageShow messageShowWin = MessageShow.GetInstance();

        private GameState roomState;
        private RoomInfo roomInfo;
        private Player client;
        private static CheckerCore gameCore = new CheckerCore();

        private Utils.UIHelper.GameUIhelper UIHelper;

        public GameRoom()
        {
            InitializeComponent();
            //gameCore = new CheckerCore(new Utils.UIHelper.GameUIhelper(this));
            UIHelper = new Utils.UIHelper.GameUIhelper(this);
            gameCore.SetUIHelper(UIHelper);
            roomState = GameState.Exit;

            //显示时间
            ShowTime();    //在这里窗体加载的时候不执行文本框赋值，窗体上不会及时的把时间显示出来，而是等待了片刻才显示了出来
            ShowTimer = new System.Windows.Threading.DispatcherTimer();
            ShowTimer.Tick += new EventHandler(ShowCurTimer);//起个Timer一直获取当前时间
            ShowTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            ShowTimer.Start();
        }

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
        /// 进入房间之前先要设置房间信息
        /// </summary>
        /// <param name="room"></param>
        public void SetRoomInfo(RoomInfo room)
        {
            roomInfo = room;

            //显示人员位置
            for(int i = 1; i <= 6; i++){
                Border playerBorder = (this.FindName("Player_" + i)) as Border; 
                playerBorder.Visibility = Visibility.Hidden;
            }
            
            if(room.RoomMaxPlayer == 6) {
                Player_3.Visibility = Visibility.Visible;
                Player_6.Visibility = Visibility.Visible;
            }
            if(room.RoomMaxPlayer == 4|| room.RoomMaxPlayer == 6){
                Player_2.Visibility = Visibility.Visible;
                Player_5.Visibility = Visibility.Visible;
            }
            if (room.RoomMaxPlayer == 2 || room.RoomMaxPlayer == 4 || room.RoomMaxPlayer == 6){
                Player_1.Visibility = Visibility.Visible;
                Player_4.Visibility = Visibility.Visible;
            }
            else{
                logger.Fatal("房间传入的RoomInfo中MaxPlayer有误！");
                return;
            }
            
            //绑定人员信息
            if(room.players!=null)
                foreach (Player p in room.players)
                    if(p!=null)
                        BindPlayer(p);

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
            
            if (     player.userState == UserState.prepared)    temp.Content = "已准备";
            else if (player.userState == UserState.unprepared)  temp.Content = "未准备";
            else if (player.userState == UserState.complete)    temp.Content = "完成";
            else if (player.userState == UserState.move)        temp.Content = "走棋中";
            else if (player.userState == UserState.wait)        temp.Content = "等待中";
            else logger.Error("BindPlayer:玩家信息错误！");
           
        }
        
        /// <summary>
        /// 玩家引用放进来
        /// </summary>
        /// <param name="player"></param>
        public void SetPlayer(Player player)
        {
            this.client = player;
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
                case GameState.Exit:{
                    if (newState != GameState.Enter)
                    {logger.Error("状态错误！从" + roomState.ToString() + "到" + newState.ToString());
                        return;
                    }
                    else 
                    {logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开Exit状态要做的事

                            //进入Enter状态要做的事
                        }
                    break; }
                case GameState.Enter:{
                    if (newState != GameState.UnPrepared)
                    {logger.Error("状态错误！从" + roomState.ToString() + "到" + newState.ToString());
                        return;
                    }
                    else
                    {logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                        //离开Enter状态要做的事
                        gameControl.Visibility = Visibility.Hidden;
                        roomControl.Visibility = Visibility.Visible;
 
                    }
                    break;
                    }
                case GameState.UnPrepared:{
                        if(newState == GameState.Exit)
                        {logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开UnPrepared状态要做的事

                        }
                        else if(newState == GameState.Prepared)
                        {logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开UnPrepared状态要做的事

                        }
                        else
                        {logger.Error("状态错误！从" + roomState.ToString() + "到" + newState.ToString());
                            return;
                        }
                        break;
                    }
                case GameState.Prepared:{
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
                case GameState.Start:{
                        if (newState == GameState.Waiting)
                        {
                            //离开Start状态要做的事
                            gameControl.IsEnabled = true;

                        }
                        else if(newState == GameState.Move)
                        {logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开Start状态要做的事
                            gameControl.IsEnabled = true;
                            
                        }
                        else
                        {logger.Error("状态错误！从" + roomState.ToString() + "到" + newState.ToString());
                            return;
                        }
                        break;
                    }
                case GameState.Move:{
                        if (newState == GameState.Choose)
                        {logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开旧状态要做的事

                        }
                        else if (newState == GameState.Waiting)
                        {logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            
                        }
                        else
                        {logger.Error("状态错误！从" + roomState.ToString() + "到" + newState.ToString());
                            return;
                        }
                        break;
                    }
                case GameState.Choose:{
                        if(newState == GameState.Move)
                        {logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开旧状态要做的事

                            //进入新状态要做的事
                            Button_submit.IsEnabled = false;
                            Button_cancel.IsEnabled = false;
                            Button_return.IsEnabled = false;
                        }
                        if (newState == GameState.Moving)
                        {logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开旧状态要做的事

                        }
                        else if (newState == GameState.Waiting)
                        {logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //中途超时
                            
                        }
                        else
                        {logger.Error("状态错误！从" + roomState.ToString() + "到" + newState.ToString());
                            return;
                        }
                        break;
                    }
                case GameState.Moving:{
                        if (newState == GameState.Choose)
                        {logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开旧状态要做的事


                        }
                        else if (newState == GameState.Move)
                        {logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
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
                case GameState.ShowMove:{
                        if (newState == GameState.Waiting)
                        {logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开旧状态要做的事


                        }
                        else
                        { logger.Error("状态错误！从" + roomState.ToString() + "到" + newState.ToString());
                            return;
                        }
                        break;
                    }
                case GameState.Waiting:{
                        if (newState == GameState.Move)
                        {logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开旧状态要做的事

                        }
                        else if (newState == GameState.ShowMove)
                        {logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开旧状态要做的事

                            //进入新状态要做的事

                        }
                        else if(newState == GameState.Ending)
                        {logger.Info("状态转换！从" + roomState.ToString() + "到" + newState.ToString());
                            //离开旧状态要做的事

                        }
                        else
                        {logger.Error("状态错误！从" + roomState.ToString() + "到" + newState.ToString());
                            return;
                        }
                        break;
                    }
                case GameState.Ending:{
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
                        //进入Exit状态要做的事
                        client.userState = UserState.exit_rome;
                        //BindPlayer(client);
                        roomInfo = null;
                        client = null;
                        break;
                    }
                case GameState.Enter:
                    {
                        
                        break;
                    }
                case GameState.UnPrepared:{
                        //进入UnPrePaerd状态要做的事
                        client.userState = UserState.unprepared;
                        BindPlayer(client);

                        prepare_Text.Text = "准备";
                        Button_exit.IsEnabled = true;
                        break;
                    }
                case GameState.Prepared:{
                        //进入Prepared状态要做的事
                        client.userState = UserState.prepared;
                        BindPlayer(client);

                        prepare_Text.Text = "取消准备";
                        Button_exit.IsEnabled = false;
                        break;
                    }
                case GameState.Start:{
                        //进入Start状态要做的事
                        InitGame();
                        break;
                    }
                case GameState.Move: {
                        //进入Move状态要做的事
                        client.userState = UserState.move;
                        BindPlayer(client);

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
                case GameState.ShowMove:
                    {
                        
                        break;
                    }
                case GameState.Waiting:
                    {
                        //进入Waiting状态要做的事
                        client.userState = UserState.wait;
                        BindPlayer(client);

                        Button_submit.IsEnabled = false;
                        Button_continue.IsEnabled = false;
                        Button_cancel.IsEnabled = false;
                        Button_return.IsEnabled = false;
                        break;
                    }
                case GameState.Ending:
                    {
                        //进入新状态要做的事
                        client.userState = UserState.in_room;
                        BindPlayer(client);
                        break;
                    }
                default:
                    break;
            }

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

        /// <summary>
        /// 初始化游戏
        /// </summary>
        public void InitGame()
        {
#if TEST
            gameCore.startGame(2);
            ChangedState(GameState.Move);
#endif 
        }
        
        /// <summary>
        /// 换一个起点
        /// </summary>
        /// <param name="p"></param>
      
       


        /// <summary>
        /// 全屏
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
        /// 准备按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_prepare_Click(object sender, RoutedEventArgs e)
        {
            if (roomState == GameState.Prepared)
                ChangedState(GameState.UnPrepared);
            else
            {
                ChangedState(GameState.Prepared);
                if (CanStart())
                    ChangedState(GameState.Start);
            }
            
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


            //解除绑定
            UIHelper.BreakCheckerBind();

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
            logger.Info("点击界面："+point.ToString()+" 逻辑坐标:"+ checkerPoint.ToString());

            //点击逻辑
            switch (roomState)
            {
                case GameState.Move:
                    if (gameCore.ChooseChecker(checkerPoint, client.pos))
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
                    else if (gameCore.ChooseChecker(checkerPoint, client.pos))
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
                    if (gameCore.CanMoveNext(checkerPoint))
                    {
                        //当前的棋子显示
                        //UIHelper.AddRoad(checkerPoint);

                        //UIHelper.MovePostion(gameCore.GetCurrentPoint(), checkerPoint);
                        gameCore.MoveNext(checkerPoint);

                        //上一步路径显示
                        //UIHelper.ShowLastRoad();


                        //显示当前可走的路径


                    }
                    else if (gameCore.ChooseChecker(checkerPoint, client.pos))
                    {//换一个点

                        //重新选择
                        gameCore.SetStartPoint(checkerPoint);

                        //显示可走的路径

                        ChangedState(GameState.Choose);
                    }
                    break;
                default :
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

            //
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
            if(gameCore.GetCurrentPoint().Equals(gameCore.GetStartPoint()))
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

        }
    }
}
