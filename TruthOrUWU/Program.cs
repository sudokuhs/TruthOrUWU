using System;
using System.Configuration;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;
using truthOrUwU.Commands;

namespace truthOrUwU
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        internal static async Task MainAsync()
        {
            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = ConfigurationManager.AppSettings["Token"],
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged,
                MinimumLogLevel = LogLevel.Information,
                LogTimestampFormat = "MMM dd yyyy - hh:mm:ss tt"
            }); 
            var commands = discord.UseCommandsNext(new CommandsNextConfiguration() 
            { 
                StringPrefixes = new[] {ConfigurationManager.AppSettings["Prefix"]},
                EnableDefaultHelp = false,
                EnableDms = false
            });
            commands.RegisterCommands<ToDCommands>();

            await discord.ConnectAsync();
            await Task.Delay(-1);

            }
        }
    }
    public class User
    {
        public ulong id { get; private set; }
        public string username { get; private set; }
        public User(ulong id, string username)
        {
            this.id = id;
            this.username = username;
        }
    }
