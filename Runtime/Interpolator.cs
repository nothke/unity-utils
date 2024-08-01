///
/// Interpolator and InertialInterpolator by Nothke
/// 
/// A utility struct for when a value needs to smoothly transition between 2 states.
/// 
/// Call Regress() to make the state advance towards 0, 
/// or Progress() to make it advance towards 1.
/// Alternatively use Toggle().
/// SetTo(value) will set it to a state immediately.
/// 
/// Call Update() every frame or fixed frame for the transition to progress.
/// 
/// An Interpolator is a version that transits at fixed speed.
/// 
/// An InertialInterpolator is a version that uses inertia, it will accelerate from one state,
/// and slow down when approaching the end state.
/// InertialInterpolator can also advance to arbitrary point with AccelerateTo(value).
/// It can also "emergency-stop" by calling StartBraking().
///
/// It doesn't matter if you call the functions repeatedly or once.
/// 
/// Make sure to have a maxSpeed parameter higher than 0, otherwise they won't move.
/// 
/// To initialize the struct correctly in inspector (with maxSpeed > 0) use:
/// public Interpolator interpolator = Interpolator.Default();
/// 
/// ============================================================================
///
/// MIT License
///
/// Copyright(c) 2021 Ivan Notaroš
/// 
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all
/// copies or substantial portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
/// SOFTWARE.
/// 
/// ============================================================================
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nothke.Utils
{
    public enum ProgressionState
    {
        AtStart,
        Progressing,
        AtEnd,
        Regressing
    };

    public interface IInterpolator
    {
        void Update(float dt);
        ProgressionState State { get; }

        /// <summary>
        /// Advance value towards 1.
        /// </summary>
        void Progress();
        /// <summary>
        /// Advance value towards 0.
        /// </summary>
        void Regress();

        void SetTo(float value);

        /// <summary>
        /// Toggle progression between progressing or regressing.
        /// </summary>
        void Toggle();
    }

    [System.Serializable]
    public struct Interpolator : IInterpolator
    {
        public float maxSpeed;

        [HideInInspector]
        public float progress, velocity;

        ProgressionState state;

        public ProgressionState State => state;
        //public bool ProgressingOrAtEnd => state == DeploymentState.Progressing || state == DeploymentState.AtEnd;

        public static Interpolator Default()
        {
            return new Interpolator()
            {
                maxSpeed = 1
            };
        }

        public void Progress()
        {
            velocity = maxSpeed;

            state = ProgressionState.Progressing;
        }

        public void Regress()
        {
            velocity = -maxSpeed;

            state = ProgressionState.Regressing;
        }

        public void Toggle()
        {
            if (state == ProgressionState.AtEnd || state == ProgressionState.Progressing)
                Regress();
            else
                Progress();
        }

        public void SetTo(float value)
        {
            value = Mathf.Clamp01(value);

            progress = value;
            velocity = 0;

            if (value == 1)
                state = ProgressionState.AtEnd;
            else if (value == 0)
                state = ProgressionState.AtStart;
            else
                state = ProgressionState.Progressing;
        }

        public void Update(float dt)
        {
            if (maxSpeed == 0)
                return;

            progress += velocity * dt;

            if (velocity < 0 && progress < 0)
            {
                progress = 0;

                if (state == ProgressionState.Regressing)
                {
                    velocity = 0;
                    state = ProgressionState.AtStart;
                }
            }
            else if (velocity > 0 && progress > 1)
            {
                progress = 1;

                if (state == ProgressionState.Progressing)
                {
                    velocity = 0;
                    state = ProgressionState.AtEnd;
                }
            }
        }
    }

    [System.Serializable]
    public struct InertialInterpolator : IInterpolator
    {
        public float maxSpeed;
        public float acceleration;
        public float brakingAcceleration;

        [HideInInspector] public float progress;
        [HideInInspector] public float velocity;

        [HideInInspector] public float accel;
        private ProgressionState state;
        [HideInInspector] public bool braking;

        private float endTarget;
        private float beginTarget;

        public ProgressionState State => state;
        public bool Stopped => velocity == 0;

        public static InertialInterpolator Default()
        {
            return new InertialInterpolator()
            {
                maxSpeed = 1f,
                acceleration = 1f,

                endTarget = 1f
            };
        }

        public void Toggle()
        {
            if (state == ProgressionState.AtEnd || state == ProgressionState.Progressing)
                Regress();
            else
                Progress();
        }

        private void ProgressTo(float to)
        {
            endTarget = to;

            accel = acceleration;

            state = ProgressionState.Progressing;

            braking = velocity < 0;

            if (braking)
                accel = brakingAcceleration > 0 ? brakingAcceleration : acceleration;
        }

        public void Progress()
        {
            ProgressTo(1);
        }

        private void RegressTo(float to)
        {
            beginTarget = to;

            accel = -acceleration;

            state = ProgressionState.Regressing;

            braking = velocity > 0;

            if (braking)
                accel = brakingAcceleration > 0 ? -brakingAcceleration : -acceleration;
        }

        public void Regress()
        {
            RegressTo(0);
        }

        /// <summary>
        /// Sets the target to start accelerating to. Note, the input should always be 0-1
        /// </summary>
        /// <param name="to"></param>
        public void AccelerateTo(float to)
        {
            if (progress < to)
                ProgressTo(to);
            else
                RegressTo(to);
        }

        /// <summary>
        /// The interpolator will start decelerating with brakingAcceleration eventually slowly coming to a stop,
        /// overriding any progression status.
        /// </summary>
        public void StartBraking()
        {
            if (braking)
                return;
            
            braking = true;

            switch (velocity)
            {
                case > 0:
                    ProgressTo(Mathf.Min(progress + StoppingDistance(velocity, -brakingAcceleration), 1));
                    return;
                case < 0:
                    RegressTo(Mathf.Max(progress - -StoppingDistance(velocity, brakingAcceleration), 0));
                    return;
            }
        }

        static float StoppingDistance(float curVelo, float accel)
        {
            return curVelo * curVelo / (2 * -accel);
        }

        public void Update(float dt)
        {
            if (maxSpeed == 0)
                Debug.Log("MaxSpeed of a Deployer is 0, there will be no movement. Please set it before using");

            // Limit velocity
            if (velocity < -maxSpeed)
            {
                accel = 0;
                velocity = -maxSpeed + Mathf.Epsilon;
            }
            else if (velocity > maxSpeed)
            {
                accel = 0;
                velocity = maxSpeed - Mathf.Epsilon;
            }

            if (brakingAcceleration > 0 && !braking)
            {
                // TODO: Handle the case where braking force is so big that it immediately reverses the direction 
                // In that case we should skip braking state entirely
                
                if (velocity > 0 && state == ProgressionState.Progressing)
                {
                    float stopDist = StoppingDistance(velocity, -brakingAcceleration);
                    
                    if (progress > endTarget - stopDist)
                    {
                        accel = -brakingAcceleration;
                        braking = true;
                    }
                }
                else if (velocity < 0 && state == ProgressionState.Regressing)
                {
                    float stopDist = -StoppingDistance(velocity, brakingAcceleration);

                    if (progress < beginTarget + stopDist)
                    {
                        accel = brakingAcceleration;
                        braking = true;
                    }
                }
            }


            if (braking)
            {
                if (accel < 0 && velocity < 0)
                {
                    if (state == ProgressionState.Progressing)
                        SetTo(endTarget);
                    else
                    {
                        braking = false;
                        accel = -acceleration;
                    }
                }
                else if (accel > 0 && velocity > 0)
                {
                    if (state == ProgressionState.Regressing)
                        SetTo(beginTarget);
                    else
                    {
                        braking = false;
                        accel = acceleration;
                    }
                }
            }

            velocity += accel * dt;
            progress += velocity * dt;

            // Clamp between 0-1
            if (velocity < 0 && progress < 0)
            {
                progress = 0;
                velocity = 0;

                if (state == ProgressionState.Regressing)
                    SetTo(0);
            }
            else if (velocity > 0 && progress > 1)
            {
                progress = 1;
                velocity = 0;

                if (state == ProgressionState.Progressing)
                    SetTo(1);
            }
        }

        public void SetTo(float value)
        {
            value = Mathf.Clamp01(value);
            progress = value;
            velocity = 0;
            accel = 0;
            braking = false;

            if (value == 1)
                state = ProgressionState.AtEnd;
            else if (value == 0)
                state = ProgressionState.AtStart;
            else
                state = ProgressionState.Progressing;
        }
    }
}
