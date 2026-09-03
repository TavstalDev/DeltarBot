using Discord;
using Discord.WebSocket;
using Tavstal.DeltarBot.Models.Commands;
using Tavstal.DeltarBot.Services;
using Tavstal.WynnNetSDK.Http;

namespace Tavstal.DeltarBot.Commands;

public class AnnihilationCommand : ICommand
{
    private readonly DiscordSocketClient _client;
    private readonly WynnHttpClient _wynnClient;
    private readonly GuildService _guildService;
    
    public string Name => "annihilation";
    public string Description => "Sets the annihilation alerts channel.";

    public AnnihilationCommand(DiscordSocketClient client, WynnHttpClient wynnClient, GuildService guildService)
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

   public async Task HandleAsync( SocketSlashCommand data)
   {
       if (data.GuildId == null || data.ChannelId == null)
       {
           await data.RespondAsync("This command can only be used in a server's text channel.", ephemeral: true);
           return;
       }

       var guildConfig = _guildService.Get(data.GuildId.Value);
       guildConfig.AnnihilationChannelId = data.ChannelId.Value;
       _guildService.Update(data.GuildId.Value, guildConfig);
       await data.RespondAsync("Annihilation alerts channel has been set to this channel.", ephemeral: true);
   }
}