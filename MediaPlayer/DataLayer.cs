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
        private TopTracksByTag mSimilarTracks;
        private bool mIsSearching;
        public DataLayer()
        {
            mSimilarTracks = new TopTracksByTag("");
            mIsSearching = false;
        }

        public async Task CancelSearch()
        {
            mIsSearching = false;
            await mSimilarTracks.CancelCurrentSearch();
        }

        public async Task GetTracksByPreferences(FrameworkElement frameElement, GridView contentHolder)
        {
            mIsSearching = true;
            List<String> tags = await Preferences.getTopTags();
            int n = tags.Count;
            for (int i = 0; i < n && mIsSearching; i++)
            {
                mSimilarTracks = new TopTracksByTag(tags[i]);
                try
                {
                    await mSimilarTracks.Get(frameElement, contentHolder, 100 / n);                    
                }
                catch (Exception error)
                {
                    if (error.Message == ExceptionMessages.CONNECTION_FAILED)
                        throw error;
                }
                await Task.Delay(1000);
                await mSimilarTracks.WaitTillFinish();
            }
        }

        public async Task GetTrackByTag(FrameworkElement frameElement, GridView contentHolder, String tag)
        {
            mIsSearching = true;
            try
            {
                mSimilarTracks = new TopTracksByTag(tag);
                await mSimilarTracks.Get(frameElement, contentHolder, 100);
            }
            catch (Exception)
            {

            }

            await mSimilarTracks.WaitTillFinish();

            if (!mIsSearching) return;
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
                

                for (int i = 0; i < tags.Count && i < 3 && mIsSearching; i++)
                {
                    try
                    {
                        mSimilarTracks = new TopTracksByTag(tags[i]);
                        await mSimilarTracks.Get(frameElement, contentHolder, 15);
                        await mSimilarTracks.WaitTillFinish();
                    }
                    catch (Exception error)
                    {
                    }
                }
            }
        }
    }
}
