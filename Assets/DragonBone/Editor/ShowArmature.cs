using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

namespace DragonBone
{
	/// <summary>
	/// 创建和显示Unity节点树
	/// author:  bingheliefeng
	/// </summary>
	public class ShowArmature {

		private static Transform m_rootBone;

		public static void AddBones(ArmatureEditor armatureEditor){
			if(armatureEditor.armatureData.boneDatas!=null)
			{
				armatureEditor.bonesKV.Clear();
				armatureEditor.bones.Clear();
				int len = armatureEditor.armatureData.boneDatas.Length;
				for(int i=0;i<len;++i){
					DragonBoneData.BoneData boneData = armatureEditor.armatureData.boneDatas[i];
					GameObject go = new GameObject(boneData.name);
					armatureEditor.bonesKV[boneData.name]=go.transform;
					if(m_rootBone==null) m_rootBone = go.transform;
					armatureEditor.bones.Add(go.transform);
				}
			}
		}
		public static void AddSlot(ArmatureEditor armatureEditor){
			if(armatureEditor.armatureData.slotDatas!=null){
				armatureEditor.slotsKV.Clear();
				DragonBoneArmature armature = armatureEditor.armature.GetComponent<DragonBoneArmature>();
				int len = armatureEditor.armatureData.slotDatas.Length;
				for(int i=0;i<len;++i){
					DragonBoneData.SlotData slotData = armatureEditor.armatureData.slotDatas[i];
					GameObject go = new GameObject(slotData.name);
					armatureEditor.slotsKV[slotData.name]=go.transform;

					Slot slot = go.AddComponent<Slot>();
					slot.zOrder = i;
					slot.armature = armature;
					slot.blendMode = slot.ConvertBlendMode( slotData.blendMode.ToLower());
					armatureEditor.slots.Add(slot);
				}
			}
		}
		public static void ShowBones(ArmatureEditor armatureEditor){

			foreach(Transform b in armatureEditor.bonesKV.Values)
			{
				DragonBoneData.BoneData boneData= armatureEditor.bonesDataKV[b.name];

				if(!string.IsNullOrEmpty(boneData.parent)){
					if(armatureEditor.bonesKV.ContainsKey(boneData.parent)){
						Transform parent = armatureEditor.bonesKV[boneData.parent];
						b.transform.parent = parent.transform;
					}
				}
				else
				{
					b.transform.parent = armatureEditor.armature;
				}
				if(boneData.transform!=null){

					Vector3 localPos = Vector3.zero;
					if(!float.IsNaN(boneData.transform.x)) localPos.x = boneData.transform.x;
					if(!float.IsNaN(boneData.transform.y)) localPos.y = boneData.transform.y;
					b.transform.localPosition = localPos;

					Vector3 localSc = Vector3.one;
					if(!float.IsNaN(boneData.transform.scx)) localSc.x = boneData.transform.scx;
					if(!float.IsNaN(boneData.transform.scy)) localSc.y = boneData.transform.scy;

					b.transform.localScale = localSc;

					if(!float.IsNaN(boneData.transform.rotate))
					{
						b.transform.localRotation = Quaternion.Euler(0,0,boneData.transform.rotate);
					}

				}else{
					b.transform.localScale = Vector3.one;
					b.transform.localPosition = Vector3.zero;
				}
			}
			//设置inheritRotation，inheritScale ( not support)
			foreach(Transform b in armatureEditor.bonesKV.Values)
			{
				DragonBoneData.BoneData boneData= armatureEditor.bonesDataKV[b.name];
				if(!boneData.inheritRotation){
					Transform parent = b.parent;
					Vector3 rotate = Vector3.zero;
					while(parent!=armatureEditor.armature){
						rotate+=parent.localEulerAngles;
						parent=parent.parent;
					}
					b.localEulerAngles -= rotate;
				}
				if(!boneData.inheritScale){
					Transform parent = b.parent;
					Vector3 scale = Vector3.one;
					while(parent!=armatureEditor.armature){
						scale= new Vector3(scale.x*parent.localScale.x,scale.y*parent.localScale.y,scale.z*parent.localScale.z);
						parent=parent.parent;
					}
					b.localScale= new Vector3(b.localScale.x/scale.x,b.localScale.y/scale.y,b.localScale.z/scale.z);
				}
			}
		}
		public static void ShowSlots(ArmatureEditor armatureEditor){
			foreach(Transform s in armatureEditor.slotsKV.Values)
			{
				DragonBoneData.SlotData slotData = armatureEditor.slotsDataKV[s.name];
				if(!string.IsNullOrEmpty(slotData.parent)){
					if(armatureEditor.bonesKV.ContainsKey(slotData.parent)){
						Transform parent = armatureEditor.bonesKV[slotData.parent];
						s.transform.parent = parent.transform;
					}
				}
				else
				{
					s.transform.parent = armatureEditor.armature;
				}
				s.transform.localScale = new Vector3(slotData.scale,slotData.scale,1f);
				s.transform.localPosition = new Vector3(0,0,slotData.z);
				s.transform.localEulerAngles = Vector3.zero;
			}
		}
		public static void ShowSkins(ArmatureEditor armatureEditor){
			if(armatureEditor.armatureData.skinDatas!=null && armatureEditor.armatureData.skinDatas.Length>0){
				DragonBoneArmature armature= armatureEditor.armature.GetComponent<DragonBoneArmature>();
				Dictionary<Texture2D,Material> matKV = new Dictionary<Texture2D, Material>();
				List<Material> mats = new List<Material>();
				//创建贴图集的材质
				Material atlasMat = null;
				if(armatureEditor.altasTexture){

					Material mat = null;
					string path = AssetDatabase.GetAssetPath(armatureEditor.altasTexture);
					path = path.Substring(0,path.LastIndexOf('.'))+"_Mat.mat";
					mat = AssetDatabase.LoadAssetAtPath<Material>(path);
					if(!mat){
						mat = new Material(Shader.Find("DragonBone/DragonBone Simple"));
						AssetDatabase.CreateAsset(mat,path);
					}
					mat.mainTexture = armatureEditor.altasTexture;
					matKV[armatureEditor.altasTexture] = mat;
					atlasMat=mat;
					mats.Add(mat);
				}

				if(armatureEditor.otherTextures!=null && armatureEditor.otherTextures.Length>0){
					for(int r=0;r<armatureEditor.otherTextures.Length;++r){
						ArmatureEditor.Atlas atlas = armatureEditor.otherTextures[r];
						Material mat = null;

						string path = AssetDatabase.GetAssetPath(atlas.texture);
						path = path.Substring(0,path.LastIndexOf('.'))+"_Mat.mat";
						mat = AssetDatabase.LoadAssetAtPath<Material>(path);
						if(!mat){
							mat = new Material(Shader.Find("DragonBone/DragonBone Simple"));
							AssetDatabase.CreateAsset(mat,path);
						}
						mat.mainTexture = atlas.texture;
						matKV[atlas.texture] = mat;
						mats.Add(mat);
					}
				}

				//create Frames
				Dictionary<Texture2D,SpriteFrame> frameKV = new Dictionary<Texture2D, SpriteFrame>();
				List<TextureFrame> tfs = new List<TextureFrame>();

				SpriteFrame frame = null;
				if(armatureEditor.altasTextAsset!=null){
					GameObject go = new GameObject();
					SpriteFrame sf = go.AddComponent<SpriteFrame>();
					sf.atlasMat = atlasMat;
					sf.atlasText = armatureEditor.altasTextAsset;
					sf.ParseAtlasText();
					sf.CreateQuad();
					frameKV[armatureEditor.altasTexture] = sf;
					frame = sf;
					tfs.AddRange(frame.frames);
				}
				if(armatureEditor.otherTextures!=null && armatureEditor.otherTextures.Length>0)
				{
					for(int r=0;r<armatureEditor.otherTextures.Length;++r){
						ArmatureEditor.Atlas atlas = armatureEditor.otherTextures[r];
						GameObject go = new GameObject();
						SpriteFrame sf = go.AddComponent<SpriteFrame>();
						sf.atlasMat = matKV[atlas.texture];
						sf.atlasText = atlas.atlasText;
						sf.ParseAtlasText();
						sf.CreateQuad();
						frameKV[atlas.texture] = sf;
						tfs.AddRange(sf.frames);
					}
				}

				List<SpriteMetaData> metaDatas = new List<SpriteMetaData>();
				List<SpriteRenderer> sprites = new List<SpriteRenderer>();

				int meshSpriteCount = 0;
				int len = armatureEditor.armatureData.skinDatas.Length;
				for(int i=0;i<len;++i){
					DragonBoneData.SkinData skinData = armatureEditor.armatureData.skinDatas[i];
					for(int j=0;j<skinData.slots.Length;++j){
						DragonBoneData.SkinSlotData skinSlotData = skinData.slots[j];
						Transform slot = armatureEditor.slotsKV[skinSlotData.slotName];
						DragonBoneData.SlotData slotData = armatureEditor.slotsDataKV[skinSlotData.slotName];
						if(slot && skinSlotData.displays!=null && skinSlotData.displays.Length>0){
							for(int k=0;k<skinSlotData.displays.Length;++k){
								DragonBoneData.SkinSlotDisplayData displayData= skinSlotData.displays[k];
								if(displayData.type!="image" && displayData.type!="mesh")  continue;

								ArmatureEditor.Atlas atlas = armatureEditor.GetAtlasByTextureName(displayData.textureName);
								if(!armatureEditor.isSingleSprite){
									atlasMat = matKV[atlas.texture];
									frame = frameKV[atlas.texture];
								}

								if(displayData.type=="image"){
									if(armatureEditor.useUnitySprite){
										if(armatureEditor.isSingleSprite)
										{
											Sprite sprite = armatureEditor.spriteKV[displayData.textureName];
											string spritePath = AssetDatabase.GetAssetPath(sprite);
											if(displayData.pivot.x!=0 || displayData.pivot.y!=0 ){
												TextureImporter textureImporter = AssetImporter.GetAtPath(spritePath) as TextureImporter;
												textureImporter.textureType = TextureImporterType.Sprite;
												textureImporter.spriteImportMode = SpriteImportMode.Single;
												textureImporter.spritePixelsPerUnit = 100;
												textureImporter.spritePivot=new Vector2((displayData.pivot.x+sprite.rect.width/2)/sprite.rect.width,(displayData.pivot.y+sprite.rect.height/2)/sprite.rect.height);
												AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);
											}
											sprites.Add(ShowUnitySpriteSingle(sprite,displayData,slot,slotData));
										}
										else
										{
											SpriteMetaData metaData = new SpriteMetaData();
											metaData.name = displayData.textureName;
											metaData.rect = frame.GetFrameByName(displayData.textureName).rect;
											metaData.rect.y = armatureEditor.altasTexture.height-metaData.rect.y-metaData.rect.height;
											if(displayData.pivot.x!=0 || displayData.pivot.y!=0 ){
												metaData.alignment = (int)SpriteAlignment.Custom;
												metaData.pivot = new Vector2((displayData.pivot.x+metaData.rect.width/2)/metaData.rect.width,(displayData.pivot.y+metaData.rect.height/2)/metaData.rect.height);
											}
											metaDatas.Add(metaData);
											sprites.Add(ShowUnitySprite(atlasMat,displayData,slot,metaData,slotData));
										}
									}
									else
									{
										ShowSpriteFrame(frame,atlasMat,displayData,slot,slotData);
									}
								}
								else if(displayData.type=="mesh")
								{
									TextureFrame textureFrame = new TextureFrame();
									if(armatureEditor.isSingleSprite)
									{
										Sprite sprite = armatureEditor.spriteKV[displayData.textureName];
										textureFrame.name = displayData.textureName;
										textureFrame.frameSize = sprite.rect;
										textureFrame.rect = sprite.rect;
										textureFrame.atlasTextureSize = new Vector2(sprite.rect.width,sprite.rect.height);

										string path = AssetDatabase.GetAssetPath(sprite);
										string materialFolder = path.Substring(0,path.LastIndexOf("/"));
										materialFolder = materialFolder.Substring(0,materialFolder.LastIndexOf("/"))+"/Materials/";
										if(!Directory.Exists(materialFolder)){
											Directory.CreateDirectory(materialFolder);
										}
										string matPath = materialFolder+sprite.name+"_Mat.mat";
										atlasMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
										if(!atlasMat){
											atlasMat = new Material(Shader.Find("DragonBone/DragonBone Simple"));
											AssetDatabase.CreateAsset(atlasMat,matPath);
										}
										atlasMat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture>(path);

										mats.Add(atlasMat);
										textureFrame.material = atlasMat;
										textureFrame.texture = atlasMat.mainTexture;
										tfs.Add(textureFrame);
									}
									else
									{
										foreach(TextureFrame st in frame.frames){
											if(st.name.Equals(displayData.textureName)){
												textureFrame = st;
												break;
											}
										}
									}
									if(textureFrame.rect.width>0&&textureFrame.rect.height>0 && atlasMat && atlasMat.mainTexture){
										ShowSpriteMesh(textureFrame,atlasMat,displayData,slot,armatureEditor,slotData);
										++meshSpriteCount;
									}
								}
							}
							Renderer[] renders = slot.GetComponentsInChildren<Renderer>();
							if(slotData.displayIndex==-1){
								foreach(Renderer render in renders){
									render.enabled = false;
								}
							}
							else
							{
								for(int p=0;p<renders.Length;++p){
									if(p!=slotData.displayIndex){
										renders[p].enabled=false;
									}else{
										renders[p].enabled=true;
									}
								}
							}
						}
					}
				}
				armature.materials = mats.ToArray();
				armature.textureFrames = tfs.ToArray();

				foreach(SpriteFrame sf in frameKV.Values)
				{
					GameObject.DestroyImmediate(sf.gameObject);
				}

				if(armatureEditor.useUnitySprite){
					if(!armatureEditor.isSingleSprite){
						if(metaDatas.Count>0){
							string textureAtlasPath = AssetDatabase.GetAssetPath(armatureEditor.altasTexture);
							TextureImporter textureImporter = AssetImporter.GetAtPath(textureAtlasPath) as TextureImporter;
							textureImporter.maxTextureSize = 2048;
							textureImporter.spritesheet = metaDatas.ToArray();
							textureImporter.textureType = TextureImporterType.Sprite;
							textureImporter.spriteImportMode = SpriteImportMode.Multiple;
							textureImporter.spritePixelsPerUnit = 100;
							AssetDatabase.ImportAsset(textureAtlasPath, ImportAssetOptions.ForceUpdate);
							Object[] savedSprites = AssetDatabase.LoadAllAssetsAtPath(textureAtlasPath);
							foreach(Object obj in savedSprites){
								Sprite objSprite = obj as Sprite;
								if(objSprite){
									len = sprites.Count;
									for(int i=0;i<len;++i){
										if(sprites[i].name.Equals(objSprite.name)){
											sprites[i].sprite = objSprite;
										}
									}
								}
							}
						}
					}
					if(atlasMat!=null){
						if(meshSpriteCount==0 )
						{
							//can delete safely
							foreach(Material mat in matKV.Values){
								AssetDatabase.DeleteAsset( AssetDatabase.GetAssetPath(mat));
							}
						}
						else
						{
							foreach(SpriteRenderer sprite in sprites){
								sprite.material = atlasMat;	
							}
						}
					}
				}
			}

		}

		static void ShowSpriteFrame(SpriteFrame frame,Material mat,DragonBoneData.SkinSlotDisplayData displayData,Transform slot,DragonBoneData.SlotData slotData){
			SpriteFrame newFrame = (SpriteFrame)GameObject.Instantiate(frame);
			newFrame.atlasMat = mat;
			newFrame.CreateQuad();
			newFrame.frameName = displayData.textureName;
			newFrame.name = displayData.textureName;
			newFrame.pivot = displayData.pivot;
			newFrame.transform.parent = slot;

			Vector3 localPos = Vector3.zero;
			if(!float.IsNaN(displayData.transform.x)) localPos.x = displayData.transform.x;
			if(!float.IsNaN(displayData.transform.y)) localPos.y = displayData.transform.y;
			newFrame.transform.localPosition = localPos;

			Vector3 localSc = Vector3.one;
			if(!float.IsNaN(displayData.transform.scx)) localSc.x = displayData.transform.scx;
			if(!float.IsNaN(displayData.transform.scy)) localSc.y = displayData.transform.scy;
			newFrame.transform.localScale = localSc;

			Color c = Color.white;
			if(slotData.color!=null){
				c.a = slotData.color.aM+slotData.color.a0;
				c.r = slotData.color.rM+slotData.color.r0;
				c.g = slotData.color.gM+slotData.color.g0;
				c.b = slotData.color.bM+slotData.color.b0;
				newFrame.color = c;
			}

			if(!float.IsNaN(displayData.transform.rotate))
				newFrame.transform.localRotation = Quaternion.Euler(0,0,displayData.transform.rotate);
		}

		static SpriteRenderer ShowUnitySprite(Material mat,DragonBoneData.SkinSlotDisplayData displayData,Transform slot,SpriteMetaData metaData,DragonBoneData.SlotData slotData){
			Sprite sprite = Sprite.Create((Texture2D)mat.mainTexture,metaData.rect,metaData.pivot,100f,0,SpriteMeshType.Tight);
			return ShowUnitySpriteSingle(sprite,displayData,slot,slotData);
		}

		static SpriteRenderer ShowUnitySpriteSingle( Sprite sprite,DragonBoneData.SkinSlotDisplayData displayData,Transform slot,DragonBoneData.SlotData slotData)
		{
			GameObject go = new GameObject(displayData.textureName);
			SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
			renderer.sprite = sprite;
			go.transform.parent = slot;

			Vector3 localPos = Vector3.zero;
			if(!float.IsNaN(displayData.transform.x)) localPos.x = displayData.transform.x;
			if(!float.IsNaN(displayData.transform.y)) localPos.y = displayData.transform.y;
			go.transform.localPosition = localPos;

			Vector3 localSc = Vector3.one;
			if(!float.IsNaN(displayData.transform.scx)) localSc.x = displayData.transform.scx;
			if(!float.IsNaN(displayData.transform.scy)) localSc.y = displayData.transform.scy;
			go.transform.localScale = localSc;

			Color c = Color.white;
			if(slotData.color!=null){
				c.a = slotData.color.aM+slotData.color.a0;
				c.r = slotData.color.rM+slotData.color.r0;
				c.g = slotData.color.gM+slotData.color.g0;
				c.b = slotData.color.bM+slotData.color.b0;
				renderer.color = c;
			}

			if(!float.IsNaN(displayData.transform.rotate))
				go.transform.localRotation = Quaternion.Euler(0,0,displayData.transform.rotate);
			return renderer;
		}

		static void ShowSpriteMesh(TextureFrame frame,Material mat,DragonBoneData.SkinSlotDisplayData displayData,Transform slot,ArmatureEditor armatureEditor,DragonBoneData.SlotData slotData){
	
			GameObject go = new GameObject(displayData.textureName);
			SpriteMesh sm = go.AddComponent<SpriteMesh>();
			sm.atlasMat = mat;
			sm.vertices = displayData.vertices;
			sm.frame = frame;
			sm.uvs = displayData.uvs;
			sm.triangles = displayData.triangles;
			sm.colors = new Color[sm.vertices.Length];
			for(int i =0;i<sm.colors.Length;++i){
				sm.colors[i] = Color.white;
			}
			if(displayData.weights!=null && displayData.weights.Length>0){
				sm.CreateMesh(true);
				if(armatureEditor.ffdKV.ContainsKey(displayData.textureName)){
					//Vertex controller
					sm.vertControlTrans = new Transform[sm.vertices.Length];
					for(int i=0;i<sm.vertices.Length;++i){
						GameObject gov = new GameObject(go.name+"_v"+i);
						gov.transform.parent = go.transform;
						gov.transform.localPosition = sm.vertices[i];
						gov.transform.localScale = Vector3.zero;
						sm.vertControlTrans[i] = gov.transform;
					}
				}
			}
			else
			{
				sm.CreateMesh(false);
				if(displayData.bonePose==null){
					//Vertex controller
					sm.vertControlTrans = new Transform[sm.vertices.Length];
					for(int i=0;i<sm.vertices.Length;++i){
						GameObject gov = new GameObject(go.name+"_v"+i);
						gov.transform.parent = go.transform;
						gov.transform.localPosition = sm.vertices[i];
						gov.transform.localScale = Vector3.zero;
						sm.vertControlTrans[i] = gov.transform;
					}
				}
			}
			sm.transform.parent = slot;

			if(displayData.bonePose!=null){
				SkinnedMeshRenderer skinnedMesh = sm.GetComponent<SkinnedMeshRenderer>();
				if(skinnedMesh){
					skinnedMesh.quality = SkinQuality.Bone4;
					if(displayData.weights!=null&&displayData.weights.Length>0){
						Transform[] bones = new Transform[displayData.bonePose.Length/7];
						for(int i=0;i<displayData.bonePose.Length;i+=7)
						{
							int index = i/7;
							int boneIndex = (int)displayData.bonePose[i];
							bones[index] = armatureEditor.bones[boneIndex];
						}

						List<BoneWeight> boneWeights = new List<BoneWeight>();
						for(int i=0;i<displayData.weights.Length;++i)
						{
							int boneCount = (int)displayData.weights[i];//骨骼数量

							List<KeyValuePair<int ,float>> boneWeightList = new List<KeyValuePair<int, float>>();
							for(int j=0;j<boneCount*2;j+=2){
								int boneIdx = (int)displayData.weights[i+j+1];
								float weight = displayData.weights[i+j+2];
								boneWeightList.Add(new KeyValuePair<int, float>(boneIdx,weight));
							}
							//sort boneWeightList，desc
							boneWeightList.Sort(delegate(KeyValuePair<int, float> x, KeyValuePair<int, float> y) {
								if(x.Value==y.Value) return 0;
								return x.Value<y.Value? 1: -1;
							});
							BoneWeight bw = new BoneWeight();
							for(int j=0;j<boneWeightList.Count;++j){
								if(j==0){
									bw.boneIndex0 = GlobalBoneIndexToLocalBoneIndex(armatureEditor, boneWeightList[j].Key,bones);
									bw.weight0 = boneWeightList[j].Value;
								}else if(j==1){
									bw.boneIndex1 = GlobalBoneIndexToLocalBoneIndex(armatureEditor, boneWeightList[j].Key,bones);
									bw.weight1 = boneWeightList[j].Value;
								}else if(j==2){
									bw.boneIndex2 = GlobalBoneIndexToLocalBoneIndex(armatureEditor, boneWeightList[j].Key,bones);
									bw.weight2 = boneWeightList[j].Value;
								}else if(j==3){
									bw.boneIndex3 = GlobalBoneIndexToLocalBoneIndex(armatureEditor, boneWeightList[j].Key,bones);
									bw.weight3 = boneWeightList[j].Value;
								}
							}
							boneWeights.Add(bw);
							i+=boneCount*2;
						}
						Matrix4x4[] matrixArray = new Matrix4x4[bones.Length];
						for(int i=0;i<matrixArray.Length;++i){
							Transform bone = bones[i];
							Vector3 bonePos = bone.localPosition;
							Quaternion boneRotate = bone.localRotation;

							Matrix2D m2d= armatureEditor.bonePoseKV[bone.name];
							bone.position = new Vector3(m2d.tx*0.01f,-m2d.ty*0.01f,bone.position.z);
							bone.rotation = Quaternion.Euler(0f,0f,-m2d.GetAngle());

							matrixArray[i] = bone.worldToLocalMatrix*armatureEditor.armature.localToWorldMatrix;
							matrixArray[i] *= Matrix4x4.TRS(slot.localPosition,slot.localRotation,slot.localScale);

							bone.localPosition = bonePos;
							bone.localRotation = boneRotate;
						}
						skinnedMesh.bones=bones;
						skinnedMesh.sharedMesh.boneWeights = boneWeights.ToArray();
						skinnedMesh.sharedMesh.bindposes = matrixArray;
						skinnedMesh.rootBone = slot;
						sm.bindposes = matrixArray;
						SpriteMesh.BoneWeightClass[] bwcs = new SpriteMesh.BoneWeightClass[boneWeights.Count];
						for(int i=0;i<boneWeights.Count;++i){
							SpriteMesh.BoneWeightClass bwc = new SpriteMesh.BoneWeightClass();
							BoneWeight bw = boneWeights[i];
							bwc.boneIndex0 = bw.boneIndex0;
							bwc.boneIndex1 = bw.boneIndex1;
							bwc.boneIndex2 = bw.boneIndex2;
							bwc.boneIndex3 = bw.boneIndex3;
							bwc.weight0 = bw.weight0;
							bwc.weight1 = bw.weight1;
							bwc.weight2 = bw.weight2;
							bwc.weight3 = bw.weight3;
							bwcs[i] = bwc;
						}
						sm.weights = bwcs;
					}
				}
			}
			DragonBoneData.TransformData tranform = displayData.transform ;
			Vector3 localPos = Vector3.zero;
			if(!float.IsNaN(tranform.x)) localPos.x = tranform.x;
			if(!float.IsNaN(tranform.y)) localPos.y = tranform.y;
			sm.transform.localPosition = localPos;

			Vector3 localSc = Vector3.one;
			if(!float.IsNaN(tranform.scx)) localSc.x = tranform.scx;
			if(!float.IsNaN(tranform.scy)) localSc.y = tranform.scy;
			sm.transform.localScale = localSc;

			Color c = Color.white;
			if(slotData.color!=null){
				c.a = slotData.color.aM+slotData.color.a0;
				c.r = slotData.color.rM+slotData.color.r0;
				c.g = slotData.color.gM+slotData.color.g0;
				c.b = slotData.color.bM+slotData.color.b0;
				sm.color = c;
			}
			sm.transform.localRotation = Quaternion.Euler(0,0,tranform.rotate);
		}

		//全局BoneIndex转 特定数组中的BoneIndex
		static int GlobalBoneIndexToLocalBoneIndex( ArmatureEditor armatureEditor,int globalBoneIndex,Transform[] localBones){
			Transform globalBone = armatureEditor.bones[globalBoneIndex];
			int len = localBones.Length;
			for(int i=0;i<len;++i){
				if(localBones[i] == globalBone) return i;
			}
			return globalBoneIndex;
		}

		public static void SetIKs(ArmatureEditor armatureEditor){
			if(armatureEditor.armatureData.ikDatas!=null){
				int len = armatureEditor.armatureData.ikDatas.Length;
				for(int i=0;i<len;++i){
					DragonBoneData.IKData ikData = armatureEditor.armatureData.ikDatas[i];
					Transform ikTrans = armatureEditor.bonesKV[ikData.target];
					Transform targetBone = armatureEditor.bonesKV[ikData.bone];
					DragonBoneData.BoneData targetBoneData = armatureEditor.bonesDataKV[ikData.bone];
					Transform parentBone = targetBone;
					int y = ikData.chain;
					while(--y>-1){
						parentBone = parentBone.parent;
					}
					BoneIK bi = parentBone.gameObject.AddComponent<BoneIK>();

					Vector3 v = Vector3.right * targetBoneData.length*0.01f;
					v = targetBone.TransformPoint(v);
					GameObject go = new GameObject(ikData.name);
					go.transform.parent = targetBone;
					go.transform.position = v;

					bi.damping = ikData.weight;
					bi.endTransform = go.transform;
					bi.targetIK = ikTrans;
					bi.iterations = 20;
					bi.bendPositive = ikData.bendPositive;
				}
			}
		}
	}

}