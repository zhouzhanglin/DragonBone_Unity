using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DragonBone
{
	/// <summary>
	/// Sprite mesh.
	/// author:  bingheliefeng
	/// </summary>
	[ExecuteInEditMode]
	public class SpriteMesh : MonoBehaviour {

		[System.Serializable]
		public class BoneWeightClass{
			public int boneIndex0;
			public int boneIndex1;
			public int boneIndex2;
			public int boneIndex3;
			public float weight0;
			public float weight1;
			public float weight2;
			public float weight3;
			public BoneWeight GetBoneWeight(){
				BoneWeight bw = new BoneWeight();
				bw.boneIndex0 = boneIndex0;
				bw.weight0 = weight0;
				bw.boneIndex1 = boneIndex1;
				bw.weight1 = weight1;
				bw.boneIndex2 = boneIndex2;
				bw.weight2 = weight2;
				bw.boneIndex3 = boneIndex3;
				bw.weight3 = weight3;
				return bw;
			}
		}

		public Vector3[] vertices;
		public Vector2[] uvs;
		public Color[] colors;//vertex color
		public int[] triangles;

		public BoneWeightClass[] weights;//for skinnedMesh
		public Matrix4x4[] bindposes; //for skinnedMesh
		public Transform[] vertControlTrans;//for ffd animation

		//	public Vector2 pivot;
		public Material atlasMat;

		[Range(0f,1f)]
		[SerializeField]
		private float m_brightness = 0f;
		public float brightness{
			get { return m_brightness;}
			set {	m_brightness=value;
				if(m_createdMesh)	UpdateVertexColor();
			}
		}

		[SerializeField]
		private Color m_color = Color.white;//for animation
		private Color __Color = Color.white;
		public Color color{
			get { return __Color;}
			set { 
				if(!__Color.Equals(value)){
					m_color = value; 
					__Color = value;
					if(m_createdMesh)	UpdateVertexColor();
				}
			}
		}

		[SerializeField]
		private string m_sortingLayerName="Default";
		public string sortingLayerName{
			get { return m_sortingLayerName;}
			set {	m_sortingLayerName=value;
				if(m_createdMesh)	UpdateSorting();
			}
		}

		[SerializeField]
		private int m_sortingOrder = 0;
		public int sortingOrder{
			get { return m_sortingOrder;}
			set {	m_sortingOrder=value;
				if(m_createdMesh)	UpdateSorting();
			}
		}

		[HideInInspector]
		[SerializeField]
		private bool m_createdMesh;
		[HideInInspector]
		[SerializeField]
		private bool m_isSkinnedMeshRenderer = false;

		private Mesh m_mesh;
		public Mesh mesh{
			get {return m_mesh;}
		}
		private Renderer m_render;

		void Start(){
			if(m_createdMesh && m_mesh==null){
				this.CreateMesh(m_isSkinnedMeshRenderer);
			}
		}

		void OnEnable(){
			UpdateVertexColor();
		}

		public void CreateMesh(bool isSkinnedMeshRenderer){
			m_isSkinnedMeshRenderer = isSkinnedMeshRenderer;
			if(vertices!=null && uvs!=null && colors!=null && triangles!=null){
				if(m_mesh==null) m_mesh = new Mesh();
				m_mesh.vertices = vertices;
				m_mesh.uv = uvs;
				m_mesh.triangles = triangles;
				UpdateVertexColor();
				m_mesh.RecalculateNormals();
				m_mesh.RecalculateBounds();
				if(isSkinnedMeshRenderer){
					m_mesh.boneWeights = GetBoneWeights();
					//SkinnedMesh Animation
					m_render = GetComponent<SkinnedMeshRenderer>();
					if(m_render==null) m_render = gameObject.AddComponent<SkinnedMeshRenderer>();
					(m_render as SkinnedMeshRenderer).sharedMesh = m_mesh;
					m_render.receiveShadows = false;
					m_render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
					m_render.sharedMaterial = atlasMat;
				}
				else
				{
					//FFD Animation
					m_render = GetComponent<MeshRenderer>();
					if(m_render==null) m_render = gameObject.AddComponent<MeshRenderer>();

					m_render.receiveShadows = false;
					m_render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
					m_render.sharedMaterial = atlasMat;

					MeshFilter mf = GetComponent<MeshFilter>();
					if(mf==null) mf = gameObject.AddComponent<MeshFilter>();
					mf.sharedMesh = m_mesh;
				}
				if(bindposes!=null && bindposes.Length>0){
					m_mesh.bindposes = bindposes;
				}

				UpdateSorting();
				m_createdMesh = true;
			}
		}

		BoneWeight[] GetBoneWeights(){
			if(weights!=null && weights.Length>0){
				int len = weights.Length;
				BoneWeight[] myWeights = new BoneWeight[len];
				for(int i=0;i<len;++i){
					BoneWeight bw = weights[i].GetBoneWeight() ;
					myWeights[i] = bw;
				}
				return myWeights;
			}
			return null;
		}

		public void UpdateVertexColor(){
			if(m_mesh){
				Color col = m_color*(Mathf.Clamp(m_brightness,0f,1f)*10f+1f);
				for(int i=0;i<colors.Length;++i){
					colors[i]=col;
				}
				m_mesh.colors=colors;
			}
		}

		public void UpdateSorting(){
			if(m_render){
				m_render.sortingLayerName = m_sortingLayerName;
				m_render.sortingOrder = m_sortingOrder;
			}
		}

		#if UNITY_EDITOR
		void Update(){
			if(!Application.isPlaying && m_createdMesh && m_mesh && vertControlTrans!=null){
				int len = vertControlTrans.Length;
				for(int i=0;i<len;++i){
					vertices[i] = vertControlTrans[i].localPosition;
				}
				m_mesh.vertices=vertices;

				m_mesh.uv=uvs;
				m_mesh.triangles=triangles;
				UpdateVertexColor();
				UpdateSorting();
				m_mesh.RecalculateBounds();
			}
		}
		#endif


		public void UpdateMesh(){
			if(m_createdMesh && m_mesh){
				if(vertControlTrans!=null){
					int len = vertControlTrans.Length;
					for(int i=0;i<len;++i){
						vertices[i] = vertControlTrans[i].localPosition;
					}
					m_mesh.vertices=vertices;
				}
				color = m_color;
			}
		}


		#if UNITY_EDITOR
		void OnDrawGizmos(){
			if(vertControlTrans!=null && m_mesh && Selection.activeTransform){
				if(Selection.activeTransform==this.transform|| Selection.activeTransform.parent==this.transform){
					Gizmos.color = Color.red;
					if(vertControlTrans!=null){
						foreach(Transform v in vertControlTrans){
							Gizmos.DrawWireSphere(v.position,0.02f);
						}
					}
				}
			}
		}
		#endif
	}

}