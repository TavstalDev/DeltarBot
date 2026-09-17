using Discord;
using Discord.WebSocket;
using Tavstal.DeltarBot.Models.Commands;
using Tavstal.DeltarBot.Services;
using Tavstal.WynnNetSDK.Http;

namespace Tavstal.DeltarBot.Commands.Feed;

public class FeedToggleCommand : SimpleCommand
{
    public FeedToggleCommand(DiscordSocketClient client, WynnHttpClient wynnClient, GuildService guildService) : base(
        "ftoggle",
        "Toggles the bot commands channel. (It will use the channel where the command was sent.)",
        client, wynnClient, guildService) { }

    public override SlashCommandBuilder HandleBuild(SlashCommandBuilder cmd)
    {
        cmd.WithDefaultMemberPermissions(GuildPermission.Administrator);
        return cmd;
    }
    
    public override async Task HandleAsync(SocketSlashCommand data)
    {
        if (data.GuildId == null || data.ChannelId == null)
        {
            await data.RespondAsync("This command can only be used in a server's text channel.", ephemeral: true);
            return;
        }

        var guildConfig = _guildService.Get(data.GuildId.Value);
        string message;
        if (guildConfig.BotChannelId != null)
        {
            guildConfig.BotChannelId = null;
            message = "The bot channel has been removed.";
        }
        else
        {
            guildConfig.BotChannelId = data.ChannelId.Value;
            message = $"The bot channel has been set to <#{data.ChannelId}>.";
        }
        _guildService.Update(data.GuildId.Value, guildConfig);
        await data.RespondAsync(message, ephemeral: true);
    }
}