﻿using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace CHEF.Components.Watcher
{
    public class Watcher : Component
    {
        private readonly AutoPastebin _autoPastebin;
        private readonly ImageParser _imageParser;

        public Watcher(DiscordSocketClient client) : base(client)
        {
            _autoPastebin = new AutoPastebin();
            _imageParser = new ImageParser();
        }

        public override async Task SetupAsync()
        {
            Client.MessageReceived += MsgWatcherAsync;

            await Task.CompletedTask;
        }

        private Task MsgWatcherAsync(SocketMessage msg)
        {
            Task.Run(async () =>
            {
                var pasteBinRes = await _autoPastebin.Try(msg);

                if (pasteBinRes.Length > 1)
                    await msg.Channel.SendMessageAsync(pasteBinRes);

                var yandexRes = await _imageParser.Try(msg);

                if (yandexRes.Length > 1)
                    await msg.Channel.SendMessageAsync(yandexRes);

                if (msg.Content.Contains("can i ask", StringComparison.InvariantCultureIgnoreCase))
                {
                    await msg.Channel.SendMessageAsync($"{msg.Author.Mention} https://dontasktoask.com/");
                }
            });
            return Task.CompletedTask;
        }
    }
}
