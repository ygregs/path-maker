using System;
using UnityEngine;

public class EventTesting : MonoBehaviour
{

    public event EventHandler OnSpacePressed;

    private void PrintOnSpacePressed(object sender, EventArgs e)
    {
        print("Space bar pressed !");
    }

    private void Start()
    {
        OnSpacePressed += PrintOnSpacePressed;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnSpacePressed?.Invoke(this, EventArgs.Empty);
        }
    }

}