using MauiVlc.Controls;
using Microsoft.Maui.Handlers;
using System;
using LibVLCSharp.Platforms.iOS;
using LibVLCSharp.Shared;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace MauiVlc.Handlers
{
    public partial class MediaViewerHandler : ViewHandler<MediaViewer, VideoView>
    {
        VideoView _videoView;
        LibVLC _libVLC;
        LibVLCSharp.Shared.MediaPlayer _mediaPlayer;
        LibVLCSharp.Shared.Media _media;


        protected override VideoView CreatePlatformView()
        {
            return new VideoView();
        }

        protected override void ConnectHandler(VideoView nativeView)
        {
            base.ConnectHandler(nativeView);

            Core.Initialize();

            PrepareControl(nativeView);
            HandleUrl(VirtualView.VideoUrl);

            VirtualView.PlayRequested += VirtualView_PlayRequested;
            VirtualView.PauseRequested += VirtualView_PauseRequested;
            VirtualView.StartRecordingRequested += VirtualView_StartRecordingRequested;
            VirtualView.StopRecordingRequested += VirtualView_StopRecordingRequested;
        }

        private void VirtualView_StartRecordingRequested()
        {
            if (_mediaPlayer == null)
                return;

            //_mediaPlayer.Stop(); // Stop current media

            _media = _mediaPlayer.Media;
            _media.AddOption(":screen-fps=60");
            _media.AddOption(":sout=#transcode{vcodec=h264,sfilter=marq:logo}:file{dst=/storage/emulated/0/Download/Video.mp4}");
            _media.AddOption(":screen-width=1920:screen-height=1080");
            //_media.AddOption(":sout-keep");
            //_media.AddOption(":no-audio");

            _mediaPlayer.Play(_media);
        }

        private void VirtualView_StopRecordingRequested()
        {
            if (_mediaPlayer == null || _mediaPlayer.Media == null)
                return;

            _mediaPlayer.Stop();
            //_mediaPlayer.Media.Dispose();
            //_mediaPlayer.Media = null;
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

        private void VirtualView_PlayRequested()
        {
            PrepareControl(_videoView);
            HandleUrl(VirtualView.VideoUrl);
            _mediaPlayer.Play();
        }

        private void VirtualView_PauseRequested()
        {
            _mediaPlayer.Pause();
        }

        public void PrepareControl(VideoView nativeView)
        {
            _libVLC = new LibVLC(enableDebugLogs: true);
            _mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVLC)
            {
                EnableHardwareDecoding = true
            };
            _media = _mediaPlayer.Media;
            _videoView = nativeView ?? new VideoView();
            _videoView.MediaPlayer = _mediaPlayer;
            _mediaPlayer.EnableHardwareDecoding = false;
            VirtualView.PauseRequested += VirtualView_PauseRequested;
            VirtualView.PlayRequested += VirtualView_PlayRequested;
            VirtualView.StartRecordingRequested += VirtualView_StartRecordingRequested;
            VirtualView.StopRecordingRequested += VirtualView_StopRecordingRequested;
        }

        public void HandleUrl(string url)
        {
            try
            {
                if (url.EndsWith("/"))
                {
                    url = url.TrimEnd('/');
                }

                if (!string.IsNullOrEmpty(url))
                {
                    var media = new LibVLCSharp.Shared.Media(_libVLC, url, FromType.FromLocation);

                    _mediaPlayer.NetworkCaching = 1500;

                    if (_mediaPlayer.Media != null)
                    {
                        _mediaPlayer.Stop();
                        _mediaPlayer.Media.Dispose();
                    }

                    _mediaPlayer.Media = media;
                    _mediaPlayer.Mute = true;

                    _videoView.MediaPlayer.Play();
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }
    }
}

