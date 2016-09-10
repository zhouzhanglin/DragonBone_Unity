using UnityEngine;
using System.Collections;

namespace DragonBone
{
	public class DragonBoneArmature : MonoBehaviour {

		public SpriteFrame[] updateFrames;
		public SpriteMesh[] updateMeshs;

		private Animator m_animator;

		void Awake(){
			m_animator=GetComponent<Animator>();
		}

		// Use this for initialization
		void Start () {

		}

		// Update is called once per frame
		void Update () {
			if(m_animator==null || m_animator.enabled)
			{
				UpdateArmature();
			}
		}

		/// <summary>
		/// update
		/// </summary>
		public void UpdateArmature(){
			int len = updateFrames.Length;
			for(int i=0;i<len;++i){
				SpriteFrame frame = updateFrames[i];
				if(frame&&frame.isActiveAndEnabled) frame.UpdateFrame();
			}

			len = updateMeshs.Length;
			for(int i=0;i<len;++i){
				SpriteMesh mesh = updateMeshs[i];
				if(mesh&&mesh.isActiveAndEnabled) mesh.UpdateMesh();
			}
		}
	}
}
