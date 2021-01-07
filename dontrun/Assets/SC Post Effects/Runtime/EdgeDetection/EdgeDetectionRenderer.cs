#if URP
using UnityEngine.Rendering.Universal;
#endif

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace SCPE
{
#if URP
    public class EdgeDetectionRenderer : ScriptableRendererFeature
    {
        class EdgeDetectionRenderPass : PostEffectRenderer<EdgeDetection>
        {

            public EdgeDetectionRenderPass()
            {
                shaderName = ShaderNames.EdgeDetection;
                ProfilerTag = this.ToString();
            }

            public void Setup(RenderTargetIdentifier cameraColorTarget)
            {
                this.source = cameraColorTarget;
                settings = VolumeManager.instance.stack.GetComponent<EdgeDetection>();
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                if (!settings) return;

                requireDepthNormals = settings.RequireDepthNormals();

                base.Configure(cmd, cameraTextureDescriptor);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (!settings) return;
                if (settings.IsActive() == false) return;

                var cmd = CommandBufferPool.Get(ProfilerTag);

                Blit(cmd, source, mainTexID);

                if (requireDepthNormals) GenerateDepthNormals(this, cmd, depthNormalsID);

                Vector2 sensitivity = new Vector2(settings.sensitivityDepth.value, settings.sensitivityNormals.value);
                Material.SetVector("_Sensitivity", sensitivity);
                Material.SetFloat("_BackgroundFade", (settings.debug.value) ? 1f : 0f);
                Material.SetFloat("_EdgeSize", settings.edgeSize.value);
                Material.SetFloat("_Exponent", settings.edgeExp.value);
                Material.SetFloat("_Threshold", settings.lumThreshold.value);
                Material.SetColor("_EdgeColor", settings.edgeColor.value);
                Material.SetFloat("_EdgeOpacity", settings.edgeOpacity.value);

                float fadeDist = (renderingData.cameraData.camera.orthographic) ? settings.fadeDistance.value * (float)1e-10 : settings.fadeDistance.value;
                Material.SetVector("_DistanceParams", new Vector4(fadeDist, (settings.invertFadeDistance.value) ? 1 : 0, 0, 0));

                Material.SetVector("_SobelParams", new Vector4((settings.sobelThin.value) ? 1 : 0, 0, 0, 0));

                FinalBlit(this, context, cmd, mainTexID, source, Material, (int)settings.mode.value);
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                base.FrameCleanup(cmd);

                cmd.ReleaseTemporaryRT(depthNormalsID);

            }
        }

        EdgeDetectionRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new EdgeDetectionRenderPass();

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
