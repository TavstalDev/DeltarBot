using Discord;
using Discord.WebSocket;
using Tavstal.DeltarBot.Models.Commands;
using Tavstal.DeltarBot.Services;
using Tavstal.WynnNetSDK.Http;

namespace Tavstal.DeltarBot.Commands.Guild;

public class GuildMembersCommand : SimpleCommand
{
    public GuildMembersCommand(DiscordSocketClient client, WynnHttpClient wynnClient, GuildService guildService) : base(
        "gmembers",
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