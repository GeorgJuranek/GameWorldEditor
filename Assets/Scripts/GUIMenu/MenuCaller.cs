using UnityEngine;

public class MenuCaller : MonoBehaviour
{
    [SerializeField]
    GameObject targetCanvas;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape))
        {
            targetCanvas.SetActive(!targetCanvas.activeSelf);
        }
    }
}
