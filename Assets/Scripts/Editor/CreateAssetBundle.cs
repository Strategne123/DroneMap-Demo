using System;
using UnityEditor;
using UnityEngine;

public class CreateAssetBundle
{
    [MenuItem("Assets/Create Asset Bundles")]
    private static void BuildAllAssetBundles()
    {
        // Указываем директорию для ассет-бандлов
        string assetBundleDirectoryPath = Application.dataPath + "/DroneMaps";
        if (!System.IO.Directory.Exists(assetBundleDirectoryPath))
        {
            System.IO.Directory.CreateDirectory(assetBundleDirectoryPath);
        }

        try
        {
            // Указываем опции для сборки ассет-бандлов
            BuildPipeline.BuildAssetBundles(
                assetBundleDirectoryPath,
                BuildAssetBundleOptions.ChunkBasedCompression | // Оптимальное сжатие
                BuildAssetBundleOptions.ForceRebuildAssetBundle, // Принудительная пересборка
                EditorUserBuildSettings.activeBuildTarget
            );

            Debug.Log("Asset Bundles successfully built!");

            // Формируем путь к целевой папке на уровне выше Assets
            string projectRootPath = System.IO.Directory.GetParent(Application.dataPath).FullName;
            string targetDirectoryPath = System.IO.Path.Combine(projectRootPath, "Builds", "Drone_Data", "DroneMaps");

            // Создаем целевую папку, если она не существует
            if (!System.IO.Directory.Exists(targetDirectoryPath))
            {
                System.IO.Directory.CreateDirectory(targetDirectoryPath);
            }

            // Копируем все файлы из исходной папки в целевую папку
            foreach (string filePath in System.IO.Directory.GetFiles(assetBundleDirectoryPath))
            {
                string fileName = System.IO.Path.GetFileName(filePath);
                string destinationPath = System.IO.Path.Combine(targetDirectoryPath, fileName);

                // Копируем файл с заменой
                System.IO.File.Copy(filePath, destinationPath, true);
            }

            Debug.Log($"Asset Bundles copied to {targetDirectoryPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error building or copying Asset Bundles: {e.Message}");
        }
    }
}
