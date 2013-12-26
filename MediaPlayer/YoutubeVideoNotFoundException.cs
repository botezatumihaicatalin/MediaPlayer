using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer
{
    class YoutubeVideoNotFoundException :  Exception
    {
        public YoutubeVideoNotFoundException() : base("Youtube video id was not found!")
        {
            
        }
    }
}
