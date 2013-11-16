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

<<<<<<< HEAD
        public String link
=======
        public String url
>>>>>>> 1b01e47b7dd6b0aa1182cf7a2189d6d6c8c4da83
        {
            get;
            set;
        }
    }
}
