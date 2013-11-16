using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer.Common
{
    class Track
    {
        public Track(String artist, String name, String link)
        {
            this.artist = artist;
            this.name = name;
            this.link = link;
        }
        public String artist
        {
            get;
            set;
        }

        public String name
        {
            get;
            set;
        }


        public String link
        {
            get;
            set;
        }
    }
}
