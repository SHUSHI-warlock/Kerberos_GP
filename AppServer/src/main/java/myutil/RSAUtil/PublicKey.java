package myutil.RSAUtil;

import java.math.BigInteger;

public class PublicKey {
    private BigInteger n;
    private BigInteger e;
    public PublicKey(BigInteger n,BigInteger e)
    {
        this.n = n;
        this.e = e;
    }

    public BigInteger getE() {
        return e;
    }

    public BigInteger getN() {
        return n;
    }
}
