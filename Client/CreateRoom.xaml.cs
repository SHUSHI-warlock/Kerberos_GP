using Client.core.Services;
using Client.Utils.LogHelper;
using Panuon.UI.Silver;
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
    /// CreateRoom.xaml 的交互逻辑
    /// </summary>
    public partial class CreateRoom : Page
    {
        private static Logger logger = Logger.GetLogger();

        public CreateRoom()
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
            if(RoomName.Text=="")
            {
                logger.Error("房间名为空！");
                MessageBoxX.Show("房间名不能为空！", "警告", Application.Current.MainWindow, MessageBoxButton.OK, new MessageBoxXConfigurations()
                {
                    MessageBoxIcon = MessageBoxIcon.Warning,
                    ButtonBrush = "#F1C825".ToColor().ToBrush(),
                    OKButton = "好",
                });
                return;
            }
            if((bool)yesButton.IsChecked&&Password.Text=="")
            {
                logger.Error("密码不能为空！");
                MessageBoxX.Show("密码不能为空！", "警告", Application.Current.MainWindow, MessageBoxButton.OK, new MessageBoxXConfigurations()
                {
                    MessageBoxIcon = MessageBoxIcon.Warning,
                    ButtonBrush = "#F1C825".ToColor().ToBrush(),
                    OKButton = "好",
                });
                return;
            }
            int res;
            if(!int.TryParse(MaxPlayer.Text,out res))
            {
                logger.Error("请选择最大人数！");
                MessageBoxX.Show("请指定游戏人数！", "警告", Application.Current.MainWindow, MessageBoxButton.OK, new MessageBoxXConfigurations()
                {
                    MessageBoxIcon = MessageBoxIcon.Warning,
                    ButtonBrush = "#F1C825".ToColor().ToBrush(),
                    OKButton = "好",
                });
                return;
            }

            Lobby.createRoom = new RoomInfo(RoomName.Text, Password.Text, int.Parse(MaxPlayer.Text));

            Window win = (Window)this.Parent;
            win.Close();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded)
                return;
            if(sender.Equals(noBytton))
            {
                yesButton.IsChecked = false;
                PswInput.Visibility = Visibility.Hidden;
                Password.Visibility = Visibility.Hidden;
                Password.Text = "";
            }
            else
            {
                noBytton.IsChecked = false;
                PswInput.Visibility = Visibility.Visible;
                Password.Visibility = Visibility.Visible;
            }
        }

      
    }
}
