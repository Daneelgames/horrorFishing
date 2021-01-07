#if URP
using UnityEngine.Rendering.Universal;
#endif

using UnityEngine.Rendering;
using UnityEngine;

namespace SCPE
{
#if URP
    public class SketchRenderer : ScriptableRendererFeature
    {
        class SketchRenderPass : PostEffectRenderer<Sketch>
        {
            public SketchRenderPass()
            {
                shaderName = ShaderNames.Sketch;
                ProfilerTag = this.ToString();
            }

            public void Setup(RenderTargetIdentifier cameraColorTarget)
            {
                this.source = cameraColorTarget;
                settings = VolumeManager.instance.stack.GetComponent<Sketch>();
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

                var p = GL.GetGPUProjectionMatrix(renderingData.cameraData.camera.projectionMatrix, false);
                p[2, 3] = p[3, 2] = 0.0f;
                p[3, 3] = 1.0f;
                var clipToWorld = Matrix4x4.Inverse(p * renderingData.cameraData.camera.worldToCameraMatrix) * Matrix4x4.TRS(new Vector3(0, 0, -p[2, 2]), Quaternion.identity, Vector3.one);
                Material.SetMatrix("clipToWorld", clipToWorld);

                if (settings.strokeTex.value) Material.SetTexture("_Strokes", settings.strokeTex.value);

                Material.SetVector("_Params", new Vector4(0, (int)settings.blendMode.value, settings.intensity.value, ((int)settings.projectionMode.value == 1) ? settings.tiling.value * 0.1f : settings.tiling.value));
                Material.SetVector("_Brightness", settings.brightness.value);

                FinalBlit(this, context, cmd, mainTexID, source, Material, (int)settings.projectionMode.value);
            }
        }

        SketchRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new SketchRenderPass();

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
