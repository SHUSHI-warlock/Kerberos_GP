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
namespace Client
{
    /// <summary>
    /// MessageShow.xaml 的交互逻辑
    /// </summary>
    public partial class MessageShow : Window
    {
        private static Logger logger = Logger.GetLogger();

        private static MessageShow instance;

        private static BindingList<Message> messagesList;



        //private 

        private MessageShow()
        {
            InitializeComponent();
            messagesList = new BindingList<Message>();
            MsgList.ItemsSource = messagesList;

            Test();
        }

        public static MessageShow GetInstance()
        {
            if (instance == null)
                instance = new MessageShow();
            return instance;
        }

        public void Test()
        {
            messagesList.Add(new Message(0, 0, 0));
            messagesList.Add(new Message(1, 1, 0));
            messagesList.Add(new Message(2, 0, 0));
            messagesList.Add(new Message(1, 0, 0));
            messagesList.Add(new Message(2, 0, 0));
            messagesList.Add(new Message(0, 1, 1));

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
