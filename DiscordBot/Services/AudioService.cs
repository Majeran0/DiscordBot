using Discord;
using Discord.WebSocket;
using Victoria;
using System;
using System.Text;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using DiscordBot.DataStructs;
using DiscordBot.Handlers;
using Victoria.Enums;
using Victoria.Responses.Search;

namespace DiscordBot.Services
{
    public class AudioService
    {
        private readonly LavaNode _lavaNode;

        public AudioService(LavaNode lavaNode)
            => _lavaNode = lavaNode;

        private readonly Lazy<ConcurrentDictionary<ulong, AudioOptions>> _lazyOptions
            = new Lazy<ConcurrentDictionary<ulong, AudioOptions>>();

        private ConcurrentDictionary<ulong, AudioOptions> Options
            => _lazyOptions.Value;

        public async Task<Embed> JoinOrPlayAsync(SocketGuildUser user, IMessageChannel textChannel, IGuild guild,
            string query = null)
        {
            if (user.VoiceChannel == null)
                return await EmbedHandler.CreateErrorEmbed("Music, Join/Play", "You must first join a voice channel.");

            if (query == null)
            {
                await _lavaNode.JoinAsync(user.VoiceChannel, textChannel as ITextChannel);
                Options.TryAdd(user.Guild.Id, new AudioOptions
                {
                    Summoner = user
                });
                await LoggingService.LogInformationAsync("Music",
                    $"Now connected to {user.VoiceChannel.Name} and bound to {textChannel}.");
                return await EmbedHandler.CreateBasicEmbed("Music",
                    $"Now connected to {user.VoiceChannel.Name} and bound to {textChannel.Name}.", Color.Blue);
            }
            else
            {
                try
                {
                    if (!_lavaNode.HasPlayer(guild))
                    {
                        //when you use play before join
                        await _lavaNode.JoinAsync(user.VoiceChannel, textChannel as ITextChannel);
                        Options.TryAdd(user.Guild.Id, new AudioOptions
                        {
                            Summoner = user
                        });
                    }

                    var player = _lavaNode.GetPlayer(guild);

                    var search = await _lavaNode.SearchYouTubeAsync(query);

                    if (search.Status == SearchStatus.NoMatches)
                        return await EmbedHandler.CreateErrorEmbed("Music", $"No matches for {query}.");

                    var track = search.Tracks.FirstOrDefault();

                    if (player.Track != null &&
                        player.PlayerState == PlayerState.Playing ||
                        player.PlayerState == PlayerState.Paused)
                    {
                        player.Queue.Enqueue(track);
                        await LoggingService.LogInformationAsync("Music",
                            $"{track.Title} has ben added to music queue");
                        return await EmbedHandler.CreateBasicEmbed("Music",
                            $"{track.Title} has ben added to music queue", Color.Blue);
                    }

                    await player.PlayAsync(track);
                    await LoggingService.LogInformationAsync("Music",
                        $"Bot Now Playing: {track.Title}\nUrl: {track.Url}");
                    return await EmbedHandler.CreateBasicEmbed("Music",
                        $"Bot Now Playing: {track.Title}\nUrl: {track.Url}", Color.Blue);
                }
                catch (Exception ex)
                {
                    return await EmbedHandler.CreateErrorEmbed("Music, Join/Play", ex.ToString());
                }
            }
        }

        public async Task<Embed> LeaveAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);
                if (player.PlayerState == PlayerState.Playing)
                    await player.StopAsync();

                var channelName = player.VoiceChannel.Name;
                await _lavaNode.LeaveAsync(player.VoiceChannel);

                await LoggingService.LogInformationAsync("Music", $"Bot has left {channelName}.");
                return await EmbedHandler.CreateBasicEmbed("Music", $"Bot has left {channelName}.", Color.Blue);
            }
            catch (InvalidOperationException ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Leave", ex.ToString());
            }
        }

        public async Task<Embed> ListAsync(IGuild guild)
        {
            try
            {
                var descriptionBuilder = new StringBuilder();
                var player = _lavaNode.GetPlayer(guild);
                if (player == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", "Could not acquire player");

                if (player.PlayerState == PlayerState.Playing)
                {
                    if (player.Queue.Count < 1 && player.Track != null)
                        return await EmbedHandler.CreateBasicEmbed($"Now Playing {player.Track.Title}",
                            "Nothing else is queued", Color.Blue);
                    else
                    {
                        var trackNum = 2;
                        foreach (var track in player.Queue)
                        {
                            descriptionBuilder.Append($"{trackNum}: [{track.Title}]({track.Url}) - {track.Id}\n");
                            trackNum++;
                        }

                        return await EmbedHandler.CreateBasicEmbed("Music Playlist",
                            $"Now Playing: [{player.Track.Title}]({player.Track.Url})\n{descriptionBuilder.ToString()}",
                            Color.Blue);
                    }
                }
                else
                {
                    return await EmbedHandler.CreateErrorEmbed("Music, List", "Nothing is played right now.");
                }
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, List", ex.Message);
            }
        }

        public async Task<Embed> SkipTrackAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);
                if (player == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not acquire player.");
                if (player.Queue.Count < 1)
                {
                    return await EmbedHandler.CreateErrorEmbed("Music, SkipTrack",
                        $"Unable To skip a track as there is only One or No songs currently playing.");
                }
                else
                {
                    try
                    {
                        var currentTrack = player.Track;
                        await player.SkipAsync();
                        await LoggingService.LogInformationAsync("Music", $"Bot skipped: {currentTrack.Title}");
                        return await EmbedHandler.CreateBasicEmbed("Music Skip",
                            $"Successfully skipped {currentTrack.Title}", Color.Blue);
                    }
                    catch (Exception ex)
                    {
                        return await EmbedHandler.CreateErrorEmbed("Music, Skip", ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Skip", ex.ToString());
            }
        }

        public async Task<Embed> StopAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);
                if (player == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", "Could not acquire player");

                if (player.PlayerState == PlayerState.Playing)
                    await player.StopAsync();
                player.Queue.Clear();
                await LoggingService.LogInformationAsync("Music", "Bot has stopped playback.");
                return await EmbedHandler.CreateBasicEmbed("Music stop",
                    "Bot has stopped playback and cleared playlist", Color.Blue);
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Stop", ex.ToString());
            }
        }

        public async Task<string> VolumeAsync(IGuild guild, ushort volume)
        {
            if (volume >= 150 || volume <= 0)
                return "Volume must be between 0 and 150";
            try
            {
                if (!_lavaNode.HasPlayer(guild))
                    return "Bot must be connected to audio channel";
                var player = _lavaNode.GetPlayer(guild);
                await player.UpdateVolumeAsync(volume);
                await LoggingService.LogInformationAsync("Music", $"Bot volume set to {volume}");
                return $"Volume has been set to {volume}.";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> Pause(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);
                if (player.PlayerState == PlayerState.Paused)
                {
                    await player.ResumeAsync();
                    return $"**Resumed:** Now playing {player.Track.Title}";
                }

                await player.PauseAsync();
                return "**Paused**";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> Resume(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);
                if (player.PlayerState != PlayerState.Paused)
                    await player.ResumeAsync();
                return $"**Resumed:** {player.Track.Title}";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }

        public async Task OnFinished(LavaPlayer player, LavaTrack track, TrackEndReason reason)
        {
            if (reason is TrackEndReason.LoadFailed or TrackEndReason.Cleanup)
                return;
            player.Queue.TryDequeue(out LavaTrack nextTrack);

            if (nextTrack is null)
            {
                await LoggingService.LogInformationAsync("Music", "Bot has stopped playback.");
                await player.StopAsync();
            }
            else
            {
                await player.PlayAsync(nextTrack);
                await LoggingService.LogInformationAsync("Music",
                    $"Bot Now Playing: {nextTrack.Title} - {nextTrack.Url}");
                await player.TextChannel.SendMessageAsync("", false,
                    await EmbedHandler.CreateBasicEmbed("Now Playing", $"[{nextTrack.Title}]({nextTrack.Url})",
                        Color.Blue));
            }
        }
    }
}