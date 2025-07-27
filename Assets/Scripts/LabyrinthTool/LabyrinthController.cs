using System.Collections.Generic;
using UnityEngine;

public class LabyrinthController
{

    LabyrinthInformationBuilder labyrinthInformationBuilder;
    LabyrinthTileBuilder labyrinthTileBuilder;
    LabyrinthObjectsSpawner labyrinthObjectsSpawner;

    LabTile[,] tiles;
    List<LabTile[]> roomGroups;
    List<LabTile> pureMaze;

    public void Build(ScriptableLabyrinthInformations scriptableLabyrinthInformations, ScriptableRoomTiles scriptableRoomTiles, ScriptableObjectSpawnForRoom scriptableObjectSpawnForRoom, Vector3 centerPosition)
    {
        
        //Information generating
        labyrinthInformationBuilder = new LabyrinthInformationBuilder(scriptableLabyrinthInformations);
        tiles = labyrinthInformationBuilder.Build( out roomGroups, out pureMaze);

        //Tiles generating
        labyrinthTileBuilder = new LabyrinthTileBuilder(scriptableRoomTiles);
        labyrinthTileBuilder.Build(tiles, centerPosition);

        //Spawn stuff
        labyrinthObjectsSpawner = new LabyrinthObjectsSpawner(scriptableObjectSpawnForRoom);
        labyrinthObjectsSpawner.SetMainElements(pureMaze);
        labyrinthObjectsSpawner.SetRoomElements(roomGroups, tiles);

    }
}
