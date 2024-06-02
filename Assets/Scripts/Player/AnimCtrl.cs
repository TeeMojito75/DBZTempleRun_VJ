using System.Collections;
using System.Collections.Generic;
using TempleRun.Player;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;

public class AnimCtrl : MonoBehaviour
{
    public Animator animator;
    public PC2 pc2;
    // Start is called before the first frame update
    void Start()
    {
        pc2 = GetComponentInParent<PC2>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PlayerManager.isGameStarted)
        {
            return;
        }

        animator.SetBool("isGameStarted", true);

        

        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            StartCoroutine(Jump());
        }

        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            StartCoroutine(Slide());
        }
    }

    private IEnumerator Slide()
    {
        animator.SetBool("isSliding", true);
        yield return new WaitForSeconds(1.3f);
        animator.SetBool("isSliding", false);
    }

    private IEnumerator Jump()
    {
        animator.SetBool("Jumped", true);
        yield return new WaitForSeconds(0.8f);
        animator.SetBool("Jumped", false);
    }
}
