using UnityEngine;

namespace DragonBone{
	[System.Serializable]
	public class TextureFrame{
		public string name;
		public Texture texture;
		public Rect rect;//Texture Size
		public Rect frameSize;//Real Size
		public Material material;
		public Vector2 atlasTextureSize;
		public bool isRotated = false;

		//偏移
		public Vector3 frameOffset{
			get{
				Vector3 v = Vector3.zero;
				v.x = (rect.width-frameSize.width)*0.5f-frameSize.x;
				v.y = (frameSize.height-rect.height)*0.5f+frameSize.y;
				return v;
			}
		}
	}
}