using System.Collections;
using UnityEngine;

public class GhostAnimation : MonoBehaviour
{
    [Header("Flicker Settings")]
    public float minInterval = 1f;
    public float maxInterval = 3f;

    private Animator animator;
    private bool isMoving = false;
    private bool isDisappearing = false;

    void Start()
    {
        animator = transform.Find("Visual").GetComponent<Animator>();

        StartCoroutine(FlickerLoop());
    }

    IEnumerator FlickerLoop()
    {
        while (!isDisappearing)
        {
            float waitTime = UnityEngine.Random.Range(
                minInterval,
                maxInterval
            );

            yield return new WaitForSeconds(waitTime);

            if (isDisappearing)
                yield break;

            if (isMoving)
                animator.SetTrigger("FlickerRun");
            else
                animator.SetTrigger("FlickerIdle");
        }
    }

    public void SetMoving(bool moving)
    {
        if (isMoving == moving)
            return;

        isMoving = moving;
        animator.SetBool("isMoving", moving);
    }

    public void Disappear()
    {
        isDisappearing = true;
        StopAllCoroutines();

        animator.ResetTrigger("FlickerIdle");
        animator.ResetTrigger("FlickerRun");
        animator.SetTrigger("Disappear");
    }
}
