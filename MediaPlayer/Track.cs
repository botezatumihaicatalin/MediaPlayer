using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer.Common
{
    class Track
    {
        
        public Track(String artist = null , String name = null , String link = null , Int32 duration = 0 , Uri imageUri = null , String videoID = null , String cacheUri = null)
        {
            this.Artist = artist;
            this.Name = name;
            this.LastFMLink = link;
            Duration = duration;
            ImageUri = imageUri;
            VideoID = videoID;
            CacheUriString = "http://127.0.0.1";
        }

        public String CacheUriString
        {
            get;
            set;
        }

        public Uri ImageUri
        {
            get;
            set;
        }
        public String Artist
        {
            get;
            set;
        }
        public String Name
        {
            get;
            set;
        }
        public String LastFMLink
        {
            get;
            set;
        }
        public int Duration
        {
            get;
            set;
        }
        public string VideoID
        {
            get;
            set;
        }
        public String toString()
        {
            return Name + "\n" + Artist + "\n" + Duration + "\n" + LastFMLink + "\n" + ImageUri.AbsoluteUri + "\n" + VideoID;
        }

        public async Task getYoutubeUri()
        {
            if (VideoID == null)
            {
                LastFMPageScrapper scrapper = new LastFMPageScrapper(new Uri(LastFMLink));
                try
                {
                    VideoID = await scrapper.getYoutubeId();
                }
                catch (Exception er)
                {

                }
            }
            if (CacheUriString == "http://127.0.0.1")
            {
                YoutubeDecoder decoder = new YoutubeDecoder(VideoID);
                await decoder.getVideoCacheURL();
                CacheUriString = decoder.DirectVideoURL;
            }
        }
    }
}
