using System;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.HDPipeline;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    public class LitShaderPreprocessor : BaseShaderPreprocessor
    {
        public LitShaderPreprocessor() {}

        public override bool ShadersStripper(HDRenderPipelineAsset hdrpAsset, Shader shader, ShaderSnippetData snippet, ShaderCompilerData inputData)
        {
            bool isGBufferPass = snippet.passName == "GBuffer";
            //bool isForwardPass = snippet.passName == "Forward";
            bool isDepthOnlyPass = snippet.passName == "DepthOnly";
            bool isTransparentPrepass = snippet.passName == "TransparentDepthPrepass";
            bool isTransparentPostpass = snippet.passName == "TransparentDepthPostpass";
            bool isDistortionPass = snippet.passName == "DistortionVectors";
            bool isTransparentForwardPass = isTransparentPostpass || snippet.passName == "TransparentBackface" || isTransparentPrepass;


            if (isDistortionPass && !hdrpAsset.renderPipelineSettings.supportDistortion)
                return true;
            //// We gather information that could be spread between multiple frame settings (e.g. is transparent pre-pass supported in any frame setting?)
            //bool isTransparentPrePassSupported = hdrpAsset.GetFrameSettings().enableTransparentPrepass                          ||
            //                                     hdrpAsset.GetRealtimeReflectionFrameSettings().enableTransparentPrepass;

            //bool isTransparentPostPassSupported = hdrpAsset.GetFrameSettings().enableTransparentPostpass                        ||
            //                                      hdrpAsset.GetRealtimeReflectionFrameSettings().enableTransparentPostpass;

            //bool isDistortionPassSupported      = hdrpAsset.GetFrameSettings().enableDistortion                                 ||
            //                                      hdrpAsset.GetRealtimeReflectionFrameSettings().enableDistortion;

            //if (isDistortionPass && !isDistortionPassSupported)
            //    return true;

            //if (isTransparentPrepass && !isTransparentPrePassSupported)
            //    return true;

            //if (isTransparentPostpass && !isTransparentPostPassSupported)
            //    return true;


            // When using forward only, we never need GBuffer pass (only Forward)
            if (hdrpAsset.renderPipelineSettings.supportedLitShaderMode == RenderPipelineSettings.SupportedLitShaderMode.ForwardOnly && isGBufferPass)
                return true;

            if (inputData.shaderKeywordSet.IsEnabled(m_Transparent))
            {
                // If transparent, we never need GBuffer pass.
                if (isGBufferPass)
                    return true;
            }
            else // Opaque
            {
                // If opaque, we never need transparent specific passes (even in forward only mode)
                if (isTransparentForwardPass)
                    return true;

                //// When we are in deferred, we only support tile lighting
                if (shader.name.Contains("Lit") && hdrpAsset.renderPipelineSettings.supportedLitShaderMode == RenderPipelineSettings.SupportedLitShaderMode.DeferredOnly && inputData.shaderKeywordSet.IsEnabled(m_ClusterLighting))
                    return true;

                if (isDepthOnlyPass)
                {
                    // When we are full forward, we don't have depth prepass without writeNormalBuffer
                    if (hdrpAsset.renderPipelineSettings.supportedLitShaderMode == RenderPipelineSettings.SupportedLitShaderMode.ForwardOnly && !inputData.shaderKeywordSet.IsEnabled(m_WriteNormalBuffer))
                        return true;
                }

                // TODO: add an option to say we are using only the deferred shader variant (for Lit)
                //if (0)
                {
                    // If opaque and not forward only, then we only need the forward debug pass.
                    //if (isForwardPass && !inputData.shaderKeywordSet.IsEnabled(m_DebugDisplay))
                    //    return true;
                }
            }

            // TODO: Tests for later
            // We need to find a way to strip useless shader features for passes/shader stages that don't need them (example, vertex shaders won't ever need SSS Feature flag)
            // This causes several problems:
            // - Runtime code that "finds" shader variants based on feature flags might not find them anymore... thus fall backing to the "let's give a score to variant" code path that may find the wrong variant.
            // - Another issue is that if a feature is declared without a "_" fall-back, if we strip the other variants, none may be left to use! This needs to be changed on our side.
            //if (snippet.shaderType == ShaderType.Vertex && inputData.shaderKeywordSet.IsEnabled(m_FeatureSSS))
            //    return true;

            return false;
        }
    }
}
