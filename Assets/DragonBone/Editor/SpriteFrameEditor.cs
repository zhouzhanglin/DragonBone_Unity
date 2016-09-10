using UnityEngine;
using System.Collections;
using UnityEditor;

namespace DragonBone
{
	/// <summary>
	/// Sprite frame editor.
	/// author:  bingheliefeng
	/// </summary>
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SpriteFrame))]
	public class SpriteFrameEditor : Editor {

		private int m_frameIndex;
		private int m_pivotIndex;
		private string[] posList;

		void OnEnable () {
			posList = new string[]{
				"_","CENTER","TOP","TOP_LEFT","TOP_RIGHT",
				"LEFT","RIGHT","BOTTOM","BOTTOM_LEFT","BOTTOM_RIGHT"
			};


			SpriteFrame sprite = target as SpriteFrame;
			if(sprite && sprite.frames!=null && sprite.frames.Length>0){
				if(!string.IsNullOrEmpty(sprite.frameName)){
					for(int i=0;i<sprite.frames.Length;++i){
						if(sprite.frameName == sprite.frames[i].name){
							m_frameIndex = i;
							break;
						}
					}
				}
			}
		}

		public override void OnInspectorGUI(){
			SpriteFrame sprite = target as SpriteFrame;

			serializedObject.Update();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("atlasText"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("atlasMat"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("textureScale"), true);
			EditorGUILayout.BeginVertical();

			if(sprite.frames!=null && sprite.frames.Length>0){
				//显示frameName列表
				string[] list = new string[sprite.frames.Length];
				for(int i=0;i<sprite.frames.Length;++i){
					list[i] = sprite.frames[i].name;
				}
				int selectIndex = EditorGUILayout.Popup("Frame",m_frameIndex,list);
				if(selectIndex!=m_frameIndex){
					m_frameIndex = selectIndex;
					sprite.CreateQuad();
					sprite.frameName = sprite.frames[m_frameIndex].name;
					UpdatePivot(sprite,m_pivotIndex);
				}
			}
			bool create = (sprite.frames==null || sprite.frames.Length==0);
			string btnString = create?"Create SpriteFrame" : "Update SpriteFrame";
			if(GUILayout.Button(btnString)){
				sprite.ParseAtlasText();
				if(sprite.atlasText!=null && sprite.atlasMat!=null && sprite.atlasMat.mainTexture!=null)
				{
					sprite.ParseAtlasText();
					if(sprite.frames.Length>0){
						sprite.CreateQuad();
						if(create){
							sprite.frameName = sprite.frames[0].name;
							sprite.pivot = new Vector2(0.5f,0.5f);
						}else {
							sprite.frameName = sprite.frameName;
						}
					}
				}
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_textureSize"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_uvOffset"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_pivot"), true);
			if(sprite.frames!=null && !string.IsNullOrEmpty(sprite.frameName)){
				//			"None","CENTER","TOP","TOP_LEFT","TOP_RIGHT",
				//			"LEFT","RIGHT","BOTTOM","BOTTOM_LEFT","BOTTOM_RIGHT"
				int selectPivot = EditorGUILayout.Popup("Auto Pivot",m_pivotIndex,posList);
				if(selectPivot!=m_pivotIndex){
					UpdatePivot(sprite,selectPivot);
					sprite.frameName = sprite.frameName;
				}
			}
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_color"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_brightness"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_sortingLayerName"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_soringOrder"), true);
			serializedObject.ApplyModifiedProperties();
		}


		void UpdatePivot( SpriteFrame sprite, int selectPivot){
			m_pivotIndex = selectPivot;
			sprite.CreateQuad();
			switch(selectPivot){
			case 1:
				sprite.pivot = new Vector2(0.5f,0.5f);
				break;
			case 2:
				sprite.pivot = new Vector2(0.5f,1f);
				break;
			case 3:
				sprite.pivot = new Vector2(0f,1f);
				break;
			case 4:
				sprite.pivot = new Vector2(1f,1f);
				break;
			case 5:
				sprite.pivot = new Vector2(0f,0.5f);
				break;
			case 6:
				sprite.pivot = new Vector2(1f,0.5f);
				break;
			case 7:
				sprite.pivot = new Vector2(0.5f,0f);
				break;
			case 8:
				sprite.pivot = new Vector2(0f,0f);
				break;
			case 9:
				sprite.pivot = new Vector2(1f,0f);
				break;
			}
		}

		[MenuItem("DragonBone/Create SpriteFrame",false,1)]
		static void CreateSpriteFrame(){
			GameObject go = new GameObject("SpriteFrame");
			if(Selection.activeTransform){
				go.transform.parent = Selection.activeTransform;
			}
			go.transform.localScale = Vector3.one;
			go.AddComponent<SpriteFrame>();
		}

	}

}