package Service;

public class Point {
    public int x;
    public int y;
    public Point(int x,int y) {
        this.x = x;
        this.y = y;
    }
    public Point(Point op) {
        this.x = op.x;
        this.y = op.y;
    }

    @Override
    public String toString() {
        return String.format("(%d,%d)",x,y);
    }

    /**
     * 判断重合
     * @return
     */
    public boolean coincide(Point p){
        return p.x==x&&p.y==y;
    }
}
