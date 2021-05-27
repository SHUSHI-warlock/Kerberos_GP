using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Client
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            
            Login login = new Login();
            App.Current.MainWindow = login;
            login.Show();

            
            /*Lobby lobby = new Lobby();
            lobby.Show();
            MessageShow messageShow = MessageShow.GetInstance();
            messageShow.Show();*/
        }
    }
}
