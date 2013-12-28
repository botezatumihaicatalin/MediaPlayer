using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MediaPlayer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Splash : Page
    {
        public SplashScreen splashScreen;
        public Rect splashImage;
        public Splash(SplashScreen splashscreen, bool loadState)
        {
            this.InitializeComponent();
            splashScreen = splashscreen;
            splashImage = splashScreen.ImageLocation; splashScreen.Dismissed += new TypedEventHandler<SplashScreen, Object>(splashScreen_Dismissed);
            PositionProgressRing();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }
        void splashScreen_Dismissed(SplashScreen sender, object args)
        {

        }
        private void PositionProgressRing()
        {
            ProgressRingImage.SetValue(Canvas.TopProperty, (splashImage.Y + splashImage.Height + 100));
            ProgressRingImage.SetValue(Canvas.LeftProperty, splashImage.X);
            ProgressRingImage.Visibility = Visibility.Visible;
            SplashScreenImage.SetValue(Canvas.TopProperty, splashImage.Y);
            SplashScreenImage.SetValue(Canvas.LeftProperty, splashImage.X);
            SplashScreenImage.Height = splashImage.Height;
            SplashScreenImage.Width = splashImage.Width;
            SplashScreenImage.Visibility = Visibility.Visible; 
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            splashScreen.Dismissed -= splashScreen_Dismissed;

        }
    }
}
