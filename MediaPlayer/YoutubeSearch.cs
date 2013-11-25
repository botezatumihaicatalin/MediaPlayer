using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace MediaPlayer
{
    class YoutubeSearch
    {
        String TrackName
        {
            get;set;
        }
        String ArtistName
        {
            get;set;
        }
        private static readonly Regex youtubeVideoRegex = new Regex(@"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)");

        public YoutubeSearch(String trackName, String artistName)
        {
            TrackName = trackName;
            ArtistName = artistName;
        }
        public async Task<Pair<string,string>> getAVideoCacheUri()
        {
            if (TrackName == null || ArtistName == null)
            {
                throw new Exception();
            }
            // example https://gdata.youtube.com/feeds/api/videos?q=Lady+Gaga+Alejandro&orderby=relevance

            string search_url = "https://gdata.youtube.com/feeds/api/videos?q=" + ArtistName + " " + TrackName + "&orderby=relevance";
            WebRequest request = WebRequest.Create(search_url);
            WebResponse response = null;

            YoutubeDecoder decoder = new YoutubeDecoder();
            try
            {
                response = await request.GetResponseAsync();
            }
            catch(Exception er)
            {
                throw new Exception(ExceptionMessages.CONNECTION_FAILED);
            }
            string contents = new StreamReader(response.GetResponseStream()).ReadToEnd();
            string string_to_search = "media:player url=";
            string youtubeVideo = "";
            string videoId = "";

            for (int index = 0; index <= contents.Length; index += string_to_search.Length)
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
                    decoder.VideoID = videoId;
                    string directVideoURL = "";
                    try
                    {
                        directVideoURL = await decoder.fetchURL();
                    }
                    catch (Exception er)
                    {
                        directVideoURL = "";
                    }
                    if (directVideoURL.Contains("&signature="))
                    {
                        return new Pair<string,string>(directVideoURL,decoder.VideoID);
                    }
                }           

            }

            return new Pair<string, string>("http://127.0.0.1", "NONE");
        }
    }
}
