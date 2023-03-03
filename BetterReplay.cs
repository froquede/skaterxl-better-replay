using System;
using UnityEngine;
using UnityModManagerNet;
using SkaterXL.Core;
using SkaterXL.Gameplay;
using SkaterXL.Sound;
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
using HarmonyLib;

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
        Transform mainCamera, replayCamera, pinCamera, pinMover;
        float distance_multiplier = 1;
        public bool light_enabled = false;
        public string EmptyCookieName = "None";
        Dictionary<string, Texture2D> Cookies = new Dictionary<string, Texture2D>();

        public void Start()
        {
            PlayerController.Instances[PlayerController.Instances.Count - 1].pinController.maxHeight = float.PositiveInfinity;
            // GetCookies();
            getReplayEditor();
            getPinCamera();
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

            mainCamera = PlayerController.Instances[PlayerController.Instances.Count - 1].gameplay.cameraController.gameObject.transform.FindChildRecursively("Gameplay Camera");
            XLGLight.transform.rotation = mainCamera.rotation;
        }

        Transform replay;
        ReplayEditor.KeyframeUIController keyframes;
        void getReplayEditor()
        {
            Transform main = PlayerController.Instances[PlayerController.Instances.Count - 1].gameplay.skaterController.transform.parent.transform.parent;
            replay = main.Find("ReplayEditor");
            keyframes = replay.GetComponent<ReplayEditor.ReplayEditorController>().cameraController.keyframeUI;
            replayCamera = replay.FindChildRecursively("VirtualCamera1");
        }

        void getPinCamera()
        {
            pinMover = PlayerController.Instances[PlayerController.Instances.Count - 1].transform.parent.FindChildRecursively("Pin Mover");
            pinCamera = PlayerController.Instances[PlayerController.Instances.Count - 1].transform.parent.FindChildRecursively("GroundLocationIndicator");
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
            /*foreach (ReplayAudioSourceController replayAudioSourceController in ReplayEditorController.Instance.playbackController.audioSourceControllers)
            {
                try
                {
                    AudioClip clip = SoundManager.Instance.GetClipForID(replayAudioSourceController.clipID.typeID, replayAudioSourceController.clipID.index);
                    UnityModManager.Logger.Log(clip.name.ToLower());
                    replayAudioSourceController.UpdateProperties(ReplayEditorController.Instance.playbackTimeScale);
                }
                catch { }
            }*/

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

                if (GameStateMachine.Instance.CurrentState.GetType() == typeof(PinMovementState))
                {
                    XLGLight.transform.position = pinMover.transform.position + new Vector3(0, 2, 0);
                    XLGLight.transform.LookAt(pinCamera);
                    distance_multiplier = Mathf.Lerp(distance_multiplier, pinMover.transform.position.y - pinCamera.transform.position.y, Time.deltaTime * 4);
                }

                XLGLight.transform.Translate(Main.settings.light_offset, Space.Self);
            }

            if (GameStateMachine.Instance.CurrentState.GetType() == typeof(ReplayState))
            {
                if (Input.GetKey("left")) ReplayEditorController.Instance.playbackController.SetPlaybackTime(ReplayEditorController.Instance.playbackController.CurrentTime - .001f);
                if (Input.GetKey("right")) ReplayEditorController.Instance.playbackController.SetPlaybackTime(ReplayEditorController.Instance.playbackController.CurrentTime + .001f);
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
                XLGLightAdditionalData.intensity = (Main.settings.light_intensity * 1000) * (GameStateMachine.Instance.CurrentState.GetType() == typeof(PinMovementState) ? distance_multiplier : 1);
                XLGLightAdditionalData.volumetricDimmer = Main.settings.light_dimmer;
                XLGLightComp.intensity = Mathf.Lerp(0, Main.settings.light_intensity * 1000, map01(frame, 0, 25));
                if (pattern.Length > frame)
                {
                    XLGLightComp.intensity = pattern[frame] == 'm' ? XLGLightComp.intensity : 100 * (float)rand.NextDouble();
                }
                else
                {
                    XLGLightComp.intensity = (Main.settings.light_intensity * 1000) * (GameStateMachine.Instance.CurrentState.GetType() == typeof(PinMovementState) ? distance_multiplier : 1); ;
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
                if (frame > 0) frame--;
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
                    state = last_state;
                }
            }
            if (last_state == typeof(PlayState) && state == typeof(ReplayState))
            {
                state = last_state;
            }

            if (Main.settings.double_tap)
            {
                if (PlayerController.Instances[PlayerController.Instances.Count - 1].gameplay.playerData.currentState == PlayerStateEnum.Pushing || PlayerController.Instances[PlayerController.Instances.Count - 1].gameplay.playerData.currentState == PlayerStateEnum.Riding || PlayerController.Instances[PlayerController.Instances.Count - 1].gameplay.playerData.currentState == PlayerStateEnum.Impact)
                {
                    if (PlayerController.Instances[PlayerController.Instances.Count - 1].gameplay.inputController.rewiredPlayer.GetButtonDoublePressDown("Left Stick Button")) ToggleLight();
                    if (PlayerController.Instances[PlayerController.Instances.Count - 1].gameplay.inputController.rewiredPlayer.GetButtonDoublePressHold("Left Stick Button"))
                    {
                        if (PlayerController.Instances[PlayerController.Instances.Count - 1].gameplay.inputController.rewiredPlayer.GetButton("RB")) Main.settings.light_intensity += .05f;
                        if (PlayerController.Instances[PlayerController.Instances.Count - 1].gameplay.inputController.rewiredPlayer.GetButton("LB")) Main.settings.light_intensity -= .05f;
                    }
                }
            }

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.L))
            {
                ToggleLight();
            }

            PlayerController.Instances[PlayerController.Instances.Count - 1].pinController.MoveSpeed = Main.settings.pin_movespeed;
            PlayerController.Instances[PlayerController.Instances.Count - 1].pinController.RotateSpeed = Main.settings.pin_rotationspeed;
        }

        public void ToggleLight()
        {
            light_enabled = !light_enabled;
            NotificationManager.Instance.ShowNotification($"Light { (light_enabled ? "enabled" : "disabled") }", 1f, false, NotificationManager.NotificationType.Normal, TextAlignmentOptions.TopRight, 0f);
        }

        string[] internals = { "Gameplay Camera", "NewIKAnim", "NewSteezeIK", "NewSkater", "Pin", "Camera Rig", "CenterOfMassPlayer", "Lean Proxy", "Coping Detection", "Skater Target", "Front Truck", "Back Truck", "Skateboard", "Skater_foot_r", "Skater_Leg_r", "Skater_UpLeg_r", "Skater_foot_l", "Skater_Leg_l", "Skater_UpLeg_l", "Skater_hand_r", "Skater_ForeArm_r", "Skater_Arm_r", "Skater_hand_l", "Skater_ForeArm_l", "Skater_Arm_l", "Skater_Head", "Skater_Spine2", "Skater_Spine", "Skater_pelvis", "Skater_foot_r", "Skater_Leg_r", "Skater_UpLeg_r", "Skater_foot_l", "Skater_Leg_l", "Skater_UpLeg_l", "WithProgressVariant", "Text (TMP)", "UI_Source", "Movement_Foley_Source", "Powerslide_Hits_Source 2", "Powerslide_Loop_Source 2", "Powerslide_Hits_Source", "Powerslide_Loop_Source", "Wheel_Rolling_Loops_High_Source", "Wheel_Rolling_Loop_Low_Source", "Music_Source", "Wheel_Hits_Source", "Grind_Loop_Source", "Deck_Source", "Grind_Hits_Source", "Shoes_Hit_Source", "Shoes_Scrape_Source", "Bearing_Source", "GamePlay", "UI_Audio_Source", "Music(Clone)", "AmbientSounds", "dots", "WithoutProgressVariant" };
        public void AddObjectTrackers()
        {
            UnityModManager.Logger.Log("Checking Hinges, RigidBodies, Animators, and AudioSources for replay tracking...");

            HingeTracker();
            RBTracker();
            AnimatorTracker();
            AudioSourceTracker();
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

            //var anims = Resources.FindObjectsOfTypeAll(typeof(Animator)) as Animator[];
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

                if (add) UnityModManager.Logger.Log(anims[i].transform.name);

                AnimationTracker ot = go.gameObject.GetComponent<AnimationTracker>();
                if (add && ot == null)
                {
                    go.gameObject.AddComponent<AnimationTracker>();
                    UnityModManager.Logger.Log("Animator tracker - " + go.gameObject.name);
                    anim_count++;
                }
            }

            if (anim_count > 0 && !Main.settings.disable_messages) MessageSystem.QueueMessage(MessageDisplayData.Type.Success, $"Tracker added - {anim_count} animators", 1.5f);
        }

        public void AudioSourceTracker()
        {
            if (Main.settings.disable_audiosource_tracker) return;

            var audiosources = FindObjectsOfType<AudioSource>();
            int anim_count = 0;

            for (int i = 0; i < audiosources.Length; i++)
            {
                AudioSource go = audiosources[i];
                bool add = true;

                if (!go.gameObject.activeSelf) add = false;
                for (int n = 0; n < internals.Length; n++)
                {
                    if (go.name == internals[n]) add = false;
                }

                AudioSourceTracker ot = go.gameObject.GetComponent<AudioSourceTracker>();
                if (add && ot == null)
                {
                    go.gameObject.AddComponent<AudioSourceTracker>();
                    UnityModManager.Logger.Log("AudioSource tracker - " + go.gameObject.name + " " + go.transform.parent.gameObject.name);
                    anim_count++;
                }
            }

            if (anim_count > 0 && !Main.settings.disable_messages) MessageSystem.QueueMessage(MessageDisplayData.Type.Success, $"Tracker added - {anim_count} audio sources", 1.5f);
        }

        public void DestroyObjectTracker()
        {
            var objects = FindObjectsOfType<ObjectTracker>();
            for (int i = 0; i < objects.Length; i++)
            {
                Destroy(objects[i].gameObject.GetComponent<ObjectTracker>());
            }
        }

        public void DestroyAudioSourceTracker()
        {
            var objects = FindObjectsOfType<AudioSourceTracker>();
            for (int i = 0; i < objects.Length; i++)
            {
                Destroy(objects[i].gameObject.GetComponent<AudioSourceTracker>());
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
