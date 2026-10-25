# Assets 目录约定

按资源类型分类，同类资源再按用途划分。目前只创建已经用到的目录。

```text
Assets/
├── Animations/                 动画片段和 Animator Controller
│   ├── Player/
│   └── Weapons/
├── Art/                        图片、精灵及其他美术源资源
│   ├── Enemies/
│   ├── Player/
│   │   ├── Assassin/
│   │   └── Druid/
│   ├── Projectiles/Bullets/
│   └── Weapons/Guns/
├── Input/                      Input Actions 输入配置
├── Prefabs/                    可复用的预制体
│   └── Projectiles/            Bullet.prefab
├── Scenes/                     当前场景
│   └── Recovery/               历史恢复场景，仅供找回内容
├── Scripts/                    C# 玩法脚本
│   ├── Player/
│   ├── Projectiles/
│   └── Weapons/
└── Settings/
    ├── Rendering/              URP、Renderer 和 Volume 配置
    └── SceneTemplates/         Unity 场景模板及其依赖场景
```

## 新资源放哪里

- 新角色图片：`Art/Player/角色名/`；敌人图片：`Art/Enemies/敌人名/`。
- 新动画：`Animations/Player/` 或 `Animations/Weapons/`。将来有多个角色时，再在对应目录内按角色细分。
- 新预制体：`Prefabs/Player/`、`Prefabs/Enemies/`、`Prefabs/Weapons/` 等；需要时再创建。
- 新脚本：放进 `Scripts/` 下对应的功能目录；例如玩家控制属于 `Scripts/Player/`。
- 新关卡或测试场景：放进 `Scenes/`；数量增加后再建 `Levels/`、`Tests/`。
- 新音频、材质、UI：首次用到时再建 `Audio/`、`Materials/`、`UI/`。
- 第三方完整资源包：首次导入时优先使用 `ThirdParty/资源包名/`，保留包内结构和许可证。

## 命名和维护

- 文件夹使用清楚的英文名称，例如 `Assassin`，避免 `1`、`2`、`New Folder`。
- 动画、脚本和预制体使用用途名称，例如 `RunBackward.anim`、`PlayerMovement.cs`、`Bullet.prefab`。
- 导入素材已有的文件名和精灵切片名称可以保留，避免丢失来源信息。
- 移动或重命名优先在 Unity 的 Project 窗口里进行；提交 Git 时包含对应的 `.meta` 文件。
- `Scenes/Recovery/PlayerTest_Recovery.unity` 是旧恢复副本；当前开发场景是 `Scenes/PlayerTest.unity`。
- `Scenes/SampleScene.unity` 仍是现有构建配置中的场景。目录整理没有调整构建入口。
- 整理目录时保留资源；确认不再使用后再单独清理。

本次移动通过 Unity 的 [AssetDatabase.MoveAsset](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AssetDatabase.MoveAsset.html) 执行，并核对原有 GUID、文件内容和引用。
