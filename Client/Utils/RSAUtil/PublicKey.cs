using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Client.Utils.RSAUtil
{
    public class PublicKey
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

        public override string ToString()
        {
            return string.Format("n:{0}\ne:{1}",n,e);
        }

    }
}
