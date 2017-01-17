using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DragonBone
{
	[ExecuteInEditMode]
	public class DragonBoneArmature : MonoBehaviour {

		[Range(0.001f,1f)]
		public float zSpace = 0.001f;
		[SerializeField]
		private bool m_FlipX;
		[SerializeField]
		private bool m_FlipY;

		public Slot[] slots;
		public Renderer[] attachments;
		public Material[] materials;
		public TextureFrame[] textureFrames;

		private List<Slot> m_OrderSlots = new List<Slot>();
		private int[] m_NewSlotOrders = null ;		
		private bool m_CanSortAllSlot = false;

		protected int __ZOrderValid = 0;
		[HideInInspector]
		[SerializeField]
		private float m_ZOrderValid = 0f;


		private Animator m_animator;
		public Animator aniamtor{
			get { 
				if(m_animator==null) m_animator = gameObject.GetComponent<Animator>();
				return m_animator;
			} 
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
						if(!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(r);
						#endif
						SpriteFrame sf = r.GetComponent<SpriteFrame>();
						if(sf) {
							sf.sortingLayerName = value;
							#if UNITY_EDITOR 
							if(!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(sf);
							#endif
						}
						else {
							SpriteMesh sm = r.GetComponent<SpriteMesh>();
							if(sm) {
								sm.sortingLayerName = value;
								#if UNITY_EDITOR 
								if(!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(sm);
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
						if(!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(r);
						#endif
						SpriteFrame sf = r.GetComponent<SpriteFrame>();
						if(sf) {
							sf.sortingOrder = value;
							#if UNITY_EDITOR 
							if(!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(sf);
							#endif
						}
						else {
							SpriteMesh sm = r.GetComponent<SpriteMesh>();
							if(sm) {
								sm.sortingOrder = value;
								#if UNITY_EDITOR 
								if(!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(sm);
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
				if(m_FlipX == value) return;
				#endif
				m_FlipX =  value;

				transform.Rotate(0f,180f,0f);

				Vector3 v = transform.localEulerAngles;
				v.x = ClampAngle(v.x,-360f,360f);
				v.y = ClampAngle(v.y,-360f,360f);
				v.z = ClampAngle(v.z,-720f,720f);
				transform.localEulerAngles=v;

				#if UNITY_EDITOR 
				if(!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(transform);
				#endif
				ResetSlotZOrder();
			}
		}

		public bool flipY{
			get { return m_FlipY; }
			set {
				#if !UNITY_EDITOR
				if(m_FlipY == value) return;
				#endif
				m_FlipY =  value;
				transform.Rotate(180f,0f,0f);

				Vector3 v = transform.localEulerAngles;
				v.x = ClampAngle(v.x,-360f,360f);
				v.y = ClampAngle(v.y,-360f,360f);
				v.z = ClampAngle(v.z,-720f,720f);
				transform.localEulerAngles=v;

				#if UNITY_EDITOR 
				if(!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(transform);
				#endif
				ResetSlotZOrder();
			}
		}

		float ClampAngle(float angle,float min ,float max){
			if (angle<90 || angle>270){       // if angle in the critic region...
				if (angle>180) angle -= 360;  // convert all angles to -180..+180
				if (max>180) max -= 360;
				if (min>180) min -= 360;
			}
			angle = Mathf.Clamp(angle, min, max);
			return angle;
		}

		/// <summary>
		/// Lates the update. Sort slot
		/// </summary>
		void LateUpdate(){
			#if UNITY_EDITOR
			if(Application.isPlaying){
				if(aniamtor!=null && aniamtor.enabled)
				{
					UpdateSlots();
				}
			}
			else
			{
				if(aniamtor!=null)
				{
					UpdateSlots();
				}
			}
			#else
			if(aniamtor!=null && aniamtor.enabled)
			{
				UpdateArmature();
			}
			#endif
			if(m_CanSortAllSlot){
				ForceSortAll();
				m_CanSortAllSlot = false;
			}
			else
			{
				int temp=(int) m_ZOrderValid;
				if(Mathf.Abs(m_ZOrderValid-temp)>0.0001f) return;
				if(temp!=__ZOrderValid){
					__ZOrderValid = temp;
					ResetSlotZOrder();
				}
			}
		}

		void CalculateOrder(){
			int orderCount = m_OrderSlots.Count;
			int slotCount = slots.Length;
			int[] unchanged = new int[slotCount - orderCount];

			if(m_NewSlotOrders==null){
				m_NewSlotOrders = new int[slotCount];
			}
			for (int i = 0; i < slotCount; ++i){
				m_NewSlotOrders[i] = -1;
			}

			int originalIndex = 0;
			int unchangedIndex = 0;
			for (int i = 0; i<orderCount ; ++i)
			{
				Slot slot = m_OrderSlots[i];
				int slotIndex = slot.zOrder;
				int offset = slot.z;

				while (originalIndex != slotIndex)
				{
					unchanged[unchangedIndex++] = originalIndex++;
				}
				m_NewSlotOrders[originalIndex + offset] = originalIndex++;
			}

			while (originalIndex < slotCount)
			{
				unchanged[unchangedIndex++] = originalIndex++;
			}

			int iC = slotCount;
			while (iC-- != 0)
			{
				if (m_NewSlotOrders[iC] == -1)
				{
					m_NewSlotOrders[iC] = unchanged[--unchangedIndex];
				}
			}

			//set order
			float zoff = m_FlipX || m_FlipY ? 1f : -1f;
			if(m_FlipX && m_FlipY) zoff = -1f;
			zoff*=zSpace;
			for(int i=0;i<slotCount;++i){
				Slot slot = slots[m_NewSlotOrders[i]];
				if(slot){
					Vector3 v = slot.transform.localPosition;
					v.z = zoff*i+zoff*0.001f;
					slot.transform.localPosition = v;
				}
			}

			m_OrderSlots.Clear();
		}

		/// <summary>
		/// update
		/// </summary>
		public void UpdateSlots(){
			int len = slots.Length;
			for(int i=0;i<len;++i){
				Slot slot = slots[i];
				if(slot && slot.isActiveAndEnabled){
					slot.UpdateSlot();
				}
			}
		}

		/// <summary>
		/// Resets the slot Z order.
		/// </summary>
		public void ResetSlotZOrder(){
			float tempZ = m_FlipX || m_FlipY ? 1f : -1f;
			if(m_FlipX && m_FlipY) tempZ = -1f;

			tempZ*=zSpace;
			int len = slots.Length;
			for(int i=0;i<len;++i){
				Slot slot = slots[i];
				if(slot){
					Vector3 v = slot.transform.localPosition;
					v.z = tempZ*slot.zOrder+tempZ*0.001f;
					slot.transform.localPosition = v;
					slot.z = 0;
					#if UNITY_EDITOR 
					if(!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(slot.transform);
					#endif
				}
			}
			m_OrderSlots.Clear();
			m_CanSortAllSlot = false;
		}

		/// <summary>
		/// Forces the sort all.
		/// </summary>
		public void ForceSortAll(){
			m_OrderSlots.Clear();
			int len = slots.Length;
			for(int i=0;i<len;++i){
				Slot slot = slots[i];
				if(slot){
					if(slot.z!=0) m_OrderSlots.Add(slot);
				}
			}
			if(m_OrderSlots.Count>0)
				CalculateOrder();
		}

		/// <summary>
		/// slot call this function
		/// </summary>
		/// <param name="slot">Slot.</param>
		public void UpdateSlotZOrder(Slot slot){
			if(slot.z!=0) m_CanSortAllSlot = true;
		}


		#region Change Skin

		/// <summary>
		/// Gets attachment by name.
		/// </summary>
		/// <returns>The attachment.</returns>
		/// <param name="attachmentName">Attachment name.</param>
		public Renderer GetAttachmentByName( string attachmentName){
			foreach(Renderer r in attachments){
				if(r && r.name.Equals(attachmentName)) {
					return r;
				}
			}
			return null;
		}

		/// <summary>
		/// Get texture frame by name.
		/// </summary>
		/// <returns>The texture frame by name.</returns>
		/// <param name="frameName">Frame name.</param>
		public TextureFrame GetTextureFrameByName( string frameName){
			foreach(TextureFrame frame in textureFrames){
				if(frame!=null && frame.name.Equals(frameName)){
					return frame;
				}
			}
			return null;
		}

		/// <summary>
		/// Changes the sprite frame.
		/// </summary>
		/// <param name="spriteFrameName">Will replace SpriteFrame's name.</param>
		/// <param name="texture">Texture.</param>
		/// <param name="mat">Mat.</param>
		public void ChangeSpriteFrame(string spriteFrameName,Texture texture,Material mat=null){
			if(string.IsNullOrEmpty(spriteFrameName) || !texture) return;

			Renderer attachment = GetAttachmentByName(spriteFrameName);
			if(!attachment) return;
			SpriteFrame sf = attachment.GetComponent<SpriteFrame>();
			ChangeSpriteFrame(sf,texture,mat);
		}

		/// <summary>
		/// Changes the sprite frame.
		/// </summary>
		/// <param name="sf">Will replace SpriteFrame</param>
		/// <param name="texture">new Texture.</param>
		/// <param name="mat">Mat.</param>
		public void ChangeSpriteFrame(SpriteFrame sf,Texture texture,Material mat=null){
			if(!sf || !texture) return;

			if(mat) sf.atlasMat = mat;
			sf.atlasMat.mainTexture = texture;
			sf.frame.atlasTextureSize=new Vector2(texture.width,texture.height);
			sf.frame.material = sf.atlasMat;
			sf.frame.rect.x = 0f;
			sf.frame.rect.y = 0f;
			sf.frame.rect.width = texture.width;
			sf.frame.rect.height = texture.height;
			sf.frame.frameSize = new Rect(0,0,texture.width,texture.height);
			sf.frame.isRotated =false;
			sf.rect = sf.frame.frameSize;
		}

		/// <summary>
		/// Changes the sprite frame.
		/// </summary>
		/// <param name="spriteFrameName">Sprite frame name.</param>
		/// <param name="newTextureFrameName">New texture frame name.</param>
		public void ChangeSpriteFrame(string spriteFrameName, string newTextureFrameName){
			Renderer attachment = GetAttachmentByName(spriteFrameName);
			if(!attachment) return;

			SpriteFrame sf = attachment.GetComponent<SpriteFrame>();
			TextureFrame frame = GetTextureFrameByName(newTextureFrameName);
			if(sf!=null && frame!=null){
				sf.atlasMat = frame.material;
				attachment.sharedMaterial = frame.material;
				sf.frames[0] = frame;
				sf.frameName = newTextureFrameName;
				sf.UpdateVertex();
			}
		}

		/// <summary>
		/// Changes the sprite mesh.
		/// </summary>
		/// <param name="spriteMeshName">Will replace SpriteMesh'name.</param>
		/// <param name="texture">new Texture.</param>
		/// <param name="mat">Mat.</param>
		public void ChangeSpriteMesh(string spriteMeshName,Texture texture,Material mat=null){
			Renderer attachment = GetAttachmentByName(spriteMeshName);
			if(!attachment) return;
			SpriteMesh sm = attachment.GetComponent<SpriteMesh>();
			ChangeSpriteMesh(sm,texture,mat);
		}
		/// <summary>
		/// Changes the sprite mesh.
		/// </summary>
		/// <param name="sm">Will replace SpriteMesh</param>
		/// <param name="texture">new Texture.</param>
		/// <param name="mat">Mat.</param>
		public void ChangeSpriteMesh(SpriteMesh sm,Texture texture,Material mat=null){
			if(!sm || !texture) return;

			if(mat) sm.atlasMat = mat;

			sm.atlasMat.mainTexture = texture;
			sm.frame.material = sm.atlasMat;
			sm.frame.texture = texture;
			sm.frame.isRotated = false;

			sm.frame.rect.x = 0;
			sm.frame.rect.y = 0;
			sm.frame.rect.width = texture.width;
			sm.frame.rect.height = texture.height;
			sm.frame.frameSize = new Rect(0,0,texture.width,texture.height);
			sm.frame.atlasTextureSize.x = texture.width;
			sm.frame.atlasTextureSize.y = texture.height;

			sm.UpdateUV();
		}


		/// <summary>
		/// Changes the sprite mesh.
		/// </summary>
		/// <param name="spriteMeshName">Sprite mesh name.</param>
		/// <param name="newTextureFrameName">New texture frame name.</param>
		public void ChangeSpriteMesh(string spriteMeshName, string newTextureFrameName){
			Renderer attachment = GetAttachmentByName(spriteMeshName);
			if(!attachment) return;

			SpriteMesh sm = attachment.GetComponent<SpriteMesh>();
			TextureFrame frame = GetTextureFrameByName(newTextureFrameName);
			if(sm!=null && frame!=null){
				sm.atlasMat = frame.material;
				attachment.sharedMaterial = frame.material;
				sm.frame = frame;
				sm.UpdateUV();
			}
		}

		/// <summary>
		/// Changes the sprite renderer.
		/// </summary>
		/// <param name="spriteRendererName">Sprite renderer name.</param>
		/// <param name="texture">Texture.</param>
		/// <param name="pivot">Pivot.</param>
		/// <param name="mat">Mat.</param>
		public void ChangeSpriteRenderer(string spriteRendererName,Texture2D texture,Material mat=null){
			SpriteRenderer attachment = GetAttachmentByName(spriteRendererName) as SpriteRenderer;
			if(!attachment) return;

			if(mat!=null) attachment.material = mat;

			Sprite sprite = Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),Vector2.one*0.5f,100f,1,SpriteMeshType.FullRect);
			attachment.sprite=sprite;
		}

		/// <summary>
		/// Changes the sprite renderer.
		/// </summary>
		/// <param name="spriteRendererName">Sprite renderer name.</param>
		/// <param name="sprite">Sprite.</param>
		/// <param name="mat">Mat.</param>
		public void ChangeSpriteRenderer(string spriteRendererName,Sprite sprite,Material mat= null){
			SpriteRenderer attachment = GetAttachmentByName(spriteRendererName) as SpriteRenderer;
			if(!attachment) return;
			if(mat!=null) attachment.material = mat;
			attachment.sprite = sprite;
		}

		/// <summary>
		/// Changes the sprite of SpriteRenderer.
		/// </summary>
		/// <param name="sr">Will replace SpriteRenderer.</param>
		/// <param name="sprite">new Sprite.</param>
		/// <param name="mat">Mat.</param>
		public void ChangeSpriteRenderer(SpriteRenderer sr,Sprite sprite,Material mat= null){
			if(mat!=null) sr.material = mat;
			sr.sprite = sprite;
		}
		#endregion
	}
}
