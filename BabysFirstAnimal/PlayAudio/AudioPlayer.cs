﻿using System;
using System.Windows;
using Microsoft.Phone.BackgroundAudio;
using System.Collections.Generic;

namespace PlayAudio
{
    public class AudioPlayer : AudioPlayerAgent
    {
        private static volatile bool _classInitialized;
        static int _currentTrackNumber;

        // A playlist made up of AudioTrack items.
        private static readonly List<AudioTrack> PlayList = new List<AudioTrack>
                                                        {
                                                            new AudioTrack(new Uri("s01.wav", UriKind.Relative),
                                                                "Cow",
                                                                "Cow",
                                                                "Hit01.png",
                                                                null),
                                                            new AudioTrack(new Uri("s02.wav", UriKind.Relative),
                                                                "Kitty Cat",
                                                                "Kitty Cat",
                                                                "Hit02.png",
                                                                null),
                                                            new AudioTrack(new Uri("s03.wav", UriKind.Relative),
                                                                "Horse",
                                                                "Horse",
                                                                "Hit03.png",
                                                                null),
                                                            new AudioTrack(new Uri("s04.wav", UriKind.Relative),
                                                                "Chicken",
                                                                "Chicken",
                                                                "Hit04.png",
                                                                null),
                                                            new AudioTrack(new Uri("s05.wav", UriKind.Relative),
                                                                "Monkey",
                                                                "Monkey",
                                                                "Hit05.png",
                                                                null),
                                                            new AudioTrack(new Uri("s06.wav", UriKind.Relative),
                                                                "Lion",
                                                                "Lion",
                                                                "Hit06.png",
                                                                null),
                                                            new AudioTrack(new Uri("s07.wav", UriKind.Relative),
                                                                "Seal",
                                                                "Seal",
                                                                "Hit07.png",
                                                                null),
                                                            new AudioTrack(new Uri("s08.wav", UriKind.Relative),
                                                                "Guinea Pig",
                                                                "Guinea Pig",
                                                                "Hit08.png",
                                                                null),
                                                            new AudioTrack(new Uri("s09.wav", UriKind.Relative),
                                                                "Dog",
                                                                "Dog",
                                                                "Hit09.png",
                                                                null),
                                                            new AudioTrack(new Uri("s10.wav", UriKind.Relative),
                                                                "Hippo",
                                                                "Hippo",
                                                                "Hit10.png",
                                                                null),
                                                        };

        /// <remarks>
        /// AudioPlayer instances can share the same process. 
        /// Static fields can be used to share state between AudioPlayer instances
        /// or to communicate with the Audio Streaming agent.
        /// </remarks>
        public AudioPlayer()
        {
            if (!_classInitialized)
            {
                _classInitialized = true;
                // Subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += AudioPlayer_UnhandledException;
                });
            }
        }

        /// Code to execute on Unhandled Exceptions
        private void AudioPlayer_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// Called when the playstate changes, except for the Error state (see OnError)
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track playing at the time the playstate changed</param>
        /// <param name="playState">The new playstate of the player</param>
        /// <remarks>
        /// Play State changes cannot be cancelled. They are raised even if the application
        /// caused the state change itself, assuming the application has opted-in to the callback.
        /// 
        /// Notable playstate events: 
        /// (a) TrackEnded: invoked when the player has no current track. The agent can set the next track.
        /// (b) TrackReady: an audio track has been set and it is now ready for playack.
        /// 
        /// Call NotifyComplete() only once, after the agent request has been completed, including async callbacks.
        /// </remarks>
        protected override void OnPlayStateChanged(BackgroundAudioPlayer player, AudioTrack track, PlayState playState)
        {
            switch (playState)
            {
                case PlayState.TrackEnded:
                    break;
                case PlayState.TrackReady:
                    player.Play();
                    break;
                case PlayState.Shutdown:
                    // TODO: Handle the shutdown state here (e.g. save state)
                    break;
                case PlayState.Unknown:
                    break;
                case PlayState.Stopped:
                    break;
                case PlayState.Paused:
                    player.Pause();
                    break;
                case PlayState.Playing:
                    break;
                case PlayState.BufferingStarted:
                    break;
                case PlayState.BufferingStopped:
                    break;
                case PlayState.Rewinding:
                    break;
                case PlayState.FastForwarding:
                    break;
            }

            NotifyComplete();
        }


        /// <summary>
        /// Called when the user requests an action using application/system provided UI
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track playing at the time of the user action</param>
        /// <param name="action">The action the user has requested</param>
        /// <param name="param">The data associated with the requested action.
        /// In the current version this parameter is only for use with the Seek action,
        /// to indicate the requested position of an audio track</param>
        /// <remarks>
        /// User actions do not automatically make any changes in system state; the agent is responsible
        /// for carrying out the user actions if they are supported.
        /// 
        /// Call NotifyComplete() only once, after the agent request has been completed, including async callbacks.
        /// </remarks>
        protected override void OnUserAction(BackgroundAudioPlayer player, AudioTrack track, UserAction action, object param)
        {
            switch (action)
            {
                case UserAction.Play:
                    PlayTrack(player);
                    break;
                case UserAction.Stop:
                    player.Stop();
                    break;
                case UserAction.Pause:
                    player.Pause();
                    break;
                case UserAction.FastForward:
                    player.FastForward();
                    break;
                case UserAction.Rewind:
                    player.Rewind();
                    break;
                case UserAction.Seek:
                    player.Position = (TimeSpan)param;
                    break;
                case UserAction.SkipNext:
                    PlayNextTrack(player);
                    break;
                case UserAction.SkipPrevious:
                    PlayPreviousTrack(player);
                    break;
            }
            NotifyComplete();
        }

        /// <summary>
        /// Called whenever there is an error with playback, such as an AudioTrack not downloading correctly
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track that had the error</param>
        /// <param name="error">The error that occured</param>
        /// <param name="isFatal">If true, playback cannot continue and playback of the track will stop</param>
        /// <remarks>
        /// This method is not guaranteed to be called in all cases. For example, if the background agent 
        /// itself has an unhandled exception, it won't get called back to handle its own errors.
        /// </remarks>
        protected override void OnError(BackgroundAudioPlayer player, AudioTrack track, Exception error, bool isFatal)
        {
            if (isFatal)
            {
                Abort();
            }
            else
            {
                NotifyComplete();
            }

        }

        /// <summary>
        /// Called when the agent request is getting cancelled
        /// </summary>
        /// <remarks>
        /// Once the request is Cancelled, the agent gets 5 seconds to finish its work,
        /// by calling NotifyComplete()/Abort().
        /// </remarks>
        protected override void OnCancel()
        {

        }

        private void PlayNextTrack(BackgroundAudioPlayer player)
        {
            if (++_currentTrackNumber >= PlayList.Count)
            {
                _currentTrackNumber = 0;
            }

            PlayTrack(player);
        }

        private void PlayPreviousTrack(BackgroundAudioPlayer player)
        {
            if (--_currentTrackNumber < 0)
            {
                _currentTrackNumber = PlayList.Count - 1;
            }

            PlayTrack(player);
        }

        private void PlayTrack(BackgroundAudioPlayer player)
        {
            player.Track = PlayList[_currentTrackNumber];
        }
    }
}
