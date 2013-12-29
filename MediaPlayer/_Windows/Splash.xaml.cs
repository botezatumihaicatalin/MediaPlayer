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
            splashScreen.Dismissed += new TypedEventHandler<SplashScreen, Object>(splashScreen_Dismissed);
            splashImage = splashscreen.ImageLocation;
            PostionImage();
            PositionProgressRing();
        }
        private void PostionImage()
        {
            SplashScreenImage.SetValue(Canvas.LeftProperty, splashImage.X);
            SplashScreenImage.SetValue(Canvas.TopProperty, splashImage.Y);
            SplashScreenImage.Height = splashImage.Height;
            SplashScreenImage.Width = splashImage.Width;
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
            ProgressRing.SetValue(Canvas.LeftProperty, splashImage.X + (splashImage.Width * 0.5) - (ProgressRing.Width * 0.5));
            ProgressRing.SetValue(Canvas.TopProperty, (splashImage.Y + splashImage.Height + splashImage.Height * 0.1));
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            splashScreen.Dismissed -= splashScreen_Dismissed;

        }
    }
}
