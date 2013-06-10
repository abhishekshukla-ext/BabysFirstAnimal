using System;
using System.Windows.Media.Imaging;
using Microsoft.Devices.Sensors;

namespace WaveMachine
{
    public partial class MainPage
    {
        Accelerometer _accelerometer;
        DateTimeOffset _acceleratingQuicklyForwardTime = DateTimeOffset.MinValue;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            TextPageLoaded();
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
                    _accelerometer.ReadingChanged -= AccelerometerReadingChanged;
                    _accelerometer.Stop();
                    _accelerometer = null;
                    Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/Oceans.xaml",UriKind.Relative)));
                    
                }
                catch (AccelerometerFailedException exception)
                {

                }
            }
        }

        private void TextPageLoaded()
        {
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