using System;
using UnityEngine;
using UnityModManagerNet;
using SkaterXL.Core;
using ReplayEditor;
using UnityEngine.SceneManagement;
using GameManagement;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
using ModIO.UI;
using System.Collections.Generic;
using TMPro;

namespace BetterReplay
{
    class BetterReplay : MonoBehaviour
    {
        public string scene = "";
        public Type state;
        int count = 0;
        GameObject XLGLight;
        Light XLGLightComp;
        HDAdditionalLightData XLGLightAdditionalData;
        Transform mainCamera, replayCamera;
        public bool light_enabled = false;

        public void Start()
        {
            PlayerController.Instance.pinMover.maxHeight = float.PositiveInfinity;
            getReplayEditor();
            CreateFixLight();
        }

        void CreateFixLight()
        {
            XLGLight = new GameObject("FixLight");
            XLGLightComp = XLGLight.AddComponent<Light>();
            XLGLightAdditionalData = XLGLight.AddComponent<HDAdditionalLightData>();
            XLGLightAdditionalData.lightUnit = LightUnit.Ev100;
            XLGLightComp.type = LightType.Spot;
            XLGLightAdditionalData.intensity = Main.settings.light_intensity * 1000;
            XLGLightComp.intensity = Main.settings.light_intensity * 1000;
            XLGLightComp.spotAngle = Main.settings.light_spotangle;
            XLGLightComp.range = Main.settings.light_range;

            mainCamera = PlayerController.Instance.cameraController.gameObject.transform.FindChildRecursively("Gameplay Camera");
            XLGLight.transform.rotation = mainCamera.rotation;
        }

        Transform replay;
        ReplayEditor.KeyframeUIController keyframes;
        void getReplayEditor()
        {
            Transform main = PlayerController.Instance.skaterController.transform.parent.transform.parent;
            replay = main.Find("ReplayEditor");
            keyframes = replay.GetComponent<ReplayEditor.ReplayEditorController>().cameraController.keyframeUI;
            replayCamera = replay.FindChildRecursively("VirtualCamera1");
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

        public void Update()
        {
            if (light_enabled)
            {
                XLGLightAdditionalData.intensity = Main.settings.light_intensity * 1000;
                XLGLightComp.intensity = Main.settings.light_intensity * 1000;
                XLGLightComp.spotAngle = Main.settings.light_spotangle;
                XLGLightComp.range = Main.settings.light_range;

                if (GameStateMachine.Instance.CurrentState.GetType() == typeof(PlayState))
                {
                    XLGLight.transform.position = mainCamera.TransformPoint(Vector3.zero);
                    XLGLight.transform.rotation = mainCamera.rotation;
                }

                if (GameStateMachine.Instance.CurrentState.GetType() == typeof(ReplayState))
                {
                    XLGLight.transform.position = replayCamera.TransformPoint(Vector3.zero);
                    XLGLight.transform.rotation = replayCamera.rotation;
                    UnityModManager.Logger.Log(XLGLight.transform.position.ToString());
                }
            }
            else
            {
                XLGLightAdditionalData.intensity = 0;
                XLGLightComp.intensity = 0;
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

            if (GameStateMachine.Instance.CurrentState.GetType() == typeof(ReplayState))
            {
                UpdateSliderHandles();
            }            
        }

        public void LateUpdate()
        {
            Type last_state = GameStateMachine.Instance.CurrentState.GetType();
            if (last_state != state)
            {
                if (last_state == typeof(ReplayState) && !GameStateMachine.Instance.loadingScreenController.IsLoading)
                {
                    ReplayEditorController.Instance.playbackController.CurrentTime = ReplayEditorController.Instance.playbackController.ClipEndTime;
                    ReplayEditorController.Instance.cameraController.OnReplayEditorStart();
                    state = last_state;
                }
            }
            if (last_state == typeof(PlayState) && state == typeof(ReplayState))
            {
                state = last_state;
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                light_enabled = !light_enabled;
                NotificationManager.Instance.ShowNotification($"Light { (light_enabled ? "enabled" : "disabled") }", 1f, false, NotificationManager.NotificationType.Normal, TextAlignmentOptions.TopRight, 0f);
            }

            PlayerController.Instance.pinMover.MoveSpeed = Main.settings.pin_movespeed;
            PlayerController.Instance.pinMover.RotateSpeed = Main.settings.pin_rotationspeed;
        }

        string[] internals = { "Gameplay Camera", "NewIKAnim", "NewSteezeIK", "NewSkater", "Pin", "Camera Rig", "CenterOfMassPlayer", "Lean Proxy", "Coping Detection", "Skater Target", "Front Truck", "Back Truck", "Skateboard", "Skater_foot_r", "Skater_Leg_r", "Skater_UpLeg_r", "Skater_foot_l", "Skater_Leg_l", "Skater_UpLeg_l", "Skater_hand_r", "Skater_ForeArm_r", "Skater_Arm_r", "Skater_hand_l", "Skater_ForeArm_l", "Skater_Arm_l", "Skater_Head", "Skater_Spine2", "Skater_Spine", "Skater_pelvis", "Skater_foot_r", "Skater_Leg_r", "Skater_UpLeg_r", "Skater_foot_l", "Skater_Leg_l", "Skater_UpLeg_l", "WithProgressVariant", "Text (TMP)" };
        public void AddObjectTrackers()
        {
            UnityModManager.Logger.Log("Checking Hinges, RigidBodies and Animators for replay tracking...");

            HingeTracker();
            RBTracker();
            AnimatorTracker();
        }

        public void HingeTracker()
        {
            if (Main.settings.disable_hinge_tracker) return;
            var hinges = FindObjectsOfType<HingeJoint>();
            int hinge_count = 0;
            for (int i = 0; i < hinges.Length; i++)
            {
                HingeJoint go = hinges[i];
                bool add = true;

                if (!go.gameObject.activeSelf) add = false;
                for (int n = 0; n < internals.Length; n++)
                {
                    if (go.name == internals[n]) add = false;
                }

                ObjectTracker ot = go.gameObject.GetComponent<ObjectTracker>();
                bool mcbtay_tracker = false;

                Component[] components = gameObject.GetComponents<MonoBehaviour>();
                foreach (Component component in components)
                {
                    if (component.GetType().ToString().Contains("TrackRigidbodyInReplay")) mcbtay_tracker = true;
                }

                if (add && ot == null && !mcbtay_tracker)
                {
                    go.gameObject.AddComponent<ObjectTracker>();
                    UnityModManager.Logger.Log("Hinge tracker - " + go.gameObject.name);
                    hinge_count++;
                }
            }
            if (hinge_count > 0 && !Main.settings.disable_messages) MessageSystem.QueueMessage(MessageDisplayData.Type.Success, $"Tracker added - {hinge_count} hinges", 1.5f);
        }

        public void RBTracker()
        {
            if (Main.settings.disable_rb_tracker) return;

            var rbs = FindObjectsOfType<Rigidbody>();
            int rb_count = 0;
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

                ObjectTracker ot = go.gameObject.GetComponent<ObjectTracker>();
                bool mcbtay_tracker = false;

                Component[] components = gameObject.GetComponents<MonoBehaviour>();
                foreach (Component component in components)
                {
                    if (component.GetType().ToString().Contains("TrackRigidbodyInReplay")) mcbtay_tracker = true;
                }

                if (add && ot == null && !mcbtay_tracker)
                {
                    go.gameObject.AddComponent<ObjectTracker>();
                    UnityModManager.Logger.Log("RigidBody tracker - " + go.gameObject.name);
                    rb_count++;
                }
            }
            if (rb_count > 0 && !Main.settings.disable_messages) MessageSystem.QueueMessage(MessageDisplayData.Type.Success, $"Tracker added - {rb_count} rigid bodies", 1.5f);
        }

        public void AnimatorTracker()
        {
            if (Main.settings.disable_animator_tracker) return;

            var anims = FindObjectsOfType<Animator>();
            int anim_count = 0;

            for (int i = 0; i < anims.Length; i++)
            {
                Animator go = anims[i];
                bool add = true;

                if (!go.gameObject.activeSelf) add = false;
                for (int n = 0; n < internals.Length; n++)
                {
                    if (go.name == internals[n]) add = false;
                }

                AnimationTracker ot = go.GetComponent<AnimationTracker>();
                if (add && ot == null)
                {
                    go.gameObject.AddComponent<AnimationTracker>();
                    UnityModManager.Logger.Log("Animator tracker - " + go.gameObject.name);
                    anim_count++;
                }
            }

            if (anim_count > 0 && !Main.settings.disable_messages) MessageSystem.QueueMessage(MessageDisplayData.Type.Success, $"Tracker added - {anim_count} animators", 1.5f);
        }

        public void DestroyObjectTracker()
        {
            var objects = FindObjectsOfType<ObjectTracker>();
            for (int i = 0; i < objects.Length; i++)
            {
                Destroy(objects[i].gameObject.GetComponent<ObjectTracker>());
            }
        }

        public void DestroyAnimatorTracker()
        {
            var objects = FindObjectsOfType<AnimationTracker>();
            for (int i = 0; i < objects.Length; i++)
            {
                Destroy(objects[i].gameObject.GetComponent<AnimationTracker>());
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
