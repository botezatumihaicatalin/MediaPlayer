using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Data.Xml.Dom;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace MediaPlayer
{
    class SimilarTags
    {
        public String Tag
        {
            get;
            set;
        }
        public SimilarTags(String tag)
        {
            Tag = tag;
        }

        public async Task<List<String>> get()
        {

            String url = "http://ws.audioscrobbler.com/2.0/?method=tag.getsimilar&tag=" +
             Tag + 
             "&api_key=30e44ae9c1e227a2f44f410e16e56586";

            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);

            System.Net.WebResponse response;
            try
            {
                response = await request.GetResponseAsync();
            }
            catch
            {
                throw new Exception(ExceptionMessages.CONNECTION_FAILED);
            }

            String resp = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
            

            XmlDocument fullXML = new XmlDocument();
            fullXML.LoadXml(resp);            
            XmlNodeList tracks = fullXML.GetElementsByTagName("name");
            List<String> list = new List<String>();

            foreach(XmlElement elements in tracks)
            {
                list.Add(elements.InnerText);
            }
            return list;

        }
    }
}
