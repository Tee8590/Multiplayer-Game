//using System;
//using Unity.Netcode;
//using UnityEngine;

//public class Movement : NetworkBehaviour
//{
//    private Animator _animator;
//    private Transform _transform;
//    [SerializeField] private float positionRange = 3f;
//    [SerializeField] private float speed = 5f;

//    public override void OnNetworkSpawn()
//    {
//        transform.position = new Vector3(UnityEngine.Random.Range(positionRange, -positionRange), 0, UnityEngine.Random.Range(positionRange, -positionRange));
//    }
//    private void Start()
//    {
//        if (!IsOwner) return;

//        _animator = GetComponent<Animator>();
//        _transform = transform;
//    }

//    private void Update()
//    {
//        if (!IsOwner) return;

//        HandleInput();
//    }

//    //private void HandleInput()
//    //{
//    //    float horizontal = Input.GetAxis("Horizontal");
//    //    float vertical = Input.GetAxis("Vertical");

//    //    Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;
//    //    if (direction.magnitude > 0)
//    //    {
//    //        Debug.Log(direction.magnitude);
//    //        RequestMoveServerRpc(direction);
//    //        TriggerAnimationServerRpc("trWalk");
//    //    }
//    //    else
//    //    {
//    //        TriggerAnimationServerRpc("trIdle");
//    //    }
//    //}

//    private void HandleInput()
//    {
//        Vector3 direction = Vector3.zero;

//        if (Input.GetKey(KeyCode.W))
//        {
//            direction += Vector3.forward;
//        }
//        if (Input.GetKey(KeyCode.S))
//        {
//            direction += Vector3.back;
//        }
//        if (Input.GetKey(KeyCode.A))
//        {
//            direction += Vector3.left;
//        }
//        if (Input.GetKey(KeyCode.D))
//        {
//            direction += Vector3.right;
//        }

//        if (direction.magnitude > 0)
//        {
//            TriggerAnimationServerRpc("trWalk");
//            direction = direction.normalized;
//            RequestMoveServerRpc(direction);
//        }
//        else
//        {
//            TriggerAnimationServerRpc("trIdle");
//        }

//        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
//        {
//            TriggerAnimationServerRpc("trIdle");
//        }
//    }


//    [ServerRpc(RequireOwnership = false)]
//    private void RequestMoveServerRpc(Vector3 direction)
//    {
//        Vector3 newPosition = transform.position + (direction * speed * Time.deltaTime);
//        transform.position = newPosition;

//        if (direction.magnitude > 0)
//        {
//            transform.rotation = GetToRotation(direction);
//        }
//        UpdatePositionAndAnimationClientRpc(newPosition, direction);
//    }

//    [ClientRpc]
//    private void UpdatePositionAndAnimationClientRpc(Vector3 newPosition, Vector3 direction)
//    {
//        if (IsOwner) return;

//        transform.position = newPosition;

//        if (direction.magnitude > 0)
//        {
//            transform.rotation = GetToRotation(direction);
//        }

//        TriggerAnimationClientRpc("trWalk");
//    }

//    [ServerRpc(RequireOwnership = false)]
//    private void TriggerAnimationServerRpc(string triggerName)
//    {
//        if (IsOwner) return;
//        TriggerAnimationClientRpc(triggerName); // Notify all clients
//    }

//    [ClientRpc]
//    private void TriggerAnimationClientRpc(string triggerName)
//    {
//        if (IsOwner) return;
//        _animator.SetTrigger(triggerName);
//    }
//    private Quaternion GetToRotation(Vector3 direction)
//    {
//        Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
//        return Quaternion.RotateTowards(transform.rotation, toRotation, 720 * Time.deltaTime);
//    }
//}