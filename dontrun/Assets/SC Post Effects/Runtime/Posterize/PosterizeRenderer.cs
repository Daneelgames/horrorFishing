#if URP
using UnityEngine.Rendering.Universal;
#endif

using UnityEngine.Rendering;
using UnityEngine;

namespace SCPE
{
#if URP
    public class PosterizeRenderer : ScriptableRendererFeature
    {
        class PosterizeRenderPass : PostEffectRenderer<Posterize>
        {
            public PosterizeRenderPass()
            {
                shaderName = ShaderNames.Posterize;
                ProfilerTag = this.ToString();
            }

            public void Setup(RenderTargetIdentifier cameraColorTarget)
            {
                this.source = cameraColorTarget;
                settings = VolumeManager.instance.stack.GetComponent<Posterize>();
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

                Material.SetVector("_Params", new Vector4(settings.hue.value, settings.saturation.value, settings.value.value, settings.levels.value));

                FinalBlit(this, context, cmd, mainTexID, source, Material, settings.hsvMode.value ? 1 : 0);
            }
        }

        PosterizeRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new PosterizeRenderPass();

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