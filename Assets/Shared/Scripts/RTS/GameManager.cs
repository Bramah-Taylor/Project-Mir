using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject WaypointTemplate;
    public ShipManager CurrentPawn;

    private List<GameObject> ActiveWaypoints;

    private int CurrentSpriteCount;

    void Start()
    {
        Application.targetFrameRate = 60;
        ActiveWaypoints = new List<GameObject>();

        CurrentSpriteCount = 0;
    }

    public void SetWaypoint(Vector3 Position)
    {
        GameObject NewWaypoint = Instantiate<GameObject>(WaypointTemplate, Position, Quaternion.identity);
        ActiveWaypoints.Add(NewWaypoint);

        CurrentPawn.BeginMove(Position, NewWaypoint);
    }

    public void RemoveWaypoint(GameObject WaypointToRemove)
    {
        if (ActiveWaypoints.Contains(WaypointToRemove))
        {
            ActiveWaypoints.Remove(WaypointToRemove);
            Destroy(WaypointToRemove);
        }
        else
        {
            Debug.Log("Unknown game object in RemoveWaypoint.");
        }
    }

    public void RegisterShipSprites(SpriteRenderer[] InputSprites)
    {
        int NewMaxSprite = 0;
        foreach (SpriteRenderer Sprite in InputSprites)
        {
            if (Sprite.sortingOrder > NewMaxSprite)
            {
                NewMaxSprite = Sprite.sortingOrder;
            }

            Sprite.sortingOrder += CurrentSpriteCount;
        }

        CurrentSpriteCount += NewMaxSprite;
    }
}
