using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
    {
    public static GameManager Instance;
    
    [Header("Player Settings")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float respawnDelay = 1.25f;
    public Player Player { get; private set; }
    
    [Header("Fruit Settings")]
    public bool fruitsHaveRandomLook; 
    public int fruitsCollected;
    public int totalFruitsAmount;
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


    private void Start()
    {
        GetTotalAmountOfFruits();
    }

    private void GetTotalAmountOfFruits()
    {
        Fruit[] allFruits = FindObjectsByType<Fruit>(FindObjectsSortMode.None);
        totalFruitsAmount = allFruits.Length;
    }

    public void AddFruit()
    {
        fruitsCollected++;
    }

    public bool FruitsHaveRandomLook()
    {
        return fruitsHaveRandomLook;
    }

    public void RespawnPlayer() => StartCoroutine(RespawnPlayerRoutine());

    private IEnumerator RespawnPlayerRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        
        Player = Instantiate(playerPrefab, respawnPoint.position, Quaternion.identity).GetComponent<Player>();
    }

    public void UpdateRespawnPosition(Transform newPosition)
    {
        respawnPoint.position = newPosition.position;
    }

    }
