using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace PathMaker
{
    public class SpawnManager : NetworkBehaviour
    {
        public Transform[] AsianSpawnsArray;
        public Transform[] GreekSpawnsArray;
        public NetworkVariable<bool> firstAsianSpawn = new NetworkVariable<bool>(true);
        public NetworkVariable<bool> secondAsianSpawn = new NetworkVariable<bool>(true);

        public NetworkVariable<bool> firstGreekSpawn = new NetworkVariable<bool>(true);
        public NetworkVariable<bool> secondGreekSpawn = new NetworkVariable<bool>(true);


        public void ResetSpawns()
        {
            if (IsServer)
            {
                firstAsianSpawn.Value = true;
                secondAsianSpawn.Value = true;
                firstGreekSpawn.Value = true;
                secondGreekSpawn.Value = true;
            }
            else
            {
                ResetSpawnsServerRpc();
            }
        }

        [ServerRpc]
        void ResetSpawnsServerRpc()
        {
            firstAsianSpawn.Value = true;
            secondAsianSpawn.Value = true;
            firstGreekSpawn.Value = true;
            secondGreekSpawn.Value = true;
        }
        public void SetSpawnDisponibility(int num, TeamState team)
        {
            if (IsServer)
            {
                if (num == 1)
                {
                    if (team == TeamState.AsianTeam)
                    {
                        firstAsianSpawn.Value = false;
                    }
                    else
                    {
                        firstGreekSpawn.Value = false;
                    }
                }
                else
                {
                    if (team == TeamState.AsianTeam)
                    {
                        secondAsianSpawn.Value = false;
                    }
                    else
                    {
                        secondGreekSpawn.Value = false;
                    }
                }
            }
            else
            {
                SetSpawnDisponibilityServerRpc(num, team);
            }
        }

        [ServerRpc]
        void SetSpawnDisponibilityServerRpc(int num, TeamState team)
        {
            if (num == 1)
            {
                if (team == TeamState.AsianTeam)
                {
                    firstAsianSpawn.Value = false;
                }
                else
                {
                    firstGreekSpawn.Value = false;
                }
            }
            else
            {
                if (team == TeamState.AsianTeam)
                {
                    secondAsianSpawn.Value = false;
                }
                else
                {
                    secondGreekSpawn.Value = false;
                }
            }
        }
    }
}