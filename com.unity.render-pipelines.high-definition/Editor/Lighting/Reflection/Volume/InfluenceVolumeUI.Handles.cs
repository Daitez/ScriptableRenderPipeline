using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;
using Object = UnityEngine.Object;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    partial class InfluenceVolumeUI
    {
        public static void DrawHandles_EditBase(InfluenceVolumeUI s, SerializedInfluenceVolume d, Editor o, Matrix4x4 matrix, Object sourceAsset)
        {
            using (new Handles.DrawingScope(k_GizmoThemeColorBase, matrix))
            {
                switch ((InfluenceShape)d.shape.intValue)
                {
                    case InfluenceShape.Box:
                        {
                            s.boxBaseHandle.center = Vector3.zero;
                            s.boxBaseHandle.size = d.boxSize.vector3Value;

                            EditorGUI.BeginChangeCheck();
                            s.boxBaseHandle.DrawHandle();
                            s.boxBaseHandle.DrawHull(true);
                            if (EditorGUI.EndChangeCheck())
                            {
                                d.boxSize.vector3Value = s.boxBaseHandle.size;
                            }
                            break;
                        }
                    case InfluenceShape.Sphere:
                        {
                            s.sphereBaseHandle.center = Vector3.zero;
                            s.sphereBaseHandle.radius = d.sphereRadius.floatValue;

                            EditorGUI.BeginChangeCheck();
                            s.sphereBaseHandle.DrawHandle();
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(sourceAsset, "Modified Base Volume AABB");

                                float radius = s.sphereBaseHandle.radius;
                                d.sphereRadius.floatValue = radius;
                                d.sphereBlendDistance.floatValue = Mathf.Clamp(d.sphereBlendDistance.floatValue, 0, radius);
                                d.sphereBlendNormalDistance.floatValue = Mathf.Clamp(d.sphereBlendNormalDistance.floatValue, 0, radius);
                            }
                            break;
                        }
                }
            }
        }

        public static void DrawHandles_EditInfluence(InfluenceVolumeUI s, SerializedInfluenceVolume d, Editor o, Matrix4x4 matrix, Object sourceAsset)
        {
            using (new Handles.DrawingScope(k_GizmoThemeColorInfluence, matrix))
            {
                switch ((InfluenceShape)d.shape.intValue)
                {
                    case InfluenceShape.Box:
                        EditorGUI.BeginChangeCheck();
                        DrawBoxFadeHandle(s, d, o, sourceAsset, s.boxInfluenceHandle, d.boxBlendDistancePositive, d.boxBlendDistanceNegative);
                        if (EditorGUI.EndChangeCheck())
                        {
                            //save advanced/simplified saved data
                            if (d.editorAdvancedModeEnabled.boolValue)
                            {
                                d.editorAdvancedModeBlendDistancePositive.vector3Value = d.boxBlendDistancePositive.vector3Value;
                                d.editorAdvancedModeBlendDistanceNegative.vector3Value = d.boxBlendDistanceNegative.vector3Value;
                            }
                            else
                            {
                                d.editorSimplifiedModeBlendDistance.floatValue = d.boxBlendDistancePositive.vector3Value.x;
                            }
                            d.Apply();
                        }
                        break;
                    case InfluenceShape.Sphere:
                        DrawSphereFadeHandle(s, d, o, sourceAsset, s.sphereInfluenceHandle, d.sphereBlendDistance);
                        break;
                }
            }
        }

        public static void DrawHandles_EditInfluenceNormal(InfluenceVolumeUI s, SerializedInfluenceVolume d, Editor o, Matrix4x4 matrix, Object sourceAsset)
        {
            using (new Handles.DrawingScope(k_GizmoThemeColorInfluenceNormal, matrix))
            {
                switch ((InfluenceShape)d.shape.intValue)
                {
                    case InfluenceShape.Box:
                        EditorGUI.BeginChangeCheck();
                        DrawBoxFadeHandle(s, d, o, sourceAsset, s.boxInfluenceNormalHandle, d.boxBlendNormalDistancePositive, d.boxBlendNormalDistanceNegative);
                        if (EditorGUI.EndChangeCheck())
                        {
                            //save advanced/simplified saved data
                            if (d.editorAdvancedModeEnabled.boolValue)
                            {
                                d.editorAdvancedModeBlendNormalDistancePositive.vector3Value = d.boxBlendNormalDistancePositive.vector3Value;
                                d.editorAdvancedModeBlendNormalDistanceNegative.vector3Value = d.boxBlendNormalDistanceNegative.vector3Value;
                            }
                            else
                            {
                                d.editorSimplifiedModeBlendNormalDistance.floatValue = d.boxBlendNormalDistancePositive.vector3Value.x;
                            }
                            d.Apply();
                        }
                        break;
                    case InfluenceShape.Sphere:
                        DrawSphereFadeHandle(s, d, o, sourceAsset, s.sphereInfluenceNormalHandle, d.sphereBlendNormalDistance);
                        break;
                }
            }
        }

        static void DrawBoxFadeHandle(InfluenceVolumeUI s, SerializedInfluenceVolume d, Editor o, Object sourceAsset, HierarchicalBox box, SerializedProperty positive, SerializedProperty negative)
        {
            box.center = - (positive.vector3Value - negative.vector3Value) * 0.5f;
            box.size = d.boxSize.vector3Value - positive.vector3Value - negative.vector3Value;
            box.monoHandle = !d.editorAdvancedModeEnabled.boolValue;

            EditorGUI.BeginChangeCheck();
            box.DrawHandle();
            box.DrawHull(true);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(sourceAsset, "Modified Influence Volume");

                var influenceCenter = Vector3.zero;
                var halfInfluenceSize = d.boxSize.vector3Value * .5f;

                var centerDiff = box.center - influenceCenter;
                var halfSizeDiff = halfInfluenceSize - box.size * .5f;
                var positiveNew = halfSizeDiff - centerDiff;
                var negativeNew = halfSizeDiff + centerDiff;
                var blendDistancePositive = Vector3.Max(Vector3.zero, Vector3.Min(positiveNew, halfInfluenceSize));
                var blendDistanceNegative = Vector3.Max(Vector3.zero, Vector3.Min(negativeNew, halfInfluenceSize));

                positive.vector3Value = blendDistancePositive;
                negative.vector3Value = blendDistanceNegative;

                d.Apply();
            }
        }

        static void DrawSphereHandle(InfluenceVolumeUI s, SerializedInfluenceVolume d, Editor o, Object sourceAsset,  SphereBoundsHandle sphere)
        {
            sphere.center = Vector3.zero;
            sphere.radius = d.sphereRadius.floatValue;

            EditorGUI.BeginChangeCheck();
            sphere.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(sourceAsset, "Modified Base Volume AABB");

                float radius = sphere.radius;
                d.sphereRadius.floatValue = radius;
                d.sphereBlendDistance.floatValue = Mathf.Clamp(d.sphereBlendDistance.floatValue, 0, radius);
                d.sphereBlendNormalDistance.floatValue = Mathf.Clamp(d.sphereBlendNormalDistance.floatValue, 0, radius);
                d.Apply();
            }
        }

        static void DrawSphereFadeHandle(InfluenceVolumeUI s, SerializedInfluenceVolume d, Editor o, Object sourceAsset, SphereBoundsHandle sphere, SerializedProperty radius)
        {
            sphere.center = Vector3.zero;
            sphere.radius = radius.floatValue;

            EditorGUI.BeginChangeCheck();
            sphere.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(sourceAsset, "Modified Influence volume");

                radius.floatValue = Mathf.Clamp(d.sphereRadius.floatValue - sphere.radius, 0, d.sphereRadius.floatValue);
                d.Apply();
            }
        }
    }
}
