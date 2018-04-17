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
        List<Collider> colliders = new List<Collider>();
        List<Light> lights = new List<Light>();

        GameObject[] rootGOs = originalScene.GetRootGameObjects();
        
        foreach (GameObject go in rootGOs)
        {
            ProcessGameObject(go, renderables, colliders, lights);
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
        List<Renderable> singleInstances = new List<Renderable>();
        List<List<Renderable>> instancedBaches = new List<List<Renderable>>();
        List<List<Renderable>> mergedBaches = new List<List<Renderable>>();

        List<Renderable> notInstanced;// = new List<Renderable>();

        for (int i = 0; i < renderables.Count; i++)
        {
            int startIdx = i;
            while (i < renderables.Count - 1 && 
                renderables[i].material == renderables[i + 1].material &&
                renderables[i].mesh == renderables[i + 1].mesh &&
                renderables[i].subMesh == renderables[i + 1].subMesh)
            {
                i++;
            }

            if (startIdx == i)
            {
                singleInstances.Add(renderables[i]);
            }
            else
            {
                List<Renderable> batch = new List<Renderable>();
                for (int j = startIdx; j <= i; j++)
                {
                    batch.Add(renderables[j]);
                }

                instancedBaches.Add(batch);
            }
        }

        notInstanced = singleInstances;
        singleInstances = new List<Renderable>();

        for (int i = 0; i < notInstanced.Count; i++)
        {
            int startIdx = i;
            while (i < notInstanced.Count - 1 &&
                notInstanced[i].material == notInstanced[i + 1].material)
            {
                i++;
            }

            if (startIdx == i)
            {
                singleInstances.Add(notInstanced[i]);
            }
            else
            {
                List<Renderable> batch = new List<Renderable>();
                for (int j = startIdx; j <= i; j++)
                {
                    batch.Add(notInstanced[j]);
                }

                mergedBaches.Add(batch);
            }
        }
        
        int totalBatches = 0;
        int totalMeshes = 0;

        int numMeshesInstanced = 0;
        int numMeshesMerged = 0;

        // instance batches
        for (int i = 0; i < instancedBaches.Count; i++)
        {
            var batch = instancedBaches[i];

            sb.AppendFormat("Instanced Batch {0} [{1}]", i, batch.Count);
            GameObject group = new GameObject(sb.ToString());
            sb.Length = 0;

            numMeshesInstanced += batch.Count;

            group.isStatic = true;

            for (int j = 0; j < batch.Count; j++)
            {
                Renderable r = batch[j];
                sb.AppendFormat("[{0}]{1}_{2}", r.material.name, r.mesh.name, r.subMesh);

                GameObject go = new GameObject(sb.ToString());
                sb.Length = 0;

                go.isStatic = true;

                go.transform.parent = group.transform;
                go.transform.position = r.gameObject.transform.position;
                go.transform.rotation = r.gameObject.transform.rotation;
                go.transform.localScale = r.gameObject.transform.lossyScale;
                      
                if (r.subMesh == 0)
                {

                    go.AddComponent<MeshFilter>().sharedMesh = r.mesh;
                    
                    go.AddComponent<MeshRenderer>().sharedMaterial = r.material;
                }
            }
        }

        totalBatches += instancedBaches.Count;
        totalMeshes += numMeshesInstanced;

        // merged batches
        for (int i = 0; i < mergedBaches.Count; i++)
        {
            var batch = mergedBaches[i];

            sb.AppendFormat("Merged Batch {0} [{1}]", i, batch.Count);
            GameObject group = new GameObject(sb.ToString());
            sb.Length = 0;

            numMeshesMerged += batch.Count;

            group.isStatic = true;

            for (int j = 0; j < batch.Count; j++)
            {
                Renderable r = batch[j];
                sb.AppendFormat("[{0}]{1}_{2}", r.material.name, r.mesh.name, r.subMesh);

                GameObject go = new GameObject(sb.ToString());
                sb.Length = 0;

                go.isStatic = true;

                go.transform.parent = group.transform;
                go.transform.position = r.gameObject.transform.position;
                go.transform.rotation = r.gameObject.transform.rotation;
                go.transform.localScale = r.gameObject.transform.lossyScale;

                if (r.subMesh == 0)
                {

                    go.AddComponent<MeshFilter>().sharedMesh = r.mesh;

                    go.AddComponent<MeshRenderer>().sharedMaterial = r.material;
                }
            }
        }

        totalBatches += mergedBaches.Count;
        totalMeshes += numMeshesMerged;

        // single instances
        foreach (var r in singleInstances)
        {
            
            sb.AppendFormat("[{0}]{1}_{2}", r.material.name, r.mesh.name, r.subMesh);
            
            GameObject go = new GameObject(sb.ToString());

            go.isStatic = true;

            sb.Length = 0;

            go.transform.position = r.gameObject.transform.position;
            go.transform.rotation = r.gameObject.transform.rotation;
            go.transform.localScale = r.gameObject.transform.lossyScale;

            if (r.subMesh > 0) continue;

            go.AddComponent<MeshFilter>().sharedMesh = r.mesh;
            
            go.AddComponent<MeshRenderer>().sharedMaterial = r.material;
        }

        totalBatches += singleInstances.Count;
        totalMeshes += singleInstances.Count;

        if (totalMeshes != renderables.Count)
        {
            throw new System.Exception("Error!");
        }

        sb.Length = 0;
        sb.AppendFormat("Total batches: {0} ({1} meshes are instancing batched, {2} meshes are merged)", totalBatches, numMeshesInstanced, numMeshesMerged);

        Debug.Log(sb.ToString());

        // colliders

        if (colliders.Count > 0)
        {
            GameObject group = new GameObject("Colliders [" + colliders.Count + "]");
            
            foreach (Collider collider in colliders)
            {
                GameObject go = GameObject.Instantiate<GameObject>(collider.gameObject);
                go.transform.parent = group.transform;

                go.transform.position = collider.gameObject.transform.position;
                go.transform.rotation = collider.gameObject.transform.rotation;
                go.transform.localScale = collider.gameObject.transform.lossyScale;
            }
        }

        // lights

        if (lights.Count > 0)
        {
            GameObject group = new GameObject("Lights [" + lights.Count + "]");

            foreach (Light light in lights)
            {
                GameObject go = GameObject.Instantiate<GameObject>(light.gameObject);
                go.transform.parent = group.transform;

                go.transform.position = light.gameObject.transform.position;
                go.transform.rotation = light.gameObject.transform.rotation;
                go.transform.localScale = light.gameObject.transform.lossyScale;
            }
        }
    }

    static void ProcessGameObject(GameObject go, List<Renderable> renderables, List<Collider> colliders, List<Light> lights)
    {
        if (!go.activeInHierarchy)
            return;

        if (go.layer == LayerMask.NameToLayer("UI"))
            return;

        if (go.tag == "MainCamera")
            return;

        if (go.tag == "Enemy" || go.tag == "Player" || go.tag == "Companion")
            return;

        // Node exporting
        if (go.tag == "PathNode") { return; }

        if (go.tag == "JumpNode") { return; }

        if (go.tag == "Spawner" || go.tag == "TriggerNode") { return; }

        if (go.tag == "PlayerSpawn") { return; }

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

        Collider collider = go.GetComponent<Collider>();
        if (collider)
        {
            colliders.Add(collider);
        }

        Light light = go.GetComponent<Light>();
        if (light)
        {
            lights.Add(light);
        }

        foreach (Transform child in go.transform)
        {
            ProcessGameObject(child.gameObject, renderables, colliders, lights);
        }
    }
    
}
