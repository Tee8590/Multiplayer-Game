using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

public class PlayerNetwork : NetworkBehaviour
{

    [SerializeField] private float positionRange = 3f;
    //Network variable section start.
    //Using Network variable we can send data between client and host
    //This way we do not need to give all access to the client.
    //You must inherit NetworkBehaviour to use NetworkVariable
    //You can just go to the NetworkBehaviour file there u can see that it's inherits MonoBehaviour. 
    private NetworkVariable<customType> randomVariable = new NetworkVariable<customType>(
    new customType
    {
        _int = 0,
        _bool = true,
        _string = "Let's Start",
    }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    //It's a function that is provided by NetworkBehaviour 
    public override void OnNetworkSpawn()
    {
        //randomVariable.OnValueChanged += (customType previousValue, customType newValue) => {

        //};

        transform.position = new Vector3(Random.Range(positionRange, -positionRange), 0, Random.Range(positionRange, -positionRange));
    }

    //I just created this to show that we can use custom types as well,
    //but whatever type you use, it must be a value type like int or bool etc
    //that's why we cant use string because its not a value type (eg: class , string etc ..)
    public struct customType : INetworkSerializable
    {
        public int _int;
        public bool _bool;
        public FixedString128Bytes _string;

        //We need to implement this because we are using INetworkSerializable interface.
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref _string);
        }
    }
    //Network variable section end.
    private void Update()
    {
        if(!IsOwner) return;

        Vector3 moveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        float moveSpeed = 3f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    //RPC start.
    //Othe way to share data is by using RPC method

    [ServerRpc]
    private void ServerServerRpc(ServerRpcParams serverRpcParams)
    {
        //do things here.
    }

    //Client rpc can only be sent datas by serve not client and server rpc can be used by both.
    [ClientRpc]
    private void ClientClientRpc(ClientRpcParams clientRpcParams)
    {
        //do things here.
    }

}
