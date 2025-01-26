using UnityEngine;

public class GameManager : MonoBehaviour
    {
    public static GameManager Instance;
    public Player Player { get; private set; }
    
    [Header("Fruit Settings")]
    public bool fruitsHaveRandomLook; 
    public int fruitsCollected;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddFruit()
    {
        fruitsCollected++;
    }

    public bool FruitsHaveRandomLook()
    {
        return fruitsHaveRandomLook;
    }
    }
