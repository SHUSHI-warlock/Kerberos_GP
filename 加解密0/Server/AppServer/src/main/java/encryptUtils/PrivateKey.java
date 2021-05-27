package encryptUtils;

import java.math.BigInteger;

public class PrivateKey {
    private BigInteger n;   //p*q
    private BigInteger d;   //满足(d*e) mod ((p-1)*(q-1)) = 1（d称为私钥指数）

    PrivateKey(BigInteger n,BigInteger d){
        this.n = n;
        this.d = d;
    }

    public BigInteger getN() {
        return n;
    }

    public BigInteger getD() {
        return d;
    }
}
