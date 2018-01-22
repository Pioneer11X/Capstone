using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace FriedTofu
{
    public class SceneExporter
    {
        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 40)]
        [Serializable]
        public struct ModelHeader
        {
            [FieldOffset(0)] public uint Magic;
            [FieldOffset(4)] public uint Version;
            [FieldOffset(8)] public byte Flags;
            [FieldOffset(9)] public byte Reserved0;
            [FieldOffset(10)] public byte Reserved1;
            [FieldOffset(11)] public byte NumTexcoordChannels;
            [FieldOffset(12)] public uint NumMeshes;
            [FieldOffset(16)] public uint NumBones;
            [FieldOffset(20)] public uint NumAnimations;
            [FieldOffset(24)] public uint NumAnimChannels;
            [FieldOffset(28)] public uint NumTotalTranslationFrames;
            [FieldOffset(32)] public uint NumTotalRotationFrames;
            [FieldOffset(36)] public uint NumTotalScaleFrames;

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
                writer.Write(NumMeshes);
                writer.Write(NumBones);
                writer.Write(NumAnimations);
                writer.Write(NumAnimChannels);
                writer.Write(NumTotalTranslationFrames);
                writer.Write(NumTotalRotationFrames);
                writer.Write(NumTotalScaleFrames);
            }
        }
        
        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
        [Serializable]
        public struct SubMesh
        {
            [FieldOffset(0)] public uint NumVertices;
            [FieldOffset(4)] public uint NumIndices;

            public void WriteTo(BinaryWriter writer)
            {
                writer.Write(NumVertices);
                writer.Write(NumIndices);
            }
        }

        private static string lastPath = null;
        

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

        [MenuItem("Tools/Set TexConv Path")]
        static void SetDXTexConv()
        {
            string path = EditorUtility.OpenFilePanel(
                "Select texconv.exe", null, "exe");

            if (string.IsNullOrEmpty(path))
                return;

            TexConverter.DXTexConvPath = path;
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
            string path = EditorUtility.SaveFolderPanel("Select output assets folder", lastPath, null);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            lastPath = path;
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
            }

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

            if (obj.tag == "MainCamera")
                return null;

            Scene.Entity entity = new Scene.Entity(obj);

            string type = "unknown";

            if (null != obj.GetComponent<Light>())
            {
                type = "light";
            }
            else if (null != obj.GetComponent<MeshRenderer>())
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
            
            Collider collider = obj.GetComponent<Collider>();
            if (null != collider)
            {
                if (null != collider as BoxCollider)
                {
                    BoxCollider box = collider as BoxCollider;
                    Scene.PhysicsComponent comp = new Scene.PhysicsComponent();
                    comp.colliderType = "box";
                    comp.center = new Scene.Float3(box.center);
                    comp.size = new Scene.Float3(box.size);
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
            if (string.IsNullOrEmpty(originalPath))
            {
                throw new Exception("Cannot find mesh asset.");
            }

            string guid = AssetDatabase.AssetPathToGUID(originalPath);

            string basename = Path.GetFileNameWithoutExtension(originalPath);

            if (basename == "unity default resources")
            {
                basename = "Unity_Builtin_" + mesh.name;
            }
            
            string newPath = Path.Combine("models", basename + ".model");

            newPath = newPath.Replace('\\', '/');

            // check if there are dupplicated filenames
            if (context.MeshTable.ContainsKey(basename))
            {
                if (guid != context.MeshTable[basename])
                {
                    throw new Exception("Duplicated model file name.");
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

                    int verticesCount = 0;

                    // writer list of sub meshes
                    for (int i = 0; i < mesh.subMeshCount; i++)
                    {
                        int[] indices = mesh.GetIndices(i);

                        if (indices.Length <= 0 || indices.Length % 3 != 0)
                        {
                            throw new Exception("Mesh not supported.");
                        }

                        int maxIndex = indices[0];
                        int minIndex = indices[0];
                        for (int j = 0; j < indices.Length; j++)
                        {
                            if (indices[j] > maxIndex)
                                maxIndex = indices[j];
                            if (indices[j] < minIndex)
                                minIndex = indices[j];

                            int finalIndex = indices[j] - verticesCount;
                            if (finalIndex < 0 || finalIndex > ushort.MaxValue)
                            {
                                throw new Exception("Mesh not supported.");
                            }

                            finalIndices.Add((ushort)finalIndex);
                        }

                        if (minIndex != verticesCount)
                        {
                            throw new Exception("Mesh not supported.");
                        }

                        startVertexLocs[i] = (uint)minIndex;
                        int verts = maxIndex - minIndex + 1;

                        submeshes[i].NumVertices = (uint)verts;
                        submeshes[i].NumIndices = (uint)indices.Length;

                        writer.Write(submeshes[i].NumVertices);
                        writer.Write(submeshes[i].NumIndices);

                        verticesCount += verts;
                    }

                    if (verticesCount != mesh.vertexCount)
                    {
                        throw new Exception("Mesh not supported.");
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
                        Vector3 tangent = mesh.tangents[i];
                        writer.Write(tangent.x);
                        writer.Write(tangent.y);
                        writer.Write(tangent.z);

                        // uv
                        Vector2 uv = mesh.uv[i];
                        writer.Write(uv.x);
                        writer.Write(uv.y);
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
                    throw new Exception("Duplicated material file name.");
                }
                else
                {
                    return basename;
                }
            }

            ResourceMap.Material material = new ResourceMap.Material();
            material.Name = basename;
            material.GUID = guid;
            material.AlbedoMap = null;
            material.NormalMap = null;

            if (mat.HasProperty("_MainTex"))
            {
                string albedoMap = ExportTexture(mat.GetTexture("_MainTex"), context);
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
                    throw new Exception("Duplicated texture file name.");
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
                    {
                        Texture2D tex2d = tex as Texture2D;
                        texture.WrapMode = tex2d.wrapMode.ToString().ToLower();
                        texture.FilterMode = tex2d.filterMode.ToString().ToLower();
                        texture.MipmapCount = tex2d.mipmapCount;
                        texture.MipmapBias = tex2d.mipMapBias;
                        
                        TexConverter.Convert(
                            Path.Combine(Path.GetDirectoryName(Application.dataPath), path), 
                            Path.Combine(context.BaseDir, newPath + ".texture"), 
                            tex2d.format, tex2d.mipmapCount, 
                            importer.convertToNormalmap, importer.heightmapScale * 1000);
                        
                        break;
                    }
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
