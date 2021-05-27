using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.core.Services
{
    public class Player : User
    {
        public Player() { }
        public Player(string userId) : base(userId)
        {
            pos = -1;
            steps = 0;
            roomId = -1;
        }
        public Player(string userId, int Pos, int roomId) : base(userId)
        {
            pos = Pos;
            steps = 0;
            this.roomId = roomId;
        }
        /// <summary>
        /// 
        /// </summary>
        public int pos { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int steps { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int roomId { get; set; }
       


    }
}
