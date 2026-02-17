using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Borodar.RainbowFolders.RFLogger;

namespace Borodar.RainbowFolders
{
    public static class ProjectRulesetExporter
    {
        private const string EXTENSION = "rfset";

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public static void Import()
        {
            var filePath = EditorUtility.OpenFilePanel("Import Ruleset", "", EXTENSION);
            if (string.IsNullOrEmpty(filePath)) return;

            ImportCustomIcons(filePath);
            ImportRuleset(filePath);
        }

        public static void Export()
        {
            var dirPath = EditorUtility.SaveFolderPanel("Export Ruleset", "", "");
            if (string.IsNullOrEmpty(dirPath)) return;

            var projectName = ProjectEditorUtility.ProjectName;
            var rulesetName = Path.GetFileNameWithoutExtension(ProjectPreferences.RulesetPath);
            var fileName = $"{projectName}.{rulesetName}";

            ExportRuleset(dirPath, fileName);
            ExportCustomIcons(dirPath, fileName);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static void ImportRuleset(string path)
        {
            var wrapperJson = File.ReadAllText(path);
            var wrapper = new ProjectRulesetWrapper();
            EditorJsonUtility.FromJsonOverwrite(wrapperJson, wrapper);

            EditorJsonUtility.FromJsonOverwrite(wrapper.RulesetJson, ProjectRuleset.Instance);
            ProjectRuleset.Instance.SaveSetting();

            Log("Ruleset successfully imported.");
        }

        private static void ImportCustomIcons(string path)
        {
            var importDir = Path.GetDirectoryName(path);
            var importFileName = Path.GetFileNameWithoutExtension(path);
            var packagePath = $"{importDir}/{importFileName}.unitypackage";

            if (!File.Exists(packagePath))
            {
                Log("There is no package with custom icons. Skipping package import.");
                return;
            }

            AssetDatabase.ImportPackage(packagePath, true);
        }

        private static void ExportRuleset(string dirPath, string fileName)
        {
            var rulesetJson = EditorJsonUtility.ToJson(ProjectRuleset.Instance);
            var wrapper = new ProjectRulesetWrapper { RulesetJson = rulesetJson };
            var wrapperJson = EditorJsonUtility.ToJson(wrapper, true);

            var filePath = $"{dirPath}/{fileName}.{EXTENSION}";
            File.WriteAllText(filePath, wrapperJson);

            Log("Ruleset successfully exported.");
        }

        private static void ExportCustomIcons(string dirPath, string fileName)
        {
            var rulesetPath = AssetDatabase.GetAssetPath(ProjectRuleset.Instance);
            var allDependencies = AssetDatabase.GetDependencies(rulesetPath);

            var textureDependencies = (from dependency in allDependencies
                let dependencyType = AssetDatabase.GetMainAssetTypeAtPath(dependency)
                where dependencyType == typeof(Texture2D)
                select dependency)
                .ToArray();

            if (!textureDependencies.Any())
            {
                Log("There is no custom icons in ruleset. Skipping package export.");
                return;
            }

            var packagePath = $"{dirPath}/{fileName}.unitypackage";
            AssetDatabase.ExportPackage(textureDependencies, packagePath);

            Log("Package with custom icons successfully exported.");
        }
    }
}