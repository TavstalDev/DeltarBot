using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Tavstal.DeltarBot.Extensions;
using Tavstal.DeltarBot.Models.Config;
using Tavstal.DeltarBot.Models.Logging;
using Tavstal.DeltarBot.Utils;
using Tavstal.WynnNetSDK.Http;

namespace Tavstal.DeltarBot;

public class Program
{
    private static DiscordSocketClient _client = null!;
    private static WynnHttpClient _wynnClient = null!;
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

        string envFile = Path.Combine(currentWorkingDirectory, ".env");
        if (!File.Exists(envFile))
        {
            _logger.ERROR("Please create a .env file in the root directory of the project and set the required environment variables.");
            PressAnyKey();
            return;
        }
        
        string[] lines = await File.ReadAllLinesAsync(envFile);
        foreach (string line in lines)
        {
            string[] parts = line.Split('=');
            if (parts.Length != 2)
                continue;
            Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }
        
        var _config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.None
        };
        _client = new DiscordSocketClient(_config);
        _client.Log += Log;
        _client.Ready += Ready;

        // Run the log queue on a background task
        _ = Task.Run(() => LoggerHelper.ProcessLogQueueAsync(_logCts.Token));

        string? token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        if (string.IsNullOrEmpty(token))
        {
            _logger.WARN("Please set DISCORD_TOKEN environment variable in the .env file.");
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
        _logger.INFO($"Logged in as {_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator} ({_client.CurrentUser.Id})");
        await _client.SetStatusAsync(UserStatus.Online);
        await _client.SetCustomStatusAsync("Playing with Wynncraft.");
        await _client.SetActivityAsync(new Game())
        
        _logger.INFO("Initializing Wynncraft API client...");
        string? token = Environment.GetEnvironmentVariable("WYNN_TOKEN");
        if (string.IsNullOrEmpty(token))
        {
            _logger.ERROR("Please set WYNN_TOKEN environment variable in the .env file.");
            return;
        }
        
        _wynnClient = new WynnHttpClient(new WynnEnvironment(token));
        
        // TEST REQUEST
        var result = await _wynnClient.Map.ListCampsAsync();
        if (result.IsError)
        {
            _logger.ERROR($"Wynn API returned an error: \nName: {result.Error.Name} - {result.Error.Code} \nMessage: {result.Error.Message}");
            return;
        }
        _logger.DEBUG("Listing Camps...");
        foreach (var camp in result.Value)
        {
            _logger.DEBUG($"{camp.Name} -> {camp.Lore}");
        }
        _logger.INFO("Wynn API client initialized successfully.");
    }

    private static Task Log(LogMessage msg)
    {
        switch (msg.Severity)
        {
            case LogSeverity.Debug:
            {
                _logger.DEBUG(msg.Message, msg.Exception);
                break;
            }
            case LogSeverity.Info:
            {
                _logger.INFO(msg.Message, msg.Exception);
                break;
            }
            case LogSeverity.Warning:
            {
                _logger.WARN(msg.Message, msg.Exception);
                break;
            }
            case LogSeverity.Error:
            {
                _logger.ERROR(msg.Message, msg.Exception);
                break;
            }
            case LogSeverity.Critical:
            {
                _logger.FATAL(msg.Message, msg.Exception);   
                break;
            }
        }
        return Task.CompletedTask;
    }
}