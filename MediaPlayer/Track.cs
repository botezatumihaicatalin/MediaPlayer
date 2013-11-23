using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer.Common
{
    class Track
    {
        
        public Track(String artist = null , String name = null , String link = null , Int32 duration = 0 , Uri imageUri = null , String videoID = null , String cacheUri = "https://127.0.0.1")
        {
            this.Artist = artist;
            this.Name = name;
            this.LastFMLink = link;
            Duration = duration;
            ImageUri = imageUri;
            VideoID = videoID;
            CacheUriString = cacheUri;
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
    }
}
