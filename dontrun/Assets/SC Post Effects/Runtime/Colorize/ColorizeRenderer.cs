#if URP
using UnityEngine.Rendering.Universal;
#endif

using UnityEngine.Rendering;
using UnityEngine;

namespace SCPE
{
#if URP
    public class ColorizeRenderer : ScriptableRendererFeature
    {
        class ColorizeRenderPass : PostEffectRenderer<Colorize>
        {
            public ColorizeRenderPass()
            {
                shaderName = ShaderNames.Colorize;
                ProfilerTag = this.ToString();
            }

            public void Setup(RenderTargetIdentifier cameraColorTarget)
            {
                this.source = cameraColorTarget;
                settings = VolumeManager.instance.stack.GetComponent<Colorize>();
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

                if (settings.colorRamp.value) Material.SetTexture("_ColorRamp", settings.colorRamp.value);
                Material.SetFloat("_Intensity", settings.intensity.value);
                Material.SetFloat("_BlendMode", (int)settings.mode.value);

                FinalBlit(this, context, cmd, mainTexID, source, Material, 0);
            }
        }

        ColorizeRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new ColorizeRenderPass();

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