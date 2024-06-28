using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ch.kainoo.platform
{

    [System.Serializable]
    public class BundleDefinition : IEquatable<BundleDefinition>
    {
        public string BundleName;
        public BundleVariant[] Variants;

        public IEnumerable<string> Dependencies
        {
            get
            {
                return Variants.SelectMany(v => v.Dependencies);
            }
        }

        public override bool Equals(object obj)
        {
            // If the passed object is null, return False
            if (obj == null)
            {
                return false;
            }

            // If the passed object is not Customer Type, return False
            if (!(obj is BundleDefinition))
            {
                return false;
            }

            return BundleName.Equals(((BundleDefinition)obj).BundleName);
        }

        public override int GetHashCode()
        {
            return BundleName.GetHashCode();
        }

        public bool Equals(BundleDefinition other)
        {
            return BundleName == other.BundleName;
        }
    }

    [System.Serializable]
    public class BundleVariant
    {
        public string VariantName;
        public long Size;
        public string[] Dependencies;

        public static BundleVariant FromName(string bundleName, long size, string[] deps)
        {
            if (bundleName.Contains('.'))
            {
                string[] parts = bundleName.Split('.');
                string bundleIdentifier = parts[0];
                string variantName = parts[1];

                return FromVariantName(variantName, size, deps);
            }
            else
            {
                return FromVariantName("", size, deps);
            }
        }

        public static BundleVariant FromVariantName(string variantName, long size, string[] deps)
        {
            return new BundleVariant()
            {
                VariantName = variantName,
                Size = size,
                Dependencies = deps
            };
        }
    }

}