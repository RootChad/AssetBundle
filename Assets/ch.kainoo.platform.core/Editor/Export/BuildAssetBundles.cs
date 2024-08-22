using ch.kainoo.core.editor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ch.kainoo.platform.editor
{
    [InitializeOnLoad]
    public static class BuildAssetBundlesAutoInvoke
    {
        static BuildAssetBundlesAutoInvoke()
        {
            //BuildPlayerWindow.RegisterBuildPlayerHandler((options) =>
            //        {
            //            Debug.Log("Building asset bundles before build...");
            //            BuildAssetBundles.BuildForCurrentEditorPlatform();
            //            Debug.Log("Starting build...");
            //            BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
            //        });
        }
    }

    public static class BuildAssetBundles
    {
        /// <summary>
        /// Splits the bundle name on the first '.' character that appears. Returns 
        /// either one or two result groups depending on if the bundle is a variant.
        /// </summary>
        public static readonly Regex BUNDLE_NAME_EXTRACTOR = new Regex(@"([\w\/\\-_]+)(?:\.?([\w\/\\-_]+))?");


        [MenuItem("Kainoo/Asset Bundles/Build -> StreamingAssets (Current platform)", priority = 0)]
        public static void BuildForCurrentEditorPlatform()
        {
            var currentPlatform = EditorUserBuildSettings.activeBuildTarget;
            if (currentPlatform != BuildTarget.StandaloneWindows64
                && currentPlatform != BuildTarget.WebGL
                && currentPlatform != BuildTarget.Android
                && currentPlatform != BuildTarget.iOS)
            {
                EditorUtility.DisplayDialog("Erreur", $"Unsupported platform: {currentPlatform}", "OK");
                return;
            }

            BuildToStreamingAssets(currentPlatform);
        }

        [MenuItem("Kainoo/Asset Bundles/Build -> StreamingAssets (Win64)", priority = 1)]
        public static void BuildToStreamingAssetsWin64()
        {
            BuildToStreamingAssets(BuildTarget.StandaloneWindows64);
        }

        [MenuItem("Kainoo/Asset Bundles/Build -> StreamingAssets (WebGL)", priority = 1)]
        public static void BuildToStreamingAssetsWebGL()
        {
            BuildToStreamingAssets(BuildTarget.WebGL);
        }

        [MenuItem("Kainoo/Asset Bundles/Build -> StreamingAssets (Android)", priority = 1)]
        public static void BuildToStreamingAssetsAndroid()
        {
            BuildToStreamingAssets(BuildTarget.Android);
        }



        public static void BuildToStreamingAssets(BuildTarget platform)
        {
            AssetDatabase.Refresh();

            try
            {
                AssetDatabase.StartAssetEditing();

                var allBundles = AssetDatabase.GetAllAssetBundleNames();
                if (allBundles.Any(b => b.Contains('.')))
                {
                    if (!EditorUtility.DisplayDialog("Variants in use", "There are asset bundle variants in use in this project. The platform system does not work with them currently. Would you like to continue?", "Yes", "No"))
                    {
                        return;
                    }
                }


                // Build to Assets/AssetBundles/{Platform}/...
                string buildPath = Path.Combine(Application.dataPath, "..", "AssetBundles", platform.ToString());
                if (Directory.Exists(buildPath))
                {
                    Directory.Delete(buildPath, true);
                }
                Directory.CreateDirectory(buildPath);


                // Build asset bundles
                CompatibilityBuildPipeline.BuildAssetBundles(buildPath, BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle, platform);

                // Get all bundles and variant definitions
                var entries = GetAllBundlesAndVariants();
                foreach (var entry in entries)
                {
                    var def = CreateBundleDefinitionFor(buildPath, entry);
                    {
                        var bundleDefPath = Path.Join(buildPath, entry.name + ".json");

                        using var writer = File.CreateText(bundleDefPath);
                        writer.Write(JsonUtility.ToJson(def));
                    }

                    var machineInfoAssetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(def.BundleName, "export_information");
                    if (machineInfoAssetPaths.Length > 0)
                    {
                        var machineInfoAssetPath = machineInfoAssetPaths[0];
                        if (machineInfoAssetPaths.Length > 1)
                        {
                            Debug.LogWarning($"More than one information file for bundle {def.BundleName}, taking the first one that was found {machineInfoAssetPath}");
                        }

                        var exportInformationObj = AssetDatabase.LoadAssetAtPath<ExportInformation>(machineInfoAssetPath);
                        var exportInformation = exportInformationObj.ToSerialized();

                        // Output image
                        var imgSrcPath = exportInformation.Image;
                        var imgDstPath = Path.Join(buildPath, def.BundleName + "_thumbnail" + Path.GetExtension(exportInformation.Image));

                        File.Copy(imgSrcPath, imgDstPath, true);

                        // Change image path
                        exportInformation.Image = def.BundleName + "_thumbnail.png";

                        var machineInfoPath = Path.Join(buildPath, entry.name + "_infos.json");
                        File.WriteAllText(machineInfoPath, JsonUtility.ToJson(exportInformation));
                    }

                    // Rename bundle to *.bundle
                    File.Move(Path.Join(buildPath, entry.name), Path.Join(buildPath, entry.name + ".bundle"));
                }


                // Prepare copy from build path to StreamingAssets/assets/...
                string outputPath = Path.Combine(Application.streamingAssetsPath, "assets");
                if (Directory.Exists(outputPath))
                {
                    Directory.Delete(outputPath, true);
                }
                Directory.CreateDirectory(outputPath);

                // Copy from folder to output
                FileEx.Copy(buildPath, outputPath);

            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                // Refresh, because otherwise Unity doesn't show the new files
                AssetDatabase.Refresh();
            }
        }

        private static (string, string) GetBundlePartsFromName(string bundleName)
        {
            var match = BUNDLE_NAME_EXTRACTOR.Match(bundleName);
            if (!match.Success)
            {
                return (bundleName, null);
            }

            if (match.Groups.Count > 2)
            {
                return (match.Groups[1].Value, match.Groups[2].Value);
            }
            else
            {
                return (match.Groups[1].Value, null);
            }
        }

        private static List<string> GetAllEntries(string startDirectory)
        {
            var result = new List<string>();

            foreach (var entry in Directory.GetFileSystemEntries(startDirectory))
            {
                // The entry is a directory
                if (Directory.Exists(entry))
                {
                    result.AddRange(GetAllEntries(entry));
                }
                // The entry is a file
                else if (File.Exists(entry))
                {
                    result.Add(entry);
                }
            }

            return result;
        }

        /// <summary>
        /// Build a list of all bundles and collects all variants into a 
        /// single entry with the parameter "Variants".
        /// </summary>
        /// <returns></returns>
        private static List<InternalBundle> GetAllBundlesAndVariants()
        {
            var bundlesList = AssetDatabase.GetAllAssetBundleNames();

            var output = new List<InternalBundle>();

            for (int i = 0; i < bundlesList.Length; i++)
            {
                var bundleName = bundlesList[i];

                if (bundleName.Contains('.'))
                {
                    var match = BUNDLE_NAME_EXTRACTOR.Match(bundleName);

                    var bundleIdentifier = match.Groups[1].Value;
                    var variants = new List<string>();
                    variants.Add(match.Groups[2].Value);

                    int j = i + 1;
                    for (; j < bundlesList.Length; j++)
                    {
                        var bundleName2 = bundlesList[j];
                        if (!bundleName2.StartsWith(bundleIdentifier))
                        {
                            break;
                        }

                        var match2 = BUNDLE_NAME_EXTRACTOR.Match(bundleName2);
                        variants.Add(match2.Groups[2].Value);
                    }
                    i = j - 1;

                    output.Add(new InternalBundle()
                    {
                        name = bundleIdentifier,
                        variants = variants.ToArray()
                    });
                }
                else
                {
                    output.Add(new InternalBundle()
                    {
                        name = bundleName
                    });
                }
            }

            return output;
        }

        private static BundleDefinition CreateBundleDefinitionFor(string basePath, InternalBundle bundle)
        {
            BundleDefinition definition = new BundleDefinition();
            definition.BundleName = bundle.name;
            if (bundle.variants.Length > 0)
            {
                definition.Variants = bundle.variants.Select(v => new BundleVariant()
                {
                    VariantName = v,
                    Size = FileEx.GetFileSize(Path.Join(basePath, $"{bundle.name}.{v}")),
                    Dependencies = GetDependencies($"{bundle.name}.{v}")
                }).ToArray();
            }
            else
            {
                definition.Variants = new[] {
                new BundleVariant()
                {
                    VariantName = "",
                    Size = FileEx.GetFileSize(Path.Join(basePath, bundle.name)),
                    Dependencies = GetDependencies(bundle.name)
                }
            };
            }
            return definition;
        }

        private static BundleDefinition CreateBundleDefinitionFor(string outputPath, string filePath)
        {
            var bundleName = Path.GetRelativePath(outputPath, filePath);
            bundleName = bundleName.Replace('\\', '/');

            var bundleDef = new BundleDefinition();

            bool isVariant = bundleName.Contains('.');
            if (isVariant)
            {
                var match = BUNDLE_NAME_EXTRACTOR.Match(bundleName);
                if (!match.Success)
                {
                    throw new System.Exception("Unexpected exception");
                }

                var bundleIdentifier = match.Groups[1];
                var variantIdentifier = match.Groups[2];

                Debug.Log($"We have a variant with: {bundleIdentifier} and {variantIdentifier}");
            }
            else
            {
                long fileSize = new FileInfo(filePath).Length;

                bundleDef.BundleName = bundleName;
                bundleDef.Variants = new[]
                {
                new BundleVariant()
                {
                    VariantName = "",
                    Size = fileSize
                }
            };
                //bundleDef.Dependencies = GetDependencies(bundleName);
            }

            return bundleDef;
        }

        private static string[] GetDependencies(string bundleName)
        {
            return AssetDatabase.GetAssetBundleDependencies(bundleName, false);
        }

        //private static string[] GetDependencies(string bundleName)
        //{
        //    IEnumerable<InternalBundle> bundles = GetAllBundlesAndVariants();
        //    if (!bundles.Any(b => b.name == bundleName))
        //    {
        //        return new string[0];
        //    }

        //    var bundle = bundles.FirstOrDefault(b => b.name == bundleName);
        //    if (bundle.variants.Length == 0)
        //    {
        //        return AssetDatabase.GetAssetBundleDependencies(bundleName, false);
        //    }
        //    else
        //    {
        //        var results = new List<string>();
        //        foreach (var variant in bundle.variants)
        //        {
        //            var compositeName = $"{bundle.name}.{variant}";
        //            var deps = GetDependencies(compositeName);
        //            results.AddRange(deps);
        //        }
        //        return results.ToArray();
        //    }
        //}

        private class InternalBundle
        {
            public string name;
            public string[] variants = new string[0];
        }

    }
}