using FiatShamirSignatory;
using FiatShamirSignatory.Algorythm;
using FiatShamirSignatory.Models;
using Org.BouncyCastle.Math;
using System.Linq;

namespace FiatShamirSignatory.App.ViewModels
{
    internal class MainWindowVM : BaseVM
    {
        private string text = "";
        private string? signatoryName;
        private PublicKey? publicKey;
        private BigInteger[]? privateKey;
        private bool? isVerified;

        public string Text
        {
            get => text;
            set => SetField(ref text, value);
        }
        public string? SignatoryName
        {
            get => signatoryName;
            set => SetField(ref signatoryName, value);
        }
        public BigInteger[]? PrivateKey
        {
            get => privateKey;
            set => SetField(ref privateKey, value).AddRelated("PrivateKeyAString", "CanSaveKeys");
        }
        public PublicKey? PublicKey
        {
            get => publicKey;
            set => SetField(ref publicKey, value).AddRelated("CanSaveKeys", "PublicKeyNString", "PublicKeyBString", "CanSaveKeys", "CanVerify");
        }
        public bool? IsVerified
        {
            get => isVerified;
            set => SetField(ref isVerified, value).AddRelated("VerificationResult");
        }

        public bool CanVerify => publicKey != null;
        public bool CanSaveKeys => publicKey != null && privateKey != null;
        public string PublicKeyNString => publicKey?.N.ToString() ?? "";
        public string PublicKeyBString => publicKey?.B.Join("\n") ?? "";
        public string PrivateKeyAString => privateKey?.Join("\n") ?? "";
        public string VerificationResult => IsVerified switch
        {
            true => "Сообщение [подлинное]",
            false => "Сообщение [не подлинное]",
            _ => "Сообщение"
        };
    }
}
