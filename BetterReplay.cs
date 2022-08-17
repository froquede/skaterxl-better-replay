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
using System.IO;
using System.Linq;

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
        public string EmptyCookieName = "None";
        Dictionary<string, Texture2D> Cookies = new Dictionary<string, Texture2D>();

        public void Start()
        {
            PlayerController.Instance.pinMover.maxHeight = float.PositiveInfinity;
            // GetCookies();
            getReplayEditor();
            CreateFixLight();
        }

        void CreateFixLight()
        {
            XLGLight = new GameObject("FixLight");
            XLGLightComp = XLGLight.AddComponent<Light>();
            XLGLightAdditionalData = XLGLight.AddComponent<HDAdditionalLightData>();
            XLGLightAdditionalData.lightUnit = LightUnit.Ev100;
            XLGLightAdditionalData.volumetricDimmer = Main.settings.light_dimmer;
            XLGLightAdditionalData.intensity = Main.settings.light_intensity * 1000;
            XLGLightComp.intensity = Main.settings.light_intensity * 1000;
            XLGLightComp.type = LightType.Spot;
            XLGLightComp.spotAngle = Main.settings.light_spotangle;
            XLGLightComp.range = Main.settings.light_range;
            XLGLightComp.colorTemperature = Main.settings.light_temperature;
            XLGLightComp.useColorTemperature = true;
            /*XLGLightComp.cookie = (Main.settings.cookie_texture == "None") ? null : Cookies[Main.settings.cookie_texture];
            if (Main.settings.cookie_texture != EmptyCookieName) XLGLightAdditionalData.SetCookie(Cookies[Main.settings.cookie_texture], new Vector2(1.0f, 1.0f));*/

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

        int frame = 0;
        public void Update()
        {
            if (light_enabled)
            {
                if (GameStateMachine.Instance.CurrentState.GetType() == typeof(PlayState))
                {
                    XLGLight.transform.position = mainCamera.TransformPoint(Vector3.zero);
                    XLGLight.transform.rotation = mainCamera.rotation;
                }

                if (GameStateMachine.Instance.CurrentState.GetType() == typeof(ReplayState))
                {
                    XLGLight.transform.position = replayCamera.TransformPoint(Vector3.zero);
                    XLGLight.transform.rotation = replayCamera.rotation;
                }
            }
        }

        public static float map01(float value, float min, float max)
        {
            float result = (value - min) * 1f / (max - min);
            if (result > 1) return 1;
            else return result;
        }

        System.Random rand = new System.Random();
        char[] pattern = "mmamammmmammamamaaamammma".ToCharArray();
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
                        CreateFixLight();
                        AddObjectTrackers();
                        // ConsolePlayerPrefs.SetString("LastLevelPath", scene);
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

            if (light_enabled)
            {
                XLGLightAdditionalData.intensity = Main.settings.light_intensity * 1000;
                XLGLightAdditionalData.volumetricDimmer = Main.settings.light_dimmer;
                XLGLightComp.intensity = Mathf.Lerp(0, Main.settings.light_intensity * 1000, map01(frame, 0, 25));
                if (pattern.Length > frame)
                {
                    XLGLightComp.intensity = pattern[frame] == 'm' ? XLGLightComp.intensity : 100 * (float)rand.NextDouble();
                }
                else
                {
                    XLGLightComp.intensity = Main.settings.light_intensity * 1000;
                }

                XLGLightComp.spotAngle = Main.settings.light_spotangle;
                XLGLightComp.range = Main.settings.light_range;
                XLGLightComp.colorTemperature = Main.settings.light_temperature;
                /*XLGLightComp.cookie = (Main.settings.cookie_texture == EmptyCookieName) ? null : Cookies[Main.settings.cookie_texture];
                if (Main.settings.cookie_texture != EmptyCookieName) XLGLightAdditionalData.SetCookie(Cookies[Main.settings.cookie_texture], new Vector2(1.0f, 1.0f));*/
                frame++;
            }
            else
            {
                frame = frame > 18 ? 18 : frame;
                if(frame > 0) frame--;
                XLGLightAdditionalData.intensity = 0;
                XLGLightComp.intensity = Mathf.Lerp(0, Main.settings.light_intensity * 1000, map01(frame, 0, 18)); ;
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
                    SoundManager.Instance.StopPowerslideSound(0, 0);
                    state = last_state;
                }
            }
            if (last_state == typeof(PlayState) && state == typeof(ReplayState))
            {
                state = last_state;
            }

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.L))
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

        void GetCookies()
        {
            Cookies[EmptyCookieName] = null;
            var gparent = Directory.GetParent(Directory.GetParent(Main.modEntry.Path).ToString());
            var main_path = gparent.ToString() + "\\XLGraphics\\Cookies";
            if (Directory.Exists(main_path))
            {
                string[] files = Directory.GetFiles(main_path, "*.png");
                foreach (string path in files)
                {
                    Texture2D texture2D = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                    texture2D.LoadImage(File.ReadAllBytes(path));
                    texture2D.filterMode = FilterMode.Point;
                    Cookies[Path.GetFileNameWithoutExtension(path)] = texture2D;
                }
            }
            else
            {
                UnityModManager.Logger.Log("No XLG or Cookies folder");
            }
        }
        public string[] CookieNames
        {
            get
            {
                return Cookies.Keys.ToArray();
            }
        }
    }
}
