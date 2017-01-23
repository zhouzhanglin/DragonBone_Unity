using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DragonBone
{
	/// <summary>
	/// Sprite mesh.
	/// author:bingheliefeng
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
		public int[] edges;

		public BoneWeightClass[] weights;//for skinnedMesh
		public Matrix4x4[] bindposes; //for skinnedMesh
		public Transform[] vertControlTrans;//for ffd animation

		//	public Vector2 pivot;
		public Material atlasMat;

		[HideInInspector]
		public TextureFrame frame;

//		[SerializeField]
//		private Vector2 m_uvOffset;
//		public Vector2 uvOffset{
//			get { return m_uvOffset; }
//			set{
//				m_uvOffset = value;
//				if(m_createdMesh) UpdateUV();
//			}
//		}

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
		public Color color{
			get { return m_color;}
			set { 
				if(!m_color.Equals(value)){
					m_color = value;
					if(m_createdMesh) UpdateVertexColor();
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


		[HideInInspector]
		[SerializeField]
		private PolygonCollider2D m_collder;

		[HideInInspector]
		[SerializeField]
		private Vector2[] m_collider_points;

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
				UpdateUV();
				m_mesh.triangles = triangles;
				if(edges!=null && edges.Length>0 && m_collder==null){
					m_collder = GetComponent<PolygonCollider2D>();
					if(m_collder==null) m_collder = gameObject.AddComponent<PolygonCollider2D>();
					UpdateEdges();
				}
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

		public void UpdateUV(){
			if(m_mesh && frame!=null && uvs!=null){
				//uv to Atlas
				Vector2[] tempUvs = new Vector2[uvs.Length] ;
				for(int i=0;i<uvs.Length;++i){
					Vector2 uv=uvs[i];
					if(frame.isRotated){
						float x = uv.y*frame.rect.width;
						float y = frame.rect.height - uv.x*frame.rect.height;
						uv.x = 1-x/frame.rect.width;
						uv.y = 1-y/frame.rect.height;
					}
					Vector2 uvPos = new Vector2(frame.rect.x,frame.atlasTextureSize.y-frame.rect.y-frame.rect.height)+ 
						new Vector2(frame.rect.width*uv.x,frame.rect.height*uv.y);
					uv.x = uvPos.x/frame.atlasTextureSize.x;
					uv.y = uvPos.y/frame.atlasTextureSize.y;
					tempUvs[i] = uv;
				}
//				for(int i=0;i<tempUvs.Length;++i){
//					tempUvs[i] = tempUvs[i]-m_uvOffset;
//				}
				m_mesh.uv = tempUvs;
			}
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

				UpdateUV();
				m_mesh.triangles=triangles;
				UpdateEdges();
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
				UpdateEdges();
			}
		}

		public void UpdateEdges(){
			if(m_collder!=null && edges!=null && edges.Length>0){
				int len = edges.Length;
				if(m_collider_points==null) m_collider_points = new Vector2[len];

				SkinnedMeshRenderer smr = m_render as SkinnedMeshRenderer;
				for(int i=0;i<len;++i){
					int vIndex = edges[i];
					Vector3 v = vertices[vIndex]; //local vertex

					if(m_isSkinnedMeshRenderer && weights!=null){
						BoneWeightClass bw = weights[vIndex];
						Matrix4x4[] boneMatrices = new Matrix4x4[smr.bones.Length];
						for (int j= 0; j< boneMatrices.Length; ++j)
							boneMatrices[j] = smr.bones[j].localToWorldMatrix * mesh.bindposes[j];

						Matrix4x4 bm0 = boneMatrices[bw.boneIndex0];
						Matrix4x4 bm1 = boneMatrices[bw.boneIndex1];
						Matrix4x4 bm2 = boneMatrices[bw.boneIndex2];
						Matrix4x4 bm3 = boneMatrices[bw.boneIndex3];

						Matrix4x4 vertexMatrix = new Matrix4x4();
						for (int n= 0; n < 16; ++n){
							vertexMatrix[n] =
								bm0[n] * bw.weight0 +
								bm1[n] * bw.weight1 +
								bm2[n] * bw.weight2 +
								bm3[n] * bw.weight3;
						}
						v = transform.InverseTransformPoint( vertexMatrix.MultiplyPoint3x4(v));
					}
					m_collider_points[i] = (Vector2)v;
				}
				m_collder.points = m_collider_points;
			}
		}

		#if UNITY_EDITOR
		void OnDrawGizmos(){
			if(vertControlTrans!=null && m_mesh && (weights==null || weights.Length==0) && Selection.activeTransform){
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