using UnityEngine;

public class PlayerSanity : MonoBehaviour
{
    public float maxSanity = 100f;
    public float sanity;
    
    [SerializeField] private float decreaseSanityAmount = 1f;
    
    public bool isDead;
    public static PlayerSanity instance;
    
    private void Awake()
    {
        instance = this;
    }
    
    private void Start()
    {
        sanity = maxSanity;
    }
    
    private void Update()
    {
        if (sanity <= 0)
        {
            isDead = true;
        }
        
        sanity -= decreaseSanityAmount * Time.deltaTime;
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            TakeDamage(20);
        }
        
        if (Input.GetKeyDown(KeyCode.H))
        {
            Heal(20);
        }
    }
    
    public void TakeDamage(int damage)
    {
        sanity -= damage;
    }
    
    public void RestoreSanity(int amount)
    {
        sanity = Mathf.Min(sanity + amount, maxSanity);
    }
    
    public void Heal(int amount)
    {
        sanity = Mathf.Min(sanity + amount, maxSanity);
    }
}
