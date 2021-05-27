using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.core
{
    public class CheckerPoint : INotifyPropertyChanged
    {
        private int X;
        private int Y;

        public int x
        {
            get { return X; }
            set
            {
                X = value;
                if (this.PropertyChanged != null)
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("x"));
            }
        }
        public int y
        {
            get { return Y; }
            set
            {
                Y = value;
                if (this.PropertyChanged != null)
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("y"));
            }
        }
        public CheckerPoint()
        {
            x = 0;y = 0;
        }

        public CheckerPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public CheckerPoint(CheckerPoint op)
        {
            this.x = op.x;
            this.y = op.y;
        }

        //绑定更新事件
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string _property)
        {
            PropertyChangedEventHandler eventhandler = this.PropertyChanged;
            if (null == eventhandler)
                return;
            eventhandler(this, new PropertyChangedEventArgs(_property));
        }
        public override bool Equals(object obj)
        {
            return this.x==(obj as CheckerPoint).x && this.y==(obj as CheckerPoint).y;
        }

        override public String ToString()
        {
            return String.Format("({0},{1})", x, y);
        }

        public override int GetHashCode()
        {
            //二维映射到一位，N*N可数
            return (x+y+1)*(x + y) / 2 + x;
        }
    }
}
