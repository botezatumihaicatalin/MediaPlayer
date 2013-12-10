using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;

namespace MediaPlayer
{
    class TrackCreator
    {
        private YoutubeDecoder mYTDecoder;
        private YoutubeSearch mYTSearch;
        private YoutubeStats mYTStats;
        private LastFMPageScrapper mPageScrapper;
        private bool mIsSearching;
        private XmlDocument mXmlDocument;

        public String XML
        {
            get;set;
        }
        

        public TrackCreator(String XML)
        {
            this.XML = XML;
            mYTDecoder = new YoutubeDecoder();
            mYTSearch = new YoutubeSearch();
            mYTStats = new YoutubeStats();
            mPageScrapper = new LastFMPageScrapper();
            mXmlDocument = new XmlDocument();
            mIsSearching = false;
        }

        public TrackCreator()
        {
            mYTDecoder = new YoutubeDecoder();
            mYTSearch = new YoutubeSearch();
            mYTStats = new YoutubeStats();
            mPageScrapper = new LastFMPageScrapper();
            mXmlDocument = new XmlDocument();
            mIsSearching = false;
            XML = "";
        }

        public void cancel()
        {
            mIsSearching = false;
            mYTSearch.cancel();
            mYTDecoder.cancel();
            mYTStats.cancel();
            mPageScrapper.cancel();
        }

        public bool isSearching()
        {
            return mIsSearching;
        }

        public async Task<Track> getFromXML()
        {
            mIsSearching = true;
            if (!mIsSearching)
            {
                return null;
            }
            
            mXmlDocument.LoadXml(XML);
            XmlNodeList names = mXmlDocument.GetElementsByTagName("name");
            XmlNodeList duration = mXmlDocument.GetElementsByTagName("duration");
            XmlNodeList music_url = mXmlDocument.GetElementsByTagName("url");
            XmlNodeList images = mXmlDocument.GetElementsByTagName("image");

            String trackName = names[0].InnerText;
            String artistName = names[1].InnerText;
            String musicLink = music_url[0].InnerText;

            Uri imageUri = new Uri("ms-appx:///Assets/default.jpg");

            String videoID = "NONE";
            String cacheUrl = "";
            Int32 durationNumber = 0;

            if (!mIsSearching)
            {
                return null;
            }

            // Check if last fm page has an youtube link
            try
            {
                mPageScrapper.LastFMUri = new Uri(musicLink);
                videoID = await mPageScrapper.getYoutubeId();

                mYTDecoder.VideoID = videoID;
                cacheUrl = await mYTDecoder.fetchURL();

                // Check if it contains signature , if not then the video it isn't good
                if (!cacheUrl.Contains("&signature="))
                {
                    videoID = "NONE";
                    cacheUrl = "";
                }
            }
            catch (Exception error)
            {
                if (error.Message == ExceptionMessages.CONNECTION_FAILED)
                {
                    return null;
                }
            }

            if (!mIsSearching)
            {
                return null;
            }

            // If videoID is "NONE" at this point it means either no video is on the LastFM page or the video cannot be played , so we will search Youtube
            if (videoID == "NONE")
            {
                try
                {
                    mYTSearch.ArtistName = artistName;
                    mYTSearch.TrackName = trackName;
                    Pair<string, string> pair = await mYTSearch.getAVideoCacheUri();

                    videoID = pair.second;
                    cacheUrl = pair.first;
                }
                catch (Exception error)
                {
                    if (error.Message == ExceptionMessages.CONNECTION_FAILED)
                        return null;
                }
            }

            if (!mIsSearching)
            {
                return null;
            }

            // If videoID is "NONE" at this point then the track doesn't have a video attached , so we dont show it.
            if (videoID == "NONE")
                return null;

            mYTStats.VideoID = videoID;

            var length = Convert.ToInt32(images.Length);
            if (length > 0)
            {
                imageUri = new Uri(images[length - 1].InnerText);
            }
            else
            {
                try
                {
                    await mYTStats.getData();
                    imageUri = new Uri(mYTStats.VideoImageURL);
                }
                catch (Exception er)
                {
                    imageUri = new Uri("ms-appx:///Assets/default.jpg");
                }
            }

            if (!mIsSearching)
            {
                return null;
            }

            durationNumber = Math.Max(mYTStats.DurationInSeconds, Convert.ToInt32(duration[0].InnerText));
            mIsSearching = false;
            return new Track(artistName, trackName, musicLink, durationNumber, imageUri, videoID, cacheUrl);
        }

    }
}
