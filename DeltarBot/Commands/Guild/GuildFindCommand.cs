using Discord;
using Discord.WebSocket;
using Tavstal.DeltarBot.Models.Commands;
using Tavstal.DeltarBot.Services;
using Tavstal.WynnNetSDK.Http;

namespace Tavstal.DeltarBot.Commands.Guild;

public class GuildFindCommand : SimpleCommand
{
    public GuildFindCommand(DiscordSocketClient client, WynnHttpClient wynnClient, GuildService guildService) : base(
        "gfind",
        "TODO",
        client, wynnClient, guildService) { }

    public override SlashCommandBuilder HandleBuild(SlashCommandBuilder cmd)
    {
        return cmd;
    }

    public override async Task HandleAsync(SocketSlashCommand data)
    {
        
    }
}