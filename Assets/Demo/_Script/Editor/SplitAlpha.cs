using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// RGB+A
/// </summary>
public class SplitAlpha {

	[MenuItem("Tools/Split Alpha",false,2)]
	static void Split(){
		if(Selection.activeObject && Selection.activeObject is Texture2D)
		{
			string name = (Selection.activeObject as Texture2D).name;
			string folder = AssetDatabase.GetAssetPath(Selection.activeObject);
			folder = Application.dataPath.Substring(0,Application.dataPath.LastIndexOf('/')+1) + folder.Substring(0,folder.LastIndexOf('/')+1);
			Texture2D t = new Texture2D(1,1,TextureFormat.ARGB32,false,true);
			t.filterMode = FilterMode.Trilinear;
			t.LoadImage(File.ReadAllBytes(folder+name+".png"));

			Texture2D jpg = new Texture2D(t.width,t.height,TextureFormat.RGB24,false,true);
			Texture2D alphaT = new Texture2D(t.width,t.height,TextureFormat.RGB24,false,true);

			for(int i=0;i<t.width;i++)
			{
				for(int j=0;j<t.height;j++)
				{
					Color c = t.GetPixel(i,j);
					jpg.SetPixel(i,j,c);
					if(c.a==0){
						if(i>0 && t.GetPixel(i-1,j).a>0) jpg.SetPixel(i,j,t.GetPixel(i-1,j));
						else if(i<t.width-1 && t.GetPixel(i+1,j).a>0 ) jpg.SetPixel(i,j,t.GetPixel(i+1,j));
						else if(j>0 && t.GetPixel(i,j-1).a>0 ) jpg.SetPixel(i,j,t.GetPixel(i,j-1));
						else if(j<t.height-1 && t.GetPixel(i,j+1).a>0  ) jpg.SetPixel(i,j,t.GetPixel(i,j+1));
					}
					alphaT.SetPixel(i,j,new Color(c.a,c.a,c.a));
				}
			}

			if(t.width>t.height)
			{
				//merge
				Texture2D atlasT = new Texture2D(t.width,t.width,TextureFormat.RGB24,false,true);
				atlasT.SetPixels(0,0,t.width,t.height,jpg.GetPixels());
				atlasT.SetPixels(0,t.height,t.width,t.height,alphaT.GetPixels());
				File.WriteAllBytes(folder+name+"_RGB_A.jpg",atlasT.EncodeToJPG(100));
			}
			else if(t.width<t.height)
			{
				//merge
				Texture2D atlasT = new Texture2D(t.height,t.height,TextureFormat.RGB24,false,true);
				atlasT.SetPixels(0,0,t.width,t.height,jpg.GetPixels());
				atlasT.SetPixels(t.width,0,t.width,t.height,alphaT.GetPixels());
				File.WriteAllBytes(folder+name+"_RGB_A.jpg",atlasT.EncodeToJPG(100));

			}else{
				byte[] alphaBytes = alphaT.EncodeToJPG(100);
				File.WriteAllBytes(folder+name+"_A.jpg",alphaBytes);

				byte[] jpgbytes = jpg.EncodeToJPG(100);
				File.WriteAllBytes(folder+name+"_RGB.jpg",jpgbytes);
			}
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
		}
	}

}
