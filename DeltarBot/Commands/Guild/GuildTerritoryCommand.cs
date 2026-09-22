using System.Text;
using Discord;
using Discord.WebSocket;
using Tavstal.DeltarBot.Models.Commands;
using Tavstal.DeltarBot.Services;
using Tavstal.WynnNetSDK.Exceptions;
using Tavstal.WynnNetSDK.Http;

namespace Tavstal.DeltarBot.Commands.Guild;

public class GuildTerritoryCommand : SimpleCommand
{
    public GuildTerritoryCommand(DiscordSocketClient client, WynnHttpClient wynnClient, GuildService guildService) : base(
        "gterritory",
        "Lists guild territories.",
        client, wynnClient, guildService) { }

    public override SlashCommandBuilder HandleBuild(SlashCommandBuilder cmd)
    {
        SlashCommandOptionBuilder opt = new SlashCommandOptionBuilder()
            .WithName("page")
            .WithDescription("The number of the page.")
            .WithRequired(true)
            .WithType(ApplicationCommandOptionType.Integer);
        cmd.AddOption(opt);
        return cmd;
    }
    
    public override async Task HandleAsync(SocketSlashCommand data)
    {
        if (data.Data.Options.Count == 0)
        {
            await data.RespondAsync("You must provide arguments.", ephemeral: true);
            return;
        }
        
        var pageOption = data.Data.Options.ElementAt(0);
        var page = Convert.ToInt32(pageOption.Value);
        
        if (page < 1)
            page = 1;
        
        try
        {
            await data.DeferAsync(true);
            var result = await _wynnClient.Guild.ListTerritoriesAsync();

            if (result.IsError)
            {
                await data.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = new Optional<string>(result.Error?.Message ?? "Failed to get item data.");
                });
                return;
            }
            
            var territories = result.Value;
            int maxPage = territories.Count / 5 + 1;
            if (page > maxPage)
                page = maxPage;
            
            var embed = new EmbedBuilder()
                .WithTitle($"Territories.")
                .WithUrl("https://github.com/TavstalDev/DeltarBot")
                .WithColor(Color.Blue)
                .WithCurrentTimestamp()
                .WithFooter("DeltarBot");

            var description = new StringBuilder();
            for (int i = 0; i < 5; i++)
            {
                int index = i + (page - 1) * 5;
                if (territories.Count - 1 < index)
                    break;

                var item = territories.ElementAt(index);
                description.AppendLine($"**{item.Key}**");
                description.AppendLine($"> **Guild:** [{item.Value.Guild.Prefix ?? "N/A"}] {item.Value.Guild.Name ?? "N/A"}");
                description.AppendLine($"> **Treasury:** {item.Value.Treasury}");
                description.AppendLine($"> **Defences:** {item.Value.Defences}");
                string resourcrs = string.Join(", ", item.Value.Resources.Select(x => x.Type));
                description.AppendLine($"> **Resources:** {resourcrs}");

                description.AppendLine();
            }
            
            embed.WithDescription(description.ToString());
            
            await data.ModifyOriginalResponseAsync(msg =>
            {
                msg.Embed = new Optional<Embed>(embed.Build());
            });
        }
        catch (RateLimitException ex)
        {
            await data.ModifyOriginalResponseAsync(msg =>
            {
                msg.Content = new Optional<string>(ex.Message);
            });
        }
    }
}