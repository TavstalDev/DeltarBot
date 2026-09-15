using Discord;
using Discord.WebSocket;
using Tavstal.DeltarBot.Models.Commands;
using Tavstal.DeltarBot.Services;
using Tavstal.WynnNetSDK.Http;

namespace Tavstal.DeltarBot.Commands.Feed;

public class FeedTrackCommand : ICommand
{
    private readonly DiscordSocketClient _client;
    private readonly WynnHttpClient _wynnClient;
    private readonly GuildService _guildService;
    
    public string Name => "ftrack";
    public string Description => "";

    public FeedTrackCommand(DiscordSocketClient client, WynnHttpClient wynnClient, GuildService guildService)
    {
        _client = client;
        _wynnClient = wynnClient;
        _guildService = guildService;
    }
    
    public async Task RegisterAsync()
    {
        var cmd = new SlashCommandBuilder();
        cmd.WithName(Name);
        cmd.WithDescription(Description);
        await _client.CreateGlobalApplicationCommandAsync(cmd.Build());
    }

    public async Task HandleAsync(SocketSlashCommand data)
    {
        
    }
}