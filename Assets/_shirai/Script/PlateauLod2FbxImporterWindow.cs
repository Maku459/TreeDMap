using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public sealed class PlateauLod2FbxImporterWindow : EditorWindow
{
    [MenuItem("Window/PLATEAU LOD2 FBX Importer")]
    public static void ShowWindow()
    {
        var window = GetWindow<PlateauLod2FbxImporterWindow>();
        window.titleContent = new GUIContent("PLATEAU LOD2 FBX Importer");
        window.position = new Rect(10, 10, 300, 300);
    }

    public void OnGUI()
    {
        GUILayout.Label("LOD2のFBXを指定してください。");

        if (GUILayout.Button("FBX読み込み", GUILayout.Height(40)))
        {
            var path = EditorUtility.OpenFilePanel("Import FBX", "", "FBX");
            if (string.IsNullOrEmpty(path)) return;

            // FBXファイル名
            var fileName = Path.GetFileName(path);

            // コピー先の絶対パス
            var newPath = Path.Combine(Application.dataPath, fileName);

            // アセットインポート用相対パス
            var importPath = Path.Combine("Assets", fileName);

            // FBMディレクトリのパス
            var fbmDir = path.Replace(".fbx", ".fbm");

            // コピー先のFBMディレクトリ名
            var newFbmDirName = "";

            var t0 = Time.realtimeSinceStartup;
            try
            {
                // FBMディレクトリをコピー
                if (Directory.Exists(fbmDir))
                {
                    newFbmDirName = fileName.Replace(".fbx", ".fbm");
                    var newFbmDirPath = Path.Combine(Application.dataPath, newFbmDirName);
                    Directory.CreateDirectory(newFbmDirPath);

                    CopyFiles(fbmDir, newFbmDirName);
                }

                // FBXファイルをコピー
                File.Copy(path, newPath);

                // FBXファイルをインポート
                AssetDatabase.ImportAsset(importPath);

                AssetDatabase.StartAssetEditing();

                var fbxObject = AssetDatabase.LoadMainAssetAtPath(importPath) as GameObject;

                // 1Prefab辺りのGameObject数
                var objectPerPrefab = 20;

                // 作成するPrefab数
                var prefabCount = fbxObject.transform.childCount / objectPerPrefab;
                if (fbxObject.transform.childCount % objectPerPrefab != 0)
                    prefabCount++;

                // PrefabのRootを作成
                var parents = new GameObject[prefabCount];
                for (var i = 0; i < parents.Length; i++)
                {
                    parents[i] = new GameObject($"{fbxObject.name}_{i:0000}");
                }

                // メッシュを読み込んで結合する
                for (var i = 0; i < fbxObject.transform.childCount; i++)
                {
                    var t = fbxObject.transform.GetChild(i);
                    var target = parents[i / objectPerPrefab];
                    MergeMesh(t, target.transform, fbxObject.name);
                }

                // Prefab保存
                foreach (var parent in parents)
                {
                    PrefabUtility.SaveAsPrefabAsset(parent, $"Assets/{fbxObject.name}/{parent.name}.prefab");
                    DestroyImmediate(parent);
                }

                // FBMをリネーム
                if (!string.IsNullOrEmpty(newFbmDirName))
                {
                    AssetDatabase.MoveAsset($"Assets/{newFbmDirName}", $"Assets/{fbxObject.name}/texture");
                }
            }
            finally
            {
                // 元のFBXファイルを削除
                AssetDatabase.DeleteAsset(importPath);

                AssetDatabase.StopAssetEditing();
                var time = Time.realtimeSinceStartup - t0;
                Debug.Log($"Done: {time}sec");
            }
        }
    }

    private void CopyFiles(string sourceDir, string destDirName)
    {
        var textureFiles = Directory.GetFiles(sourceDir);
        foreach (var path in textureFiles)
        {
            var fileName = Path.GetFileName(path);
            var assetPath = Path.Combine("Assets", destDirName, fileName);
            File.Copy(path, assetPath);
            AssetDatabase.ImportAsset(assetPath);
        }
    }

    private void MergeMesh(Transform t, Transform parent, string assetDir)
    {
        var newMesh = new Mesh();
        var vertices = new List<Vector3>();
        var indexes = new List<int>();
        var uvs = new List<Vector2>();
        var index = 0;

        foreach (Transform child in t)
        {
            var meshFilter = child.GetComponent<MeshFilter>();
            var mesh = meshFilter.sharedMesh;

            vertices.AddRange(mesh.vertices);

            if (mesh.uv.Length == 0)
            {
                // UVが無いモデルは0で埋める。
                for (var i = 0; i < mesh.vertices.Length; i++)
                    uvs.Add(Vector2.zero);
            }
            else
            {
                uvs.AddRange(mesh.uv);
            }

            var tris = mesh.triangles;
            foreach (var i in tris) indexes.Add(i + index);

            index += mesh.vertices.Length;
        }
        var flaf = false;
        if (t && t.childCount !=0 )
        {
            flaf = true;
        }
        // FIXME 最後のマテリアルをセットするが、意図したマテリアルになる保証はない。
        var meshRenderer = flaf == true ? t.GetChild(t.childCount-1).GetComponent<MeshRenderer>() :null;
        var material = meshRenderer != null ? new Material(meshRenderer.sharedMaterial) : null ;

        newMesh.vertices = vertices.ToArray();
        newMesh.triangles = indexes.ToArray();
        newMesh.uv = uvs.ToArray();
        newMesh.RecalculateNormals();
        CreateObject(newMesh, material, t.name, assetDir, parent);
    }

    // GameObject生成
    private void CreateObject(Mesh mesh, Material material, string objName, string assetDir, Transform parent)
    {
        CreateAsset(mesh, objName, $"{assetDir}/mesh");
        if(material)
            CreateAsset(material, objName, $"{assetDir}/material");

        var go = new GameObject(objName);
        go.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        go.transform.SetParent(parent);
        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = material;
    }

    // アセット生成
    private void CreateAsset(Object obj, string assetName, string directory)
    {
        if (AssetDatabase.Contains(obj)) return;

        var dir = Path.Combine(Application.dataPath, directory);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var fileName = assetName + ".asset";

        var path = Path.Combine(Application.dataPath, directory, fileName);

        var cnt = 1;
        while (File.Exists(path))
        {
            fileName = $"{assetName} {cnt}.asset";
            path = Path.Combine(Application.dataPath, directory, fileName);
            cnt++;
        }

        AssetDatabase.CreateAsset(obj, Path.Combine("Assets", directory, fileName));
    }
}