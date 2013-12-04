using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer
{
    abstract class ExceptionMessages
    {
        public static string INVALID_URI = "Invalid URI!";
        public static string YOUTUBE_VIDEO_ID_NOT_FOUND = "Youtube video id was not found!";
        public static string CONNECTION_FAILED = "Connection failed!";
        public static string YOUTUBE_VIDEO_URL_NOT_FOUND = "Youtube video URL was not found!";
        public static string NO_SIMILAR_TAGS_FOUND = "No similar tags were found!";
    }
}
