using Tavstal.WynnNetSDK.Http;

namespace Tavstal.DeltarBot.Services;

public class WynnApiService : IDisposable
{
    private readonly WynnHttpClient _client;
    private readonly CancellationTokenSource _cts;

    public WynnApiService(WynnHttpClient client)
    {
        _client = client;
        _cts = new CancellationTokenSource();
        
        Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    _cts.Token.ThrowIfCancellationRequested();
                    Upadte();
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            }
            catch (TaskCanceledException) { /* ignored */ }
        });
    }
    
    public void Dispose()
    {
        if (_cts.IsCancellationRequested)
            return;
        _cts.Cancel();
        _client.Dispose();
    }
     
    private void Upadte()
    {
        _client.Ability.ResetRpm();
        _client.Classes.ResetRpm();
        _client.Guild.ResetRpm();
        _client.Items.ResetRpm();
        _client.Leaderboard.ResetRpm();
        _client.Map.ResetRpm();
        _client.Player.ResetRpm();
        _client.News.ResetRpm();
        _client.Recipes.ResetRpm();
        _client.Search.ResetRpm();
    }
}