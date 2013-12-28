using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer
{
    class HttpDownloader
    {
        private bool mIsCanceled = false;
        private bool mIsDownloading = false;
        private int mBufferSize = 1024 * 100; // default is 100 kilobytes

        public bool IsDownloading
        {
            get { return mIsDownloading; }
        }

        public bool IsCanceled
        {
            get { return mIsCanceled; }
        }

        public int BufferSize
        {
            get { return mBufferSize; }
            set { mBufferSize = value; }
        }

        public async Task<String> GetHttp(Uri requestUri)
        {
            byte[] buffer = new byte[mBufferSize];

            lock(this)
            {
                mIsCanceled = false;
                mIsDownloading = true;
            }


            String result = "";
            HttpWebRequest request = WebRequest.CreateHttp(requestUri);
            request.Method = "GET";
            using (WebResponse response = await request.GetResponseAsync())
            using (Stream responseStream = response.GetResponseStream())
            {
                
                if (mIsCanceled)
                    throw new OperationCanceledException();

                responseStream.ReadTimeout = 1000;
                responseStream.WriteTimeout = 1000;

                int bytesRead;
                while (!mIsCanceled && (bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    result += UTF8Encoding.UTF8.GetString(buffer, 0, bytesRead);
                }
            }

            lock(this)
            {
                mIsDownloading = false;
                if (mIsCanceled)
                    throw new OperationCanceledException();
            }

            return result;
        }

        public void Cancel()
        {
            mIsCanceled = true;
        }
    }
}
