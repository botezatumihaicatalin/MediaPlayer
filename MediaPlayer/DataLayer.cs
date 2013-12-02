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
        private TopTracksByTag[] similarTracks;
        public DataLayer()
        {
            similarTracks = new TopTracksByTag[0];
        }

        public async Task cancelSearch()
        {
            for (int i = 0; i < similarTracks.Length; i++)
                await similarTracks[i].cancelCurrentSearch();
        }

        public async Task getTracksByPreferences(FrameworkElement frameElement, GridView contentHolder)
        {
            List<String> tags = await Preferences.getTopTags();
            similarTracks = new TopTracksByTag[tags.Count];
            int n = tags.Count;
            for (int i = 0; i < n; i++)
            {
                similarTracks[i] = new TopTracksByTag(tags[i]);
            }

            for (int i = 0; i < n; i++)
            {
                try
                {
                    await similarTracks[i].get(frameElement, contentHolder, (int)100 / n);
                }
                catch (Exception error)
                {
                    if (error.Message == ExceptionMessages.CONNECTION_FAILED)
                        throw error;
                }
            }
        }

        public async Task getTrackByTag(FrameworkElement frameElement, GridView contentHolder, String tag)
        {
            similarTracks = new TopTracksByTag[4];
            for (int i = 0; i < 4; i++)
                similarTracks[i] = new TopTracksByTag("None");

            try
            {
                similarTracks[0] = new TopTracksByTag(tag);
                await similarTracks[0].get(frameElement, contentHolder, 100);
            }
            catch (Exception)
            {

            }

            if (contentHolder.Items.Count == 0)
            {
                //luam primele 3 taguri asemanatoare

                SimilarTags similarTags = new SimilarTags(tag);
                List<String> tags = null;
                try
                {
                    tags = await similarTags.get();
                }
                catch (Exception err)
                {
                    if (err.Message == ExceptionMessages.CONNECTION_FAILED)
                        return;
                }

                for (int i = 0; i < tags.Count && i < 3; i++)
                {
                    try
                    {
                        similarTracks[i + 1] = new TopTracksByTag(tags[i]);
                        await similarTracks[i + 1].get(frameElement, contentHolder, 15);
                    }
                    catch (Exception error)
                    {
                    }
                }
            }
        }
    }
}
