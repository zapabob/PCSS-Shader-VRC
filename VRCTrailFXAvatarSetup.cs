using System.Collections;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

public class VRCTrailFXAvatarSetup : Editor
{
    [MenuItem("VRCTrailFX/Setup Avatar")]
    public static void SetupAvatar()
    {
        GameObject avatar = Selection.activeGameObject;
        if (avatar == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select an avatar GameObject.", "OK");
            return;
        }

        VRCAvatarDescriptor descriptor = avatar.GetComponent<VRCAvatarDescriptor>();
        if (descriptor == null)
        {
            EditorUtility.DisplayDialog("Error", "Selected GameObject is not a valid avatar.", "OK");
            return;
        }

        VRCExpressionParameters expressionParameters = descriptor.expressionParameters;
        if (expressionParameters == null)
        {
            expressionParameters = CreateInstance<VRCExpressionParameters>();
            AssetDatabase.CreateAsset(expressionParameters, "Assets/VRCTrailFX/ExpressionParameters.asset");
            descriptor.expressionParameters = expressionParameters;
        }

        GameObject trailFXPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VRCTrailFX/VRCTrailFXPrefab.prefab");
        if (trailFXPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "VRCTrailFXPrefab not found. Please create the prefab first.", "OK");
            return;
        }

        GameObject trailFXInstance = PrefabUtility.InstantiatePrefab(trailFXPrefab) as GameObject;
        trailFXInstance.transform.SetParent(avatar.transform, false);

        VRCTrailFX trailFX = trailFXInstance.GetComponent<VRCTrailFX>();
        trailFX.expressionParameters = expressionParameters;
        trailFX.avatarDescriptor = descriptor;
    }
}