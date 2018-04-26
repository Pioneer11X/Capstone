using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using Newtonsoft.Json;

namespace FriedTofu
{
    [Serializable]
    public class ResourceMap
    {
        public enum MaterialType
        {
            Opaque,
            Cutoff,
            Transparent,
            Additive
        }

        public static MaterialType GetMaterialType(UnityEngine.Material mat)
        {
            if (mat.renderQueue < 2450)
            {
                return MaterialType.Opaque;
            }
            else if (mat.renderQueue < 3000)
            {
                return MaterialType.Cutoff;
            }
            else if (mat.shader.name.ToLower().Contains("additive"))
            {
                return MaterialType.Additive;
            }
            else
            {
                return MaterialType.Transparent;
            }
        }

        [Serializable]
        public class Material
        {
            public string Name;
            public string GUID;
            public string Type;
            public string AlbedoMap;
            public string NormalMap;
            public string MetallicGlossMap;
            public string OcclusionMap;
            public string EmissionMap;
            public Color TintColor;
            public Color EmissionColor; 
            public Vector4 TextureScaleOffset;
            public float Cutoff;
        }

        [Serializable]
        public class Texture
        {
            public string Path;
            public string GUID;
            public string Dimension;
            public string WrapMode;
            public string FilterMode;
            public int MipmapCount;
            public float MipmapBias;
        }

        [Serializable]
        public class Model
        {
            public string name;
            public string guid;
        }

        public string BaseDir { get; private set; }

        public Dictionary<string, string> MeshTable { get; private set; }
        public Dictionary<string, Material> MaterialTable { get; private set; }
        public Dictionary<string, Texture> TextureTable { get; private set; }

        // serailized field
        public List<Model> models;
        public List<Material> materials;
        public List<Texture> textures;
        

        public ResourceMap(string path)
        {
            BaseDir = path;
            Load();
        }

        private void Load()
        {
            MeshTable = new Dictionary<string, string>();
            MaterialTable = new Dictionary<string, Material>();
            TextureTable = new Dictionary<string, Texture>();

            string resFile = Path.Combine(BaseDir, "res.json");
            string sceneFolder = Path.Combine(BaseDir, "scenes");
            string modelFolder = Path.Combine(BaseDir, "models");
            string textureFolder = Path.Combine(BaseDir, "textures");

            if (!Directory.Exists(sceneFolder))
            {
                Directory.CreateDirectory(sceneFolder);
            }

            if (!Directory.Exists(modelFolder))
            {
                Directory.CreateDirectory(modelFolder);
            }

            if (!Directory.Exists(textureFolder))
            {
                Directory.CreateDirectory(textureFolder);
            }

            if (File.Exists(resFile))
            {
                string json = File.ReadAllText(resFile);
                EditorJsonUtility.FromJsonOverwrite(json, this);
            }
            else
            {
                models = new List<Model>();
                materials = new List<Material>();
                textures = new List<Texture>();
            }

            foreach (Model model in models)
            {
                MeshTable.Add(model.name, model.guid);
            }

            foreach (Material mat in materials)
            {
                MaterialTable.Add(mat.Name, mat);
            }

            foreach (Texture tex in textures)
            {
                TextureTable.Add(tex.Path, tex);
            }
        }

        public void Save()
        {
            string resFile = Path.Combine(BaseDir, "res.json");

            models = new List<Model>();
            foreach (string k in MeshTable.Keys)
            {
                models.Add(new Model() { name = k, guid = MeshTable[k] });
            }

            materials = new List<Material>();
            foreach (string k in MaterialTable.Keys)
            {
                materials.Add(MaterialTable[k]);
            }

            textures = new List<Texture>();
            foreach (string k in TextureTable.Keys)
            {
                textures.Add(TextureTable[k]);
            }

            File.WriteAllText(resFile, EditorJsonUtility.ToJson(this, true));
        }
    }
}
