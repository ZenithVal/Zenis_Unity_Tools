//ZenithVal 2024
//Replace .001 bones in the VRCAvatarDescriptor with the real ones.

#if VRC_SDK_VRCSDK3
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using static VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;

public class BatchVRCColliderEdits : MonoBehaviour
{
    [UnityEditor.MenuItem("CONTEXT/VRCAvatarDescriptor/Set Colliders to Real Bones")]
    public static void SetCollidersToRealBones(UnityEditor.MenuCommand command)
    {
        VRCAvatarDescriptor descriptor = (VRCAvatarDescriptor)command.context;

        ReplaceGhostBone(descriptor, ref descriptor.collider_handL);
        ReplaceGhostBone(descriptor, ref descriptor.collider_fingerIndexL);
        ReplaceGhostBone(descriptor, ref descriptor.collider_fingerMiddleL);
        ReplaceGhostBone(descriptor, ref descriptor.collider_fingerRingL);
        ReplaceGhostBone(descriptor, ref descriptor.collider_fingerLittleL);

        ReplaceGhostBone(descriptor, ref descriptor.collider_handR);
        ReplaceGhostBone(descriptor, ref descriptor.collider_fingerIndexR);
        ReplaceGhostBone(descriptor, ref descriptor.collider_fingerMiddleR);
        ReplaceGhostBone(descriptor, ref descriptor.collider_fingerRingR);
        ReplaceGhostBone(descriptor, ref descriptor.collider_fingerLittleR);
        
        ReplaceGhostBone(descriptor, ref descriptor.collider_footL);
        ReplaceGhostBone(descriptor, ref descriptor.collider_footR);
    }

    private static void ReplaceGhostBone(VRCAvatarDescriptor descriptor, ref VRCAvatarDescriptor.ColliderConfig dest)
    {
        string oldName = dest.transform.name;
        Debug.Log("ReplaceGhostBone: " + oldName);

        // If the tranform is null or does not contain .001, return
        if (!oldName.Contains(".001"))
        {
            Debug.Log(oldName + " does not contain .001, skipped");
            return;
        }

        //Old bone
        GameObject oldBone = dest.transform.gameObject;
        //Get the heirarchy path of the bone
        string oldBonePath = AnimationUtility.CalculateTransformPath(oldBone.transform, descriptor.transform);

        //New Bone
        string newBonePath = oldBonePath.Replace(".001", "");
        Transform newBone = descriptor.transform.Find(newBonePath);

        Debug.Log("Path Converted from `" + oldBonePath + "` to `" + newBonePath);

        //assign the tranform of newbone to the collider
        dest.transform = newBone.transform;
        dest.state = VRCAvatarDescriptor.ColliderConfig.State.Custom;
    }


    // Ring finger to foot bone
    [UnityEditor.MenuItem("CONTEXT/VRCAvatarDescriptor/Set Ring Fingers to Foot Bones")]
    public static void SetRingFingersToFootBones(UnityEditor.MenuCommand command)
    {
        VRCAvatarDescriptor descriptor = (VRCAvatarDescriptor)command.context;

        ReplaceBone(descriptor, ref descriptor.collider_fingerRingL, ref descriptor.collider_footL);
        ReplaceBone(descriptor, ref descriptor.collider_fingerRingR, ref descriptor.collider_footR);
    }

    private static void ReplaceBone(VRCAvatarDescriptor descriptor, ref VRCAvatarDescriptor.ColliderConfig dest, ref VRCAvatarDescriptor.ColliderConfig source)
    {
        if (source.transform == null)
        {
            Debug.Log(source.transform.name + "is null, skipped");
            return;
        }

        dest.transform = source.transform;
        dest.state = ColliderConfig.State.Custom;
        dest.radius = source.radius;
        dest.height = source.height;
        dest.position = source.position;
        dest.rotation = source.rotation;
    }
}

#endif
#endif