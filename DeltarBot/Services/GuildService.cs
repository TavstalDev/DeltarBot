using System.Collections.Concurrent;
using Discord.WebSocket;
using Newtonsoft.Json;
using Tavstal.DeltarBot.Models.Guilds;

namespace Tavstal.DeltarBot.Services;

public class GuildService : IDisposable
{
    private readonly DiscordSocketClient _client;
    private readonly CancellationTokenSource _cts = new();
    private readonly ConcurrentDictionary<ulong, GuildConfig> _guilds = [];
    private bool _isDirty;
    
    public GuildService(DiscordSocketClient client)
    {
        _client = client;
        if (File.Exists("guilds.json"))
        {
            string config = File.ReadAllText("guilds.json");
            if (!string.IsNullOrWhiteSpace(config))
            {
                var guilds = JsonConvert.DeserializeObject<Dictionary<ulong, GuildConfig>>(config, Program.JsonSerializerSettings);
                if (guilds != null)
                {
                    foreach (var kvp in guilds)
                        _guilds[kvp.Key] = kvp.Value;
                }
            }
        }

        Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    _cts.Token.ThrowIfCancellationRequested();
                    await UpdateAsync();
                    await Task.Delay(TimeSpan.FromMinutes(1), _cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                /*  ignored*/
            }
        });
    }

    public void Dispose()
    {
        _cts.Cancel();
        _guilds.Clear();
    }

    public GuildConfig Get(ulong guildId)
    {
        if (!_guilds.TryGetValue(guildId, out var guildConfig))
        {
            guildConfig = new GuildConfig();
            _guilds[guildId] = guildConfig;
            _isDirty = true;
        }
        return guildConfig;
    }

    public void Update(ulong guildId, GuildConfig guildConfig)
    {
        _guilds[guildId] = guildConfig;
        _isDirty = true;
    }

    private async Task UpdateAsync()
    {
        if (!_isDirty)
            return;
        await File.WriteAllTextAsync("guilds.json", JsonConvert.SerializeObject(_guilds.ToDictionary(), Program.JsonSerializerSettings));
        _isDirty = false;
    }
}