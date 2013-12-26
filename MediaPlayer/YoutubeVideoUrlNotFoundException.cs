using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer
{
    class YoutubeVideoUrlNotFoundException : Exception
    {
        public YoutubeVideoUrlNotFoundException() : base("Youtube video URL was not found!")
        {

        }
    }
}
