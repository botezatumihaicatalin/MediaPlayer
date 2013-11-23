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

            string[] artist_tokens = Regex.Split(ArtistName, " ");
            string[] track_tokens = Regex.Split(TrackName, " ");
            string query_string = "";
            int i;
            for (i = 0; i < artist_tokens.Length; i++)
            {
                query_string += artist_tokens[i] + "+";
            }
            for (i = 0; i < track_tokens.Length-1; i++)
            {
                query_string += track_tokens[i] + "+";
            }
            if (track_tokens.Length - 1 >= 0)
                query_string += track_tokens[track_tokens.Length - 1];

            string search_url = "https://gdata.youtube.com/feeds/api/videos?q=" + query_string + "&orderby=relevance";
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
                    await decoder.getVideoCacheURL();
                    if (decoder.DirectVideoURL.Contains("&signature="))
                    {
                        return new Pair<string,string>(decoder.DirectVideoURL,decoder.VideoID);
                    }
                }           

            }

            return new Pair<string, string>("http://127.0.0.1", "NONE");
        }
    }
}
