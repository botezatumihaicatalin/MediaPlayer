using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
namespace MediaPlayer 
{ 
    class Preferences 
    { 
        private static List<String> tags = new List<String>();
       
        public async static void readTagsFromFile()
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile storageFile = null;
            bool found = false;

            try
            {
                storageFile = await storageFolder.GetFileAsync("tags.txt");
                found = true;
            }
            catch(Exception er)
            {
                found = false;
            }
            if (!found)
            {
                storageFile = await storageFolder.CreateFileAsync("tags.txt");
                await FileIO.WriteTextAsync(storageFile, "Music");
            }
            var readFile = await FileIO.ReadLinesAsync(storageFile);            
            tags = readFile.ToList();

        }

        public static List<String> getTags() 
        { 
            return tags; 
        } 
        
        public async static void addTag(String tag) 
        { 
            if (!tags.Contains(tag)) 
            {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile storageFile = null;
                bool found = false;

                try
                {
                    storageFile = await storageFolder.GetFileAsync("tags.txt");
                    found = true;
                }
                catch (Exception er)
                {
                    found = false;
                }
                await FileIO.WriteLinesAsync(storageFile,tags.AsEnumerable());
                tags.Add(tag); 
            } 
        } 
    } 
}