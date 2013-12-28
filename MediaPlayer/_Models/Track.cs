using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer
{
    [DataContract]
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

            //setting trackID
            setTrackID();

            //setting updating date
            setUpdatingDate();
        }

        public String CacheUriString
        {
            get;
            set;
        }

        [DataMember]
        public Uri ImageUri
        {
            get;
            set;
        }
        [DataMember]
        public String Artist
        {
            get;
            set;
        }
        [DataMember]
        public String Name
        {
            get;
            set;
        }
        [DataMember]
        public String LastFMLink
        {
            get;
            set;
        }
        [DataMember]
        public int Duration
        {
            get;
            set;
        }
        [DataMember]
        public DateTime UpdatingDate
        {
            get;
            set;
        }
        [DataMember]
        public int TrackID
        {
            get;
            set;
        }
        [DataMember]
        public string VideoID
        {
            get;
            set;
        }
        public String toString()
        {
            return Name + "\n" + Artist + "\n" + Duration + "\n" + LastFMLink + "\n" + ImageUri.AbsoluteUri + "\n" + VideoID;
        }

        public String ToolTipInfo
        {
            get
            {
                // this is used for the Tool Tip. Not to use in generally.
                int cpyDuration = Duration;
                int hours = (cpyDuration / 3600);
                cpyDuration -= hours * 3600;
                int minutes = cpyDuration / 60;
                cpyDuration -= minutes * 60;

                string hoursString = hours.ToString();
                if (hoursString.Length == 1) hoursString = "0" + hoursString;
                string minutesString = minutes.ToString();
                if (minutesString.Length == 1) minutesString = "0" + minutesString;
                string secondsString = cpyDuration.ToString();
                if (secondsString.Length == 1) secondsString = "0" + secondsString;
                return "Track name : " + Name + "\nArtist : " + Artist + "\nDuration (h:m:s) : " + hoursString + ":" + minutesString + ":"+secondsString;
            }
        }

        public int setTrackID()//generates trackID from link to LastFM
        {
            const int BASE = 137;
            const int MOD = 10000000;

            if (LastFMLink == null)
                return 0;

            for (int i = 0; i < this.LastFMLink.Length; i++)
            {
                TrackID = TrackID * BASE + this.LastFMLink[i];
                while (TrackID > MOD)
                    TrackID -= MOD;
            }

            return TrackID;
        }

        private void setUpdatingDate()
        {
            UpdatingDate = DateTime.Now;
        }
    }
}
