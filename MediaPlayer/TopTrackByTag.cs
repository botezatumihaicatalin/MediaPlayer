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
        private bool mIsSearching;
        private int mRunningThreads;
        private HttpClient mClient;
        private HttpResponseMessage mResponse;
        private TrackCreator[] mTrackCreators;
        

        public String Tag
        {
            get;
            set;
        }
        public TopTracksByTag(String tag)
        {
            Tag = tag;
            mIsSearching = false;
            mRunningThreads = 0;
            mClient = new HttpClient();
            mClient.MaxResponseContentBufferSize = 66000;
            //mClient.Timeout = new TimeSpan(5000);
            mClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            mTrackCreators = new TrackCreator[12];
            for (int i = 0; i < 12; i++)
                mTrackCreators[i] = new TrackCreator();         

        }

        public async Task CancelCurrentSearch()
        {
            mIsSearching = false;
            mClient.CancelPendingRequests();

            for (int i = 0; i < 12; i++)
                mTrackCreators[i].Cancel();
            
            while (mRunningThreads > 0)
                await Task.Delay(10);
        }

        public async Task WaitTillFinish()
        {
            while (mRunningThreads > 0)
                await Task.Delay(10);
        }

        private async Task mThread(int index, FrameworkElement frameElement, GridView contentHolder , XmlNodeList tracks)
        {
            mRunningThreads++;
            for (int i = index; i < tracks.Length && mIsSearching; i += 12)
            {
                try
                {
                    mTrackCreators[index].XML = tracks[i].GetXml();
                    Track compute = await mTrackCreators[index].GetFromXML();
                    if (compute == null)
                    {
                        continue;
                    }
                    else if (mIsSearching)
                    {                        
                        await frameElement.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                        {
                            contentHolder.Items.Add(compute);
                            GlobalArray.list.Add(compute);
                        });
                    }
                }
                catch (Exception error)
                {

                }
            }
            mRunningThreads--;
        }

        public async Task Get(FrameworkElement frameElement, GridView contentHolder, int no)
        {
            mRunningThreads++;
            mIsSearching = true;
            mClient.CancelPendingRequests();
            String url = "http://ws.audioscrobbler.com/2.0/?method=tag.gettoptracks&tag=" +
             Tag +
             "&limit=" +
             no +
             "&api_key=30e44ae9c1e227a2f44f410e16e56586";
            string resp;

            try
            {
                mResponse = await mClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                resp = await mResponse.Content.ReadAsStringAsync();                
            }
            catch (Exception err)
            {
                mRunningThreads--;
                throw err;
            }

            XmlDocument fullXML = new XmlDocument();
            fullXML.LoadXml(resp);
            XmlNodeList tracks = fullXML.GetElementsByTagName("track");
            mRunningThreads = 0;

            if (tracks.Length != 0)
                Preferences.addTag(Tag);
            Task.Run(()=>mThread(0,frameElement,contentHolder,tracks));            
            Task.Run(()=>mThread(1,frameElement,contentHolder,tracks));
            Task.Run(()=>mThread(2,frameElement,contentHolder,tracks));
            Task.Run(()=>mThread(3,frameElement,contentHolder,tracks));
            Task.Run(()=>mThread(4,frameElement,contentHolder,tracks));
            Task.Run(()=>mThread(5,frameElement,contentHolder,tracks));
            Task.Run(()=>mThread(6,frameElement,contentHolder,tracks));
            Task.Run(()=>mThread(7,frameElement,contentHolder,tracks));            
            Task.Run(()=>mThread(8,frameElement,contentHolder,tracks));
            Task.Run(()=>mThread(9,frameElement,contentHolder,tracks));
            Task.Run(()=>mThread(10,frameElement,contentHolder,tracks));
            Task.Run(()=>mThread(11,frameElement,contentHolder,tracks));
            mRunningThreads--;
        }
    }
}
