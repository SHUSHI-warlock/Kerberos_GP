using Client.core;
using MyBilliards.Converter;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client.Utils.UIHelper
{
    /// <summary>
    /// Checker.xaml 的交互逻辑
    /// </summary>
    public partial class Checker : UserControl
    {
        private CheckerPoint InitPoint;
        private int player;
        private CheckerPoint point;

        public Checker(CheckerPoint start,int player)
        {
            InitializeComponent();
            this.player = player;
            img.Source = GameUIhelper.BallsImage[player - 1];
            img.Width = GameUIhelper.checkerRadius;
            InitPoint = new CheckerPoint(start);

            point = start;

            /*
            //设置绑定
            Binding binding1 = new Binding("x");
            Binding binding2 = new Binding("y");
            MultiBinding bindingLeft = new MultiBinding();
            bindingLeft.Bindings.Add(binding1);
            bindingLeft.Bindings.Add(binding2);
            bindingLeft.Converter = new PostionX_Converter();
            this.SetBinding(Canvas.LeftProperty, bindingLeft);

            Binding bindingTop = new Binding("x");
            bindingTop.Converter = new PostionY_Converter();
            this.SetBinding(Canvas.TopProperty, bindingTop);

            this.DataContext = point;
            */

        }

        private Point GetPostion(CheckerPoint point)
        {
            Point temp = new Point();
            temp.Y= (double)point.x * GameUIhelper.checkerRowSpan + GameUIhelper.Top_Offset;
            double res = point.y * GameUIhelper.checkerColSpan;
            if (point.x % 2 != 0)
                res += GameUIhelper.checkerColSpan / 2;
            temp.X = res + GameUIhelper.Left_Offset;
            return temp;
        }

        public CheckerPoint GetStartPoint()
        {
            return InitPoint;
        }

        public void ReSetChecker()
        {
            point.x = InitPoint.x;
            point.y = InitPoint.y;
        }

        public void ShowChecker()
        {
            Point temp = GetPostion(point);
            tt.X = temp.X;
            tt.Y = temp.Y;
            this.Visibility = Visibility.Visible;
        }
        public void HideChecker()
        {
            this.Visibility = Visibility.Hidden;
        }

        public void MoveChecker(CheckerPoint p)
        {
            point.x = p.x;
            point.y = p.y;
            ShowChecker();
        }

        //动画显示
        public void ShowRoad(List<CheckerPoint> roads)
        {
            //测试：先把棋子放回起点
            //Point temp = GetPostion(roads[0]);
            //tt.X = temp.X;
            //tt.Y = temp.Y;

            int step = roads.Count-1;
            //创建动画
            DoubleAnimationUsingKeyFrames dakX = new DoubleAnimationUsingKeyFrames();
            DoubleAnimationUsingKeyFrames dakY = new DoubleAnimationUsingKeyFrames();

            dakX.FillBehavior = FillBehavior.Stop;
            dakY.FillBehavior = FillBehavior.Stop;


            dakX.Duration = new Duration(TimeSpan.FromMilliseconds(GameUIhelper.timeSpan * step));
            dakY.Duration = new Duration(TimeSpan.FromMilliseconds(GameUIhelper.timeSpan * step));
            //创建关键帧
            for(int i=0;i<=step;i++)
            {
                SplineDoubleKeyFrame x_kf = new SplineDoubleKeyFrame();
                SplineDoubleKeyFrame y_kf = new SplineDoubleKeyFrame();
                if (i == 0)
                {
                    x_kf.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0));
                    y_kf.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0));
                }
                else
                {
                    x_kf.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(GameUIhelper.timeSpan * i));
                    y_kf.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(GameUIhelper.timeSpan * i));
                }
                Point point = GetPostion(roads[i]);
                x_kf.Value = point.X;
                y_kf.Value = point.Y;
                //添加关键帧
                dakX.KeyFrames.Add(x_kf);
                dakY.KeyFrames.Add(y_kf);

            }

            //执行动画
            this.tt.BeginAnimation(TranslateTransform.XProperty, dakX);
            this.tt.BeginAnimation(TranslateTransform.YProperty, dakY);
            
        }
    }
}
