namespace MauiVlc
{
    public partial class MainPage : ContentPage
    {
        public string VideoUrl { get; set; } = "rtsp://admin:Korgi123@70.41.96.204:554";

        public MainPage()
        {
            InitializeComponent();

            this.BindingContext = this;

            ((App)Application.Current).OnSleepEvent += MainPage_OnSleepEvent;
            ((App)Application.Current).OnResumeEvent += MainPage_OnResumeEvent;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        private void MainPage_OnResumeEvent()
        {
            videoViewer.Play();
        }

        private void MainPage_OnSleepEvent()
        {
            videoViewer.Pause();
        }

        private void OnStartRecordingClicked(object sender, EventArgs e)
        {
            videoViewer.StartRecording();
        }

        private void OnStopRecordingClicked(object sender, EventArgs e)
        {
            videoViewer.StopRecording();
        }
    }

}
