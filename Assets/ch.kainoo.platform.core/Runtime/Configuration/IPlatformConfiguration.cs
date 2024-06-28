using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ch.kainoo.platform
{

    public abstract class IPlatformConfiguration : MonoBehaviour
    {
        public abstract Task<string> GetBundlePath(string bundleName);
        public abstract Task<BundleDefinition> GetBundleInfo(string bundleName);
        public abstract Task<TreeNode<BundleDefinition>> GetBundleInfoDependencyTree(string bundleName);

    }

}