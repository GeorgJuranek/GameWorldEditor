using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableObjectSpawnForRoom", menuName = "ScriptableObjects/LabyrinthGenerator/ObjectSpawnInformation")]
public class ScriptableObjectSpawnForRoom : ScriptableObject
{
    public List<Order> mainHallwayOrders;

    public GameObject doorPrefab;

    public float spawnDiameterPerTile;

    public List<RoomPlan> furtherRoomOrders;
}


// This Containers are used only in each other or in the context of the ScriptableObjectSpawnForRoom
[System.Serializable]
public struct Order
{
    [SerializeField]
    public ETile tileTypeToSpawnOn;

    [SerializeField]
    public bool useRandomizedPositionInsideTile;

    [SerializeField]
    [Min(0)]
    public int minAmount;

    [SerializeField]
    public bool useRandomAmount;

    [Range(0, 1), SerializeField]
    public float probability;

    [SerializeField]
    public List<GameObject> prefabs;

}

[System.Serializable]
public struct RoomPlan
{
    [SerializeField]
    public string naming;

    [SerializeField]
    public Color roomColor;

    [SerializeField]
    public int minSize;

    [SerializeField]
    public List<Order> npcOrders;
}

