using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Windows.Storage;

namespace MediaPlayer._Data
{
    class DatabaseHelper
    {
        public async static Task addTrackToDatabase(Track track)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile storageFile = null;
            
            storageFile = await storageFolder.CreateFileAsync(""+track.trackID);

            DataContractSerializer serializer = new DataContractSerializer(typeof(Track));
            using (Stream fileStream = await storageFile.OpenStreamForWriteAsync())
            {
                serializer.WriteObject(fileStream, track);
                fileStream.Flush();
            }
        }

        public static async Task<Track> getTrackFromDatabase(String lastFmLink)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile storageFile = null;
            
            Track track = new Track();
            track.LastFMLink = lastFmLink;

            String fileName = "" + track.setTrackID();
            //we're looking to cached items
            try
            {
                storageFile = await storageFolder.GetFileAsync(fileName);

                DataContractSerializer serializer = new DataContractSerializer(typeof(Track));
                //we know that the file is in the database so we parse it
                using (Stream fileStream = await storageFile.OpenStreamForWriteAsync())
                {
                    track = (Track)serializer.ReadObject(fileStream);

                    return track;
                }
            }
            catch (Exception er)
            {
                return null;
            }

            return null;
        }
    }
}
