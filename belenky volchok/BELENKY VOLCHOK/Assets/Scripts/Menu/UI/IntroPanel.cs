using UnityEngine;

public class SceneIntroPanel : MonoBehaviour
{
    [SerializeField] private GameObject[] panels;
    
    private int currentIndex = 0;
    
    private void Start()
    {
        if (panels.Length > 0)
            panels[0].SetActive(true);
    }
    
    private void Update()
    {
        if (Input.anyKeyDown)
        {
            panels[currentIndex].SetActive(false);
            currentIndex++;
            
            if (currentIndex < panels.Length)
                panels[currentIndex].SetActive(true);
            else
                this.enabled = false;
        }
    }
}
