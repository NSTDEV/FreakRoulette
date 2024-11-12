using UnityEngine;

public class MenuButtons : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnClickExit();
        }
    }

    public void OnClickExit()
    {
        Application.Quit();
    }
}
