using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ch.kainoo.core.serialization
{

    public static class ColorExtensions
    {
        public static SerializableColor ToSerializable(this Color color)
        {
            return SerializableColor.ToSerializable(color);
        }
    }

    [System.Serializable]
    public class SerializableColor
    {
        public float r, g, b, a;

        public SerializableColor(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public static Color FromSerializable(SerializableColor color)
        {
            return new Color(
                color.r,
                color.g,
                color.b,
                color.a
            );
        }

        public static SerializableColor ToSerializable(Color color)
        {
            return new SerializableColor(
                color.r,
                color.g,
                color.b,
                color.a
            );
        }

    }

}