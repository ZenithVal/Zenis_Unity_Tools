//Zenithval 2024
//Very specific niche tool for adding .001 bones as constraint sources to originals

using UnityEngine;
using UnityEditor;
using UnityEngine.Animations;
using VRC.Dynamics;

public class AssignConstraintSourcesToFakes : MonoBehaviour
{
    [MenuItem("CONTEXT/RotationConstraint/Add .001 Variant as source")]
    [MenuItem("CONTEXT/ParentConstraint/Add .001 Variant as source")]
    [MenuItem("CONTEXT/PositionConstraint/Add .001 Variant as source")]
    public static void AssignConstraintSources(MenuCommand command)
    {
        //Get the gameobject the constraint is on
        GameObject gameObject = null;

        if (command.context is RotationConstraint)
        {
            gameObject = ((RotationConstraint)command.context).gameObject;
        }
        else if (command.context is ParentConstraint)
        {
            gameObject = ((ParentConstraint)command.context).gameObject;
        }
        else if (command.context is PositionConstraint)
        {
            gameObject = ((PositionConstraint)command.context).gameObject;
        }
        else
        {
            Debug.LogError("Invalid constraint type");
            return;
        }


        GameObject armature = GetArmatureOfGameObject(gameObject);

        if (armature == null)
        {
            Debug.LogError("No armature found");
            return;
        }

        //add .001 to the gameObject name and find the corresponding fake
        string fakeName = gameObject.name + ".001";

        if (fakeName == null)
        {
            Debug.LogError("No fake found");
            return;
        }

        //check children of the armature for the fake
        GameObject fake = FindGameObjectInArmature(armature, fakeName);

        if (fake == null)
        {
            Debug.LogError("No fake found");
            return;
        }

        //make a new source
        ConstraintSource source = new ConstraintSource();
        source.sourceTransform = fake.transform;
        source.weight = 1.0f;

        if (command.context is RotationConstraint)
        {
            RotationConstraint constraint = (RotationConstraint)command.context;
            constraint.AddSource(source);
        }
        else if (command.context is ParentConstraint)
        {
            ParentConstraint constraint = (ParentConstraint)command.context;
            constraint.AddSource(source);
        }
        else if (command.context is PositionConstraint)
        {
            PositionConstraint constraint = (PositionConstraint)command.context;
            constraint.AddSource(source);
        }
    }

    private static GameObject FindGameObjectInArmature(GameObject armature, string boneToFind)
    {
        Transform[] allChildren = armature.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.name == boneToFind)
            {
                return child.gameObject;
            }
        }

        return null;
    }

    private static GameObject GetArmatureOfGameObject(GameObject gameObject)
    {
        Transform parent = gameObject.transform.parent;
        while (parent != null)
        {
            if (parent.name == "Armature")
            {
                return parent.gameObject;
            }
            parent = parent.parent;
        }
        return null;
    }
}


