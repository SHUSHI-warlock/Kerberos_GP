using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace AppServer
{
    class PrivateKey
    {

        private BigInteger n;   //p*q
        private BigInteger d;   //满足(d*e) mod ((p-1)*(q-1)) = 1（d称为私钥指数）

        public PrivateKey(BigInteger n, BigInteger d)
        {
            this.n = n;
            this.d = d;
        }

        public BigInteger getN()
        {
            return n;
        }

        public BigInteger getD()
        {
            return d;
        }
    }
}
