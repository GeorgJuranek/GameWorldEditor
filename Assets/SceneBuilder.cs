using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneBuilder : MonoBehaviour
{

    [SerializeField] ScriptableLabyrinthInformations scriptableLabyrinthInformation;
    [SerializeField] ScriptableRoomTiles scriptableRoomTiles;
    [SerializeField] ScriptableObjectSpawnForRoom scriptableRoomSpawn;

    [SerializeField] Vector3 centerPosition = Vector3.zero;



    LabyrinthInformationBuilder labyrinthInformationBuilder;
    LabyrinthTileBuilder labyrinthTileBuilder;
    LabyrinthObjectsSpawner labyrinthObjectsSpawner;

    LabTile[,] tiles;
    List<LabTile[]> roomGroups;
    List<LabTile> pureMaze;

    private void Awake()
    {
        GenerateMaze(scriptableLabyrinthInformation, scriptableRoomTiles, scriptableRoomSpawn, centerPosition);
    }

    public void GenerateMaze(ScriptableLabyrinthInformations scriptableLabyrinthInformations, ScriptableRoomTiles scriptableRoomTiles, ScriptableObjectSpawnForRoom scriptableObjectSpawnForRoom, Vector3 centerPosition)
    {

        //Information generating
        labyrinthInformationBuilder = new LabyrinthInformationBuilder(scriptableLabyrinthInformations);
        tiles = labyrinthInformationBuilder.Build(out roomGroups, out pureMaze);

        //Tiles generating
        labyrinthTileBuilder = new LabyrinthTileBuilder(scriptableRoomTiles);
        labyrinthTileBuilder.Build(tiles, centerPosition);

        //Spawn stuff
        labyrinthObjectsSpawner = new LabyrinthObjectsSpawner(scriptableObjectSpawnForRoom);
        labyrinthObjectsSpawner.SetMainElements(pureMaze);
        labyrinthObjectsSpawner.SetRoomElements(roomGroups, tiles);

    }
}
