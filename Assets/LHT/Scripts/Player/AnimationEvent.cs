using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    public void FootstepSound()
    {
        if (PlayerMove.Instance.isInGrass == true)
        {
            EventHandler.CallPlaySoundEvent(SoundName.WalkOnGrass);
        }
        else
        {
            EventHandler.CallPlaySoundEvent(SoundName.WalkOnSoft);
        }
    }
}
