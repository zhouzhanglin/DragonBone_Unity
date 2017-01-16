using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System.Collections.Generic;
#endif

namespace DragonBone
{
	/// <summary>
	/// Sprite frame.
	/// author:bingheliefeng
	/// </summary>
	[ExecuteInEditMode]
	public class SpriteFrame : MonoBehaviour {

		public TextAsset atlasText;
		public Material atlasMat;
		[Range(0.1f,10f)]
		public float textureScale=1f;

		[HideInInspector]
		[SerializeField]
		private string m_frameName;
		public string frameName{
			get { return m_frameName;}
			set {
				m_frameName = value;
				if(m_createdMesh && !string.IsNullOrEmpty(m_frameName) && frames!=null){
					int len = frames.Length;
					for(int i=0;i<len;++i){
						TextureFrame frame = frames[i];
						if(frame.name.Equals(m_frameName)){
							rect = frame.rect;
							m_frame = frame;
							m_displayIndex = i;
							break;
						}
					}
				}
			}
		}

		[HideInInspector]
		[SerializeField]
		private float m_displayIndex=-1;
		public float displayIndex{
			get{ return m_displayIndex;}
			set {
				if(frames!=null && m_displayIndex!=value){
					int index = (int)value;
					if(index==-1)
					{
						m_rect.width = 0f;
						m_rect.height = 0f;
						rect = m_rect;
					}else{
						if(index>-1 && index<frames.Length){
							frameName = frames[index].name;
						}
					}
					m_displayIndex=value;
				}
			}
		}

		[SerializeField]
		private Vector2 m_uvOffset;
		public Vector2 uvOffset{
			get { return m_uvOffset; }
			set { 
				if(!m_uvOffset.Equals(value)){
					m_uvOffset = value;
					if(m_createdMesh)	UpdateUV();
				}
			}
		}

		[SerializeField]
		private Vector2 m_pivot;
		public Vector2 pivot{
			get { return m_pivot; }
			set { 
				if(!m_pivot.Equals(value)){
					m_pivot=value;
					if(m_createdMesh)	UpdateVertex();
				}
			}
		}

		[HideInInspector]
		[SerializeField]
		private Rect m_rect;
		public Rect rect{
			get { return m_rect;}
			set { 
				if(!m_rect.Equals(value)){
					m_rect = value;
					if(m_createdMesh)	UpdateUV();
				}
			}
		}

		[SerializeField]
		private Color m_color = Color.white;//For Animation
		private Color __Color = Color.white;
		public Color color{
			get { return __Color;}
			set { 
				if(!__Color.Equals(value)  || !m_color.Equals(value)){
					m_color = value; 
					__Color = value;
					if(m_createdMesh)	UpdateVertexColor();
				}
			}
		}

		[Range(0f,1f)]
		[SerializeField]
		private float m_brightness = 0f;
		public float brightness{
			get { return m_brightness;}
			set {	
				if(m_brightness!=value){
					m_brightness=value;
					if(m_createdMesh)	UpdateVertexColor();
				}
			}
		}

		[SerializeField]
		private string m_sortingLayerName="Default";
		public string sortingLayerName{
			get { return m_sortingLayerName;}
			set {	
				if(!m_sortingLayerName.Equals(value)){
					m_sortingLayerName=value;
					if(m_createdMesh)	UpdateSorting();
				}
			}
		}

		[SerializeField]
		private int m_sortingOrder = 0;
		public int sortingOrder{
			get { return m_sortingOrder;}
			set {	
				if(m_sortingOrder!=value){
					m_sortingOrder=value;
					if(m_createdMesh)	UpdateSorting();
				}
			}
		}

		[HideInInspector]
		public TextureFrame[] frames;

		[HideInInspector]
		[SerializeField]
		private TextureFrame m_frame;
		public TextureFrame frame{
			get { return m_frame; }
		}

		[HideInInspector]
		[SerializeField]
		private bool m_createdMesh = false;
		private MeshFilter m_meshFilter;
		private MeshRenderer m_meshRenderer;
		private Mesh m_mesh ;
		private static Vector3[] m_normals = new Vector3[4]{Vector3.forward,Vector3.forward,Vector3.forward,Vector3.forward};

		void Start(){
			if(m_createdMesh && m_mesh==null){
				this.CreateQuad();
				this.frameName = m_frameName;
			}
			if(m_mesh)
				m_mesh.normals = m_normals;
		}

		void OnEnable(){
			UpdateVertexColor();
		}

		public void CreateQuad(){
			if(m_mesh==null) m_mesh = new Mesh();
			m_mesh.vertices = new Vector3[4];
			m_mesh.uv = new Vector2[4];
			m_mesh.colors = new Color[4];
			m_mesh.triangles = new int[]{0,1,2,2,3,0};

			m_meshFilter= gameObject.GetComponent<MeshFilter>();
			if(m_meshFilter==null) m_meshFilter = gameObject.AddComponent<MeshFilter>();
			m_meshFilter.mesh = m_mesh;

			m_meshRenderer = gameObject.GetComponent<MeshRenderer>();
			if(m_meshRenderer==null) m_meshRenderer = gameObject.AddComponent<MeshRenderer>();
			m_meshRenderer.receiveShadows = false;
			m_meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			m_meshRenderer.material = atlasMat;

			UpdateVertex();
			UpdateUV();
			UpdateVertexColor();
			UpdateSorting();
			m_mesh.RecalculateBounds();
			m_createdMesh = true;
		}

		#if UNITY_EDITOR
		void LateUpdate(){
			if( m_createdMesh && !Application.isPlaying) {
				UpdateSorting();
				UpdateVertex();
				UpdateUV();
				UpdateVertexColor();
				m_mesh.RecalculateBounds();
				m_mesh.normals=m_normals;
			}
		}
		#endif

		public void UpdateSorting(){
			if(m_meshRenderer){
				m_meshRenderer.sortingLayerName = m_sortingLayerName;
				m_meshRenderer.sortingOrder = m_sortingOrder;
			}
		}

		public void UpdateUV(){
			if(m_mesh!=null && m_frame!=null && m_frame.atlasTextureSize.x>1 && m_frame.atlasTextureSize.y>1){
				Vector2[] rectUV = new Vector2[]{
					new Vector2(m_rect.x,  m_frame.atlasTextureSize.y-m_rect.y-m_rect.height),
					new Vector2(m_rect.x,  m_frame.atlasTextureSize.y-m_rect.y),
					new Vector2(m_rect.x+m_rect.width,  m_frame.atlasTextureSize.y-m_rect.y),
					new Vector2(m_rect.x+m_rect.width, m_frame.atlasTextureSize.y-m_rect.y-m_rect.height),
				};
				Vector2[] uvs= m_mesh.uv;
				for(int i=0;i<4;++i){
					uvs[i] =new Vector2(rectUV[i].x/m_frame.atlasTextureSize.x,rectUV[i].y/m_frame.atlasTextureSize.y)-m_uvOffset*0.01f;
				}
				if(m_frame!=null && m_frame.isRotated){
					rectUV[0] = uvs[3];
					rectUV[1] = uvs[0];
					rectUV[2] = uvs[1];
					rectUV[3] = uvs[2];
					uvs = rectUV;
				}

				m_mesh.uv = uvs;
			}
		}

		public void UpdateVertex(){
			float x = 0f;
			float y = 0f;
			float w = m_rect.width;
			float h = m_rect.height;
			if(m_frame!=null){
				if(m_frame.isRotated){
					w = m_rect.height;
					h = m_rect.width;
				}
				Vector3 offset = m_frame.frameOffset*0.01f;
				x = offset.x;
				y = offset.y;
			}
			Vector3[] verts = m_mesh.vertices;
			verts[0].x = x;
			verts[0].y = y;

			verts[1].x = x;
			verts[1].y = y+h*0.01f ;

			verts[2].x = x+w*0.01f;
			verts[2].y = y+h*0.01f;

			verts[3].x = x+w*0.01f;
			verts[3].y = y;

			Vector3 pivot = new Vector3(m_pivot.x*w*0.01f ,m_pivot.y*h*0.01f,0f);

			for(int i=0;i<4;++i){
				verts[i]-=pivot;
			}
			m_mesh.vertices = verts;
		}

		public void UpdateVertexColor(){
			if(m_mesh){
				float brightness1 = Mathf.Clamp(m_brightness,0f,1f)*10f+1f;
				Color[] colors = m_mesh.colors;
				Color c = m_color*brightness1;
				for(int i=0;i<4;++i){
					colors[i]=c;
				}
				m_mesh.colors=colors;
			}
		}
		#if UNITY_EDITOR

		public void ParseAtlasText(){
			if(atlasText!=null && atlasMat!=null && atlasMat.mainTexture!=null)
			{
				SimpleJSON.JSONClass obj = SimpleJSON.JSON.Parse(atlasText.text).AsObject ;
				SimpleJSON.JSONArray arr = obj["SubTexture"].AsArray;
				frames = new TextureFrame[arr.Count];
				Vector2 textureSize = new Vector2(atlasMat.mainTexture.width,atlasMat.mainTexture.height);
				for(int i=0;i<arr.Count;++i){
					SimpleJSON.JSONClass frameObj = arr[i].AsObject;
					TextureFrame frame = new TextureFrame();
					frame.atlasTextureSize = textureSize;
					frame.name = frameObj["name"];
					frame.name = frame.name.Replace('/','_');
					frame.texture = atlasMat.mainTexture;
					frame.material = atlasMat;
					Rect rect = new Rect();
					rect.x = frameObj["x"].AsFloat*textureScale;
					rect.y = frameObj["y"].AsFloat*textureScale;
					rect.width = frameObj["width"].AsFloat*textureScale;
					rect.height = frameObj["height"].AsFloat*textureScale;
					Rect frameSize=new Rect(0,0,rect.width,rect.height);
					if(frameObj.ContainKey("frameX")){
						frameSize.x = frameObj["frameX"].AsFloat*textureScale;
					}
					if(frameObj.ContainKey("frameY")){
						frameSize.y = frameObj["frameY"].AsFloat*textureScale;
					}
					if(frameObj.ContainKey("frameWidth")){
						frameSize.width = frameObj["frameWidth"].AsFloat*textureScale;
					}
					if(frameObj.ContainKey("frameHeight")){
						frameSize.height = frameObj["frameHeight"].AsFloat*textureScale;
					}
					frame.rect = rect;
					frame.frameSize = frameSize;
					frames[i] = frame;
				}
			}
		}
		#endif

		/// <summary>
		/// Get TextureFrame By Name
		/// </summary>
		/// <returns>The frame by name.</returns>
		/// <param name="name">Name.</param>
		public TextureFrame GetFrameByName(string name){
			if(frames!=null){
				int len = frames.Length;
				for(int i=0;i<len;++i){
					TextureFrame frame = frames[i];
					if(frame.name.Equals(name)){
						return frame;
					}
				}
			}
			return null;
		}

		#if UNITY_EDITOR
		void OnDrawGizmos(){
			if(!Application.isPlaying && m_frame!=null && Selection.activeTransform){
				if(Selection.activeTransform==this.transform||Selection.activeTransform.parent==this.transform){
					Gizmos.color = Color.red;
					Matrix4x4 cubeTransform = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
					Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
					Gizmos.matrix *= cubeTransform;
					Gizmos.DrawWireCube(new Vector3(
						-m_pivot.x*m_frame.frameSize.width*0.01f+m_frame.frameSize.width*0.005f,
						-m_pivot.y*m_frame.frameSize.height*0.01f+m_frame.frameSize.height*0.005f,
						0
					),new Vector3(m_frame.frameSize.width*0.01f,m_frame.frameSize.height*0.01f,0.1f));
					Gizmos.matrix = oldGizmosMatrix;
				}
			}
		}
		#endif
	}
}