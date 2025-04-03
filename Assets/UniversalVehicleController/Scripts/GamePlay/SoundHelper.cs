using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PG
{
    public static class SoundHelper
    {
        public static bool SoundSupportSplitScreen => true;

        public static void ChangeSoundTimeScale (float timeScale)
        {
            var bus = FMODUnity.RuntimeManager.GetBus("bus:/Game");
            FMOD.ChannelGroup fmodTimescaleGroup;
            bus.getChannelGroup (out fmodTimescaleGroup);
            fmodTimescaleGroup.setPitch (timeScale);
        }

        public static void TryAddAudioListiner (GameObject go)
        {
            if (go.GetComponent<FMODUnity.StudioListener> () == null)
            {
                go.AddComponent<FMODUnity.StudioListener> ();
            }
        }
    }
}
