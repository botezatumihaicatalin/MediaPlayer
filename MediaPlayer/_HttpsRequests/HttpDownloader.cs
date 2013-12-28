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
        private byte[] mBuffer = new byte[1024 * 10];

        public bool IsDownloading
        {
            get { return mIsDownloading; }
        }

        public bool IsCanceled
        {
            get { return mIsCanceled; }
        }

        public async Task<String> GetHttp(Uri requestUri)
        {
            mIsCanceled = false;
            mIsDownloading = true;

            String result = "";
            HttpWebRequest request = WebRequest.CreateHttp(requestUri);
            request.Method = "GET";
            using (WebResponse response = await request.GetResponseAsync())
            using (Stream responseStream = response.GetResponseStream())
            {
                responseStream.ReadTimeout = 1000;
                responseStream.WriteTimeout = 1000;

                int bytesRead;
                while (!mIsCanceled && (bytesRead = responseStream.Read(mBuffer, 0, mBuffer.Length)) > 0)
                {
                    result += UTF8Encoding.UTF8.GetString(mBuffer, 0, bytesRead);
                }
            }

            mIsDownloading = false;
            if (mIsCanceled)
                throw new OperationCanceledException();

            return result;
        }

        public void Cancel()
        {
            mIsCanceled = true;
        }
    }
}
