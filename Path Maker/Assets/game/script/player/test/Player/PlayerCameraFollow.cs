using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DilmerGames.Core.Singletons;
using Cinemachine;

public class PlayerCameraFollow : Singleton<PlayerCameraFollow>
{
    private CinemachineVirtualCamera cinemachineVirtualCamera;
    void Awake()
    {
        cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
    }
    public void FollowPlayer (Transform transform)
    {
        cinemachineVirtualCamera.Follow = transform;
    }
}
