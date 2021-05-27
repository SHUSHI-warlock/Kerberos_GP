using Client.core.Services;
using Client.Utils.LogHelper;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// PswInput.xaml 的交互逻辑
    /// </summary>
    public partial class PswInput : Page
    {
        private static Logger logger = Logger.GetLogger();
        public PswInput()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_return_Click(object sender, RoutedEventArgs e)
        {
            Lobby.createRoom = null;
            Window win = (Window)this.Parent;
            win.Close();
        }

        private void Button_submit_Click(object sender, RoutedEventArgs e)
        {
            if (RoomPsw.Text == "")
            {
                logger.Error("房间密码为空！");
                return;
            }
            
            Lobby.createRoom = new RoomInfo(-1, RoomPsw.Text);

            Window win = (Window)this.Parent;
            win.Close();
        }

    }
}
