using UnityEngine;
using System.Collections;

namespace DragonBone
{
	public class Slot : MonoBehaviour {

		public enum BlendMode{
			Normal,
			Add,
			Erase,
			Multiply,
			Screen,
			Mask,
			Below,
			None,
		}

		[HideInInspector]
		public DragonBoneArmature armature;

		[HideInInspector]
		public int zOrder=0;//default z order

		public Color color=Color.white;
		private SpriteFrame m_SpriteFrame = null;
		private SpriteMesh m_SpriteMesh = null;
		private SpriteRenderer m_SpriteRenderer = null;

		[HideInInspector]
		[SerializeField]
		private int __z=0;
		[HideInInspector]
		[SerializeField]
		private float m_z=0;
		public int z{
			get { return __z; }
			set {
				__z = value;
				m_z = value;
			}
		}

		[HideInInspector]
		[SerializeField]
		protected int __displayIndex;
		[SerializeField]
		private float m_DisplayIndex;
		public int displayIndex{
			get {
				return __displayIndex;
			}
			set
			{
				m_DisplayIndex = value;
				__displayIndex = value;
				for(int i=0;i<transform.childCount;++i){
					if(value>-1 && i==value) transform.GetChild(i).gameObject.SetActive(true);
					else transform.GetChild(i).gameObject.SetActive(false);
				}
			}
		}

		[SerializeField]
		private BlendMode m_blendMode = BlendMode.Normal;
		public BlendMode blendMode{
			get { return m_blendMode; }
			set {
				m_blendMode = value;
				if(!Application.isPlaying) return;

				Renderer[] renders = GetComponentsInChildren<Renderer>(true);
				for(int i=0;i<renders.Length;++i){
					Renderer r = renders[i];
					if(m_blendMode!=BlendMode.Normal){
						if(r && r.sharedMaterial){
							r.material.SetFloat("_BlendSrc",GetSrcFactor());
							r.material.SetFloat("_BlendDst",GetDstFactor());
						}
					}else if(r && armature && r.sharedMaterial){
						int last = r.sharedMaterial.name.LastIndexOf(" (Instance)");
						if(last>0){
							string matName = r.sharedMaterial.name.Substring(0,last);
							foreach(Material mat in armature.materials){
								if(matName.Equals(mat)){
									r.sharedMaterial = mat;
									break;
								}
							}
						}
					}
				}
			}
		}

		public void UpdateSlot(){
			if(transform.childCount>0){
				int tempIndex = Mathf.RoundToInt(m_DisplayIndex);
				if(Mathf.Abs(m_DisplayIndex-tempIndex)<0.0001f){
					if(tempIndex!=__displayIndex){
						if(__displayIndex>-1) transform.GetChild(__displayIndex).gameObject.SetActive(false);
						if(tempIndex>-1) transform.GetChild(tempIndex).gameObject.SetActive(true);
						__displayIndex = tempIndex;
						UpdateCurrentDisplay();
					}
				}
			}
			#if UNITY_EDITOR
			if(!Application.isPlaying) UpdateCurrentDisplay();
			#endif
			UpdateDisplayColor();

			int temp=Mathf.RoundToInt( m_z);
			if(Mathf.Abs(m_z-temp)>0.0001f) return;
			if(temp!=__z){
				__z = temp;
				armature.UpdateSlotZOrder(this);
			}
		}

		private int GetSrcFactor(){
			if(m_blendMode== BlendMode.Normal){
				return (int)UnityEngine.Rendering.BlendMode.SrcAlpha;
			}
			if(m_blendMode== BlendMode.Add){
				return (int)UnityEngine.Rendering.BlendMode.DstColor;
			}
			if(m_blendMode== BlendMode.Erase){
				return (int)UnityEngine.Rendering.BlendMode.Zero;
			}
			if(m_blendMode== BlendMode.Multiply){
				return (int)UnityEngine.Rendering.BlendMode.DstColor;
			}
			if(m_blendMode== BlendMode.Screen){
				return (int)UnityEngine.Rendering.BlendMode.SrcAlpha;
			}
			if(m_blendMode== BlendMode.Mask){
				return (int)UnityEngine.Rendering.BlendMode.Zero;
			}
			if(m_blendMode== BlendMode.Below){
				return (int)UnityEngine.Rendering.BlendMode.OneMinusDstAlpha;
			}
			if(m_blendMode== BlendMode.None){
				return (int)UnityEngine.Rendering.BlendMode.One;
			}
			return (int)UnityEngine.Rendering.BlendMode.SrcAlpha;
		}

		private int GetDstFactor(){
			if(m_blendMode== BlendMode.Normal){
				return (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
			}	
			if(m_blendMode== BlendMode.Add){
				return (int)UnityEngine.Rendering.BlendMode.DstAlpha;
			}
			if(m_blendMode== BlendMode.Erase){
				return (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
			}
			if(m_blendMode== BlendMode.Multiply){
				return (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
			}
			if(m_blendMode== BlendMode.Screen){
				return (int)UnityEngine.Rendering.BlendMode.One;
			}
			if(m_blendMode== BlendMode.Mask){
				return (int)UnityEngine.Rendering.BlendMode.SrcAlpha;
			}
			if(m_blendMode== BlendMode.Below){
				return (int)UnityEngine.Rendering.BlendMode.DstAlpha;
			}
			if(m_blendMode== BlendMode.None){
				return (int)UnityEngine.Rendering.BlendMode.Zero;
			}
			return (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
		}

		public BlendMode ConvertBlendMode(string blendMode){
			if(blendMode.Equals("add")||blendMode.Equals("additive")){
				return BlendMode.Add;
			}
			if(blendMode.Equals("erase")){
				return BlendMode.Erase;
			}
			if(blendMode.Equals("screen")){
				return BlendMode.Screen;
			}
			if(blendMode.Equals("mask")){
				return BlendMode.Mask;
			}
			if(blendMode.Equals("multiply")){
				return BlendMode.Multiply;
			}
			if(blendMode.Equals("below")){
				return BlendMode.Below;
			}
			if(blendMode.Equals("none")){
				return BlendMode.None;
			}
			return BlendMode.Normal;
		}

		void Start(){
			blendMode = m_blendMode;
			__displayIndex = Mathf.RoundToInt(m_DisplayIndex);
			__z = 0;
			UpdateCurrentDisplay();
			UpdateDisplayColor();
		}

		void UpdateCurrentDisplay(){
			if(__displayIndex>-1 && transform.childCount>0){
				m_SpriteFrame = null;
				m_SpriteMesh = null;
				m_SpriteRenderer = null;
				Transform child = transform.GetChild(__displayIndex);
				m_SpriteFrame = child.GetComponent<SpriteFrame>();
				m_SpriteMesh = child.GetComponent<SpriteMesh>();
				m_SpriteRenderer = child.GetComponent<SpriteRenderer>();
			}
		}

		void UpdateDisplayColor(){
			if(m_SpriteFrame){
				m_SpriteFrame.color = color;
			}else if(m_SpriteMesh){
				m_SpriteMesh.color = color;
				m_SpriteMesh.UpdateMesh();
			}else if(m_SpriteRenderer){
				m_SpriteRenderer.color = color;
			}
		}
	}

}