using ch.kainoo.platform.storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ch.kainoo.platform
{
    public class StreamingAssetsStorage : IStorageProvider
    {
        [SerializeField]
        private string m_relativePath = "";

        private WebClient _webClient = new WebClient();

        public override Task<string> Read(string uri)
        {
            var path = System.IO.Path.Combine(Application.streamingAssetsPath, m_relativePath, uri);
            return _webClient.Get(path);
        }

        public override Task<bool> Write(string uri, string data)
        {
            throw new System.IO.IOException("Read-only storage provider");
        }
    }
}