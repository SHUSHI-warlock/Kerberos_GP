using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace AppServer
{
    class PublicKey
    {
        private BigInteger n;
        private BigInteger e;
        public PublicKey(BigInteger n, BigInteger e)
        {
            this.n = n;
            this.e = e;
        }

        public BigInteger getE()
        {
            return e;
        }

        public BigInteger getN()
        {
            return n;
        }
    }
}
