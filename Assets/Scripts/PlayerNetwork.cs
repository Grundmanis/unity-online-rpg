using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{

    // GENERIC CAN BE ONLY VALUE TYPES (which might be nullable), NO REFERECE TYPES LIKE OBJECT/CLASS/array/string
    private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(
        new MyCustomData {
        _int = 56,
        _bool = true,
        }, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Owner
    );

    // HAS TO BE PUBLIC
    // TODO: what is STRUCT and virtual ?????????
    // error in lower version, need to implement INetworkSerializable and implement the method // https://www.youtube.com/watch?v=3yuBOB3VrCk
    public struct MyCustomData {
        public int _int;
        public bool _bool;
        // cant use strings, but CAN use FixedString type with specific byte count, 1 char = 1 byte
    }

    // NO AWARE OR START related to NETWORK
    public override void OnNetworkSpawn()
    {
        // base.OnNetworkSpawn();
        randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) => {
            Debug.Log(OwnerClientId + "; number: " + newValue._int + "; bool: " + newValue._bool);
        };
    }

    private void Update()
    {

        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.T)) {
            randomNumber.Value = new MyCustomData {
                _int = Random.Range(0, 100),
                _bool = !randomNumber.Value._bool
            };
        }

        Vector3 moveDirection = new Vector3(0,0,0);

        if (Input.GetKey(KeyCode.W)) moveDirection.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDirection.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDirection.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDirection.x = +1f;

        float moveSpeed = 3f;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}
