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
