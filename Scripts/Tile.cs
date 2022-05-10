using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Tile : MonoBehaviour
{
    // Type Enum

    public enum Type
    {
        DeepOcean = 0,
        Ocean = 1,
        Forest = 2,
        Desert = 3,
        Tundra = 4,
    }

    // Public

    public Type type;
    public int x;
    public int y;
    public List<Tile> neighbors;

    // Private

    private Renderer rend;
    private WorldGenerator worldGenerator;

    // Functions

    public void SetCoords (int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Tile>() && !neighbors.Contains(other.GetComponent<Tile>()))
            neighbors.Add(other.GetComponent<Tile>());
    }

    public void ChangeType (Type newType)
    {
        if (rend == null)
            rend = GetComponent<Renderer>();

        if (worldGenerator == null)
            worldGenerator = FindObjectOfType<WorldGenerator>();

        type = newType;
        rend.material = worldGenerator.biomeMaterials[newType];
    }
}