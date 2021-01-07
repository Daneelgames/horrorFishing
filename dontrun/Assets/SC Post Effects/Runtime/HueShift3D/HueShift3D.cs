#if URP
using UnityEngine.Rendering.Universal;
#endif

using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace SCPE
{
#if URP
    [Serializable, VolumeComponentMenu("SC Post Effects/Image/3D Hue Shift")]
    public sealed class HueShift3D : VolumeComponent, IPostProcessComponent
    {
        [Range(0f, 1f)]
        public ClampedFloatParameter intensity = new ClampedFloatParameter(0f,0f,1f);

        [Range(0f, 1f), Tooltip("Speed")]
        public ClampedFloatParameter speed = new ClampedFloatParameter(0.3f, 0f,1f);

        [Range(0f, 3f), Tooltip("Size")]
        public ClampedFloatParameter size = new ClampedFloatParameter(1f,0f,3f);

        [Range(0f, 10f), Tooltip("Bends the effect over the scene's geometry normals\n\nHigh values may induce banding artifacts")]
        public ClampedFloatParameter geoInfluence = new ClampedFloatParameter(5f,0f,10f);

        public static bool isOrtho = false;

        public bool RequireDepthNormals() => true;

        public bool IsActive() => intensity.value > 0f && this.active;

        public bool IsTileCompatible() => false;
#endif
    }
}