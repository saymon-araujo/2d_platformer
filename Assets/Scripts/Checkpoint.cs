using System;
using Unity.VisualScripting;
using UnityEngine;

public class Checkpoint : MonoBehaviour
    {

    private Animator _anim;
    private bool _isActive;

    private void Awake()
    {
        _anim = GetComponentInChildren<Animator>();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if(_isActive) return;
        
        Player player = other.GetComponent<Player>();

        if(player != null)
        {
            ActivateCheckpoint();
        }
    }

    private void ActivateCheckpoint()
    {
        _isActive = true;
        _anim.SetTrigger("isActive");
        GameManager.Instance.UpdateRespawnPosition(transform);
    }
    
    }
