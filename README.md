# DeltarBot

![Release (latest by date)](https://img.shields.io/github/v/release/TavstalDev/DeltarBot?style=plastic-square)
![Workflow Status](https://img.shields.io/github/actions/workflow/status/TavstalDev/DeltarBot/release.yml?branch=stable&label=build&style=plastic-square)
![License](https://img.shields.io/github/license/TavstalDev/DeltarBot?style=plastic-square)
![Downloads](https://img.shields.io/github/downloads/TavstalDev/DeltarBot/total?style=plastic-square)
![Issues](https://img.shields.io/github/issues/TavstalDev/DeltarBot?style=plastic-square)

A Discord bot that provides Wynncraft player, guild, item, and leaderboard data as slash commands, along with configurable world-event notifications for your server.

Built on [Discord.Net](https://github.com/discord-net/Discord.Net) and [WynnNetSDK](assets/WynnNetSDK.dll), a client SDK for the [Wynncraft API](https://api.wynncraft.com).

## Features

- **Player lookup** — global stats, progress, PvP and raid data.
- **Guild lookup** — general info, member list, season ranks, and territory listings.
- **Item search** — quick-search items by display name.
- **Leaderboards** — top 10 for any leaderboard type (with autocomplete).
- **Feed system** — per-guild channel tracking for Wynncraft world events (currently `Prelude to Annihilation`), checked and announced automatically every 5 minutes.
- **Persistent config** — per-guild command channel and feed channels stored in `data.json`.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [WynnNetSDK](https://github.com/TavstalDev/WynnNetSDK)
- Discord bot token and Wynncraft API token

## Setup

```bash
# 1. Create your .env from the example
cp .env_example .env
#    Fill in DISCORD_TOKEN and WYNN_TOKEN

# 2. Run the bot
dotnet run --project DeltarBot
```

On first run the bot generates a `config.json` (log level, development guild ID) and a `data.json` (annihilation feed state) in the working directory. Command registration happens automatically on startup via `BulkOverwriteGlobalApplicationCommandsAsync`.

## Commands

| Command                           | Description                                                     |
|-----------------------------------|-----------------------------------------------------------------|
| `/pfind <player>`                 | Finds a player and their global stats.                          |
| `/gfind <guild> [isprefix]`       | Shows basic information about a guild.                          |
| `/gmembers <guild> [isprefix]`    | Lists all members of a guild.                                   |
| `/gseason <guild> [isprefix]`     | Shows a guild's seasonal (ranking) information.                 |
| `/gterritory <page>`              | Lists guild territories, 5 per page.                            |
| `/ifind <name>`                   | Searches for items with a matching name.                        |
| `/leaderboard <leaderboard_type>` | Shows the top 10 for a leaderboard type (autocomplete).         |
| `/ftrack <channel> <type>`        | Sets a channel to track a feed type (administrator).            |
| `/fremove <type>`                 | Removes a channel from the guild's feed config (administrator). |
| `/ftoggle`                        | Toggles the bot command channel restriction (administrator).    |

## Configuration Files

| File          | Purpose                                                                 |
|---------------|-------------------------------------------------------------------------|
| `.env`        | `DISCORD_TOKEN` and `WYNN_TOKEN` secrets.                               |
| `config.json` | Runtime configuration (`LogLevel`, `DevelopmentGuildId`). Auto-created. |
| `data.json`   | Persistent bot state, e.g. last annihilation announcement.              |

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.

## Contact

For issues or feature requests, please use the [GitHub issue tracker](https://github.com/TavstalDev/DeltarBot/issues).