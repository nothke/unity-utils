///
/// Interpolator and InertialInterpolator by Nothke
/// 
/// A utility struct for when a value needs to smoothly transition between 2 states.
/// 
/// Call Degress() to make the state progress towards 0, 
/// or Progress() to make it progress towards 1
/// Alternatively use Toggle()
/// SetTo(value) will set it to a state immediately
/// 
/// Call Update() every frame or fixed frame for the transition to progress.
/// 
/// An Interpolator is a version that transits at fixed speed.
/// 
/// An InertialInterpolator is a version that uses inertia, it will accelerate from one state,
/// and slow down when approaching the end state.
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
    public enum ProgressionState { AtStart, Progressing, AtEnd, Regressing };

    public interface IInterpolator
    {
        void Update(float dt);
        ProgressionState State { get; }

        void Progress();
        void Regress();

        void SetTo(float value);

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
        [HideInInspector] public ProgressionState state;
        [HideInInspector] public bool braking;

        public ProgressionState State => state;
        //public bool ProgressingOrAtEnd => state == DeploymentState.Progressing || state == DeploymentState.AtEnd;

        public static InertialInterpolator Default()
        {
            return new InertialInterpolator()
            {
                maxSpeed = 1,
                acceleration = 1,
            };
        }

        public void Toggle()
        {
            if (state == ProgressionState.AtEnd || state == ProgressionState.Progressing)
                Regress();
            else
                Progress();
        }

        public void Progress()
        {
            accel = acceleration;

            state = ProgressionState.Progressing;

            braking = velocity < 0;
            if (braking)
            {
                if (brakingAcceleration > 0)
                    accel = brakingAcceleration;
                else accel = acceleration;
            }
        }

        public void Regress()
        {
            accel = -acceleration;

            state = ProgressionState.Regressing;

            braking = velocity > 0;
            if (braking)
            {
                if (brakingAcceleration > 0)
                    accel = -brakingAcceleration;
                else accel = -acceleration;
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
                if (velocity > 0 && state == ProgressionState.Progressing)
                {
                    float stopDist = StoppingDistance(velocity, -brakingAcceleration);

                    if (progress > 1 - stopDist)
                    {
                        accel = -brakingAcceleration;
                        braking = true;
                    }
                }
                else if (velocity < 0 && state == ProgressionState.Regressing)
                {
                    float stopDist = -StoppingDistance(velocity, brakingAcceleration);

                    if (progress < stopDist)
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
                        SetTo(1);
                    else
                    {
                        braking = false;
                        accel = -acceleration;
                    }
                }
                else if (accel > 0 && velocity > 0)
                {
                    if (state == ProgressionState.Regressing)
                        SetTo(0);
                    else
                    {
                        braking = false;
                        accel = acceleration;
                    }
                }
            }

            velocity += accel * dt;
            progress += velocity * dt;

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