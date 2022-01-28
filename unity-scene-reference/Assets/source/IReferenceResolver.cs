using System;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BAStudio.UnitySceneReference
{
    public interface IReferenceSceneLoader
    {
        void Load(SceneReference reference, LoadSceneMode mode);
    }

    public class DefaultReferenceResolver : IReferenceSceneLoader
    {
        public void Load(SceneReference reference, LoadSceneMode mode)
        {
            switch (reference.provider)
            {
                case ProviderType.InBuild:
                {
                    SceneManager.LoadSceneAsync(reference.key, mode);
                    break;
                }
                case ProviderType.AssetBundle:
                {
                    AssetBundleInfo
                    break;
                }
                case ProviderType.Addressables:
                {
                    break;
                }
            }
        }
    }

    public class SceneReferenceLoadHandle
    {
        SceneReference Referencee { get; }
        public bool IsDone { get; }
        public float Progress { get; }
        public int Priority { get; set; }
        public bool AllowSceneActivation { get; set; }
        public event Action<SceneReferenceLoadHandle> Completed;
        AsyncOperation InBuildOp;
    }
}