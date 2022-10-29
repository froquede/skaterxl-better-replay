using GameManagement;
using ModIO.UI;
using ReplayEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityModManagerNet;

namespace BetterReplay
{
    class AnimationTimeTracker
    {
        public List<float> time = new List<float>();
        public List<float> animation_time = new List<float>();
        public List<string> animation_name = new List<string>();

        public void pushState(float time, float anim_time, string anim_name)
        {
            this.time.Add(time);
            this.animation_time.Add(anim_time);
            this.animation_name.Add(anim_name);
        }

        public void Shift()
        {
            this.time.RemoveAt(0);
            this.animation_time.RemoveAt(0);
            this.animation_name.RemoveAt(0);
        }
    }

    class AnimationTracker : MonoBehaviour
    {
        public AnimationTimeTracker tracker;
        public float nextRecordTime;
        public float spf = 60f;
        public Animator animator;
        public int BufferFrameCount;
        AnimatorStateInfo animationState;
        AnimatorClipInfo[] animatorClip;

        public void Start()
        {
            tracker = new AnimationTimeTracker();
            animator = GetComponent<Animator>();
            BufferFrameCount = Mathf.RoundToInt(ReplaySettings.Instance.FPS * ReplaySettings.Instance.MaxRecordedTime);
            ResetAnimator();
        }

        public void Update()
        {
            if (GameStateMachine.Instance.CurrentState.GetType() == typeof(ReplayState))
            {
                int index = getFrame();
                animator.StopPlayback();
                if (index >= 0 && tracker.animation_name[index] != null)
                {
                    animator.speed = ReplayEditorController.Instance.playbackController.TimeScale;
                    animator.Play(tracker.animation_name[index], 0, tracker.animation_time[index]);
                }
            }

            if (GameStateMachine.Instance.CurrentState.GetType() == typeof(PlayState))
            {
                if (animatorClip.Length > 0)
                {
                    if (animator.speed != 1) {
                        animator.speed = 1;
                        animator.Play(animatorClip[0].clip.name);
                    }
                    animationState = animator.GetCurrentAnimatorStateInfo(0);
                    tracker.pushState(PlayTime.time, animationState.normalizedTime, animatorClip[0].clip.name);

                    if (tracker.time.Count >= BufferFrameCount)
                    {
                        tracker.Shift();
                    }
                }
                else
                {
                    UnityModManager.Logger.Log($"Incompatible animator for now, removing tracker @ {gameObject.name}");
                    Destroy(gameObject.GetComponent<AnimationTracker>());
                }
            }
        }

        float last_time, last_anim_time;
        public void ResetAnimator()
        {
            animatorClip = animator.GetCurrentAnimatorClipInfo(0);
            animationState = animator.GetCurrentAnimatorStateInfo(0);
            /*last_time = Time.unscaledTime;
            last_anim_time = animationState.normalizedTime;*/
        }

        public int getFrame()
        {
            for (int i = tracker.time.Count - 1; i >= 0; i--)
            {
                if (tracker.time[i] <= ReplayEditorController.Instance.playbackController.CurrentTime) return i;
            }
            return -1;
        }
    }
}
