using Discord.WebSocket;

namespace Tavstal.DeltarBot.Models.Commands;

public interface ICommand
{
    string Name { get; }
    
    string Description { get; }
    
    Task RegisterAsync();

    Task HandleAsync(SocketSlashCommand data);
}