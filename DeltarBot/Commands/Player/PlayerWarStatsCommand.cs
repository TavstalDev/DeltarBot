using Discord;
using Discord.WebSocket;
using Tavstal.DeltarBot.Models.Commands;
using Tavstal.DeltarBot.Services;
using Tavstal.WynnNetSDK.Http;

namespace Tavstal.DeltarBot.Commands.Player;

public class PlayerWarStatsCommand : SimpleCommand
{
    public PlayerWarStatsCommand(DiscordSocketClient client, WynnHttpClient wynnClient, GuildService guildService) : base(
        "pwstats",
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