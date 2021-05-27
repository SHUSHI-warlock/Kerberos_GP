using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;


namespace AppServer
{
    class KeyPair
    {
        private PrivateKey sk;
        private PublicKey pk;
        
        public PublicKey getPk()
        {
            return pk;
        }
        public PrivateKey getSk()
        {
            return sk;
        }

        public static void main(String[] args)
        {
            KeyPair keyPair = new KeyPair();
            keyPair.GenKey(1024);

            BigInteger a = 7;

            //BigInteger b = a.modPow((keyPair.getPk().getE()).multiply(keyPair.getSk().getD()), keyPair.getPk().getN());
            BigInteger b = BigInteger.ModPow(a,BigInteger.Multiply(keyPair.getPk().getE(), keyPair.getSk().getD()), keyPair.getPk().getN());
            Console.WriteLine(b);
        }

        public void GenKey(int length)
        {
            BigInteger p, q, n, phi;
            //e通常固定
            BigInteger e = 65537;
            BigInteger d;
            //选取p,q
            Random rnd = new Random(DateTime.Now.Millisecond);
            do
            {
                //p = GenPrime(length / 2);
                //q = GenPrime(length / 2);
                string p1 = "13270105224317614236751682578991306958403804140802264123436289071647772134721074621627810677892558393516891531173109559908800870712899771044050309478694619";
                string q1 = "361652441970737804256142513580635582151788610967881070532269319190443930912552472022168419490732880686065685721035639692926272455114358867727322622868121";
                p = BigInteger.Parse(p1);
                q = BigInteger.Parse(q1);
                //p = BigInteger.probablePrime(length / 2, rnd);              
                //q = BigInteger.probablePrime(length / 2, rnd);

                n = BigInteger.Multiply(p,q);
                //phi = p.subtract(BigInteger.One).multiply(q.subtract(BigInteger.One));
                phi = BigInteger.Multiply(BigInteger.Subtract(p, BigInteger.One), BigInteger.Subtract(q, BigInteger.One));
            } while (!BigInteger.GreatestCommonDivisor(phi,e).IsOne);

           
            //求d
            d = Inverse(e, phi);
            pk = new PublicKey(n, e);
            sk = new PrivateKey(n, d);
        }

        /**
         * 快速幂运算
         * @param p 底数
         * @param k 指数
         * @param mod 模数
         * @return 结果
         */
        private static BigInteger QuickPow(BigInteger p, BigInteger k, BigInteger mod)
        {
            // 等于0直接返回
            if (k.IsZero)
                return p.IsZero ? BigInteger.Zero : BigInteger.One;

            //计算的基数
            BigInteger base1 = p;
            //大于0
            
            if (p.CompareTo(mod) > 0)
                base1 = p%mod;
            //结果
            BigInteger res = BigInteger.One;

            while (!k.IsZero)
            {
                if (!k.IsEven)
                    
                {//getLowestSetBit返回最右侧的第一个‘1’位的位置
                 //等于0说明是奇数
                 //res = res.multiply(base1).mod(mod);
                   res = BigInteger.Multiply(res, base1) % mod;
                }
                //base1 = base1.multiply(base1).mod(mod);
                base1 = BigInteger.Multiply(base1, base1) % mod;
                k = k >> 1;
                
            }
            return res;
        }

        /**
         * 大质数生成器
         * @param length 长度
         * @return
         */
        private static BigInteger GenPrime(int length)
        {

            Random random = new Random();
            //return BigInteger.probablePrime(length,random);

            BigInteger res;
            do
            {
                res = BigInteger.Zero;
                for (int i = 0; i < length - 1; i++)
                {
                    res = res<<1;
                    if (random.Next(2) == 1)
                       
                        res = res + BigInteger.One;
                }
                //确保是奇数
                res = res<<1;
                res = BigInteger.Add(res,BigInteger.One);
                
            } while (!IsPrime(res));

            return res;

        }

        /**
         * MillerRabin算法
         * @param a 测试的底数
         * @param n 带测试的数
         * @return 返回是否可信
         */
        private static bool MillerRabbin(BigInteger a, BigInteger n)
        {
            //把n-1转化成（2^r）*d的形式
            int r = 0;
            BigInteger d = BigInteger.Subtract(n, BigInteger.One);
            
            while (!d.IsEven)
            {
                //仍是偶数
                r++;
                d = d>>1;
            }

            //逐个尝试(a^d)~(a^(d*r))看看是不是存在非平凡平方根
            BigInteger k = QuickPow(a, d, n);
            if (k.IsOne)
                return true;

            //n-1, -1
            BigInteger n_1 = BigInteger.Subtract(n, BigInteger.One);
            for (int i = 0; i < r; i++)
            {
                if (k.Equals(n_1))
                    return true;
                k = BigInteger.Multiply(k,k);
            }

            return false;
        }

        /**
         * IsPrime
         */
        private static bool IsPrime(BigInteger n)
        {
            if (n.Equals(2))
                return true;
            else
            {
                BigInteger[] tests = {2,3,5,7,11,233,331};
                foreach (BigInteger test in tests)
                {
                    if (n.Equals(test))
                        return true;
                    if (!MillerRabbin(test, n))
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
        private  class Result
        {
            public BigInteger X;
            public BigInteger Y;
            public Result(BigInteger x, BigInteger y)
            {
                X = x;
                Y = y;
            }
        }

        private static Result Exgcd(BigInteger a, BigInteger b)
        {//e*d+n*k=1
            if (b.IsZero)
                return new Result(BigInteger.One, BigInteger.Zero);
            else
            {
                Result r = Exgcd(b, a%b);
                //return new Result(r.Y, r.X.subtract(a.divide(b).multiply(r.Y)));
                return new Result(r.Y,BigInteger.Subtract(r.X,(a/b) * r.Y) );
            }
        }

        private static BigInteger Inverse(BigInteger e, BigInteger n)
        {
            Result r = Exgcd(e, n);
            while (r.X.CompareTo(BigInteger.Zero) < 0)
                r.X = r.X + n;

            return r.X;
        }

    }
}
