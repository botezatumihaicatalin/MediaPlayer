using MediaPlayer._Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public bool IsSearching
        {
            get { return mIsSearching; }
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

        public void Cancel()
        {
            mIsSearching = false;
            mYTSearch.Cancel();
            mYTDecoder.Cancel();
            mYTStats.Cancel();
            mPageScrapper.Cancel();
        }

        /// <summary>
        /// Creates the Track from the XML given in constructor of TrackCreator.
        /// </summary>
        /// <returns> Returns Track </returns>
        public async Task<Track> GetFromXML()
        {
            mIsSearching = true;

            if (!mIsSearching)
            {
                throw new OperationCanceledException();
            }
            
            mXmlDocument.LoadXml(XML);
            XmlNodeList names = mXmlDocument.GetElementsByTagName("name");
            XmlNodeList duration = mXmlDocument.GetElementsByTagName("duration");
            XmlNodeList music_url = mXmlDocument.GetElementsByTagName("url");
            XmlNodeList images = mXmlDocument.GetElementsByTagName("image");

            String trackName = names[0].InnerText;
            String artistName = names[1].InnerText;
            String musicLink = music_url[0].InnerText;

            if (!mIsSearching)
            {
                throw new OperationCanceledException();
            }

            Track track = new Track();
            //try to get this track from database
            track = await DatabaseHelper.GetTrackFromDatabase(musicLink);

            if (!mIsSearching)
            {
                throw new OperationCanceledException();
            }

            if (track != null)
            {
                mYTDecoder.VideoID = track.VideoID;
                track.CacheUriString = await mYTDecoder.FetchURL();               
                return track;
            }

            //track wasn't found, so we get all the needed info
            Uri imageUri = new Uri("http://simpleicon.com/wp-content/uploads/music-note-5.png");

            String videoID = "NONE";
            String cacheUrl = "";
            Int32 durationNumber = -1;

            if (!mIsSearching)
            {
                throw new OperationCanceledException();
            }

            // Check if last fm page has an youtube link
            try
            {
                mPageScrapper.LastFMUri = new Uri(musicLink);
                videoID = await mPageScrapper.GetYoutubeVideoId();

                if (!mIsSearching)
                {
                    throw new OperationCanceledException();
                }

                mYTDecoder.VideoID = videoID;
                cacheUrl = await mYTDecoder.FetchURL();

                if (!mIsSearching)
                {
                    throw new OperationCanceledException();
                }

                // Check if it contains signature. If not, the video it isn't good
                if (!cacheUrl.Contains("&signature="))
                {
                    videoID = "NONE";
                    cacheUrl = "";
                }
                else
                {
                    // If the video is OK , then we check if XML contains duration. If it does then we get it from XML , no need to download anymore for this.
                    if (duration.Length == 1)
                    {
                        durationNumber = Convert.ToInt32(duration[0].InnerText);
                    }
                }
            }
            catch (YoutubeVideoUrlNotFoundException)
            {
                videoID = "NONE";
                cacheUrl = "";
            }
            catch (YoutubeVideoNotFoundException)
            {
                videoID = "NONE";
                cacheUrl = "";
            }
            catch (Exception error)
            {
                throw error;
            }

            if (!mIsSearching)
            {
                throw new OperationCanceledException();
            }

            // If videoID is "NONE" at this point it means either no video is on the LastFM page or the video cannot be played , so we will search Youtube
            if (videoID == "NONE")
            {
                mYTSearch.ArtistName = artistName;
                mYTSearch.TrackName = trackName;
                Pair<string, string> pair = await mYTSearch.GetAVideoCacheUri();

                if (!mIsSearching)
                {
                    throw new OperationCanceledException();
                }

                videoID = pair.Second;
                cacheUrl = pair.First;

                if (videoID != "NONE")
                {
                    mYTStats.VideoID = videoID;
                    await mYTStats.GetData();
                    durationNumber = mYTStats.DurationInSeconds;
                }
            }

            if (!mIsSearching)
            {
                throw new OperationCanceledException();
            }

            // If videoID is "NONE" at this point then the track doesn't have a video attached or we couldnt find one, so we dont show it.
            if (videoID == "NONE")
                return null;

            if (!mIsSearching)
            {
                throw new OperationCanceledException();
            }

            var length = Convert.ToInt32(images.Length);

            if (length > 0)
            {
                imageUri = new Uri(images[length - 1].InnerText);
            }
            else
            {
                try
                {
                    mYTStats.VideoID = videoID;
                    await mYTStats.GetData();
                    imageUri = new Uri(mYTStats.VideoImageURL);
                }
                catch (Exception)
                {
                    imageUri = new Uri("http://simpleicon.com/wp-content/uploads/music-note-5.png");
                }
            }

            if (!mIsSearching)
            {
                throw new OperationCanceledException();
            }

            // Check if durationNumber is set , we dont want it to be -1.
            // At this point we are sure we have a video 
            if (durationNumber == -1)
            {
                mYTStats.VideoID = videoID;
                await mYTStats.GetData();
                durationNumber = mYTStats.DurationInSeconds;
            }

            if (!mIsSearching)
            {
                throw new OperationCanceledException();
            }


            //now we have all the info for the track, we store them to the database
            track = new Track(artistName, trackName, musicLink, durationNumber, imageUri, videoID, cacheUrl);
            await DatabaseHelper.AddTrackToDatabase(track);
            
            mIsSearching = false;

            return track;
        }

    }
}
