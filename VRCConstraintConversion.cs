//Zenithval 2024
//Simple additions to VRC constraint conversion via AvatarDynamicSetups's public API.

#if VRC_SDK_VRCSDK3
#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Animations;
using VRC.SDK3.Avatars;

public class VRCConstraintConversion : MonoBehaviour
{
    [MenuItem("VRChat SDK/Constraints/Convert Constraints on Selected Objects ")]
    private static void ConvertSelectedGameObjects()
    {
        ConvertConstraintGameObjects(false);
    }

    [MenuItem("VRChat SDK/Constraints/Convert Constraints on Selected Objects + Children")]
    private static void ConvertSelectedGameObjectsAndChildren()
    {
        ConvertConstraintGameObjects(true);
    }


    private static void ConvertConstraintGameObjects(bool includeChildObjects)
    {
        var TargetObjects = new List<GameObject>();
        foreach (var obj in Selection.objects)
        {
            if (obj is GameObject)
            {
                TargetObjects.Add(obj as GameObject);
            }
        }

        if (TargetObjects.Count == 0)
        {
            Debug.LogWarning("Manual VRC Constraint Conversion: No GameObjects selected");
            return;
        }

        IConstraint[] unityConstraints = GetUnityConstraintsSelected(TargetObjects, includeChildObjects);

        string targetName = TargetObjects.Count == 1 ? TargetObjects[0].name : "Multiple Objects";
        ExecuteConstraintConversion(unityConstraints, targetName);
    }

    private static IConstraint[] GetUnityConstraintsSelected(List<GameObject> targetObjects, bool includeChildObjects)
    {
        List<IConstraint> unityConstraints = new List<IConstraint>();

        foreach (var targetObject in targetObjects)
        {
            if (includeChildObjects)
            {
                unityConstraints.AddRange(targetObject.GetComponentsInChildren<IConstraint>());
            }
            else
            {
                unityConstraints.AddRange(targetObject.GetComponents<IConstraint>());
            }
        }

        return unityConstraints.ToArray();
    }

    private static void ExecuteConstraintConversion(IConstraint[] unityConstraints, string target)
    {
        Undo.SetCurrentGroupName($"Manual Convert Constraints of ({target})");
        int undoGroup = Undo.GetCurrentGroup();

        try
        {
            AvatarDynamicsSetup.DoConvertUnityConstraints(unityConstraints, null, false);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Manual VRC Constraint Conversion: {ex.Message}");
        }
        finally
        {
            Undo.CollapseUndoOperations(undoGroup);
        }
    }

    [MenuItem("VRChat SDK/Constraints/Convert Constraints in Selected Animation Clips")]
    private static void ConvertSelectedAnimationClips()
    {
        var targetAnimationClips = new List<AnimationClip>();

        foreach (var obj in Selection.objects)
        {
            if (obj is AnimationClip)
            {
                targetAnimationClips.Add(obj as AnimationClip);
            }
        }

        if (targetAnimationClips.Count == 0)
        {
            Debug.LogWarning("Manual VRC Constraint Conversion: No Animation Clips selected");
            return;
        }


        foreach (var clip in targetAnimationClips)
        {
            try
            {
                AvatarDynamicsSetup.RebindConstraintAnimationClip(clip);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Manual VRC Constraint Conversion (Anim): {ex.Message}");
            }
        }
    }

    [MenuItem("CONTEXT/AnimationClip/Convert Constraints")]
    private static void ConvertAnimationClip(MenuCommand command)
    {
        var clip = command.context as AnimationClip;

        if (clip == null)
        {
            Debug.LogWarning($"Manual VRC Constraint Conversion (Anim): {command.context.name} is not an AnimationClip? How");
            return;
        }

        try
        {
            AvatarDynamicsSetup.RebindConstraintAnimationClip(clip);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Manual VRC Constraint Conversion (Anim): {ex.Message}");
        }
    }
}

#endif
#endif