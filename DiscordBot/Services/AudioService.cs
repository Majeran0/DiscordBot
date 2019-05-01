using Discord;
using Discord.WebSocket;
using Victoria;
using Victoria.Entities;
using Victoria.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using DiscordBot.DataStructs;
using DiscordBot.Handlers;

namespace DiscordBot.Services {
    public class AudioService {
        private Lavalink _lavalink;

        public AudioService(Lavalink lavalink)
            => _lavalink = lavalink;

        private readonly Lazy<ConcurrentDictionary<ulong, AudioOptions>> _lazyOptions
            = new Lazy<ConcurrentDictionary<ulong, AudioOptions>>();

        private ConcurrentDictionary<ulong, AudioOptions> Options
            => _lazyOptions.Value;

        public async Task<Embed> JoinOrPlayAsync(SocketGuildUser user, IMessageChannel textChannel, ulong guildID, string query = null) {
            if (user.VoiceChannel == null)
                return await EmbedHandler.CreateErrorEmbed("Music, Join/Play", "You must first join a voice channel.");

            if (query == null) {
                await _lavalink.DefaultNode.ConnectAsync(user.VoiceChannel, textChannel);
                Options.TryAdd(user.Guild.Id, new AudioOptions {
                    Summoner = user
                });
                await LoggingService.LogInformationAsync("Music", $"Now connected to {user.VoiceChannel.Name} and bound to {textChannel}.");
                return await EmbedHandler.CreateBasicEmbed("Music", $"Now connected to {user.VoiceChannel.Name} and bound to {textChannel.Name}.", Color.Blue);
            } else {
                try {
                    var player = _lavalink.DefaultNode.GetPlayer(guildID);
                    if (player == null) { //when you use play before join
                        await _lavalink.DefaultNode.ConnectAsync(user.VoiceChannel, textChannel);
                        Options.TryAdd(user.Guild.Id, new AudioOptions {
                            Summoner = user
                        });
                        player = _lavalink.DefaultNode.GetPlayer(guildID);
                    }

                    LavaTrack track;
                    var search = await _lavalink.DefaultNode.SearchYouTubeAsync(query);

                    if (search.LoadResultType == LoadResultType.NoMatches)
                        return await EmbedHandler.CreateErrorEmbed("Music", $"No matches for {query}.");

                    track = search.Tracks.FirstOrDefault();

                    if (player.CurrentTrack != null && player.IsPlaying || player.IsPaused) {
                        player.Queue.Enqueue(track);
                        await LoggingService.LogInformationAsync("Music", $"{track.Title} has ben added to music queue");
                        return await EmbedHandler.CreateBasicEmbed("Music", $"{track.Title} has ben added to music queue", Color.Blue);
                    }

                    await player.PlayAsync(track);
                    await LoggingService.LogInformationAsync("Music", $"Bot Now Playing: { track.Title}\nUrl: { track.Uri}");
                    return await EmbedHandler.CreateBasicEmbed("Music", $"Bot Now Playing: {track.Title}\nUrl: {track.Uri}", Color.Blue);
                } catch (Exception ex) {
                    return await EmbedHandler.CreateErrorEmbed("Music, Join/Play", ex.ToString());
                }
            }
        }

        public async Task<Embed> LeaveAsync(ulong guildID) {
            try {
                var player = _lavalink.DefaultNode.GetPlayer(guildID);
                if (player.IsPlaying)
                    await player.StopAsync();

                var channelName = player.VoiceChannel.Name;
                await LoggingService.LogInformationAsync("Music", $"Bot has left {channelName}.");
                return await EmbedHandler.CreateBasicEmbed("Music", $"Bot has left {channelName}.", Color.Blue);
            } catch (InvalidOperationException ex) {
                return await EmbedHandler.CreateErrorEmbed("Music, Leave", ex.ToString());
            }
        }

        public async Task<Embed> ListAsync(ulong guildID) {
            try {
                var descriptionBuilder = new StringBuilder();
                var player = _lavalink.DefaultNode.GetPlayer(guildID);
                if (player == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", "Could not aquire player");

                if (player.IsPlaying) {
                    if (player.Queue.Count < 1 && player.CurrentTrack != null)
                        return await EmbedHandler.CreateBasicEmbed($"Now Platying {player.CurrentTrack.Title}", "Nothing else is queued", Color.Blue);
                    else {
                        var trackNum = 2;
                        foreach (var track in player.Queue.Items) {
                            descriptionBuilder.Append($"{trackNum}: [{track.Title}]({track.Uri}) - {track.Id}\n");
                            trackNum++;
                        }
                        return await EmbedHandler.CreateBasicEmbed("Music Playlist", $"Now Playing: [{player.CurrentTrack.Title}]({player.CurrentTrack.Uri})\n{descriptionBuilder.ToString()}", Color.Blue);
                    }
                } else {
                    return await EmbedHandler.CreateErrorEmbed("Music, List", "Nothing is played right now.");
                }
            } catch (Exception ex) {
                return await EmbedHandler.CreateErrorEmbed("Music, List", ex.Message);
            }
        }

        public async Task<Embed> SkipTrackAsync(ulong guildId) {
            try {
                var player = _lavalink.DefaultNode.GetPlayer(guildId);
                if (player == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.");
                if (player.Queue.Count < 1) {
                    return await EmbedHandler.CreateErrorEmbed("Music, SkipTrack", $"Unable To skip a track as there is only One or No songs currently playing.");
                } else {
                    try {
                        var currentTrack = player.CurrentTrack;
                        await player.SkipAsync();
                        await LoggingService.LogInformationAsync("Music", $"Bot skipped: {currentTrack.Title}");
                        return await EmbedHandler.CreateBasicEmbed("Music Skip", $"Successfully skiped {currentTrack.Title}", Color.Blue);
                    } catch (Exception ex) {
                        return await EmbedHandler.CreateErrorEmbed("Music, Skip", ex.ToString());
                    }

                }
            } catch (Exception ex) {
                return await EmbedHandler.CreateErrorEmbed("Music, Skip", ex.ToString());
            }
        }

        public async Task<Embed> StopAsync(ulong guildID) {
            try {
                var player = _lavalink.DefaultNode.GetPlayer(guildID);
                if (player == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", "Could not aquire player");

                if (player.IsPlaying)
                    await player.StopAsync();
                foreach (var track in player.Queue.Items)
                    player.Queue.Dequeue();
                await LoggingService.LogInformationAsync("Music", "Bot has stopped playback.");
                return await EmbedHandler.CreateBasicEmbed("Music stop", "Bot has stopped playback and cleared playlist", Color.Blue);
            } catch (Exception ex) {
                return await EmbedHandler.CreateErrorEmbed("Music, Stop", ex.ToString());
            }
        }

        public async Task<string> VolumeAsync(ulong guildID, int volume) {
            if (volume >= 150 || volume <= 0)
                return "Volume must be between 0 and 150";
            try {
                var player = _lavalink.DefaultNode.GetPlayer(guildID);
                await player.SetVolumeAsync(volume);
                await LoggingService.LogInformationAsync("Music", $"Bot volume set to {volume}");
                return $"Volume has been set to {volume}.";
            } catch (InvalidOperationException ex) {
                return ex.Message;
            }
        }

        public async Task<string> Pause(ulong guildID) {
            try {
                var player = _lavalink.DefaultNode.GetPlayer(guildID);
                if (player.IsPaused) {
                    await player.PauseAsync();
                    return $"**Resumed:** Now playing {player.CurrentTrack.Title}";
                }

                await player.PauseAsync();
                return "**Paused**";
            } catch (InvalidOperationException ex) {
                return ex.Message;
            }
        }

        public async Task<string> Resume(ulong guildID) {
            try {
                var player = _lavalink.DefaultNode.GetPlayer(guildID);
                if (!player.IsPaused)
                    await player.PauseAsync();
                return $"**Resumed:** {player.CurrentTrack.Title}";
            } catch (InvalidOperationException ex) {
                return ex.Message;
            }
        }

        public async Task OnFinished(LavaPlayer player, LavaTrack track, TrackReason reason) {
            if (reason is TrackReason.LoadFailed || reason is TrackReason.Cleanup)
                return;
            player.Queue.TryDequeue(out LavaTrack nextTrack);

            if (nextTrack is null) {
                await LoggingService.LogInformationAsync("Music", "Bot has stoppped playback.");
                await player.StopAsync();
            } else {
                await player.PlayAsync(nextTrack);
                await LoggingService.LogInformationAsync("Music", $"Bot Now Playing: {nextTrack.Title} - {nextTrack.Uri}");
                await player.TextChannel.SendMessageAsync("", false, await EmbedHandler.CreateBasicEmbed("Now Playing", $"[{nextTrack.Title}]({nextTrack.Uri})", Color.Blue));
            }
        }
    }
}
