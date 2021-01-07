#if URP
using UnityEngine.Rendering.Universal;
#endif

using UnityEngine.Rendering;
using UnityEngine;

namespace SCPE
{
#if URP
    public class DitheringRenderer : ScriptableRendererFeature
    {
        class DitheringRenderPass : PostEffectRenderer<Dithering>
        {
            public DitheringRenderPass()
            {
                shaderName = ShaderNames.Dithering;
                ProfilerTag = this.ToString();
            }

            public void Setup(RenderTargetIdentifier cameraColorTarget)
            {
                this.source = cameraColorTarget;
                settings = VolumeManager.instance.stack.GetComponent<Dithering>();
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                if (!settings) return;

                base.Configure(cmd, cameraTextureDescriptor);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (!settings) return;
                if (settings.IsActive() == false) return;

                var cmd = CommandBufferPool.Get(ProfilerTag);

                Blit(cmd, source, mainTexID);

                var lutTexture = settings.lut.value == null ? Texture2D.blackTexture : settings.lut.value;
                Material.SetTexture("_LUT", lutTexture);
                float luminanceThreshold = QualitySettings.activeColorSpace == ColorSpace.Gamma ? Mathf.LinearToGammaSpace(settings.luminanceThreshold.value) : settings.luminanceThreshold.value;

                Vector4 ditherParams = new Vector4(0f, settings.tiling.value, luminanceThreshold, settings.intensity.value);
                Material.SetVector("_Dithering_Coords", ditherParams);

                FinalBlit(this, context, cmd, mainTexID, source, Material, 0);
            }
        }

        DitheringRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new DitheringRenderPass();

            // Configures where the render pass should be injected.
            m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            m_ScriptablePass.Setup(renderer.cameraColorTarget);
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }
#endif
}