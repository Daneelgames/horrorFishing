#if URP
using UnityEngine.Rendering.Universal;
#endif

using UnityEngine.Rendering;
using UnityEngine;

namespace SCPE
{
#if URP
    public class MosaicRenderer : ScriptableRendererFeature
    {
        class MosaicRenderPass : PostEffectRenderer<Mosaic>
        {
            public MosaicRenderPass()
            {
                shaderName = ShaderNames.Mosaic;
                ProfilerTag = this.ToString();
            }

            public void Setup(RenderTargetIdentifier cameraColorTarget)
            {
                this.source = cameraColorTarget;
                settings = VolumeManager.instance.stack.GetComponent<Mosaic>();
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

                float size = settings.size.value;

                switch ((Mosaic.MosaicMode)settings.mode)
                {
                    case Mosaic.MosaicMode.Triangles:
                        size = 10f / settings.size.value;
                        break;
                    case Mosaic.MosaicMode.Hexagons:
                        size = settings.size.value / 10f;
                        break;
                    case Mosaic.MosaicMode.Circles:
                        size = (1 - settings.size.value) * 100f;
                        break;
                }

                Vector4 parameters = new Vector4(size, ((renderingData.cameraData.camera.scaledPixelWidth * 2 / renderingData.cameraData.camera.scaledPixelHeight) * size / Mathf.Sqrt(3f)), 0f, 0f);

                Material.SetVector("_Params", parameters);

                FinalBlit(this, context, cmd, mainTexID, source, Material, (int)settings.mode.value);
            }
        }

        MosaicRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new MosaicRenderPass();

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