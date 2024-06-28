using ch.kainoo.core.utilities.async;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace ch.kainoo.platform
{

    public class StreamingAssetsConfiguration : IPlatformConfiguration
    {
        private static string BASE_URL => Path.Combine(Application.streamingAssetsPath, "assets");

        private WebClient webClient = new WebClient();

        public override Task<string> GetBundlePath(string bundleName)
        {
            return Task.FromResult(Path.Combine(BASE_URL, bundleName));
        }

        public async override Task<BundleDefinition> GetBundleInfo(string bundleName)
        {
            var bundlePath = await GetBundlePath(bundleName) + ".json";
            var bundleDef = await webClient.GetJson<BundleDefinition>(bundlePath);
            return bundleDef;
        }

        public async override Task<TreeNode<BundleDefinition>> GetBundleInfoDependencyTree(string bundleName)
        {
            var baseBundleInfo = await GetBundleInfo(bundleName);
            if (baseBundleInfo == null) return null;

            var baseNode = new TreeNode<BundleDefinition>(baseBundleInfo);

            foreach (var depBundleName in baseBundleInfo.Dependencies)
            {
                var tree = await GetBundleInfoDependencyTree(depBundleName);
                if (tree != null)
                {
                    baseNode.AddChild(tree);
                }
            }

            return baseNode;
        }
    }

}