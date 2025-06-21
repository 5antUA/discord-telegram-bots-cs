﻿using DSharpPlus;
using DSharpPlus.EventArgs;
using RostCont;

namespace MainSpace
{
    public sealed class DiscordBot
    {
        private static string? Token => Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");

        private DiscordClient? Client { get; set; }

        private readonly SwearPromptGenerator _promptGenerator;

        public DiscordBot(DIContainer container)
        {
            _promptGenerator = container.Resolve<SwearPromptGenerator>();
        }

        public async Task Start()
        {
            Client = new DiscordClient(GetDiscordConfig());

            Client.Ready += ClientOnReady;

            await Client.ConnectAsync();

            Client.MessageCreated += HandleMessage;
        }

        private DiscordConfiguration GetDiscordConfig()
        {
            return new DiscordConfiguration()
            {
                Token = Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
                AutoReconnect = true
            };
        }

        private async Task HandleMessage(DiscordClient client, MessageCreateEventArgs eventArgs)
        {
            if (eventArgs.Author.IsBot)
                return;

            var message = eventArgs.Message;
            var guild = eventArgs.Guild;
            var authorId = message.Author.Id;

            if (CheckMessageCondition(eventArgs))
            {
                /// message responce
                string username =
                    guild != null ?
                    (await eventArgs.Guild.GetMemberAsync(authorId)).Nickname :
                    message.Author.Username;

                string? referencedMessage = message.ReferencedMessage?.Content;
                string responce = await _promptGenerator.GenerateAsync(message.Content, username, referencedMessage);
                await message.RespondAsync(responce);
            }
        }

        private static bool CheckMessageCondition(MessageCreateEventArgs eventArgs)
        {
            var message = eventArgs.Message;

            return message.Content.ToLower().Contains(Configuration.KEYWORD) || message.ReferencedMessage != null;
        }

        private Task ClientOnReady(DiscordClient sender, ReadyEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
