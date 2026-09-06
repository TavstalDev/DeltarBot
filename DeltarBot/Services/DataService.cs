using Newtonsoft.Json;
using Tavstal.DeltarBot.Extensions;
using Tavstal.DeltarBot.Models.Data;
using Tavstal.DeltarBot.Models.Logging;

namespace Tavstal.DeltarBot.Services;

public class DataService
{
    private readonly DeltarLogger _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly string _filePath;
    public DeltarData Data { get; private set; } = new();

    public DataService(string filePath)
    {
        _filePath = filePath;
        _logger = new DeltarLogger(nameof(DataService));
    }
    
    public async Task LoadAsync()
    {
        try
        {
            await _semaphore.WaitAsync();
            if (!File.Exists(_filePath))
            {
                await File.WriteAllTextAsync(_filePath, JsonConvert.SerializeObject(Data, Formatting.Indented));
                return;
            }

            var json = await File.ReadAllTextAsync(_filePath);
            var data = JsonConvert.DeserializeObject<DeltarData>(json);
            if (data == null)
            {
                _logger.WARN("Failed to load data from file, using default data.");
                return;
            }

            Data = data;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SaveAsync()
    {
        try
        {
            await _semaphore.WaitAsync();
            var json = JsonConvert.SerializeObject(Data, Formatting.Indented);
            await File.WriteAllTextAsync(_filePath, json);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}