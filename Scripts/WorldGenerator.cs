using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour
{
    // Public

    [Header("World Settings")]

    public int width;
    public int height;

    public float xOrg;
    public float yOrg;

    public float scale;
    public string seed;

    [Space(10)]

    public GeneralBiome[] genBiomes;
    public LocalizedBiome[] localBiomes;

    [Space(10)]
    [Header("UI")]

    public GameObject widthField;
    public GameObject heightField;
    public GameObject seedField;

    // Hidden

    public Dictionary<Tile.Type, Material> biomeMaterials;

    // Private

    private int seedInt;
    private System.Random rand;
    private Tile[,] terrainTiles;
    private float[,] terrainHeight;

    // Functions

    public void GenerateWorld ()
    {
        ClearWorld();
        ParseWorldSettings();
        StartCoroutine(Generate());
    }

    public IEnumerator Generate ()
    {
        terrainTiles = new Tile[width, height];
        terrainHeight = new float[width, height];
        biomeMaterials = new Dictionary<Tile.Type, Material>();

        SetSeed();
        GenerateTerrainHeight();
        PlaceTiles();

        yield return new WaitForSeconds(0.1f);

        GenerateBiomes();
    }

    private void ClearWorld ()
    {
        if (transform.childCount == 0)
            return;

        for (int i = 0; i < transform.childCount; i++)
            Destroy(transform.GetChild(i).gameObject);
    }

    private void ParseWorldSettings ()
    {
        if (!int.TryParse(widthField.GetComponentInChildren<TMPro.TMP_InputField>().text, out width))
            width = 32;

        if (!int.TryParse(heightField.GetComponentInChildren<TMPro.TMP_InputField>().text, out height))
            height = 32;

        if (int.TryParse(seedField.GetComponentInChildren<TMPro.TMP_InputField>().text, out seedInt))
            seed = seedField.GetComponentInChildren<TMPro.TMP_InputField>().text;

        else 
            seed = "";
    }

    private void SetSeed ()
    {
        if (seed == "")
        {
            var t = System.DateTime.Now;
            seed = "0" + Mathf.RoundToInt(t.Day * t.Second * Time.deltaTime % t.Millisecond * t.Second).ToString();
            seedInt = int.Parse(seed);
        }

        rand = new System.Random(seedInt);
        xOrg = rand.Next(-seedInt, seedInt);
        yOrg = rand.Next(-seedInt, seedInt);
    }

    private void GenerateTerrainHeight ()
    {
        float y = 0;

        while (y < height)
        {
            float x = 0;

            while (x < width)
            {
                float xCoord = xOrg + x / width * scale;
                float yCoord = yOrg + y / height * scale;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                terrainHeight[(int)x, (int)y] = sample;
                x++;
            }

            y++;
        }
    }

    private void PlaceTiles ()
    {
        GameObject tile = genBiomes[0].tile;

        for (int y = 0; y < height; y++) 
        {
            for (int x = 0; x < width; x++)
            {
                foreach (GeneralBiome b in genBiomes)
                {
                    if (terrainHeight[x, y] >= b.minElevation && terrainHeight[x, y] < b.maxElevation)
                    {
                        tile = b.tile;
                        break;
                    }
                }

                terrainTiles[x, y] = Instantiate(tile, new Vector3(y % 2 == 0 ? x : x + 0.5f, 0, y * 0.87f), Quaternion.Euler(-90f, 0f, 0f), transform).GetComponent<Tile>();
                terrainTiles[x, y].SetCoords(x, y);
            }
        }
    }

    private async void GenerateBiomes ()
    {
        foreach (Biome b in genBiomes)
            biomeMaterials.Add(b.type, b.material);

        foreach (Biome b in localBiomes)
            biomeMaterials.Add(b.type, b.material);

        await System.Threading.Tasks.Task.Delay(10);

        GenerateDeepOcean();

        foreach (LocalizedBiome b in localBiomes)
            GenerateLocalizedBiome(rand.Next(b.minZones, b.maxZones), b.zoneSize, b);
    }

    private void GenerateDeepOcean ()
    {
        foreach (Tile t in terrainTiles)
        {
            if (t.type == Tile.Type.Ocean &&
                t.neighbors.Aggregate(true, (deep, nextTile) => deep && (int)nextTile.type <= 1))
            {
                t.ChangeType(Tile.Type.DeepOcean);
            }
        }
    }

    private void GenerateLocalizedBiome (int numZones, float radius, LocalizedBiome biome)
    {
        void Generate(Vector3 pos)
        {
            foreach (Collider col in Physics.OverlapSphere(pos, radius))
            {
                Tile t = col.GetComponent<Tile>();

                if ((int)t.type > 1)
                    t.ChangeType(biome.type);
            }
        }

        for (int i = 0; i < numZones; i++)
        {
            Vector3 pos = new Vector3(rand.Next(0, width), 0, rand.Next(0, height));

            Generate(pos);

            for (int j = 0; j < rand.Next(1, (int)radius * 2); j++)
            {
                Vector3 offset = new Vector3(rand.Next(-(int)(radius / 2), (int)(radius / 2)), 0f, rand.Next(-(int)(radius / 2), (int)(radius / 2)));

                Generate(pos + offset);
            }
                
        }
    }

    public void Quit ()
    {
        Application.Quit();
    }
}