using UnityEngine;
using Unity.AI.Navigation;

public sealed class LabyrinthTileBuilder
{
    public enum ERoomDirections
    {
        up,
        right,
        down,
        left
    }

    float averageSize = 10f;
    GameObject prefab1out;
    GameObject prefab2outCorner;
    GameObject prefab2outStraight;
    GameObject prefab3out;
    GameObject prefab4out;
    LabTile[,] currentMap;
    int currentMapWidth;
    int currentMapHeight;

    [Tooltip("If you are already using a NavMeshSurface ignore this field, otherwise put a prefab with a navmeshSurface you want be initialised in the scene in here.")]
    GameObject navMeshSurface;


    public LabyrinthTileBuilder(ScriptableRoomTiles scriptableRoomTiles)
    {

        averageSize = scriptableRoomTiles.averageSize;
        prefab1out = scriptableRoomTiles.DeadEnd;
        prefab2outCorner = scriptableRoomTiles.Corner;
        prefab2outStraight = scriptableRoomTiles.Line;
        prefab3out = scriptableRoomTiles.Ternary;
        prefab4out = scriptableRoomTiles.Crossing;
        navMeshSurface = scriptableRoomTiles.NavMeshSurface;
    }

    public void Build(LabTile[,] mapInformations, Vector3 centerPosition)
    {
        if (prefab1out==null || prefab2outCorner == null || prefab2outStraight == null || prefab3out == null || prefab4out == null)
        {
            Debug.LogError("Error in ScriptableRoomTiles: missing Prefabs!");
            return;
        }

        currentMap = mapInformations;
        currentMapWidth = mapInformations.GetLength(0);
        currentMapHeight = mapInformations.GetLength(1);


        bool[] hasRoomsAround = new bool[4];

        for (int y = 0; y < mapInformations.GetLength(1); y++)
        {
            for (int x = 0; x < mapInformations.GetLength(0); x++)
            {
                if (mapInformations[x, y].Type == ETile.wall)
                {
                    continue;
                }

                int neighboursInDirections = GetDirectionsForNeighbours(ref hasRoomsAround, new Vector2Int(x, y));


                //Default Case
                GameObject roomToCreate = prefab4out;
                float toCreateRotationInDegree = 0f;

                switch (neighboursInDirections)
                {
                    case 1:
                        {
                            roomToCreate = prefab1out;

                            if (hasRoomsAround[(int)ERoomDirections.up])
                                toCreateRotationInDegree = 0f;
                            else if (hasRoomsAround[(int)ERoomDirections.right])
                                toCreateRotationInDegree = 90f;
                            else if (hasRoomsAround[(int)ERoomDirections.down])
                                toCreateRotationInDegree = 180f;
                            else if (hasRoomsAround[(int)ERoomDirections.left])
                                toCreateRotationInDegree = -90f;

                            break;
                        }

                    case 2:
                        {
                            //Straights
                            if (hasRoomsAround[(int)ERoomDirections.up] && hasRoomsAround[(int)ERoomDirections.down])
                            {
                                roomToCreate = prefab2outStraight;
                                toCreateRotationInDegree = 0f;
                            }
                            else if (hasRoomsAround[(int)ERoomDirections.left] && hasRoomsAround[(int)ERoomDirections.right])
                            {
                                roomToCreate = prefab2outStraight;
                                toCreateRotationInDegree = 90f;
                            }
                            //Corners
                            else if (hasRoomsAround[(int)ERoomDirections.up] && hasRoomsAround[(int)ERoomDirections.right])
                            {
                                roomToCreate = prefab2outCorner;
                                toCreateRotationInDegree = 0f;
                            }
                            else if (hasRoomsAround[(int)ERoomDirections.up] && hasRoomsAround[(int)ERoomDirections.left])
                            {
                                roomToCreate = prefab2outCorner;
                                toCreateRotationInDegree = -90f;
                            }
                            else if (hasRoomsAround[(int)ERoomDirections.right] && hasRoomsAround[(int)ERoomDirections.down])
                            {
                                roomToCreate = prefab2outCorner;
                                toCreateRotationInDegree = 90f;
                            }
                            else if (hasRoomsAround[(int)ERoomDirections.left] && hasRoomsAround[(int)ERoomDirections.down])
                            {
                                roomToCreate = prefab2outCorner;
                                toCreateRotationInDegree = 180f;
                            }

                            break;
                        }

                    case 3:
                        {
                            roomToCreate = prefab3out;

                            if (!hasRoomsAround[(int)ERoomDirections.up])
                                toCreateRotationInDegree = 180f;
                            else if (!hasRoomsAround[(int)ERoomDirections.right])
                                toCreateRotationInDegree = -90f;
                            else if (!hasRoomsAround[(int)ERoomDirections.down])
                                toCreateRotationInDegree = 0f;
                            else if (!hasRoomsAround[(int)ERoomDirections.left])
                                toCreateRotationInDegree = 90f;

                            break;
                        }
                }

                Vector3 newPosition = centerPosition + new Vector3(x, 0, y) * averageSize; ;
                Quaternion newRotation = Quaternion.Euler(Vector3.up * toCreateRotationInDegree);

                GameObject newRoom = StaticHelper.InstantiateAsPrefabInEditor(roomToCreate, newPosition, newRotation);

                mapInformations[x, y].SetReference(newRoom.transform);
                mapInformations[x, y].SetPositionInWorld(newRoom.transform.position);
            }
        }

        CreateNavMesh();
    }


    void CreateNavMesh()
    {

        NavMeshSurface previousNavMeshSurface = GameObject.FindAnyObjectByType<NavMeshSurface>();

        if (previousNavMeshSurface != null)
        {
            previousNavMeshSurface.BuildNavMesh();
        }
        else
        {
            if (navMeshSurface == null)
            {
                Debug.LogWarning("Missing NavMeshSurface in TileInterpreter. Make sure you have a valid NavMeshSurface in your scene.");
            }
            navMeshSurface = GameObject.Instantiate(navMeshSurface);
            navMeshSurface.GetComponent<NavMeshSurface>().BuildNavMesh();
        }
    }

    int GetDirectionsForNeighbours(ref bool[] hasRoomsAround, Vector2Int position)
    {
        if (hasRoomsAround == null || hasRoomsAround.Length != 4)
            return 0;

        int neighbourCount = GetNeighbourCount(position, ref hasRoomsAround);

        return neighbourCount;

    }

    int GetNeighbourCount(Vector2Int _coord, ref bool[] hasRoomsAround)
    {
        Vector2Int[] neighbourCoords = new Vector2Int[]
        {
                _coord + Vector2Int.up,
                _coord + Vector2Int.right,
                _coord + Vector2Int.down,
                _coord + Vector2Int.left,
        };

        int neighbourCount = 0;
        for (int i = 0; i < neighbourCoords.Length; i++)
        {
            Vector2Int currentNeighbourCoord = neighbourCoords[i];
            hasRoomsAround[i] = false;

            if (!IsCoordInBounds(currentNeighbourCoord))
                continue;

            if (currentMap[currentNeighbourCoord.x, currentNeighbourCoord.y].Type != ETile.wall)
            {
                hasRoomsAround[i] = true;
                ++neighbourCount;
            }
        }

        return neighbourCount;
    }

    bool IsCoordInBounds(Vector2Int _coord)
    {
        return _coord.x >= 0 && _coord.x < currentMapWidth && _coord.y >= 0 && _coord.y < currentMapHeight;
    }

}
