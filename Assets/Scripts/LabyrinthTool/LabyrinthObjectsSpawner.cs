using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class LabyrinthObjectsSpawner
{

    List<Order> orders = new List<Order>();
    float spawnRadiusOnTile = 10f;
    GameObject prefabDoor;
    List<RoomPlan> roomPlans = new List<RoomPlan>();

    public LabyrinthObjectsSpawner(ScriptableObjectSpawnForRoom scriptableObjectSpawnForRoom)
    {
        orders = scriptableObjectSpawnForRoom.mainHallwayOrders;
        spawnRadiusOnTile = scriptableObjectSpawnForRoom.spawnDiameterPerTile;
        prefabDoor = scriptableObjectSpawnForRoom.doorPrefab;
        roomPlans = scriptableObjectSpawnForRoom.furtherRoomOrders;
    }

    public void SetMainElements(List<LabTile> roomGroup)
    {
        if (roomGroup == null) return;
        if (orders == null) return;

        ApplyOrdersOnRoomgroup(roomGroup.ToArray(), orders);
    }

    public void SetRoomElements(List<LabTile[]> roomGroups, LabTile[,] mapInformations)
    {
        if (roomGroups == null) return;
        if (orders == null) return;

        foreach (var roomGroup in roomGroups)
        {
            SetDoor(roomGroup, mapInformations);

            ApplyRoomPlans(roomGroup);

        }
    }

    private void ApplyRoomPlans(LabTile[] roomGroup)
    {
        roomPlans = BubbleSortBiggestToSmallest(roomPlans);

        foreach (var roomPlan in roomPlans)
        {
            if (roomGroup.Length >= roomPlan.minSize)
            {
                ChangeRoomColors(roomGroup, roomPlan.roomColor);
                ApplyOrdersOnRoomgroup(roomGroup, roomPlan.npcOrders);
                break;
            }

        }
    }

    List<RoomPlan> BubbleSortBiggestToSmallest(List<RoomPlan> roomPlans)
    {
        int count = roomPlans.Count;
        for (int i = 0; i < count - 1; i++)
        {
            for (int j = 0; j < count - i - 1; j++)
            {
                if (roomPlans[j].minSize < roomPlans[j + 1].minSize)
                {
                    RoomPlan temp = roomPlans[j];
                    roomPlans[j] = roomPlans[j + 1];
                    roomPlans[j + 1] = temp;
                }
            }
        }

        return roomPlans;
    }



    private void ApplyOrdersOnRoomgroup(LabTile[] roomGroup, List<Order> orders)
    {
        foreach (var order in orders)
        {
            List<LabTile> specificLabTiles = new List<LabTile>();
            int count = 0;

            foreach (var room in roomGroup)
            {
                if (room.Type == order.tileTypeToSpawnOn)
                {
                    specificLabTiles.Add(room);
                    count++;
                }
            }

            if (count == 0) //Errorcase
            {
                Debug.LogWarning($"No Tiles of specific Etile '{order.tileTypeToSpawnOn}' were found during room-npc-generation");
                continue;
            }

            if (order.minAmount > 0)
                CreateAmountOfNPCS(specificLabTiles, order);

            if (order.useRandomAmount)
                CreateRandomAmountNPCs(specificLabTiles, order);
        }
    }

    void SetDoor(LabTile[] roomGroup, LabTile[,] mapInformations)
    {
        if (prefabDoor == null)
        {
            Debug.LogWarning("Missing doorPrefab!");
            return;
        }

        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left,
        };

        for (int i = 0; i < roomGroup.Length; i++)
        {
            foreach (var direction in directions)
            {
                Vector2Int newPosition = roomGroup[i].PositionInLabTileContext + direction;

                if (!(newPosition.x >= 0) || !(newPosition.x < mapInformations.GetLength(0)) ||
                    !(newPosition.y >= 0) || !(newPosition.y < mapInformations.GetLength(1)))
                {
                    continue;
                }


                if (!roomGroup.Contains(mapInformations[newPosition.x, newPosition.y]) && mapInformations[newPosition.x, newPosition.y].Type != ETile.wall && mapInformations[newPosition.x, newPosition.y].Type != ETile.roomEntry)
                {
                    Vector3 newDirection = (mapInformations[newPosition.x, newPosition.y].PositionInWorld - roomGroup[i].PositionInWorld).normalized;
                    Quaternion rotation = Quaternion.LookRotation(newDirection);
                    StaticHelper.InstantiateAsPrefabInEditor(prefabDoor, roomGroup[i].PositionInWorld, rotation);
                    break;
                }
            }

        }
    }


    void CreateAmountOfNPCS(List<LabTile> tilesOfSpecificType, Order Order)
    {
        //Check for enough tiles of type
        int amountToProduce = 0;
        if (tilesOfSpecificType.Count < Order.minAmount)
        {
            Debug.LogWarning("Too less tiles for ordered amount, amount will be reduced to fitting amount");
            amountToProduce = tilesOfSpecificType.Count;
        }
        else
        {
            amountToProduce = Order.minAmount;
        }


        int count = 0;
        while (count < amountToProduce)
        {
            LabTile randomLabtile = tilesOfSpecificType[Random.Range(0, tilesOfSpecificType.Count)];

            if (Order.probability >= Random.Range(0.0f, 1.0f))
            {
                int random = Random.Range(0, Order.prefabs.Count);

                if (Order.useRandomizedPositionInsideTile)
                {
                    Vector3 randomizedPosition = randomLabtile.PositionInWorld + (Vector3.up * 0.1f) + new Vector3(Random.Range(0, spawnRadiusOnTile / 2), 0f, Random.Range(0, spawnRadiusOnTile / 2));

                    StaticHelper.InstantiateAsPrefabInEditor(Order.prefabs[random], randomizedPosition, Quaternion.identity);
                }
                else
                {
                    StaticHelper.InstantiateAsPrefabInEditor(Order.prefabs[random], randomLabtile.PositionInWorld + (Vector3.up * 0.1f), Quaternion.identity);
                }

                ++count;

                if (count >= Order.minAmount && Order.minAmount > 0)
                    break;

                //Cant spawn on same field, so take tile out
                tilesOfSpecificType.Remove(randomLabtile);
            }
        }
    }

    void CreateRandomAmountNPCs(List<LabTile> tilesOfSpecificType, Order Order)
    {
        foreach (var specificTile in tilesOfSpecificType)
        {
            if (Order.probability >= UnityEngine.Random.Range(0.0f, 1.0f))
            {
                int random = UnityEngine.Random.Range(0, Order.prefabs.Count);

                if (Order.useRandomizedPositionInsideTile)
                {
                    Vector3 randomizedPosition = specificTile.PositionInWorld + (Vector3.up * 0.1f) + new Vector3(UnityEngine.Random.Range(0, spawnRadiusOnTile / 2), 0f, UnityEngine.Random.Range(0, spawnRadiusOnTile / 2));
                    StaticHelper.InstantiateAsPrefabInEditor(Order.prefabs[random], randomizedPosition, Quaternion.identity);
                }
                else
                {
                    StaticHelper.InstantiateAsPrefabInEditor(Order.prefabs[random], specificTile.PositionInWorld + (Vector3.up * 0.1f), Quaternion.identity);
                }
            }
        }
    }


    public void ChangeRoomColors(LabTile[] roomGroup, Color newColor)
    {
        foreach (var room in roomGroup)
        {
            Transform currentRoom = room.Represents;

            CarpetColorChanger[] carpets = currentRoom.GetComponentsInChildren<CarpetColorChanger>();

            foreach (CarpetColorChanger carpet in carpets)
            {
                carpet.ChangeColor(newColor);
            }
        }
    }

}
