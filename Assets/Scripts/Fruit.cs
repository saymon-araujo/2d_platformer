using System;
using UnityEngine;

public class Fruit : MonoBehaviour
{

    private GameManager _gameManager;
    private Animator _anim;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        
        if (player != null)
        {
            _gameManager.AddFruit();
            Destroy(gameObject);
        }
    }
    }
