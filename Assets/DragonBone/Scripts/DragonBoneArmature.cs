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
		public SpriteFrame[] updateFrames;
		public SpriteMesh[] updateMeshs;
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
					UpdateArmature();
				}
			}
			else
			{
				if(aniamtor!=null)
				{
					UpdateArmature();
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

			len = slots.Length;
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

	}
}
