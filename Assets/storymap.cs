using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapLoader : MonoBehaviour
{
    public string mapFilePath = "Assets/Data/EM2000001_42.tmu"; // Path to the map file
    public string IndexFilePath = "Assets/Index/TclientO.idx";
    public string ObjectsFilePath = "Assets/Data/OBJ/Map.tob";


    private Terrain terrain;

    void Start()
    {
        StartCoroutine(LoadMapUnit(mapFilePath));  // Load the unit when the scene starts
    }

    private IEnumerator LoadMapUnit(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"File not found: {path}");
            yield break;
        }

        using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
        using (BinaryReader reader = new BinaryReader(fileStream))
        {
            try
            {
                LoadTerrainData(reader);
                LoadTextures(reader);
                LoadMapObjects(reader);
            }
            catch (IOException e)
            {
                Debug.LogError($"Error reading map file: {e.Message}");
            }
        }
        yield return null;
    }

    private void LoadTerrainData(BinaryReader reader)
    {
        //Read height data
        int dataSize = reader.ReadInt32();
        float[] heightData = new float[dataSize];
        for (int i = 0; i < dataSize; i++)
        {
            heightData[i] = reader.ReadSingle();
        }

        // Set up TerrainData
        TerrainData terrainData = new TerrainData();
        int resolution = Mathf.FloorToInt(Mathf.Sqrt(dataSize));
        terrainData.heightmapResolution = resolution;
        terrainData.size = new Vector3(1024, 255, 1024);  // Set terrain size
        terrainData.SetHeights(0, 0, ConvertToHeightmap(heightData, resolution));
        terrainData.SetHeights(0, 0, ConvertToHeightmap(heightData, resolution));

        // Create terrain in Unity and assign terrain data
        terrain = Terrain.CreateTerrainGameObject(terrainData).GetComponent<Terrain>();
        terrain.transform.position = Vector3.zero;

    }

    private void LoadTextures(BinaryReader reader)
    {
        int TextureSize = reader.ReadInt32();
        for (int i = 0; i < TextureSize; i++)
        {
            reader.ReadInt32();
        }
        reader.ReadInt32();
        reader.ReadInt32();
    }

    int LoadMeshFromPosition(int MeshPosition)
    {
        FileStream fileStreamIndex = new FileStream(ObjectsFilePath, FileMode.Open, FileAccess.Read);
        BinaryReader Indexreader = new BinaryReader(fileStreamIndex);

        for (int x = 0; x < MeshPosition; x++)
        {
            Indexreader.ReadByte();
        }

        int meshid = 0;
        int pivotcount = Indexreader.ReadByte();
        int realpivotcount = Indexreader.ReadInt32();
        for (int a = 0; a < realpivotcount; a++)
        {
            Indexreader.ReadInt32();
            Indexreader.ReadInt32();
        }

        int sfxcount = Indexreader.ReadInt32();
        for (int s = 0; s < sfxcount; s++)
        {
            Indexreader.ReadInt32();
            Indexreader.ReadInt32();
            Indexreader.ReadInt32();
            Indexreader.ReadInt32();
            Indexreader.ReadInt32();
            Indexreader.ReadInt32();
            Indexreader.ReadInt32();
            Indexreader.ReadInt32();
            Indexreader.ReadInt32();
            Indexreader.ReadInt32();
            Indexreader.ReadInt32();
            Indexreader.ReadInt32();
            Indexreader.ReadByte();
        }

        int soundcount = Indexreader.ReadInt32();
        for (int b = 0; b < soundcount; b++)
        {
            Indexreader.ReadInt32();
            Indexreader.ReadInt32();
            Indexreader.ReadInt32();
            Indexreader.ReadInt32();
            Indexreader.ReadInt32();
        }

        int attributescount = Indexreader.ReadInt32();
        for (int c = 0; c < attributescount; c++)
        {
            Indexreader.ReadInt32();
            Indexreader.ReadInt32();
            Indexreader.ReadInt32();
            Indexreader.ReadByte();
        }

        int actioncount = Indexreader.ReadInt32();
        for (int ac = 0; ac < actioncount; ac++)
        {
            Indexreader.ReadInt32();
            int secondactioncount = Indexreader.ReadInt32();
            if (secondactioncount > 0)
            {
                for (int secac = 0; secac < secondactioncount; secac++)
                {
                    Indexreader.ReadInt32();
                    Indexreader.ReadInt32();
                    Indexreader.ReadInt32();
                    Indexreader.ReadInt32();
                    Indexreader.ReadInt32();
                    Indexreader.ReadInt32();
                    Indexreader.ReadInt32();
                    Indexreader.ReadInt32();
                    Indexreader.ReadInt32();
                    Indexreader.ReadInt32();
                    Indexreader.ReadInt32();
                    Indexreader.ReadInt32();
                    Indexreader.ReadByte();

                    int actionsfxcount = Indexreader.ReadInt32();
                    for (int actionsfxcountindex = 0; actionsfxcountindex < actionsfxcount; actionsfxcountindex++)
                    {
                        Indexreader.ReadInt32();
                        Indexreader.ReadInt32();
                        Indexreader.ReadInt32();
                        Indexreader.ReadInt32();
                    }
                }
            }
            else
            {
                Indexreader.ReadInt32();
                Indexreader.ReadInt32();
            }
        }

        int ClKindCount = Indexreader.ReadInt32();
        for (int d = 0; d < ClKindCount; d++)
        {
            Indexreader.ReadInt32();
        }

        int MapClothCount = Indexreader.ReadInt32();
        for (int e = 0; e < MapClothCount; e++)
        {
            Indexreader.ReadInt32();
        }

        int MapMeshCount = Indexreader.ReadInt32();
        for (int f = 0; f < 1; f++)
        {
            Indexreader.ReadInt32();
            meshid = Indexreader.ReadInt32();
        }

        return meshid;
    }

    int LoadMeshID(int IndexID, BinaryReader Indexreader)
    {
        int currentposition = 0;
        int meshindexid = 0;
        int meshposition = 0;
        int IndexedObjects = Indexreader.ReadInt32();
        for (int a = 0; a < 27; a++)
        {
            Indexreader.ReadInt32();
        }

        while (meshindexid == 0)
        {
            currentposition = Indexreader.ReadInt32();
            if (currentposition == IndexID)
            {
                meshindexid = currentposition;
                Indexreader.ReadInt32();
                meshposition = Indexreader.ReadInt32();
            }
        }

        return meshposition;
    }


    private void LoadMapObjects(BinaryReader reader)
    {
        int objectCount = reader.ReadInt32();
        for (int i = 0; i < objectCount; i++)
        {
            int instanceID = reader.ReadInt32();
            int objectID = reader.ReadInt32();
            int meshposition = 0;
            int meshobjid = 0;

            Vector3 scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Vector3 position = new Vector3(reader.ReadSingle() - 2048f, reader.ReadSingle(), reader.ReadSingle() - 4096f);
            Vector3 rotation = new Vector3(reader.ReadSingle()/0.0174f, reader.ReadSingle()/0.0174f, reader.ReadSingle()*0f);

            using (FileStream fileStreamIndex = new FileStream(IndexFilePath, FileMode.Open, FileAccess.Read))
            using (BinaryReader Indexreader = new BinaryReader(fileStreamIndex))
            {
                try
                {
                    meshposition = LoadMeshID(objectID, Indexreader);
                    meshobjid = LoadMeshFromPosition(meshposition);
                }
                catch (IOException e)
                {
                    Debug.LogError($"Error reading map file: {e.Message}");
                }
            }

            for (int j = 0; j < 5; j++)
                reader.ReadInt32();

            int nCICount = reader.ReadInt32();
            if (nCICount > 0)
                for (int a = 0; a < nCICount; a++)
                {
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                }


            int nATTR = reader.ReadInt32();
            if (nATTR > 0)
                for (int b = 0; b < nATTR; b++)
                {
                    reader.ReadInt32();
                    reader.ReadByte();
                    int pATTR = reader.ReadInt32();
                    if (pATTR > 0)
                        for (int x = 0; x < pATTR; x++)
                            reader.ReadByte();
                }

                GameObject obj = CreateMapObject(instanceID, scale, position, rotation, meshobjid);
            //obj.name = $"Object_{instanceID}_{meshposition}_{meshobjid}";
            obj.name = $"Object_{meshobjid}_{meshposition}";

        }
    }

    private GameObject CreateMapObject(int instanceID, Vector3 scale, Vector3 position, Vector3 rotation, int meshid)
    {

        GameObject obj;
        if (meshid == 0 || meshid == 1 || meshid == 16777311 || meshid == 2316487 || meshid == 2316127 || meshid == 2315900 || meshid == 2316548 || meshid == 2001049 || meshid == 2001113 || meshid == 2315988 || meshid == 2316125 || meshid == 2318732 || meshid == 2000995 || meshid == 2315984 || meshid == 2316418 || meshid == 2316110
            || meshid == 2001116 || meshid == 2001119 || meshid == 2001120 || meshid == 2001114 || meshid == 2001118 || meshid == 2001051 || meshid == 2000827 || meshid == 2001029)
             obj = GameObject.CreatePrimitive(PrimitiveType.Cube); // Replace cube with the desired object type
        else
        {
            string prefab = $"{meshid}";
            if(Resources.Load(prefab))
             obj = Instantiate(Resources.Load(prefab) as GameObject);
            else
                obj = GameObject.CreatePrimitive(PrimitiveType.Cube); // Replace cube with the desired object type
        }
        obj.transform.localScale = scale;
        obj.transform.position = position;
        obj.transform.eulerAngles = rotation;

        return obj;
    }

    private float[,] ConvertToHeightmap(float[] flatData, int resolution)
    {
        float[,] heightmap = new float[resolution, resolution];
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                heightmap[i, j] = flatData[i * resolution + j]/255;
                Debug.Log(flatData[i * resolution + j]);
            }
        }
        return heightmap;
    }


}