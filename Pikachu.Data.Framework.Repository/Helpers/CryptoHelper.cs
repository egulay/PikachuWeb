using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Pikachu.Data.Framework.Repository.Helpers
{
    public class CryptoHelper
    {
        public enum ReforgeLevel : short
        {
            Normal = 1,
            Medium = 5,
            High = 10
        }

        public enum IterationsLevel
        {
            Normal = 1000,
            Medium = 2000,
            High = 4000
        }

        private readonly byte[] _saltBytes;
        private readonly int _iterations;

        public CryptoHelper(string saltText, IterationsLevel iterationsLevel)
        {
            _saltBytes = Encoding.Unicode.GetBytes(saltText);
            _iterations = (int) iterationsLevel;
        }

        public string EncryptText(string plainText, string password, ReforgeLevel reforgeLevel)
        {
            if ((short) reforgeLevel == 1)
                return Encrypt(plainText, password);

            return Enumerable.Range(0, (short) reforgeLevel - 1)
                .ToArray()
                .Aggregate(Encrypt(plainText, password), (current, i) => Encrypt(current, password));
        }

        public string DecryptText(string cipherText, string password, ReforgeLevel reforgeLevel)
        {
            if ((short) reforgeLevel == 1)
                return Decrypt(cipherText, password);

            return Enumerable.Range(0, (short) reforgeLevel - 1)
                .ToArray()
                .Aggregate(Decrypt(cipherText, password), (current, i) => Decrypt(current, password));
        }

        private string Encrypt(string plainText, string password)
        {
            try
            {
                var plainBytes = Encoding.Unicode.GetBytes(plainText);
                var aes = Aes.Create();

                var pbkdf2 = new Rfc2898DeriveBytes(password, _saltBytes, _iterations);

                aes.Key = pbkdf2.GetBytes(32);
                aes.IV = pbkdf2.GetBytes(16);

                var ms = new MemoryStream();

                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(plainBytes, 0, plainBytes.Length);
                }

                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.GetAllMessages());
            }
        }

        private string Decrypt(string cryptoText, string password)
        {
            try
            {
                var cryptoBytes = Convert.FromBase64String(cryptoText);
                var aes = Aes.Create();

                var pbkdf2 = new Rfc2898DeriveBytes(password, _saltBytes, _iterations);

                aes.Key = pbkdf2.GetBytes(32);
                aes.IV = pbkdf2.GetBytes(16);

                var ms = new MemoryStream();

                using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cryptoBytes, 0, cryptoBytes.Length);
                }

                return Encoding.Unicode.GetString(ms.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.GetAllMessages());
            }
        }
    }
}
