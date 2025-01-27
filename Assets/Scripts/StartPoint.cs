using UnityEngine;

public class StartPoint : MonoBehaviour
{
    private Animator _anim;

    private void Awake()
    {
        _anim = GetComponentInChildren<Animator>();
    }
            
    private void OnTriggerExit2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();

        if(player != null)
        {
            _anim.SetTrigger("didExit");
        }
    }
}
