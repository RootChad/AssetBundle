using ch.kainoo.platform.storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ch.kainoo.platform
{
    public class PlayerPrefsStorage : IStorageProvider
    {
        [SerializeField]
        private string m_keyName = "config";

        private string BuildKey(string uri) => m_keyName + "_" + uri;

        public override Task<string> Read(string uri)
        {
            var data = PlayerPrefs.GetString(BuildKey(uri), "");
            return Task.FromResult(data);
        }

        public override Task<bool> Write(string uri, string data)
        {
            PlayerPrefs.SetString(BuildKey(uri), data);
            PlayerPrefs.Save();
            return Task.FromResult(true);
        }
    }
}