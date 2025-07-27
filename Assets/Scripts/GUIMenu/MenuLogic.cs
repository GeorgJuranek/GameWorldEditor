using UnityEngine;

public class MenuLogic : MonoBehaviour
{
    [SerializeField]
    GameObject CreditsBox;

    [SerializeField]
    GameObject ControlsBox;


    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 0;
    }

    public void OnResume()
    {
        if (CreditsBox.activeSelf)
        {
            CreditsBox.SetActive(false);
        }
        if (ControlsBox.activeSelf)
        {
            ControlsBox.SetActive(false);
        }

        Time.timeScale = 1;
        this.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnControls()
    {
        if (CreditsBox.activeSelf)
        {
            CreditsBox.SetActive(false);
        }

        ControlsBox.SetActive(!ControlsBox.activeSelf);
    }

    public void OnCredits()
    {
        if (ControlsBox.activeSelf)
        {
            ControlsBox.SetActive(false);
        }

        CreditsBox.SetActive(!CreditsBox.activeSelf);

    }

    public void OnQuit()
    {
        Application.Quit();
    }

    private void OnDisable()
    {
        OnResume();
    }
}
