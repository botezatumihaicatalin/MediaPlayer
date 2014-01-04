using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;

namespace MediaPlayer._HttpsRequests
{
    class ThmbnailImageDownloader
    {
        private static ThmbnailImageDownloader mInstance = null;
        private bool mIsRunning;
        private Queue<Uri> mUriQueue;
        private static object mLock = new object();

        private ThmbnailImageDownloader()
        {
            mIsRunning = false;
            mUriQueue = new Queue<Uri>();
            Task.Run(() => mLoop());
        }

        /// <summary>
        /// Gets the instance of this object
        /// </summary>
        /// <returns>Returns the instance of this object</returns>
        public static ThmbnailImageDownloader GetInstance()
        {
            lock (mLock)
            {
                if (mInstance == null)
                    mInstance = new ThmbnailImageDownloader();

                return mInstance;
            }
        }

        /// <summary>
        /// Enqueues the Uri to be downloaded and saved to Local Application Folder with the name 'thumbnail.jpg' and sets 
        /// the MediaControl.AlbumArt value with the image when its downloaded.
        /// </summary>
        /// <param name="uri">The Uri to be downloaded</param>
        public void EnqueueToDownload(Uri uri)
        {
            if (uri != null)
            {
                lock (mLock)
                    mUriQueue.Enqueue(uri);
            }
        }

        /// <summary>
        /// Clears the download queue.
        /// </summary>
        public void ClearDownloadQueue()
        {
            lock(mLock)
            {
                mUriQueue.Clear();
            }
        }

        private async Task mLoop()
        {
            mIsRunning = true;
            while(mIsRunning)
            {
                if (mUriQueue.Count > 0)
                    await mSaveImageToFile(mUriQueue.Dequeue());
                MediaControl.AlbumArt = new Uri("ms-appdata:///Local/thumbnail.jpg");
                await Task.Delay(10);
            }
        }

        private static async Task mSaveImageToFile(Uri path)
        {
            HttpWebRequest request;
            WebResponse response;
            Stream stream;
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile storageFile = await storageFolder.CreateFileAsync("thumbnail.jpg", CreationCollisionOption.ReplaceExisting);

            request = (HttpWebRequest)WebRequest.Create(path);
            try
            {
                request = (HttpWebRequest)WebRequest.Create(path);
                response = await request.GetResponseAsync();
                stream = response.GetResponseStream();

                using (Stream fileStream = await storageFile.OpenStreamForWriteAsync())
                {
                    byte[] buffer = new byte[1024 * 10];
                    int bytesRead = -1;
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, bytesRead);
                    }
                }
                stream.Dispose();
                response.Dispose();
            }
            catch (Exception er)
            {

            }
        }

        ~ThmbnailImageDownloader()
        {
            mIsRunning = false;
        }

    }
}
