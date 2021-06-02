package myutil.RSAUtil;

import java.math.BigInteger;
import java.util.Random;


public class KeyPair {
    private PrivateKey sk;
    private PublicKey pk;

    public PublicKey getPk() {
        return pk;
    }
    public PrivateKey getSk(){
        return sk;
    }

    public static void main(String[] args) {
        KeyPair keyPair = new KeyPair();
        keyPair.GenKey(1024);

        BigInteger a = BigInteger.valueOf(7);
        BigInteger b = a.modPow((keyPair.getPk().getE()).multiply(keyPair.getSk().getD()),keyPair.getPk().getN());
        System.out.println(b);
//        BigInteger a = BigInteger.valueOf(5);
//        System.out.println(a);
//
//        BigInteger b = Inverse(a,BigInteger.valueOf(17));
//        System.out.println(b);
    }

    public void GenKey(int length)
    {
        BigInteger p,q,n,phi;
        //e通常固定
        BigInteger e = BigInteger.valueOf(65537);
        BigInteger d;
        //选取p,q
        Random rnd = new Random(System.currentTimeMillis());
        do {
            //p = GenPrime(length / 2);
            //q = GenPrime(length / 2);
            p = BigInteger.probablePrime(length/2,rnd);
            q = BigInteger.probablePrime(length/2,rnd);

            n = p.multiply(q);
            phi = p.subtract(BigInteger.ONE).multiply(q.subtract(BigInteger.ONE));
        } while(!phi.gcd(e).equals(BigInteger.ONE));
        //求d
        d = Inverse(e,phi);
//        BigInteger dd = e.modInverse(phi);
//        System.out.println(d);
//        System.out.println(dd);
//        System.out.println(d.multiply(e).subtract(BigInteger.ONE).mod(phi));
//        System.out.println(dd.multiply(e).subtract(BigInteger.ONE).mod(phi));

        pk = new PublicKey(n,e);
        sk = new PrivateKey(n,d);
    }

    /**
     * 快速幂运算
     * @param p 底数
     * @param k 指数
     * @param mod 模数
     * @return 结果
     */
    private static BigInteger QuickPow(BigInteger p,BigInteger k,BigInteger mod){
        // 等于0直接返回
        if(k.intValue()==0)
            return p.equals(BigInteger.ZERO)?BigInteger.ZERO:BigInteger.ONE;

        //计算的基数
        BigInteger base = p;
        //大于0
        if(p.compareTo(mod)>0)
            base = p.mod(mod);
        //结果
        BigInteger res = BigInteger.ONE;

        while (!k.equals(BigInteger.ZERO)) {
            if (k.getLowestSetBit() == 0) {//getLowestSetBit返回最右侧的第一个‘1’位的位置
                //等于0说明是奇数
                res = res.multiply(base).mod(mod);
            }
            base = base.multiply(base).mod(mod);
            k = k.shiftRight(1);
        }
        return res;
    }

    /**
     * 大质数生成器
     * @param length 长度
     * @return
     */
    private static BigInteger GenPrime(int length){

        Random random = new Random();
        //return BigInteger.probablePrime(length,random);

        BigInteger res;
        do {
            res = BigInteger.ZERO;
            for(int i=0;i<length-1;i++)
            {
                res = res.shiftLeft(1);
                if(random.nextInt(2)==1)
                    res = res.add(BigInteger.ONE);
            }
            //确保是奇数
            res = res.shiftLeft(1);
            res = res.add(BigInteger.ONE);

        } while (!IsPrime(res));

        return res;

    }

    /**
     * MillerRabin算法
     * @param a 测试的底数
     * @param n 带测试的数
     * @return 返回是否可信
     */
    private static boolean MillerRabbin(BigInteger a,BigInteger n)
    {
        //把n-1转化成（2^r）*d的形式
        int r=0;
        BigInteger d = n.subtract(BigInteger.ONE);
        while (d.getLowestSetBit()==0){
            //仍是偶数
            r++;
            d = d.shiftRight(1);
        }

        //逐个尝试(a^d)~(a^(d*r))看看是不是存在非平凡平方根
        BigInteger k = QuickPow(a,d,n);
        if(k.equals(BigInteger.ONE))
            return true;

        //n-1, -1
        BigInteger n_1 = n.subtract(BigInteger.ONE);
        for(int i=0;i<r;i++){
            if(k.equals(n_1))
                return true;
            k=k.multiply(k);
        }

        return false;
    }

    /**
     * IsPrime
     */
    private static boolean IsPrime(BigInteger n){
        if(n.equals(BigInteger.TWO))
            return true;
        else {
            BigInteger[] tests = {
                    BigInteger.valueOf(2),
                    BigInteger.valueOf(3),
                    BigInteger.valueOf(5),
                    BigInteger.valueOf(7),
                    BigInteger.valueOf(11),
                    BigInteger.valueOf(233),
                    BigInteger.valueOf(331)
            };
            for(BigInteger test:tests){
                if(n.equals(test))
                    return true;
                if(!MillerRabbin(test,n))
                    return false;
            }
            //所有探测都通过了
            return true;
        }
    }

    /**
     * 求d，e的逆元
     */
    //必须封装xy，因为BigInteger是一个引用类型，不能修改引用指向的位置
    private static class Result{
        public BigInteger X;
        public BigInteger Y;
        public Result(BigInteger x,BigInteger y)
        {
            X = x;
            Y = y;
        }
    }

    private static Result Exgcd(BigInteger a,BigInteger b)
    {//e*d+n*k=1
        if(b.equals(BigInteger.ZERO))
            return new Result(BigInteger.ONE,BigInteger.ZERO);
        else{
            Result r = Exgcd(b,a.mod(b));
            return new Result(r.Y,r.X.subtract(a.divide(b).multiply(r.Y)));
        }
    }

    private static BigInteger Inverse(BigInteger e,BigInteger n)
    {
        Result r = Exgcd(e,n);
        while (r.X.compareTo(BigInteger.ZERO)<0)
            r.X = r.X.add(n);

        return r.X;
    }


/*
    int gcdEx(int a,int b,int*x,int*y)
    {
        if(b==0)
        {
        *x=1,*y=0 ;
            return a ;
        }
        else
        {
            int r=gcdEx(b,a%b,x,y);
            // r = GCD(a, b) = GCD(b, a%b)
            int t=*x;
        *x=*y ;
        *y=t-a/b**y ;
            return r ;
        }
    }
*/
}
