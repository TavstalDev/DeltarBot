using System.Text;
using Discord;
using Discord.WebSocket;
using Tavstal.DeltarBot.Models.Commands;
using Tavstal.DeltarBot.Services;
using Tavstal.WynnNetSDK.Exceptions;
using Tavstal.WynnNetSDK.Http;

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
            await data.DeferAsync(true);
            var result = await _wynnClient.Items.QuickSearchAsync(itemName);

            if (result.IsError)
            {
                await data.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = new Optional<string>(result.Error?.Message ?? "Failed to get item data.");
                });
                return;
            }
            
            var itemResult = result.Value;
            var embed = new EmbedBuilder()
                .WithTitle($"Found {itemResult.Count} items.")
                .WithUrl("https://github.com/TavstalDev/DeltarBot")
                .WithColor(Color.Blue)
                .WithCurrentTimestamp()
                .WithFooter("DeltarBot");

            var description = new StringBuilder();
            foreach (var item in itemResult)
            {
                description.AppendLine($"**{item.DisplayName}**");
                description.AppendLine($"> **Type:** {item.Type}");
                description.AppendLine($"> **SubType:** {item.SubType}");
                description.AppendLine($"> **Tier:** {item.Tier}");
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