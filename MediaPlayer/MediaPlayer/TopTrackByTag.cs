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
<<<<<<< HEAD
            request.Timeout = 10;
=======
>>>>>>> Now we can get youtube from LastFM
>>>>>>> 9fc57ed9f3849e6019e8c0c8d97d1259f4cd879b
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
