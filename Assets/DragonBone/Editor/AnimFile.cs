using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using CurveExtended;

namespace DragonBone
{
	/// <summary>
	/// generate Animation file
	/// author:  bingheliefeng
	/// </summary>
	public class AnimFile {


		private static Dictionary<string,SpriteFrame> changedSpriteFramesKV = null;
		private static Dictionary<string,SpriteMesh> changedSpriteMeshsKV = null;

		public static void CreateAnimFile(ArmatureEditor armatureEditor)
		{
			changedSpriteFramesKV=  new Dictionary<string, SpriteFrame>();
			changedSpriteMeshsKV =  new Dictionary<string, SpriteMesh>();

			string path = AssetDatabase.GetAssetPath(armatureEditor.animTextAsset);
			path = path.Substring(0,path.LastIndexOf('/'))+"/Anims";
			if(!AssetDatabase.IsValidFolder(path)){
				Directory.CreateDirectory(path);
			}
			path+="/";

			Animator animator= armatureEditor.armature.gameObject.AddComponent<Animator>();
			AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path+armatureEditor.armature.name+".controller");
			AnimatorStateMachine rootStateMachine = null;
			if(controller==null){
				controller = AnimatorController.CreateAnimatorControllerAtPath(path+armatureEditor.armature.name+".controller");
				rootStateMachine = controller.layers[0].stateMachine;
			}
			animator.runtimeAnimatorController = controller;
			if(armatureEditor.armatureData.animDatas!=null)
			{
				for(int i=0;i<armatureEditor.armatureData.animDatas.Length ;++i)
				{
					DragonBoneData.AnimationData animationData = armatureEditor.armatureData.animDatas[i];
					AnimationClip clip = new AnimationClip();
					clip.name = animationData.name;
					clip.frameRate = armatureEditor.armatureData.frameRate;

					CreateAnimBoneAndSlot(armatureEditor ,clip,animationData.boneDatas , armatureEditor.bonesKV , true);
					CreateAnimBoneAndSlot(armatureEditor ,clip,animationData.slotDatas , armatureEditor.slotsKV , false);
					CreateAnimBoneAndSlot(armatureEditor ,clip,animationData.ffdDatas , armatureEditor.slotsKV , false,true);
					SetDragonBoneArmature(armatureEditor);
					SetEvent(armatureEditor,clip,animationData.keyDatas);

					SerializedObject serializedClip = new SerializedObject(clip);
					AnimationClipSettings clipSettings = new AnimationClipSettings(serializedClip.FindProperty("m_AnimationClipSettings"));
					clipSettings.loopTime = animationData.playTimes==0;
					serializedClip.ApplyModifiedProperties();

					AssetDatabase.CreateAsset(clip,path+clip.name+".anim");
					if(rootStateMachine!=null){
						AnimatorState state = rootStateMachine.AddState(clip.name);
						state.motion = clip;
					}
				}
				AssetDatabase.SaveAssets();
			}
			if(rootStateMachine!=null && rootStateMachine.states!=null && rootStateMachine.states.Length>0){
				rootStateMachine.defaultState= rootStateMachine.states[0].state;
			}

			//createAvatar
			if(armatureEditor.createAvatar)
				CreateAvatar(armatureEditor,animator,path);
		}
		static void CreateAvatar( ArmatureEditor armatureEditor,Animator animator,string path){
			Avatar avatar = AvatarBuilder.BuildGenericAvatar(armatureEditor.armature.gameObject,"");
			animator.avatar = avatar;
			AvatarMask avatarMask = new AvatarMask();
			string[] transofrmPaths = GetTransformPaths(armatureEditor);
			avatarMask.transformCount = transofrmPaths.Length;
			for (int i=0; i< transofrmPaths.Length; i++){
				avatarMask.SetTransformPath(i, transofrmPaths[i]);
				avatarMask.SetTransformActive(i, true);
			}
			AssetDatabase.CreateAsset(avatar    , path + "/" + armatureEditor.armature.name + "Avatar.asset");
			AssetDatabase.CreateAsset(avatarMask, path + "/" + armatureEditor.armature.name + "Mask.asset");
			AssetDatabase.SaveAssets();
		}

		static string[] GetTransformPaths(ArmatureEditor armatureEditor ){
			List<string> result = new List<string>();
			result.Add("");
			foreach(Transform t in armatureEditor.bones){
				string path = AnimationUtility.CalculateTransformPath(t,armatureEditor.armature);
				result.Add(path);
			}
			return result.ToArray();
		}


		static TangentMode GetPrevFrameTangentMode(float easingTween,float[] curves){
			if(curves!=null && curves.Length>0) return TangentMode.Editable;

			if(easingTween==float.PositiveInfinity){
				return TangentMode.Stepped;
			}
			else if(easingTween==0){
				return TangentMode.Linear;
			}else if(easingTween==1){
				return TangentMode.Linear;
			}else if(easingTween==2){
				return TangentMode.Smooth;
			}
			return TangentMode.Linear;
		}

		static void CreateAnimBoneAndSlot(ArmatureEditor armatureEditor, AnimationClip clip , DragonBoneData.AnimSubData[] subDatas , Dictionary<string,Transform> transformKV , bool boneOrSlot , bool isffd = false)
		{
			for(int i=0;i<subDatas.Length;++i)
			{
				DragonBoneData.AnimSubData animSubData = subDatas[i];
				string name = string.IsNullOrEmpty(animSubData.slot) ? animSubData.name : animSubData.slot;
				Transform node = transformKV[name];
				DragonBoneData.TransformData defaultTransformData = boneOrSlot ? armatureEditor.bonesDataKV[animSubData.name].transform:null;
				float defaultZ = boneOrSlot ? 0: armatureEditor.slotsDataKV[name].z ;;
				DragonBoneData.SlotData defaultSlotData = boneOrSlot ? null:armatureEditor.slotsDataKV[name];
				DragonBoneData.ColorData defaultColorData = boneOrSlot ? null: defaultSlotData.color ;
				AnimationCurve xcurve = new AnimationCurve();
				AnimationCurve ycurve = new AnimationCurve();
				AnimationCurve zcurve = new AnimationCurve();
				AnimationCurve sxcurve = new AnimationCurve();
				AnimationCurve sycurve = new AnimationCurve();
				AnimationCurve color_rcurve = new AnimationCurve();
				AnimationCurve color_gcurve = new AnimationCurve();
				AnimationCurve color_bcurve = new AnimationCurve();
				AnimationCurve color_acurve = new AnimationCurve();
				AnimationCurve rotatecurve = new AnimationCurve();

				Renderer[] renders = node.GetComponentsInChildren<Renderer>();
				AnimationCurve[] renderCurves = new AnimationCurve[renders.Length];
				for(int r=0;r<renderCurves.Length;++r){
					renderCurves[r] = new AnimationCurve();
				}

				List<AnimationCurve[]> vertexcurvexArray = null;
				List<AnimationCurve[]> vertexcurveyArray = null;
				if(isffd && node.childCount>0){
					vertexcurvexArray = new List<AnimationCurve[]>();
					vertexcurveyArray = new List<AnimationCurve[]>();

					for(int j=0;j<node.childCount;++j){
						Transform ffdNode = node.GetChild(j);
						if(ffdNode.name==animSubData.name){
							AnimationCurve[] vertex_xcurves = new AnimationCurve[ffdNode.childCount];
							AnimationCurve[] vertex_ycurves = new AnimationCurve[ffdNode.childCount];
							for(int r=0;r<vertex_xcurves.Length;++r){
								vertex_xcurves[r] = new AnimationCurve();
								vertex_ycurves[r] = new AnimationCurve();
							}
							vertexcurvexArray.Add(vertex_xcurves);
							vertexcurveyArray.Add(vertex_ycurves);
						}
					}

				}

				float during = animSubData.offset;
				float perKeyTime = 1f/armatureEditor.armatureData.frameRate;
				bool isHaveCurve = false;
				for(int j=0;j<animSubData.frameDatas.Length;++j)
				{
					DragonBoneData.AnimFrameData frameData = animSubData.frameDatas[j];

					float prevTweeneasing = float.PositiveInfinity;//前一帧的tweenEasing
					float[] prevCurves = null;
					if(j>0) {
						prevTweeneasing = animSubData.frameDatas[j-1].tweenEasing;
						prevCurves = animSubData.frameDatas[j-1].curve;
					}
					TangentMode tanModeL = GetPrevFrameTangentMode(prevTweeneasing,prevCurves);
					TangentMode tanModeR = TangentMode.Linear;

					if(frameData.curve!=null && frameData.curve.Length>0){
						tanModeR = TangentMode.Editable;
						isHaveCurve = true;
					}else{
						if(frameData.tweenEasing==float.PositiveInfinity){
							tanModeR = TangentMode.Stepped;
						}
						else if(frameData.tweenEasing==0){
							tanModeR = TangentMode.Linear;
						}else if(frameData.tweenEasing==1){
							tanModeR = TangentMode.Smooth;
						}else if(frameData.tweenEasing==2){
							tanModeR = TangentMode.Linear;
						}
					}
					if(frameData.transformData!=null){
						if(!float.IsNaN(frameData.transformData.x)) {
							if(!float.IsNaN(defaultTransformData.x)){
								xcurve.AddKey(KeyframeUtil.GetNew(during,frameData.transformData.x+defaultTransformData.x,tanModeL,tanModeR));
							}else {
								xcurve.AddKey(KeyframeUtil.GetNew(during,frameData.transformData.x,tanModeL,tanModeR));
							}
						}
						else if(!float.IsNaN(defaultTransformData.x)){
							xcurve.AddKey(KeyframeUtil.GetNew(during,defaultTransformData.x,tanModeL,tanModeR));
						}

						if(!float.IsNaN(frameData.transformData.y)) {
							if(!float.IsNaN(defaultTransformData.y)) {
								ycurve.AddKey(KeyframeUtil.GetNew(during,frameData.transformData.y+defaultTransformData.y,tanModeL,tanModeR));
							}else {
								ycurve.AddKey(KeyframeUtil.GetNew(during,frameData.transformData.y,tanModeL,tanModeR));
							}
						}
						else if(!float.IsNaN(defaultTransformData.y))
						{
							ycurve.AddKey(KeyframeUtil.GetNew(during,defaultTransformData.y,tanModeL,tanModeR));
						}

						if(!float.IsNaN(frameData.z)) zcurve.AddKey(new Keyframe(during,frameData.z,float.PositiveInfinity,float.PositiveInfinity));
						else if(!boneOrSlot) zcurve.AddKey(new Keyframe(during,node.localPosition.z,float.PositiveInfinity,float.PositiveInfinity));

						if(!float.IsNaN(frameData.transformData.rotate)) {
							float rotate = frameData.transformData.rotate+defaultTransformData.rotate;
							rotatecurve.AddKey(KeyframeUtil.GetNew(during,rotate,tanModeL,tanModeR));
						}
						else if(!float.IsNaN(defaultTransformData.rotate)){
							rotatecurve.AddKey(KeyframeUtil.GetNew(during,node.localEulerAngles.z,tanModeL,tanModeR));
						}

						if(!float.IsNaN(frameData.transformData.scx)){
							sxcurve.AddKey(KeyframeUtil.GetNew(during,frameData.transformData.scx*defaultTransformData.scx,tanModeL,tanModeR));
						}
						else{
							sxcurve.AddKey(KeyframeUtil.GetNew(during,node.localScale.x,tanModeL,tanModeR));
						}

						if(!float.IsNaN(frameData.transformData.scy)) {
							sycurve.AddKey(KeyframeUtil.GetNew(during,frameData.transformData.scy*defaultTransformData.scy,tanModeL,tanModeR));
						}
						else {
							sycurve.AddKey(KeyframeUtil.GetNew(during,node.localScale.y,tanModeL,tanModeR));
						}

					}
					if(!boneOrSlot){
						if(frameData.color!=null){
							if(defaultColorData==null) defaultColorData = new DragonBoneData.ColorData();
							Color c = new Color(  
								(defaultColorData.rM+defaultColorData.r0)*frameData.color.rM+frameData.color.r0,
								(defaultColorData.gM+defaultColorData.g0)*frameData.color.gM+frameData.color.g0,
								(defaultColorData.bM+defaultColorData.b0)*frameData.color.bM+frameData.color.b0,
								(defaultColorData.aM+defaultColorData.a0)*frameData.color.aM+frameData.color.a0
							);
							color_rcurve.AddKey(KeyframeUtil.GetNew(during,c.r,tanModeL,tanModeR));
							color_gcurve.AddKey(KeyframeUtil.GetNew(during,c.g,tanModeL,tanModeR));
							color_bcurve.AddKey(KeyframeUtil.GetNew(during,c.b,tanModeL,tanModeR));
							color_acurve.AddKey(KeyframeUtil.GetNew(during,c.a,tanModeL,tanModeR));
						}

						if(!isffd){
							//改displyindex
							if(frameData.displayIndex==-1){
								for(int r=0;r<renders.Length;++r){
									renderCurves[r].AddKey( new Keyframe(during,0f,float.PositiveInfinity,float.PositiveInfinity));
								}
							}
							else
							{
								for(int r=0;r<renders.Length;++r){
									if(r!=frameData.displayIndex){
										renderCurves[r].AddKey( new Keyframe(during,0f,float.PositiveInfinity,float.PositiveInfinity));
									}else{
										renderCurves[r].AddKey( new Keyframe(during,1f,float.PositiveInfinity,float.PositiveInfinity));
									}
								}
							}
						}
					}


					//mesh animation
					if(isffd && vertexcurvexArray!=null){
						for(int k=0;k<vertexcurvexArray.Count;++k)
						{
							Transform ffdNode = node.GetChild(k);
							if(ffdNode.name==animSubData.name){
								AnimationCurve[] vertex_xcurves = vertexcurvexArray[k];
								AnimationCurve[] vertex_ycurves = vertexcurveyArray[k];
								int len = ffdNode.childCount;
								if(frameData.vertices!=null && frameData.vertices.Length>0)
								{
									for(int r =0;r<len;++r){
										AnimationCurve vertex_xcurve = vertex_xcurves[r];
										AnimationCurve vertex_ycurve = vertex_ycurves[r];
										Transform vCtr = ffdNode.GetChild(r);//顶点控制点
										if(r>=frameData.offset && frameData.vertices!=null && r-frameData.offset<frameData.vertices.Length){
											Keyframe kfx = KeyframeUtil.GetNew(during,vCtr.localPosition.x+frameData.vertices[r-frameData.offset].x,tanModeL,tanModeR);
											vertex_xcurve.AddKey(kfx);
											Keyframe kfy = KeyframeUtil.GetNew(during,vCtr.localPosition.y+frameData.vertices[r-frameData.offset].y,tanModeL,tanModeR);
											vertex_ycurve.AddKey(kfy);
										}
										else
										{
											Keyframe kfx = KeyframeUtil.GetNew(during,vCtr.localPosition.x,tanModeL,tanModeR);
											vertex_xcurve.AddKey(kfx);
											Keyframe kfy = KeyframeUtil.GetNew(during,vCtr.localPosition.y,tanModeL,tanModeR);
											vertex_ycurve.AddKey(kfy);
										}
									}
								}
								else
								{
									//add default vertex position
									for(int r =0;r<len;++r){
										AnimationCurve vertex_xcurve = vertex_xcurves[r];
										AnimationCurve vertex_ycurve = vertex_ycurves[r];
										Transform vCtr = node.GetChild(k).GetChild(r);//顶点控制点
										Keyframe kfx = KeyframeUtil.GetNew(during,vCtr.localPosition.x,tanModeL,tanModeR);
										vertex_xcurve.AddKey(kfx);
										Keyframe kfy = KeyframeUtil.GetNew(during,vCtr.localPosition.y,tanModeL,tanModeR);
										vertex_ycurve.AddKey(kfy);
									}
								}
							}
						}

					}

					during+= frameData.duration*perKeyTime;
				}

				OptimizesCurve(xcurve);
				OptimizesCurve(ycurve);
				OptimizesCurve(zcurve);
				OptimizesCurve(sxcurve);
				OptimizesCurve(sycurve);
				OptimizesCurve(color_rcurve);
				OptimizesCurve(color_gcurve);
				OptimizesCurve(color_bcurve);
				OptimizesCurve(color_acurve);
				OptimizesCurve(rotatecurve);


				string path = GetNodeRelativePath(armatureEditor,node) ;
				bool localPosFlag = false;
				if(xcurve.keys !=null && xcurve.keys.Length>0 && CheckCurveValid(xcurve,node.localPosition.x)) localPosFlag = true;
				if(ycurve.keys !=null && ycurve.keys.Length>0 && CheckCurveValid(ycurve,node.localPosition.y))  localPosFlag = true;
				if(zcurve.keys !=null && zcurve.keys.Length>0 && CheckCurveValid(zcurve,defaultZ))  localPosFlag = true;
				if(localPosFlag){
					if(isHaveCurve) SetCustomCurveTangents(xcurve,animSubData.frameDatas);
					CurveExtension.UpdateAllLinearTangents(xcurve);
					clip.SetCurve(path,typeof(Transform),"localPosition.x",xcurve);
					if(isHaveCurve) SetCustomCurveTangents(ycurve,animSubData.frameDatas);
					CurveExtension.UpdateAllLinearTangents(ycurve);
					clip.SetCurve(path,typeof(Transform),"localPosition.y",ycurve);
					clip.SetCurve(path,typeof(Transform),"localPosition.z",zcurve);
				}

				bool localSc = false;
				if(sxcurve.keys !=null && sxcurve.keys.Length>0 && CheckCurveValid(sxcurve,defaultTransformData.scx)) localSc=true;
				if(sycurve.keys !=null && sycurve.keys.Length>0 && CheckCurveValid(sycurve,defaultTransformData.scy)) localSc=true;
				if(localSc){
					if(isHaveCurve) SetCustomCurveTangents(sxcurve,animSubData.frameDatas);
					CurveExtension.UpdateAllLinearTangents(sxcurve);
					clip.SetCurve(path,typeof(Transform),"localScale.x",sxcurve);
					if(isHaveCurve) SetCustomCurveTangents(sycurve,animSubData.frameDatas);
					CurveExtension.UpdateAllLinearTangents(sycurve);
					clip.SetCurve(path,typeof(Transform),"localScale.y",sycurve);
				}

				if(rotatecurve.keys !=null && rotatecurve.keys.Length>0 && CheckCurveValid(rotatecurve,defaultTransformData.rotate)){
					if(rotatecurve.length>1){
						for(int f=1;f<rotatecurve.length;++f){
							float prev = rotatecurve.keys[f-1].value;
							float curr = rotatecurve.keys[f].value;
							while ((curr - prev) > 180 ){
								curr -= 360;
							}
							while ((curr - prev) < -180){
								curr += 360;
							}
							if (rotatecurve.keys[f].value != curr){
								rotatecurve.MoveKey(f, new Keyframe(rotatecurve.keys[f].time , curr));
							}
						}
					}
					if(isHaveCurve) SetCustomCurveTangents(rotatecurve,animSubData.frameDatas);
					CurveExtension.UpdateAllLinearTangents(rotatecurve);
					clip.SetCurve(path,typeof(Transform),"localEulerAngles.z",rotatecurve);
				}

				if(!boneOrSlot){
					if(armatureEditor.useUnitySprite)
					{
						SpriteRenderer[] sprites = node.GetComponentsInChildren<SpriteRenderer>();
						if(sprites!=null){
							for(int z=0;z<sprites.Length;++z){
								string childPath = path+"/"+sprites[z].name;
								if(color_rcurve.keys !=null&& color_rcurve.keys.Length>0&& CheckCurveValid(color_rcurve,defaultColorData.rM+defaultColorData.r0)) 
								{
									if(isHaveCurve) SetCustomCurveTangents(color_rcurve,animSubData.frameDatas);
									CurveExtension.UpdateAllLinearTangents(color_rcurve);
									clip.SetCurve(childPath,typeof(SpriteRenderer),"m_Color.r",color_rcurve);
								}

								if(color_gcurve.keys !=null&& color_gcurve.keys.Length>0&& CheckCurveValid(color_gcurve,defaultColorData.gM+defaultColorData.g0)) 
								{
									if(isHaveCurve) SetCustomCurveTangents(color_gcurve,animSubData.frameDatas);
									CurveExtension.UpdateAllLinearTangents(color_gcurve);
									clip.SetCurve(childPath,typeof(SpriteRenderer),"m_Color.g",color_gcurve);
								}

								if(color_bcurve.keys !=null&& color_bcurve.keys.Length>0&& CheckCurveValid(color_bcurve,defaultColorData.bM+defaultColorData.b0)) 
								{
									if(isHaveCurve) SetCustomCurveTangents(color_bcurve,animSubData.frameDatas);
									CurveExtension.UpdateAllLinearTangents(color_bcurve);
									clip.SetCurve(childPath,typeof(SpriteRenderer),"m_Color.b",color_bcurve);
								}

								if(color_acurve.keys !=null&& color_acurve.keys.Length>0&& CheckCurveValid(color_acurve,defaultColorData.aM+defaultColorData.a0)) 
								{
									if(isHaveCurve) SetCustomCurveTangents(color_acurve,animSubData.frameDatas);
									CurveExtension.UpdateAllLinearTangents(color_acurve);
									clip.SetCurve(childPath,typeof(SpriteRenderer),"m_Color.a",color_acurve);
								}
							}
						}
					}
					else
					{
						SpriteFrame[] sprites = node.GetComponentsInChildren<SpriteFrame>();
						if(sprites!=null){
							for(int z=0;z<sprites.Length;++z){
								bool haveAnim = false;
								string childPath = path+"/"+sprites[z].name;
								if(color_rcurve.keys !=null&& color_rcurve.keys.Length>0&& CheckCurveValid(color_rcurve,defaultColorData.rM+defaultColorData.r0)) 
								{
									if(isHaveCurve) SetCustomCurveTangents(color_rcurve,animSubData.frameDatas);
									CurveExtension.UpdateAllLinearTangents(color_rcurve);
									clip.SetCurve(childPath,typeof(SpriteFrame),"m_color.r",color_rcurve);
									haveAnim = true;
								}

								if(color_gcurve.keys !=null&& color_gcurve.keys.Length>0&& CheckCurveValid(color_gcurve,defaultColorData.gM+defaultColorData.g0)) 
								{
									if(isHaveCurve) SetCustomCurveTangents(color_gcurve,animSubData.frameDatas);
									CurveExtension.UpdateAllLinearTangents(color_gcurve);
									clip.SetCurve(childPath,typeof(SpriteFrame),"m_color.g",color_gcurve);
									haveAnim = true;
								}

								if(color_bcurve.keys !=null&& color_bcurve.keys.Length>0&& CheckCurveValid(color_bcurve,defaultColorData.bM+defaultColorData.b0)) 
								{
									if(isHaveCurve) SetCustomCurveTangents(color_bcurve,animSubData.frameDatas);
									CurveExtension.UpdateAllLinearTangents(color_bcurve);
									clip.SetCurve(childPath,typeof(SpriteFrame),"m_color.b",color_bcurve);
									haveAnim = true;
								}

								if(color_acurve.keys !=null&& color_acurve.keys.Length>0&& CheckCurveValid(color_acurve,defaultColorData.aM+defaultColorData.a0)) 
								{
									if(isHaveCurve) SetCustomCurveTangents(color_acurve,animSubData.frameDatas);
									CurveExtension.UpdateAllLinearTangents(color_acurve);
									clip.SetCurve(childPath,typeof(SpriteFrame),"m_color.a",color_acurve);
									haveAnim = true;
								}
								if(haveAnim){
									changedSpriteFramesKV[childPath] = sprites[z];
								}
							}
						}

						SpriteMesh[] spriteMeshs = node.GetComponentsInChildren<SpriteMesh>();
						if(spriteMeshs!=null){
							for(int z=0;z<spriteMeshs.Length;++z){
								string childPath = path+"/"+spriteMeshs[z].name;
								bool haveAnim = false;
								if(color_rcurve.keys !=null&& color_rcurve.keys.Length>0&& CheckCurveValid(color_rcurve,defaultColorData.rM+defaultColorData.r0)) 
								{
									if(isHaveCurve) SetCustomCurveTangents(color_rcurve,animSubData.frameDatas);
									CurveExtension.UpdateAllLinearTangents(color_rcurve);
									clip.SetCurve(childPath,typeof(SpriteMesh),"m_color.r",color_rcurve);
									haveAnim = true;
								}

								if(color_gcurve.keys !=null&& color_gcurve.keys.Length>0&& CheckCurveValid(color_gcurve,defaultColorData.gM+defaultColorData.g0)) 
								{
									if(isHaveCurve) SetCustomCurveTangents(color_gcurve,animSubData.frameDatas);
									CurveExtension.UpdateAllLinearTangents(color_gcurve);
									clip.SetCurve(childPath,typeof(SpriteMesh),"m_color.g",color_gcurve);
									haveAnim = true;
								}

								if(color_bcurve.keys !=null&& color_bcurve.keys.Length>0&& CheckCurveValid(color_bcurve,defaultColorData.bM+defaultColorData.b0)) 
								{
									if(isHaveCurve) SetCustomCurveTangents(color_bcurve,animSubData.frameDatas);
									CurveExtension.UpdateAllLinearTangents(color_bcurve);
									clip.SetCurve(childPath,typeof(SpriteMesh),"m_color.b",color_bcurve);
									haveAnim = true;
								}

								if(color_acurve.keys !=null&& color_acurve.keys.Length>0&& CheckCurveValid(color_acurve,defaultColorData.aM+defaultColorData.a0)) 
								{
									if(isHaveCurve) SetCustomCurveTangents(color_acurve,animSubData.frameDatas);
									CurveExtension.UpdateAllLinearTangents(color_acurve);
									clip.SetCurve(childPath,typeof(SpriteMesh),"m_color.a",color_acurve);
									haveAnim = true;
								}
								if(haveAnim){
									changedSpriteMeshsKV[childPath] = spriteMeshs[z];
								}
							}
						}
					}

					for(int r=0;r<renderCurves.Length;++r){
						AnimationCurve ac = renderCurves[r];
						Renderer render = renders[r];
						float defaultValue = render.enabled? 1: 0;
						if(ac.keys!=null && ac.keys.Length>0 && CheckCurveValid(ac,defaultValue)){
							clip.SetCurve(path+"/"+render.name,typeof(GameObject),"m_IsActive",ac);	//m_Enabled
						}
					}

					if(isffd && vertexcurvexArray!=null)
					{
						for(int k=0;k<vertexcurvexArray.Count;++k){
							Transform ffdNode = node.GetChild(k);
							if(ffdNode.name==animSubData.name){

								changedSpriteMeshsKV[path+"/"+ffdNode.name] = ffdNode.GetComponent<SpriteMesh>();

								AnimationCurve[] vertex_xcurves= vertexcurvexArray[k];
								AnimationCurve[] vertex_ycurves= vertexcurveyArray[k];
								for(int r=0;r<vertex_xcurves.Length;++r){
									AnimationCurve vertex_xcurve = vertex_xcurves[r];
									AnimationCurve vertex_ycurve = vertex_ycurves[r];
									Transform v = ffdNode.GetChild(r);
									string ctrlPath = path+"/"+ffdNode.name+"/"+v.name;

									OptimizesCurve(vertex_xcurve);
									OptimizesCurve(vertex_ycurve);

									bool vcurveFlag = false;
									if(vertex_xcurve.keys !=null&& vertex_xcurve.keys.Length>0&& CheckCurveValid(vertex_xcurve,v.localPosition.x)) vcurveFlag = true;
									if(vertex_ycurve.keys !=null&& vertex_ycurve.keys.Length>0&& CheckCurveValid(vertex_ycurve,v.localPosition.y)) vcurveFlag=  true;
									if(vcurveFlag){
										if(isHaveCurve) SetCustomCurveTangents(vertex_xcurve,animSubData.frameDatas);
										CurveExtension.UpdateAllLinearTangents(vertex_xcurve);
										clip.SetCurve(ctrlPath,typeof(Transform),"localPosition.x",vertex_xcurve);
										if(isHaveCurve) SetCustomCurveTangents(vertex_ycurve,animSubData.frameDatas);
										CurveExtension.UpdateAllLinearTangents(vertex_ycurve);
										clip.SetCurve(ctrlPath,typeof(Transform),"localPosition.y",vertex_ycurve);
									}
								}
							}
						}

					}
				}
			}
		}

		static void SetDragonBoneArmature(ArmatureEditor armature){
			DragonBoneArmature dbArmature = armature.armature.GetComponent<DragonBoneArmature>();
			if(dbArmature){
				dbArmature.updateFrames = new SpriteFrame[changedSpriteFramesKV.Count];
				int i=0;
				foreach(SpriteFrame frame in changedSpriteFramesKV.Values){
					dbArmature.updateFrames[i] = frame;
					++i;
				}

				dbArmature.updateMeshs = new SpriteMesh[changedSpriteMeshsKV.Count];
				i=0;
				foreach(SpriteMesh mesh in changedSpriteMeshsKV.Values){
					dbArmature.updateMeshs[i] = mesh;
					++i;
				}
			}
		}

		/// <summary>
		/// set events
		/// </summary>
		static void SetEvent( ArmatureEditor armatureEditor,AnimationClip clip,DragonBoneData.AnimKeyData[] frameDatas)
		{
			if(frameDatas==null || frameDatas.Length==0) return;

			armatureEditor.armature.gameObject.AddComponent<DragonBoneEvent>();
			float during = 0;
			float perKeyTime = 1f/armatureEditor.armatureData.frameRate;

			List<AnimationEvent> evts=new List<AnimationEvent>();
			foreach(DragonBoneData.AnimKeyData keyData in frameDatas)
			{
				if(!string.IsNullOrEmpty(keyData.eventName))
				{
					AnimationEvent ae = new AnimationEvent();
					ae.messageOptions = SendMessageOptions.DontRequireReceiver;

					string param = keyData.eventName+"$";
					if(!string.IsNullOrEmpty(keyData.actionName))
					{
						param+=keyData.actionName+"$";
					}
					else
					{
						param+="$";
					}

					if(!string.IsNullOrEmpty(keyData.soundName))
					{
						param+=keyData.soundName;
					}

					ae.functionName = "OnAnimEvent";
					ae.time = during;
					ae.stringParameter = param;
					evts.Add(ae);
				}

				during += keyData.duration*perKeyTime;
			}
			if(evts.Count>0){
				AnimationUtility.SetAnimationEvents(clip,evts.ToArray());
			}

		}

		//check invalid curve
		static bool CheckCurveValid(AnimationCurve curve , float defaultValue){
			Keyframe frame = curve.keys[0];
			if(curve.length==1) {
				if(frame.value==defaultValue) return false;
				return true;
			}
			for(int i=0;i<curve.keys.Length;++i){
				Keyframe frame2 = curve.keys[i];
				if(frame.value!=defaultValue || frame.value!=frame2.value) {
					return true;
				}
			}
			return false;
		}

		static void OptimizesCurve( AnimationCurve curve){
			if(curve!=null && curve.length>0){
				for(int i=1;i<curve.keys.Length-1;++i){
					Keyframe f1 = curve.keys[i-1];//前一帧
					Keyframe frame = curve.keys[i];
					Keyframe f2 = curve.keys[i+1];//后一帧
					if(frame.value==f1.value && frame.value==f2.value) {
						curve.RemoveKey(i);
						i--;
					}
				}
			}
		}

		static string GetNodeRelativePath(ArmatureEditor armatureEditor ,Transform node){
			List<string> path = new List<string>();
			while(node!=armatureEditor.armature)
			{
				path.Add(node.name);
				node = node.parent;
			}
			string result="";
			for(int i=path.Count-1;i>=0;i--){
				result+=path[i]+"/";
			}
			return result.Substring(0,result.Length-1);
		}






		#region custom curve

		public static void SetCustomCurveTangents(AnimationCurve curve, DragonBoneData.AnimFrameData[] frameDatas){
			int len=curve.keys.Length;
			for (int i = 0; i < len; i++) {
				int nextI = i + 1;
				if (nextI < curve.keys.Length){
					if (frameDatas[i].curve != null ){ 
						SetCustomTangents(curve, i, nextI, frameDatas[i].curve);
					}
				}
			}
		}

		// p0, p3 - start, end points
		// p1, p2 - conrol points
		// t - value on x [0,1]
		static Vector2 GetBezierPoint(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t){
			float y = (1 - t) * (1 - t) * (1 - t) * p0.y +
				3 * t * (1 - t) * (1 - t) * p1.y +
				3 * t * t * (1 - t) * p2.y +
				t * t * t * p3.y;
			return new Vector2(p0.x + t * (p3.x - p0.x) ,y);
		}

		// a - start point
		// b - on t= 1/3
		// c - on t = 2/3
		// d - end point
		// c1,c2 control points of bezier.
		static void CalcControlPoints(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 c1, out Vector2 c2){
			c1 = (-5 * a + 18 * b - 9 * c + 2 * d)/6;
			c2 = ( 2 * a - 9 * b + 18 * c - 5 * d)/6;	
		}

		static void SetCustomTangents(AnimationCurve curve, int i, int nextI, float[] tangentArray){
			float diffValue = curve[nextI].value - curve[i].value;
			float diffTime = curve[nextI].time - curve[i].time;
			if (diffValue == 0)
				return; 


			float cx1 = tangentArray[0];
			float cy1 = tangentArray[1];
			float cx2 = tangentArray[2];
			float cy2 = tangentArray[3];

			Vector2 p0     = new Vector2(0  , curve[i].value);
			Vector2 p3     = new Vector2(diffTime  , curve[nextI].value);
			Vector2 cOrig1 = new Vector2(diffTime * cx1, curve[i].value);
			cOrig1.y += diffValue > 0 ? diffValue * cy1 : -1.0f * Mathf.Abs(diffValue * cy1);

			Vector2 cOrig2 = new Vector2(diffTime * cx2, curve[i].value);
			cOrig2.y += diffValue > 0 ? diffValue * cy2 : -1.0f * Mathf.Abs(diffValue * cy2);

			Vector2 p1 = GetBezierPoint(p0, cOrig1, cOrig2, p3, 1.0f / 3.0f);
			Vector2 p2 = GetBezierPoint(p0, cOrig1, cOrig2, p3, 2.0f / 3.0f);


			Vector2 c1tg, c2tg, c1, c2;
			CalcControlPoints(p0,p1,p2,p3, out c1, out c2);

			c1tg = c1 - p0;
			c2tg = c2 - p3;

			float outTangent = c1tg.y / c1tg.x;
			float inTangent  = c2tg.y / c2tg.x;


			object thisKeyframeBoxed = curve[i];
			object nextKeyframeBoxed = curve[nextI];


			if (!KeyframeUtil.isKeyBroken(thisKeyframeBoxed))
				KeyframeUtil.SetKeyBroken(thisKeyframeBoxed, true);
			
			TangentMode mode = TangentMode.Editable;
			if(cx1==0f&&cy1==0f) mode=TangentMode.Linear;
			KeyframeUtil.SetKeyTangentMode(thisKeyframeBoxed, 1, mode);

			if (!KeyframeUtil.isKeyBroken(nextKeyframeBoxed))
				KeyframeUtil.SetKeyBroken(nextKeyframeBoxed, true);	

			mode = TangentMode.Editable;
			if(cx2==1f&&cy2==1f) mode=TangentMode.Linear;
			KeyframeUtil.SetKeyTangentMode(nextKeyframeBoxed, 0, mode);

			Keyframe thisKeyframe = (Keyframe)thisKeyframeBoxed;
			Keyframe nextKeyframe = (Keyframe)nextKeyframeBoxed;

			thisKeyframe.outTangent = outTangent;
			nextKeyframe.inTangent  = inTangent;

			curve.MoveKey(i, 	 thisKeyframe);
			curve.MoveKey(nextI, nextKeyframe);

			//* test method
			float startTime = thisKeyframe.time;

			for (float j=0; j < 25f; j++) {
				float t  = j/25.0f;
				curve.Evaluate(startTime + diffTime * t);			
			}

		}

		public static bool NearlyEqual(float a, float b, float epsilon)
		{
			float absA = Mathf.Abs(a);
			float absB = Mathf.Abs(b);
			float diff = Mathf.Abs(a - b);

			if (a == b)
			{ // shortcut, handles infinities
				return true;
			} 
			else if (a == 0 || b == 0 || diff < System.Double.MinValue) 
			{
				// a or b is zero or both are extremely close to it
				// relative error is less meaningful here
				return diff < (epsilon * System.Double.MinValue);
			}
			else
			{ // use relative error
				return diff / (absA + absB) < epsilon;
			}
		}
		#endregion
	}
}
