using System.CommandLine;
using System.Reflection;
using Decryptor.Utils;

namespace Decryptor
{
    class Program
    {
        static int Main(string[] args)
        {
            // dotnet run 跳过项目路径参数
            // if (args.Length > 0 && args[0].EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            // {
            //     args = args.Skip(1).ToArray();
            // }

            // 创建根命令
            var rootCommand = new RootCommand("MobaXterm Decryptor Application");

            // 添加 --debug 选项作为全局选项
            var debugOption = new Option<bool>("--debug", "Enable debug mode");
            rootCommand.AddGlobalOption(debugOption);

            // 自动模式命令
            // var autoModeCommand = new Command("auto", "Run all tools in automatic mode");
            // autoModeCommand.SetHandler(
            //     (bool debug) =>
            //     {
            //         Logger.Initialize(debug);
            //         Logger.Info("Automatic mode");
            //         CallAllToolsClasses();
            //     },
            //     debugOption
            // );

            // rootCommand.AddCommand(autoModeCommand);

            // 动态加载子命令
            LoadSubCommands(rootCommand, debugOption);

            // 设置默认命令
            rootCommand.SetHandler(
                (bool debug) =>
                {
                    Logger.Initialize(debug);
                    Logger.Info("Automatic mode");
                    CallAllSubCommandClasses();
                },
                debugOption
            );

            return rootCommand.InvokeAsync(args).Result;
        }

        private static void LoadSubCommands(Command rootCommand, Option<bool> debugOption)
        {
            // 获取当前执行程序集
            Assembly assembly = Assembly.GetExecutingAssembly();

            // 查找 Decryptor.Softwares 命名空间下的所有类型
            var toolTypes = assembly
                .GetTypes()
                .Where(t => t.Namespace == "Decryptor.Softwares" && t.GetMethod("Run") != null)
                .ToList();

            foreach (var type in toolTypes)
            {
                var commandName = type.Name.ToLower();

                var toolCommand = new Command(commandName, $"Execute {commandName} command");

                // 添加位置参数
                var argsOption = new Argument<string[]>("args", "Arguments for the command")
                {
                    Arity = ArgumentArity.ZeroOrMore
                };

                toolCommand.AddArgument(argsOption);

                toolCommand.SetHandler(
                    (string[] classArgs, bool debug) =>
                    {
                        Logger.Initialize(debug);
                        Logger.Info($"Execute {commandName} command");
                        CallSpecificClass(type, classArgs);
                    },
                    argsOption,
                    debugOption
                );

                rootCommand.AddCommand(toolCommand);
            }
        }

        private static void CallSpecificClass(Type type, string[] classArgs)
        {
            try
            {
                object instance = Activator.CreateInstance(type);

                MethodInfo methodInfo = type.GetMethod("Run");

                if (methodInfo != null)
                {
                    methodInfo.Invoke(instance, new object[] { classArgs });
                }
                else
                {
                    Logger.Info($"Run method not found for class {type.Name}", label: "[-]");
                }
            }
            catch (Exception e)
            {
                Logger.Info(
                    $"An error occurred while calling class {type.Name}: {e.Message}",
                    label: "[-]"
                );
            }
        }

        private static void CallAllSubCommandClasses()
        {
            // 获取当前执行程序集
            Assembly assembly = Assembly.GetExecutingAssembly();

            // 查找 Decryptor.Softwares 命名空间下的所有类型
            var toolTypes = assembly
                .GetTypes()
                .Where(t => t.Namespace == "Decryptor.Softwares" && t.GetMethod("Run") != null)
                .ToList();

            foreach (var type in toolTypes)
            {
                try
                {
                    object instance = Activator.CreateInstance(type);
                    MethodInfo methodInfo = type.GetMethod("Run");
                    methodInfo.Invoke(instance, new object[] { new string[] { } });
                }
                catch (Exception e)
                {
                    Logger.Info(
                        $"An error occurred while calling class {type.Name}: {e.Message}",
                        label: "[-]"
                    );
                    continue;
                }
            }
        }
    }
}
