using Unity.Netcode;
using UnityEngine;

public class CollisionDetection : NetworkBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision != null)
        {
            Debug.Log("kjhgskdgf");
        }
    }
}
