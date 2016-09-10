using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DragonBone
{
	/// <summary>
	/// 用于合并Mesh，主要用于非动画对象
	/// author:  bingheliefeng
	/// </summary>
	public class SpriteMeshCombine : MonoBehaviour {

		class SpriteVetex{
			public MeshFilter mf;
			public SkinnedMeshRenderer smr;
			public int vertexStartIndex;//顶点索引的起始
			public int vertexCount;//顶点的数量
			public Matrix4x4 matrix;
		}

		private SkinnedMeshRenderer m_skinRenderer;
		private Mesh m_mesh;

		private List<SpriteVetex> m_svs = new List<SpriteVetex>();

		// Use this for initialization
		IEnumerator Start () {
			yield return new WaitForEndOfFrame();
			Combine();
		}

		/// <summary>
		/// 合并网格
		/// </summary>
		public void Combine(){
			m_svs.Clear();
			m_mesh = new Mesh();

			Renderer[] renders = GetComponentsInChildren<Renderer>();
			List<Material> mats = new List<Material>();
			foreach(Renderer render in renders)
			{
				if(render is MeshRenderer)
				{
					MeshFilter filter = render.GetComponent<MeshFilter>();
					if(filter) {
						if(render.sharedMaterial && !CheckMaterialExist(render.sharedMaterial,mats)){
							mats.Add(render.sharedMaterial);
						}
						SpriteVetex sv = new SpriteVetex();
						sv.mf = filter;
						sv.matrix = transform.worldToLocalMatrix*render.transform.localToWorldMatrix;
						m_svs.Add(sv);
					}
				}
				else if(render is SkinnedMeshRenderer)
				{
					if(render.sharedMaterial && !CheckMaterialExist(render.sharedMaterial,mats)){
						mats.Add(render.sharedMaterial);
					}
					SpriteVetex sv = new SpriteVetex();
					sv.smr = render as SkinnedMeshRenderer;
					sv.matrix = transform.worldToLocalMatrix*render.transform.localToWorldMatrix;
					m_svs.Add(sv);
				}
				render.gameObject.SetActive(false);
			}
			if(m_svs.Count>0){
				m_svs.Sort(delegate(SpriteVetex x, SpriteVetex y) {
					return x.matrix.MultiplyPoint(Vector3.zero).z-y.matrix.MultiplyPoint(Vector3.zero).z>0?-1:1;	
				});
				List<CombineInstance> cis = new List<CombineInstance>();
				int vIdx = 0;
				for(int i=0;i<m_svs.Count;++i)
				{
					SpriteVetex sv = m_svs[i];
					CombineInstance ci =  new CombineInstance();
					if(sv.mf){
						ci.mesh = sv.mf.sharedMesh;
					}else{
						ci.mesh = sv.smr.sharedMesh;
					}
					sv.vertexStartIndex = vIdx;
					sv.vertexCount = ci.mesh.vertexCount;
					vIdx+=sv.vertexCount;

					ci.transform = sv.matrix;
					cis.Add(ci);
				}

				m_mesh.CombineMeshes(cis.ToArray());
				if(m_skinRenderer==null){
					m_skinRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();
					m_skinRenderer.shadowCastingMode= UnityEngine.Rendering.ShadowCastingMode.Off;
					m_skinRenderer.receiveShadows = false;
				}
				m_skinRenderer.sharedMesh = m_mesh;
				m_skinRenderer.sharedMaterials = mats.ToArray();
			}
		}

		bool CheckMaterialExist( Material mat , List<Material> mats){
			foreach(Material material in mats){
				if(mat==material) return true;
			}
			return false;
		}

		/// <summary>
		/// 更新Mesh的状态
		/// </summary>
		public void UpdateMesh () {
			if(m_mesh){
				Vector3[] vertices=m_mesh.vertices;
				Color[] colors=m_mesh.colors;
				for(int i=0;i<m_svs.Count;++i)
				{
					SpriteVetex sv = m_svs[i];
					Mesh mesh = sv.mf!=null ? sv.mf.sharedMesh : sv.smr.sharedMesh ;
					Transform trans = sv.mf!=null ? sv.mf.transform : sv.smr.transform ;
					if(mesh)
					{
						Matrix4x4 matrix =transform.worldToLocalMatrix*trans.localToWorldMatrix;
						for(int j=0;j<sv.vertexCount;++j)
						{
							vertices[sv.vertexStartIndex+j] = matrix.MultiplyPoint3x4(mesh.vertices[j]);
							colors[sv.vertexStartIndex+j] = mesh.colors[j];
						}
					}
				}
				m_mesh.colors = colors;
				m_mesh.vertices = vertices;
			}
		}
	}
}