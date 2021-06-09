using Client.MsgTrans;
using Client.Utils.LogHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Panuon.UI.Silver;
using Panuon.UI.Silver.Core;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Client.Utils.RSAUtil;
using Client.Utils.DesUtil;
using Client.Controls;

namespace Client
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MessageShow : Window
    {
        private static Logger logger = Logger.GetLogger();

        private static MessageShow instance;

        private static BindingList<MsgRecord> messagesList;

        //接收开关
        private volatile bool canStop = false;

        //private 

        private MessageShow()
        {
            InitializeComponent();
            messagesList = new BindingList<MsgRecord>();
            MsgList.ItemsSource = messagesList;
            canStop = false;
            //Test();
        }
        /// <summary>
        /// 获得窗口实例
        /// </summary>
        /// <returns></returns>
        public static MessageShow GetInstance()
        {
            if (instance == null)
                instance = new MessageShow();
            return instance;
        }

        /// <summary>
        /// 自己测试用
        /// </summary>
        private void Test()
        {

        }

        public void ShowMsg(Message msg, EncryptionType type, string key, string m, string c)
        {
            if (!canStop)
            {//未停止时都接收
                Application.Current.Dispatcher.Invoke(() =>
                {
                    messagesList.Add(new MsgRecord(msg, type, key, m, c));
                });
            }
        }


        /// <summary>
        /// 禁止运行期间关闭
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }

        /// <summary>
        /// 选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MsgList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //选中房间？
            if (MsgList.SelectedValue == null)
            {
                logger.Info("查看消息:未选中消息！");
                //提示框显示
                return;
            }

            MsgRecord record = (MsgRecord)MsgList.SelectedValue;
            BindAMsg(record);
        }

        /// <summary>
        /// 显示一条记录
        ///
        /// </summary>
        /// <param name="record"></param>
        private void BindAMsg(MsgRecord record)
        {
            string Tsender, Treceiver, Ttype, Tstate;
            Tsender = "未知";
            Treceiver = "未知";
            Ttype = "未知";
            Tstate = "正常";

            Tsender = ParseSender(record.MessageP2P);
            Treceiver = ParseReceiver(record.MessageP2P);


            //报文类型/状态需要根据具体的来定
            switch ((P2PType)record.MessageP2P)
            {
                case P2PType.CtoS:
                    Ttype = CtoSType(record.MessageType);
                    break;
                case P2PType.StoC:
                    Ttype = StoCType(record.MessageType);
                    break;
                case P2PType.CtoAS:
                    Ttype = CtoASType(record.MessageType);
                    break;
                case P2PType.AStoC:
                    Ttype = AStoCType(record.MessageType);
                    break;
                case P2PType.CtoTGS:
                    Ttype = CtoTGSType(record.MessageType);
                    break;
                case P2PType.TGStoC:
                    Ttype = TGStoCType(record.MessageType);
                    break;
                default:
                    break;
            }

            if(record.StateCode!=0)
            {
                Tstate = "异常";
            }
            
            switch (record.Type)
            {
                case EncryptionType.Plain://明文
                    EncryptionText.Text = "明文";
                    KeyText.Text = "无";
                    break;
                case EncryptionType.Des:
                    EncryptionText.Text = "DES";
                    KeyText.Text = record.Key;
                    break;
                case EncryptionType.Rsa_pk:
                    EncryptionText.Text = "RSA加密";
                    KeyText.Text = record.Key;

                    break;
                case EncryptionType.Rsa_sk:
                    EncryptionText.Text = "RSA解密";
                    KeyText.Text = record.Key;

                    break;
            }
            
            if (record.C != "")
                CipherText.Text = record.C;
            else
                CipherText.Text = "无";
            if (record.M != "")
                PlainText.Text = record.M;
            else
                PlainText.Text = "无";

            //显示
            ReceiverText.Text = Treceiver;
            SenderText.Text =  Tsender;
            MsgStateText.Text = Tstate;
            MsgTypeText.Text = Ttype;

        }

        public static string ParseSender(int p2p)
        {
            switch ((P2PType)p2p)
            {
                case P2PType.CtoAS:
                case P2PType.CtoTGS:
                case P2PType.CtoS:
                    return "客户端";
                case P2PType.AStoC:
                    return "AS服务器";
                case P2PType.TGStoC:
                    return "TGS服务器";
                case P2PType.StoC:
                    return "应用服务器";
                default:
                    return "未知";
            }
        }

        public static string ParseReceiver(int p2p)
        {
            switch ((P2PType)p2p)
            {
                case P2PType.CtoAS:
                    return "AS服务器";
                case P2PType.CtoTGS:
                    return "TGS服务器";
                case P2PType.CtoS:
                    return "应用服务器";
                case P2PType.AStoC:
                case P2PType.TGStoC:
                case P2PType.StoC:
                    return "客户端";
                default:
                    return "未知";
            }
        }

        public string CtoASType(int type)
        {
            switch (type)
            {
                case 0: return "请求验证";
                case 1:return "请求注册";
                case 2:return "建立连接";
                case 3: return "发送注册数据";
                case 4:return "取消注册";
                default: return "未知";
            }
        }
        public string AStoCType(int type)
        {
            switch (type)
            {
                case 0: return "返回验证结果";
                case 1: return "回复注册请求";
                case 2: return "建立连接";
                case 3: return "返回注册结果";
                default: return "未知";
            }
        }
        public string CtoTGSType(int type)
        {
            if(type==0)
                return "请求验证";
            else
                return "未知";
        }
        public string TGStoCType(int type)
        {
            if (type == 0)
                return "返回验证结果";
            else
                return "未知";
        }
        public string CtoSType(int type)
        {
            switch (type)
            {
                case 0: return "请求登录";
                case 2: return "请求房间信息";
                case 3: return "创建房间";
                case 4: return "进入房间";
                case 5: return "退出房间";
                case 6: return "游戏准备";
                case 7: return "取消准备";
                case 8: return "聊天消息";
                case 9: return "移动提交";
                case 10: return "退出大厅";
                default: return "未知";
            }
        }
        public string StoCType(int type)
        {
            switch (type)
            {
                case 0: return "返回验证结果";
                case 2: return "返回房间信息";
                case 3: return "返回创建结果";
                case 4: return "返回加入结果";
                case 5: return "接收聊天消息";
                case 6: return "玩家状态通知";
                case 7: return "游戏消息通知";
                case 8: return "返回移动结果";
               
                default: return "未知";
            }
        }


        /// <summary>
        /// 继续按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void continueButton_Click(object sender, RoutedEventArgs e)
        {
            if(canStop)
            {
                logger.Info("开始抓包！");
                canStop = true;
                currText.Text = "运行中";
                currText.Foreground = Colors.SpringGreen.ToBrush();
                MessageBoxX.Show("开始抓包！", "", Application.Current.MainWindow, MessageBoxButton.OK, new MessageBoxXConfigurations()
                {
                    OKButton = "好！",
                    MessageBoxStyle = MessageBoxStyle.Modern
                });
            }
            else
                logger.Info("已经在抓包了！");
        }
        /// <summary>
        /// 清空按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxX.Show("是否重新抓包？", "", Application.Current.MainWindow, MessageBoxButton.YesNo, new MessageBoxXConfigurations()
            {
                MessageBoxStyle = MessageBoxStyle.Standard,
                MessageBoxIcon = MessageBoxIcon.Question

            }) == MessageBoxResult.Yes)
            {
                
                canStop = true;
                
                logger.Info("清空抓包记录！");
                messagesList.Clear();

                canStop = false;
                currText.Text = "运行中";
                currText.Foreground = Colors.SpringGreen.ToBrush();
            }
        }
        /// <summary>
        /// 停止按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!canStop)
            {
                if(MessageBoxX.Show("是否停止抓包？", "", Application.Current.MainWindow, MessageBoxButton.YesNo, new MessageBoxXConfigurations()
                {
                    MessageBoxStyle = MessageBoxStyle.Standard,
                    MessageBoxIcon = MessageBoxIcon.Question
                   
                }) == MessageBoxResult.Yes)
                {
                    logger.Info("停止抓包！");
                    canStop = true;
                    currText.Text = "已停止";
                    currText.Foreground = Colors.Red.ToBrush();
                }
            }
            else
                logger.Info("已经停止抓包了！");
        }
    }
}
