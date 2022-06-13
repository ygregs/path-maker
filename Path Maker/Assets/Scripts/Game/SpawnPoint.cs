using UnityEngine;
using Unity.Netcode;

public class SpawnPoint : NetworkBehaviour
{
    public NetworkVariable<bool> IsOccupied = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> IsAsian;

    public void SetOccupied(bool state)
    {
        if (IsServer)
        {
            IsOccupied.Value = state;
        }
        else
        {
            SetOccupied_ServerRpc(state);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SetOccupied_ServerRpc(bool state)
    {
        IsOccupied.Value = state;
    }
}