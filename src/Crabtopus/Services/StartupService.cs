﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Crabtopus.App.Services
{
    public class StartupService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly CardsService _cardsService;

        public StartupService(IServiceProvider provider, DiscordSocketClient discord, CommandService commands, IConfigurationRoot config, CardsService cardsService)
        {
            _provider = provider;
            _config = config;
            _discord = discord;
            _commands = commands;
            _cardsService = cardsService;
        }

        public async Task StartAsync()
        {
            string discordToken = _config["Token"];

            await _discord.LoginAsync(TokenType.Bot, discordToken);
            await _discord.StartAsync();

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);

            await _cardsService.LoadCardsAsync();
        }
    }
}