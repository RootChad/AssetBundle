using ch.kainoo.core;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ch.kainoo.platform.storage
{

    public interface IFileData
    {
        Task<string> Read(string uri, ProgressCallback progressCallback);
        Task<bool> Write(string uri, string data);
    }

}