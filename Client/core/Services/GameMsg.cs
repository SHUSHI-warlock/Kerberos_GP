using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.core.Services
{
    /// <summary>
    /// 通报游戏消息
    /// </summary>
    public class GameMsg
    {
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

        /// <summary>
        /// 游戏状态 0游戏开始 1游戏结束 2通知移动方 3移动结果 4玩家跳过
        /// </summary>
        public int gameState { get; set; }
        /// <summary>
        /// 玩家位置
        /// </summary>
        public int pos { get; set; }
        /// <summary>
        /// 起点
        /// </summary>
        public CheckerPoint s { get; set; }
        /// <summary>
        /// 终点
        /// </summary>
        public CheckerPoint e { get; set; }

    }

}
