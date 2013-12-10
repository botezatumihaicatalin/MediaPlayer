using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace MediaPlayer
{
    class PlayList
    {
        private static List<Track> mTrackList = new List<Track>();
        private static List<String> mFileNames = new List<String>();

        public static async Task addToPlayList(Track track)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFolder nextFolder = null;
            bool is_found = false;
            try
            {
                nextFolder = await storageFolder.GetFolderAsync("Playlist");
                is_found = true;
            }
            catch (Exception er)
            {
            }

            if (!is_found)
            {
                nextFolder = await storageFolder.CreateFolderAsync("Playlist");
            }
            Random random = new Random();
            string fileName = track.Name + " " + track.Artist + "-" + random.Next(0, 100000);
            fileName = fileName.Replace("\\", "");
            fileName = fileName.Replace("/", "");
            fileName = fileName.Replace(":", "");
            fileName = fileName.Replace("\"", "");
            fileName = fileName.Replace("?", "");
            fileName = fileName.Replace("<", "");
            fileName = fileName.Replace(">", "");
            fileName = fileName.Replace("|", "");
            fileName = fileName.Replace("*", "");
            StorageFile file = await nextFolder.CreateFileAsync(fileName + ".track");
            DataContractSerializer serializer = new DataContractSerializer(typeof(Track));
            using (Stream fileStream = await file.OpenStreamForWriteAsync())
            {
                serializer.WriteObject(fileStream , track);
                fileStream.Flush();
            }
        }

        public static async Task removeFromPlayList(Track track , GridView contentHolder)
        {
            int position = mTrackList.IndexOf(track);

            if (position == -1)
                return;

            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFolder nextFolder = null;
            try
            {
                nextFolder = await storageFolder.GetFolderAsync("Playlist");
            }
            catch (Exception er)
            {
                return;
            }
            StorageFile file = await nextFolder.GetFileAsync(mFileNames[position]);
            await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            await contentHolder.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                lock (contentHolder)
                {
                    mFileNames.RemoveAt(position);
                    mTrackList.RemoveAt(position);
                    contentHolder.Items.RemoveAt(position);
                }
            });
        }

        public static async Task readPlayList(GridView contentHolder = null)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFolder nextFolder = null;
            try
            {
                nextFolder = await storageFolder.GetFolderAsync("Playlist");
            }
            catch (Exception er)
            {
                return;
            }
            var read = await nextFolder.GetFilesAsync();
            DataContractSerializer serializer = new DataContractSerializer(typeof(Track));
            for (int i = 0; i < read.Count; i++)
            {
                using (Stream fileStream = await read[i].OpenStreamForWriteAsync())
                {
                    Track track = (Track)serializer.ReadObject(fileStream);
                    await contentHolder.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        lock (contentHolder)
                        {
                            if (contentHolder != null)
                                contentHolder.Items.Add(track);
                            mTrackList.Add(track);
                            mFileNames.Add(read[i].Name);
                        }
                    });
                }
            }            
        }

        public static Track getElement(int index)
        {
            return mTrackList[index];
        }

        public static int getIndex(Track track)
        {
            return mTrackList.IndexOf(track);
        }

        public static int getSize()
        {
            return mTrackList.Count;
        }

        public static void updateElement(Track track , int index)
        {
            mTrackList[index] = track;
        }

    }
}
