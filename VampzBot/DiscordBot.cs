using System;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using VampzBot.Logic;

namespace VampzBot
{
    public class DiscordBot
    {
        private DiscordSocketClient _client;
        private CommandService _commandService;
        private IServiceProvider _services;

        private string _token;

        static void Main(string[] args) => new DiscordBot().Start().GetAwaiter().GetResult();

        public async Task Start()
        {
            SecretManager.GetSecrets();
            _token = SecretManager._secrets.DiscordToken;
            GoogleSheetsHandler._googleAPIKey = SecretManager._secrets.GoogleApiKey;
            GoogleSheetsHandler._googleSpreadsheetId = SecretManager._secrets.GoogleSpreadsheetId;

            _client = new DiscordSocketClient();
            _commandService = new CommandService();

            _services = new ServiceCollection().AddSingleton(_client).AddSingleton(_commandService).BuildServiceProvider();

            _client.Log += Log;

            await RegisterCommandAsync();

            await _client.LoginAsync(TokenType.Bot, _token);

            await _client.StartAsync();

            await _client.SetGameAsync("!help");

            await Task.Delay(-1);
        }

        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);

            return Task.CompletedTask;
        }

        public async Task RegisterCommandAsync()
        {
            _client.MessageReceived += HandleCommandAsync;

            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;

            if (message is null || message.Author.IsBot) return;

            int argPos = 0;

            if (message.HasStringPrefix("!", ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var context = new SocketCommandContext(_client, message);

                var result = await _commandService.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess)
                    Console.WriteLine(result.ErrorReason);
            }
        }
    }
}
