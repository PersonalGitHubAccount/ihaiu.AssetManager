﻿using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace com.ihaiu
{
    public class AssetBundleEditor 
    {
        public static string resourceRoot = AssetManagerSetting.EditorRoot.MResources;
        static string[] resourcesPaths = new string[]{
            resourceRoot,
    //        "Assets/Ihaiu/AssetManagerExampleFiles/Component"

        };


        static string assetbundleExt { get { return AssetManagerSetting.AssetbundleExt; } }

        static string[] filterDirList = new string[]{};
        static List<string> filterExts = new List<string>{".cs", ".js"};
        static List<string> imageExts = new List<string>{".png", ".jpg", ".jpeg", ".bmp", "gif", ".tga", ".tiff", ".psd"};
        static bool isSpriteTag = true;

        public static List<string> exts = new List<string>(new string[]{ ".prefab", ".png", ".jpg", ".jpeg", ".bmp", "gif", ".tga", ".tiff", ".psd", ".mat", ".mp3", ".wav" , ".shader", ".ttf"});


        public static void ClearAssetBundleNames()
        {
            string[] names = AssetDatabase.GetAllAssetBundleNames();

            int count = names.Length;
            for(int i = 0; i < count; i ++)
            {
                if (names[i].IndexOf(assetbundleExt) != -1)
                {
                    string[] assets = AssetDatabase.GetAssetPathsFromAssetBundle(names[i]);
                    for (int j = 0; j < assets.Length; j++)
                    {
                        AssetImporter importer = AssetImporter.GetAtPath(assets[j]);
                        importer.assetBundleName = null;
                    }
                }
            }
        }


        public static void SetNames()
        {
            List<string> list = new List<string>();
            PathUtil.RecursiveFile(resourcesPaths, list, exts);

            if (list.Count == 0)
                return;


            // 生成所有节点
            Dictionary<string, AssetNode> nodeDict = AssetNodeUtil.GenerateAllNode(list, filterDirList, filterExts, imageExts, isSpriteTag, assetbundleExt);

            // 生成每个节点依赖的节点
            AssetNodeUtil.GenerateNodeDependencies(nodeDict);


            // 寻找入度为0的节点
            List<AssetNode> roots = AssetNodeUtil.FindRoots(nodeDict);

            // 移除父节点的依赖和自己依赖相同的节点
            AssetNodeUtil.RemoveParentShare(roots);


            // 强制设置某些节点为Root节点，删掉被依赖
            AssetNodeUtil.ForcedSetRoots(nodeDict, list, imageExts);


            // 寻找入度为0的节点
            roots = AssetNodeUtil.FindRoots(nodeDict);

            // 入度为1的节点自动打包到上一级节点
            AssetNodeUtil.MergeParentCountOnce(roots);

            // 生成需要设置AssetBundleName的节点
            Dictionary<string, AssetNode> assetDict = AssetNodeUtil.GenerateAssetBundleNodes(roots);


            // 设置AssetBundleNames
            AssetNodeUtil.SetAssetBundleNames(assetDict, resourceRoot, assetbundleExt);


            AssetDatabase.RemoveUnusedAssetBundleNames();
        }


        public static void SetNames_Develop()
        {
            List<string> list = new List<string>();
            PathUtil.RecursiveFile(resourceRoot, list, exts);

            int count = list.Count;
            for(int i =0; i < count; i ++ )
            {
                string path = list[i];
                AssetImporter assetImporter = AssetImporter.GetAtPath(path);
                if (!string.IsNullOrEmpty(assetImporter.assetBundleName))
                {
                    if (assetImporter.assetBundleName.IndexOf(assetbundleExt) == -1)
                    {
                        continue;
                    }
                }


                string assetBundleName =  path.Replace(resourceRoot + "/", "").ToLower();
                assetBundleName = PathUtil.ChangeExtension(assetBundleName, assetbundleExt);
                assetImporter.assetBundleName  = assetBundleName;

            }
        }

        public static void BuildAssetBundles()
        {
//            string outputPath = AssetManagerSetting.EditorRoot.StreamPlatform;
            string outputPath = AssetManagerSetting.EditorRoot.WorkspaceStreamPlatform;
            if (outputPath.EndsWith("/"))
            {
                outputPath = outputPath.Substring(0, outputPath.Length - 1);
            }
            PathUtil.CheckPath(outputPath, false);
            Debug.Log("outputPath=" + outputPath);

			BuildPipeline.BuildAssetBundles (outputPath, BuildAssetBundleOptions.DeterministicAssetBundle, EditorUserBuildSettings.activeBuildTarget);

            AssetDatabase.Refresh();
        }

        public static void ClearManifestHelpFile()
        {
            string outputPath = AssetManagerSetting.EditorRoot.StreamPlatform;

            List<string> fileList = new List<string>();
            PathUtil.RecursiveFile(outputPath, fileList, new List<string>(new string[]{".manifest"}));

            for(int i = 0; i < fileList.Count; i ++)
            {
                PathUtil.DeleteFile(fileList[i]);
            }

            EditorUtility.ClearProgressBar();
        }

       
    }
}