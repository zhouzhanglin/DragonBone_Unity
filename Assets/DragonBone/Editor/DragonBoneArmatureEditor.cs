using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;

namespace DragonBone
{
	[CustomEditor(typeof(DragonBoneArmature))]
	public class DragonBoneArmatureEditor : Editor {

		string[] sortingLayerNames;
		int selectedOption;
		bool flipX;

		void OnEnable(){
			DragonBoneArmature armature = target as DragonBoneArmature;
			if(armature==null) return;
			sortingLayerNames = GetSortingLayerNames();
			selectedOption = GetSortingLayerIndex(armature.sortingLayerName);
			flipX = armature.flipX;
		}

		public override void OnInspectorGUI(){
			DragonBoneArmature armature = target as DragonBoneArmature;
			if(armature==null) return;

			selectedOption = EditorGUILayout.Popup("Sorting Layer", selectedOption, sortingLayerNames);
			if (sortingLayerNames[selectedOption] != armature.sortingLayerName)
			{
				Undo.RecordObject(armature, "Sorting Layer");
				armature.sortingLayerName = sortingLayerNames[selectedOption];
				EditorUtility.SetDirty(armature);
			}
			int newSortingLayerOrder = EditorGUILayout.IntField("Order in Layer", armature.sortingOrder);
			if (newSortingLayerOrder != armature.sortingOrder)
			{
				Undo.RecordObject(armature, "Edit Sorting Order");
				armature.sortingOrder = newSortingLayerOrder;
				EditorUtility.SetDirty(armature);
			}
			if(GUILayout.Button("Update All Order",GUILayout.Height(24))){
				foreach(Renderer render in armature.GetComponentsInChildren<Renderer>()){
					render.sortingLayerName = armature.sortingLayerName;
					render.sortingOrder = armature.sortingOrder;
					EditorUtility.SetDirty(render);

					SpriteFrame sf = render.GetComponent<SpriteFrame>();
					if(sf) {
						sf.sortingLayerName = armature.sortingLayerName;
						sf.soringOrder = armature.sortingOrder;
					}
					else {
						SpriteMesh sm = render.GetComponent<SpriteMesh>();
						if(sm) {
							sm.sortingLayerName = armature.sortingLayerName;
							sm.sortingOrder = armature.sortingOrder;
						}
					}
				}
			}

			serializedObject.Update();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FlipX"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("slots"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("updateFrames"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("updateMeshs"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("attachments"), true);
			serializedObject.ApplyModifiedProperties();

			if(armature.flipX!=flipX){
				armature.flipX = armature.flipX;
				flipX = armature.flipX;
			}
		}

		public string[] GetSortingLayerNames() {
			System.Type internalEditorUtilityType = typeof(InternalEditorUtility);
			PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
			return (string[])sortingLayersProperty.GetValue(null, new object[0]);
		}
		public int[] GetSortingLayerUniqueIDs()
		{
			System.Type internalEditorUtilityType = typeof(InternalEditorUtility);
			PropertyInfo sortingLayerUniqueIDsProperty = internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);
			return (int[])sortingLayerUniqueIDsProperty.GetValue(null, new object[0]);
		}
		int GetSortingLayerIndex(string layerName){
			for(int i = 0; i < sortingLayerNames.Length; ++i){  
				if(sortingLayerNames[i] == layerName) return i;  
			}  
			return 0;
		}
	}
}