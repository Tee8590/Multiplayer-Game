using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using static UnityEngine.GraphicsBuffer;
using TMPro;

public class Movement : NetworkBehaviour
{
    private Animator _animator;
    private NetworkAnimator _animator2;
    private Transform _transform;
    [SerializeField] private float positionRange = 3f;
    [SerializeField] private float speed = 5f;

    public override void OnNetworkSpawn()
    {
        transform.position = new Vector3(Random.Range(positionRange, -positionRange), 0, Random.Range(positionRange, -positionRange));
    }

    private void Start()
    {
        if (!IsOwner) return;

        _animator = GetComponent<Animator>();
        _animator2 = GetComponent<NetworkAnimator>();
        _transform = transform;
    }

    private void Update()
    {
        if (!IsOwner) return;

        HandleInput();
    }

    private void HandleInput()
    {
        Vector3 direction = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            direction += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A))
        {
            direction += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction += Vector3.right;
        }

        if (direction.magnitude > 0)
        {
            TriggerAnimation("trWalk");
            direction = direction.normalized;
            MovePlayer(direction);
        }
        else
        {
            TriggerAnimation("trIdle");
        }

        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
        {
            TriggerAnimation("trIdle");
        }
    }

    private void MovePlayer(Vector3 direction)
    {
        if (direction.magnitude > 0)
        {
            Vector3 targetPosition = _transform.position + (direction * speed * Time.deltaTime);
            _transform.rotation = GetToRotation(direction);
            _transform.position = Vector3.Lerp(_transform.position, targetPosition, 0.7f); //0.1f is the lerp factor
        }
    }

    private Quaternion GetToRotation(Vector3 direction)
    {
        Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
        return Quaternion.RotateTowards(_transform.rotation, toRotation, 720 * Time.deltaTime);
    }

    private void TriggerAnimation(string triggerName)
    {
        _animator2.SetTrigger(triggerName);
    }
}
