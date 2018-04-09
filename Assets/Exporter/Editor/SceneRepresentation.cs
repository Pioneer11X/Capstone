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

        public class PathNode
        {
            public string name;
            public string tag;
            public int index;
            public Float3 position;
            public int nearby_1 = -1;
            public int nearby_2 = -1;
            public int nearby_3 = -1;
            public int nearby_4 = -1;

            public PathNode()
            {
                name = "";
                tag = "";
                position = new Float3(Vector3.zero);
            }

            public PathNode(GameObject obj)
            {
                name = obj.name;
                tag = obj.tag;
                index = obj.GetComponent<PathingNode>().index;
                position = new Float3(obj.transform.position);
               if(obj.GetComponent<PathingNode>().connectedNodes.Count > 0)
                {
                    // I cannot believe I need a goto statement, come on C#.
                    switch(obj.GetComponent<PathingNode>().connectedNodes.Count)
                    {
                        case 4:
                            nearby_4 = obj.GetComponent<PathingNode>().connectedNodes[3].index;
                            goto case 3;
                        case 3:
                            nearby_3 = obj.GetComponent<PathingNode>().connectedNodes[2].index;
                            goto case 2;
                        case 2:
                            nearby_2 = obj.GetComponent<PathingNode>().connectedNodes[1].index;
                            goto case 1;
                        case 1:
                            nearby_1 = obj.GetComponent<PathingNode>().connectedNodes[0].index;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public class SpawnNode
        {
            public string name;
            public string tag;
            public Float3 position;
            public string trigger;
            public SpawnNode()
            {
                name = "";
                tag = "";
                trigger = null;
                position = new Float3(Vector3.zero);
            }

            public SpawnNode(GameObject obj)
            {
                name = obj.name;
                tag = obj.tag;
                position = new Float3(obj.transform.position);
            }

            public void AddChild(string name)
            {
                trigger = name;
            }
        }

        public class TriggerNode
        {
            public string name;
            public string tag;
            public Float3 position;

            public TriggerNode()
            {
                name = "";
                tag = "";
                position = new Float3(Vector3.zero);
            }

            public TriggerNode(GameObject obj)
            {
                name = obj.name;
                tag = obj.tag;
                position = new Float3(obj.transform.position);
            }
        }

        public class SpawnPoint
        {
            public Float3 position;
            public SpawnPoint()
            {
                position = new Float3(Vector3.zero);
            }

            public SpawnPoint(GameObject obj)
            {
                position = new Float3(obj.transform.position);
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

            public Float4(Color v)
            {
                x = v.r;
                y = v.g;
                z = v.b;
                w = v.a;
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
        
        [Serializable]
        public class Light : Component
        {
            public string lightType;
            public Float4 color;
            public float range;
            public float intensity;
            public float spotAngle;
            public bool castShadow;

            public Light()
            {
                type = "light";
            }
        }

        public List<Entity> entities;
        public List<SpawnNode> spawnerNodes;
        public List<TriggerNode> triggerNodes;
        public List<PathNode> pathNodes;
        public List<SpawnPoint> playerSpawn;

        public void Add(Entity entity)
        {
            if (null == entities)
                entities = new List<Entity>();
            entities.Add(entity);
        }

        public void AddPathNode(PathNode node)
        {
            if (null == pathNodes)
                pathNodes = new List<PathNode>();
            pathNodes.Add(node);
        }

        public void AddSpwanerNode(SpawnNode node)
        {
            if (null == spawnerNodes)
                spawnerNodes = new List<SpawnNode>();
            spawnerNodes.Add(node);
        }

        public void AddTriggerNode(TriggerNode node)
        {
            if (null == triggerNodes)
                triggerNodes = new List<TriggerNode>();
            triggerNodes.Add(node);
        }

        public void AddSpawnPoint(SpawnPoint node)
        {
            if (null == playerSpawn)
                playerSpawn = new List<SpawnPoint>();
            playerSpawn.Add(node);
        }

        public void SortPathNodes()
        {
            if (pathNodes == null)
            { return; }

            //for(int i = 0; i < pathNodes.Count; i++)
            //{
            //    bool added;
            //    float dist;
            //    PathNode current = pathNodes[i];
            //    PathNode closest = null;

            //    do
            //    {
            //        added = false;
            //        dist = float.MaxValue;
            //        for (int j = 0; j < pathNodes.Count; j++)
            //        {
            //            Vector3 a = new Vector3(current.position.x, current.position.y, current.position.z);
            //            Vector3 b = new Vector3(pathNodes[j].position.x, pathNodes[j].position.y, pathNodes[j].position.z);
            //            float distBetween = Vector3.Distance(a, b);
            //            Debug.Log(distBetween);
            //            if (distBetween > 0 && distBetween < 14)
            //            {
            //                if (distBetween < dist && (pathNodes[j].name != current.nearby_1 && pathNodes[j].name != current.nearby_2
            //                        && pathNodes[j].name != current.nearby_3 && pathNodes[j].name != current.nearby_4))
            //                {
            //                    dist = distBetween;
            //                    closest = pathNodes[j];
            //                    added = true;
            //                }
            //            }
            //        }

            //        if(current.nearby_1 == "" && added)
            //        {
            //            current.nearby_1 = closest.name;
            //        }
            //        else if (current.nearby_2 == "" && added && current.nearby_1 != "")
            //        {
            //            current.nearby_2 = closest.name;
            //        }
            //        else if (current.nearby_3 == "" && added && current.nearby_2 != "")
            //        {
            //            current.nearby_3 = closest.name;
            //        }
            //        else if (current.nearby_4 == "" && added && current.nearby_3 != "")
            //        {
            //            current.nearby_4 = closest.name;
            //            break;
            //        }
            //    } while (added);
            //}
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


