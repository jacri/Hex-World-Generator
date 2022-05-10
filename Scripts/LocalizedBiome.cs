using UnityEngine;

[System.Serializable]
public class LocalizedBiome : Biome
{
    public LocalizedBiome() { }

    [Space(10)]
    [Header("Zones")]

    public int minZones;
    public int maxZones;
    public float zoneSize;
}
