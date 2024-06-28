using ch.kainoo.platform.storage;
using ch.kainoo.platform.webgl;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace ch.kainoo.platform
{
    public class LocalStorageProvider : IStorageProvider
    {

        private string ComposePath(string uri)
        {
            string folder = Application.persistentDataPath;
#if !UNITY_WEBGL || UNITY_EDITOR
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
#endif
#if UNITY_WEBGL && !UNITY_WEBGL
            return folder + "/uri.json";            
#else
            return Path.Combine(folder, uri);
#endif
        }

        private async Task<string> ReadText(string path)
        {
            using (var file = File.Open(path, FileMode.Open))
            {
                var reader = new StreamReader(file);
                var text = await reader.ReadToEndAsync();
                return text;
            }
        }

        private async Task WriteText(string path, string text)
        {
            using (var file = File.Open(path, FileMode.OpenOrCreate))
            {
                StreamWriter writer = new StreamWriter(file);
                await writer.WriteAsync(text);
            }
        }

        public override async Task<string> Read(string uri)
        {
            string path = ComposePath(uri);
            Debug.Log($"Reading config from '{path}'...");
            string text = await ReadText(path);
            Debug.Log("Got data:\n" + text);
            return text;
            //return await File.ReadAllTextAsync(path);
        }

        public override async Task<bool> Write(string uri, string data)
        {
            string path = ComposePath(uri);
            Debug.Log($"Writing config to '{path}'...");
            Debug.Log($"Data is:\n" + data);
            await WriteText(path, data);
            Debug.Log("... finished!");
            HandleIO.SyncFiles();
            Debug.Log("Synced");
            return true;
        }

    }

}