using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ch.kainoo.platform
{

    [CreateAssetMenu(
        fileName = "export_information",
        menuName = "Kainoo/Configurator/Export Information",
        order = 1
        )]
    public class ExportInformation : ScriptableObject
    {
        public string DisplayName;
        [TextArea]
        public string Description;
        public AssetReference<Texture2D> Thumbnail;
        public StringKVP[] AdditionalProperties;

        public SerializableExportInformation ToSerialized()
        {
            return (SerializableExportInformation)this;
        }
    }

    [Serializable]
    public class SerializableExportInformation {
        public string DisplayName;
        public string Description;
        public string Image;
        public StringKVP[] AdditionalProperties;

        public static explicit operator SerializableExportInformation(ExportInformation info)
        {
            return new SerializableExportInformation()
            {
                DisplayName = info.DisplayName,
                Description = info.Description,
                Image = info.Thumbnail.AssetPath,
                AdditionalProperties = info.AdditionalProperties
            };
        }
    }

    [Serializable]
    public class StringKVP
    {
        public string Key;
        public string Value;
    }

}