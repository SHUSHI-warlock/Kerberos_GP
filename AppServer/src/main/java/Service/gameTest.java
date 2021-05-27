package Service;


import java.util.Scanner;

public class gameTest {
    CheckerCore game;


    public gameTest()
    {
        game = new CheckerCore();
    }

    public void StartGame(){
        Scanner sc = new Scanner(System.in);
        game.startGame(2);
        int cur;
        int aSteps=0,bSteps = 0;
        Point start = new Point(0,0);
        Point end = new Point(0,0);
        cur = game.getCurPlayer();
        while (!game.isGameOver())
        {
            System.out.println("输出当前棋盘");
            game.printBoard();
            System.out.println("当前轮到玩家"+cur+"行棋");
            System.out.print("请输入要走的棋的坐标：");
            start.x = sc.nextInt();
            start.y = sc.nextInt();
            System.out.print("请输入目标的坐标：");
            end.x = sc.nextInt();
            end.y = sc.nextInt();

            if(game.getChess(start)==cur && game.canMove(start,end)) {
                //移动
                game.move(start, end);
                if(cur==1)
                    aSteps++;
                else
                    bSteps++;
                //检测
                game.isFinished(cur);
                //游戏结束？
                if(game.isGameOver())
                {
                    System.out.println("游戏结束");
                    System.out.println("玩家a用步数："+aSteps);
                    System.out.println("玩家b用步数："+aSteps);
                    break;
                }
                else {
                    cur = game.getNextPlayer();
                }

            }
            else{
                System.out.println("错误移动！");
            }
        }
    }

    public static void main(String[] args) {
        gameTest test = new gameTest();
        test.StartGame();
    }
}
