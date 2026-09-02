using System.Reflection;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Tavstal.DeltarBot.Extensions;
using Tavstal.DeltarBot.Models.Config;
using Tavstal.DeltarBot.Models.Logging;
using Tavstal.DeltarBot.Utils;

namespace Tavstal.DeltarBot;

public class Program
{
    private static DiscordSocketClient _client = null!;
    private static readonly CancellationTokenSource _logCts = new();
    private static readonly DeltarLogger _logger = new();
    public static DeltarConfiguration Config { get; private set; } = null!;

    public static JsonSerializerSettings JsonSerializerSettings { get; } = new()
    {
        Formatting = Formatting.Indented,
        Converters = { new StringEnumConverter() }
    };
    
    public static async Task Main()
    {
        string currentWorkingDirectory = Directory.GetCurrentDirectory();
        string configPath = Path.Combine(currentWorkingDirectory, "config.json");
        if (!File.Exists(configPath))
        {
            try
            {
                await File.WriteAllTextAsync(configPath,
                    JsonConvert.SerializeObject(new DeltarConfiguration(), JsonSerializerSettings));
            }
            catch (Exception ex)
            {
                _logger.FATAL("Failed to create config.json file.", ex);
                PressAnyKey();
                return;
            }
        }

        string json = await File.ReadAllTextAsync(configPath);
        if (string.IsNullOrEmpty(json))
        {
            _logger.ERROR("Failed to read config.json file or the file is empty.");
            PressAnyKey();
            return;
        }

        try
        {
            var config = JsonConvert.DeserializeObject<DeltarConfiguration>(json, JsonSerializerSettings);
            if (config == null)
            {
                _logger.ERROR("Couldn't parse config.json file.");
                PressAnyKey();
                return;
            }
            Config = config;
        }
        catch (Exception ex)
        {
            _logger.FATAL("Failed to parse config.json file and an unexpected error occurred.", ex);
            PressAnyKey();
            return;
        }

        _client = new DiscordSocketClient();
        _client.Log += Log;
        _client.Ready += Ready;
        _client.Connected += OnConnected;
        _client.Disconnected += OnDisconnected;
        
        // Run the log queue on a background task
        _ = Task.Run(() => LoggerHelper.ProcessLogQueueAsync(_logCts.Token));

        string? token = Environment.GetEnvironmentVariable("TOKEN");
        if (string.IsNullOrEmpty(token))
        {
            _logger.WARN("Please set TOKEN environment variable in the .env file.");
            PressAnyKey();
            return;
        }
        
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        
        await Task.Delay(-1);
    }

    private static void PressAnyKey()
    {
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
    
    private static async Task Ready()
    {
        
    }
    
    private static async Task OnConnected()
    {
        
    }
    
    private static async Task OnDisconnected(Exception arg)
    {
        
    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}