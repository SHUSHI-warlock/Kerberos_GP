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

using System.Runtime.InteropServices;
using System.Windows.Interop;
using Client.Utils.RSAUtil;
using Client.Utils.DesUtil;
using Client.Controls;

namespace Client
{
    /// <summary>
    /// MessageShow.xaml 的交互逻辑
    /// </summary>
    public partial class MessageShow : Window
    {
        private static Logger logger = Logger.GetLogger();

        private static MessageShow instance;

        private static BindingList<MsgRecord> messagesList;



        //private 

        private MessageShow()
        {
            InitializeComponent();
            messagesList = new BindingList<MsgRecord>();
            MsgList.ItemsSource = messagesList;

            Test();
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

        public void Test()
        {


        }

        public void ShowMsg(Message msg, EncryptionType type, string key, string m, string c)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                messagesList.Add(new MsgRecord(msg, type, key, m, c));
            });
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
        /// </summary>
        /// <param name="record"></param>
        private void BindAMsg(MsgRecord record)
        {
            ReceiverText.Text = ParseReceiver(record.MessageP2P);
            SenderText.Text = ParseSender(record.MessageP2P);
            //报文类型需要根据具体的来定
            
            //报文状态需要根据具体的来定

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
    }
}
