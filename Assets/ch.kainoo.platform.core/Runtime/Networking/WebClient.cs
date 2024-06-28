using ch.kainoo.core;
using ch.kainoo.core.utilities.extensions;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ch.kainoo.platform
{

    public class WebClient
    {
        public WebClient()
        {
        }

        public async Task<string> Get(string url, ProgressCallback callback = null)
        {
            using var uwr = UnityWebRequest.Get(url);

            var asyncOp = uwr.SendWebRequest();
            while (!asyncOp.isDone)
            {
                callback?.Invoke(asyncOp.progress);
                await UniTask.Yield();
            }
            callback?.Invoke(1.0f);

            return uwr.downloadHandler.text;
        }

        public async Task<T> GetJson<T>(string url, ProgressCallback callback = null) where T : class
        {
            return await Get(url, callback).AsJson<T>();
        }

        public async Task<Texture2D> GetImage(string url, ProgressCallback callback = null)
        {
            using var uwr = UnityWebRequestTexture.GetTexture(url);

            var asyncOp = uwr.SendWebRequest();
            while (!asyncOp.isDone)
            {
                callback?.Invoke(asyncOp.progress);
                await UniTask.Yield();
            }
            callback?.Invoke(1.0f);

            if (uwr.result != UnityWebRequest.Result.Success || uwr.responseCode != 200)
            {
                return null;
            }

            var downloadHandler = uwr.downloadHandler as DownloadHandlerTexture;
            return downloadHandler.texture;
        }

        public Task<U> PostJson<T, U>(string url, T body) where T : class where U : class
        {
            throw new System.NotImplementedException();
        }

    }

}