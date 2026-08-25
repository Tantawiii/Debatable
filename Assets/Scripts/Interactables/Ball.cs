using UnityEngine;

public class Ball : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("BALL: Interacted with " + gameObject.name);
    }
}
