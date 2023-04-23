using Org.BouncyCastle.Math;

namespace FiatShamirSignatory.Models
{
    public class SignedMessage
    {
        private byte[] message;
        private byte[] hash;
        private BigInteger s;

        public byte[] Message => message;
        public byte[] Hash => hash;
        public BigInteger S => s;

        public SignedMessage(byte[] msg, byte[] hash, BigInteger s)
        {
            this.message = msg;
            this.hash = hash;
            this.s = s;
        }
    }
}
