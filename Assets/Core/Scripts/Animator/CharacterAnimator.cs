using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void PlayAnim(string triggerName)
    {
        if (anim != null)
        {
            anim.SetTrigger(triggerName);
            Debug.Log($"<color=orange>Play {triggerName} Animation!</color>");
        }
    }

    public float GetAnimDuration()
    {
        if (anim == null) return 1f;

        if (anim.IsInTransition(0))
        {
            return anim.GetNextAnimatorStateInfo(0).length;
        }
        else
        {
            return anim.GetCurrentAnimatorStateInfo(0).length;
        }
    }
}
