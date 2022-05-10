using UnityEngine;

[System.Serializable]
public class GeneralBiome : Biome
{
    public GeneralBiome() { }

    [Space(10)]
    [Header("Elevation")]

    public float minElevation;
    public float maxElevation;
}
