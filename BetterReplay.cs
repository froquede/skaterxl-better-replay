using System;
using UnityEngine;
using UnityModManagerNet;
using SkaterXL.Core;
using ReplayEditor;
using UnityEngine.SceneManagement;
using GameManagement;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;

namespace BetterReplay
{
    class BetterReplay : MonoBehaviour
    {
        public string scene = "";
        public Type state;
        int count = 0;
        GameObject XLGraphics;
        Light XLGLight;

        public void Start()
        {
            PlayerController.Instance.pinMover.maxHeight = float.PositiveInfinity;
            getReplayEditor();
            //CheckXLGraphicsLight();
            //CreateFixLight();
        }

        void CheckXLGraphicsLight()
        {
            HDAdditionalLightData[] list = FindObjectsOfType(typeof(HDAdditionalLightData)) as HDAdditionalLightData[];
            foreach (HDAdditionalLightData obj in list)
            {
                Component[] components = obj.gameObject.GetComponents<MonoBehaviour>();
                foreach (Component component in components)
                {
                    if (component.GetType().ToString().Contains("XLGraphics"))
                    {
                        XLGraphics = obj.gameObject;
                        if (XLGraphics.GetComponent<ObjectTracker>() == null)
                        {
                            XLGraphics.AddComponent<BoxCollider>().enabled = false;
                            XLGraphics.AddComponent<Rigidbody>();
                            XLGraphics.AddComponent<ObjectTracker>();
                        }

                        XLGLight = XLGraphics.GetComponent<Light>();
                        return;
                    }
                }
            }
        }

        void CreateFixLight()
        {

        }

        Transform replay;
        ReplayEditor.KeyframeUIController keyframes;
        void getReplayEditor()
        {
            Transform main = PlayerController.Instance.skaterController.transform.parent.transform.parent;
            replay = main.Find("ReplayEditor");
            keyframes = replay.GetComponent<ReplayEditor.ReplayEditorController>().cameraController.keyframeUI;
        }

        void UpdateSliderHandles()
        {
            foreach (Slider s in keyframes.keyframeSliders)
            {
                s.handleRect.SetWidth(Main.settings.handle_size);
                s.handleRect.SetHeight(Main.settings.handle_size);
                Image img = s.handleRect.GetComponent<Image>();
                img.color = Main.settings.handle_color;
            }
        }

        public void FixedUpdate()
        {
            string actual_scene = SceneManager.GetActiveScene().name;

            if (actual_scene != scene)
            {
                if (count > 10)
                {
                    if (SceneManager.GetActiveScene().isLoaded)
                    {
                        scene = actual_scene;
                        AddObjectTrackers();
                        count = 0;
                    }
                }
                else
                {
                    count++;
                }
            }

            if(GameStateMachine.Instance.CurrentState.GetType() == typeof(ReplayState))
            {
                UpdateSliderHandles();
            }
        }

        public void LateUpdate()
        {
            /*Type last_state = GameStateMachine.Instance.CurrentState.GetType();
            if (last_state != state)
            {
                if (last_state == typeof(ReplayState))
                {
                    if (ReplayEditorController.Instance.cameraController.mode == ReplayEditor.CameraMode.Free && !GameStateMachine.Instance.loadingScreenController.IsLoading)
                    {
                        state = last_state;
                    }
                    SetFreeCameraMode();
                }
                else
                {
                    SetOrbitCameraMode();
                    state = last_state;
                }
            }*/
        }

        public void AddObjectTrackers()
        {
            UnityModManager.Logger.Log("Checking Hinges and RigidBodies for replay tracking...");
            string[] internals = { "Gameplay Camera", "NewIKAnim", "NewSteezeIK", "NewSkater", "Pin", "Camera Rig", "CenterOfMassPlayer", "Lean Proxy", "Coping Detection", "Skater Target", "Front Truck", "Back Truck", "Skateboard", "Skater_foot_r", "Skater_Leg_r", "Skater_UpLeg_r", "Skater_foot_l", "Skater_Leg_l", "Skater_UpLeg_l", "Skater_hand_r", "Skater_ForeArm_r", "Skater_Arm_r", "Skater_hand_l", "Skater_ForeArm_l", "Skater_Arm_l", "Skater_Head", "Skater_Spine2", "Skater_Spine", "Skater_pelvis", "Skater_foot_r", "Skater_Leg_r", "Skater_UpLeg_r", "Skater_foot_l", "Skater_Leg_l", "Skater_UpLeg_l" };

            var hinges = UnityEngine.Object.FindObjectsOfType<UnityEngine.HingeJoint>();
            for (int i = 0; i < hinges.Length; i++)
            {
                HingeJoint go = hinges[i];
                bool add = true;

                if (!go.gameObject.activeSelf) add = false;
                for (int n = 0; n < internals.Length; n++)
                {
                    if (go.name == internals[n]) add = false;
                }

                ObjectTracker ot = go.GetComponent<ObjectTracker>();
                bool mcbtay_tracker = false;

                Component[] components = gameObject.GetComponents<MonoBehaviour>();
                foreach (Component component in components)
                {
                    if (component.GetType().ToString().Contains("TrackRigidbodyInReplay")) mcbtay_tracker = true;
                }

                if (add && ot == null && !mcbtay_tracker)
                {
                    go.gameObject.AddComponent<ObjectTracker>();
                    UnityModManager.Logger.Log($"Added ObjectTracker - Hinge @ {go.gameObject.name}");
                }
            }

            var rbs = UnityEngine.Object.FindObjectsOfType<UnityEngine.Rigidbody>();
            for (int i = 0; i < rbs.Length; i++)
            {
                Rigidbody go = rbs[i];
                bool add = true;

                if (!go.gameObject.activeSelf) add = false;
                if (go.isKinematic) add = false;
                for (int n = 0; n < internals.Length; n++)
                {
                    if (go.name == internals[n]) add = false;
                }

                ObjectTracker ot = go.GetComponent<ObjectTracker>();
                bool mcbtay_tracker = false;

                Component[] components = gameObject.GetComponents<MonoBehaviour>();
                foreach (Component component in components)
                {
                    if (component.GetType().ToString().Contains("TrackRigidbodyInReplay")) mcbtay_tracker = true;
                }

                if (add && ot == null && !mcbtay_tracker)
                {
                    go.gameObject.AddComponent<ObjectTracker>();
                    UnityModManager.Logger.Log($"Added ObjectTracker - RB @ {go.gameObject.name}");
                }
            }
        }

        void SetFreeCameraMode()
        {
            ReplayEditorController.Instance.cameraController.SetCameraMode(ReplayEditor.CameraMode.Free, true);
            ReplayEditorController.Instance.cameraController.FreeCamera.gameObject.SetActive(true);
        }

        void SetOrbitCameraMode()
        {
            ReplayEditorController.Instance.cameraController.SetCameraMode(ReplayEditor.CameraMode.Orbit, true);
            ReplayEditorController.Instance.cameraController.OrbitCamera.gameObject.SetActive(true);
        }
    }
}
