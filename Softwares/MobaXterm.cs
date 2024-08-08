using System;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Decryptor.Utils;
using IniParser;
using IniParser.Model;
using Microsoft.Win32;

namespace Decryptor.Softwares
{
    public class MobaXterm
    {
        private static (string UserPrincipalName, string MasterPassword) Sesspass = ("", "");
        private static string SessionP = "";
        private static string IniPath = "";
        private static int Installed = 0;

        // DPAPI 数据格式固定前缀字节数组
        private static readonly byte[] DpapiHeader =
        {
            0x01,
            0x00,
            0x00,
            0x00,
            0xd0,
            0x8c,
            0x9d,
            0xdf,
            0x01,
            0x15,
            0xd1,
            0x11,
            0x8c,
            0x7a,
            0x00,
            0xc0,
            0x4f,
            0xc2,
            0x97,
            0xeb
        };

        public string decryptWithoutMasterPassword(string ciphertext)
        {
            // Extend sessionP to at least 20 characters
            StringBuilder sessionPBuilder = new StringBuilder(SessionP);
            while (sessionPBuilder.Length < 20)
            {
                sessionPBuilder.Append(sessionPBuilder);
            }
            SessionP = sessionPBuilder.ToString().Substring(0, 20);

            // Construct s2 using Environment variables
            string s2 = (Environment.UserName + Environment.UserDomainName)
                .PadRight(20, ' ')
                .Substring(0, 20);

            // Create key space array with both upper and lower cases
            string[] keySpace = { SessionP.ToUpper(), SessionP.ToLower() };

            // Initialize the base key
            byte[] key = Encoding.UTF8.GetBytes("0d5e9n1348/U2+67");
            string validCharacters =
                "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz+/";

            for (int i = 0; i < key.Length; i++)
            {
                char potentialKeyChar = keySpace[(i + 1) % keySpace.Length][i % 20];
                if (
                    !key.Contains((byte)potentialKeyChar)
                    && validCharacters.Contains(potentialKeyChar)
                )
                {
                    key[i] = (byte)potentialKeyChar;
                }
            }

            HashSet<byte> keySet = new HashSet<byte>(key);
            List<byte> filteredText = new List<byte>();

            foreach (byte t in Encoding.ASCII.GetBytes(ciphertext))
            {
                if (keySet.Contains(t))
                {
                    filteredText.Add(t);
                }
            }

            byte[] ct = filteredText.ToArray();
            List<byte> ptArray = new List<byte>();

            if (ct.Length % 2 == 0)
            {
                for (int i = 0; i < ct.Length; i += 2)
                {
                    int l = Array.IndexOf(key, ct[i]);
                    key = RotateRightBytes(key);
                    int h = Array.IndexOf(key, ct[i + 1]);
                    key = RotateRightBytes(key);
                    ptArray.Add((byte)(16 * h + l));
                }

                return Encoding.UTF8.GetString(ptArray.ToArray());
            }

            return string.Empty;

            // Rotate the key bytes to the right by one position
            byte[] RotateRightBytes(byte[] input)
            {
                byte[] rotatedBytes = new byte[input.Length];
                Array.Copy(input, 0, rotatedBytes, 1, input.Length - 1);
                rotatedBytes[0] = input[input.Length - 1];
                return rotatedBytes;
            }
        }

        private string decryptWithMasterPassword(string ciphertext)
        {
            // 将 base64 字符串解码转换为字节数组
            byte[] MasterPasswordBytes = Convert.FromBase64String(Sesspass.MasterPassword);

            // 将 DPAPI 前缀和 MasterPassword 拼接出完整的加密数据
            byte[] fullEncryptedData = new byte[DpapiHeader.Length + MasterPasswordBytes.Length];
            Buffer.BlockCopy(DpapiHeader, 0, fullEncryptedData, 0, DpapiHeader.Length);
            Buffer.BlockCopy(
                MasterPasswordBytes,
                0,
                fullEncryptedData,
                DpapiHeader.Length,
                MasterPasswordBytes.Length
            );

            // 使用 DPAPI 解密。SessionP 为 DPAPI 加解密的 Entropy。
            byte[] temp = ProtectedData.Unprotect(
                fullEncryptedData,
                Encoding.UTF8.GetBytes(SessionP),
                DataProtectionScope.CurrentUser
            );

            // 将解密后的字节数组转换为字符串（现在获取到的这个字符串是 base64 编码过的）
            string temp2 = Encoding.UTF8.GetString(temp);

            // 将解密后的 base64 字符串解码转换为字节数组
            byte[] output = Convert.FromBase64String(temp2);

            // 提取 AES 密钥。
            byte[] aeskey = new byte[32];
            Array.Copy(output, aeskey, 32);

            // 生成初始向量
            byte[] ivbytes = AES.Encrypt(new byte[16], aeskey);
            byte[] iv = new byte[16];
            Array.Copy(ivbytes, iv, 16);

            // AES 解密，获取到明文密码。
            byte[] cipherBytes = Convert.FromBase64String(ciphertext);
            string plaintext = AES.Decrypt(cipherBytes, aeskey, iv);
            return plaintext;
        }

        public void Run(string[] args)
        {
            Initialize(args);

            // 不存在配置文件，就从注册表里获取信息。即使卸载了，可能还在注册表中保有密码。
            if (!File.Exists(IniPath))
            {
                // Output Credentials
                RegistryKey C = Registry.CurrentUser.OpenSubKey(@"Software\Mobatek\MobaXterm\C");
                if (C != null && C.GetValueNames().Length > 0)
                {
                    Logger.Info("MobaXterm Credentials: ");
                    foreach (string valueName in C.GetValueNames())
                    {
                        string name = valueName;
                        string[] temp = C.GetValue(valueName)
                            .ToString()
                            .Split(new char[] { ':' }, 2);

                        // string[] temp = C.GetValue(valueName).ToString().Split([':'], 2);
                        string userName = temp[0];
                        string ciphertext = temp[1];
                        output(name, userName, ciphertext);
                    }
                }

                // Output Passwords
                RegistryKey P = Registry.CurrentUser.OpenSubKey(@"Software\Mobatek\MobaXterm\P");
                if (P != null && P.GetValueNames().Length > 0)
                {
                    Logger.Info("MobaXterm Passwords: ");
                    foreach (string valueName in P.GetValueNames())
                    {
                        string connName = valueName;
                        string ciphertext = P.GetValue(valueName).ToString();
                        output(null, connName, ciphertext, false);
                    }
                }
            }
            else
            {
                // 从 ini 配置文件加载数据
                IniData data = new FileIniDataParser().ReadFile(IniPath);
                SessionP = data["Misc"]["SessionP"];
                // string MPSetAccount = data["Misc"]["MPSetAccount"];
                // string MPSetComputer = data["Misc"]["MPSetComputer"];
                // Sesspass.UserPrincipalName = $"{MPSetAccount}@{MPSetComputer}";
                Sesspass.MasterPassword = data["Sesspass"][Sesspass.UserPrincipalName];
                KeyDataCollection Passwords = data["Passwords"];
                KeyDataCollection Credentials = data["Credentials"];

                if (Credentials.Count > 0)
                {
                    Logger.Info("MobaXterm Credentials: ");
                    foreach (KeyData keyData in Credentials)
                    {
                        string name = keyData.KeyName;
                        string[] temp = keyData.Value.Split(new char[] { ':' }, 2);

                        // string[] temp = keyData.Value.Split([':'], 2);
                        string userName = temp[0];
                        string ciphertext = temp[1];
                        output(name, userName, ciphertext);
                    }
                }

                if (Passwords.Count > 0)
                {
                    Logger.Info("MobaXterm Passwords: ");
                    foreach (KeyData keyData in Passwords)
                    {
                        string connName = keyData.KeyName;
                        string ciphertext = keyData.Value;
                        output(null, connName, ciphertext, false);
                    }
                }
            }
        }

        private void output(
            string name,
            string username,
            string ciphertext,
            bool isCredential = true
        )
        {
            string plaintext = string.IsNullOrWhiteSpace(Sesspass.MasterPassword)
                ? decryptWithoutMasterPassword(ciphertext)
                : decryptWithMasterPassword(ciphertext);
            if (isCredential)
            {
                Logger.Info($"Name:     {name}", indent: true, label: "[*]");
                Logger.Info($"Username: {username}", indent: true, label: "[*]");
            }
            else
            {
                Logger.Info($"ConnName: {username}", indent: true, label: "[*]");
            }
            Logger.Info($"Password: {plaintext}", indent: true, label: "[*]");
            Logger.Info($"", label: "[*]");
        }

        private void Initialize(string[] args)
        {
            Sesspass.UserPrincipalName = $"{Environment.UserName}@{Environment.MachineName}";
            // 如果传递了配置文件路径，则从配置文件中加载信息进行解密。
            if (args != null && args.Length > 0)
            {
                IniPath = args[0];
            }
            else
            {
                // 便携版，尝试从进程信息中查找 MobaXterm.ini 文件。
                foreach (Process process in Process.GetProcesses())
                {
                    try
                    {
                        string description = process.MainModule.FileVersionInfo.FileDescription;
                        if (description == "MobaXterm")
                        {
                            Logger.Debug("Searching for MobaXterm process:");
                            Logger.Debug(
                                $"Process Name: {process.ProcessName}, Version: {process.MainModule.FileVersionInfo.ProductVersion}",
                                indent: true
                            );
                            Logger.Debug(
                                $"Process ID: {process.Id}, Path: {process.MainModule.FileName}",
                                indent: true
                            );
                            IniPath = Path.Combine(
                                Path.GetDirectoryName(process.MainModule.FileName),
                                "MobaXterm.ini"
                            );
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
            }
            // 安装版。如果没有加载到 MobaXterm.ini 文件，就从注册表中查询信息进行解密
            if (!string.IsNullOrWhiteSpace(IniPath) && File.Exists(IniPath))
            {
                Logger.Info($"MobaXterm.ini configuration file path: {IniPath}");
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(IniPath) && !File.Exists(IniPath))
                {
                    Logger.Debug(
                        $"MobaXterm.ini configuration file does not exist: {IniPath}",
                        label: "[-]"
                    );
                }
                Logger.Debug(
                    @"Read information from the registry: HKEY_CURRENT_USER\Software\Mobatek\MobaXterm\"
                );

                // 也可以通过注册表项 SessionP 判断主机上是否运行过 MobaXterm，因为不管是安装版本（Installer），还是便携版本（Portable）运行过都会有这个注册表项。
                SessionP = (string)
                    Registry.GetValue(
                        @"HKEY_CURRENT_USER\Software\Mobatek\MobaXterm\",
                        "SessionP",
                        ""
                    );

                // 少数逆天情况，即使注册表中没有 SessionP，也会默认在“用户文档”目录下存在配置文件。
                string tempIniPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    @"MobaXterm\MobaXterm.ini"
                );

                // 当注册表没有 SessionP，且没有有效的 ini 配置文件的时候，判定电脑中没有该软件。
                if (string.IsNullOrWhiteSpace(SessionP) && !File.Exists(tempIniPath))
                {
                    Logger.Info("MobaXterm does not exist on this machine.", label: "[x]");
                    return;
                }
                else if (File.Exists(tempIniPath))
                {
                    IniPath = tempIniPath;
                    Logger.Info($"MobaXterm.ini configuration file path: {IniPath}");
                }

                // 通过注册表项 installed 是否存在，来判断主机上的 MobaXterm 是安装版本（Installer），还是便携版本（Portable）。
                object temp_installed = Registry.GetValue(
                    @"HKEY_CURRENT_USER\Software\Mobatek\MobaXterm\",
                    "installed",
                    0
                );

                if (temp_installed != null && temp_installed is int)
                {
                    Installed = (int)temp_installed;
                }

                // Sesspass.UserPrincipalName = $"{Environment.UserName}@{Environment.MachineName}";
                // Master Password
                Sesspass.MasterPassword =
                    (string)
                        Registry.GetValue(
                            @"HKEY_CURRENT_USER\Software\Mobatek\MobaXterm\M",
                            Sesspass.UserPrincipalName,
                            ""
                        ) ?? string.Empty;
            }
            if (Installed.Equals(0))
            {
                Logger.Info("MobaXterm Portable Edition");
            }
            else
            {
                Logger.Info("MobaXterm Installer Edition");
            }
        }
    }
}
