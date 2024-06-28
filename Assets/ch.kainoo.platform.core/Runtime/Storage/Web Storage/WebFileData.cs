using ch.kainoo.core;
using ch.kainoo.core.utilities.extensions;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ch.kainoo.platform.storage
{

    public class WebFileData : IFileData
    {
        public async Task<string> Read(string uri, ProgressCallback progressCallback = null)
        {
            using UnityWebRequest uwr = UnityWebRequest.Get(uri);

            var asyncOp = uwr.SendWebRequest();

            while (asyncOp.isDone)
            {
                progressCallback?.Invoke(asyncOp.progress);
                await UniTask.Yield();
            }

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                DebugEO.LogError($"Error processing HTTP Request to {uwr.url}");
                return null;
            }
            var handler = uwr.downloadHandler;
            if (handler == null)
            {
                Debug.LogError("Download handler is null");
                return null;
            }

            return handler.text;
        }

        public async Task<bool> Write(string uri, string data)
        {
            await Task.Yield();
            throw new System.NotImplementedException();
        }
    }

}