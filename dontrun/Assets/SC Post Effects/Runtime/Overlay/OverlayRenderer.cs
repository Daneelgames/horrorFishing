#if URP
using UnityEngine.Rendering.Universal;
#endif

using UnityEngine.Rendering;
using UnityEngine;

namespace SCPE
{
#if URP
    public class OverlayRenderer : ScriptableRendererFeature
    {
        class OverlayRenderPass : PostEffectRenderer<Overlay>
        {
            public OverlayRenderPass()
            {
                shaderName = ShaderNames.Overlay;
                ProfilerTag = this.ToString();
            }

            public void Setup(RenderTargetIdentifier cameraColorTarget)
            {
                this.source = cameraColorTarget;
                settings = VolumeManager.instance.stack.GetComponent<Overlay>();
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

                if (settings.overlayTex.value) Material.SetTexture("_OverlayTex", settings.overlayTex.value);
                Material.SetVector("_Params", new Vector4(settings.intensity.value, Mathf.Pow(settings.tiling.value + 1, 2), settings.autoAspect.value ? 1f : 0f, (int)settings.blendMode.value));

                FinalBlit(this, context, cmd, mainTexID, source, Material, 0);
            }
        }

        OverlayRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new OverlayRenderPass();

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