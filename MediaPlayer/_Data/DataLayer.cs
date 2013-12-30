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

        private async void mSetupProgressBar(ProgressBar progressBar, Visibility vizibility)
        {
            if (progressBar == null)
                return;

            await progressBar.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                progressBar.Visibility = vizibility;
            });
        }

        public async Task GetTracksByPreferences(ProgressBar progressBar, GridView contentHolder)
        {
            mIsSearching = true;
            if (mIsSearching)
                mSetupProgressBar(progressBar, Visibility.Visible);
            List<String> tags = await Preferences.getTopTags();

            int n = tags.Count;


            for (int i = 0; i < n && mIsSearching; i++)
            {
                mSimilarTracks = new TopTracksByTag(tags[i]);
                try
                {
                    await mSimilarTracks.Get(100 / n, contentHolder);
                }
                catch (Exception error)
                {
                }
            }

            if (mIsSearching)
                mSetupProgressBar(progressBar, Visibility.Collapsed);
        }

        public async Task GetTrackByTag(ProgressBar progressBar, GridView contentHolder, String tag)
        {
            mIsSearching = true;
            if (mIsSearching)
                mSetupProgressBar(progressBar, Visibility.Visible);
            
            try
            {
                mSimilarTracks = new TopTracksByTag(tag);
                await mSimilarTracks.Get(100, contentHolder);
            }
            catch (Exception)
            {

            }

            if (mIsSearching)
                mSetupProgressBar(progressBar, Visibility.Collapsed);

            if (contentHolder.Items.Count == 0 && mIsSearching)
            {
                //luam primele 3 taguri asemanatoare
                if (mIsSearching)
                    mSetupProgressBar(progressBar, Visibility.Visible);
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
                for (int i = 0; i < tags.Count && i < 3 && mIsSearching; i++)
                {
                    try
                    {
                        mSimilarTracks = new TopTracksByTag(tags[i]);
                        await mSimilarTracks.Get(15, contentHolder);
                    }
                    catch (Exception error)
                    {
                    }
                }
                if (mIsSearching)
                    mSetupProgressBar(progressBar, Visibility.Collapsed);
            }
        }
    }
}
