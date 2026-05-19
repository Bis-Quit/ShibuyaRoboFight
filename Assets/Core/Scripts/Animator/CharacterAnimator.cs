using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public Animator anim;

    public void PlayAnim(string animName)
    {
        if (anim != null) anim.Play(animName);
    }

    public float GetAnimDuration(string animName)
    {
        if (anim == null) return 1f;

        RuntimeAnimatorController ac = anim.runtimeAnimatorController;
        foreach (AnimationClip clip in ac.animationClips)
        {
            if (clip.name == animName) return clip.length;
        }
        return 1f;
    }
}