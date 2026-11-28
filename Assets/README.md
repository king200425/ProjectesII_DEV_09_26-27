# Assets 目录

按资源类型存放，类型内部再按角色、关卡或功能划分。

| 目录 | 内容 |
| --- | --- |
| `Scripts/` | 所有 C# 代码；玩家、技能、敌人、机关、UI 等按功能分组 |
| `Scripts/Editor/` | Unity 编辑器工具；该目录内代码不会进入游戏构建 |
| `Art/` | 图片、精灵及美术源文件；角色、环境、特效、UI、武器、机关分类 |
| `Animations/` | 动画片段和 Animator Controller |
| `Fonts/` | 游戏界面字体 |
| `Materials/` | 材质，包括 `Physics/` 物理材质 |
| `Meshes/` | 关卡生成网格；不是原始美术图片 |
| `Shaders/` | Shader、Shader Graph 和 GPU 着色代码 |
| `Prefabs/` | 玩家、敌人、关卡对象、机关和 UI 预制体 |
| `Scenes/Menu/` | MainMenu 和 SaveSelect |
| `Scenes/Levels/` | Mapx11 和现有塔关场景 |
| `Scenes/Tests/` | 玩家沙盒、机关测试场景 |
| `Settings/` | 输入、渲染、构建配置及场景模板 |
| `Tiles/` | Tile 资源和 Tile Palette；图片仍放在 Art |
| `ThirdParty/` | 第三方完整依赖，保留包内结构 |

游戏流程：`MainMenu → SaveSelect → Mapx11`。
打开正式关卡：`Scenes/Levels/Mapx11.unity`。
原先打开的 `0 (2)` 改名为 `Scenes/Tests/PlayerSandbox.unity`，内容保留。

常用资源：

- 当前玩家：`Prefabs/Player/Player.prefab`。
- E 分身：`Prefabs/Player/Ghost/`。
- 旧塔关专用玩家：`Prefabs/Player/TowerPrototype/Player.prefab`。它仍被现有塔关和相关测试引用，和当前玩家的组件配置不同。
- 射击按钮：`Prefabs/Level/`；按钮图片：`Art/Traps/ShotPressureSwitch/`。
- 白块 Tile 与调色板：`Tiles/Blockout/`、`Tiles/Palettes/BlockoutPalette.prefab`。
- 科幻实验室图片：`Art/Environment/SciFiLab/`；关卡网格：`Meshes/Environment/SciFiLab/`。

已淘汰的 Aftertime 旧界面、旧素材包的未使用内容、恢复副本和无引用的生成网格已清理。
仍被现有菜单引用的 SpaceMono 字体独立保存在 Fonts，许可证位于项目根目录 `Docs/Licenses/`。
重复图片已共享同一纹理文件，原有精灵切片 ID、轴心和动画引用保留。

备份、检查报告和说明文件放在 Assets 之外：备份及报告在本机工作目录或 `Logs/`，说明与许可证在 `Docs/`。
移动资源时连同 `.meta` 一起移动，优先使用 Unity Project 窗口，保留 GUID。
