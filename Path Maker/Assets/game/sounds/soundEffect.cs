using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundEffect : MonoBehaviour
{
    // Start is called before the first frame update
   
    public AudioSource jumpSound;

   
    // Update is called once per frame
    void Update()
    {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                
                    jumpSound.Play();
                    Debug.Log("sound Jump");
            }
     }
  
}
