using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Data.Xml.Dom;
using Windows.UI.Popups;

namespace MediaPlayer
{
    class TopTrackByTag
    {
        public TopTrackByTag()
        {
            
        }

        public async void get(string tag)
        {

            String url = "http://ws.audioscrobbler.com/2.0/?method=tag.gettoptracks&tag=" +
             tag +
             "&api_key=30e44ae9c1e227a2f44f410e16e56586";

            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
<<<<<<< HEAD
=======

>>>>>>> d2f3ab2b52bfab936cc28968f422a88ac74e564b
            System.Net.WebResponse response;
            try
            {
                response = await request.GetResponseAsync();
            }
            catch
            {
                throw new Exception("No internet connection or bad request!");
            }

            String resp = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
            

            XmlDocument fullXML = new XmlDocument();
            fullXML.LoadXml(resp);

            XmlNodeList itemNodes = fullXML.GetElementsByTagName("name");

            foreach (XmlElement item in itemNodes)
            {
                String pageUrl = item.InnerText;
                await new MessageDialog(pageUrl).ShowAsync();
            }


        }
    }
}
