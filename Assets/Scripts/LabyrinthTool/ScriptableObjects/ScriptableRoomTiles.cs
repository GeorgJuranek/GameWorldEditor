using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableRoomTiles", menuName = "ScriptableObjects/LabyrinthGenerator/RoomTilesInformation")]
public class ScriptableRoomTiles : ScriptableObject
{
    public int averageSize;
    public GameObject DeadEnd;
    public GameObject Line;
    public GameObject Corner;
    public GameObject Ternary;
    public GameObject Crossing;
    public GameObject NavMeshSurface;
}
