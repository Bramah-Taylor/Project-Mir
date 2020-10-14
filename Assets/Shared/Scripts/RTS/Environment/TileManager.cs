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

    // Class that represents a star tile, and holds all of the information required for spawn checks.
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

    // Values defining the size of the tile map.
    public int Columns = 10;
    public int Rows = 10;

    // How many times we should iterate over the tile map to ensure the neighbouring tile types are correct.
    public int NeighbourIterations = 3;

    // Values defining how tiles are placed and moved in the world.
    public float ScaleFactor = 1.28f;
    public float ScrollSpeed = 0.9f;

    // Values defining the types of stars to spawn.
    public int DenseTileChance = 33;
    public int SparseTileChance = 33;
    public int EmptyTileChance = 33;

    // The maximum number of individual stars to spawn on the closer background layer.
    public int MaxStarsToSpawn = 100;

    // GameObject arrays holding the tiles and stars we'll spawn into the world.
    public GameObject[] DenseStarTiles;
    public GameObject[] SparseStarTiles;
    public GameObject[] EmptyStarTiles;
    public GameObject[] IndividualStars;

    // Public reference to the player's camera.
    public GameObject Camera;

    // Private transforms which we'll use to transform the stars, and keep them contained neatly in the object hierarchy.
    private Transform TileHolder;
    private Transform StarHolder;

    // Dictionary of array indices to tile objects.
    // #TODO: This could probably be replaced with an array.
    private Dictionary<int, Tile> StarTiles = new Dictionary<int, Tile>();

    // Calculated position offset for the tiles.
    private float PositionOffset;
    private float StarScrollSpeed;

    private void Start()
    {
        TileHolder = new GameObject("Tileset").transform;
        StarHolder = new GameObject("Individual Stars").transform;

        PositionOffset = ScaleFactor * (Rows / 2);

        StarScrollSpeed = ScrollSpeed - (1.0f - ScrollSpeed);

        // Start by spawning a random number of dense star tiles.
        int DenseTilesToSpawn = Random.Range(DenseTileChance / 2, DenseTileChance);
        int DenseTilesSpawned = 0;
        while (DenseTilesSpawned < DenseTilesToSpawn)
        {
            // First, check that we're spawning the tile in an unoccupied space.
            int ArrayIndex = Random.Range(0, 99);
            if (!StarTiles.ContainsKey(ArrayIndex))
            {
                // Calculate the position.
                int xPos = ArrayIndex % Columns;
                int yPos = (ArrayIndex - xPos) / Rows;

                // Set a random tile number out of the tiles in the dense tiles array.
                int TileNumber = Random.Range(0, DenseStarTiles.Length);

                // Add the star tile.
                StarTiles.Add(ArrayIndex, new Tile(TileDensity.Dense, TileNumber, xPos, yPos));

                // Increment the count of star tiles spawned.
                DenseTilesSpawned++;
            }
        }

        // Add a small number of sparse tiles, using the same method we used for the dense star tiles.
        int SparseTilesToSpawn = Random.Range(SparseTileChance / 4, SparseTileChance / 2);
        int SparseTilesSpawned = 0;
        while (SparseTilesSpawned < SparseTilesToSpawn)
        {
            int ArrayIndex = Random.Range(0, 99);
            if (!StarTiles.ContainsKey(ArrayIndex))
            {
                int xPos = ArrayIndex % Columns;
                int yPos = (ArrayIndex - xPos) / Rows;

                int TileNumber = Random.Range(0, SparseStarTiles.Length);

                StarTiles.Add(ArrayIndex, new Tile(TileDensity.Sparse, TileNumber, xPos, yPos));

                SparseTilesSpawned++;
            }
        }

        // Create a temporary dictionary for adding new elements to the star tile dictionary.
        Dictionary<int, Tile> TempStarTiles = new Dictionary<int, Tile>();

        // Now add valid neighbours around each tile.
        for (int i = 0; i < NeighbourIterations; i++)
        {
            foreach (KeyValuePair<int, Tile> Entry in StarTiles)
            {
                Tile StarTile = Entry.Value;

                int CurrentTile = StarTile.x + StarTile.y * Rows;

                // Left neighbour.
                if (StarTile.x > 0)
                {
                    int ArrayIndex = CurrentTile - 1;

                    TryAddNewTile(ref TempStarTiles, ArrayIndex, StarTile);
                }

                // Right neighbour.
                if (StarTile.x < Columns - 1)
                {
                    int ArrayIndex = CurrentTile + 1;

                    TryAddNewTile(ref TempStarTiles, ArrayIndex, StarTile);
                }

                // Bottom neighbour.
                if (StarTile.y > 0)
                {
                    int ArrayIndex = CurrentTile - Rows;

                    TryAddNewTile(ref TempStarTiles, ArrayIndex, StarTile);
                }

                // Top neighbour.
                if (StarTile.y < Rows - 1)
                {
                    int ArrayIndex = CurrentTile + Rows;

                    TryAddNewTile(ref TempStarTiles, ArrayIndex, StarTile);
                }
            }

            // Add the new neighbouring tiles to the dictionary.
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

            // Empty the temp star tiles dictionary for the next iteration.
            TempStarTiles.Clear();
        }

        // Now go through the entire tile dictionary; if it doesn't contain an entry, add an empty tile there.
        // Also, if the tile type is None for some reason, set it to be empty as well.
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
            int OutTileNumber = Entry.Value.Number;
            int MaxTileNumber = EmptyStarTiles.Length - 1;

            // Right neighbour.
            if (StarTiles.ContainsKey(Entry.Key + 1))
            {
                IncrementTileNumber(StarTile.Number, StarTiles[Entry.Key + 1].Number, MaxTileNumber, ref OutTileNumber);
            }

            // Left neighbour.
            if (StarTiles.ContainsKey(Entry.Key - 1))
            {
                IncrementTileNumber(StarTile.Number, StarTiles[Entry.Key - 1].Number, MaxTileNumber, ref OutTileNumber);
            }

            // Top neighbour.
            if (StarTiles.ContainsKey(Entry.Key + Rows))
            {
                IncrementTileNumber(StarTile.Number, StarTiles[Entry.Key + Rows].Number, MaxTileNumber, ref OutTileNumber);
            }

            // Bottom neighbour.
            if (StarTiles.ContainsKey(Entry.Key - Rows))
            {
                IncrementTileNumber(StarTile.Number, StarTiles[Entry.Key - Rows].Number, MaxTileNumber, ref OutTileNumber);
            }

            Entry.Value.Number = OutTileNumber;
        }

        // Now we can finally create the actual star tiles GameObjects.
        foreach (KeyValuePair<int, Tile> Entry in StarTiles)
        {
            Tile StarTile = Entry.Value;

            // Get the tile prefab for the correct density and number.
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

            // If the GameObject is valid, instantiate it and set the new GameObject's parent to be the TileHolder.
            if (TileToInstantiate)
            {
                GameObject TileInstance = Instantiate<GameObject>(TileToInstantiate, new Vector3((StarTile.x * ScaleFactor) - PositionOffset, (StarTile.y * ScaleFactor) - PositionOffset, 0.0f), Quaternion.identity);
                TileInstance.transform.SetParent(TileHolder);
            }
        }

        // Finally, spawn in the individual stars.
        int StarsToSpawn = Random.Range(MaxStarsToSpawn / 2, MaxStarsToSpawn);
        for (int i = 0; i < StarsToSpawn; i++)
        {
            // Get a random star from the individual stars array.
            GameObject StarToInstantiate = IndividualStars[Random.Range(0, IndividualStars.Length)];

            // Give the star a random position.
            float XPosition = Random.Range(-(ScaleFactor * Columns), ScaleFactor * Columns);
            float YPosition = Random.Range(-(ScaleFactor * Rows), ScaleFactor * Rows);
            Vector3 RandomPosition = new Vector3(XPosition, YPosition, 0.0f);

            // Instantiate the star, and set its parent to be the StarHolder.
            GameObject StarInstance = Instantiate<GameObject>(StarToInstantiate, RandomPosition, Quaternion.identity);
            StarInstance.transform.SetParent(StarHolder);
        }
    }

    // Try to add a new, valid tile to the input dictionary. Otherwise decrement the tile type at the already present location.
    // This function is used for adding neighbouring tiles of lower types around already existing tiles.
    private void TryAddNewTile(ref Dictionary<int, Tile> TempStarTiles, int ArrayIndex, Tile StarTile)
    {
        if (!TempStarTiles.ContainsKey(ArrayIndex))
        {
            if (!StarTiles.ContainsKey(ArrayIndex))
            {
                // Add a new tile of the correct density at this location.
                TileDensity NewTileDensity = (StarTile.Type - 1 >= TileDensity.Empty) ? StarTile.Type - 1 : TileDensity.Empty;
                int TileNumber = Random.Range(0, SparseStarTiles.Length);
                int xPos = ArrayIndex % Columns;
                int yPos = (ArrayIndex - xPos) / Rows;

                TempStarTiles.Add(ArrayIndex, new Tile(NewTileDensity, TileNumber, xPos, yPos));
            }
            else if ((StarTiles[ArrayIndex].Type >= TileDensity.Sparse) && (StarTiles[ArrayIndex].Type < StarTile.Type - 1))
            {
                // Decrement the index of the alreadt existing tile.
                StarTiles[ArrayIndex].Type = StarTile.Type - 1;
            }
        }
    }

    // Increment the input tile up to the max number of tiles. If we hit the max, wrap around to 0.
    // This function is used to ensure that no two neighbouring tiles of the same density have the same tile number.
    private void IncrementTileNumber(int InTileNum, int NeighbourTileNum, int Max, ref int OutTileNum)
    {
        if (InTileNum == NeighbourTileNum)
        {
            if (InTileNum == Max)
            {
                OutTileNum = 0;
            }
            else
            {
                OutTileNum = InTileNum + 1;
            }
        }
    }

    private void Update()
    {
        // Calculate scroll distances for the parallax scrolling effect on the background layers.
        Vector2 ScrollDist = new Vector2(Camera.transform.position.x * ScrollSpeed, Camera.transform.position.y * ScrollSpeed);
        Vector2 StarScrollDist = new Vector2(Camera.transform.position.x * StarScrollSpeed, Camera.transform.position.y * StarScrollSpeed);

        // Transform the holder GameObjects based on the scroll distances.
        TileHolder.position = new Vector3(ScrollDist.x, ScrollDist.y, TileHolder.transform.position.z);
        StarHolder.position = new Vector3(StarScrollDist.x, StarScrollDist.y, StarHolder.position.z);
    }
}
