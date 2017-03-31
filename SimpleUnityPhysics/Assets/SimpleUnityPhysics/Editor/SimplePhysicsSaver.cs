using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.IO;
namespace SimpleUnityPhysics
{
    [Serializable]
    public class Vec3
    {
        [JsonProperty("x")]
        public float x { get; set; }

        [JsonProperty("y")]
        public float y { get; set; }

        [JsonProperty("z")]
        public float z { get; set; }

        public Vec3()
        {

        }

        public Vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vec3(Vector3 vec3)
        {
            this.x = vec3.x;
            this.y = vec3.y;
            this.z = vec3.z;
        }

        public Vector3 Get()
        {
            return new Vector3(x, y, z);
        }
    }

    [Serializable]
    public class Quat
    {
        [JsonProperty("x")]
        public float x { get; set; }

        [JsonProperty("y")]
        public float y { get; set; }

        [JsonProperty("z")]
        public float z { get; set; }

        [JsonProperty("w")]
        public float w { get; set; }


        public Quat()
        {

        }

        public Quat(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Quat(Quaternion quat)
        {
            this.x = quat.x;
            this.y = quat.y;
            this.z = quat.z;
            this.w = quat.w;
        }

        public Quaternion Get()
        {
            return new Quaternion(x, y, z, w);
        }
    }

    [Serializable]
    public class Transfm
    {
        [JsonProperty("rotation")]
        public Quat rotation { get; set; }

        [JsonProperty("position")]
        public Vec3 position { get; set; }

        [JsonProperty("scale")]
        public Vec3 scale { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        public Transfm()
        {

        }

        public Transfm(Transform transform)
        {
            rotation = new Quat(transform.rotation);
            position = new Vec3(transform.position);
            scale = new Vec3(transform.localScale);
            name = transform.name;
        }

        public void Set(Transform transform)
        {
            transform.rotation = rotation.Get();
            transform.localScale = scale.Get();
            transform.position = position.Get();
            transform.name = name;
        }
    }

    [Serializable]
    public class SavedEverything
    {
        [JsonProperty("savedBoxes")]
        SavedBox[] savedBoxes { get; set; }

        [JsonProperty("savedBodies")]
        SavedBody[] savedBodies { get; set; }

        [JsonProperty("timeName")]
        public string timeName { get; set; }

        [JsonProperty("version")]
        public string version { get; set; }

        [JsonProperty("dt")]
        public float dt { get; set; }

        [JsonProperty("itersPerFrame")]
        public int itersPerFrame { get; set; }

        [JsonProperty("collisionIters")]
        public int collisionIters { get; set; }

        [JsonProperty("jointIters")]
        public int jointIters { get; set; }

        [JsonProperty("populationSize")]
        public int populationSize { get; set; }

        [JsonProperty("perturb")]
        public float perturb { get; set; }

        [JsonProperty("gravity")]
        public Vec3 gravity { get; set; }

        [JsonProperty("keepingDist")]
        public float keepingDist { get; set; }

        [JsonProperty("numSteps")]
        public int numSteps { get; set; }

        [JsonProperty("numBestKeeping")]
        public int numBestKeeping { get; set; }

        [JsonProperty("mutants")]
        public int mutants { get; set; }

        public SavedEverything()
        {

        }

        public SavedEverything(SimplePhysics time)
        {

            timeName = time.name;
            dt = time.dt;
            itersPerFrame = time.itersPerFrame;
            collisionIters = time.collisionIters;
            jointIters = time.jointIters;
            gravity = new Vec3(time.gravity);

            version = "v1";


            List<SavedBox> savedBoxes = new List<SavedBox>();
            List<SavedBody> savedBodies = new List<SavedBody>();
            List<SimpleRigidbody3D> savedRigidbodies = new List<SimpleRigidbody3D>();
            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

            foreach (GameObject go in allObjects)
            {
                // From http://answers.unity3d.com/questions/329395/how-to-get-all-gameobjects-in-scene.html
                if (go.activeInHierarchy)
                {
                    SimpleRigidbody3D simpleRigid = go.GetComponent<SimpleRigidbody3D>();

                    if (simpleRigid != null)
                    {
                        simpleRigid.id = savedRigidbodies.Count;
                        savedRigidbodies.Add(simpleRigid);
                    }
                }
            }

            foreach (SimpleRigidbody3D rigid in savedRigidbodies)
            {
                savedBodies.Add(new SavedBody(rigid.gameObject));
            }

            foreach (GameObject go in allObjects)
            {
                if (go.activeInHierarchy)
                {
                    if (go.GetComponent<SimpleRigidbody3D>() == null && go.GetComponent<BoxCollider>() != null)
                    {
                        savedBoxes.Add(new SavedBox(go));
                    }
                }
            }

            this.savedBoxes = savedBoxes.ToArray();
            this.savedBodies = savedBodies.ToArray();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static SavedEverything FromJson(string jsonString)
        {
            return JsonConvert.DeserializeObject<SavedEverything>(jsonString);
        }

        public void LoadToScene()
        {
            if (version == "v1")
            {
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

                foreach (GameObject go in allObjects)
                {
                    // From http://answers.unity3d.com/questions/329395/how-to-get-all-gameobjects-in-scene.html
                    if (go.activeInHierarchy)
                    {
                        if (go.GetComponent<BoxCollider>() != null || go.GetComponent<SimplePhysics>() != null || go.GetComponent<SimpleRigidbody3D>() != null)
                        {
                            GameObject.DestroyImmediate(go);
                        }
                    }
                }

                foreach (SavedBox savedBox in savedBoxes)
                {
                    GameObject newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    savedBox.transform.Set(newCube.transform);
                }

                Dictionary<int, SimpleRigidbody3D> simpleRigidbodiesDict = new Dictionary<int, SimpleRigidbody3D>();
                Dictionary<int, SavedBody> savedBodiesDict = new Dictionary<int, SavedBody>();
                List<int> ids = new List<int>();

                foreach (SavedBody savedBody in savedBodies)
                {
                    GameObject newBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    savedBody.transform.Set(newBody.transform);
                    SimpleRigidbody3D newRigid = newBody.AddComponent<SimpleRigidbody3D>();
                    newRigid.id = savedBody.id;
                    simpleRigidbodiesDict[newRigid.id] = newRigid;
                    savedBodiesDict[newRigid.id] = savedBody;
                    ids.Add(newRigid.id);
                }

                foreach (int id in ids)
                {
                    foreach (SavedBone bone in savedBodiesDict[id].bones)
                    {
                        SimpleRigidbody3D body = simpleRigidbodiesDict[id];
                        Bone newBone = body.gameObject.AddComponent<Bone>();
                        newBone.other = simpleRigidbodiesDict[bone.idOther];
                        newBone.creatureUsingMe = bone.usableByCreature;
                    }
                }

                GameObject timeObject = new GameObject();
                timeObject.name = timeName;



                SimplePhysics time = timeObject.AddComponent<SimplePhysics>();

                time.dt = dt;
                time.itersPerFrame = itersPerFrame;
                time.collisionIters = collisionIters;
                time.jointIters = jointIters;
                time.gravity = gravity.Get();
            }


            if (GameObject.FindObjectOfType<Light>() == null)
            {
                GameObject lightObject = new GameObject();
                lightObject.name = "Sun";
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.transform.localEulerAngles = new Vector3(50, -30, 0);
                light.transform.position = new Vector3(100, 100, 100);
            }
        }
    }

    [Serializable]
    public class SavedThing
    {
        [JsonProperty("transform")]
        public Transfm transform { get; set; }
    }

    [Serializable]
    public class SavedBox : SavedThing
    {
        public SavedBox()
        {

        }

        public SavedBox(GameObject box)
        {
            transform = new Transfm(box.transform);
        }
    }

    [Serializable]
    public class SavedBone
    {

        [JsonProperty("usableByCreature")]
        public bool usableByCreature { get; set; }

        [JsonProperty("idMe")]
        public int idMe { get; set; }

        [JsonProperty("idOther")]
        public int idOther { get; set; }

        public SavedBone()
        {

        }


        public SavedBone(Bone bone)
        {
            usableByCreature = bone.creatureUsingMe;
            idMe = bone.GetComponent<SimpleRigidbody3D>().id;
            idOther = bone.other.id;
        }
    }

    public class SavedBody : SavedThing
    {
        [JsonProperty("bones")]
        public SavedBone[] bones { get; set; }

        [JsonProperty("timesteps")]
        public int timesteps { get; set; }

        [JsonProperty("rangeModifier")]
        public float rangeModifier { get; set; }

        [JsonProperty("frameUpdate")]
        public int frameUpdate { get; set; }


        [JsonProperty("id")]
        public int id { get; set; }

        public SavedBody()
        {

        }

        public SavedBody(GameObject node)
        {
            transform = new Transfm(node.transform);

            SimpleRigidbody3D body = node.GetComponent<SimpleRigidbody3D>();
            id = body.id;

            List<SavedBone> bones = new List<SavedBone>();

            foreach (Bone bone in node.GetComponents<Bone>())
            {
                bones.Add(new SavedBone(bone));
            }
            this.bones = bones.ToArray();
        }
    }


    [CustomEditor(typeof(SimplePhysics))]
    public class SimplePhysicsSaver : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SimplePhysics myScript = (SimplePhysics)target;
            if (GUILayout.Button("Save Configuration"))
            {
                SavedEverything everything = new SavedEverything(myScript);
                string stringSaving = everything.ToJson();

                string path = EditorUtility.SaveFilePanel("Save objects", "", "newConfig.json", "json");
                if (path.Length != 0)
                {
                    Debug.Log("Saving to path: " + path);
                    File.WriteAllText(path, stringSaving);
                }
            }
            if (GUILayout.Button("Load Configuration"))
            {
                string path = EditorUtility.OpenFilePanel("Load configuration", "", "json");
                if (path.Length != 0)
                {
                    Debug.Log(path);
                    string jsonString = File.ReadAllText(path);

                    try
                    {
                        SavedEverything configLoading = SavedEverything.FromJson(jsonString);

                        configLoading.LoadToScene();
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Failed to load json");
                        throw e;
                    }
                }
            }
        }
    }
}