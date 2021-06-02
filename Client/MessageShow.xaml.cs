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

        public void ShowMsg(Message msg,EncryptionType type,string key,string m,string c)
        {
            messagesList.Add(new MsgRecord(msg,type,key,m,c));
        }


        /// <summary>
        /// 禁止运行期间关闭
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
