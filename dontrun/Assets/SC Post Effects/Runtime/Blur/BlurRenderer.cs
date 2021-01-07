#if URP
using UnityEngine.Rendering.Universal;
#endif

using UnityEngine.Rendering;
using UnityEngine;

namespace SCPE
{
#if URP
    public class BlurRenderer : ScriptableRendererFeature
    {
        class BlurRenderPass : PostEffectRenderer<Blur>
        {
            int blurredID;
            int blurredID2;

            enum Pass
            {
                Blend,
                Gaussian,
                Box
            }
            public BlurRenderPass()
            {
                shaderName = ShaderNames.Blur;
                ProfilerTag = this.ToString();
                mainTexID = Shader.PropertyToID("_MainTex");
                blurredID = Shader.PropertyToID("_Temp1");
                blurredID2 = Shader.PropertyToID("_Temp2");
            }

            public void Setup(RenderTargetIdentifier cameraColorTarget)
            {
                this.source = cameraColorTarget;
                settings = VolumeManager.instance.stack.GetComponent<Blur>();
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                if (!settings) return;
                cmd.GetTemporaryRT(mainTexID, cameraTextureDescriptor);

                RenderTextureDescriptor opaqueDesc = cameraTextureDescriptor;
                //opaqueDesc.depthBufferBits = 0;

                opaqueDesc.width /= settings.downscaling.value;
                opaqueDesc.height /= settings.downscaling.value;
                //opaqueDesc.msaaSamples = 0;

                cmd.GetTemporaryRT(blurredID, opaqueDesc);
                cmd.GetTemporaryRT(blurredID2, opaqueDesc);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (!settings) return;

                if (settings.IsActive() == false) return;

                var cmd = CommandBufferPool.Get(ProfilerTag);

                Blit(cmd, source, mainTexID);
                Blit(cmd, source, blurredID);

                int blurPass = (settings.mode == Blur.BlurMethod.Gaussian) ? (int)Pass.Gaussian : (int)Pass.Box;

                for (int i = 0; i < settings.iterations.value; i++)
                {
                    //Safeguard for exploding GPUs
                    if (settings.iterations.value > 12) return;

                    // horizontal blur
                    cmd.SetGlobalVector("_BlurOffsets", new Vector4(settings.amount.value / renderingData.cameraData.camera.scaledPixelWidth, 0, 0, 0));
                    Blit(this, cmd, blurredID, blurredID2, Material, blurPass);

                    // vertical blur
                    cmd.SetGlobalVector("_BlurOffsets", new Vector4(0, settings.amount.value / renderingData.cameraData.camera.scaledPixelHeight, 0, 0));
                    Blit(this, cmd, blurredID2, blurredID, Material, blurPass);

                    //Double blur
                    if (settings.highQuality.value)
                    {
                        // horizontal blur
                        cmd.SetGlobalVector("_BlurOffsets", new Vector4(settings.amount.value / renderingData.cameraData.camera.scaledPixelWidth, 0, 0, 0));
                        Blit(this, cmd, blurredID, blurredID2, Material, blurPass);

                        // vertical blur
                        cmd.SetGlobalVector("_BlurOffsets", new Vector4(0, settings.amount.value / renderingData.cameraData.camera.scaledPixelHeight, 0, 0));
                        Blit(this, cmd, blurredID2, blurredID, Material, blurPass);
                    }
                }

                FinalBlit(this, context, cmd, blurredID, source, Material, (int)Pass.Blend);
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                base.FrameCleanup(cmd);

                cmd.ReleaseTemporaryRT(blurredID);
                cmd.ReleaseTemporaryRT(blurredID2);
            }
        }

        BlurRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new BlurRenderPass();

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