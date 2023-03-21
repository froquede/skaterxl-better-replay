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
    class TransformTrackerStruct
    {
        public List<float> time = new List<float>();
        public List<Vector3> position = new List<Vector3>();
        public List<Quaternion> rotation = new List<Quaternion>();

        public void pushState(float time, Vector3 position, Quaternion rotation)
        {
            this.time.Add(time);
            this.position.Add(position);
            this.rotation.Add(rotation);
        }

        public void Shift()
        {
            this.time.RemoveAt(0);
            this.position.RemoveAt(0);
            this.rotation.RemoveAt(0);
        }
    }

    class TransformTracker : MonoBehaviour
    {
        public TransformTrackerStruct tracker;
        public int BufferFrameCount;
        public Transform replay_object, tracked_object;

        public void Start()
        {
            tracker = new TransformTrackerStruct();
            BufferFrameCount = Mathf.RoundToInt(ReplaySettings.Instance.FPS * ReplaySettings.Instance.MaxRecordedTime);
        }

        public void Update()
        {
            if (GameStateMachine.Instance.CurrentState.GetType() == typeof(ReplayState))
            {
                int index = getFrame();
                if (index >= 0 && tracker.rotation[index] != null && tracker.position[index] != null)
                {
                    if (replay_object)
                    {
                        replay_object.rotation = Quaternion.Slerp(replay_object.rotation, tracker.rotation[index], Time.smoothDeltaTime * 16f);
                        //replay_object.position = tracker.position[index];
                    }
                }
            }

            if (GameStateMachine.Instance.CurrentState.GetType() == typeof(PlayState))
            {
                tracker.pushState(PlayTime.time, tracked_object.position, tracked_object.rotation * ((tracked_object.gameObject.name == "Wheel2" || tracked_object.gameObject.name == "Wheel4") ? Quaternion.Euler(0, 180, 0) : Quaternion.identity));

                if (tracker.time.Count >= BufferFrameCount)
                {
                    tracker.Shift();
                }
            }
        }

        public int getFrame()
        {
            for (int i = 0; i < tracker.time.Count; i++)
            {
                if (tracker.time[i] >= ReplayEditorController.Instance.playbackController.CurrentTime) return i;
            }
            return -1;
        }
    }
}
