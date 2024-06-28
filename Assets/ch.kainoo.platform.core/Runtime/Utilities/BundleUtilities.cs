using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ch.kainoo.platform
{

    public static class BundleUtilities
    {
        public static (string, string) SplitBundleName(string bundleName)
        {
            ReadOnlySpan<char> bundleSpan = bundleName;
            int pos = bundleSpan.IndexOf(".");
            if (pos >= 0)
            {
                var part1 = bundleSpan.Slice(0, pos + 1);
                var part2 = bundleSpan.Slice(pos + 1);
                return (part1.ToString(), part2.ToString());
            }
            else
            {
                return (bundleName, "");
            }
        }
    }

}