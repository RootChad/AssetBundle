using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ch.kainoo.platform.storage
{

    public abstract class IStorageProvider : MonoBehaviour
    {
        public abstract Task<string> Read(string uri);
        public abstract Task<bool> Write(string uri, string data);
    }

}