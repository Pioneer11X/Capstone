using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.IO;

public class StaticBatcher
{
    struct Renderable
    {
        public GameObject  gameObject;
        public Mesh mesh;
        public Material material;
        public int subMesh;
        public int shaderType;
    }

    [MenuItem("Tools/Static Batch")]
    static void StaticBatch()
    {
        var originalScene = SceneManager.GetActiveScene();
        string originalPath = originalScene.path;
        string basename = Path.GetFileNameWithoutExtension(originalPath);
        string dir = Path.GetDirectoryName(originalPath);

        string newPath = dir + "/" + basename + "_StaticBatched.unity";

        var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(newScene);


        List<Renderable> renderables = new List<Renderable>();

        GameObject[] rootGOs = originalScene.GetRootGameObjects();
        
        foreach (GameObject go in rootGOs)
        {
            ProcessGameObject(go, renderables);
        }

        renderables.Sort((a, b) =>
        {
            // comprare order: shader, material, mesh, submesh
            int ret = a.shaderType - b.shaderType;
            if (0 == ret)
            {
                ret = a.material.GetInstanceID() - b.material.GetInstanceID();
                if (0 == ret)
                {
                    ret = a.mesh.GetInstanceID() - b.mesh.GetInstanceID();
                    if (0 == ret)
                    {
                        ret = a.subMesh - b.subMesh;
                    }
                }
            }
            
            return ret;
        });

        Debug.Log("Total meshes: " + renderables.Count);

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (Renderable r in renderables)
        {
            sb.AppendFormat("[{0}] {1} ({2})\n", r.material.name, r.mesh.name, r.subMesh);
        }
        Debug.Log(sb.ToString());
    }

    static void ProcessGameObject(GameObject go, List<Renderable> renderables)
    {
        Renderer renderer = go.GetComponent<Renderer>();
        MeshFilter meshFilter = go.GetComponent<MeshFilter>();

        if (renderer && meshFilter)
        {
            Mesh mesh = meshFilter.sharedMesh;
            Material[] materials = renderer.sharedMaterials;
            
            if (materials.Length > mesh.subMeshCount)
            {
                throw new System.Exception("Materials count is larger than sub mesh count");
            }

            int subMeshCount = Mathf.Max(mesh.subMeshCount, materials.Length);

            for (int i = 0; i < subMeshCount; i++)
            {
                int matIdx = Mathf.Min(i, materials.Length - 1);

                Renderable r = new Renderable()
                {
                    gameObject = go,
                    mesh = mesh,
                    material = materials[matIdx],
                    subMesh = i
                };

                r.shaderType = r.material.shader.name.Contains("Transparent") ? 1 : 0;
                
                renderables.Add(r);
            }
        }

        foreach (Transform child in go.transform)
        {
            ProcessGameObject(child.gameObject, renderables);
        }
    }
    
}
