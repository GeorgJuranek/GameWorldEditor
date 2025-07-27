using UnityEngine;

public class CarpetColorChanger : MonoBehaviour
{
    [SerializeField]
    Renderer carpetRenderer;

    public void ChangeColor(Color newColor)
    {
        if (Application.isEditor && !Application.isPlaying) 
        {
            Material tempMaterial = new Material(carpetRenderer.sharedMaterial);
            tempMaterial.color = newColor;

            carpetRenderer.material = tempMaterial;
        }
        else
        {
            carpetRenderer.material.SetColor("_BaseColor", newColor);
        }
    }
}
