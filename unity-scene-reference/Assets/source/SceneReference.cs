using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BAStudio.UnitySceneReference
{

    // Author: JohannesMP (2018-08-12)
    //
    // A wrapper that provides the means to safely serialize Scene Asset References.
    //
    // Internally we serialize an Object to the SceneAsset which only exists at editor time.
    // Any time the object is serialized, we store the path provided by this Asset (assuming it was valid).
    //
    // This means that, come build time, the string path of the scene asset is always already stored, which if 
    // the scene was added to the build settings means it can be loaded.
    //
    // It is up to the user to ensure the scene exists in the build settings so it is loadable at runtime.
    // To help with this, a custom PropertyDrawer displays the scene build settings state.
    //
    //  Known issues:
    // - When reverting back to a prefab which has the asset stored as null, Unity will show the property 
    // as modified despite having just reverted. This only happens on the fist time, and reverting again fix it. 
    // Under the hood the state is still always valid and serialized correctly regardless.


    /// <summary>
    /// A wrapper that provides the means to safely serialize Scene Asset References.
    /// </summary>
    [Serializable]
    public class SceneReference : ISerializationCallbackReceiver
    {
        public ProviderType provider;
        public string key;

    #if UNITY_EDITOR
        // What we use in editor to select the scene
        [SerializeField] private Object sceneAsset;
        private bool IsValidSceneAsset
        {
            get
            {
                if (!sceneAsset) return false;

                return sceneAsset is UnityEditor.SceneAsset;
            }
        }

        // What we use in editor to select the scene
        [SerializeField] private AssetReference sceneRef;
        private bool IsValidSceneAddressable
        {
            get
            {
                if (sceneRef == null ||　sceneRef.editorAsset == null) return false;

                return sceneRef.Asset is UnityEditor.SceneAsset;
            }
        }
    #endif

        // Use this when you want to actually have the scene path
        public string Key
        {
            get
            {
    #if UNITY_EDITOR
                // In editor we always use the asset's path
                return GetScenePathFromAsset();
    #else
                // At runtime we rely on the stored path value which we assume was serialized correctly at build time.
                // See OnBeforeSerialize and OnAfterDeserialize
                return key;
    #endif
            }
            set
            {
                key = value;
    #if UNITY_EDITOR
                sceneAsset = GetSceneAssetFromPath();
    #endif
            }
        }

        public static implicit operator string(SceneReference sceneReference)
        {
            return sceneReference.Key;
        }

        // Called to prepare this data for serialization. Stubbed out when not in editor.
        public void OnBeforeSerialize()
        {
    #if UNITY_EDITOR
            HandleBeforeSerialize();
    #endif
        }

        // Called to set up data for deserialization. Stubbed out when not in editor.
        public void OnAfterDeserialize()
        {
    #if UNITY_EDITOR
            // We sadly cannot touch assetdatabase during serialization, so defer by a bit.
            EditorApplication.update += HandleAfterDeserialize;
    #endif
        }



    #if UNITY_EDITOR
        private SceneAsset GetSceneAssetFromPath()
        {
            return string.IsNullOrEmpty(key) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(key);
        }

        private string GetScenePathFromAsset()
        {
            return sceneAsset == null ? string.Empty : AssetDatabase.GetAssetPath(sceneAsset);
        }

        private void HandleBeforeSerialize()
        {
            // Asset is invalid but have Path to try and recover from
            if (!isAddressable && IsValidSceneAsset == false && string.IsNullOrEmpty(key) == false)
            {
                sceneAsset = GetSceneAssetFromPath();
                if (sceneAsset == null) key = string.Empty;

                EditorSceneManager.MarkAllScenesDirty();
            }
            // Asset takes precendence and overwrites Path
            else
            {
                if (!isAddressable) key = GetScenePathFromAsset();
            }
        }

        private void HandleAfterDeserialize()
        {
            EditorApplication.update -= HandleAfterDeserialize;
            // Asset is valid, don't do anything - Path will always be set based on it when it matters
            if (IsValidSceneAsset) return;

            // Asset is invalid but have path to try and recover from
            if (string.IsNullOrEmpty(key)) return;

            sceneAsset = GetSceneAssetFromPath();
            // No asset found, path was invalid. Make sure we don't carry over the old invalid path
            if (!sceneAsset) key = string.Empty;

            if (!Application.isPlaying) EditorSceneManager.MarkAllScenesDirty();
        }
    #endif
    }

}