namespace Tavstal.DeltarBot.Models.Guilds;

public class GuildConfig
{
    public ulong? BotChannelId { get; set; }
    public Dictionary<EGuildChannel, ulong> FeedChannels { get; set; } = [];
}