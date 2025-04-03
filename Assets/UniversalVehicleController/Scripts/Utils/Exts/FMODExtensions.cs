using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FMODUnity
{
	public static class FMODExtentions
	{
        static FMOD.ATTRIBUTES_3D Attributes;

        public static void PlayOneShot (EventReference eventRef, float volume, Vector3 position = default, Vector3 velocity = default)
		{
            if (eventRef.IsNull)
            {
#if UNITY_EDITOR
                Debug.LogWarning ("[FMOD] Event not found: " + eventRef.Path);
#endif
            }
            else
            {
                var instance = RuntimeManager.CreateInstance(eventRef);
                Attributes = RuntimeUtils.To3DAttributes (position);
                Attributes.velocity = velocity.ToFMODVector();
                instance.set3DAttributes (Attributes);
                instance.setVolume (volume);
                instance.start ();
                instance.release ();
            }
        }

        public static void SetParameterNonAlloc (this StudioEventEmitter emitter, FMOD.Studio.PARAMETER_ID id, float value, bool ignoreseekspeed = false)
        {
            if (emitter.EventInstance.isValid ())
            {
                emitter.EventInstance.setParameterByID (id, value, ignoreseekspeed);
            }
        }
    }
}
