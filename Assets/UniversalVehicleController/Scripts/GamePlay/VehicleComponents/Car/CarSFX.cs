using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using System.Linq;

namespace PG
{
    /// <summary>
    /// Sound effects, using FMOD.
    /// </summary>
    public class CarSFX :VehicleSFX
    {
        [Header("CarSFX")]

#pragma warning disable 0649

        [SerializeField] EventReference StartEngineReference;
        [SerializeField] EventReference StopEngineReference;
        [SerializeField] StudioEventEmitter EngineEmitter;
        [SerializeField] StudioEventEmitter SpeedWindEmitter;
        [SerializeField] float MinTimeBetweenBlowOffSounds = 1;

#pragma warning restore 0649

        //PARAMETER_ID to not use a strings when calling "SetParameter" methods.
        FMOD.Studio.PARAMETER_ID RPMID;
        FMOD.Studio.PARAMETER_ID LoadID;
        FMOD.Studio.PARAMETER_ID TurboID;
        FMOD.Studio.PARAMETER_ID TurboBlowOffID;
        FMOD.Studio.PARAMETER_ID AdditionalSoundsID;
        FMOD.Studio.PARAMETER_ID Boost;
        FMOD.Studio.PARAMETER_ID SpeedID;

        CarController Car;
        float LastBlowOffTime;

        protected override void Start ()
        {
            base.Start ();

            Car = Vehicle as CarController;

            if (Car == null)
            {
                Debug.LogErrorFormat ("[{0}] CarSFX without CarController in parent", name);
                enabled = false;
                return;
            }

            //Get PARAMETER_ID for all the necessary events.
            FMOD.Studio.PARAMETER_DESCRIPTION paramDescription;

            if (EngineEmitter != null && EngineEmitter.gameObject.activeInHierarchy)
            {
                EngineEmitter.EventDescription.getParameterDescriptionByName ("RPM", out paramDescription);
                RPMID = paramDescription.id;

                EngineEmitter.EventDescription.getParameterDescriptionByName ("Load", out paramDescription);
                LoadID = paramDescription.id;

                EngineEmitter.EventDescription.getParameterDescriptionByName ("Turbo", out paramDescription);
                TurboID = paramDescription.id;

                EngineEmitter.EventDescription.getParameterDescriptionByName ("TurboBlowOff", out paramDescription);
                TurboBlowOffID = paramDescription.id;

                EngineEmitter.EventDescription.getParameterDescriptionByName ("AdditionalSounds", out paramDescription);
                AdditionalSoundsID = paramDescription.id;

                EngineEmitter.EventDescription.getParameterDescriptionByName ("Boost", out paramDescription);
                Boost = paramDescription.id;

                EngineEmitter.SetParameterNonAlloc (RPMID, Car.MinRPM);
                EngineEmitter.SetParameterNonAlloc (LoadID, 1);

                if (AdditionalSoundsID.data1 != 0 || AdditionalSoundsID.data2 != 0)
                {
                    Car.BackFireAction += OnBackFire;
                    Car.OnStartEngineAction += StartEngine;
                    Car.OnStopEngineAction += StopEngine;
                }

                UpdateAction += UpdateEngine;

                if (Car.Engine.EnableTurbo)
                {
                    if (TurboID.data1 != 0 || TurboID.data2 != 0)
                    {
                        UpdateAction += UpdateTurbo;
                    }
                }

                if (Car.Engine.EnableBoost)
                {
                    if (Boost.data1 != 0 || Boost.data2 != 0)
                    {
                        UpdateAction += UpdateBoost;
                    }
                }
            }

            if (SpeedWindEmitter && SpeedWindEmitter.gameObject.activeInHierarchy)
            {
                if (!SpeedWindEmitter.IsPlaying ())
                {
                    SpeedWindEmitter.Play ();
                }
                SpeedWindEmitter.EventDescription.getParameterDescriptionByName ("Speed", out paramDescription);
                SpeedID = paramDescription.id;

                UpdateAction += UpdateWindEffect;
            }
        }

        void StartEngine (float startDellay)
        {
            EngineEmitter.SetParameterNonAlloc (AdditionalSoundsID, 1);
        }

        void StopEngine ()
        {
            EngineEmitter.SetParameterNonAlloc (AdditionalSoundsID, 2);
        }

        //Base engine sounds
        void UpdateEngine ()
        {
            if (EngineEmitter.IsPlaying ())
            {
                EngineEmitter.SetParameterNonAlloc (RPMID, Car.EngineRPM);
                EngineEmitter.SetParameterNonAlloc (LoadID, Car.EngineLoad.Clamp (-1, 1));
            }
        }

        //Additional turbo sound
        void UpdateTurbo ()
        {
            EngineEmitter.SetParameterNonAlloc (TurboID, Car.CurrentTurbo);
            if (Car.CurrentTurbo > 0.2f && (Car.CurrentAcceleration < 0.2f || Car.InChangeGear) && ((Time.realtimeSinceStartup - LastBlowOffTime) > MinTimeBetweenBlowOffSounds))
            {
                EngineEmitter.SetParameterNonAlloc (TurboBlowOffID, 0);
                EngineEmitter.SetParameterNonAlloc (TurboBlowOffID, Car.CurrentTurbo);
                LastBlowOffTime = Time.realtimeSinceStartup;
            }
        }

        //Additional boost sound
        void UpdateBoost ()
        {
            EngineEmitter.SetParameterNonAlloc (Boost, Car.InBoost ? 1 : 0);
        }

        void UpdateWindEffect ()
        {
            if (Car.IsPlayerVehicle && (SpeedID.data1 != 0 || SpeedID.data2 != 0))
            {
                if (!SpeedWindEmitter.IsPlaying ())
                {
                    SpeedWindEmitter.Play ();
                }
                SpeedWindEmitter.SetParameterNonAlloc (SpeedID, Car.CurrentSpeed);
            }
            else if (SpeedWindEmitter.IsPlaying ())
            {
                SpeedWindEmitter.Stop ();
            }
        }

        void OnBackFire ()
        {
            EngineEmitter.SetParameterNonAlloc (AdditionalSoundsID, Random.Range (3, 6));
            EngineEmitter.SetParameterNonAlloc (AdditionalSoundsID, 0);
        }
    }
}
