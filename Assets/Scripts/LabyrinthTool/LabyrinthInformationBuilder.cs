using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class LabyrinthInformationBuilder
{
    int labyrinthWidth;

    int labyrinthHeight;

    LabTile[,] tiles;

    int mazeDensity = 5;

    List<Vector2Int> deadEnds = new List<Vector2Int>();

    List<FixedGenerationPoint> fixedGenerationPoints;

    public LabyrinthInformationBuilder(ScriptableLabyrinthInformations labyrinthInformation)
    {
        labyrinthWidth = labyrinthInformation.labyrinthWidth;
        labyrinthHeight = labyrinthInformation.labyrinthHeight;
        mazeDensity = labyrinthInformation.LabyrinthDensity;
        fixedGenerationPoints = labyrinthInformation.fixedPoints;
    }

    #region CreateTheMaze
    public LabTile[,] Build(out List<LabTile[]> roomGroups, out List<LabTile> pureMaze)
    {
        if (labyrinthHeight <= 2 || labyrinthWidth <= 2 || mazeDensity <= 2)
        {
            Debug.LogError("Error in ScriptableLabyrinthInformation: values must be bigger than 2 to create maze!");
            roomGroups = null;
            pureMaze = null;
            return null;
        }


        InitLabyrinth();

        GenerateMazeRecursive();

        foreach (var point in fixedGenerationPoints)
        {
            OverwriteTile(point.GetVector2Int(labyrinthWidth, labyrinthHeight), ETile.constructionPath);
        }

        CheckForUnconnectedPositionsAndTryToConnect();

        // Find base main labyrinth and Update Identities
        List<LabTile> currentTiles = new List<LabTile>();
        foreach (var tile in tiles)
        {
            if (tile.Type != ETile.wall)
                currentTiles.Add(tile);
        }
        pureMaze = currentTiles;

        foreach (var mazeTile in pureMaze)
        {
            CheckAndSetTileIdentity(new Vector2Int(mazeTile.PositionInLabTileContext.x, mazeTile.PositionInLabTileContext.y));
        }


        deadEnds = FindAllPositionsOfType(ETile.deadEnd);

        RecursiveWidenRooms(deadEnds);

        RecursiveFilterFillUpGapsInWidenedRooms();

        RecursiveFilterEmptyOutUnnecessary();


        roomGroups = IdentifyRoomGroups(deadEnds);

        foreach (var roomGroup in roomGroups)
        {
            foreach (var room in roomGroup)
            {
                if (room == roomGroup[0])
                {
                    room.SetTileTypeTo(ETile.roomEntry);
                    continue;
                }

                CheckAndSetTileIdentity(new Vector2Int(room.PositionInLabTileContext.x, room.PositionInLabTileContext.y));
            }
        }

        return tiles;
    }

    private void CheckForUnconnectedPositionsAndTryToConnect()
    {
        if (!DoesPathExistForAll(out List<Vector2Int> unconnected))
        {

            int count = 0;
            while (!DoesPathExistForAll(out List<Vector2Int> stillUnconnected) && count < 100)
            {
                for (int i = 0; i < stillUnconnected.Count; i++)
                {
                    ConnectWithPath(stillUnconnected[i], 2);
                    count++;
                    Debug.Log("Try to connect unconnected fixed points...");
                }
            }
        }

        if (!DoesPathExistForAll(out List<Vector2Int> unconnectedAfterConnecting))
        {
            foreach (var unconnecting in unconnectedAfterConnecting)
            {
                Debug.LogWarning("Not all Points are connected! Please try other customization of LabyrinthGenerator. " +
                "The Position that is not connected to the others: " + unconnecting);
            }
        }
        else
        {
            Debug.Log("All Paths connected!");
        }
    }

    List<Vector2Int> FindAllPositionsOfType(ETile typeToFind)
    {
        List<Vector2Int> foundTiles = new List<Vector2Int>();

        for (int y = 0; y < labyrinthHeight; y++)
        {
            for (int x = 0; x < labyrinthWidth; x++)
            {
                if (tiles[x, y].Type == typeToFind)
                {
                    Vector2Int newPosition = new Vector2Int(x, y);
                    foundTiles.Add(newPosition);
                }
            }
        }

        return foundTiles;
    }
    #endregion

    List<LabTile[]> IdentifyRoomGroups(List<Vector2Int> deadEnds)
    {
        List<LabTile[]> results = new List<LabTile[]>();

        Queue<Vector2Int> nextToCheck = new Queue<Vector2Int>();
        HashSet<LabTile> foundRoomTiles = new HashSet<LabTile>();

        List<Vector2Int> directions = new List<Vector2Int>()
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        foreach (var deadEnd in deadEnds)
        {
            nextToCheck.Clear();
            foundRoomTiles.Clear();

            foundRoomTiles.Add(tiles[deadEnd.x, deadEnd.y]); // add first Tile roomEntry
            nextToCheck.Enqueue(deadEnd);

            while (nextToCheck.Count > 0)
            {
                Vector2Int current = nextToCheck.Dequeue();

                foreach (var direction in directions)
                {
                    Vector2Int toCheckPosition = current + direction;

                    // Check if the position is within bounds
                    if (toCheckPosition.x >= 0 && toCheckPosition.x < tiles.GetLength(0) &&
                        toCheckPosition.y >= 0 && toCheckPosition.y < tiles.GetLength(1))
                    {
                        var tile = tiles[toCheckPosition.x, toCheckPosition.y];

                        if (tile.Type == ETile.constructionPath && !foundRoomTiles.Contains(tile))
                        {
                            foundRoomTiles.Add(tile);
                            nextToCheck.Enqueue(toCheckPosition);
                        }
                    }
                }
            }

            if (foundRoomTiles.Count > 0)
            {
                results.Add(foundRoomTiles.ToArray());
            }
        }

        // Debugging
        foreach (var result in results)
        {
            Debug.Log("Room found with " + result.Length + " tiles.");
        }

        return results;
    }

    #region StepsToCreateMaze
    void InitLabyrinth()
    {
        tiles = new LabTile[labyrinthWidth, labyrinthHeight];

        Vector3 initStartPosition = new Vector3(labyrinthWidth, 0, labyrinthHeight) * 0.5f;

        for (int y = 0; y < labyrinthHeight; y++)
        {
            for (int x = 0; x < labyrinthWidth; x++)
            {
                LabTile newTile = new LabTile(ETile.wall);

                tiles[x, y] = newTile;

                tiles[x, y].SetPositionInContext(new Vector2Int(x, y));
            }
        }
    }

    void GenerateMazeRecursive()
    {
        if (fixedGenerationPoints.Count == 0)
        {
            GenerateNewTileRecursive(new Vector2Int(0, 0), mazeDensity);
        }
        else
        {
            foreach (var point in fixedGenerationPoints)
            {
                GenerateNewTileRecursive(point.GetVector2Int(labyrinthWidth, labyrinthHeight), mazeDensity);
            }
        }

        for (int y = 0; y < labyrinthHeight; y++)
        {
            for (int x = 0; x < labyrinthWidth; x++)
            {
                CheckAndSetTileIdentity(new Vector2Int(x, y));
            }
        }
    }

    void GenerateNewTileRecursive(Vector2Int position, int mazeDensity)
    {
        List<Vector2Int> directions = new List<Vector2Int>()
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        Vector2Int[] searchDirections = new Vector2Int[directions.Count];

        for (int i = 0; i < searchDirections.Length; i++)
        {
            int random = Random.Range(0, directions.Count);
            searchDirections[i] = directions[random];

            directions.RemoveAt(random);
        }

        for (int i = 0; i < searchDirections.Length; i++)
        {
            Vector2Int currentNeighbourPosition = position + this.mazeDensity * searchDirections[i];

            if (!IsInBoundsCheck(currentNeighbourPosition))
            {
                continue;
            }

            if (tiles[currentNeighbourPosition.x, currentNeighbourPosition.y].Type != ETile.wall)
            {
                continue;
            }

            for (int j = 0; j < this.mazeDensity; j++)
            {
                Vector2Int inBetweenNeighbour = position + (searchDirections[i] * j);

                OverwriteTile(inBetweenNeighbour, ETile.constructionPath);
            }
            OverwriteTile(currentNeighbourPosition, ETile.constructionPath);


            GenerateNewTileRecursive(currentNeighbourPosition, mazeDensity);
        }

    }

    void ConnectWithPath(Vector2Int position, int mazeDensity)
    {
        List<Vector2Int> directions = new List<Vector2Int>()
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        Vector2Int[] searchDirections = new Vector2Int[directions.Count];

        for (int i = 0; i < searchDirections.Length; i++)
        {
            int random = Random.Range(0, directions.Count);
            searchDirections[i] = directions[random];

            directions.RemoveAt(random);
        }

        for (int i = 0; i < searchDirections.Length; i++) 
        {

            Vector2Int currentNeighbourPosition = position + this.mazeDensity * searchDirections[i];

            if (!IsInBoundsCheck(currentNeighbourPosition))
            {
                continue;
            }

            if (tiles[currentNeighbourPosition.x, currentNeighbourPosition.y].Type != ETile.wall)
            {
                for (int j = 0; j < this.mazeDensity; j++)
                {
                    Vector2Int inBetweenNeighbour = position + (searchDirections[i] * j);

                    OverwriteTile(inBetweenNeighbour, ETile.constructionPath);
                }
                OverwriteTile(currentNeighbourPosition, ETile.constructionPath);
            }
        }

    }

    #endregion


    #region FilterMethods
    void RecursiveWidenRooms(List<Vector2Int> positions)
    {
        List<Vector2Int> directions = new List<Vector2Int>
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left,
        };

        List<Vector2Int> nextPositions = new List<Vector2Int>();

        foreach (Vector2Int position in positions)
        {
            foreach (Vector2Int direction in directions)
            {
                Vector2Int positionToTest = position + direction;
                Vector2Int nextPositionToTest = position + (2 * direction);

                if (!IsInBoundsCheck(positionToTest) || !IsInBoundsCheck(nextPositionToTest))
                    continue;

                if (tiles[positionToTest.x, positionToTest.y].Type != ETile.wall || tiles[nextPositionToTest.x, nextPositionToTest.y].Type != ETile.wall)
                    continue;

                bool hasGap = true;
                foreach (Vector2Int adjDirection in directions)
                {
                    Vector2Int adjacentPos = nextPositionToTest + adjDirection;
                    if (IsInBoundsCheck(adjacentPos) && tiles[adjacentPos.x, adjacentPos.y].Type != ETile.wall)
                    {
                        hasGap = false;
                        break;
                    }
                }

                if (hasGap)
                {
                    OverwriteTile(nextPositionToTest, ETile.constructionPath);
                    OverwriteTile(positionToTest, ETile.constructionPath);
                    if (!nextPositions.Contains(nextPositionToTest))
                    {
                        nextPositions.Add(nextPositionToTest);
                    }
                }
            }
        }

        if (nextPositions.Count > 0)
        {
            RecursiveWidenRooms(nextPositions);
        }
    }

    void RecursiveFilterFillUpGapsInWidenedRooms()
    {
        List<Vector2Int> directions = new List<Vector2Int>
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left,
        };

        for (int y = 0; y < labyrinthHeight; y++)
        {
            for (int x = 0; x < labyrinthWidth; x++)
            {
                Vector2Int currentPosition = new Vector2Int(x, y);

                if (tiles[currentPosition.x, currentPosition.y].Type == ETile.wall)
                {
                    int pathCount = 0;

                    foreach (var direction in directions)
                    {
                        Vector2Int position = currentPosition + direction;

                        if (!IsInBoundsCheck(position))
                            continue;

                        if (tiles[position.x, position.y].Type == ETile.constructionPath)
                        {
                            pathCount++;
                        }
                    }

                    if (pathCount >= 3)
                    {
                        OverwriteTile(currentPosition, ETile.constructionPath);
                        RecursiveFilterFillUpGapsInWidenedRooms();
                    }
                }
            }
        }
    }

    void RecursiveFilterEmptyOutUnnecessary()
    {
        List<Vector2Int> directions = new List<Vector2Int>
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left,
    };

        List<Vector2Int> found = new List<Vector2Int>();

        for (int y = 0; y < labyrinthHeight; y++)
        {
            for (int x = 0; x < labyrinthWidth; x++)
            {
                Vector2Int currentPosition = new Vector2Int(x, y);

                if (tiles[currentPosition.x, currentPosition.y].Type == ETile.constructionPath)
                {
                    int pathCount = 0;

                    foreach (var direction in directions)
                    {
                        Vector2Int position = currentPosition + direction;

                        if (!IsInBoundsCheck(position))
                            continue;

                        if (tiles[position.x, position.y].Type == ETile.constructionPath)
                        {
                            pathCount++;
                        }
                    }

                    if (pathCount == 1 && !CheckNeighbourOfType(currentPosition, ETile.deadEnd))
                    {
                        found.Add(currentPosition);
                    }
                }
            }
        }

        foreach (var finding in found)
        {
            OverwriteTile(finding, ETile.wall);
            RecursiveFilterEmptyOutUnnecessary();
        }
    }

    bool CheckNeighbourOfType(Vector2Int positionToCheck, ETile typeToCheck)
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int nextPosition = positionToCheck + direction;
            if (IsInBoundsCheck(nextPosition) && (tiles[nextPosition.x, nextPosition.y].Type == typeToCheck))
            {
                return true;
            }
        }

        return false;
    }
    #endregion


    #region PathChecker

    bool DoesPathExistForAll(out List<Vector2Int> unconnectedPositions)
    {
        List<Vector2Int> foundPositions = new List<Vector2Int>();

        foreach (var currentPoint in fixedGenerationPoints)
        {

            if (DoesPathExist(currentPoint.GetVector2Int(labyrinthWidth, labyrinthHeight), fixedGenerationPoints[0].GetVector2Int(labyrinthWidth, labyrinthHeight))) // "0" bc if it fits zero-entry it fits any
            {
                continue;
            }
            else
            {
                foundPositions.Add(currentPoint.GetVector2Int(labyrinthWidth, labyrinthHeight));
            }
        }

        if (foundPositions.Count == 0)
        {
            unconnectedPositions = new List<Vector2Int>();
            return true;
        }

        unconnectedPositions = foundPositions;
        return false;
    }


    // This is not my own method, it is the Breadth-First Search
    private bool DoesPathExist(Vector2Int start, Vector2Int end)
    {
        if (!IsInBoundsCheck(start) || !IsInBoundsCheck(end))
        {
            return false;
        }

        if (tiles[start.x, start.y].Type == ETile.wall || tiles[end.x, end.y].Type == ETile.wall)
        {
            return false;
        }

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == end)
            {
                return true;
            }

            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighbor = current + direction;

                if (IsInBoundsCheck(neighbor) &&
                    tiles[neighbor.x, neighbor.y].Type != ETile.wall &&
                    !visited.Contains(neighbor))
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }
        }

        return false;
    }

    #endregion


    void CheckAndSetTileIdentity(Vector2Int position)
    {
        if (tiles[position.x, position.y].Type == ETile.wall)
            return;

        List<Vector2Int> paths = new List<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int positionToCheck = position + direction;
            if (IsInBoundsCheck(positionToCheck) && (tiles[positionToCheck.x, positionToCheck.y].Type != ETile.wall))
            {
                paths.Add(direction);
            }
        }

        // CornerCase
        if (paths.Count == 2 &&
            (paths.Contains(Vector2Int.up) && paths.Contains(Vector2Int.right) ||
            paths.Contains(Vector2Int.up) && paths.Contains(Vector2Int.left) ||
            paths.Contains(Vector2Int.down) && paths.Contains(Vector2Int.right) ||
            paths.Contains(Vector2Int.down) && paths.Contains(Vector2Int.left)))
        {
            OverwriteTile(position, ETile.corner);
        }
        // Straight Case
        else if (paths.Count == 2 &&
            (paths.Contains(Vector2Int.left) && paths.Contains(Vector2Int.right) ||
            paths.Contains(Vector2Int.up) && paths.Contains(Vector2Int.down)))
        {
            OverwriteTile(position, ETile.line);
        }
        // Ternary(and above) Case
        else if (paths.Count == 3)
        {
            OverwriteTile(position, ETile.ternary);
        }
        else if (paths.Count == 4)
        {
            OverwriteTile(position, ETile.crossing);
        }
        // DeadEnd Case
        else if (paths.Count == 1)
        {
            OverwriteTile(position, ETile.deadEnd);
        }
    
    }

    void OverwriteTile(Vector2Int position, ETile tileType)
    {
        LabTile newTile;
        newTile = tiles[position.x, position.y];
        newTile.SetTileTypeTo(tileType);
        tiles[position.x, position.y] = newTile;
    }

    bool IsInBoundsCheck(Vector2Int position)
    {
        return position.x >= 0 && position.x < labyrinthWidth
            && position.y >= 0 && position.y < labyrinthHeight;
    }


}


