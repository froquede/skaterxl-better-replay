using System;
using UnityEngine;
using UnityModManagerNet;
using SkaterXL.Core;
using UnityEngine.SceneManagement;

namespace BetterReplay
{
    class BetterReplay : MonoBehaviour
    {
        string scene = "";

        public void Start()
        {

        }

        public void FixedUpdate()
        {
            string actual_scene = SceneManager.GetActiveScene().name;
            
            if (actual_scene != scene)
            {
                AddObjectTrackers();
                scene = actual_scene;
            }
        }

        public void AddObjectTrackers()
        {
            UnityModManager.Logger.Log("Checking Hinges and RigidBodies for replay tracking...");
            string[] internals = { "Gameplay Camera", "NewIKAnim", "NewSteezeIK", "NewSkater", "Pin", "Camera Rig", "CenterOfMassPlayer", "Lean Proxy", "Coping Detection", "Skater Target", "Front Truck", "Back Truck", "Skateboard", "Slater_foot_r", "Slater_Leg_r", "Slater_UpLeg_r", "Slater_foot_l", "Slater_Leg_l", "Slater_UpLeg_l", "Skater_hand_r", "Skater_ForeArm_r", "Skater_Arm_r", "Skater_hand_l", "Skater_ForeArm_l", "Skater_Arm_l", "Skater_Head", "Skater_Spine2", "Skater_Spine", "Skater_pelvis", "Skater_foot_r", "Skater_Leg_r", "Skater_UpLeg_r", "Skater_foot_l", "Skater_Leg_l", "Skater_UpLeg_l" };

            var cloths = UnityEngine.Object.FindObjectsOfType<UnityEngine.Cloth>();
            for (int i = 0; i < cloths.Length; i++)
            {
                UnityEngine.Cloth go = cloths[i];
                bool add = true;
                for (int n = 0; n < internals.Length; n++)
                {
                    if (go.name == internals[n]) add = false;
                }

                ObjectTracker ot = go.GetComponent<ObjectTracker>();

                if (add && ot == null)
                {
                    go.gameObject.AddComponent<ObjectTracker>();
                    UnityModManager.Logger.Log($"Added ObjectTracker - Cloth @ {go.gameObject.name}");
                }
            }

            var hinges = UnityEngine.Object.FindObjectsOfType<UnityEngine.HingeJoint>();
            for (int i = 0; i < hinges.Length; i++)
            {
                UnityEngine.HingeJoint go = hinges[i];
                bool add = true;
                for (int n = 0; n < internals.Length; n++)
                {
                    if (go.name == internals[n]) add = false;
                }

                ObjectTracker ot = go.GetComponent<ObjectTracker>();

                if (add && ot == null)
                {
                    go.gameObject.AddComponent<ObjectTracker>();
                    UnityModManager.Logger.Log($"Added ObjectTracker - Hinge @ {go.gameObject.name}");
                }
            }

            var rbs = UnityEngine.Object.FindObjectsOfType<UnityEngine.Rigidbody>();
            for (int i = 0; i < rbs.Length; i++)
            {
                UnityEngine.Rigidbody go = rbs[i];
                bool add = true;
                for (int n = 0; n < internals.Length; n++)
                {
                    if (go.name == internals[n]) add = false;
                }

                ObjectTracker ot = go.GetComponent<ObjectTracker>();

                if (add && ot == null)
                {
                    go.gameObject.AddComponent<ObjectTracker>();
                    UnityModManager.Logger.Log($"Added ObjectTracker - RB @ {go.gameObject.name}");
                }
            }
        }
    }
}
