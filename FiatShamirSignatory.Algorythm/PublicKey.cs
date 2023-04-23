using Org.BouncyCastle.Math;

namespace FiatShamirSignatory.Models
{
    public class PublicKey
    {
        private BigInteger[] b;
        private BigInteger n;

        public BigInteger[] B => b;
        public BigInteger N => n;

        public PublicKey(BigInteger n, BigInteger[] b)
        {
            this.n = n;
            this.b = b;
        }
    }
}
