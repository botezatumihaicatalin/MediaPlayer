using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MediaPlayer
{
    class DataLayer
    {
        private TopTrackByTag[] similarTracks;
        public DataLayer()
        {
            similarTracks = new TopTrackByTag[0];
        }

        public void cancelSearch()
        {
            for (int i = 0; i < similarTracks.Length; i++)
                similarTracks[i].cancelCurrentSearch();
        }

        public async Task getTracksByPreferences(FrameworkElement frameElement, GridView contentHolder)
        {
            List<String> tags = await Preferences.getTopTags();
            similarTracks = new TopTrackByTag[tags.Count];
            int n = tags.Count;
            for (int i = 0; i < n; i++)
            {
                similarTracks[i] = new TopTrackByTag(tags[i]);
            }

            for (int i = 0; i < n; i++)
            {
                try
                {
                    similarTracks[i].get(frameElement, contentHolder, (int)100 / n);
                }
                catch (Exception)
                {

                }
            }
        }

        public async Task getTrackByTag(FrameworkElement frameElement, GridView contentHolder, String tag)
        {
            similarTracks = new TopTrackByTag[4];
            for (int i = 0; i < 4; i++)
                similarTracks[i] = new TopTrackByTag("None");

            try
            {
                similarTracks[0] = new TopTrackByTag(tag);
                similarTracks[0].get(frameElement, contentHolder, 50);
            }
            catch (Exception er)
            {

            }

            if (contentHolder.Items.Count == 0)
            {
                //luam primele 3 taguri asemanatoare
                SimilarTags similarTags = new SimilarTags(tag);
                List<String> tags = await similarTags.get();

                for (int i = 0; i < tags.Count && i < 3; i++)
                {
                    try
                    {
                        similarTracks[i + 1] = new TopTrackByTag(tags[i]);
                        similarTracks[i + 1].get(frameElement, contentHolder, 15);
                    }
                    catch (Exception er)
                    {

                    }
                }
            }
        }
    }
}
