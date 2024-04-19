// 無償版のコード
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace VRCTrailFX
{
    [AddComponentMenu("VRCTrailFX/VRCTrailFX Free")]
    public class VRCTrailFXFree : MonoBehaviour
    {
        public VRCAvatarDescriptor avatarDescriptor;
        public VRCExpressionsMenu expressionsMenu;
        public Material trailMaterial;
        public float trailWidthMultiplier = 1f;
        public float trailLengthMultiplier = 1f;

        private VRCExpressionParameters expressionParameters;
        private VRCAvatarParameter trailWidthParameter;
        private VRCAvatarParameter trailLengthParameter;

        private TrailRenderer[] trailRenderers;

        void Start()
        {
            expressionParameters = avatarDescriptor.expressionParameters;
            trailWidthParameter = expressionParameters.FindParameter("TrailWidth");
            trailLengthParameter = expressionParameters.FindParameter("TrailLength");

            trailRenderers = GetComponentsInChildren<TrailRenderer>();
            foreach (var trail in trailRenderers)
            {
                trail.sharedMaterial = trailMaterial;
                trail.widthMultiplier = trailWidthMultiplier;
                trail.time = trailLengthMultiplier;
            }

            if (expressionsMenu != null)
            {
                expressionsMenu.controls.Add(new VRCExpressionsMenu.Control
                {
                    name = "Trail Width",
                    parameter = trailWidthParameter,
                    type = VRCExpressionsMenu.Control.ControlType.RadialPuppet
                });

                expressionsMenu.controls.Add(new VRCExpressionsMenu.Control
                {
                    name = "Trail Length",
                    parameter = trailLengthParameter,
                    type = VRCExpressionsMenu.Control.ControlType.RadialPuppet
                });
            }
        }

        void Update()
        {
            float trailWidth = trailWidthParameter.value;
            float trailLength = trailLengthParameter.value;

            foreach (var trail in trailRenderers)
            {
                trail.widthMultiplier = trailWidth * trailWidthMultiplier;
                trail.time = trailLength * trailLengthMultiplier;
            }
        }
    }
}