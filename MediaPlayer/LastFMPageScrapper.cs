using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer
{
    
    class LastFMPageScrapper
    {
        public Uri LastFMUri
        {
            get;
            set;
        }
        private HttpDownloader mClient;
        private HttpResponseMessage mResponse;
        public LastFMPageScrapper(Uri uri)
        {
            LastFMUri = uri;
            mClient = new HttpDownloader();
        }

        public LastFMPageScrapper()
        {
            LastFMUri = new Uri("http://127.0.0.1");
            mClient = new HttpDownloader();
        }

        public void Cancel()
        {
            mClient.Cancel();
        }
        public async Task<string> GetYoutubeId()
        {
            string ID = "";
            string page_source = await mClient.GetHttp(LastFMUri);
            string search_string = "<embed src=\"http://www.youtube.com/v/";
            
            int index = page_source.IndexOf(search_string);

            if (index == -1)
            {
                throw new YoutubeVideoNotFoundException(); 
            }
            index += search_string.Length;
            int end = index;
            while (page_source[end] != '?' && page_source[end] != '&' && page_source[end] != '"')
            {
                end++;
            }
            ID = page_source.Substring(index, end - index);

            if (ID.Length <= 3)
            {
                throw new YoutubeVideoNotFoundException();
            }

            return ID;
        }
    }
}
