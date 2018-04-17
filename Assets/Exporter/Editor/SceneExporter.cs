using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using System.Reflection;

namespace FriedTofu
{
    public class SceneExporter
    {
        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 44)]
        [Serializable]
        public struct ModelHeader
        {
            [FieldOffset(0)] public uint Magic;
            [FieldOffset(4)] public uint Version;
            [FieldOffset(8)] public byte Flags;
            [FieldOffset(9)] public byte Reserved0;
            [FieldOffset(10)] public byte Reserved1;
            [FieldOffset(11)] public byte NumTexcoordChannels;
            [FieldOffset(12)] public uint NumVertices;
            [FieldOffset(16)] public uint NumMeshes;
            [FieldOffset(20)] public uint NumBones;
            [FieldOffset(24)] public uint NumAnimations;
            [FieldOffset(28)] public uint NumAnimChannels;
            [FieldOffset(32)] public uint NumTotalTranslationFrames;
            [FieldOffset(36)] public uint NumTotalRotationFrames;
            [FieldOffset(40)] public uint NumTotalScaleFrames;
            [FieldOffset(44)] public uint NumAnimationFrames;

            public void Initialize()
            {
                Magic = 0x004C444Du;
                Version = 1u;
                Flags = 0xC; // AoS, No Animation, Has Index, Has Tangent
            }

            public void WriteTo(BinaryWriter writer)
            {
                writer.Write(Magic);
                writer.Write(Version);
                writer.Write(Flags);
                writer.Write(Reserved0);
                writer.Write(Reserved1);
                writer.Write((byte)(NumTexcoordChannels << 4));
                writer.Write(NumVertices);
                writer.Write(NumMeshes);
                writer.Write(NumBones);
                writer.Write(NumAnimations);
                writer.Write(NumAnimChannels);
                writer.Write(NumTotalTranslationFrames);
                writer.Write(NumTotalRotationFrames);
                writer.Write(NumTotalScaleFrames);
                writer.Write(NumAnimationFrames);
            }
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
        [Serializable]
        public struct SubMesh
        {
            [FieldOffset(0)] public uint BaseVertex;
            [FieldOffset(4)] public uint NumIndices;

            public void WriteTo(BinaryWriter writer)
            {
                writer.Write(BaseVertex);
                writer.Write(NumIndices);
            }
        }

        //[MenuItem("Tools/Test")]
        //static void Test()
        //{
        //    string path = EditorUtility.SaveFilePanel("Save to", null, null, "bin");
        //    if (string.IsNullOrEmpty(path)) return;

        //    using (var stream = File.Open(path, FileMode.Create))
        //    {
        //        using (var writer = new BinaryWriter(stream))
        //        {
        //            List<ushort> list = new List<ushort>();
        //            for (int i = 0; i < 128; i++)
        //            {
        //                list.Add((ushort)i);
        //                writer.Write((ushort)i);
        //            }
        //        }
        //    }
        //}

        [Serializable]
        public struct Config
        {
            public string LastExportPath;
            public string DXTexConvPath;
        }

        [MenuItem("Tools/Set TexConv Path")]
        static void SetDXTexConv()
        {
            string configPath = Application.dataPath + "exporter.cfg";
            Config config = new Config();

            if (File.Exists(configPath))
                config = JsonUtility.FromJson<Config>(File.ReadAllText(configPath));

            string path = EditorUtility.OpenFilePanel(
                "Select texconv.exe", config.DXTexConvPath, "exe");

            if (string.IsNullOrEmpty(path))
                return;

            config.DXTexConvPath = path;
            File.WriteAllText(configPath, JsonUtility.ToJson(config));
        }

        [MenuItem("Tools/Export Current Scene (readable)")]
        static void MenuEntry()
        {
            ExportCurrentScene(false);
        }

        [MenuItem("Tools/Export Current Scene (compact)")]
        static void MenuEntryCompact()
        {
            ExportCurrentScene(true);
        }

        static void ExportCurrentScene(bool compact)
        {
            string configPath = Application.dataPath + "exporter.cfg";
            string lastPath = null;

            Config config = new Config();

            if (File.Exists(configPath))
            {
                config = JsonUtility.FromJson<Config>(File.ReadAllText(configPath));
                lastPath = config.LastExportPath;
            }

            string path = EditorUtility.SaveFolderPanel("Select output assets folder", lastPath, null);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            config.LastExportPath = path;
            File.WriteAllText(configPath, JsonUtility.ToJson(config));

            ResourceMap context = new ResourceMap(path);
            Debug.Log("Assets folder: " + path);

            // get all root gameobjects in current scene
            var unityScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            GameObject[] rootObjects = unityScene.GetRootGameObjects();

            Scene scene = new Scene();

            // recursively export all gameobjects
            foreach (GameObject obj in rootObjects)
            {
                Scene.Entity entity = ExportGameObject(obj, context, "");
                if (null != entity)
                {
                    scene.Add(entity);
                }
                else if (obj.tag == "PathNode" || obj.tag == "JumpNode")
                {
                    Scene.PathNode node = new Scene.PathNode(obj);
                    scene.AddPathNode(node);
                }
                else if (obj.tag == "Spawner")
                {
                    Scene.SpawnNode node = new Scene.SpawnNode(obj);
                    foreach (Transform child in obj.transform)
                    {
                        node.AddChild(child.gameObject.name);
                        Scene.TriggerNode childNode = new Scene.TriggerNode(child.gameObject);
                        scene.AddTriggerNode(childNode);
                    }

                    scene.AddSpwanerNode(node);
                }
                else if(obj.tag == "PlayerSpawn")
                {
                    Scene.SpawnPoint node = new Scene.SpawnPoint(obj);
                    scene.AddSpawnPoint(node);
                }
            }

            // sort the path nodes here
            scene.SortPathNodes();


            scene.Save(
                Path.Combine(
                    Path.Combine(
                        context.BaseDir,
                        "scenes"
                        ),
                    unityScene.name + ".json"
                    ),
                compact);

            context.Save();

            Debug.Log("Done.");
        }

        static Scene.Entity ExportGameObject(GameObject obj, ResourceMap context, string indent = "")
        {
            if (!obj.activeInHierarchy)
                return null;

            if (obj.layer == LayerMask.NameToLayer("UI"))
                return null;

            if (obj.tag == "MainCamera")
                return null;

            if (obj.tag == "Enemy" || obj.tag == "Player" || obj.tag == "Companion")
                return null;

            // Node exporting
            if(obj.tag == "PathNode") { return null; }

            if (obj.tag == "JumpNode") { return null; }

            if (obj.tag == "Spawner" || obj.tag == "TriggerNode") { return null; }

            if(obj.tag == "PlayerSpawn") { return null; }

            Scene.Entity entity = new Scene.Entity(obj);

            string type = "unknown";

            if (null != obj.GetComponent<Light>() && obj.GetComponent<Light>().enabled)
            {
                type = "light";
            }
            else if (null != obj.GetComponent<MeshRenderer>() && obj.GetComponent<MeshRenderer>().enabled)
            {
                type = "mesh";
            }

            if (type == "mesh")
            {
                Scene.Renderable renderable = new Scene.Renderable();

                // export mesh
                {
                    MeshFilter filter = obj.GetComponent<MeshFilter>();
                    if (null == filter)
                    {
                        throw new Exception("MeshFilter not found.");
                    }

                    //Debug.Log(filter.sharedMesh.name + " [" + filter.sharedMesh.subMeshCount + "]");
                    
                    string meshPath = ExportMesh(filter.sharedMesh, context);

                    renderable.model = meshPath;
                }

                // materials
                {
                    MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                    for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                    {
                        Material mat = renderer.sharedMaterials[i];
                        string name = ExportMaterial(mat, context);
                        if (string.IsNullOrEmpty(name))
                        {
                            throw new Exception("Cannot find material asset.");
                        }

                        renderable.Add(name);
                    }
                }

                entity.Add(renderable);
            }
            else if (type == "light")
            {
                Light light = obj.GetComponent<Light>();
                if (light.type != LightType.Area)
                {
                    Scene.Light sceneLight = new Scene.Light();
                    sceneLight.lightType = light.type.ToString().ToLower();
                    sceneLight.color = new Scene.Float4(light.color);
                    sceneLight.range = light.range;
                    sceneLight.intensity = light.intensity;
                    sceneLight.spotAngle = light.spotAngle;
                    sceneLight.castShadow = (light.shadows != LightShadows.None);
                    entity.Add(sceneLight);
                }
            }

            Collider collider = obj.GetComponent<Collider>();
            if (null != collider && !collider.isTrigger && collider.enabled)
            {
                if (null != collider as BoxCollider)
                {
                    BoxCollider box = collider as BoxCollider;
                    Scene.PhysicsComponent comp = new Scene.PhysicsComponent();
                    comp.colliderType = "box";
                    comp.center = new Scene.Float3(box.center);
                    comp.size = new Scene.Float3(box.size * 0.5f);
                    entity.Add(comp);
                }
                else if (null != collider as SphereCollider)
                {
                    SphereCollider sphere = collider as SphereCollider;
                    Scene.PhysicsComponent comp = new Scene.PhysicsComponent();
                    comp.colliderType = "sphere";
                    comp.center = new Scene.Float3(sphere.center);
                    comp.size = new Scene.Float3(sphere.radius, 0, 0);
                    entity.Add(comp);
                }
                else if (null != collider as CapsuleCollider && (collider as CapsuleCollider).direction == 1)
                {
                    CapsuleCollider capsule = collider as CapsuleCollider;
                    Scene.PhysicsComponent comp = new Scene.PhysicsComponent();
                    comp.colliderType = "capsule";
                    comp.center = new Scene.Float3(capsule.center);
                    comp.size = new Scene.Float3(capsule.radius, capsule.height - 2 * capsule.radius, 0);
                    entity.Add(comp);
                }
                else if (null != collider as MeshCollider)
                {
                    MeshCollider meshCollider = collider as MeshCollider;
                    Scene.PhysicsComponent comp = new Scene.PhysicsComponent();
                    comp.colliderType = "mesh";
                    comp.model = ExportMesh(meshCollider.sharedMesh, context);
                    entity.Add(comp);
                }
                else
                {
                    Debug.LogError("Unsupported collider on [" + entity.name + "]");
                }
            }

            foreach (Transform child in obj.transform)
            {
                Scene.Entity childEntity = ExportGameObject(child.gameObject, context, indent + "  ");
                if (null != childEntity)
                {
                    entity.Add(childEntity);
                }
            }

            return entity;
        }

        static string ExportMesh(Mesh mesh, ResourceMap context)
        {
            string originalPath = AssetDatabase.GetAssetPath(mesh);
            string guid = string.Empty;
            string basename = string.Empty;
            int localId = 0;

            {
                PropertyInfo inspectorModeInfo = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
                SerializedObject so = new SerializedObject(mesh);
                inspectorModeInfo.SetValue(so, InspectorMode.Debug, null);

                SerializedProperty localIdProp = so.FindProperty("m_LocalIdentfierInFile");

                localId = localIdProp.intValue;
            }

            if (string.IsNullOrEmpty(originalPath))
            {   
                guid = localId.ToString();
                basename = mesh.name + "_" + localId;
            }
            else
            {
                guid = AssetDatabase.AssetPathToGUID(originalPath);

                basename = Path.GetFileNameWithoutExtension(originalPath);

                if (basename == "unity default resources")
                {
                    basename = "Unity_Builtin_" + mesh.name;
                }
                else
                {
                    basename += "_" + mesh.name;
                }
            }

            string newPath = Path.Combine("models", basename + ".model");

            newPath = newPath.Replace('\\', '/');

            // check if there are dupplicated filenames
            if (context.MeshTable.ContainsKey(basename))
            {
                if (guid != context.MeshTable[basename])
                {
                    string newname = basename + "_" + localId;
                    Debug.LogWarning("Mesh name [" + basename + "] duplicated, changed name to [" + newname + "]");

                    basename = newname;

                    if (context.MeshTable.ContainsKey(basename))
                    {
                        if (guid != context.MeshTable[basename])
                        {
                            throw new Exception("Mesh conflict: " + basename);
                        }
                        else
                        {
                            return newPath;
                        }
                    }

                    newPath = Path.Combine("models", basename + ".model").Replace('\\', '/');

                }
                else
                {
                    return newPath;
                }
            }

            // create a new file header
            ModelHeader header = new ModelHeader();
            header.Initialize();

            header.NumTexcoordChannels = 1;
            header.NumVertices = (uint)mesh.vertexCount;
            header.NumMeshes = (uint)mesh.subMeshCount;
            
            //
            using (var stream = File.Open(Path.Combine(context.BaseDir, newPath), FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    header.WriteTo(writer);

                    SubMesh[] submeshes = new SubMesh[mesh.subMeshCount];
                    uint[] startVertexLocs = new uint[mesh.subMeshCount];
                    
                    List<ushort> finalIndices = new List<ushort>();
                    
                    // writer list of sub meshes
                    for (int i = 0; i < mesh.subMeshCount; i++)
                    {
                        int[] indices = mesh.GetIndices(i);

                        if (indices.Length < 0 || indices.Length % 3 != 0)
                        {
                            throw new Exception("Mesh not supported. [" + basename + "][" + i + "] num of indices : " + indices.Length);
                        }

                        if (indices.Length == 0)
                        {
                            if (mesh.subMeshCount > 1 || mesh.vertexCount == 0)
                            {
                                throw new Exception("Mesh not supported. [" + basename + "][" + i + "] num of indices : " + indices.Length + ", num of vertices: " + mesh.vertexCount);
                            }

                            indices = new int[mesh.vertexCount];

                            for (int j = 0; j < indices.Length; j++)
                                indices[j] = j;
                        }
                        
                        for (int j = 0; j < indices.Length; j++)
                        {
                            int finalIndex = indices[j];
                            if (finalIndex < 0 || finalIndex > ushort.MaxValue)
                            {
                                throw new Exception("Mesh not supported.");
                            }

                            finalIndices.Add((ushort)finalIndex);
                        }
                        
                        submeshes[i].BaseVertex = 0;
                        submeshes[i].NumIndices = (uint)indices.Length;

                        writer.Write(submeshes[i].BaseVertex);
                        writer.Write(submeshes[i].NumIndices);
                    }
                    
                    for (int i = 0; i < mesh.vertexCount; i++)
                    {
                        // position
                        Vector3 position = mesh.vertices[i];
                        writer.Write(position.x);
                        writer.Write(position.y);
                        writer.Write(position.z);

                        // normal
                        Vector3 normal = mesh.normals[i];
                        writer.Write(normal.x);
                        writer.Write(normal.y);
                        writer.Write(normal.z);

                        // tangent
                        Vector4 tangent = mesh.tangents[i];
                        writer.Write(tangent.x);
                        writer.Write(tangent.y);
                        writer.Write(tangent.z);
                        writer.Write(tangent.w);
                        
                        // uv
                        Vector2 uv = mesh.uv[i];
                        writer.Write(uv.x);
                        writer.Write(1.0f - uv.y);
                    }

                    for (int i = 0; i < finalIndices.Count; i++)
                    {
                        writer.Write(finalIndices[i]);
                    }
                }
            }

            context.MeshTable[basename] = guid;

            return newPath;
        }

        static string ExportMaterial(Material mat, ResourceMap context)
        {
            string originalPath = AssetDatabase.GetAssetPath(mat);
            if (string.IsNullOrEmpty(originalPath))
            {
                throw new Exception("Cannot find material asset.");
            }

            string guid = AssetDatabase.AssetPathToGUID(originalPath);

            string basename = Path.GetFileNameWithoutExtension(originalPath);
            //string newPath = Path.Combine("materials", basename + ".json");

            //newPath = newPath.Replace('\\', '/');

            if (context.MaterialTable.ContainsKey(basename))
            {
                if (guid != context.MaterialTable[basename].GUID)
                {
                    PropertyInfo inspectorModeInfo = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
                    SerializedObject so = new SerializedObject(mat);
                    inspectorModeInfo.SetValue(so, InspectorMode.Debug, null);

                    SerializedProperty localIdProp = so.FindProperty("m_LocalIdentfierInFile");

                    int localId = localIdProp.intValue;

                    string newname = basename + "_" + localId;
                    Debug.LogWarning("Material name [" + basename + "] duplicated, changed name to [" + newname + "]");

                    basename = newname;

                    if (context.MaterialTable.ContainsKey(basename))
                    {
                        if (guid != context.MaterialTable[basename].GUID)
                        {
                            throw new Exception("Material conflict: " + basename);
                        }
                        else
                        {
                            return basename;
                        }
                    }
                }
                else
                {
                    return basename;
                }
            }

            ResourceMap.Material material = new ResourceMap.Material();
            material.Name = basename;
            material.GUID = guid;
            material.Type =  mat.shader.name.Contains("Transparent") ? "Transparent" : "Opaque";
            material.AlbedoMap = null;
            material.NormalMap = null;

            

            if (mat.HasProperty("_MainTex"))
            {
                string albedoMap = ExportTexture(mat.GetTexture("_MainTex"), context);

                Vector2 scale = mat.GetTextureScale("_MainTex");
                Vector2 offset = mat.GetTextureOffset("_MainTex");
                material.TextureScaleOffset = new Vector4(scale.x, scale.y, offset.x, offset.y);

                if (!string.IsNullOrEmpty(albedoMap))
                {
                    material.AlbedoMap = albedoMap;
                }
            }

            if (mat.HasProperty("_BumpMap"))
            {
                string normalMap = ExportTexture(mat.GetTexture("_BumpMap"), context);
                if (!string.IsNullOrEmpty(normalMap))
                {
                    material.NormalMap = normalMap;
                }
            }

            if (mat.HasProperty("_MetallicGlossMap"))
            {
                string metallicMap = ExportTexture(mat.GetTexture("_MetallicGlossMap"), context);
                if (!string.IsNullOrEmpty(metallicMap))
                {
                    material.MetallicGlossMap = metallicMap;
                }
            }

            if (mat.HasProperty("_OcclusionMap"))
            {
                string occlusionMap = ExportTexture(mat.GetTexture("_OcclusionMap"), context);
                if (!string.IsNullOrEmpty(occlusionMap))
                {
                    material.OcclusionMap = occlusionMap;
                }
            }

            if (mat.HasProperty("_EmissionMap"))
            {
                string emissionMap = ExportTexture(mat.GetTexture("_EmissionMap"), context);
                if (!string.IsNullOrEmpty(emissionMap))
                {
                    material.EmissionMap = emissionMap;
                }
            }

            if (mat.HasProperty("_Color"))
            {
                material.TintColor = mat.GetColor("_Color");
            }

            if (mat.HasProperty("_EmissionColor"))
            {
                material.EmissionColor = mat.GetColor("_EmissionColor");
            }

            context.MaterialTable.Add(basename, material);

            return basename;
        }

        static string ExportTexture(Texture tex, ResourceMap context)
        {
            if (null == tex)
            {
                return null;
            }

            string path = AssetDatabase.GetAssetPath(tex);
            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("cannot find texture asset file");
            }

            string guid = AssetDatabase.AssetPathToGUID(path);

            string basename = Path.GetFileNameWithoutExtension(path);
            string newPath = Path.Combine("textures", basename);
            string name = newPath.Replace('\\', '/');

            if (context.TextureTable.ContainsKey(name))
            {
                if (guid != context.TextureTable[name].GUID)
                {
                    PropertyInfo inspectorModeInfo = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
                    SerializedObject so = new SerializedObject(tex);
                    inspectorModeInfo.SetValue(so, InspectorMode.Debug, null);

                    SerializedProperty localIdProp = so.FindProperty("m_LocalIdentfierInFile");

                    int localId = localIdProp.intValue;

                    string newname = basename + "_" + localId;
                    Debug.LogWarning("Texture name [" + basename + "] duplicated, changed name to [" + newname + "]");

                    basename = newname;
                    newPath = Path.Combine("textures", basename);
                    name = newPath.Replace('\\', '/');

                    if (context.TextureTable.ContainsKey(name))
                    {
                        if (guid != context.TextureTable[name].GUID)
                        {
                            throw new Exception("Texture conflict: " + name);
                        }
                        else
                        {
                            return name;
                        }
                    }
                }
                else
                {
                    return name;
                }
            }

            ResourceMap.Texture texture = new ResourceMap.Texture();
            texture.Path = name;
            texture.GUID = guid;
            texture.Dimension = tex.dimension.ToString().ToLower();
            TextureImporter importer = TextureImporter.GetAtPath(path) as TextureImporter;
            switch (tex.dimension)
            {
                case UnityEngine.Rendering.TextureDimension.Tex2D:
                    if (importer.convertToNormalmap)
                    {
                        if (!importer.isReadable)
                        {
                            importer.isReadable = true;
                            importer.SaveAndReimport();
                        }
                        
                        Texture2D tex2d = tex as Texture2D;
                        TexConverter.ConvertNormalMap(tex2d, Path.Combine(context.BaseDir, newPath + ".texture"));
                    }
                    else
                    {
                        Texture2D tex2d = tex as Texture2D;
                        texture.WrapMode = tex2d.wrapMode.ToString().ToLower();
                        texture.FilterMode = tex2d.filterMode.ToString().ToLower();
                        texture.MipmapCount = tex2d.mipmapCount;
                        texture.MipmapBias = tex2d.mipMapBias;

                        TexConverter.Convert(
                            Path.Combine(Path.GetDirectoryName(Application.dataPath), path),
                            Path.Combine(context.BaseDir, newPath + ".texture"),
                            tex2d.format,
                            tex2d.width, tex2d.height,
                            tex2d.mipmapCount,
                            importer.convertToNormalmap, importer.heightmapScale * 1000);
                    }
                    break;
                case UnityEngine.Rendering.TextureDimension.Cube:
                    {
                        //Cubemap cubemap = texture as Cubemap;

                        //break;
                        throw new Exception("cubemap export: not implemented yet.");
                    }
                default:
                    throw new Exception("unsupported texture.");
            }

            context.TextureTable.Add(name, texture);

            return name;
        }
    }
}
