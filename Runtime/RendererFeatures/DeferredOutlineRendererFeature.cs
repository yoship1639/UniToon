using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_2021_2_OR_NEWER
namespace UniToon
{
    public class DeferredOutlineRendererFeature : ScriptableRendererFeature
    {
        class DeferredOutlineRenderPass : ScriptableRenderPass
        {
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
            }

            public bool Setup(ScriptableRenderer renderer)
            {
                ConfigureInput(ScriptableRenderPassInput.Normal);
                return true;
            }
        }

        DeferredOutlineRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new DeferredOutlineRenderPass();
            m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingGbuffer;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (m_ScriptablePass.Setup(renderer))
            {
                renderer.EnqueuePass(m_ScriptablePass);
            }
        }
    }
}
#endif
