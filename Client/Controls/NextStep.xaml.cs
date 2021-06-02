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

namespace Client.Utils.UIHelper
{
    /// <summary>
    /// NextStep.xaml 的交互逻辑
    /// </summary>
    public partial class NextStep : UserControl
    {
        private Point point;

        public NextStep()
        {
            InitializeComponent();
        }

        public void SetPoint(Point point)
        {
            this.point = point;
            HideRoad();
        }

        public Point GetPoint()
        {
            return point;
        }

        public void ShowRoad()
        {
            if (point == null)
                throw new Exception("下一步动画未初始化坐标!");
            this.SetValue(Canvas.TopProperty, point.X);
            this.SetValue(Canvas.LeftProperty, point.Y);
            this.Visibility = Visibility.Visible;
        }
        public void HideRoad()
        {
            this.Visibility = Visibility.Hidden;

        }
    }
}
