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
		bool flipX,flipY;
		float zspace;

		void OnEnable(){
			DragonBoneArmature armature = target as DragonBoneArmature;
			if(armature==null) return;
			sortingLayerNames = GetSortingLayerNames();
			selectedOption = GetSortingLayerIndex(armature.sortingLayerName);
			flipX = armature.flipX;
			flipY = armature.flipY;
			zspace = armature.zSpace;
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
			if(GUILayout.Button("Update All Sorting Order",GUILayout.Height(20))){
				foreach(Renderer render in armature.GetComponentsInChildren<Renderer>(true)){
					render.sortingLayerName = armature.sortingLayerName;
					render.sortingOrder = armature.sortingOrder;
					EditorUtility.SetDirty(render);

					SpriteFrame sf = render.GetComponent<SpriteFrame>();
					if(sf) {
						sf.sortingLayerName = armature.sortingLayerName;
						sf.sortingOrder = armature.sortingOrder;
					}
					else {
						SpriteMesh sm = render.GetComponent<SpriteMesh>();
						if(sm) {
							sm.sortingLayerName = armature.sortingLayerName;
							sm.sortingOrder = armature.sortingOrder;
						}
					}
				}
				EditorUtility.SetDirty(armature);
				if (!string.IsNullOrEmpty(armature.gameObject.scene.name)){
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
				}
			}

			serializedObject.Update();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FlipX"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FlipY"), true);
			if(!Application.isPlaying){
				EditorGUILayout.PropertyField(serializedObject.FindProperty("zSpace"), true);
			}
			EditorGUILayout.PropertyField(serializedObject.FindProperty("poseData"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("slots"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("bones"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("attachments"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("materials"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("textureFrames"), true);
			serializedObject.ApplyModifiedProperties();

			if(!Application.isPlaying){
				if(armature.flipX!=flipX){
					armature.flipX = armature.flipX;
					flipX = armature.flipX;
					if (!string.IsNullOrEmpty(armature.gameObject.scene.name)){
						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
					}
				}
				if(armature.flipY!=flipY){
					armature.flipY = armature.flipY;
					flipY = armature.flipY;
					if (!string.IsNullOrEmpty(armature.gameObject.scene.name)){
						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
					}
				}
				if(armature.zSpace!=zspace){
					zspace = armature.zSpace;
					armature.ResetSlotZOrder();
					if (!string.IsNullOrEmpty(armature.gameObject.scene.name)){
						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
					}
				}
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