using UnityEditor;

namespace VYaml.Editor
{
    class VYamlAssetPostprocessor : AssetPostprocessor
    {
        const string SourceGeneratorDll =
#if UNITY_2022_2_OR_NEWER
    "VYaml.SourceGenerator.dll";
#else
    "VYaml.SourceGenerator.Roslyn3.dll";
#endif

        void OnPreprocessAsset()
        {
            if (assetPath.EndsWith(SourceGeneratorDll))
            {
                var plugin = AssetImporter.GetAtPath(assetPath) as PluginImporter;
                if (plugin == null)
                {
                    UnityEngine.Debug.LogWarning($"Failed to import plug-in at {assetPath}");
                }

                AssetDatabase.SetLabels(plugin, new[] { "RoslynAnalyzer" });
            }
        }
    }
}
