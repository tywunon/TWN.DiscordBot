namespace TWN.DiscordBot.Settings;
public sealed record Settings(WatcherSettings Watcher,
                              DiscordSettings Discord,
                              TwitchSettings Twitch,
                              IEnumerable<GuildConfig> GuildConfig,
                              DataStoreSettings DataStore,
                              TCPProbeSettings TCPProbe);

public sealed record WatcherSettings(int Delay,
                                     double Horizon);

public sealed record DiscordSettings(string Status,
                                     string AppToken);

public sealed record TwitchSettings(string OAuthURL,
                                    string BaseURL,
                                    string ClientID,
                                    string ClientSecret);

public sealed record GuildConfig(ulong GuildID,
                                 string Color,
                                 string Text,
                                 string FooterText,
                                 int ThumbnailWidth,
                                 int ThumbnailHeight);

public sealed record DataStoreSettings(string FilePath);

public sealed record TCPProbeSettings(short Port);

