using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Zenject;

namespace Zenject
{
    public abstract class BaseCompositetInstallerEditor<T, TLeaf> : Editor
        where T : UnityEngine.Object, ICompositeInstaller<TLeaf>
        where TLeaf : UnityEngine.Object, IInstaller
    {
        List<ReorderableList> _installersLists;

        protected virtual void OnEnable()
        {
            _installersLists = new List<ReorderableList>
            {
                CreateInstallerList(),
            };
        }

        private ReorderableList CreateInstallerList()
        {
            var installersProperty = serializedObject.FindProperty(PropertyInfo.name);

            ReorderableList installersList = new ReorderableList(serializedObject, installersProperty, true, true, true, true);

            var closedName = PropertyInfo.displayName;
            var closedDesc = PropertyInfo.description;

            var parentInstaller = this.target as T;

            installersList.drawHeaderCallback += rect =>
            {
                GUI.Label(rect,
                new GUIContent(closedName, closedDesc));
            };
            installersList.drawElementCallback += (rect, index, active, focused) =>
            {
                var installerProperty = installersProperty.GetArrayElementAtIndex(index);
                var leafInstaller = installerProperty.objectReferenceValue as TLeaf;

                bool isValid = leafInstaller.ValidateAsComposite(parentInstaller);

                if (!isValid) { GUI.color = Color.red; }

                rect.width -= 40;
                rect.x += 20;
                EditorGUI.PropertyField(rect, installerProperty, new GUIContent("", closedDesc), true);
                if (!isValid) { EditorGUI.LabelField(rect, new GUIContent("", CompositeInstallerEditorDescriptions.ErrorTooltip)); }

                GUI.color = Color.white;
            };

            return installersList;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            OnGui();

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnGui()
        {
            if (Application.isPlaying)
            {
                GUI.enabled = false;
            }

            foreach (var list in _installersLists)
            {
                list.DoLayoutList();
            }

            GUI.enabled = true;
        }

        protected virtual InstallerPropertyInfo PropertyInfo => new InstallerPropertyInfo
        {
            name = "_leafInstallers",
            displayName = "Leaf Scriptable Object Installers",
            description = "Drag any assets in your Project that implement ScriptableObjectInstaller here",
        };
    }
}