using System.Text;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Tavstal.DeltarBot.Models.Commands;
using Tavstal.DeltarBot.Services;
using Tavstal.WynnNetSDK.Exceptions;
using Tavstal.WynnNetSDK.Http;
using Tavstal.WynnNetSDK.Models.Leaderboard;

namespace Tavstal.DeltarBot.Commands.Leaderboard;

public class LeaderboardCommand : AutoCompleteCommand
{
    public LeaderboardCommand(DiscordSocketClient client, WynnHttpClient wynnClient, GuildService guildService) : base(
        "leaderboard",
        "Shows information about the provided leaderboard type.",
        client, wynnClient, guildService) { }

    public override SlashCommandBuilder HandleBuild(SlashCommandBuilder cmd)
    {
        SlashCommandOptionBuilder opt = new SlashCommandOptionBuilder()
            .WithName("leaderboard_type")
            .WithDescription("The type of leaderboard to show data.")
            .WithRequired(true)
            .WithAutocomplete(true)
            .WithType(ApplicationCommandOptionType.String);
        cmd.AddOption(opt);
        return cmd;
    }
    
    public override async Task HandleAsync(SocketSlashCommand data)
    {
        if (data.Data.Options.Count == 0)
        {
            await data.RespondAsync("You must provide arguments.", ephemeral: true);
            return;
        }
        
        var typeOption = data.Data.Options.ElementAt(0);
        var typeName = (string)typeOption.Value;
        string leaderboardName = Regex.Replace(typeName, "([A-Z])", " $1");
        leaderboardName = char.ToUpper(leaderboardName[0]) + leaderboardName.Substring(1);
        
        try
        {
            var result = await _wynnClient.Leaderboard.GetAsync(typeName, 10);

            if (result.IsError)
            {
                await data.RespondAsync(result.Error?.Message ?? "Failed to get leaderboard data.", ephemeral: true);
                return;
            }
            
            var resultDic = result.Value;
            var embed = new EmbedBuilder()
                .WithTitle($"{leaderboardName} Leaderboard")
                .WithThumbnailUrl("https://raw.githubusercontent.com/TavstalDev/DeltarBot/refs/heads/master/assets/images/icon_guild.png")
                .WithColor(Color.Blue)
                .WithCurrentTimestamp();

            var description = new StringBuilder();
            foreach (var entry in resultDic)
            {
                description.AppendLine($"**{entry.Key}.** "); 

                var val = entry.Value;
                
                if (val is LeaderboardPlayerEntry playerEntry)
                {
                    string playerName = playerEntry.Name ?? "Unknown Player";
                    string rank = string.IsNullOrWhiteSpace(playerEntry.SupportRank) ? "" : $"[{playerEntry.SupportRank}] ";
                    
                    description.AppendLine($"{rank}{playerName}");
                    description.AppendLine($"Score: {playerEntry.Score:N2} | Meta Score: {playerEntry.MetaScore}");
                }
                else if (val is LeaderboardGuildLegacyEntry legacyEntry)
                {
                    description.AppendLine($"[{legacyEntry.Prefix}] {legacyEntry.Name}");
                    description.AppendLine($"Level: {legacyEntry.Level} | XP: {legacyEntry.Xp:N0}");
                    description.AppendLine($"Territories: {legacyEntry.Territories} | Wars: {legacyEntry.Wars}");
                }
                else if (val is LeaderboardGuildEntry guildEntry)
                {
                    description.AppendLine($"[{guildEntry.Prefix}] {guildEntry.Name}");
                    description.AppendLine($"Score: {guildEntry.Score:N2} | Meta Score: {guildEntry.MetaScore}");
                }
    
                description.AppendLine();
            }
            embed.WithDescription(description.ToString());
            
            await data.RespondAsync(embed: embed.Build());
        }
        catch (RateLimitException ex)
        {
            await data.RespondAsync(ex.Message, ephemeral: true);
        }
    }
    
    public override async Task AutocompleteHandler(SocketAutocompleteInteraction autocomplete)
    {
        string userInput = autocomplete.Data.Current.Value?.ToString() ?? string.Empty;
        var matches = Program.WynnData.LeaderboardTypes
            .Where(x => x.Contains(userInput, StringComparison.OrdinalIgnoreCase))
            .Take(25) // Discord limit for autocomplete results
            .Select(x => new AutocompleteResult(x, x))
            .ToArray();
        await autocomplete.RespondAsync(matches);
    }
}