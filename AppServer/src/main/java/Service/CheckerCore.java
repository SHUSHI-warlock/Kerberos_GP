package Service;

import java.util.ArrayList;
import java.util.List;
import java.util.Stack;

public class CheckerCore {
    public static final int Row = 17;
    public static final int Col = 13;
    public static final int MaxPlayer = 6;
    public static final int[] AroundY =  {-1,-1,0,0,0,0,1,1};

    private boolean[] finished;
    private int curPlayer;
    private int playerNum;
    private int[][] board;

    public CheckerCore(){
        finished = new boolean[MaxPlayer+1];
        board = new int[Row][Col];
    };

    public void startGame(int playerNum)
    {
        //初始化棋盘
        for (int i = 0; i < Row; i++)
            for (int j = 0; j < Col; j++)
                board[i][j] = -1;

        setRegion(true,new Point(0,6), 13, 0);
        setRegion(false,new Point(7,10), 4, 0);
        setRegion(false,new Point(7,1), 4, 0);
        switch (playerNum)
            {
                case 6:
                    setRegion(true,new Point(9,10), 4, 5);
                    setRegion(false,new Point(7,1), 4, 6);
                case 4:
                    setRegion(true,new Point(9,1), 4, 4);
                    setRegion(false,new Point(7,10), 4, 3);
                case 2:
                    setRegion(true,new Point(0,6), 4, 1);
                    setRegion(false,new Point(16,6), 4, 2);
                    break;
            }

        //初始化玩家
        this.playerNum = playerNum;
        for (int i = 0; i <= playerNum; i++)
            finished[i]=false;
        curPlayer = 1;
    }

    /**
     * 返回当前能走的路
     * @param p 当前点
     * @param pre 之前走过的点
     * @return 当前可走的路 没有返回null
     */
    private List<Point> getNextStep(Point p,Stack<Point> pre) {
        List<Point> cur = new ArrayList<>(6);
        Point temp;boolean isPre;
        for (int i = 0; i < 6; i++) {
            temp = getAJump(p,i);   //六个方向全看一遍
            if(temp!=null){
                isPre = false;
                for (Point pp:pre) {//判断有没有走之前走的
                    if(temp.coincide(pp)){
                        isPre = true;
                        break;
                    }
                }
                if(!isPre)
                    cur.add(temp);
            }
        }
        if(cur.size()==0)return null;
        else return cur;
    }

    /**
     * 搜索路径
     * @param end 终点
     * @param pre 路径
     * @return 是否可达
     */
    private boolean dfs(Point end,Stack<Point> pre) {
        Point cur = pre.peek();
        if(cur.coincide(end)) return true;
        List<Point> curSteps = getNextStep(cur,pre);
        if(curSteps==null)  //无处可走
            return false;
        for (Point pp:curSteps) {   //这步可以走到
            if(pp.coincide(end)) {
                pre.push(pp);
                return true;
            }
        }
        for (Point pp:curSteps) {   //尝试走这一步
            pre.push(pp);
            if(dfs(end, pre)) return true;
            else pre.pop();
        }
        return false;
    }

    /**
     * 是否可达
     * @param s 起点
     * @param e 终点
     * @return 是否可达(也可以返回路径)
     */
    public boolean canMove(Point s,Point e) {
        //判断s和e的合法性
        if(isOut(s)||isOut(e)||isBlank(s)||!isBlank(e))
            return false;

        // TODO: 2021/4/29 (怎么判断进了非对家？)

        //挪动操作
        Point temp;
        for (int i = 0; i < 6; i++) {
            temp = getAround(s,i);
            if(!isOut(temp)&&isBlank(temp)&&e.coincide(temp)) return true;
        }

        //DFS搜索
        Stack<Point> steps = new Stack<>();
        steps.push(s);
        if(dfs(e,steps))
            return true;
        else
            return false;
    }

    /**
     * 获取一个方向上的一跳
     * @param way 方向，与getAround规定一致
     * @return 返回落点，否则返回null
     */
    private Point getAJump(Point p, int way){
        Point temp = new Point(p);
        for(int i=1;;i++) {
            temp = getAround(temp, way);
            if (isOut(temp)) return null;
            if (!isBlank(temp)) {//不为空，找到中间节点，此时两点之间间隔i
                for (int j = 0; j < i; j++)
                    temp = getAround(temp, way);
                if(!isOut(temp)&&isBlank(temp))
                    return temp;
                else return null;
            }
        }
    }

    /**
     * 执行移动
     * @param s 起点
     * @param e 终点
     */
    public void move(Point s,Point e) {
        int temp = board[s.x][s.y];
        board[s.x][s.y] = 0;
        board[e.x][e.y] = temp;
    }

    /**
     * 获取玩家完成情况
     * @param player 玩家
     * @return 返回完成情况
     */
    public boolean getFinished(int player) {
        return finished[player];
    }

    /**
     * 检查玩家是否完成了游戏
     * @param player 玩家
     * @return 完成:true 未完成:false
     */
    public boolean isFinished(int player) {
        boolean trType ;Point top;int length=4;
        switch (player)
        {//这里要设置对家的信息
            case 1:trType = false;top = new Point(16,6);break;
            case 3:trType = true; top = new Point(9,1); break;
            case 5:trType = false;top = new Point(7,1); break;
            case 2:trType = true; top = new Point(0,6); break;
            case 4:trType = false;top = new Point(7,10);break;
            case 6:trType = true; top = new Point(9,10);break;
            default:// TODO: 2021/4/29 添加错误日志
                System.out.println("错误");
                return false;
        }
        Point temp = new Point(top);
        int way = trType?1:0;       //三角形方向，1左下，0左上
        int up = trType?1:-1;       //增加方向
        for (int i = top.x, k=1; up*i < up*top.x+length; i+=up,k++) {
            for (int j = 0; j < k; j++)
                if(board[i][temp.y+j] != player)
                    return false;
            temp = getAround(temp,way);
        }

        finished[player] = true;
        return true;
    }

    /**
     * 判断是否游戏结束(所有玩家都完成了游戏)
     * @return 结束：true 未结束：false
     */
    public boolean isGameOver(){
        for (int i = 0; i < playerNum; i++)
            if(!finished[i])
                return false;
        return true;
    }

    /**
     * 获取当期回合移动的玩家
     * @return 玩家
     */
    public int getCurPlayer() {
        return curPlayer;
    }

    /**
     * 计算下一个回合移动的玩家
     * @return 如果其他人都完成就返回自己（哪怕自己也完成了）
     */
    public int getNextPlayer() {
        for(int i=curPlayer+1; ;i++)
        {
            i = (i-1)%playerNum+1;
            if(i==curPlayer)    //循环了，其他都走完了
                return curPlayer;
            if(!getFinished(i))
                return i;
        }
    }

    /**
     * 判断一点所属的区域
     * @param p 坐标
     * @return 区域号
     */
    private int getRegion(Point p) {
        if(isOut(p))
            return -1;
        if(isInRegion(false,new Point(7,10), 4, p)) return 3;
        if(isInRegion(false,new Point(16,6), 4, p)) return 2;
        if(isInRegion(false,new Point(7,1), 4, p))  return 6;

        if(isInRegion(true,new Point(0,6), 4, p))   return 1;
        if(isInRegion(true,new Point(9,10), 4, p))  return 5;
        if(isInRegion(true,new Point(9,1), 4, p))   return 4;

        if(isInRegion(true,new Point(0,6), 13, p))  return 0;
        else return -1;
    }

    public int getChess(Point p){
        if(isOut(p))
            return -1;
        return board[p.x][p.y];
    }

    /**
     * 判断一点是否在区域内
     * @param trType 正三角形true or 倒三角形flase
     * @param top   顶点
     * @param length 边长
     * @param p 坐标
     * @return 在:true 不在:false
     */
    private boolean isInRegion(boolean trType,Point top,int length,Point p) {
        Point temp = new Point(top);
        int way = trType?1:0;       //三角形方向，1左下，0左上
        int up = trType?1:-1;       //增加方向
        for (int i = top.x, k=1; up*i < up*top.x+length; i+=up,k++) {
            if(i==p.x&&temp.y<=p.y&&p.y<temp.y+k)
                return true;
            temp = getAround(temp,way);
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
    private void setRegion(boolean trType,Point top,int length,int value) {
        Point temp = new Point(top);
        int way = trType?1:0;       //三角形方向，1左下，0左上
        int up = trType?1:-1;       //增加方向
        for (int i = top.x, k=1; up*i < up*top.x+length; i+=up,k++) {
            for (int j = 0; j < k; j++)
                board[i][temp.y+j] = value;
            temp = getAround(temp,way);
        }
    }

    /**
     * 判断是否出界（在棋盘上）
     * @param p 坐标
     * @return 出界:true 未出界:false
     */
    private boolean isOut(Point p){
        if(p.x<0||p.x>=Row||p.y<0||p.y>=Col)
            return true;
        return board[p.x][p.y] == -1;
    }

    /**
     * 判断是否为空
     * @param p 点
     * @return 是否
     */
    private boolean isBlank(Point p) {
        return board[p.x][p.y]==0;
    }

    /**
     * 获取周围方向的点
     * @param p 点
     * @param way 方向 0 左上  1 左下 2 右上 3 右下 4 左 5右
     * @return 周围一点
     * 测试样例： p(1,1) 0(0,1) 1(2,1) 2(0,2) 3(2,2) 4(1,0) 5(1,2)
     *          p(2,1) 0(1,0) 1(3,0) 2(1,1) 3(3,1) 4(2,0) 5(2,2)
     */
    private Point getAround(Point p,int way){
        Point temp = new Point(p);
        if(way==4)
            temp.y--;
        else if(way == 5)
            temp.y++;
        else{
            temp.x += way%2==1?1:-1;
            temp.y += AroundY[4*(p.x%2)+way];
        }
        return temp;
    }

    /**
     * 控制台打印棋盘
     */
    public void printBoard() {
        for (int i = 0; i < Row; i++) {
            if(i%2==1)
                System.out.print("  ");
            for (int j = 0; j < Col; j++) {
                if(board[i][j]==-1)
                    System.out.printf("###%c",j==Col-1?'\n':' ');
                else
                    System.out.printf(" %d %c",board[i][j],j==Col-1?'\n':' ');
            }
        }
    }

    public static void main(String[] args) {
        CheckerCore game =  new CheckerCore();
        /*
        Point p = new Point(2,1);
        for (int i = 0; i < 6; i++) {
            System.out.println(game.getAround(p,i));
        }*/
        game.startGame(1);

        //game.printBoard();
        game.setRegion(true,new Point(0,6), 13, 0);
        game.setRegion(true,new Point(0,6), 4, 1);
        game.setRegion(true,new Point(9,10), 4, 5);
        game.setRegion(true,new Point(9,1), 4, 4);
        game.setRegion(false,new Point(7,10), 4, 3);
        game.setRegion(false,new Point(16,6), 4, 2);
        game.setRegion(false,new Point(7,1), 4, 6);

        System.out.println("");
        //game.printBoard();

        //验证区域判断错误
        for (int i = 0; i < Row; i++)
            for (int j = 0; j < Col; j++)
                if(game.getRegion(new Point(i,j))!=game.board[i][j]) {
                    System.out.printf("错误：[%d,%d]=%d region=%d\n",i,j,game.board[i][j],game.getRegion(new Point(i,j)));
                }

        //game.setRegion(false,new Point(7,10), 4, 4);
        //System.out.println(game.isFinished(4));

        game.board[6][5]=9;
        game.board[2][5]=0;
        game.board[2][7]=0;
        //game.board[9][3]=9;

        game.printBoard();
/*
        Stack<Point> pre = new Stack<>();
        for (Point pp: game.getNextStep(new Point(4,6),pre)) {
            System.out.println("下一跳:「"+pp.x+","+pp.y+"」");
        }
*/
        boolean a = game.canMove(new Point(0,6),new Point(8,4));
        System.out.println(a);
    }

}
