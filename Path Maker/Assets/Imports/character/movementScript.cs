using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movementScript : MonoBehaviour
{
    public Animator anim;

    private bool run = false;
    private bool crouch = false;
    private bool jump = false;
    private float Frun;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            run = !run;
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            crouch = !crouch;
        }

        jump = Input.GetKeyDown("space");

        anim.SetBool("jump", jump);
        anim.SetBool("crouch", crouch);
        anim.SetBool("run", run);

        Frun = Input.GetAxis("Vertical") * 2;

        if (run)
        {
            anim.SetFloat("vertical", Frun);
        }
        else
        {
            anim.SetFloat("vertical", Input.GetAxis("Vertical"));
        }

        anim.SetFloat("horizontal", Input.GetAxis("Horizontal"));
    }
}
