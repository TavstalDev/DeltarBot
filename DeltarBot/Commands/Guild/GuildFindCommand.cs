using Discord;
using Discord.WebSocket;
using Tavstal.DeltarBot.Models.Commands;
using Tavstal.DeltarBot.Services;
using Tavstal.WynnNetSDK.Exceptions;
using Tavstal.WynnNetSDK.Http;

namespace Tavstal.DeltarBot.Commands.Guild;

public class GuildFindCommand : SimpleCommand
{
    public GuildFindCommand(DiscordSocketClient client, WynnHttpClient wynnClient, GuildService guildService) : base(
        "gfind",
        "Shows basic information about the provided guild.",
        client, wynnClient, guildService) { }

    public override SlashCommandBuilder HandleBuild(SlashCommandBuilder cmd)
    {
        SlashCommandOptionBuilder opt = new SlashCommandOptionBuilder()
            .WithName("guild")
            .WithDescription("The name of the guild you want to search for.")
            .WithRequired(true)
            .WithType(ApplicationCommandOptionType.String);
        cmd.AddOption(opt);
        opt = new SlashCommandOptionBuilder()
            .WithName("isprefix")
            .WithDescription("Whether it should be the prefix of the guild you want to search for.")
            .WithRequired(false)
            .WithType(ApplicationCommandOptionType.Boolean);
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
        
        var guildOption = data.Data.Options.ElementAt(0);
        var guildName = (string)guildOption.Value;
        
        bool isPrefix = false;
        if (data.Data.Options.Count == 2)
        {
            var prefixOption = data.Data.Options.ElementAt(1);
            isPrefix = (bool)prefixOption.Value;
        }

        if (isPrefix && guildName.Length > 4)
        {
            await data.RespondAsync("The prefix name cannot be longer than 4 characters.", ephemeral: true);
            return;
        }
        
        try
        {
            var result = isPrefix
                ? await _wynnClient.Guild.GetByPrefixAsync(guildName)
                : await _wynnClient.Guild.GetByNameAsync(guildName);

            if (result.IsError)
            {
                await data.RespondAsync(result.Error?.Message ?? "Failed to get guilds.", ephemeral: true);
                return;
            }
            
            var guild = result.Value;
            var embed = new EmbedBuilder()
                .WithTitle($"[{guild.Prefix}] {guild.Name}")
                .WithThumbnailUrl("https://raw.githubusercontent.com/TavstalDev/DeltarBot/refs/heads/master/assets/images/icon_guild.png")
                .WithColor(Color.Blue)
                .WithTimestamp(guild.Created)
                .WithFooter("Guild Created On");
            
            // General Information
            string owner = "N/A";
            if (guild.Members.Owner.Count > 0)
            {
                var ownedData = guild.Members.Owner.FirstOrDefault();
                if (!string.IsNullOrEmpty(ownedData.Value.Username))
                    owner = ownedData.Value.Username;
                if (!string.IsNullOrEmpty(ownedData.Value.Uuid))
                    owner = ownedData.Value.Uuid;
                if (ownedData.Key != ownedData.Value.Uuid)
                    owner = ownedData.Key;
            }

            // Group 1: General Info 
            embed.AddField("👑 Owner", owner, true);
            embed.AddField("📈 Level", $"{guild.Level} (`{guild.XpPercent}%`)", true);
            embed.AddField("🟢 Online", $"{guild.Online}/{guild.Members.Total}", true);

            // Group 2: Progression & Combat
            embed.AddField("🗺️ Territories", guild.Territories.ToString(), true);
            embed.AddField("⚔️ Wars Won", guild.Wars.ToString(), true);
            embed.AddField("🛡️ Raids Completed", guild.Raids.ToString(), true);

            // Group 3: Rankings
            if (guild.Ranking.Count > 0)
            {
                var rankingsText = string.Join("\n", guild.Ranking.OrderBy(x => x.Key).Select(r => $"{r.Key} • #{r.Value}"));
                embed.AddField("🏆 Global Rankings", rankingsText);
            }
            
            await data.RespondAsync(embed: embed.Build());
        }
        catch (RateLimitException ex)
        {
            await data.RespondAsync(ex.Message, ephemeral: true);
        }
    }
}