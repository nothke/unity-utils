///
/// Deployer by Nothke
/// 
/// A utility struct for when a thing needs to smoothly transition between 2 states.
/// 
/// Call Stow() to make the state progress towards 0, 
/// or Deploy() to make it progress towards 1
/// Alternatively use Toggle()
/// 
/// Call Update() every frame or fixed frame for the transition to progress.
/// 
/// A Deployer is a version that transits at fixed speed.
/// 
/// An InertialDeployer is a version that uses inertia, it will accelerate from one state,
/// and slow down when approaching the end state.
/// 
/// Make sure to have a maxSpeed parameter higher than 0, otherwise they won't move.
/// 
/// To initialize the struct correctly in inspector (with maxSpeed > 0) use:
/// public Deployer deployer = Deployer.Default();
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
    public enum DeploymentState { Stowed, Deploying, Deployed, Stowing };

    public interface IDeployer
    {
        void Update(float dt);
        DeploymentState State { get; }
        bool DeployingOrDeployed { get; }

        void Deploy();
        void Stow();

        void Toggle();
    }

    [System.Serializable]
    public struct Deployer : IDeployer
    {
        public float maxSpeed;

        [HideInInspector]
        public float progress, velocity;

        DeploymentState state;

        public DeploymentState State => state;
        public bool DeployingOrDeployed => state == DeploymentState.Deploying || state == DeploymentState.Deployed;

        public static Deployer Default()
        {
            return new Deployer()
            {
                maxSpeed = 1
            };
        }

        public void Deploy()
        {
            velocity = maxSpeed;

            state = DeploymentState.Deploying;
        }

        public void Stow()
        {
            velocity = -maxSpeed;

            state = DeploymentState.Stowing;
        }

        public void Toggle()
        {
            if (state == DeploymentState.Deployed || state == DeploymentState.Deploying)
                Stow();
            else
                Deploy();
        }

        public void Update(float dt)
        {
            if (maxSpeed == 0)
                return;

            progress += velocity * dt;

            if (velocity < 0 && progress < 0)
            {
                progress = 0;

                if (state == DeploymentState.Stowing)
                {
                    velocity = 0;
                    state = DeploymentState.Stowed;
                }
            }
            else if (velocity > 0 && progress > 1)
            {
                progress = 1;

                if (state == DeploymentState.Deploying)
                {
                    velocity = 0;
                    state = DeploymentState.Deployed;
                }
            }
        }
    }

    [System.Serializable]
    public struct InertialDeployer : IDeployer
    {
        public float maxSpeed;
        public float acceleration;
        public float brakingAcceleration;

        [HideInInspector] public float progress;
        [HideInInspector] public float velocity;

        [HideInInspector] public float accel;
        [HideInInspector] public DeploymentState state;
        [HideInInspector] public bool braking;

        public DeploymentState State => state;
        public bool DeployingOrDeployed => state == DeploymentState.Deploying || state == DeploymentState.Deployed;

        public static InertialDeployer Default()
        {
            return new InertialDeployer()
            {
                maxSpeed = 1,
                acceleration = 1,
            };
        }

        public void Toggle()
        {
            if (state == DeploymentState.Deployed || state == DeploymentState.Deploying)
                Stow();
            else
                Deploy();
        }

        public void Deploy()
        {
            accel = acceleration;

            state = DeploymentState.Deploying;

            braking = velocity < 0;
            if (braking)
            {
                if (brakingAcceleration > 0)
                    accel = brakingAcceleration;
                else accel = acceleration;
            }
        }

        public void Stow()
        {
            accel = -acceleration;

            state = DeploymentState.Stowing;

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

            if (velocity < -maxSpeed || velocity > maxSpeed)
            {
                accel = 0;
            }

            if (brakingAcceleration > 0 && !braking)
            {
                if (velocity > 0 && state == DeploymentState.Deploying)
                {
                    float stopDist = StoppingDistance(velocity, -brakingAcceleration);

                    if (progress > 1 - stopDist)
                    {
                        accel = -brakingAcceleration;
                        braking = true;
                    }
                }
                else if (velocity < 0 && state == DeploymentState.Stowing)
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
                    if (state == DeploymentState.Deploying)
                        SetDeployed();
                    else
                    {
                        braking = false;
                        accel = -acceleration;
                    }
                }
                else if (accel > 0 && velocity > 0)
                {
                    if (state == DeploymentState.Stowing)
                        SetStowed();
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

                if (state == DeploymentState.Stowing)
                    SetStowed();
            }
            else if (velocity > 0 && progress > 1)
            {
                progress = 1;
                velocity = 0;

                if (state == DeploymentState.Deploying)
                    SetDeployed();
            }
        }

        public void SetStowed()
        {
            progress = 0;
            velocity = 0;
            accel = 0;

            state = DeploymentState.Stowed;
            braking = false;
        }

        public void SetDeployed()
        {
            progress = 1;
            velocity = 0;
            accel = 0;

            state = DeploymentState.Deployed;
            braking = false;
        }
    }
}