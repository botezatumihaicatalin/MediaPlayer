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

        public void CancelSearch()
        {
            mIsSearching = false;
            mSimilarTracks.CancelCurrentSearch();
        }

        private async void mSetupProgressBar(ProgressBar progressBar , int maxValue)
        {
            if (progressBar == null || maxValue < 0)
                return;

            await progressBar.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                progressBar.Value = 0;
                progressBar.Maximum = maxValue;
            });
        }

        public async Task GetTracksByPreferences(ProgressBar progressBar , GridView contentHolder)
        {
            mIsSearching = true;
            List<String> tags = await Preferences.getTopTags();

            int n = tags.Count;

            mSetupProgressBar(progressBar, ((int)(100 / n) * n));
            
            for (int i = 0; i < n && mIsSearching; i++)
            {
                mSimilarTracks = new TopTracksByTag(tags[i]);
                try
                {
                    await mSimilarTracks.Get(100 / n , progressBar , contentHolder);                    
                }
                catch (Exception error)
                {
                }
            }
        }

        public async Task GetTrackByTag(ProgressBar progressBar, GridView contentHolder, String tag)
        {
            mIsSearching = true;
            try
            {
                mSetupProgressBar(progressBar, 100);
                mSimilarTracks = new TopTracksByTag(tag);
                await mSimilarTracks.Get(100, progressBar , contentHolder);
            }
            catch (Exception)
            {

            }

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
                    return;
                }
                mSetupProgressBar(progressBar, 15 * Math.Min(tags.Count, 3));
                for (int i = 0; i < tags.Count && i < 3 && mIsSearching; i++)
                {
                    try
                    {
                        mSimilarTracks = new TopTracksByTag(tags[i]);
                        await mSimilarTracks.Get(15 , progressBar , contentHolder);
                    }
                    catch (Exception error)
                    {
                    }
                }
            }
        }
    }
}
