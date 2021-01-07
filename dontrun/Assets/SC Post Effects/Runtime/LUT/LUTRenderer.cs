#if URP
using UnityEngine.Rendering.Universal;
#endif

using UnityEngine.Rendering;
using UnityEngine;

namespace SCPE
{
#if URP
    public class LUTRenderer : ScriptableRendererFeature
    {
        class LUTRenderPass : PostEffectRenderer<LUT>
        {

            public LUTRenderPass()
            {
                shaderName = ShaderNames.LUT;
                ProfilerTag = this.ToString();
            }

            public void Setup(RenderTargetIdentifier cameraColorTarget)
            {
                this.source = cameraColorTarget;
                settings = VolumeManager.instance.stack.GetComponent<LUT>();
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

                if (settings.lutNear.value)
                {
                    Material.SetTexture("_LUT_Near", settings.lutNear.value);
                    Material.SetVector("_LUT_Params", new Vector4(1f / settings.lutNear.value.width, 1f / settings.lutNear.value.height, settings.lutNear.value.height - 1f, settings.intensity.value));
                }

                if ((int)settings.mode.value == 1)
                {
                    Material.SetFloat("_Distance", settings.distance.value);
                    if (settings.lutFar.value) Material.SetTexture("_LUT_Far", settings.lutFar.value);
                }

                FinalBlit(this, context, cmd, mainTexID, source, Material, (int)settings.mode.value);
            }
        }

        LUTRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new LUTRenderPass();

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
