using FiatShamirSignatory.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Org.BouncyCastle.Math;
using FiatShamirSignatory.Algorythm;

namespace FiatShamirSignatory.App.Utils
{
    public static class FilesService
    {
        [Serializable]
        public class PublicKeyDto
        {
            public string[] B { get; set; }
            public string N { get; set; }

            public PublicKeyDto(PublicKey pk)
            {
                B = pk.B.Select(b => b.ToString()).ToArray();
                N = pk.N.ToString();
            }

            public PublicKeyDto()
            {
            }

            public PublicKey ToPublickKey() =>
                new PublicKey(new BigInteger(N), B.Select(b => new BigInteger(b)).ToArray());
        }


        [Serializable]
        public class SignedMessageDto
        {
            public string Message { get; set; }
            public string Hash { get; set; }
            public string S { get; set; }


            public SignedMessageDto(SignedMessage msg)
            {
                Message = msg.Message.UTF8();
                Hash = msg.Hash.Hex();
                S = msg.S.ToString();
            }

            public SignedMessageDto()
            {
            }

            public SignedMessage ToSignedMessage() =>
                new SignedMessage(Encoding.UTF8.GetBytes(Message), 
                    Enumerable.Range(0, Hash.Length)
                        .Where(x => x % 2 == 0)
                        .Select(x => Convert.ToByte(Hash.Substring(x, 2), 16))
                        .ToArray(),
                    new BigInteger(S));
        }


        public static PublicKey ReadPublickKey(string filename)
        {
            try
            {
                var formatter = new XmlSerializer(typeof(PublicKeyDto));
                using var fs = new FileStream(filename, FileMode.Open);

                var dto = (PublicKeyDto?)formatter.Deserialize(fs);

                if (dto == null)
                    throw new Exception("Не удалось прочитать открытый ключ.");

                return dto.ToPublickKey();
            }
            catch (Exception ex)
            {
                throw new Exception("Не удалось загрузить открытый ключ", ex);
            }
        }

        public static void SavePublickKey(PublicKey pk, string filename)
        {
            try
            {
                XmlSerializer formatter = new(typeof(PublicKeyDto));
                using FileStream fs = new(filename, FileMode.Create);
                formatter.Serialize(fs, new PublicKeyDto(pk));
            }
            catch (Exception ex)
            {
                throw new Exception("Не удалось сохранить открытый ключ.", ex);
            }
        }

        public static SignedMessage ReadSignedMessage(string filename)
        {
            try
            {
                var formatter = new XmlSerializer(typeof(SignedMessageDto));
                using var fs = new FileStream(filename, FileMode.Open);

                var message = (SignedMessageDto?)formatter.Deserialize(fs);

                if (message == null)
                    throw new Exception("Не удалось загрузить подписанный документ.");
               
                return message.ToSignedMessage();
            }
            catch (Exception ex)
            {
                throw new Exception("Не удалось загрузить  подписанный документ", ex);
            }
        }

        public static void SaveSignedMessage(SignedMessage message, string filename)
        {
            try
            {
                XmlSerializer formatter = new(typeof(SignedMessageDto));
                using FileStream fs = new(filename, FileMode.Create);
                formatter.Serialize(fs, new SignedMessageDto(message));
            }
            catch (Exception ex)
            {
                throw new Exception($"Не удалось сохранить сообщение.", ex);
            }
        }

        public static BigInteger[] ReadPrivateKey(string filename)
        {
            try
            {
                return File.ReadAllLines(filename).Select(l => new BigInteger(l)).ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception($"Не удалось загрузить закрытый ключ.", ex);
            }
        }

        public static void SavePrivateKey(BigInteger[] keys, string filename)
        {
            try
            {
                File.WriteAllLines(filename, keys.Select(k => k.ToString()));
            }
            catch (Exception ex)
            {
                throw new Exception($"Не удалось сохранить закрытый ключ.", ex);
            }
        }
    }
}
