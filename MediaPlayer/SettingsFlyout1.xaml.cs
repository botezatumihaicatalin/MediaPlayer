using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace MediaPlayer
{
    public sealed partial class SettingsFlyout1 : SettingsFlyout
    {
        public static Boolean Queue = false;
        public static bool History = true;
        public static String DefaultQuery = "";
        public SettingsFlyout1()
        {
            this.InitializeComponent();
            this.Loaded += (sender, e) =>
            {
                Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += SettingsFlyout1_AcceleratorKeyActivated;
            };
            this.Unloaded += (sender, e) =>
            {
                Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated -= SettingsFlyout1_AcceleratorKeyActivated;
            };
        }
        void SettingsFlyout1_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            if (args.EventType == CoreAcceleratorKeyEventType.SystemKeyDown &&
                args.VirtualKey == VirtualKey.Left)
            {
                var coreWindow = Window.Current.CoreWindow;
                var downState = CoreVirtualKeyStates.Down;

                bool menuKey = (coreWindow.GetKeyState(VirtualKey.Menu) & downState) == downState;
                bool controlKey = (coreWindow.GetKeyState(VirtualKey.Control) & downState) == downState;
                bool shiftKey = (coreWindow.GetKeyState(VirtualKey.Shift) & downState) == downState;

                if (menuKey && !controlKey && !shiftKey)
                {
                    args.Handled = true;
                    this.Hide();
                }
            }
        }

        private void SearchQueue_Toggled(object sender, RoutedEventArgs e)
        {
            if (SearchQueue.IsOn == true)
                Queue = true;
            else if (SearchQueue.IsOn == false)
                Queue = false;
            
        }

        private void SearchHistory_Toggled(object sender, RoutedEventArgs e)
        {
            if (SearchHistory.IsOn == true)
                History = true;
            else if (SearchHistory.IsOn == false)
                History = false;

        }

          
        private void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            var searchSuggestionManager = new Windows.ApplicationModel.Search.Core.SearchSuggestionManager();
            searchSuggestionManager.ClearHistory();   
        }

        private void SearchHistory_Loaded(object sender, RoutedEventArgs e)
        {
            SearchHistory.IsOn = History;
        }

        private void SearchQueue_Loaded(object sender, RoutedEventArgs e)
        {
            SearchQueue.IsOn = Queue; 
        }
    }
}
