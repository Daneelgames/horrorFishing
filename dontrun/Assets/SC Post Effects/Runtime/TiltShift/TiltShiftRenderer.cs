#if URP
using UnityEngine.Rendering.Universal;
#endif

using UnityEngine.Rendering;
using UnityEngine;

namespace SCPE
{
#if URP
    public class TiltShiftRenderer : ScriptableRendererFeature
    {
        class TiltShiftRenderPass : PostEffectRenderer<TiltShift>
        {
            int screenCopyID;
            private RenderTextureDescriptor blurredBuffDsc;           

            public TiltShiftRenderPass()
            {
                shaderName = ShaderNames.TiltShift;
                ProfilerTag = this.ToString();
                screenCopyID = Shader.PropertyToID("_BlurredTex");
            }

            enum Pass
            {
                FragHorizontal,
                FragHorizontalHQ,
                FragRadial,
                FragRadialHQ,
                FragBlend,
                FragDebug
            }

            public void Setup(RenderTargetIdentifier cameraColorTarget)
            {
                this.source = cameraColorTarget;
                settings = VolumeManager.instance.stack.GetComponent<TiltShift>();
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                if (!settings) return;

                base.Configure(cmd, cameraTextureDescriptor);

                blurredBuffDsc = cameraTextureDescriptor;
                //Require a high-precision alpha channel
                blurredBuffDsc.colorFormat = RenderTextureFormat.ARGBHalf;
                blurredBuffDsc.msaaSamples = 1; //No need to resolve AA for a blurred RT
                cmd.GetTemporaryRT(screenCopyID, blurredBuffDsc);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (!settings) return;
                if (settings.IsActive() == false) return;

                var cmd = CommandBufferPool.Get(ProfilerTag);

                Blit(cmd, source, mainTexID);

                Material.SetVector("_Params", new Vector4(settings.areaSize.value, settings.areaFalloff.value, settings.amount.value, (int)settings.mode.value));

                int pass = (int)settings.mode.value + (int)settings.quality.value;
                switch ((int)settings.mode.value)
                {
                    case 0:
                        pass = 0 + (int)settings.quality.value;
                        break;
                    case 1:
                        pass = 2 + (int)settings.quality.value;
                        break;
                }
                Blit(this, cmd, source, screenCopyID, Material, pass);
                cmd.SetGlobalTexture("_BlurredTex", screenCopyID);

                FinalBlit(this, context, cmd, mainTexID, source, Material, TiltShift.debug ? (int)Pass.FragDebug : (int)Pass.FragBlend);
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                base.FrameCleanup(cmd);
                cmd.ReleaseTemporaryRT(screenCopyID);
            }
        }

        TiltShiftRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new TiltShiftRenderPass();

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