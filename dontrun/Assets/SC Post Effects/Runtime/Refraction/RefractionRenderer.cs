#if URP
using UnityEngine.Rendering.Universal;
#endif

using UnityEngine.Rendering;
using UnityEngine;

namespace SCPE
{
#if URP
    public class RefractionRenderer : ScriptableRendererFeature
    {
        class RefractionRenderPass : PostEffectRenderer<Refraction>
        {
            public RefractionRenderPass()
            {
                shaderName = ShaderNames.Refraction;
                ProfilerTag = this.ToString();
            }

            public void Setup(RenderTargetIdentifier cameraColorTarget)
            {
                this.source = cameraColorTarget;
                settings = VolumeManager.instance.stack.GetComponent<Refraction>();
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

                Material.SetFloat("_Amount", settings.amount.value);
                if (settings.refractionTex.value) Material.SetTexture("_RefractionTex", settings.refractionTex.value);

                FinalBlit(this, context, cmd, mainTexID, source, Material, (settings.convertNormalMap.value) ? 1 : 0);
            }
        }

        RefractionRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new RefractionRenderPass();

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