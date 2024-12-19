//ZenithVal 2024
//Some various functions for unity constraints

//Parent Constraint Splitter
//Splits a parent constraint into a rotation and position constraint

//AssignConstraintSourcesToFakes
//Very specific niche tool for adding .001 bones as constraint sources to originals

#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

public class ConstraintTools : MonoBehaviour
{
    #region Parent Constraint Splitter
    [MenuItem("CONTEXT/ParentConstraint/Split into Position and Rotation")]
    public static void SplitParentConstraint(MenuCommand command)
    {
        Undo.SetCurrentGroupName($"Split Parent Constraint of {command.context.name}");
        int undoGroup = Undo.GetCurrentGroup();

        try
        {
            ParentConstraint parentConstraint = (ParentConstraint)command.context;
            GameObject parent = parentConstraint.gameObject;

            if (parent.GetComponent<RotationConstraint>() != null || parent.GetComponent<PositionConstraint>() != null)
            {
                Debug.LogWarning("Rotation or Position Constraint already exists on this object");
                return;
            }

            Undo.AddComponent<RotationConstraint>(parent);
            Undo.AddComponent<PositionConstraint>(parent);

            RotationConstraint rotationConstraint = parent.GetComponent<RotationConstraint>();
            PositionConstraint positionConstraint = parent.GetComponent<PositionConstraint>();

            parentConstraint.constraintActive = false;
            rotationConstraint.constraintActive = false;
            positionConstraint.constraintActive = false;

            int sourceCount = parentConstraint.sourceCount;

            for (int i = 0; i < sourceCount; i++)
            {
                ConstraintSource source = parentConstraint.GetSource(i);

                if (source.sourceTransform != null)
                {
                    rotationConstraint.AddSource(new ConstraintSource {
                        sourceTransform = source.sourceTransform,
                        weight = source.weight
                    });

                    positionConstraint.AddSource(new ConstraintSource {
                        sourceTransform = source.sourceTransform,
                        weight = source.weight
                    });
                }
            }

            Undo.DestroyObjectImmediate(parentConstraint);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to split parent constraint: " + e.Message);
        }
        finally
        {
            Undo.CollapseUndoOperations(undoGroup);
        }
    }
    #endregion

    #region Convert Position/Rotation constraint to Parent
    [MenuItem("CONTEXT/RotationConstraint/Convert to Parent Constraint")]
    [MenuItem("CONTEXT/PositionConstraint/Convert to Parent Constraint")]
    public static void ConvertToParentConstraint(MenuCommand command)
    {
        Undo.SetCurrentGroupName($"Convert {command.context.GetType().Name} to Parent Constraint of {command.context.name}");
        int undoGroup = Undo.GetCurrentGroup();

        try
        {
            ParentConstraint parentConstraint = Undo.AddComponent<ParentConstraint>(((Component)command.context).gameObject);

            int sourceCount = 0;
            if (command.context is RotationConstraint)
            {
                RotationConstraint constraint = (RotationConstraint)command.context;
                sourceCount = constraint.sourceCount;
                for (int i = 0; i < sourceCount; i++)
                {
                    ConstraintSource source = constraint.GetSource(i);
                    parentConstraint.AddSource(new ConstraintSource {
                        sourceTransform = source.sourceTransform,
                        weight = source.weight
                    });
                }

            }
            else if (command.context is PositionConstraint)
            {
                PositionConstraint constraint = (PositionConstraint)command.context;
                sourceCount = constraint.sourceCount;
                for (int i = 0; i < sourceCount; i++)
                {
                    ConstraintSource source = constraint.GetSource(i);
                    parentConstraint.AddSource(new ConstraintSource {
                        sourceTransform = source.sourceTransform,
                        weight = source.weight
                    });
                }
            }

            Undo.DestroyObjectImmediate((Component)command.context);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to convert constraint to parent constraint: " + e.Message);
        }
        finally
        {
            Undo.CollapseUndoOperations(undoGroup);
        }
    }

    #endregion


    #region AssignConstraintSourcesToFakes
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
    #endregion
}

#endif