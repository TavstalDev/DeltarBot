using System.Text;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Tavstal.DeltarBot.Models.Commands;
using Tavstal.DeltarBot.Services;
using Tavstal.WynnNetSDK.Exceptions;
using Tavstal.WynnNetSDK.Http;
using Tavstal.WynnNetSDK.Http.Requests.Items.Bodies;
using Tavstal.WynnNetSDK.Models.Leaderboard;

namespace Tavstal.DeltarBot.Commands.Item;

public class ItemFindCommand : SimpleCommand
{
    public ItemFindCommand(DiscordSocketClient client, WynnHttpClient wynnClient, GuildService guildService) : base(
        "ifind",
        "Searches for items with matching name",
        client, wynnClient, guildService) { }

    public override SlashCommandBuilder HandleBuild(SlashCommandBuilder cmd)
    {
        SlashCommandOptionBuilder opt = new SlashCommandOptionBuilder()
            .WithName("name")
            .WithDescription("Name of the item to find.")
            .WithRequired(true)
            .WithType(ApplicationCommandOptionType.String);
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
        
        var itemOption = data.Data.Options.ElementAt(0);
        var itemName = (string)itemOption.Value;

        try
        {
            var result = await _wynnClient.Items.SearchAsync(new ItemSearchRequestBody
            {
                Query = itemName
            });

            if (result.IsError)
            {
                await data.RespondAsync(result.Error?.Message ?? "Failed to get item data.", ephemeral: true);
                return;
            }
            
            var itemResult = result.Value;
            var embed = new EmbedBuilder()
                .WithTitle($"Found {itemResult.Results.Count} items.")
                .WithUrl("https://github.com/TavstalDev/DeltarBot")
                .WithColor(Color.Blue)
                .WithCurrentTimestamp()
                .WithFooter("DeltarBot");

            var description = new StringBuilder();
            foreach (var item in itemResult.Results)
            {
                description.AppendLine($"**{item.DisplayName}**");
                description.AppendLine($"> **Type:** {item.Type}");
                description.AppendLine($"> **SubType:** {item.SubType}");
                description.AppendLine($"> **Tier:** {item.Tier}");
            }
            embed.WithDescription(description.ToString());
            
            await data.RespondAsync(embed: embed.Build());
        }
        catch (RateLimitException ex)
        {
            await data.RespondAsync(ex.Message, ephemeral: true);
        }
    }
}