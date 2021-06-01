using Client.core;
using Client.Utils.LogHelper;
using MyBilliards.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Client.Utils.UIHelper
{
    public class GameUIhelper
    {
        private static Logger logger = Logger.GetLogger();

        public static double Top_Offset = 2;
        public static double Left_Offset = 2;
        public static double checkerRadius = 32;
        public static double checkerColSpan = 47;              //同一行的两个棋子之间间隔
        public static double checkerRowSpan = Math.Sqrt(3) * checkerColSpan / 2; //行间距

        public static int timeSpan = 400;

        public static double Mask_Top_Offset = -4;
        public static double Mask_Left_Offset = -4;

        public static double Road_Top_Offset = -4;
        public static double Road_Left_Offset = -4;

        public static double Step_Top_Offset = 0;
        public static double Step_Left_Offset = 0;


        public static BitmapImage[] BallsImage = new BitmapImage[6] {
            new BitmapImage( new Uri(@"/Resources/Image/gameImage/红球.png",UriKind.Relative)),
            new BitmapImage(  new Uri(@"/Resources/Image/gameImage/蓝球.png",UriKind.Relative)),
            new BitmapImage( new Uri(@"/Resources/Image/gameImage/黄球.png",UriKind.Relative)),
            new BitmapImage( new Uri(@"/Resources/Image/gameImage/紫球.png",UriKind.Relative)),
            new BitmapImage( new Uri(@"/Resources/Image/gameImage/绿球.png",UriKind.Relative)),
            new BitmapImage( new Uri(@"/Resources/Image/gameImage/粉球.png",UriKind.Relative)),
        };
        //棋子map
        private static Checker[,] checkers = new Checker[7, 10];
        private static int[] checkersNum = new int[7] { 0,0,0,0,0,0,0};

        private Dictionary<CheckerPoint, CheckerPoint> boards;
        private bool[] playerSet;


        private GameRoom roomPage;

        private List<RoadLabel> roadLabels;
        private int roadNum=0;
        //private bool Moved;

        private List<NextStep> nextSteps;

        public GameUIhelper(GameRoom room)
        {
            roomPage = room;
            nextSteps = new List<NextStep>();
            roadLabels = new List<RoadLabel>();
            boards = new Dictionary<CheckerPoint, CheckerPoint>();
            playerSet = new bool[7] { false, false, false, false, false, false, false };
            roadNum = 0;

        }

        /// <summary>
        /// 逻辑坐标转界面坐标
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static double GetLeftPostion(int x, int y)
        {
            double res = y * checkerColSpan;
            if (x % 2 != 0)
                res += checkerColSpan / 2;
            return res + Left_Offset;
        }
        public static double GetTopPostion(int x)
        {
            return (double)x * GameUIhelper.checkerRowSpan + GameUIhelper.Top_Offset;
        }
        public static Point GetPoint(CheckerPoint point)
        {
            return new Point(GetLeftPostion(point.x, point.y) + checkerRadius / 2, GetTopPostion(point.x) + checkerRadius / 2);
        }
        /// <summary>
        /// 界面坐标转逻辑坐标
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static CheckerPoint GetLogicPoint(Point point)
        {
            CheckerPoint temp = new CheckerPoint();
            temp.x = (int)((point.Y - Top_Offset) / checkerRowSpan);
            double a = (point.X - Left_Offset - (temp.x % 2 == 0 ? 0 : checkerColSpan / 2));
            temp.y = (int)(a / checkerColSpan);

            //这只是获得了该正方形区域，还需要与具体的中心点进行距离判断
            Vector dis = Point.Subtract(point,GetPoint(temp));
            if (dis.LengthSquared <= checkerRadius * checkerRadius / 4)
                return temp;
            else
                return null;
        }

        
        public void BreakCheckerBind()
        {
            for(int k = 1; k < 7; k++)
                if(playerSet[k])    
                    for (int i = 0; i < 10; i++)
                        roomPage.Board.Children.Remove(checkers[k, i]);
        }
        public void InitChecker(int player,CheckerPoint point)
        {
            if (player < 1 || player > 6)
                return;
            Checker temp = null;

            if (checkersNum[player] == 10)
            {
                for(int i=0;i<10;i++)
                    if(checkers[player, i].GetStartPoint().Equals(point))
                    {
                        temp = checkers[player, i];
                        temp.ReSetChecker();
                        boards.Add(point, new CheckerPoint(player, i));
                        break;
                    }
            }
            else
            {
                temp = new Checker(point, player);
                checkers[player, checkersNum[player]] = temp;
                boards.Add(point, new CheckerPoint(player, checkersNum[player]));

                checkersNum[player]++;
            }

            //temp.Visibility = Visibility.Visible;
            if(temp==null)
            {
                logger.Error("棋子初始化问题！");
                return;
            }

            playerSet[player] = true;
            roomPage.Board.Children.Add(temp);
            temp.ShowChecker();
            
        }

        public void MovePostion(CheckerPoint oldPoint, CheckerPoint newPoint)
        {
            CheckerPoint checkerP = null;
            if (boards.TryGetValue(oldPoint, out checkerP) && !boards.ContainsKey(newPoint))
            {//存老无新
                logger.Debug("移动棋子 " + oldPoint.ToString() + "-->" + newPoint.ToString());
                boards.Remove(oldPoint);
                checkers[checkerP.x, checkerP.y].MoveChecker(newPoint);
                boards.Add(newPoint, checkerP);
            }
            else
                logger.Error(string.Format("MovePostion ：指定了错误的起点{0}或终点{1}！",oldPoint.ToString(), newPoint.ToString()));
        }

        public void ShowMovePath(Stack<CheckerPoint> path)
        {

            if(path==null||path.Count==0)
            {
                logger.Error("显示移动动画传参错误!");
                return;
            }
            logger.Debug("ShowMovePath 显示移动动画!");
            List<CheckerPoint> roadpath = new List<CheckerPoint>(path);
            
            checkers[boards[roadpath[0]].x, boards[roadpath[0]].y].ShowRoad(roadpath);

        }
        
        /// <summary>
        /// 标记起点
        /// </summary>
        /// <param name="point"></param>
        public void ShowMark(CheckerPoint point)
        {
            logger.Info("ShowMark 显示选中标记!");

            double top = GetTopPostion(point.x) + GameUIhelper.Mask_Top_Offset;
            double left = GetLeftPostion(point.x,point.y) + GameUIhelper.Mask_Left_Offset;

            roomPage.MaskImage.SetValue(Canvas.TopProperty,top);
            roomPage.MaskImage.SetValue(Canvas.LeftProperty,left);
            roomPage.MaskImage.Visibility = Visibility.Visible;
        }
        public void HideMark()
        {
            logger.Info("HideMark 隐藏选中标记!");
            roomPage.MaskImage.Visibility = Visibility.Hidden;

        }

        /// <summary>
        /// 显示路径
        /// </summary>

        public void AddRoad(CheckerPoint point)
        {
            logger.Info("AddRoad 添加走过路径!");
            if (roadLabels.Count < roadNum + 1)
            {
                RoadLabel temp = new RoadLabel(roadNum+1);
                roomPage.Board.Children.Add(temp);
                roadLabels.Add(temp);
            }

            roadNum++;

            double top = GetTopPostion(point.x) + GameUIhelper.Road_Top_Offset;
            double left = GetLeftPostion(point.x, point.y) + GameUIhelper.Road_Left_Offset;

            roadLabels[roadNum-1].SetPoint(new Point(top, left));

        }
        public void RemoveRoad( )
        {
            logger.Info("RemoveRoad 删除走过路径!");
            if (roadNum > 0 && roadLabels.Count > 0)
            {
                roadNum--;
                roadLabels[roadNum].HideRoad();
            }
        }
        public void ShowRoad(int index)
        {
            logger.Info("ShowRoad 显示路径"+index+"!");

            roadLabels[index-1].ShowRoad();
        }
        public void HideRoad(int index)
        {
            logger.Info("ShowRoad 隐藏路径" + index + "!");

            roadLabels[index - 1].HideRoad();
        }
        public void ShowLastRoad()
        {
            if (roadNum < 2)
            {
                logger.Error("ShowLastRoad 没有上一个");
                //没有上一个
                return;
            }
            logger.Info("ShowLastRoad 显示上一步路径!");

            roadLabels[roadNum-2].ShowRoad();
        }
        public void HideCurRoad()
        {
            if (roadNum < 1)
            {
                logger.Error("HideCurRoad 当前没有路径");
                //没有上一个
                return;
            }
            logger.Info("HideCurRoad 影藏当前步路径!");

            roadLabels[roadNum - 1].HideRoad();
        }
        public void ClearRoads()
        {
            logger.Info("ClearRoads 清除路径!");

            for (int i = 1; i <= roadNum; i++)
                HideRoad(i);
            roadNum = 0;
        }

        /// <summary>
        /// 显示可走的路径
        /// </summary>
        public void ShowNextStep(List<CheckerPoint> steps)
        {
            logger.Info("ShowNextStep 显示当前可走的位置!");

            if (steps != null)
            {
                for(int i=0;i<steps.Count;i++)
                {
                    if(i==nextSteps.Count)
                    {
                        NextStep temp = new NextStep();

                        roomPage.Board.Children.Add(temp);
                        nextSteps.Add(temp);
                    }
                    double top = GetTopPostion(steps[i].x) + GameUIhelper.Step_Top_Offset;
                    double left = GetLeftPostion(steps[i].x, steps[i].y) + GameUIhelper.Step_Left_Offset;

                    nextSteps[i].SetPoint( new Point(top, left));

                    nextSteps[i].ShowRoad();
                }
            }
        }
        public void HideNextStep()
        {
            foreach (NextStep sp in nextSteps)
            {
                sp.HideRoad();
            }
        }
    }
}
