package Service;

public class CheckerPoint {
    public int x;
    public int y;
    public CheckerPoint(int x, int y) {
        this.x = x;
        this.y = y;
    }
    public CheckerPoint(CheckerPoint op) {
        this.x = op.x;
        this.y = op.y;
    }

    @Override
    public String toString() {
        return String.format("(%d,%d)",x,y);
    }

    @Override
    public boolean equals(Object obj) {
        return this.coincide((CheckerPoint)obj);
    }

    @Override
    public int hashCode() {
        //二维映射到一位，N*N有限域
        return (x+y+1)*(x + y) / 2 + x;
    }

    /**
     * 判断重合
     * @return
     */
    public boolean coincide(CheckerPoint p){
        return p.x==x&&p.y==y;
    }
}
