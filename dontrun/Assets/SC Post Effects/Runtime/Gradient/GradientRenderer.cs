#if URP
using UnityEngine.Rendering.Universal;
#endif

using UnityEngine.Rendering;
using UnityEngine;

namespace SCPE
{
#if URP
    public class GradientRenderer : ScriptableRendererFeature
    {
        class GradientRenderPass : PostEffectRenderer<Gradient>
        {
            public GradientRenderPass()
            {
                shaderName = ShaderNames.Gradient;
                ProfilerTag = this.ToString();
            }

            public void Setup(RenderTargetIdentifier cameraColorTarget)
            {
                this.source = cameraColorTarget;
                settings = VolumeManager.instance.stack.GetComponent<Gradient>();
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

                if (settings.gradientTex.value) Material.SetTexture("_Gradient", settings.gradientTex.value);
                Material.SetColor("_Color1", settings.color1.value);
                Material.SetColor("_Color2", settings.color2.value);
                Material.SetFloat("_Rotation", settings.rotation.value * 6);
                Material.SetFloat("_Intensity", settings.intensity.value);
                Material.SetFloat("_BlendMode", (int)settings.mode.value);

                FinalBlit(this, context, cmd, mainTexID, source, Material, (int)settings.input.value);
            }
        }

        GradientRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new GradientRenderPass();

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