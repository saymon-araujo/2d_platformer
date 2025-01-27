using UnityEngine;

public class EndPoint : MonoBehaviour
{
    private Animator _anim;

    private void Awake()
    {
        _anim = GetComponentInChildren<Animator>();
    }
        
    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();

        if(player != null)
        {
            _anim.SetTrigger("didFinish");
            Debug.Log("Level Completed!");
        }
    }

}
