using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Decryptor.Utils
{
    public class AES
    {
        public static byte[] Encrypt(byte[] plainBytes, byte[] bKey)
        {
            using (MemoryStream mStream = new MemoryStream())
            using (RijndaelManaged aes = new RijndaelManaged())
            {
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = bKey;

                using (
                    CryptoStream cryptoStream = new CryptoStream(
                        mStream,
                        aes.CreateEncryptor(),
                        CryptoStreamMode.Write
                    )
                )
                {
                    cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                    cryptoStream.FlushFinalBlock();
                }

                return mStream.ToArray();
            }
        }

        public static string Decrypt(byte[] encryptedBytes, byte[] bKey, byte[] iv)
        {
            using (MemoryStream mStream = new MemoryStream(encryptedBytes))
            using (RijndaelManaged aes = new RijndaelManaged())
            {
                aes.Mode = CipherMode.CFB;
                aes.FeedbackSize = 8;
                aes.Padding = PaddingMode.Zeros;
                aes.Key = bKey;
                aes.IV = iv;

                using (
                    CryptoStream cryptoStream = new CryptoStream(
                        mStream,
                        aes.CreateDecryptor(),
                        CryptoStreamMode.Read
                    )
                )
                using (StreamReader reader = new StreamReader(cryptoStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    };
}
