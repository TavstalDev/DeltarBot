using Discord;
using Discord.WebSocket;
using Tavstal.DeltarBot.Services;
using Tavstal.WynnNetSDK.Http;

namespace Tavstal.DeltarBot.Models.Commands;

public abstract class AutoCompleteCommand: ICommand
{
    protected readonly DiscordSocketClient _client;
    protected readonly WynnHttpClient _wynnClient;
    protected readonly GuildService _guildService;
    
    public string Name { get; }
    public string Description { get; }

    protected AutoCompleteCommand(string name, string description, DiscordSocketClient client, WynnHttpClient wynnClient, GuildService guildService)
    {
        Name = name;
        Description = description;
        _client = client;
        _wynnClient = wynnClient;
        _guildService = guildService;
    }
    
    public SlashCommandProperties Build()
    {
        var cmd = new SlashCommandBuilder();
        cmd.WithName(Name);
        cmd.WithDescription(Description);
        cmd.WithDefaultMemberPermissions(GuildPermission.UseApplicationCommands);
        cmd = HandleBuild(cmd);
        return cmd.Build();
    }

    public virtual SlashCommandBuilder HandleBuild(SlashCommandBuilder cmd) => cmd;

    public abstract Task HandleAsync(SocketSlashCommand data);

    public abstract Task AutocompleteHandler(SocketAutocompleteInteraction autocomplete);
}