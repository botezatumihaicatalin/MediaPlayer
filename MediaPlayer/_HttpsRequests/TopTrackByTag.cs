using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Data.Xml.Dom;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MediaPlayer
{
    class TopTracksByTag
    {
        private HttpClient mClient;
        private HttpResponseMessage mResponse;
        private TrackCreator[] mTrackCreators;
        private bool mIsSearching;
        private int mRunningTasks;

        public String Tag
        {
            get;
            set;
        }
        public TopTracksByTag(String tag)
        {
            mRunningTasks = 0;
            Tag = tag;
            mClient = new HttpClient();
            mClient.MaxResponseContentBufferSize = 66000;
            mClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            mTrackCreators = new TrackCreator[12];
            for (int i = 0; i < 12; i++)
                mTrackCreators[i] = new TrackCreator();
        }

        public async Task CancelCurrentSearch()
        {
            mClient.CancelPendingRequests();
            mIsSearching = false;
            for (int i = 0; i < 12; i++)
                mTrackCreators[i].Cancel();
        }

        private async Task mThread(int index,  GridView contentHolder , XmlNodeList tracks)
        {
            for (int i = index; i < tracks.Length && mIsSearching; i += 12)
            {
                try
                {
                    mTrackCreators[index].XML = tracks[i].GetXml();
                    Track compute = await Task.Run(()=>mTrackCreators[index].GetFromXML());

                    if (compute == null)
                        continue; 

                    if (mIsSearching)
                    {                        
                        await contentHolder.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                        {
                            contentHolder.Items.Add(compute);
                            GlobalArray.list.Add(compute);
                        });
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        public async Task Get(GridView contentHolder, int no)
        {
            mIsSearching = true;
            mRunningTasks ++;
            String url = "http://ws.audioscrobbler.com/2.0/?method=tag.gettoptracks&tag=" +
             Tag +
             "&limit=" +
             no +
             "&api_key=30e44ae9c1e227a2f44f410e16e56586";
            string resp;
            
            if (!mIsSearching)
                return;

            try
            {
                mResponse = await mClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                resp = await mResponse.Content.ReadAsStringAsync();                
            }
            catch (Exception err)
            {
                throw err;
            }

            XmlDocument fullXML = new XmlDocument();
            fullXML.LoadXml(resp);
            XmlNodeList tracks = fullXML.GetElementsByTagName("track");

            if (tracks.Length != 0 && mIsSearching)
            {
                Preferences.addTag(Tag);
            }
            else
            {
                return;
            }
            List<Task> tasksList = new List<Task>();
            tasksList.Add(Task.Run(()=>mThread(0,contentHolder,tracks)));
            tasksList.Add(Task.Run(() => mThread(1, contentHolder, tracks)));
            tasksList.Add(Task.Run(() => mThread(2, contentHolder, tracks)));
            tasksList.Add(Task.Run(() => mThread(3, contentHolder, tracks)));
            tasksList.Add(Task.Run(() => mThread(4, contentHolder, tracks)));
            tasksList.Add(Task.Run(() => mThread(5, contentHolder, tracks)));
            tasksList.Add(Task.Run(() => mThread(6, contentHolder, tracks)));
            tasksList.Add(Task.Run(() => mThread(7, contentHolder, tracks)));
            tasksList.Add(Task.Run(() => mThread(8, contentHolder, tracks)));
            tasksList.Add(Task.Run(() => mThread(9, contentHolder, tracks)));
            tasksList.Add(Task.Run(() => mThread(10, contentHolder, tracks)));
            tasksList.Add(Task.Run(() => mThread(11, contentHolder, tracks)));
            await Task.WhenAll(tasksList.ToArray());
            mIsSearching = false;
        }
    }
}
