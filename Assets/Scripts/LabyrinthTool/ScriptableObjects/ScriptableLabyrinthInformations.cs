using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableLabyrinthInformation", menuName = "ScriptableObjects/LabyrinthGenerator/StructureInformation")]
public class ScriptableLabyrinthInformations : ScriptableObject
{
    public int labyrinthWidth;
    public int labyrinthHeight;
    public int LabyrinthDensity;
    public List<FixedGenerationPoint> fixedPoints;

}


// This Container is used only in the context of the ScriptableLabyrinthInformations
[System.Serializable]
public class FixedGenerationPoint
{
    [Range(0, 1)] public float xInPercent;
    [Range(0, 1)] public float yInPercent;

    public Vector2Int GetVector2Int(int labyrinthWidth, int labyrinthHeight)
    {
        return new Vector2Int((int)(xInPercent * (labyrinthWidth - 1)), (int)(yInPercent * (labyrinthHeight - 1)));
    }
}