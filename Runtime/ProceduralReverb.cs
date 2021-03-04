///
/// ProceduralReverb by Nothke
/// 
/// Calculates reverb parameters by raycasting into the world around the playe
/// (naive and not really physically accurate).
/// 
/// To use, drop it on the main camera.
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

namespace Nothke.Audio
{
    public class ProceduralReverb : MonoBehaviour
    {
        public AudioReverbZone reverb;
        public float smoothFactor = 1;

        public float maxRayDistance = 100;
        public LayerMask raycastLayerMask = -1;
        [Range(1, 13)]
        public int raysPerFrame = 2;

        public AnimationCurve decayTimeBySpaceSize = new AnimationCurve(new Keyframe[] {
        new Keyframe(0, 0),
        new Keyframe(100, 10) });

        public bool debugRays = true;

        float enclosure;
        float roomSize;

        Vector3[] rays;
        float[] hits;

        const int NUM_OF_RAYS = 13;

        int atRay;

        void Start()
        {
            hits = new float[NUM_OF_RAYS];
            rays = new Vector3[NUM_OF_RAYS];

            // Straight rays
            rays[0] = Vector3.up;
            rays[1] = Vector3.forward;
            rays[2] = Vector3.right;
            rays[3] = Vector3.left;
            rays[4] = Vector3.back;

            // Diagonal rays
            rays[5] = new Vector3(-1, 0, -1);
            rays[6] = new Vector3(-1, 0, 1);
            rays[7] = new Vector3(1, 0, -1);
            rays[8] = new Vector3(1, 0, 1);

            rays[9] = new Vector3(-1, 1, -1);
            rays[10] = new Vector3(-1, 1, 1);
            rays[11] = new Vector3(1, 1, -1);
            rays[12] = new Vector3(1, 1, 1);

            reverb = GetComponent<AudioReverbZone>();
            if (!reverb)
                reverb = gameObject.AddComponent<AudioReverbZone>();

            reverb.reverbPreset = AudioReverbPreset.User;
        }

        void Update()
        {
            for (int i = 0; i < raysPerFrame; i++)
            {
                atRay++;
                if (atRay > 12) atRay = 0;
                hits[atRay] = Cast(rays[atRay]);
            }

            // enclosure, the percent (0-1) of number of rays that hit solid surface
            float enclosureTarget = 0;
            // roomSize, average distance of rays that hit solid surface
            float roomSizeTarget = 0;

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i] != Mathf.Infinity)
                {
                    enclosureTarget += 1;
                    roomSizeTarget += hits[i];
                }
            }

            if (enclosureTarget != 0)
                roomSizeTarget /= enclosureTarget;
            else roomSizeTarget = 1;
            enclosureTarget /= hits.Length;

            enclosure = Mathf.Lerp(enclosure, enclosureTarget, Time.deltaTime * smoothFactor);
            roomSize = Mathf.Lerp(roomSize, roomSizeTarget, Time.deltaTime * smoothFactor);

            float reverbRoom = -5000 + enclosure * 5000;
            float reverbDecayTime = decayTimeBySpaceSize.Evaluate(roomSize);

            reverb.room = Mathf.RoundToInt(reverbRoom);
            reverb.decayTime = reverbDecayTime;

            /*
            if (MuteWhenEnclosed.all != null)
                for (int i = 0; i < MuteWhenEnclosed.all.Count; i++)
                    MuteWhenEnclosed.all[i].SetVolumeMult(1 - enclosure);*/
        }

        float Cast(in Vector3 dir)
        {
            float d = Mathf.Infinity;

#if UNITY_EDITOR
            Color c = Color.red;
#endif

            RaycastHit hit;
            if (Physics.Raycast(transform.position, dir, out hit, maxRayDistance, raycastLayerMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.name.StartsWith("Terrain")) // Stupid
                    return Mathf.Infinity;

                d = hit.distance;
#if UNITY_EDITOR
                c = Color.green;
#endif
            }

#if UNITY_EDITOR
            if (debugRays)
                Debug.DrawRay(transform.position, dir * (d == Mathf.Infinity ? maxRayDistance : d), c);
#endif

            return d;
        }
    }
}