using System;
using UnityEngine;

public class EventTestingSubscriber : MonoBehaviour
{

    private void Start()
    {
        EventTesting eventTesting = GetComponent<EventTesting>();
        eventTesting.OnSpacePressed += OnSpacePressedFunction;
    }

    private void OnSpacePressedFunction(object sender, EventArgs e)
    {
        print("Space!");
    }
}