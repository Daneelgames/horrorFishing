#if URP
using UnityEngine.Rendering.Universal;
#endif

using UnityEngine.Rendering;
using UnityEngine;

namespace SCPE
{
#if URP
    public class RadialBlurRenderer : ScriptableRendererFeature
    {
        class RadialBlurRenderPass : PostEffectRenderer<RadialBlur>
        {
            public RadialBlurRenderPass()
            {
                shaderName = ShaderNames.RadialBlur;
                ProfilerTag = this.ToString();
            }

            public void Setup(RenderTargetIdentifier cameraColorTarget)
            {
                this.source = cameraColorTarget;
                settings = VolumeManager.instance.stack.GetComponent<RadialBlur>();
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

                Material.SetFloat("_Amount", settings.amount.value / 50);
                Material.SetFloat("_Iterations", settings.iterations.value);

                FinalBlit(this, context, cmd, mainTexID, source, Material, 0);
            }
        }

        RadialBlurRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new RadialBlurRenderPass();

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