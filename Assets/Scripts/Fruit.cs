using System;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public enum FruitType { Apple, Banana, Cherry, Kiwi, Melon, Orange,Pineapple, Strawberry  }

public class Fruit : MonoBehaviour
{

    [SerializeField] private FruitType fruitType;
    [SerializeField] private GameObject pickupVFX;
    private GameManager _gameManager;
    private Animator _anim;

    private void Awake()
    {
        _anim = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;
        SetRandomLookIfNeeded();
    }

    private void SetRandomLookIfNeeded()
    {
        
        if(!_gameManager.FruitsHaveRandomLook())
        {
            UpdateFruitVisuals();
            return;
        }
        
        var random = Random.Range(0, 8);
        _anim.SetFloat("fruitIndex", random);
    }

    private void UpdateFruitVisuals()
    {
        _anim.SetFloat("fruitIndex", (int)fruitType);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        
        if (player != null)
        {
            _gameManager.AddFruit();
            Destroy(gameObject);

            var newVFX = Instantiate(pickupVFX, transform.position, Quaternion.identity);
            
            Destroy(newVFX, 1f);
            
        }
    }
    }
