using System;
using Android.Graphics;
using Android.Media;
using Android.Views;
using LibVLCSharp.Platforms.Android;
using LibVLCSharp.Shared;
using MauiVlc.Controls;
using Microsoft.Maui.Handlers;

namespace MauiVlc.Handlers
{
	public partial class MediaViewerHandler: ViewHandler<MediaViewer, VideoView>
    {
        VideoView _videoView;
        LibVLC _libVLC;
        LibVLCSharp.Shared.MediaPlayer _mediaPlayer;
        Media _media;
        MediaRecorder _mediaRecorder;

        protected override VideoView CreatePlatformView() => new VideoView(Context);

        protected override void ConnectHandler(VideoView nativeView)
        {
            base.ConnectHandler(nativeView);

            PrepareControl(nativeView);
            HandleUrl(VirtualView.VideoUrl);
            VirtualView.PlayRequested += VirtualView_PlayRequested;
            VirtualView.PauseRequested += VirtualView_PauseRequested;
            VirtualView.StartRecordingRequested += VirtualView_StartRecordingRequested;
            VirtualView.StopRecordingRequested += VirtualView_StopRecordingRequested;
            //base.ConnectHandler(nativeView);
        }

        private void VirtualView_PlayRequested()
        {
            PrepareControl(_videoView);

            // Ensure the surface is ready before starting playback
            if (_videoView.Holder.Surface != null && _videoView.Holder.Surface.IsValid)
            {
                HandleUrl(VirtualView.VideoUrl);
                _mediaPlayer.Play(); // Ensure video playback starts only after the surface is ready
            }
            else
            {
                // Set a listener for surface readiness
                _videoView.Holder.AddCallback(new SurfaceHolderCallback(() =>
                {
                    HandleUrl(VirtualView.VideoUrl);
                    _mediaPlayer.Play();
                }));
            }
        }

        public class SurfaceHolderCallback : Java.Lang.Object, ISurfaceHolderCallback
        {
            private Action _onSurfaceCreated;

            public SurfaceHolderCallback(Action onSurfaceCreated)
            {
                _onSurfaceCreated = onSurfaceCreated;
            }

            public void SurfaceCreated(ISurfaceHolder holder)
            {
                _onSurfaceCreated?.Invoke();
            }

            public void SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height) { }
            public void SurfaceDestroyed(ISurfaceHolder holder) { }
        }

        private void VirtualView_PauseRequested()
        {
            _mediaPlayer.Pause();
        }

        private void VirtualView_StartRecordingRequested()
        {
            Task.Run(() =>
            {
                if (_mediaPlayer == null || _mediaPlayer.Media == null)
                    return;

                VirtualView.Dispatcher.Dispatch(() => {
                    var media = _mediaPlayer.Media;
                    if (media != null)
                    {
                        _mediaPlayer.Pause();
                        string path = Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
                        string filePath = System.IO.Path.Combine(path, "recording.mp4");
                        media.AddOption($":sout=#file{{dst={filePath}}}");
                        //media.AddOption(":sout=#file{dst=/storage/emulated/0/Download/recording.mp4}");
                        media.AddOption(":sout-mux-caching=2000");
                        media.AddOption(":sout-keep");
                        media.AddOption(":no-audio");
                        _mediaPlayer.Play();
                    }
                });
            });
        }

        private void VirtualView_StopRecordingRequested()
        {
            Task.Run(() =>
            {
                if (_mediaPlayer == null || _mediaPlayer.Media == null)
                    return;

                VirtualView.Dispatcher.Dispatch(() => {
                    var media = _mediaPlayer.Media;
                    if (media != null)
                    {
                        // Remove the sout (stream output) options to stop recording
                        media.AddOption(":no-sout"); // Stop stream output

                        // Resume playback without recording if necessary
                        if (_mediaPlayer.IsPlaying)
                        {
                            _mediaPlayer.Play();  // Continue playback without recording
                        }

                        // Optionally dispose of the media if recording is finished
                        _mediaPlayer.Media.Dispose();
                        _mediaPlayer.Media = null;
                    }
                });
            });
        }
        protected override void DisconnectHandler(VideoView nativeView)
        {
            VirtualView.PauseRequested -= VirtualView_PauseRequested;
            VirtualView.PlayRequested -= VirtualView_PlayRequested;
            VirtualView.StartRecordingRequested -= VirtualView_StartRecordingRequested;
            VirtualView.StopRecordingRequested -= VirtualView_StopRecordingRequested;
            nativeView.Dispose();
            base.DisconnectHandler(nativeView);
        }

        private void PrepareControl(VideoView nativeView)
        {
            _libVLC = new LibVLC(enableDebugLogs: true);
            _mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVLC)
            {
                EnableHardwareDecoding = true
            };

            _videoView = nativeView ?? new VideoView(Context);
            _videoView.MediaPlayer = _mediaPlayer;

            VirtualView.PauseRequested += VirtualView_PauseRequested;
            VirtualView.PlayRequested += VirtualView_PlayRequested;
            VirtualView.PauseRequested += VirtualView_StartRecordingRequested;
            VirtualView.PlayRequested += VirtualView_StopRecordingRequested;
        }

        private void HandleUrl(string url)
        {
            try
            {

                if (url.EndsWith("/"))
                {
                    url = url.TrimEnd('/');
                }

                if (!string.IsNullOrEmpty(url))
                {
                    var media = new Media(_libVLC, url, FromType.FromLocation);

                    _mediaPlayer.NetworkCaching = 1500;

                    if (_mediaPlayer.Media != null)
                    {
                        _mediaPlayer.Stop();
                        _mediaPlayer.Media.Dispose();
                    }

                    _mediaPlayer.Media = media;
                    _mediaPlayer.Mute = true;
                    _mediaPlayer.Play();
                }
            }
            catch (Exception ex)
            {
            }
        }

    }
}

