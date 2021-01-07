#if URP
using UnityEngine.Rendering.Universal;
#endif

using UnityEngine.Rendering;
using UnityEngine;

namespace SCPE
{
#if URP
    public class KuwaharaRenderer : ScriptableRendererFeature
    {
        class KuwaharaRenderPass : PostEffectRenderer<Kuwahara>
        {
            public KuwaharaRenderPass()
            {
                shaderName = ShaderNames.Kuwahara;
                ProfilerTag = this.ToString();
            }

            public void Setup(RenderTargetIdentifier cameraColorTarget)
            {
                this.source = cameraColorTarget;
                settings = VolumeManager.instance.stack.GetComponent<Kuwahara>();
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

                Material.SetFloat("_Radius", (float)settings.radius);
                Material.SetFloat("_FadeDistance", settings.fadeDistance.value);
                Material.SetVector("_DistanceParams", new Vector4(settings.fadeDistance.value, (settings.invertFadeDistance.value) ? 1 : 0, 0, 0));

                FinalBlit(this, context, cmd, mainTexID, source, Material, (int)settings.mode.value);
            }
        }

        KuwaharaRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new KuwaharaRenderPass();

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