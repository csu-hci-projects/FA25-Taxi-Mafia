using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Water_Volume : ScriptableRendererFeature
{
  class CustomRenderPass : ScriptableRenderPass
  {
    public RTHandle source;

    private readonly Material _material;
    private RTHandle _tempColor;

    public CustomRenderPass(Material mat)
    {
      _material = mat;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
      var desc = renderingData.cameraData.cameraTargetDescriptor;
      desc.depthBufferBits = 0;
      RenderingUtils.ReAllocateIfNeeded(ref _tempColor, desc, name: "_TemporaryColourTexture");
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
      if (renderingData.cameraData.cameraType == CameraType.Reflection) return;

      var cmd = CommandBufferPool.Get("Water_Volume Pass");

      // Material pass index 0 by default; change if you have multiple passes
      Blitter.BlitCameraTexture(cmd, source, _tempColor, _material, 0);
      Blitter.BlitCameraTexture(cmd, _tempColor, source);

      context.ExecuteCommandBuffer(cmd);
      CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
      _tempColor?.Release();
    }
  }

  [System.Serializable]
  public class _Settings
  {
    public Material material = null;
    public RenderPassEvent renderPass = RenderPassEvent.AfterRenderingSkybox;
  }

  public _Settings settings = new _Settings();
  CustomRenderPass m_ScriptablePass;

  public override void Create()
  {
    if (settings.material == null)
      settings.material = (Material)Resources.Load("Water_Volume");

    m_ScriptablePass = new CustomRenderPass(settings.material)
    {
      renderPassEvent = settings.renderPass
    };
  }

  public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
  {
    m_ScriptablePass.source = renderer.cameraColorTargetHandle;
    renderer.EnqueuePass(m_ScriptablePass);
  }
}
