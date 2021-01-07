#if URP
using UnityEngine.Rendering.Universal;
#endif

using UnityEngine.Rendering;
using UnityEngine;

namespace SCPE
{
#if URP
    public class SpeedLinesRenderer : ScriptableRendererFeature
    {
        class SpeedLinesRenderPass : PostEffectRenderer<SpeedLines>
        {
            public SpeedLinesRenderPass()
            {
                shaderName = ShaderNames.SpeedLines;
                ProfilerTag = this.ToString();
            }

            public void Setup(RenderTargetIdentifier cameraColorTarget)
            {
                this.source = cameraColorTarget;
                settings = VolumeManager.instance.stack.GetComponent<SpeedLines>();
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

                float falloff = 2f + (settings.falloff.value - 0.0f) * (16.0f - 2f) / (1.0f - 0.0f);
                Material.SetVector("_Params", new Vector4(settings.intensity.value, falloff, settings.size.value * 2, 0));
                if (settings.noiseTex.value) Material.SetTexture("_NoiseTex", settings.noiseTex.value);

                FinalBlit(this, context, cmd, mainTexID, source, Material, 0);
            }
        }

        SpeedLinesRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new SpeedLinesRenderPass();

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