using System;
using System.Windows.Media.Imaging;
using Microsoft.Devices.Sensors;
using Microsoft.Phone.BackgroundAudio;

namespace WaveMachine
{
    public partial class Oceans
    {
        Accelerometer _accelerometer;
        DateTimeOffset _acceleratingQuicklyForwardTime = DateTimeOffset.MinValue;
        
        public Oceans()
        {
            InitializeComponent();
            BackgroundAudioPlayer.Instance.PlayStateChanged += InstancePlayStateChanged;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            TextPageLoaded();
            if (PlayState.Playing == BackgroundAudioPlayer.Instance.PlayerState)
            {
                BackgroundImage.Source = new BitmapImage(new Uri("Assets/" + BackgroundAudioPlayer.Instance.Track.Album, UriKind.Relative)); 
            }
            else
            {
                BackgroundAudioPlayer.Instance.Play();
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Dispatcher.BeginInvoke(() => BackgroundAudioPlayer.Instance.Stop());
            _accelerometer.ReadingChanged -= AccelerometerReadingChanged;
            _accelerometer.Stop();
            _accelerometer = null;
        }

        void InstancePlayStateChanged(object sender, EventArgs e)
        {
            if (null != BackgroundAudioPlayer.Instance.Track)
            {
                BackgroundImage.Source = new BitmapImage(new Uri("Assets/" + BackgroundAudioPlayer.Instance.Track.Album,UriKind.Relative)); 
            }
        }

        public void AccelerometerReadingChanged(object sender, AccelerometerReadingEventArgs e)
        {
            // Only pay attention to large-enough magnitudes in the X dimension
            if (Math.Abs(e.X) < Math.Abs(1.5))
                return;

            // See if the force is in the same direction as the threshold
            // (forward punching motion)
            if (e.X * 1.5 > 0)
            {
                // Forward acceleration
                _acceleratingQuicklyForwardTime = e.Timestamp;
            }
            else if (e.Timestamp - _acceleratingQuicklyForwardTime
                      < TimeSpan.FromSeconds(.2))
            {
                // This is large backward force shortly after the forward force.
                // Time to make the punching noise!

                _acceleratingQuicklyForwardTime = DateTimeOffset.MinValue;

                // We're on a different thread, so transition to the UI thread.
                // This is a requirement for playing the sound effect.
                try
                {
                    Dispatcher.BeginInvoke(() => BackgroundAudioPlayer.Instance.SkipNext());
                }
                catch (AccelerometerFailedException exception)
                {

                }
            }
        }

        private void TextPageLoaded()
        {
            BackgroundImage.Source = new BitmapImage(new Uri("Assets/Hit.png", UriKind.RelativeOrAbsolute));
            if (_accelerometer == null)
            {
                _accelerometer = new Accelerometer();
                _accelerometer.ReadingChanged += AccelerometerReadingChanged;
                try
                {
                    _accelerometer.Start();
                }
                catch (AccelerometerFailedException exception)
                {

                }
            }
        }
    }
}