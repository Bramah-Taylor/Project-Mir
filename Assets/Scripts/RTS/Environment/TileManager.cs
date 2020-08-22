using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileManager : MonoBehaviour
{
    enum TileDensity
    {
        None,
        Empty,
        Sparse,
        Dense
    };

    private class Tile
    {
        public TileDensity Type;
        public int Number;
        public int x;
        public int y;

        public Tile (TileDensity DensityType, int TileNumber, int xPos, int yPos)
        {
            Type = DensityType;
            Number = TileNumber;
            x = xPos;
            y = yPos;
        }
    }

    public int NeighbourIterations = 3;
    public int Columns = 10;
    public int Rows = 10;
    public float ScaleFactor = 1.28f;
    public int DenseTileChance = 33;
    public int SparseTileChance = 33;
    public int EmptyTileChance = 33;
    public GameObject[] DenseStarTiles;
    public GameObject[] SparseStarTiles;
    public GameObject[] EmptyStarTiles;

    private Transform TileHolder;
    private Dictionary<int, Tile> StarTiles = new Dictionary<int, Tile>();

    private void Start()
    {
        TileHolder = new GameObject("Tileset").transform;

        int DenseTilesToSpawn = Random.Range(DenseTileChance / 2, DenseTileChance);
        int DenseTilesSpawned = 0;
        while (DenseTilesSpawned < DenseTilesToSpawn)
        {
            int ArrayPos = Random.Range(0, 99);
            if (!StarTiles.ContainsKey(ArrayPos))
            {
                int xPos = ArrayPos % Columns;
                int yPos = (ArrayPos - xPos) / Rows;

                int TileNumber = Random.Range(0, DenseStarTiles.Length);

                StarTiles.Add(ArrayPos, new Tile(TileDensity.Dense, TileNumber, xPos, yPos));

                DenseTilesSpawned++;
            }
        }

        // Add a small number of sparse tiles.
        int SparseTilesToSpawn = Random.Range(SparseTileChance / 4, SparseTileChance / 2);
        int SparseTilesSpawned = 0;
        while (SparseTilesSpawned < SparseTilesToSpawn)
        {
            int ArrayPos = Random.Range(0, 99);
            if (!StarTiles.ContainsKey(ArrayPos))
            {
                int xPos = ArrayPos % Columns;
                int yPos = (ArrayPos - xPos) / Rows;

                int TileNumber = Random.Range(0, SparseStarTiles.Length);

                StarTiles.Add(ArrayPos, new Tile(TileDensity.Sparse, TileNumber, xPos, yPos));

                SparseTilesSpawned++;
            }
        }

        Dictionary<int, Tile> TempStarTiles = new Dictionary<int, Tile>();

        // Now add valid neighbours around each tile.
        for (int i = 0; i < NeighbourIterations; i++)
        {
            foreach (KeyValuePair<int, Tile> Entry in StarTiles)
            {
                Tile StarTile = Entry.Value;

                int CurrentTile = StarTile.x + StarTile.y * Rows;

                if (StarTile.x > 0)
                {
                    int ArrayPos = CurrentTile - 1;

                    TryAddNewTile(ref TempStarTiles, ArrayPos, StarTile);
                }

                if (StarTile.x < Columns - 1)
                {
                    int ArrayPos = CurrentTile + 1;

                    TryAddNewTile(ref TempStarTiles, ArrayPos, StarTile);
                }

                if (StarTile.y > 0)
                {
                    int ArrayPos = CurrentTile - Rows;

                    TryAddNewTile(ref TempStarTiles, ArrayPos, StarTile);
                }

                if (StarTile.y < Rows - 1)
                {
                    int ArrayPos = CurrentTile + Rows;

                    TryAddNewTile(ref TempStarTiles, ArrayPos, StarTile);
                }
            }

            foreach (KeyValuePair<int, Tile> Entry in TempStarTiles)
            {
                if (!StarTiles.ContainsKey(Entry.Key))
                {
                    StarTiles.Add(Entry.Key, Entry.Value);
                }
                else
                {
                    StarTiles[Entry.Key] = Entry.Value;
                }
            }

            TempStarTiles.Clear();
        }

        for (int i = 0; i < Columns * Rows; i++)
        {
            if (!StarTiles.ContainsKey(i))
            {
                int TileNumber = Random.Range(0, EmptyStarTiles.Length);
                int xPos = i % Columns;
                int yPos = (i - xPos) / Rows;
                StarTiles.Add(i, new Tile(TileDensity.Empty, TileNumber, xPos, yPos));
            }
            else if (StarTiles[i].Type == TileDensity.None)
            {
                StarTiles[i].Type = TileDensity.Empty;
            }
        }

        // Add final step, iterating through the map and ensuring that the tile is distinct from its neighbours by comparing
        // tile numbers for tiles of equal density types.

        foreach (KeyValuePair<int, Tile> Entry in StarTiles)
        {
            Tile StarTile = Entry.Value;

            GameObject TileToInstantiate = DenseStarTiles[0];
            if (StarTile.Type == TileDensity.Dense)
            {
                TileToInstantiate = DenseStarTiles[StarTile.Number];
            }
            else if (StarTile.Type == TileDensity.Sparse)
            {
                TileToInstantiate = SparseStarTiles[StarTile.Number];
            }
            else if (StarTile.Type == TileDensity.Empty)
            {
                TileToInstantiate = EmptyStarTiles[StarTile.Number];
            }
            else
            {
                Debug.Log("Invalid tile type in TileManager::Start().");
                TileToInstantiate = EmptyStarTiles[StarTile.Number];
            }

            if (TileToInstantiate)
            {
                GameObject TileInstance = Instantiate<GameObject>(TileToInstantiate, new Vector3(StarTile.x * ScaleFactor, StarTile.y * ScaleFactor, 0.0f), Quaternion.identity);
                TileInstance.transform.SetParent(TileHolder);
            }
        }
    }

    private void TryAddNewTile(ref Dictionary<int, Tile> TempStarTiles, int ArrayPos, Tile StarTile)
    {
        if (!TempStarTiles.ContainsKey(ArrayPos))
        {
            if (!StarTiles.ContainsKey(ArrayPos))
            {
                TileDensity NewTileDensity = (StarTile.Type - 1 >= TileDensity.Empty) ? StarTile.Type - 1 : TileDensity.Empty;
                int TileNumber = Random.Range(0, SparseStarTiles.Length);
                int xPos = ArrayPos % Columns;
                int yPos = (ArrayPos - xPos) / Rows;

                TempStarTiles.Add(ArrayPos, new Tile(NewTileDensity, TileNumber, xPos, yPos));
            }
            else if ((StarTiles[ArrayPos].Type >= TileDensity.Sparse) && (StarTiles[ArrayPos].Type < StarTile.Type - 1))
            {
                StarTiles[ArrayPos].Type = StarTile.Type - 1;
            }
        }
    }

    private void Update()
    {
        
    }
}
