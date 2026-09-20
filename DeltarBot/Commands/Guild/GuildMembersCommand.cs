using Discord;
using Discord.WebSocket;
using Tavstal.DeltarBot.Models.Commands;
using Tavstal.DeltarBot.Services;
using Tavstal.WynnNetSDK.Exceptions;
using Tavstal.WynnNetSDK.Http;

namespace Tavstal.DeltarBot.Commands.Guild;

public class GuildMembersCommand : SimpleCommand
{
    public GuildMembersCommand(DiscordSocketClient client, WynnHttpClient wynnClient, GuildService guildService) : base(
        "gmembers",
        "Shows all members of the provided guild.",
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
                .WithTitle($"[{guild.Prefix}] {guild.Name} Members")
                .WithUrl("https://github.com/TavstalDev/DeltarBot")
                .WithThumbnailUrl("https://raw.githubusercontent.com/TavstalDev/DeltarBot/refs/heads/master/assets/images/icon_guild.png")
                .WithColor(Color.Blue)
                .WithCurrentTimestamp()
                .WithFooter($"The guild has {guild.Members.Total} members.");

            if (guild.Members.Owner.Count > 0)
            {
                string members = string.Join(", ", guild.Members.Owner.Select(x => x.Key)
                    .OrderBy(x => x));
                embed.AddField("👑 Owners", members);
            }

            if (guild.Members.Chief.Count > 0)
            {
                string members = string.Join(", ", guild.Members.Chief.Select(x => x.Key)
                    .OrderBy(x => x));
                embed.AddField("⚔️ Chiefs", members);
            }
            
            if (guild.Members.Strategist.Count > 0)
            {
                string members = string.Join(", ", guild.Members.Strategist.Select(x => x.Key)
                    .OrderBy(x => x));
                embed.AddField("🧭 Strategists", members);
            }
            
            if (guild.Members.Recruiter.Count > 0)
            {
                string members = string.Join(", ", guild.Members.Recruiter.Select(x => x.Key)
                    .OrderBy(x => x));
                embed.AddField("🤝 Recruiters", members);
            }
            
            if (guild.Members.Recruit.Count > 0)
            {
                string members = string.Join(", ", guild.Members.Recruit.Select(x => x.Key)
                    .OrderBy(x => x));
                embed.AddField("🌱 Recruits", members);
            }
            
            await data.RespondAsync(embed: embed.Build());
        }
        catch (RateLimitException ex)
        {
            await data.RespondAsync(ex.Message, ephemeral: true);
        }
    }
}