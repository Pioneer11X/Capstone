using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;

namespace FriedTofu
{
    public class Scene
    {
        public class Entity
        {
            public string name;
            public Transform transform;
            public List<Component> components;
            public List<Entity> children;

            // create an empty entity
            public Entity()
            {
                name = "";
                transform = new Transform();
            }

            // create a representation for gameobject
            public Entity(GameObject obj)
            {
                name = obj.name;
                transform = new Transform(obj.transform);
            }

            // Add a component
            public void Add(Component component)
            {
                if (null == components)
                    components = new List<Component>();
                components.Add(component);
            }

            // Add a child entity
            public void Add(Entity entity)
            {
                if (null == children)
                    children = new List<Entity>();
                children.Add(entity);
            }
        }

        public class Node
        {
            public string tag;
            public Transform transform;

            public Node()
            {
                tag = "";
                transform = new Transform();
            }

            public Node(GameObject obj)
            {
                tag = obj.name;
                transform = new Transform(obj.transform);
            }
        }

        public struct Float3
        {
            public float x, y, z;
            
            public Float3(float x, float y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public Float3(Vector3 v)
            {
                x = v.x;
                y = v.y;
                z = v.z;
            }
        }

        public struct Float4
        {
            public float x, y, z, w;

            public Float4(float x, float y, float z, float w)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.w = w;
            }

            public Float4(Vector4 v)
            {
                x = v.x;
                y = v.y;
                z = v.z;
                w = v.w;
            }

            public Float4(Quaternion v)
            {
                x = v.x;
                y = v.y;
                z = v.z;
                w = v.w;
            }
        }

        public class Transform
        {
            public Float3 translate;
            public Float4 rotate;
            public Float3 scale;

            public Transform()
            {
                translate = new Float3(Vector3.zero);
                rotate = new Float4(Quaternion.identity);
                scale = new Float3(Vector3.one);
            }

            public Transform(UnityEngine.Transform t)
            {
                translate = new Float3(t.localPosition);
                rotate = new Float4(t.localRotation);
                scale = new Float3(t.localScale);
            }
        }

        [Serializable]
        public class Component
        {
            public string type;
        }

        [Serializable]
        public class Renderable : Component
        {
            public string model;
            public List<string> materials;

            public Renderable()
            {
                type = "renderable";
            }

            // add a material
            public void Add(string material)
            {
                if (null == materials)
                    materials = new List<string>();
                materials.Add(material);
            }
        }

        [Serializable]
        public class PhysicsComponent : Component
        {
            public string colliderType;
            public Float3 center;
            public Float3 size;
            public string model;

            public PhysicsComponent()
            {
                type = "physics";
                size = new Float3(new Vector3(1, 1, 1));
            }
        }
        

        public List<Entity> entities;
        public List<Node> nodes;

        public void Add(Entity entity)
        {
            if (null == entities)
                entities = new List<Entity>();
            entities.Add(entity);
        }

        public void AddNode(Node node)
        {
            if (null == nodes)
                nodes = new List<Node>();
            nodes.Add(node);
        }

        public void Save(string filename, bool compact = true)
        {
            File.WriteAllText(filename,
                JsonConvert.SerializeObject(this,
                compact ? Formatting.None : Formatting.Indented)
                );
        }
    }
}


