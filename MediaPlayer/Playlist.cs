using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace MediaPlayer
{
    class PlayList
    {
        private static List<Track> mTrackList = new List<Track>();
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
            StorageFile file = await nextFolder.CreateFileAsync(fileName);
            await FileIO.AppendTextAsync(file, track.Name + "\r\n");
            await FileIO.AppendTextAsync(file, track.Artist + "\r\n");
            await FileIO.AppendTextAsync(file, track.LastFMLink + "\r\n");
            await FileIO.AppendTextAsync(file, track.ImageUri.AbsoluteUri + "\r\n");
            await FileIO.AppendTextAsync(file, track.VideoID + "\r\n");
            await FileIO.AppendTextAsync(file, track.Duration.ToString());
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
            for (int i = 0; i < read.Count; i++)
            {
                IList<String> lines = await FileIO.ReadLinesAsync(read[i]);
                Track t = new Track(lines[1], lines[0], lines[2], Convert.ToInt32(lines[5]), new Uri(lines[3]), lines[4], null);
                if (contentHolder != null) contentHolder.Items.Add(t);
                mTrackList.Add(t);
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
    }
}
