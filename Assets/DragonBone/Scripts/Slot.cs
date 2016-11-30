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
		public int zOrder;//default z order
		protected internal bool _zOrderValid = false;

		private int __z;
		[HideInInspector]
		[SerializeField]
		private float m_z;
		public int z{
			get { return __z; }
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
			int temp=(int) m_z;
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
			__z = zOrder;
			m_z = zOrder;

		}

	}

}