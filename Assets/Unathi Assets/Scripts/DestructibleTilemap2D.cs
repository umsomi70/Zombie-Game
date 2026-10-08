using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DestructibleTilemap2D : MonoBehaviour
{
    [Header("Tile Health")]
    public int hitsToBreak = 2;

    private Tilemap tilemap;

    // Stores the remaining hits for each individual tile
    private Dictionary<Vector3Int, int> tileHealth =
        new Dictionary<Vector3Int, int>();

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public void BreakTile(Vector3 worldPosition)
    {
        // Convert world position into Tilemap cell position
        Vector3Int cellPosition =
            tilemap.WorldToCell(worldPosition);

        // Make sure there is actually a tile here
        if (!tilemap.HasTile(cellPosition))
            return;

        // If this tile doesn't have health yet,
        // give it the starting number of hits
        if (!tileHealth.ContainsKey(cellPosition))
        {
            tileHealth[cellPosition] = hitsToBreak;
        }

        // Remove one hit
        tileHealth[cellPosition]--;

        // Break the tile when it reaches 0
        if (tileHealth[cellPosition] <= 0)
        {
            tilemap.SetTile(cellPosition, null);

            // Remove it from the dictionary
            tileHealth.Remove(cellPosition);
        }
    }
}