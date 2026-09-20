using System.Text;
using Discord;
using Discord.WebSocket;
using Tavstal.DeltarBot.Extensions;
using Tavstal.DeltarBot.Models.Commands;
using Tavstal.DeltarBot.Services;
using Tavstal.WynnNetSDK.Exceptions;
using Tavstal.WynnNetSDK.Http;

namespace Tavstal.DeltarBot.Commands.Player;

public class PlayerFindCommand : SimpleCommand
{
    public PlayerFindCommand(DiscordSocketClient client, WynnHttpClient wynnClient, GuildService guildService) : base(
        "pfind",
        "Finds a player and their global stats.",
        client, wynnClient, guildService) { }

    public override SlashCommandBuilder HandleBuild(SlashCommandBuilder cmd)
    {
        SlashCommandOptionBuilder opt = new SlashCommandOptionBuilder()
            .WithName("player")
            .WithDescription("The name of the player you want to find.")
            .WithRequired(true)
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
        
        var playerOption = data.Data.Options.ElementAt(0);
        var playerName = (string)playerOption.Value;
        
        try
        {
            var result = await _wynnClient.Player.GetProfileAsync(playerName);

            if (result.IsError)
            {
                await data.RespondAsync(result.Error?.Message ?? "Failed to get leaderboard data.", ephemeral: true);
                return;
            }
            
            var resultData = result.Value;
            var embed = new EmbedBuilder()
                .WithTitle($"Profile Of {resultData.Username}")
                .WithUrl("https://github.com/TavstalDev/DeltarBot")
                .WithThumbnailUrl($"https://render.crafty.gg/2d/head/{resultData.Username}")
                .WithColor(Color.Blue)
                .WithCurrentTimestamp()
                .WithFooter("DeltarBot");

            var description = new StringBuilder();
            description.AppendLine($"**UUID:** {resultData.Uuid}");
            string onlineText = resultData.Online ? $"Online ({resultData.Server})" : "Offline";
            description.AppendLine($"**Status:** {onlineText}");
            description.AppendLine($"**Supporter Rank:** {resultData.SupportRank}");
            description.AppendLine($"**Rank:** {resultData.Rank}");
            if (resultData.Guild == null)
                description.AppendLine($"**Guild:** N/A");
            else
                description.AppendLine($"**Guild:** [{resultData.Guild.Prefix}] {resultData.Guild.Name} - {resultData.Guild.Rank}");
            description.AppendLine($"**First Join:** {resultData.FirstJoin:yyyy MMMM dd}");
            if (resultData.LastJoin != null)
                description.AppendLine($"**Last Join:** {resultData.LastJoin:yyyy MMMM dd}");
            description.AppendLine();

            description.AppendLine("**Progress**");
            description.AppendLine($"> **Total Levels:** {resultData.GlobalData.TotalLevel}");
            description.AppendLine($"> **Playtime:** {resultData.Playtime:0.00} hours");
            description.AppendLine($"> **Dungeons:** {resultData.GlobalData.Dungeons.Total.ToShortString()}");
            description.AppendLine($"> **Quests:** {resultData.GlobalData.CompletedQuests.ToShortString()}");
            description.AppendLine($"> **World Events:** {resultData.GlobalData.WorldEvents.ToShortString()}");
            description.AppendLine($"> **Lootruns:** {resultData.GlobalData.Lootruns.ToShortString()}");
            description.AppendLine($"> **Chests Opened:** {resultData.GlobalData.ChestsFound.ToShortString()}");
            description.AppendLine($"> **Mobs Killed:** {resultData.GlobalData.MobsKilled.ToShortString()}");
            description.AppendLine();

            description.AppendLine("**PvP Stats**");
            description.AppendLine($"> **Wars:** {resultData.GlobalData.Wars.ToShortString()}");
            double kdr = (double)resultData.GlobalData.PvP.Kills / Math.Clamp( resultData.GlobalData.PvP.Deaths, 1, int.MaxValue);
            description.AppendLine($"> **K/D:** {resultData.GlobalData.PvP.Kills}/{resultData.GlobalData.PvP.Deaths} ({kdr:0.00})");
            description.AppendLine();
            
            description.AppendLine("**Raid Stats**");
            description.AppendLine($"> **Damage Dealt:** {resultData.GlobalData.RaidStats.DamageDealt.ToShortString()}");
            description.AppendLine($"> **Damage Taken:** {resultData.GlobalData.RaidStats.DamageTaken.ToShortString()}");
            description.AppendLine($"> **Healing:** {resultData.GlobalData.RaidStats.HealthHealed.ToShortString()}");
            description.AppendLine($"> **Deaths:** {resultData.GlobalData.RaidStats.Deaths.ToShortString()}");
            description.AppendLine($"> **Buffs Taken:** {resultData.GlobalData.RaidStats.BuffsTaken.ToShortString()}");
            description.AppendLine($"> **Gambits Taken:** {resultData.GlobalData.RaidStats.GambitsUsed.ToShortString()}");
            description.AppendLine();
            
            description.AppendLine("**Raid Completions (Normal/Guid)**");
            foreach (var raid in resultData.GlobalData.Raids.List.OrderBy(x => x.Key))
            {
                resultData.GlobalData.GuildRaids.List.TryGetValue(raid.Key, out int raidValue);
                description.AppendLine($"> **{raid.Key}:** {raid.Value} - {raidValue}");
            }
            
            embed.WithDescription(description.ToString());
            
            await data.RespondAsync(embed: embed.Build());
        }
        catch (RateLimitException ex)
        {
            await data.RespondAsync(ex.Message, ephemeral: true);
        }
    }
}