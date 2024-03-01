﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;

namespace DiscordBot
{
    public class CommandHandler
    {
        private readonly DiceGameManager _diceGameManager;

        private readonly WeatherHandler _weatherHandler;

        public CommandHandler(DiceGameManager diceGameManager, WeatherHandler weatherHandler)
        {
            _diceGameManager = diceGameManager;
            _weatherHandler = weatherHandler;
        }

        public async Task Execute(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case "coldest":
                    await command.RespondAsync(_weatherHandler.GetColdestTemperature((string)command.Data.Options.First().Value)); break;
                case "dice":
                    if (command.Data.Options.Count > 0)
                    {
                        int pts = Convert.ToInt32(command.Data.Options.First().Value);
                        if (pts < 20 || pts > 100)
                        {
                            await command.RespondAsync("20 és 100 közötti értéket adj meg!", ephemeral: true);
                        }
                        else
                        {
                            await _diceGameManager.HandleDiceCommand(command, pts);
                        }

                    }
                    else
                    {
                        await _diceGameManager.HandleDiceCommand(command);
                    }
                    break;
                case "hottest":
                    await command.RespondAsync(_weatherHandler.GetHottestTemperature((string)command.Data.Options.First().Value)); break;
                case "nextclear":
                    //case "nextsun":               //TODO: Implement alias handling in RegisterCommand()
                    await command.RespondAsync(_weatherHandler.GetNextClear((string)command.Data.Options.First().Value)); break;
                case "nextrain":
                    await command.RespondAsync(_weatherHandler.GetNextRain((string)command.Data.Options.First().Value)); break;
                case "nextsnow":
                    await command.RespondAsync(_weatherHandler.GetNextSnow((string)command.Data.Options.First().Value)); break;
                case "pong":
                    await command.RespondAsync("Pong"); break;
                case "wr":
                    if (command.Data.Options.Count == 1) //Nincs forecast
                    {
                        var response = _weatherHandler.GetWeatherDataForCity((string)command.Data.Options.First().Value);
                        if (response != null)
                        {
                            await command.RespondAsync(embed: _weatherHandler.GetWeatherDataForCity((string)command.Data.Options.First().Value).Build()); //FIXME: Reklamál, hogy possible null reference.
                        }
                        else
                        {
                            await command.RespondAsync("Nincs ilyen város");
                        }
                    }
                    else                                //Van forecast
                    {
                        var temp = command.Data.Options.FirstOrDefault(param => param.Name == "előrejelzés").Value;
                        int hours = Convert.ToInt32(temp);
                        if (hours > 100 || hours < 0)
                        {
                            await command.RespondAsync("3 és 100 óra közötti időtávot adj meg.", ephemeral: true);
                        }
                        else
                            await command.RespondAsync(embed: _weatherHandler.GetWeatherForecastForCity((string)command.Data.Options.First().Value, hours).Build());
                    }
                    break;
                default:
                    await command.RespondAsync("Valami nem jó"); break;
            }
        }
        public async Task RegisterCommand(DiscordSocketClient client, string name, string description, params SlashCommandOptionBuilder[] options)
        {
            var commandBuilder = new SlashCommandBuilder()
                .WithName(name)
                .WithDescription(description);

            foreach (var option in options)
            {
                commandBuilder.AddOption(option);
            }

            try
            {
                await client.CreateGlobalApplicationCommandAsync(commandBuilder.Build());
                Console.WriteLine($"DEBUG: {commandBuilder.Name} betöltve.");
            }
            catch (HttpException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
    }
}