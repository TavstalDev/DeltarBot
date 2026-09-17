using Discord;
using Discord.WebSocket;
using Tavstal.DeltarBot.Models.Commands;
using Tavstal.DeltarBot.Models.Guilds;
using Tavstal.DeltarBot.Services;
using Tavstal.WynnNetSDK.Http;

namespace Tavstal.DeltarBot.Commands.Feed;

public class FeedRemoveCommand : SimpleCommand
{
    public FeedRemoveCommand(DiscordSocketClient client, WynnHttpClient wynnClient, GuildService guildService) : base(
        "fremove",
        "Removes a channel from the guild's feed config.",
        client, wynnClient, guildService) { }
    
    public override SlashCommandBuilder HandleBuild(SlashCommandBuilder cmd)
    {
        cmd.WithDefaultMemberPermissions(GuildPermission.Administrator);
        
        var enumType = typeof(EGuildChannel);
        var enumNames = Enum.GetNames(enumType);

        SlashCommandOptionBuilder typeOption = new SlashCommandOptionBuilder()
            .WithName("type")
            .WithDescription("Type of the feed.")
            .WithRequired(true)
            .WithType(ApplicationCommandOptionType.String);
        
        foreach (var t in enumNames)
            typeOption.AddChoice(t, t);
        
        cmd.AddOption(typeOption);
        
        return cmd;
    }

    public override async Task HandleAsync(SocketSlashCommand data)
    {
        if (data.GuildId == null || data.ChannelId == null)
        {
            await data.RespondAsync("This command can only be used in a server's text channel.", ephemeral: true);
            return;
        }

        if (data.Data.Options.Count == 0)
        {
            await data.RespondAsync("You must provide the type of the feed.", ephemeral: true);
            return;
        }

        var guildConfig = _guildService.Get(data.GuildId.Value);
        var option = data.Data.Options.ElementAt(0);
        var value = Enum.Parse<EGuildChannel>((string)option.Value);
        if (!guildConfig.FeedChannels.Remove(value, out _))
        {
            await data.RespondAsync("The provided feed type has not been set.", ephemeral: true);
            return;
        }
        _guildService.Update(data.GuildId.Value, guildConfig);
        await data.RespondAsync($"The provided feed type has been successfully removed.", ephemeral: true);
    }
}