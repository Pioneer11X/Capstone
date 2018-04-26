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

    [MenuItem("Tools/Batch GameObject")]
    static void BatchGameObject()
    {
        GameObject root = Selection.activeObject as GameObject;
        if (!root)
            return;

        string rootName = root.name;
        string fileName = null;
        string folderPath = null;

        GameObject prefab = PrefabUtility.GetPrefabParent(root) as GameObject;
        if (null != prefab)
        {
            rootName = prefab.name;
            folderPath = AssetDatabase.GetAssetPath(prefab);
            fileName = Path.GetFileNameWithoutExtension(folderPath);
            folderPath = Path.GetDirectoryName(folderPath);
        }

        List<Renderable> renderables = new List<Renderable>();
        List<Collider> colliders = new List<Collider>();
        List<Light> lights = new List<Light>();
        List<GameObject> miscs = new List<GameObject>();
        
        ProcessGameObject(root, renderables, colliders, lights, miscs);

        List<Renderable> singleInstances = new List<Renderable>();
        List<List<Renderable>> mergedBaches = new List<List<Renderable>>();

        StaticBatch(renderables, null, mergedBaches, singleInstances, false);

        List<GameObject> directChildren = new List<GameObject>();
        foreach (Transform t in root.transform)
        {
            directChildren.Add(t.gameObject);
        }

        Debug.Log("Total meshes: " + renderables.Count);

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        int totalBatches = 0;
        int totalMeshes = 0;

        int numMeshesInstanced = 0;
        int numMeshesMerged = 0;

        // merged batches
        for (int i = 0; i < mergedBaches.Count; i++)
        {
            var batch = mergedBaches[i];

            sb.AppendFormat("Merged Batch {0} [{1}]", i, batch.Count);
            GameObject group = new GameObject(sb.ToString());
            sb.Length = 0;

            numMeshesMerged += batch.Count;

            group.transform.parent = root.transform;
            group.isStatic = root.isStatic;
            
            Mesh mergedMesh = new Mesh();

            CombineInstance[] insts = new CombineInstance[batch.Count];

            for (int j = 0; j < batch.Count; j++)
            {
                insts[j].mesh = batch[j].mesh;
                insts[j].subMeshIndex = batch[j].subMesh;
                insts[j].transform = batch[j].gameObject.transform.localToWorldMatrix * group.transform.worldToLocalMatrix;
            }

            mergedMesh.CombineMeshes(insts, true, true, false);

            mergedMesh.name = rootName + "_Merged_Mesh_" + i;

            if (!string.IsNullOrEmpty(folderPath))
            {
                string meshPath = folderPath + "/" + mergedMesh.name + ".asset";
                AssetDatabase.CreateAsset(mergedMesh, meshPath);
            }

            group.AddComponent<MeshFilter>().sharedMesh = mergedMesh;
            group.AddComponent<MeshRenderer>().sharedMaterial = batch[0].material;
        }

        AssetDatabase.SaveAssets();

        totalBatches += mergedBaches.Count;
        totalMeshes += numMeshesMerged;

        // single instances
        foreach (var r in singleInstances)
        {
        
            sb.AppendFormat("[{0}]{1}_{2}", r.material.name, r.mesh.name, r.subMesh);
        
            GameObject go = new GameObject(sb.ToString());

            sb.Length = 0;
            go.isStatic = root.isStatic;

            go.transform.position = r.gameObject.transform.position;
            go.transform.rotation = r.gameObject.transform.rotation;
            go.transform.localScale = r.gameObject.transform.lossyScale;

            go.transform.SetParent(root.transform, true);
        
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
            group.transform.parent = root.transform;

            foreach (Collider collider in colliders)
            {
                GameObject go = new GameObject(collider.gameObject.name);
                
                go.transform.position = collider.gameObject.transform.position;
                go.transform.rotation = collider.gameObject.transform.rotation;
                go.transform.localScale = collider.gameObject.transform.lossyScale;

                go.transform.SetParent(group.transform, true);

                BoxCollider box = collider as BoxCollider;
                SphereCollider sphere = collider as SphereCollider;
                CapsuleCollider capsule = collider as CapsuleCollider;
                MeshCollider mesh = collider as MeshCollider;

                if (box)
                {
                    var comp = go.AddComponent<BoxCollider>();
                    comp.center = box.center;
                    comp.size = box.size;
                }
                else if (sphere)
                {
                    var comp = go.AddComponent<SphereCollider>();
                    comp.center = sphere.center;
                    comp.radius = sphere.radius;
                }
                else if (capsule)
                {
                    var comp = go.AddComponent<CapsuleCollider>();
                    comp.center = capsule.center;
                    comp.radius = capsule.radius;
                    comp.height = capsule.height;
                }
                else if (mesh)
                {
                    var comp = go.AddComponent<MeshCollider>();
                    comp.sharedMesh = mesh.sharedMesh;
                }
                else
                {
                    throw new System.Exception("Unsupported collider.");
                }
            }
        }

        // lights
        if (lights.Count > 0)
        {
            GameObject group = new GameObject("Lights [" + lights.Count + "]");
            group.transform.parent = root.transform;

            foreach (Light light in lights)
            {
                GameObject go = new GameObject(light.gameObject.name);

                go.transform.position = light.gameObject.transform.position;
                go.transform.rotation = light.gameObject.transform.rotation;
                go.transform.localScale = light.gameObject.transform.lossyScale;

                go.transform.SetParent(group.transform, true);

                Light comp = go.AddComponent<Light>();
                comp.type = light.type;
                comp.color = light.color;
                comp.range = light.range;
                comp.intensity = light.intensity;
                comp.spotAngle = light.spotAngle;
                comp.shadows = light.shadows;
            }
        }

        // miscs

        foreach (GameObject miscGo in miscs)
        {
            GameObject go = GameObject.Instantiate<GameObject>(miscGo);

            go.transform.position = miscGo.gameObject.transform.position;
            go.transform.rotation = miscGo.gameObject.transform.rotation;
            go.transform.localScale = miscGo.gameObject.transform.lossyScale;

            go.transform.SetParent(root.transform, true);
        }

        foreach (GameObject child in directChildren)
        {
            GameObject.DestroyImmediate(child);
        }
    }

    [MenuItem("Tools/Static Batch (preview)")]
    static void StaticBatchPreview()
    {
        StaticBatchScene(false, false);
    }

    [MenuItem("Tools/Static Batch")]
    static void StaticBatchDefault()
    {
        StaticBatchScene(true, false);
    }

    [MenuItem("Tools/Static Batch (merge instancing)")]
    static void StaticBatchMergeInstancing()
    {
        StaticBatchScene(true, true);
    }

    static void StaticBatchScene(bool merge, bool mergeInstancingObjs)
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
        List<GameObject> miscs = new List<GameObject>();

        GameObject[] rootGOs = originalScene.GetRootGameObjects();
        
        foreach (GameObject go in rootGOs)
        {
            ProcessGameObject(go, renderables, colliders, lights, miscs);
        }

        List<Renderable> singleInstances = new List<Renderable>();
        List<List<Renderable>> instancedBaches = new List<List<Renderable>>();
        List<List<Renderable>> mergedBaches = new List<List<Renderable>>();

        StaticBatch(renderables, instancedBaches, mergedBaches, singleInstances, mergeInstancingObjs);


        Debug.Log("Total meshes: " + renderables.Count);

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
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

            if (mergeInstancingObjs)
            {
                Mesh mergedMesh = new Mesh();

                CombineInstance[] insts = new CombineInstance[batch.Count];

                for (int j = 0; j < batch.Count; j++)
                {
                    insts[j].mesh = batch[j].mesh;
                    insts[j].subMeshIndex = batch[j].subMesh;
                    insts[j].transform = batch[j].gameObject.transform.localToWorldMatrix;
                }

                mergedMesh.CombineMeshes(insts, true, true, false);

                mergedMesh.name = "Merged_Instancing_Mesh_" + originalScene.name + "_" + i;

                group.AddComponent<MeshFilter>().sharedMesh = mergedMesh;
                group.AddComponent<MeshRenderer>().sharedMaterial = batch[0].material;
            }
            else
            { 
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

            if (merge) // actuall merge the meshes
            {
                Mesh mergedMesh = new Mesh();

                CombineInstance[] insts = new CombineInstance[batch.Count];

                for (int j = 0; j < batch.Count; j++)
                {
                    insts[j].mesh = batch[j].mesh;
                    insts[j].subMeshIndex = batch[j].subMesh;
                    insts[j].transform = batch[j].gameObject.transform.localToWorldMatrix;
                }

                mergedMesh.CombineMeshes(insts, true, true, false);

                mergedMesh.name = "Merged_Mesh_" + originalScene.name + "_" + i;

                group.AddComponent<MeshFilter>().sharedMesh = mergedMesh;
                group.AddComponent<MeshRenderer>().sharedMaterial = batch[0].material;
            }
            else // just list meshes to be merged
            {
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
                GameObject go = new GameObject(collider.gameObject.name);
                go.transform.parent = group.transform;

                go.transform.position = collider.gameObject.transform.position;
                go.transform.rotation = collider.gameObject.transform.rotation;
                go.transform.localScale = collider.gameObject.transform.lossyScale;

                BoxCollider box = collider as BoxCollider;
                SphereCollider sphere = collider as SphereCollider;
                CapsuleCollider capsule = collider as CapsuleCollider;
                MeshCollider mesh = collider as MeshCollider;

                if (box)
                {
                    var comp = go.AddComponent<BoxCollider>();
                    comp.center = box.center;
                    comp.size = box.size;
                }
                else if (sphere)
                {
                    var comp = go.AddComponent<SphereCollider>();
                    comp.center = sphere.center;
                    comp.radius = sphere.radius;
                }
                else if (capsule)
                {
                    var comp = go.AddComponent<CapsuleCollider>();
                    comp.center = capsule.center;
                    comp.radius = capsule.radius;
                    comp.height = capsule.height;
                }
                else if (mesh)
                {
                    var comp = go.AddComponent<MeshCollider>();
                    comp.sharedMesh = mesh.sharedMesh;
                }
                else
                {
                    throw new System.Exception("Unsupported collider.");
                }
            }
        }

        // lights
        if (lights.Count > 0)
        {
            GameObject group = new GameObject("Lights [" + lights.Count + "]");

            foreach (Light light in lights)
            {
                GameObject go = new GameObject(light.gameObject.name);
                go.transform.parent = group.transform;

                go.transform.position = light.gameObject.transform.position;
                go.transform.rotation = light.gameObject.transform.rotation;
                go.transform.localScale = Vector3.one;

                Light comp = go.AddComponent<Light>();
                comp.type = light.type;
                comp.color = light.color;
                comp.range = light.range;
                comp.intensity = light.intensity;
                comp.spotAngle = light.spotAngle;
                comp.shadows = light.shadows;
            }
        }

        // miscs
        
        foreach (GameObject miscGo in miscs)
        {
            GameObject go = GameObject.Instantiate<GameObject>(miscGo);

            go.transform.position = miscGo.gameObject.transform.position;
            go.transform.rotation = miscGo.gameObject.transform.rotation;
            go.transform.localScale = miscGo.gameObject.transform.lossyScale;
        }
        
    }

    static void ProcessGameObject(GameObject go, List<Renderable> renderables, List<Collider> colliders, List<Light> lights, List<GameObject> miscs)
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
        if (go.tag == "PathNode")
        {
            miscs.Add(go);
            return;
        }

        if (go.tag == "JumpNode")
        {
            miscs.Add(go);
            return;
        }

        if (go.tag == "Spawner" || go.tag == "TriggerNode")
        {
            miscs.Add(go);
            return;
        }

        if (go.tag == "PlayerSpawn")
        {
            miscs.Add(go);
            return;
        }

        Renderer renderer = go.GetComponent<Renderer>();
        MeshFilter meshFilter = go.GetComponent<MeshFilter>();
        
        if (renderer && meshFilter && 
            null != meshFilter.sharedMesh && 
            null != renderer.sharedMaterials && 
            renderer.sharedMaterials.Length > 0)
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
                r.shaderType = (int)FriedTofu.ResourceMap.GetMaterialType(r.material);
                
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
            ProcessGameObject(child.gameObject, renderables, colliders, lights, miscs);
        }
    }
    
    static void StaticBatch(List<Renderable> renderables, List<List<Renderable>> instancedBaches, List<List<Renderable>> mergedBaches, List<Renderable> singleInstances, bool limitMaxIndexCount = false)
    {

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
        
        List<Renderable> notInstanced;// = new List<Renderable>();

        if (null != instancedBaches)
        {
            for (int i = 0; i < renderables.Count; i++)
            {
                int startIdx = i;
                uint indicesCount = renderables[i].mesh.GetIndexCount(renderables[i].subMesh);

                while (i < renderables.Count - 1 &&
                    renderables[i].material == renderables[i + 1].material &&
                    renderables[i].mesh == renderables[i + 1].mesh &&
                    renderables[i].subMesh == renderables[i + 1].subMesh)
                {
                    indicesCount += renderables[i + 1].mesh.GetIndexCount(renderables[i + 1].subMesh);
                    if (limitMaxIndexCount && indicesCount >= 65536)
                        break;

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

            notInstanced = new List<Renderable>(singleInstances);
            singleInstances.Clear();
        }
        else
        {
            notInstanced = renderables;
        }

        for (int i = 0; i < notInstanced.Count; i++)
        {
            int startIdx = i;
            uint indicesCount = notInstanced[i].mesh.GetIndexCount(notInstanced[i].subMesh);

            while (i < notInstanced.Count - 1 &&
                notInstanced[i].material == notInstanced[i + 1].material)
            {
                indicesCount += notInstanced[i + 1].mesh.GetIndexCount(notInstanced[i + 1].subMesh);
                if (indicesCount >= 65536)
                    break;

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
    }
}
