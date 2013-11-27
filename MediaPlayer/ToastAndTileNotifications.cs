using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace MediaPlayer
{
    class ToastAndTileNotifications
    {
        public static void LiveTileOn(String artists, String tracks, String images)
        {
            string tileXmlString =
               "<tile>"
               + "<visual version='2'>"
               + "<binding template='TileSquare150x150PeekImageAndText02' fallback='TileSquarePeekImageAndText02' branding='None'>"
               + "<image id='1' " + "src='" + images + "' />"
               + "<text id='1'>" + artists + "</text>"
               + "<text id='2'>" + tracks + "</text>"
               + "</binding>"
               + "<binding template='TileWide310x150ImageAndText02' fallback='TileWide310x150ImageAndText02' branding='None'>"
               + "<image id='1' " + "src='" + images + "' />"
               + "<text id='1'>" + artists + "</text>"
               + "<text id='2'>" + tracks + "</text>"
               + "</binding>"
               + "</visual>"
               + "</tile>";

            // Create a DOM.
            XmlDocument tileDOM = new XmlDocument();
            tileDOM.LoadXml(tileXmlString);
            TileNotification tile = new TileNotification(tileDOM);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tile);

        }
        public static void ToastNotifications(string artists, string tracks, string images)
        {
            artists = artists.Replace("&", "and");
            tracks = tracks.Replace("&", "and");
            string toastXmlString = "<toast>"
                            + "<visual version='2'>"
                            + "<binding template='ToastImageAndText04'>"
                            + "<image id='1' " + "src='" + images + "' />"
                            + "<text id='1'>" + artists + "</text>"
                            + "<text id='2'>" + tracks + "</text>"
                            + "</binding>"
                            + "</visual>"
                            + "</toast>";

            Windows.Data.Xml.Dom.XmlDocument toastDOM = new Windows.Data.Xml.Dom.XmlDocument();
            toastDOM.LoadXml(toastXmlString);

            // Create a toast, then create a ToastNotifier object to show
            // the toast
            try
            {
                ToastNotification toast = new ToastNotification(toastDOM);
                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
            catch (Exception er)
            {
                toastXmlString = "<toast>"
                            + "<visual version='2'>"
                            + "<binding template='ToastImageAndText04'>"
                            + "<text id='1'>" + artists + "</text>"
                            + "<text id='2'>" + tracks + "</text>"
                            + "</binding>"
                            + "</visual>"
                            + "</toast>";
                ToastNotification toast = new ToastNotification(toastDOM);
                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
        }
    }
}
