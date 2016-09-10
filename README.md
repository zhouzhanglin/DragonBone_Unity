#DragonBone_Unity
直接将DragonBone数据转换成Unity自带的动画，支持骨骼动画，网格动画，蒙皮动画, IK动画。需要Unity5.3.x以上
#使用方法
1.菜单栏 -> DragonBone -> DragonBone创建面板(手动) -> 选择动画文件,动画贴图集,贴图集配置 -----------> 此面板包含了本项目所有功能<br/> 
2.菜单栏 -> DragonBone -> 选中文件夹创建DragonBone (选中文件夹，此文件夹中需要包含动画文件,动画贴图集,贴图集配置，图集的两个文件名称要为texture)<br/> 
3.菜单栏 -> DragonBone -> 选中文件夹创建DragonBone(UnitySprite) (选中文件夹，此文件夹中需要包含动画文件) ，此项支持图集或散图<br/>       
#说明
暂时不支持的有: 骨骼嵌套, IK嵌套可能有问题<br/>
由于unity动画限制，所以此插件不支持nonInheritRotations，nonInheritScales (也就是DragonBone编辑器里这两个属性必须勾选)<br/>
在龙骨做绑定时，同一层级的骨骼和插槽名称不能相同<br/>
导出动画需要用相对坐标<br/>
如果你只想使用spine的运行库，可以使用[DragonBoneToSpineData](http://git.oschina.net/bingheliefeng/DragonBoneToSpineData)