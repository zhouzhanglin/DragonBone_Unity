#DragonBone_Unity
Dragon Bone是一款免费的动画制作工具，下载地址[http://dragonbones.com](http://dragonbones.com)<br/>
此插件是直接将DragonBone数据转换成Unity自带的动画，支持骨骼动画，网格动画，蒙皮动画, 碰撞框。需要Unity5.3.x以上
#使用方法
1.(手动)菜单栏 -> DragonBone -> DragonBone Panel(All Function) -> 选择动画文件,动画贴图集,贴图集配置 -----------> 此面板包含了本项目所有功能<br/> 
2.(自动)选中动画文件夹->打开Unity菜单栏 -> DragonBone -> DragonBone(SpriteFrame)/(UnitySprite)<br/>     
#功能限制说明
暂时不支持的有: 1）骨架嵌套(一个骨架里包含另一个骨架)    2）IK嵌套可能有问题<br/>
3) 不支持曲线中使用多控制点，只会取前面和最后面一个点<br/>
#注意事项
在龙骨做绑定时，同一层级的骨骼和插槽名称不能相同<br/>
如果你只想使用spine的运行库，可以使用[DragonBoneToSpineData](http://git.oschina.net/bingheliefeng/DragonBoneToSpineData)
