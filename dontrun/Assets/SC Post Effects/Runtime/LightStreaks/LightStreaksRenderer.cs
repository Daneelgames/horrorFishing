#if URP
using UnityEngine.Rendering.Universal;
#endif

using UnityEngine.Rendering;
using UnityEngine;

namespace SCPE
{
#if URP
    public class LightStreaksRenderer : ScriptableRendererFeature
    {
        class LightStreaksRenderPass : PostEffectRenderer<LightStreaks>
        {
            private int emissionTex;
            int blurredID;
            int blurredID2;

            enum Pass
            {
                LuminanceDiff,
                BlurFast,
                Blur,
                Blend,
                Debug
            }

            public LightStreaksRenderPass()
            {
                shaderName = ShaderNames.LightStreaks;
                ProfilerTag = this.ToString();
                emissionTex = Shader.PropertyToID("_BloomTex");
                blurredID = Shader.PropertyToID("_Temp1");
                blurredID2 = Shader.PropertyToID("_Temp2");
            }

            public void Setup(RenderTargetIdentifier cameraColorTarget)
            {
                this.source = cameraColorTarget;
                settings = VolumeManager.instance.stack.GetComponent<LightStreaks>();
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                if (!settings) return;

                base.Configure(cmd, cameraTextureDescriptor);

                cmd.GetTemporaryRT(emissionTex, cameraTextureDescriptor);

                RenderTextureDescriptor opaqueDesc = cameraTextureDescriptor;
                opaqueDesc.width /= settings.downscaling.value;
                opaqueDesc.height /= settings.downscaling.value;

                cmd.GetTemporaryRT(blurredID, opaqueDesc);
                cmd.GetTemporaryRT(blurredID2, opaqueDesc);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (!settings) return;
                if (settings.IsActive() == false) return;

                var cmd = CommandBufferPool.Get(ProfilerTag);

                int blurMode = (settings.quality.value == LightStreaks.Quality.Performance) ? (int)Pass.BlurFast : (int)Pass.Blur;

                float luminanceThreshold = Mathf.GammaToLinearSpace(settings.luminanceThreshold.value);

                Material.SetVector("_Params", new Vector4(luminanceThreshold, settings.intensity.value, 0f, 0f));

                Blit(cmd, source, mainTexID);
                Blit(this, cmd, source, emissionTex, Material, (int)Pass.LuminanceDiff);
                Blit(cmd, emissionTex, blurredID);

                float ratio = Mathf.Clamp(settings.direction.value, -1, 1);
                float rw = ratio < 0 ? -ratio * 1f : 0f;
                float rh = ratio > 0 ? ratio * 4f : 0f;

                int iterations = (settings.quality.value == LightStreaks.Quality.Performance) ? settings.iterations.value * 3 : settings.iterations.value;

                for (int i = 0; i < iterations; i++)
                {
                    // horizontal blur
                    cmd.SetGlobalVector("_BlurOffsets", new Vector4(rw * settings.blur.value / renderingData.cameraData.camera.scaledPixelWidth, rh / renderingData.cameraData.camera.scaledPixelHeight, 0, 0));
                    Blit(this, cmd, blurredID, blurredID2, Material, blurMode);

                    // vertical blur
                    cmd.SetGlobalVector("_BlurOffsets", new Vector4((rw * settings.blur.value) * 2f / renderingData.cameraData.camera.scaledPixelWidth, rh * 2f / renderingData.cameraData.camera.scaledPixelHeight, 0, 0));
                    Blit(this, cmd, blurredID2, blurredID, Material, blurMode);
                }

                cmd.SetGlobalTexture("_BloomTex", blurredID);

                FinalBlit(this, context, cmd, mainTexID, source, Material, (settings.debug.value) ? (int)Pass.Debug : (int)Pass.Blend);
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                base.FrameCleanup(cmd);

                cmd.ReleaseTemporaryRT(emissionTex);
                cmd.ReleaseTemporaryRT(blurredID);
                cmd.ReleaseTemporaryRT(blurredID2);
            }
        }

        LightStreaksRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new LightStreaksRenderPass();

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