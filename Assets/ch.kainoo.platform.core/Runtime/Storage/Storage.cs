using ch.kainoo.platform;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace ch.kainoo.platform.storage
{

    [System.Serializable]
    public class StorageProviderFor {
        public string Situation;
        public IStorageProvider StorageProvider;
    }

    public class Storage : MonoBehaviour
    {
        [SerializeField]
        private StorageProviderFor[] storageProviders;

        public async Task<string> Read(string uri)
        {
            return await Read("", uri);
        }

        public async Task<string> Read(string situation, string uri)
        {
            var storageProvider = GetStorageProviderFor(situation);
            return await storageProvider.Read(uri);
        }

        public async Task<bool> Write(string uri, string data)
        {
            return await Write("", uri, data);
        }

        public async Task<bool> Write(string situation, string uri, string data)
        {
            var storageProvider = GetStorageProviderFor(situation);
            return await storageProvider.Write(uri, data);
        }

        private IStorageProvider GetStorageProviderFor(string situation)
        {
            foreach (var s in storageProviders)
            {
                if (s.Situation == situation)
                {
                    return s.StorageProvider;
                }
            }

            return null;
        }

    }

}