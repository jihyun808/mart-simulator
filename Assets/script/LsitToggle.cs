using UnityEngine;

public class ListToggle : MonoBehaviour
{
    public GameObject listPanel;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool nextState = !listPanel.activeSelf;
            listPanel.SetActive(nextState);
        }
    }
}
