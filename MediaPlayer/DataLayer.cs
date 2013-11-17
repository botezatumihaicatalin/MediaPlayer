using MediaPlayer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MediaPlayer
{
    class DataLayer
    {
        public async Task getTracksByPreferences(FrameworkElement frameElement, GridView contentHolder) 
        { 
            List<String> tags = Preferences.getTags(); 
            int n = tags.Count; 
            for (int i = 0; i < n; i++) 
            { 
                try 
                { 
                    TopTrackByTag similarTracks = new TopTrackByTag(tags[i]); 
                    await similarTracks.get(frameElement, contentHolder, (int)60 / n); 
                } 
                catch (Exception e) { } 
            } 
        }

        public async Task getTrackByTag(FrameworkElement frameElement , GridView contentHolder , String tag)
        {
            try
            {
                TopTrackByTag t = new TopTrackByTag(tag);
                await t.get(frameElement , contentHolder);
            }
            catch (Exception er)
            {

            }           

            if (contentHolder.Items.Count == 0) // empty list
            {
                //luam primele 3 taguri asemanatoare
                SimilarTags similarTags = new SimilarTags(tag);
                List<String> tags = await similarTags.get();

                for(int i = 0; i < tags.Count && i < 3; i++)
                {
                    try
                    {
                        TopTrackByTag similarTracks = new TopTrackByTag(tags[i]);
                        await similarTracks.get(frameElement , contentHolder,15);
                    }
                    catch (Exception er)
                    {

                    }
                }
            }
        }
    }
}
