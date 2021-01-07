#if URP
using UnityEngine.Rendering.Universal;
#endif

using UnityEngine.Rendering;
using UnityEngine;

namespace SCPE
{
#if URP
    public class RipplesRenderer : ScriptableRendererFeature
    {
        class RipplesRenderPass : PostEffectRenderer<Ripples>
        {
            public RipplesRenderPass()
            {
                shaderName = ShaderNames.Ripples;
                ProfilerTag = this.ToString();
            }

            public void Setup(RenderTargetIdentifier cameraColorTarget)
            {
                this.source = cameraColorTarget;
                settings = VolumeManager.instance.stack.GetComponent<Ripples>();
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

                Material.SetFloat("_Strength", (settings.strength.value * 0.01f));
                Material.SetFloat("_Distance", (settings.distance.value * 0.01f));
                Material.SetFloat("_Speed", settings.speed.value);
                Material.SetVector("_Size", new Vector2(settings.width.value, settings.height.value));

                FinalBlit(this, context, cmd, mainTexID, source, Material, (int)settings.mode.value);
            }
        }

        RipplesRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new RipplesRenderPass();

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