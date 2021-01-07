#if URP
using UnityEngine.Rendering.Universal;
#endif

using UnityEngine;
using UnityEngine.Rendering;

namespace SCPE
{
#if URP
    public class SunshaftsRenderer : ScriptableRendererFeature
    {
        class SunshaftsRenderPass : PostEffectRenderer<Sunshafts>
        {
            private int skyboxBufferID;
            int blurredID;
            int blurredID2;
            private Vector2Int resolution;
            private int downsampling;

            public SunshaftsRenderPass()
            {
                shaderName = ShaderNames.Sunshafts;
                ProfilerTag = this.ToString();
                skyboxBufferID = Shader.PropertyToID("_SkyboxBuffer");
                blurredID = Shader.PropertyToID("_Temp1");
                blurredID2 = Shader.PropertyToID("_Temp2");
            }

            public void Setup(RenderTargetIdentifier cameraColorTarget)
            {
                this.source = cameraColorTarget;
                settings = VolumeManager.instance.stack.GetComponent<Sunshafts>();
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                if (!settings) return;

                downsampling = (int)settings.resolution.value;

                RenderTextureDescriptor opaqueDesc = cameraTextureDescriptor;
                opaqueDesc.depthBufferBits = 0;
                resolution = new Vector2Int(opaqueDesc.width, opaqueDesc.height);
                opaqueDesc.width /= 2;
                opaqueDesc.height /= 2;
                cmd.GetTemporaryRT(skyboxBufferID, opaqueDesc);

                opaqueDesc.width = cameraTextureDescriptor.width / (int)settings.resolution.value;
                opaqueDesc.height = cameraTextureDescriptor.height / (int)settings.resolution.value;
                opaqueDesc.msaaSamples = 1;

                cmd.GetTemporaryRT(blurredID, opaqueDesc);
                cmd.GetTemporaryRT(blurredID2, opaqueDesc);

                base.Configure(cmd, cameraTextureDescriptor);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (!settings) return;
                if (settings.IsActive() == false) return;

                var cmd = CommandBufferPool.Get(ProfilerTag);

                int res = (int)settings.resolution.value;

                //If resolution changed, recreate buffers
                if(res != downsampling)
                {
                    cmd.ReleaseTemporaryRT(blurredID);
                    cmd.ReleaseTemporaryRT(blurredID2);

                    cmd.GetTemporaryRT(blurredID, resolution.x / res, resolution.y / res, 0, FilterMode.Bilinear);
                    cmd.GetTemporaryRT(blurredID2, resolution.x / res, resolution.y / res, 0, FilterMode.Bilinear);

                    cmd.ReleaseTemporaryRT(mainTexID);
                    cmd.GetTemporaryRT(mainTexID, resolution.x / res, resolution.y / res, 0, FilterMode.Bilinear);

                    Debug.Log("Recreated blur buffers");
                    downsampling = res;
                }

                #region Parameters
                float sunIntensity = (settings.useCasterIntensity.value) ? SunshaftCaster.intensity : settings.sunShaftIntensity.value;

                //Screen-space sun position
                Vector3 v = Vector3.one * 0.5f;
                if (Sunshafts.sunPosition != Vector3.zero)
                    v = renderingData.cameraData.camera.WorldToViewportPoint(Sunshafts.sunPosition);
                else
                    v = new Vector3(0.5f, 0.5f, 0.0f);
                cmd.SetGlobalVector("_SunPosition", new Vector4(v.x, v.y, sunIntensity, settings.falloff.value));

                Color col = (settings.useCasterColor.value) ? SunshaftCaster.color : settings.sunColor.value;
                cmd.SetGlobalFloat("_BlendMode", (int)settings.blendMode.value);
                cmd.SetGlobalColor("_SunColor", (v.z >= 0.0f) ? col : new Color(0, 0, 0, 0));
                cmd.SetGlobalColor("_SunThreshold", settings.sunThreshold.value);
                #endregion


                #region Blur
                cmd.BeginSample("Sunshafts blur");

                Blit(cmd, source, mainTexID);
                Blit(this, cmd, mainTexID, skyboxBufferID, Material, (int)SunshaftsBase.Pass.SkySource);
                Blit(cmd, skyboxBufferID, blurredID);

                float offset = settings.length.value * (1.0f / 768.0f);

                int iterations = (settings.highQuality.value) ? 2 : 1;
                float blurAmount = (settings.highQuality.value) ? settings.length.value / 2.5f : settings.length.value;

                for (int i = 0; i < iterations; i++)
                {
                    Blit(this, cmd, blurredID, blurredID2, Material, (int)SunshaftsBase.Pass.RadialBlur);
                    offset = blurAmount * (((i * 2.0f + 1.0f) * 6.0f)) / renderingData.cameraData.camera.pixelWidth;
                    cmd.SetGlobalFloat("_BlurRadius", offset);

                    Blit(this, cmd, blurredID2, blurredID, Material, (int)SunshaftsBase.Pass.RadialBlur);
                    offset = blurAmount * (((i * 2.0f + 1.0f) * 6.0f)) / renderingData.cameraData.camera.pixelHeight;
                    cmd.SetGlobalFloat("_BlurRadius", offset);

                }
                cmd.EndSample("Sunshafts blur");

                #endregion

                cmd.SetGlobalTexture("_SunshaftBuffer", blurredID);

                FinalBlit(this, context, cmd, mainTexID, source, Material, (int)SunshaftsBase.Pass.Blend);
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                base.FrameCleanup(cmd);

                cmd.ReleaseTemporaryRT(skyboxBufferID);
                cmd.ReleaseTemporaryRT(blurredID);
                cmd.ReleaseTemporaryRT(blurredID2);
            }
        }

        SunshaftsRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new SunshaftsRenderPass();

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
