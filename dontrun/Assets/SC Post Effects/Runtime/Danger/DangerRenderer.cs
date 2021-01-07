#if URP
using UnityEngine.Rendering.Universal;
#endif

using UnityEngine.Rendering;
using UnityEngine;

namespace SCPE
{
#if URP
    public class DangerRenderer : ScriptableRendererFeature
    {
        class DangerRenderPass : PostEffectRenderer<Danger>
        {
            public DangerRenderPass()
            {
                shaderName = ShaderNames.Danger;
                ProfilerTag = this.ToString();
            }

            public void Setup(RenderTargetIdentifier cameraColorTarget)
            {
                this.source = cameraColorTarget;
                settings = VolumeManager.instance.stack.GetComponent<Danger>();
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

                Material.SetVector("_Params", new Vector4(settings.intensity.value, settings.size.value, 0, 0));
                Material.SetColor("_Color", settings.color.value);
                var overlayTexture = settings.overlayTex.value == null ? Texture2D.blackTexture : settings.overlayTex.value;
                Material.SetTexture("_Overlay", overlayTexture);

                FinalBlit(this, context, cmd, mainTexID, source, Material, 0);
            }
        }

        DangerRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new DangerRenderPass();

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