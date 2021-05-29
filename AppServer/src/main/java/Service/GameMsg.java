package Service;

public class GameMsg {

    public GameMsg() { }
    public GameMsg(int GameState,int Pos)
    {
        gameState = GameState;
        pos = Pos;
    }
    public GameMsg(int GameState, int Pos,CheckerPoint S,CheckerPoint E)
    {
        gameState = GameState;
        pos = Pos;
        s = S;
        e = E;
    }

    // 游戏状态 0游戏开始 1游戏结束 2通知移动方 3广播移动结果
    public int gameState;
    // 玩家位置
    public int pos;
    // 起点
    public CheckerPoint s ;
    /// 终点
    public CheckerPoint e ;
}
