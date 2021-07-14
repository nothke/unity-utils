///
/// AssetDatabaseUtils by Nothke
/// 
/// Provides a few useful functions to help with AssetDatabase usage.
/// Make sure to not use any of the methods at runtime, as these are editor only methods
/// (surround with #if UNITY_EDITOR if using inside runtime scripts).
/// 
/// ============================================================================
///
/// MIT License
///
/// Copyright(c) 2021 Ivan Notaroš
/// 
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all
/// copies or substantial portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
/// SOFTWARE.
/// 
/// ============================================================================
///

#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Nothke.Utils
{
    public static class AssetDatabaseUtils
    {
        public static bool AssetExists(in string path)
        {
            return AssetDatabase.LoadAssetAtPath<Object>(path) != null;
        }

        public static bool AssetExists<T>(in string path) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path) != null;
        }

        /// <summary>
        /// Creates asset while making a folder hierarchy in the process.
        /// </summary>
        public static void CreateAsset(Object o, in string path)
        {
            string folderPath = System.IO.Path.GetDirectoryName(path);
            Debug.Log(folderPath);

            if (!AssetDatabase.IsValidFolder(folderPath))
                CreateFolder(folderPath);

            AssetDatabase.CreateAsset(o, path);
        }

        /// <summary>
        /// Creates folder at path, creating any parent folders in the process.
        /// Returns true if successful. If folder already exists it will still return true.
        /// </summary>
        public static bool CreateFolder(in string path)
        {
            var dir = new System.IO.DirectoryInfo(path);
            Debug.Log("DIR:" + dir.ToString());

            var strs = dir.ToString().Split('\\');

            if (strs[0] != "Assets")
            {
                Debug.LogError("Path needs to start with 'Assets\\'");
                return false;
            }

            string curFolderPath = strs[0];
            for (int i = 1; i < strs.Length; i++)
            {
                if (!AssetDatabase.IsValidFolder(curFolderPath + "\\" + strs[i]))
                    AssetDatabase.CreateFolder(curFolderPath, strs[i]);

                curFolderPath += "\\" + strs[i];
            }

            return true;
        }
    }
}

#endif