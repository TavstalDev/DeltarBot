using Discord;
using Discord.WebSocket;
using Tavstal.DeltarBot.Extensions;
using Tavstal.DeltarBot.Models.Logging;
using Tavstal.WynnNetSDK.Exceptions;
using Tavstal.WynnNetSDK.Http;

namespace Tavstal.DeltarBot.Services;

public class AnnihilationService : IDisposable
{
    private readonly WynnHttpClient _wynnClient;
    private readonly DiscordSocketClient _discordClient;
    private readonly GuildService _guildService;
    private readonly CancellationTokenSource _cts = new();
    private static readonly DeltarLogger _logger = new(nameof(AnnihilationService));

    public AnnihilationService(WynnHttpClient wynnClient, DiscordSocketClient discordClient, GuildService guildService)
    {
        _wynnClient = wynnClient;
        _discordClient = discordClient;
        _guildService = guildService;
        Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    _cts.Token.ThrowIfCancellationRequested();
                    await UpdateAsync();
                    await Task.Delay(TimeSpan.FromMinutes(5), _cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                /* ignored */
            }
        });
    }

    public void Dispose()
    {
        _cts.Cancel();
    }

    private async Task UpdateAsync()
    {
        // TODO: Add check for was the message sent for the next event or not to prevent spamming
        try
        {
            var result = await _wynnClient.Map.ListEventsAsync();
            if (result.IsError)
            {
                _logger.ERROR($"Failed to check annihilation world event: {result.Error?.Message}");
                return;
            }

            var wes = result.Value;
            if (wes == null || wes.Count == 0)
            {
                _logger.ERROR($"No world events were found.");
                return;
            }
            
            var annihilationEvent = wes.FirstOrDefault(we => we.Name == "Prelude to Annihilation");
            if (annihilationEvent == null)
            {
                _logger.ERROR($"Failed to find Prelude to Annihilation world event.");
                return;
            }

            if (annihilationEvent.Schedule == null)
            {
                _logger.DEBUG("Annihilation event was not scheduled yet.");
                return;
            }

            EmbedBuilder  builder = new EmbedBuilder();
            builder.WithTitle(annihilationEvent.Name);
            builder.WithDescription(annihilationEvent.Lore + "\n");
            builder.WithColor(Color.Red);
            EmbedFieldBuilder embedFieldBuilder = new EmbedFieldBuilder();
            embedFieldBuilder.WithName("Starting");
            embedFieldBuilder.WithValue(TimestampTag.FormatFromDateTime(annihilationEvent.Schedule.Value, TimestampTagStyles.Relative));
            builder.WithFields(embedFieldBuilder);
            builder.WithCurrentTimestamp();
            builder.WithFooter("This was automated with DeltarBot.");
            builder.WithThumbnailUrl("https://raw.githubusercontent.com/TavstalDev/DeltarBot/refs/heads/master/assets/images/icon_cb_world_event.png");
            builder.WithImageUrl("https://wynncraft.wiki.gg/images/Annihilation.png");
            var embed = builder.Build();

            foreach (var guild in _discordClient.Guilds)
            {
                var config = _guildService.Get(guild.Id);
                if (config.AnnihilationChannelId == null)
                {
                    _logger.DEBUG($"Guild {guild.Name} ({guild.Id}) has no annihilation channel set.");
                    continue;
                }
                
                var anniChannel = await _discordClient.GetChannelAsync(config.AnnihilationChannelId.Value);
                if (anniChannel is not ITextChannel textChannel)
                {
                    _logger.DEBUG($"Guild {guild.Name} ({guild.Id}) has an invalid annihilation channel set.");
                    continue;
                }

                await textChannel.SendMessageAsync(embed: embed);
            }
            _logger.INFO($"Successfully sent annihilation world event alert to {_discordClient.Guilds.Count} guilds.");
        }
        catch (RateLimitException)
        {
            _logger.ERROR("Failed to check annihilation world event due to rate limiting.");
        }
    }
}