using UnityEditor;
using Zenject;

namespace Zenject
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CompositeScriptableObjectInstaller))]
    [NoReflectionBaking]
    public class CompositeScriptableObjectInstallerEditor : BaseCompositetInstallerEditor<CompositeScriptableObjectInstaller, ScriptableObjectInstallerBase>
    {
    }
}