using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ch.kainoo.platform.storage
{

    public class WebStorageProvider : IStorageProvider
    {
        public override async Task<string> Read(string uri)
        {
            var wfd = new WebFileData();
            return await wfd.Read(uri);
        }

        public override async Task<bool> Write(string uri, string data)
        {
            var wfd = new WebFileData();
            return await wfd.Write(uri, data);
        }
    }

}