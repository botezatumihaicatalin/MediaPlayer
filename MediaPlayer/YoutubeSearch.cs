using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Http;

namespace MediaPlayer
{
    class YoutubeSearch
    {
        public String TrackName
        {
            get;set;
        }
        public String ArtistName
        {
            get;set;
        }
        private static readonly Regex youtubeVideoRegex = new Regex(@"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)");
        private HttpDownloader mClient;
        private HttpResponseMessage mResponse;
        private YoutubeDecoder mDecoder;
        private bool mIsRunning;


        public YoutubeSearch(String trackName, String artistName)
        {
            TrackName = trackName;
            ArtistName = artistName;
            mClient = new HttpDownloader();
            mDecoder = new YoutubeDecoder();
            mIsRunning = false;
        }

        public YoutubeSearch()
        {
            mClient = new HttpDownloader();
            mDecoder = new YoutubeDecoder();
            mIsRunning = false;
            TrackName = "";
            ArtistName = "";
        }
        
        public void Cancel()
        {
            mIsRunning = false;
            mClient.Cancel();
        }
        public async Task<Pair<string,string>> GetAVideoCacheUri()
        {
            mClient.Cancel();
            // example https://gdata.youtube.com/feeds/api/videos?q=Lady+Gaga+Alejandro&orderby=relevance
            mIsRunning = true;
            string search_url = "https://gdata.youtube.com/feeds/api/videos?q=" + ArtistName + " " + TrackName + "&orderby=relevance";        
            string contents;
            try
            {
                contents = await mClient.GetHttp(new Uri(search_url));
            }
            catch (Exception error)
            {
                mIsRunning = false;
                throw error;
            }
            
            string string_to_search = "media:player url=";
            string youtubeVideo = "";
            string videoId = "";

            for (int index = 0; index <= contents.Length && mIsRunning; index += string_to_search.Length)
            {
                index = contents.IndexOf(string_to_search, index);
                if (index == -1) return new Pair<string,string>("http://127.0.0.1","NONE");
                int copy = index;
                copy += string_to_search.Length + 1;
                youtubeVideo = "";
                while (contents[copy] != "'"[0])
                {
                    youtubeVideo += contents[copy];
                    copy++;
                }
                Match youtubeMatch = youtubeVideoRegex.Match(youtubeVideo);
                if (youtubeMatch.Success)
                {
                    videoId = youtubeMatch.Groups[1].Value;
                    mDecoder.VideoID = videoId;
                    string directVideoURL = "";
                    try
                    {
                        directVideoURL = await mDecoder.FetchURL();
                    }
                    catch (Exception)
                    {
                        directVideoURL = "";
                    }
                    if (directVideoURL.Contains("&signature="))
                    {
                        mIsRunning = false;
                        return new Pair<string,string>(directVideoURL,mDecoder.VideoID);
                    }
                }
            }
            mIsRunning = false;
            return new Pair<string, string>("http://127.0.0.1", "NONE");
        }
    }
}
