using Discord;
using Discord.WebSocket;

namespace Tavstal.DeltarBot.Models.Commands;

public interface ICommand
{
    string Name { get; }
    
    string Description { get; }
    
    SlashCommandProperties Build();

    Task HandleAsync(SocketSlashCommand data);
}