#define TEST

using Client.Utils.LogHelper;
using Client.Utils.UIHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.core
{
    public class CheckerCore
    {
        public static Logger logger = Logger.GetLogger();
        private GameUIhelper UIhelper;

        public const int Row = 17;
        public const int Col = 13;
        public const int MaxPlayer = 6;
        public static int[] AroundY = new int[8] { -1, -1, 0, 0, 0, 0, 1, 1 };

        private bool[] finished;
        private int curPlayer;
        private int playerNum;
        private int[,] board;


        //当前步数
        public int steps { get; set; }
        
        private List<CheckerPoint> nextStepList;    //这步可走的点
        private Stack<CheckerPoint> preRoad;        //走过的路
        private CheckerPoint StartPoint;
        private bool IsMoveEnd;
        //对路径的上锁
        //private readonly object preRoadLock = new object();


        public CheckerCore()
        {
            finished = new bool[MaxPlayer+1];
            board = new int[Row, Col];
            preRoad = new Stack<CheckerPoint>();
            nextStepList = new List<CheckerPoint>();
            //初始化棋盘
            for (int i = 0; i < Row; i++)
                for (int j = 0; j < Col; j++)
                    board[i, j] = -1;
        }

        public void SetUIHelper(GameUIhelper gameUIhelper)
        {
            UIhelper = gameUIhelper;
        }

        public void startGame(int playerNum)
        {
            //初始化玩家
            this.playerNum = playerNum;
            for (int i = 1; i <= MaxPlayer; i++)
                finished[i] = true;
            
            //初始化棋盘

            setRegion(true, new CheckerPoint(0, 6), 13, 0);
            setRegion(false, new CheckerPoint(7, 10), 4, 0);
            setRegion(false, new CheckerPoint(7, 1), 4, 0);

            //显示人员位置
            if (playerNum == 6)
            {
                setRegion(true, new CheckerPoint(9, 10), 4, 3);
                setRegion(false, new CheckerPoint(7, 1), 4, 6);
                finished[3] = false; finished[6] = false;
            }
            if (playerNum == 4 || playerNum == 6)
            {
                setRegion(true, new CheckerPoint(9, 1), 4, 5);
                setRegion(false, new CheckerPoint(7, 10), 4, 2);                
                finished[2] = false; finished[5] = false;
            }
            if (playerNum == 6 || playerNum == 4 || playerNum == 2)
            {
                setRegion(true, new CheckerPoint(0, 6), 4, 1);
                setRegion(false, new CheckerPoint(16, 6), 4, 4);
                finished[1] = false; finished[4] = false;
            }
            else
            {
                logger.Fatal("游戏传入的PlayerNum有误！");
                return;
            }

            //初始化其他
            curPlayer = 1;
            steps = 0;
            ClearMove();

        }

        /// <summary>
        /// 选中一颗棋子
        /// </summary>
        /// <param name="p"></param>
        /// <returns>是否有效</returns>
        public bool ChooseChecker(CheckerPoint p,int player)
        {
            int type = getChess(p);
            if (type == player && !p.Equals(StartPoint))
            {//不选同一个起点
                logger.Info("选中自己棋子");
                return true;
            }
            else if (type == -1)
                logger.Info("点击不在棋盘上！");
            else if (type == 0)
                logger.Info("点击空地！");
            else
                logger.Info("选中的不是自己棋子");
            return false;
        }

        public bool CanMoveNext(CheckerPoint p)
        {
            if (IsMoveEnd||isOut(p))
                return false;

            foreach (CheckerPoint pp in nextStepList)
                if (pp.Equals(p))
                    return true;
            return false;
        }
        public void MoveNext(CheckerPoint p)
        {
            logger.Info("--- MoveNext ---");

            try
            {
                if (p == null || isOut(p))
                {
                    logger.Error("MoveNext传入错误！");
                    return;
                }
                if (preRoad.Count == 1)
                {
                    //挪动操作
                    CheckerPoint temp;
                    for (int i = 0; i < 6; i++)
                    {
                        temp = getAround(p, i);
                        if (temp.Equals(StartPoint))
                        {
                            IsMoveEnd = true;
                            break;
                        }
                    }
                }
                //棋盘移动
                move(preRoad.Peek(), p);
                
            }
            catch (Exception e)
            {
                logger.Info("CheckerCore : MoveNext 逻辑异常捕获");
                logger.Fatal(e.Message);
                logger.Fatal(e.StackTrace);
            }

            try
            {
                //UI棋子移动
                UIhelper.MovePostion(preRoad.Peek(),p);

                //入队列
                preRoad.Push(p);

                //UI
                //选中的棋子显示
                UIhelper.HideMark();
                UIhelper.ShowMark(p);

                //取消之前可走
                UIhelper.HideNextStep();

                //添加路径
                UIhelper.AddRoad(p);
                UIhelper.ShowLastRoad();

                //计算可走
                if (!IsMoveEnd)
                    nextStepList = getNextStep(p);
                else
                    nextStepList.Clear();

                //UI显示下一步
                UIhelper.ShowNextStep(nextStepList);
            }
            catch (Exception e)
            {
                logger.Info("CheckerCore : MoveNext UI异常捕获");
                logger.Fatal(e.Message);
                logger.Fatal(e.StackTrace);
            }
        }
        public CheckerPoint GetCurrentPoint()
        {
            if (preRoad.Count == 0)
                return null;
            else
                return preRoad.Peek();
        }
        public void SetStartPoint(CheckerPoint p)
        {
            logger.Info("--- SetStartPoint ---");

            try
            {
                if (p == null || isOut(p) || isBlank(p))
                {
                    logger.Error("SetStartPoint传入错误！");
                    return;
                }
                
                    ClearMove();
                    preRoad.Push(p);//入栈
                
                StartPoint = p;

                nextStepList = getNextStep(p); //找可走
                
                                //挪动操作
                CheckerPoint temp;
                for (int i = 0; i < 6; i++)
                {
                    temp = getAround(p, i);
                    if (!isOut(temp) && isBlank(temp))
                        nextStepList.Add(temp);
                }
            }
            catch (Exception e)
            {
                logger.Info("CheckerCore : SetStartPoint 逻辑异常捕获");
                logger.Fatal(e.Message);
                logger.Fatal(e.StackTrace);
            }

            try
            {
                //UI
                //选中的棋子显示
                UIhelper.HideMark();
                UIhelper.ShowMark(p);
                //添加路径
                UIhelper.ClearRoads();
                UIhelper.AddRoad(p);
                //显示可走
                UIhelper.ShowNextStep(nextStepList);
            }
            catch (Exception e)
            {
                logger.Info("CheckerCore : SetStartPoint UI异常捕获");
                logger.Fatal(e.Message);
                logger.Fatal(e.StackTrace);
            }
        }
        public CheckerPoint GetStartPoint()
        {
            return StartPoint;
        }

        public void ReturnMove()
        {
            logger.Info("--- ReturnMove ---");

            try
            {
                //路径退回
                CheckerPoint temp = preRoad.Pop();
                //棋子撤回
                move(temp, preRoad.Peek());
                //重新计算
                nextStepList = getNextStep(preRoad.Peek());

                //路径回退
                UIhelper.RemoveRoad();
                UIhelper.HideCurRoad();

                //UI退回
                UIhelper.MovePostion(temp, preRoad.Peek());

                //当前可走撤销
                UIhelper.HideNextStep();              
                //显示
                UIhelper.ShowNextStep(nextStepList);

                //选中标记更新
                UIhelper.HideMark();
                UIhelper.ShowMark(preRoad.Peek());
            }
            catch (Exception e)
            {
                logger.Info("CheckerCore : ReturnMove 异常捕获");
                logger.Fatal(e.Message);
                logger.Fatal(e.StackTrace);
            }
        }
        
        /// <summary>
        /// 清除走棋
        /// </summary>
        public void ClearMove()
        {
            logger.Info("--- ClearMove ---");
            try
            {
                //UI
                UIhelper.HideMark();
                UIhelper.ClearRoads();
                UIhelper.HideNextStep();
                //棋子移动
                if (preRoad.Count > 1)
                    UIhelper.MovePostion(preRoad.Peek(), StartPoint);
            }
            catch (Exception e)
            {
                logger.Info("CheckerCore : ClearMove UI异常捕获");
                logger.Fatal(e.Message);
                logger.Fatal(e.StackTrace);
            }
            try
            {
                StartPoint = new CheckerPoint(0, 0);
                nextStepList.Clear();
                preRoad.Clear();
                IsMoveEnd = false;
            }
            catch (Exception e)
            {
                logger.Info("CheckerCore : ClearMove 逻辑异常捕获");
                logger.Fatal(e.Message);
                logger.Fatal(e.StackTrace);
            }
        }
        /// <summary>
        /// 自己走棋
        /// </summary>
        public void SubmitMove()
        {
            logger.Info("--- SubmitMove ---");

            //已经在了
            //move(StartPoint, preRoad.Peek());
            preRoad.Clear();    //清除后，会跳过ClearMove中棋子移动操作
            ClearMove();
        }
        /// <summary>
        /// 其他用户走棋
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        public void ShowMove(CheckerPoint s,CheckerPoint e)
        {
            logger.Info("--- ShowMove ---");

            try
            {
                Stack<CheckerPoint> path;
                
                    ClearMove();
                    if (!canMove(s, e))
                    {///???这我是没想到的！
                        printBoard();
                        throw new Exception("这我是没想到的！");
                    }
                    //UI
                    //CheckerPoint EndPoint = preRoad.Peek();
                    //UIhelper.MovePostion(EndPoint, StartPoint);   //测试：将棋子放回原来位置
                    //在其他人移动时，此时棋子就在起始点
                    //自己是不会显示动画的
                    path = new Stack<CheckerPoint>(preRoad.Count);
                    while (preRoad.Count != 0)
                        path.Push(preRoad.Pop());
                
                UIhelper.ShowMovePath(path);                        //显示动画

                //实际执行
                move(s, e);
                UIhelper.MovePostion(s, e);
            }
            catch (Exception ex)
            {
                logger.Info("CheckerCore : ShowMove 异常捕获");
                logger.Fatal(ex.Message);
                logger.Fatal(ex.StackTrace);
            }
        }

        /**
         * 返回当前能走的路
         * @param p 当前点
         * @param pre 之前走过的点
         * @return 当前可走的路 没有返回null
         */
        private List<CheckerPoint> getNextStep(CheckerPoint p)
        {
            if (p == null || isOut(p))
            {
                throw new Exception("getNextStep 接收到的参数有误！");
            }
            List<CheckerPoint> points = new List<CheckerPoint>();
            ///修复记录：不能直接对nextStepList进行修改，因为dfs的时候再foreach它
            ///不能在容器遍历时修改它，这会导致异常
            //nextStepList.Clear();

            CheckerPoint temp; bool isPre;
            for (int i = 0; i < 6; i++)
            {
                temp = getAJump(p, i);   //六个方向全看一遍
                if (temp != null)
                {
                    isPre = false;
                    foreach(CheckerPoint pp in preRoad)
                    {//判断有没有走之前走的
                        if (temp.Equals(pp))
                        {
                            isPre = true;
                            break;
                        }
                    }
                    if (!isPre)
                        points.Add(temp);
                }
            }
            return points;
        }

        /**
         * 搜索路径
         * @param end 终点
         * @param pre 路径
         * @return 是否可达
         */
        private bool dfs(CheckerPoint end)
        {
            try
            {
                CheckerPoint cur = preRoad.Peek();
                if (cur.Equals(end))
                    return true;
                List<CheckerPoint> next_point = getNextStep(cur);
                if (next_point.Count == 0)  //无处可走
                    return false;
                foreach (CheckerPoint pp in next_point)
                {   //这步可以走到
                    if (pp.Equals(end))
                    {
                        preRoad.Push(pp);
                        return true;
                    }
                }
                foreach (CheckerPoint pp in next_point)
                {   //尝试走这一步
                    preRoad.Push(pp);
                    if (dfs(end)) return true;
                    else preRoad.Pop();
                }
            }
            catch (Exception ex)
            {
                logger.Info("CheckerCore : dfs 异常捕获");
                logger.Fatal(ex.Message);
                logger.Fatal(ex.StackTrace);
            }
            return false;
        }

        /**
         * 是否可达
         * @param s 起点
         * @param e 终点
         * @return 是否可达(也可以返回路径)
         */
        public bool canMove(CheckerPoint s, CheckerPoint e)
        {
            logger.Info("--- canMove ---");

            try
            {
                //判断s和e的合法性
                if (s == null || e == null || isOut(s) || isOut(e) || isBlank(s) || !isBlank(e))
                    return false;

                // TODO: 2021/4/29 (怎么判断进了非对家？)

                //挪动操作
                CheckerPoint temp;
                for (int i = 0; i < 6; i++)
                {
                    temp = getAround(s, i);
                    if (!isOut(temp) && isBlank(temp) && e.Equals(temp))
                    {
                        preRoad.Push(s);
                        preRoad.Push(e);
                        return true;
                    }
                }

                //DFS搜索

                preRoad.Clear();
                preRoad.Push(s);
                if (dfs(e))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                logger.Info("CheckerCore : canMove 异常捕获");
                logger.Fatal(ex.Message);
                logger.Fatal(ex.StackTrace);
                return false;
            }
        }

        /**
         * 获取一个方向上的一跳
         * @param way 方向，与getAround规定一致
         * @return 返回落点，否则返回null
         */
        private CheckerPoint getAJump(CheckerPoint p, int way)
        {
            CheckerPoint temp = new CheckerPoint(p);
            for (int i = 1; ; i++)
            {
                temp = getAround(temp, way);
                if (isOut(temp)) 
                    return null;
                if (!isBlank(temp))
                {//不为空，找到中间节点，此时两点之间间隔i
                    for (int j = 1; j < i; j++)
                    {
                        temp = getAround(temp, way);
                        if (isOut(temp)||!isBlank(temp))
                            return null;
                    }
                    temp = getAround(temp, way);
                    if (!isOut(temp) && isBlank(temp))
                        return temp;
                    else 
                        return null;
                }
            }
        }

        /**
         * 执行移动
         * @param s 起点
         * @param e 终点
         */
        public void move(CheckerPoint s, CheckerPoint e)
        {
            logger.Info(string.Format("CheckerCore : move  {0}-->{1}",s.ToString(),e.ToString()));

            int temp = board[s.x,s.y];
            board[s.x,s.y] = 0;
            board[e.x,e.y] = temp;
        }

        /**
         * 获取玩家完成情况
         * @param player 玩家
         * @return 返回完成情况
         */
        public bool getFinished(int player)
        {
            return finished[player];
        }

        /**
         * 检查玩家是否完成了游戏
         * @param player 玩家
         * @return 完成:true 未完成:false
         */
        public bool isFinished(int player)
        {
            bool trType;
            CheckerPoint top; int length = 4;
            switch (player)
            {//这里要设置对家的信息
                case 1: trType = false; top = new CheckerPoint(16, 6); break;
                case 2: trType = true; top = new CheckerPoint(9, 1); break;
                case 3: trType = false; top = new CheckerPoint(7, 1); break;
                case 4: trType = true; top = new CheckerPoint(0, 6); break;
                case 5: trType = false; top = new CheckerPoint(7, 10); break;
                case 6: trType = true; top = new CheckerPoint(9, 10); break;
                default:// TODO: 2021/4/29 添加错误日志
                    logger.Error("isFinished 错误！传入值" + player);
                    return false;
            }
            CheckerPoint temp = new CheckerPoint(top);
            int way = trType ? 1 : 0;       //三角形方向，1左下，0左上
            int up = trType ? 1 : -1;       //增加方向
            for (int i = top.x, k = 1; up * i < up * top.x + length; i += up, k++)
            {
                for (int j = 0; j < k; j++)
                    if (board[i,temp.y + j] != player)
                        return false;
                temp = getAround(temp, way);
            }

            finished[player] = true;
            return true;
        }

        /**
         * 判断是否游戏结束(所有玩家都完成了游戏)
         * @return 结束：true 未结束：false
         */
        public bool isGameOver()
        {
            for (int i = 1; i <= MaxPlayer; i++)
                if (!finished[i])
                    return false;
            return true;
        }


        public void setCurPlayer(int player)
        {
            curPlayer = player;
        }
        /**
         * 获取当前回合移动的玩家
         * @return 玩家
         */
        public int getCurPlayer()
        {
            return curPlayer;
        }

        /**
        * 计算下一个回合移动的玩家
        * @return 返回下一个该走的，如果所有人都走完了返回-1
        */
        public int getNextPlayer()
        {
            for (int i = curPlayer; ;)
            {
                i = i % MaxPlayer + 1;
              
                if (!getFinished(i))
                    return i;
                if (i == curPlayer)    //循环了，其他都走完了
                    return -1;
            }
        }

        /**
         * 判断一点所属的区域
         * @param p 坐标
         * @return 区域号
         */
        private int getRegion(CheckerPoint p)
        {
            if (isOut(p))
                return -1;
            if (isInRegion(false, new CheckerPoint(7, 10), 4, p)) return 2;
            if (isInRegion(false, new CheckerPoint(16, 6), 4, p)) return 4;
            if (isInRegion(false, new CheckerPoint(7, 1), 4, p)) return 6;

            if (isInRegion(true, new CheckerPoint(0, 6), 4, p)) return 1;
            if (isInRegion(true, new CheckerPoint(9, 10), 4, p)) return 3;
            if (isInRegion(true, new CheckerPoint(9, 1), 4, p)) return 5;

            if (isInRegion(true, new CheckerPoint(0, 6), 13, p)) return 0;
            else return -1;
        }

        public int getChess(CheckerPoint p)
        {
            if (isOut(p))
                return -1;
            return board[p.x,p.y];
        }

        /**
         * 判断一点是否在区域内
         * @param trType 正三角形true or 倒三角形flase
         * @param top   顶点
         * @param length 边长
         * @param p 坐标
         * @return 在:true 不在:false
         */
        private bool isInRegion(bool trType, CheckerPoint top, int length, CheckerPoint p)
        {
            CheckerPoint temp = new CheckerPoint(top);
            int way = trType ? 1 : 0;       //三角形方向，1左下，0左上
            int up = trType ? 1 : -1;       //增加方向
            for (int i = top.x, k = 1; up * i < up * top.x + length; i += up, k++){
                if (i == p.x && temp.y <= p.y && p.y < temp.y + k)return true;
                temp = getAround(temp, way);
            }
            return false;
        }

        /**
         * 设置棋盘区域值
         * @param trType 正三角形true or 倒三角形flase
         * @param top   顶点
         * @param length 边长
         * @param value 值
         */
        private void setRegion(bool trType, CheckerPoint top, int length, int value)
        {
            CheckerPoint temp = new CheckerPoint(top);
            int way = trType ? 1 : 0;       //三角形方向，1左下，0左上
            int up = trType ? 1 : -1;       //增加方向
            for (int i = top.x, k = 1; up * i < up * top.x + length; i += up, k++)
            {
                for (int j = 0; j < k; j++)
                {
                    board[i, temp.y + j] = value;
                    ///界面显示
                    UIhelper.InitChecker(value, new CheckerPoint(i, temp.y + j));
                }
                temp = getAround(temp, way);
            }
        }

        /**
         * 判断是否出界（在棋盘上）
         * @param p 坐标
         * @return 出界:true 未出界:false
         */
        private bool isOut(CheckerPoint p)
        {
            if (p.x < 0 || p.x >= Row || p.y < 0 || p.y >= Col)
                return true;
            return board[p.x,p.y] == -1;
        }

        /**
         * 判断是否为空
         * @param p 点
         * @return 是否
         */
        private bool isBlank(CheckerPoint p)
        {
            return board[p.x,p.y] == 0;
        }

        /**
         * 获取周围方向的点
         * @param p 点
         * @param way 方向 0 左上  1 左下 2 右上 3 右下 4 左 5右
         * @return 周围一点
         * 测试样例： p(1,1) 0(0,1) 1(2,1) 2(0,2) 3(2,2) 4(1,0) 5(1,2)
         *          p(2,1) 0(1,0) 1(3,0) 2(1,1) 3(3,1) 4(2,0) 5(2,2)
         */
        private CheckerPoint getAround(CheckerPoint p, int way)
        {
            
            CheckerPoint temp = new CheckerPoint(p);
            if (way == 4)
                temp.y--;
            else if (way == 5)
                temp.y++;
            else
            {
                temp.x += way % 2 == 1 ? 1 : -1;
                if((p.x % 2)==0)
                    temp.y += AroundY[ way];
                else
                    temp.y += AroundY[4  +way];
            }
            return temp;
        }

        /**
         * 控制台打印棋盘
         */
        public void printBoard()
        {
            for (int i = 0; i < Row; i++)
            {
                if (i % 2 == 1)
                    Console.WriteLine("  ");
                for (int j = 0; j < Col; j++)
                {
                    if (board[i,j] == -1)
                        Console.Write("###%c", j == Col - 1 ? '\n' : ' ');
                else
                        Console.Write(" %d %c", board[i,j], j == Col - 1 ? '\n' : ' ');
                }
            }
        }


    }

}
