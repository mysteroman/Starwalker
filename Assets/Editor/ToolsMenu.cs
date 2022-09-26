using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TopDownShooter.Editor
{
    public static class ToolsMenu
    {
        [MenuItem("Tools/Setup/Create Default Folders")]
        public static void CreateDefaultFolders()
        {
            var directories = new[]
            {
                "Art",
                "Art/Particles",
                "Animations",
                "Animations/Player",
                "Animations" +
                "Particles",
                "Prefabs",
                "Resources",
                "_Scripts",
                "_Scripts/Player",
                "_Scripts/Managers",
                "Scenes",
                "Systems",
                "Systems/Input"
            };

            CreateDirectories("_Project", directories);
            AssetDatabase.Refresh();
        }

        private static void CreateDirectories(string root, string[] directories)
        {
            var fullPath = Path.Combine(Application.dataPath, root);
            foreach (var d in directories)
            {
                var path = Path.Combine(fullPath, d);
                if (Directory.Exists(path)) continue;
                Directory.CreateDirectory(path);
            }
        }
    }
}
