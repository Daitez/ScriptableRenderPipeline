using UnityEngine.Experimental.Rendering.HDPipeline;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    internal abstract class SerializedHDProbe
    {
        internal SerializedObject serializedObject;
        
        internal SerializedProperty proxyVolumeReference;
        internal SerializedProperty useInfluenceVolumeAsProxyVolume;

        internal SerializedInfluenceVolume influenceVolume;

        internal SerializedFrameSettings frameSettings;

        internal SerializedProperty lightLayers;
        internal SerializedProperty weight;
        internal SerializedProperty multiplier;

        internal SerializedProperty mode;

        internal SerializedProperty resolution;
        internal SerializedProperty shadowDistance;
        internal SerializedProperty cullingMask;
        internal SerializedProperty useOcclusionCulling;
        internal SerializedProperty nearClip;
        internal SerializedProperty farClip;

        internal HDProbe target { get { return serializedObject.targetObject as HDProbe; } }

        internal SerializedHDProbe(SerializedObject serializedObject)
        {
            this.serializedObject = serializedObject;

            proxyVolumeReference = serializedObject.Find((HDProbe p) => p.proxyVolume);
            influenceVolume = new SerializedInfluenceVolume(serializedObject.Find((HDProbe p) => p.influenceVolume));
            useInfluenceVolumeAsProxyVolume = serializedObject.Find((HDProbe p) => p.useInfluenceVolumeAsProxyVolume);

            frameSettings = new SerializedFrameSettings(serializedObject.Find((HDProbe p) => p.frameSettings));

            lightLayers = serializedObject.Find((HDProbe p) => p.lightLayers);
            weight = serializedObject.Find((HDProbe p) => p.weight);
            multiplier = serializedObject.Find((HDProbe p) => p.multiplier);

            mode = serializedObject.Find((HDProbe p) => p.mode);
        }

        internal virtual void Update()
        {
            serializedObject.Update();
            //InfluenceVolume does not have Update. Add it here if it have in the future.
        }

        internal virtual void Apply()
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
