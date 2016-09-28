using UnityEngine;
using UnityEditor;

namespace DragonBone
{
	public class ModifyLayerEditor : ScriptableWizard {

		public Transform dragonBoneRoot;
		public string sortingLayerName="Default";
		public int sortingOrder = 0;

		[MenuItem("DragonBone/Modify Sorting",false,1)]
		static void CreateWizard () {
			ScriptableWizard.DisplayWizard<ModifyLayerEditor>("Modify Sorting Layer", "OK");
		}

		void Awake(){
			dragonBoneRoot = Selection.activeTransform;
		}

		public void OnWizardCreate(){
			if(dragonBoneRoot){
				foreach(Renderer render in dragonBoneRoot.GetComponentsInChildren<Renderer>(true)){
					render.sortingLayerName = sortingLayerName;
					render.sortingOrder = sortingOrder;
					SpriteFrame sf = render.GetComponent<SpriteFrame>();
					if(sf){
						sf.sortingLayerName = sortingLayerName;
						sf.soringOrder = sortingOrder;
						EditorUtility.SetDirty(sf);
					}
					else
					{
						SpriteMesh sm = render.GetComponent<SpriteMesh>();
						if(sm){
							sm.sortingLayerName = sortingLayerName;
							sm.sortingOrder = sortingOrder;
							EditorUtility.SetDirty(sm);
						}
					}
				}
			}
		}
	}
}
