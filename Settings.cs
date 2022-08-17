using System;
using System.Collections.Generic;
using UnityEngine;
using UnityModManagerNet;

namespace BetterReplay
{
    [Serializable]

    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        public Color32 handle_color = new Color32(255, 0, 0, 255);
        public float handle_size = 25;
        public bool disable_messages = false;
        public bool disable_rb_tracker = true;
        public bool disable_hinge_tracker = true;
        public bool disable_animator_tracker = true;

        public float light_intensity = 6.00f;
        public float light_spotangle = 120f;
        public float light_range = 5f;
        public float light_temperature = 6500f;
        public float light_dimmer = 0f;
        public string cookie_texture = "None";

        public float pin_movespeed = 10f;
        public float pin_rotationspeed = 180f;

        public void OnChange()
        {
            throw new NotImplementedException();
        }

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save<Settings>(this, modEntry);
        }
    }
}
