using System.Collections;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Math;
using FiatShamirSignatory.Models;

namespace FiatShamirSignatory.Algorythm
{
    public class FSSignatory 
    {
        private BigInteger[] privateA;
        private PublicKey publicKey;

        public PublicKey PublicKey => publicKey;
        public BigInteger[] PrivateA => privateA;
        public int CountM => PrivateA.Length;


        public FSSignatory(BigInteger n, int countM, Random? rnd = null)
        {
            (privateA, BigInteger[] privateB) = GenerateKeys(n, countM, rnd: rnd);
            publicKey = new PublicKey(n, privateB);
        }

        public FSSignatory(int countM, int nLength = 256, Random? rnd = null)
        {
            var n = GenerateN(nLength, rnd);
            (privateA, BigInteger[] privateB) = GenerateKeys(n, countM, rnd: rnd);
            publicKey = new PublicKey(n, privateB);
        }

        public FSSignatory(BigInteger n, BigInteger[] publicB, BigInteger[] privateA)
        {
            VerifyKeys(privateA, publicB, n);

            this.privateA = privateA;
            this.publicKey = new PublicKey(n, publicB);
        }

        public FSSignatory(PublicKey pk, BigInteger[] privateA)
        {
            VerifyKeys(privateA, pk.B, pk.N);

            this.privateA = privateA;
            this.publicKey = pk;
        }

        public SignedMessage Sign(string msg, Random? rnd = null)
        {
            (BigInteger k, BigInteger r) = GenerateKR(publicKey.N, rnd);

            Trace.WriteLine($"Sign:");
            Trace.IndentLevel++;
            Trace.WriteLine($"Inputs");
            Trace.IndentLevel++;
            Trace.WriteLine($"msg = '{msg}'");
            Trace.WriteLine($"n = {publicKey.N}");
            Trace.WriteLine($"b = {{{publicKey.B.Join()}}}");
            Trace.IndentLevel--;
            Trace.WriteLine($"Calculations");
            Trace.IndentLevel++;

            byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
            byte[] hash = GetSha256(msgBytes, r, privateA.Length);

            int intend = Trace.IndentLevel;
           
            Trace.WriteLine($"k = {k}");
            Trace.WriteLine($"r = {r}");
            Trace.WriteLine($"Hash = {{{hash.Join()}}}");

            BigInteger s = GenerateS(privateA, k, hash, publicKey.N);

            Trace.IndentLevel = intend + 2;
            Trace.WriteLine($"s = {s}");
            Trace.IndentLevel -= 2;
            Trace.WriteLine("Sign over ▶");

            return new SignedMessage(msgBytes, hash, s);
        }

        public static bool Verify(SignedMessage signedMessage, PublicKey pk)
        {
            // _r = s^2 * (b_i ^ h_i for i [1,m]) (mod n)
            Trace.WriteLine("Verify:");
            Trace.IndentLevel++;
            Trace.WriteLine($"Inputs");
            Trace.IndentLevel++;
            Trace.WriteLine($"msg = '{signedMessage.Message.UTF8()}'");
            Trace.WriteLine($"s = {signedMessage.S}");
            Trace.WriteLine($"Hash = {{{signedMessage.Hash.Join()}}}");
            Trace.WriteLine($"b = {{{pk.B.Join()}}}");
            Trace.WriteLine($"n = {pk.N}");
            Trace.IndentLevel--;
            Trace.WriteLine($"Calculations");
            Trace.IndentLevel++;
            int indent = Trace.IndentLevel;
            Trace.WriteLine("GenerateS(A := pk.B, k := S^2, hash := msg.Hash, n := pk.N);");
            BigInteger _r = GenerateS(pk.B, signedMessage.S.ModPow(BigInteger.Two, pk.N), signedMessage.Hash, pk.N);
            byte[] generatedHash = GetSha256(signedMessage.Message, _r, pk.B.Length);

            Trace.IndentLevel = indent;
            Trace.WriteLine($"_Hash= {{{generatedHash.Join()}}}");
            Trace.IndentLevel -= 2;

            for (int i = 0; i < signedMessage.Hash.Length && i < generatedHash.Length; i++)
            {
                if (generatedHash[i] != signedMessage.Hash[i])
                {
                    Trace.WriteLine("Verify over ▶");
                    return false;
                }
            }
            Trace.WriteLine("Verify over ▶");
            return true;
        }

        public static BigInteger GenerateN(int length = 256, Random? rnd = null)
        {
            BigInteger p = BigInteger.ProbablePrime(length, rnd ?? new Random());
            BigInteger q = BigInteger.ProbablePrime(length, rnd ?? new Random());
            return p.Multiply(q);
        }

        public static void VerifyKeys(BigInteger[] a, PublicKey pk)
        {
            VerifyKeys(a, pk.B, pk.N);
        }

        public static void VerifyKeys(BigInteger[] a, BigInteger[] b, BigInteger n)
        {
            if (a.Length != b.Length)
                throw new ArgumentException("Количество чисел в открытом ключе B должно быть равно количеству чисел в закрытом ключе A.");

            if (a.Any(_a => _a.CompareTo(n) != -1 && _a.Gcd(n) != BigInteger.One))
                throw new ArgumentException($"Числа из закрытого ключа A не удовлетворяют условиям a < n, a = 1 (mod n) -> a={a}, n={n}");

            for (int i = 0; i < a.Length; i++)
            {
               // if (b[i] != a[i].ModInverse(n).ModPow(BigInteger.Two, n))
               //     throw new ArgumentException($"Числа открытого ключа B не удовлетворяет условию b = sqrt(a^-1) (mod n) -> Ai={a[i]}, Bi={b[i]}, n={n}");
            }
        }


        private static (BigInteger[] privateA, BigInteger[] publicB) GenerateKeys(BigInteger n, int count, Random? rnd = null)
        {
            BigInteger[] A = new BigInteger[count];
            BigInteger[] B = new BigInteger[count];

            for (int i = 0; i < count; i++)
            {
                BigInteger a = BigInteger.ProbablePrime(n.BitLength - 1, rnd ?? new Random());
                while (a.CompareTo(n) != -1 && a.Gcd(n) != BigInteger.One)
                    a = BigInteger.ProbablePrime(n.BitLength - 1, rnd ?? new Random());

                A[i] = a;
                B[i] = a.ModInverse(n).ModPow(BigInteger.Two, n);
            }

            return (A, B);
        }

        private static (BigInteger k, BigInteger r) GenerateKR(BigInteger n, Random? rnd = null)
        {
            BigInteger k = new BigInteger(n.BitLength - 1, rnd ?? new Random());
            BigInteger r = k.ModPow(BigInteger.Two, n);
            return (k, r);
        }

        private static byte[] GetSha256(byte[] msg, BigInteger r, int countM)
        {
            byte[] combined = msg.Concat(r.ToByteArrayUnsigned()).ToArray();
            using SHA256 sha256 = SHA256.Create();
            return sha256.ComputeHash(combined)[0..(countM - 1)];
        }

        private static BigInteger GenerateS(BigInteger[] privateA, BigInteger k, byte[] combinedHash, BigInteger n)
        {
            if (combinedHash.Length * 8 < privateA.Length)
                throw new ArgumentException("Hash length must be greater or equal then A array length");

            BigInteger y = BigInteger.One;
            BitArray hashBits = new BitArray(combinedHash);
            
            Trace.WriteLine("GenerateS:");
            Trace.IndentLevel++;
            Trace.WriteLine("Inputs");
            Trace.IndentLevel++;
            Trace.WriteLine($"a = {{{privateA.Join()}}}");
            Trace.WriteLine($"k = {k}");
            Trace.WriteLine($"n = {n}");
            Trace.WriteLine($"hash = {{{combinedHash.Join()}}}");
            Trace.IndentLevel--;
            Trace.WriteLine("Calculations");
            Trace.IndentLevel++;
            Trace.WriteLine($"bits = {hashBits.ToBitString()}");
            Trace.Write($"s = {y}");

            for (int i = 0; i < privateA.Length; i++)
            {
                // s = k * (a_i ^ hash_i for i [0, m]) (mod n)
                if (hashBits[i])
                {
                    Trace.Write($" * {privateA[i]}^1");
                    y = y.Multiply(privateA[i]);
                }
            }
            y = y.Multiply(k).Mod(n);
            Trace.WriteLine($" * {k} (mod {n}) = {y}");
            Trace.IndentLevel -= 2;
            Trace.WriteLine("GenerateS over ▶");

            return y;
        }
    }
}
