using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Map : MonoBehaviour {

    public float[,] noise;              // the heightvalues obtained by perlin noise

    // all these values are set in the menu by the player
    public int width;                   // width of the map
    public int length;                  // length of the map
    public float scale;                 // scale of the map. Lower scale leads to more mountains and water
    public float height;                // height of the map. lower is more water, higher is more mountains
    public float heightScale;           // difference in height. Higher value is bigger mountains and hills

    private MeshFilter MF;              // the meshfilter
    private MeshRenderer MR;            // the meshrenderer

    public GameObject wayPoint;         // the waypoints that will be spawned for finding the way
    public GameObject character;        // the character, looking for loot
    public GameObject cactus;           // cacti. for visual purposes and functions as obstacle to avoid
    public GameObject mineral;          // minerals. will be gathered by the player

    // sets the values from the gamemanager
    void Start()
    {
        width = GameManager.size;
        length = GameManager.size;
        scale = GameManager.scale;
        height = GameManager.height;
        heightScale = GameManager.heightScale;

        MF = GetComponent<MeshFilter>();
        MR = GetComponent<MeshRenderer>();

        // set the noisemap, and use it to generate terrain
        noise = NoiseMap();
        sineMap();
        GenerateTerrain();
    } 

    // using perlin noise to create a noisemap. different values give different result, and randomizers are added for more variety
    float[,] NoiseMap()
    {
        noise = new float[width, length];

        float randomXstart = Random.Range(-1f, 1f);
        float randomYstart = Random.Range(-1f, 1f);
        float randomXmultiplier = Random.Range(0.5f, 1.5f);
        float randomYmultiplier = Random.Range(0.5f, 1.5f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                float xValue = (randomXstart + x / scale) * randomXmultiplier;
                float yValue = (randomYstart + y / scale) * randomYmultiplier;

                float PN = Mathf.PerlinNoise(xValue, yValue) * height;
                noise[x, y] = PN;
            }
        }
        return noise;
    }

    // this part makes sure that the map will be island shaped, in order to prevent small islands with minerals on the edge of the map, which cannot be reached
    // This is done by adding a sinus curve to the heightmap  
    void sineMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                float xSine = Mathf.Sin(x * Mathf.PI / width);
                float ySine = Mathf.Sin(y * Mathf.PI / width);
                float combindedSine = (xSine + ySine);

                noise[x, y] -= (combindedSine - 1) / 2;
            }
        }
    }

    // generates the color of the terrain depending on height. Will also create monument valley-like mountains/vulcanos 
    // Additionaly, spawns the waypoints at the right pos, and give it the proper height value and make sure if its walkable
    void GenerateTerrain()
    {
        Texture2D _texture = new Texture2D(width, length);
        _texture.wrapMode = TextureWrapMode.Clamp;

        Color[] colors = new Color[width * length];
        for (int w = 0; w < width; w++)
        {
            for (int l = 0; l < length; l++)
            {
                GameObject WP = Instantiate(wayPoint, new Vector3(), Quaternion.identity) as GameObject;
                if (noise[l, w] < 0.1)
                {
                    colors[w * length + l] = Color.red; // vulcano
                    float lavaHeight = 0.1f;
                    noise[l, w] = lavaHeight;
                }
                else if (noise[l, w] < 0.2)
                {
                    colors[w * length + l] = Color.gray; //mountain
                    float reverseValue = 0.2f - noise[l, w];
                    noise[l, w] = reverseValue;
                }
                // this part is added to make sure the edge of the mountains is also unwalkable. prevents the character from moving through the base of the wall
                else if (noise[l, w] < 0.21)
                {
                    colors[w * length + l] = new Color(0.5f, 0.2f, 0.2f); // brownish desert like
                }
                else if (noise[l, w] < 0.7)
                {
                    colors[w * length + l] = new Color(0.5f, 0.2f, 0.2f); // brownish desert like
                    WP.GetComponent<Waypoint>().walkable = true;
                }
                else if (noise[l, w] <= 0.9)
                {
                    colors[w * length + l] = new Color(0.9f, 0.8f, 0.2f);  // sand
                    WP.GetComponent<Waypoint>().walkable = true;
                }
                else
                {
                    colors[w * length + l] = Color.blue; //water 
                    noise[l, w] = 0.9f;
                }
                WP.transform.position = new Vector3(l, -noise[l, w] * heightScale, w);
                GameManager.waypoints[l, w] = WP; // add the waypoint to the waypoint array
            }
        }
        _texture.SetPixels(colors);
        _texture.Apply();

        DrawPolygon(MeshGeneration.Generate(noise, heightScale), _texture);

        SpawnObjects();
    }

    // uses the meshgeneration to create vertices and data for the meshes
    void DrawPolygon(TriangleData PD, Texture2D _texture)
    {
        MF.sharedMesh = PD.CreateMesh();
        MR.sharedMaterial.mainTexture = _texture;
    }

    // spawn the character, cactuses and minerals
    void SpawnObjects()
    {
        int cactusAmount = width;
        for (int i = 0; i < cactusAmount; i++)
        {
            GameObject _cactus = Instantiate(cactus) as GameObject;
            _cactus.GetComponent<Cactus>().mapScript = this;
        }
        for (int i = 0; i < GameManager.mineralAmount; i++)
        {
            GameObject _mineral = Instantiate(mineral) as GameObject;
            _mineral.GetComponent<Mineral>().mapScript = this;
        }

        GameObject _char = Instantiate(character) as GameObject;
        _char.GetComponent<Character>().mapScript = this;
        GameManager.character = _char;
    }
}
