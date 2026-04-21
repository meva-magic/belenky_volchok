using UnityEngine;

public class DynamicSound : MonoBehaviour
{
    public string soundName;
    public Transform player;
    public float innerRadius = 5f;
    public float outerRadius = 15f;
    
    private AudioSource source;
    private Sound soundData;
    private float defaultVolume;
    
    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
        
        if (AudioManager.instance != null)
        {
            soundData = System.Array.Find(AudioManager.instance.sounds, s => s.name == soundName);
            if (soundData != null)
            {
                source = soundData.source;
                defaultVolume = soundData.volume;
            }
        }
    }
    
    void Update()
    {
        if (player == null || source == null) return;
        
        float distance = Vector3.Distance(transform.position, player.position);
        float volume = defaultVolume;
        
        if (distance > outerRadius)
        {
            volume = 0f;
        }
        else if (distance > innerRadius)
        {
            float t = (distance - innerRadius) / (outerRadius - innerRadius);
            volume = Mathf.Lerp(defaultVolume, 0f, t);
        }
        
        source.volume = volume;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, innerRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, outerRadius);
    }
}
