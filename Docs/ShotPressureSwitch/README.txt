射击压柱机关

预制体：Assets/Prefabs/Level/ShotPressureSwitch.prefab
已在当前 TrapTesting 场景放置一个“射击压柱机关_9发激活”，位于白色平台右侧。

使用：将预制体从 Project 拖入任意关卡，底座放在地面上，运行后用当前枪射击。
连续命中 9 发后压到底，红色变绿色，绿色粒子和轻微屏幕闪光提示成功。
中途停火，从最后一发命中算起约 1 秒完全弹回，需要重新打 9 发。
前 0.18 秒容许正常连发的间隔，之后在剩下 0.82 秒内回弹。
激活后在本次关卡运行中永久保持。额外射击、停火、隐藏再显示都不会重置。
此处的永久不写入跨关卡存档；以后有存档系统时可接入关卡状态。

选中机关，在 Shot Pressure Switch 组件中修改：
Hits Required：所需命中数，默认 9，可改 8。
Return Seconds：最后一发之后完全弹回所需时间，默认 1 秒。
Shot Grace Seconds：正常连发的短间隔容差，已包含在总回弹时间内。
Press Travel Seconds：压头移动速度，数值越大越慢。
Accept Ghost Bullets：是否接受 E 技能分身的子弹，默认接受，与原有射击机关一致。

门的联动：
在 On Activated 事件里点 +，拖入门对象，选择门脚本中的公开 Open 方法。
事件只在压到底、绿色亮起时触发一次。也可以由其他脚本读取 IsActive。
当前未替换已有门逻辑，也未与已有的单发 ShootSwitch 混用。

素材（更新）：当前使用精细侧视 2D 压头、竖直活塞和方形底座。原来用户提供的三个 PNG 原文件及 2.5D 预制体备份均保留。
新素材在 Unity 中切为底座、轴、压头；底座固定、压头下移；完全压下后仅红色指示灯变绿。外形始终保持侧视 2D。下压行程增加，逐发变化更清楚。
使用 Point 过滤、关闭 MipMap、无压缩。新图以 128 高度导入，提供更细的像素密度，部件按原来的世界尺寸配置；原始素材 16 PPU 保留。
图片在 Assets/Art/ShotPressureSwitch；逻辑在 Assets/Scripts/Level；特效在 Assets/Scripts/Effects；预制体在 Assets/Prefabs/Level；材质和着色器在 Assets/Shaders/ShotPressureSwitch；编辑工具在 Assets/Editor/ShotPressureSwitch。
备份在项目 Logs/ShotPressureSwitch/Backups，说明文档在 Docs/ShotPressureSwitch。

切片工具采用 Unity 2D Sprite 1.0 的 Sprite Editor Data Provider API：
https://docs.unity3d.com/Packages/com.unity.2d.sprite@1.0/manual/DataProvider.html

检查：地图 > 射击机关 > 运行实弹验证。
验证使用实际 Bullet 预制体和 2D 物理命中，自动检查回弹、9 发激活、事件去重、分身过滤与重叠碰撞体。
结果保存在项目 Logs/ShotPressureSwitch/RuntimeCheck.json，截图保存在同目录 Checks 文件夹。

碰撞与天花板：仅压头参与碰撞，碰撞框随压头下压和回弹移动，柱杆及底座不挡玩家或子弹。
天花板版本：Assets/Prefabs/Level/ShotPressureSwitch_Ceiling.prefab，已绕 Z 轴旋转 180 度，底座原点是挂点；下方射击会把压头向上压入。

