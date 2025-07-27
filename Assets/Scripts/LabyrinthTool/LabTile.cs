using UnityEngine;

public class LabTile
{
    public ETile Type { get; private set; }

    //private Color color;
    public Color Color { get; private set; }// tileRenderer.material.GetColor("_BaseColor"); }

    //TEST
    public Vector3 PositionInWorld { get; private set; }

    public Transform Represents { get; private set; }

    public Vector2Int PositionInLabTileContext { get; private set; }

    public LabTile(ETile type)
    {
        Type = type;
    }

    public LabTile(ETile type, Color color)
    {
        Type = type;
        Color = color;
    }

    public void SetTileTypeTo(ETile newTile)
    {
        Type = newTile;
    }

    public void SetTileColorTo(Color newColor)
    {
        //tileRenderer.material.SetColor("_BaseColor", newColor);
        Color = newColor;
    }

    public void SetPositionInWorld(Vector3 newPosition)
    {
        PositionInWorld = newPosition;
    }

    public void SetReference(Transform reference)
    {
        Represents = reference;
    }

    public void SetPositionInContext(Vector2Int newPosition)
    {
        PositionInLabTileContext = newPosition;
    }
}
