using UnityEngine;
using System.Collections;

namespace DragonBone
{
	public class DragonBoneArmature : MonoBehaviour {

		[SerializeField]
		private bool m_FlipX;
		public Transform[] slots;
		public SpriteFrame[] updateFrames;
		public SpriteMesh[] updateMeshs;
		public Renderer[] attachments;

		private Animator m_animator;
		public Animator aniamtor{
			get { return m_animator; } 
		}

		[SerializeField]
		protected string m_SortingLayerName = "Default";
		/// <summary>
		/// Name of the Renderer's sorting layer.
		/// </summary>
		public string sortingLayerName
		{
			get {
				return m_SortingLayerName;
			}
			set {
				m_SortingLayerName = value;
				foreach(Renderer r in attachments){
					if(r) {
						r.sortingLayerName = value;
						#if UNITY_EDITOR 
						UnityEditor.EditorUtility.SetDirty(r);
						#endif
						SpriteFrame sf = r.GetComponent<SpriteFrame>();
						if(sf) {
							sf.sortingLayerName = value;
							#if UNITY_EDITOR 
							UnityEditor.EditorUtility.SetDirty(sf);
							#endif
						}
						else {
							SpriteMesh sm = r.GetComponent<SpriteMesh>();
							if(sm) {
								sm.sortingLayerName = value;
								#if UNITY_EDITOR 
								UnityEditor.EditorUtility.SetDirty(sm);
								#endif
							}
						}
					}
				}
			}
		}

		[SerializeField]
		protected int m_SortingOrder = 0;
		/// <summary>
		/// Renderer's order within a sorting layer.
		/// </summary>
		public int sortingOrder
		{
			get {
				return m_SortingOrder;
			}
			set {
				m_SortingOrder = value;
				foreach(Renderer r in attachments){
					if(r){
						r.sortingOrder = value;
						#if UNITY_EDITOR 
						UnityEditor.EditorUtility.SetDirty(r);
						#endif
						SpriteFrame sf = r.GetComponent<SpriteFrame>();
						if(sf) {
							sf.soringOrder = value;
							#if UNITY_EDITOR 
							UnityEditor.EditorUtility.SetDirty(sf);
							#endif
						}
						else {
							SpriteMesh sm = r.GetComponent<SpriteMesh>();
							if(sm) {
								sm.sortingOrder = value;
								#if UNITY_EDITOR 
								UnityEditor.EditorUtility.SetDirty(sm);
								#endif
							}
						}
					}
				}
			}
		}

		public bool flipX{
			get { return m_FlipX; }
			set {
				#if !UNITY_EDITOR
				if(m_FlipX != value) return;
				#endif

				m_FlipX =  value;
				Vector3 rotate = transform.localEulerAngles;
				if(m_FlipX){
					rotate.y = 180f;
					int len = slots.Length;
					for(int i=0;i<len;++i){
						Transform slot = slots[i];
						if(slot){
							Vector3 v = slot.localPosition;
							v.z = Mathf.Abs(v.z);
							slot.localPosition = v;
							#if UNITY_EDITOR 
							UnityEditor.EditorUtility.SetDirty(slot);
							#endif
						}
					}
				}else{
					rotate.y = 0f;
					int len = slots.Length;
					for(int i=0;i<len;++i){
						Transform slot = slots[i];
						if(slot){
							Vector3 v = slot.localPosition;
							v.z = -Mathf.Abs(v.z);
							slot.localPosition = v;
							#if UNITY_EDITOR 
							UnityEditor.EditorUtility.SetDirty(slot);
							#endif
						}
					}
				}
				transform.localEulerAngles = rotate;
				#if UNITY_EDITOR 
				UnityEditor.EditorUtility.SetDirty(transform);
				#endif
			}
		}

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
