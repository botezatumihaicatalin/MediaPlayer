using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
namespace MediaPlayer 
{ 
    class Preferences 
    {   
        private async static Task<List<Pair<String, int>>> readTagsFromFile()
        {
            List<Pair<String, int>> list = new List<Pair<string, int>>();
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFolder tagsFolder = null;
            bool isCreated = false;
            try
            {
                tagsFolder = await storageFolder.GetFolderAsync("Tags");
                isCreated = true;
            }
            catch (Exception er)
            {
            }

            if (!isCreated)
            {
                tagsFolder = await storageFolder.CreateFolderAsync("Tags");
                StorageFile file = await tagsFolder.CreateFileAsync("Music");
                await FileIO.WriteTextAsync(file, "1");
            }


            var read = await tagsFolder.GetFilesAsync();
            for (int i = 0; i < read.Count; i++)
            {
                int contents = Convert.ToInt32(await FileIO.ReadTextAsync(read[i]));
                list.Add(new Pair<String, int>(read[i].Name, contents));
            }
            return list;

        }

        public async static Task<List<String>> getTopTags()
        {
            List<Pair<String, int>> list = await readTagsFromFile();
            int max1 = -1, max2 = -1, max3 = -1;
            string tag1 = null, tag2 = null, tag3 = null;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Second > max1)
                {
                    max3 = max2;
                    tag3 = tag2;
                    max2 = max1;
                    tag2 = tag1;
                    max1 = list[i].Second;
                    tag1 = list[i].First;
                }
                else if (list[i].Second > max2)
                {
                    max3 = max2;
                    tag3 = tag2;
                    max2 = list[i].Second;
                    tag2 = list[i].First;
                }
                else if (list[i].Second > max3)
                {
                    max3 = list[i].Second;
                    tag3 = list[i].First;
                }
            }

            List<string> TOPTAGS = new List<string>();
            if (max1 != -1)
                TOPTAGS.Add(tag1);
            if (max2 != -1)
                TOPTAGS.Add(tag2);
            if (max3 != -1)
                TOPTAGS.Add(tag3);

            return TOPTAGS; 

        }

        public async static void addTag(string tag)
        {
            tag = tag.Replace("\\", "");
            tag = tag.Replace("/", "");
            tag = tag.Replace(":", "");
            tag = tag.Replace("\"", "");
            tag = tag.Replace("?", "");
            tag = tag.Replace("<", "");
            tag = tag.Replace(">", "");
            tag = tag.Replace("|", "");
            tag = tag.Replace("*", "");
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFolder tagsFolder = await storageFolder.GetFolderAsync("Tags");
            StorageFile storageFile = null;
            bool isFound = false;
            try
            {
                storageFile = await tagsFolder.GetFileAsync(tag);
                string data = await FileIO.ReadTextAsync(storageFile);
                await FileIO.WriteTextAsync(storageFile, (Convert.ToInt32(data) + 1).ToString());
                isFound = true;
            }
            catch (Exception er)
            {

            }

            if (!isFound)
            {
                storageFile = await tagsFolder.CreateFileAsync(tag);
                await FileIO.WriteTextAsync(storageFile, "1");
            }
        }       
    } 
}