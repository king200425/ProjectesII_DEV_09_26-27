# Assets 整理记录

已按类型整理 Assets。目录说明见 Assets/README.md。

- 所有 C# 代码位于 Scripts；编辑器代码位于 Scripts/Editor。
- Art 仅保留图片/美术源文件。字体、材质、网格、Shader、Tile 和第三方依赖各自独立。
- 704 项资源整理为 502 项（不计 .meta），从 Assets 清出 202 项旧内容，其中 2 项许可证移到 Docs/Licenses。
- 8 组重复纹理合并为共享纹理；每个原精灵的切片 ID、轴心、尺寸、PPU 和动画引用均保留并在 Unity 中核对。
- Aftertime 旧界面已移除；菜单仍在用的 SpaceMono 字体保留在 Fonts，许可证独立归档。
- 旧素材包只保留白块调色板和现有场景实际引用的资源。旧恢复副本、无用生成网格、生成报告和过时工具已清理。
- 正式 Mapx11 位于 Scenes/Levels；原 0 (2) 以 PlayerSandbox 名称保留在 Scenes/Tests。
- 当前 Player 位于 Prefabs/Player；旧塔关仍在使用的不同配置玩家隔离于 Prefabs/Player/TowerPrototype。
- 构建列表和工具中的硬编码路径同步更新，场景顺序保持原样。

验证通过：保留资源的 GUID、所有保留场景和预制体的绑定、合并精灵几何、无新增失效 GUID、所有图片/字体字节一致。
实际运行 MainMenu → SaveSelect → Mapx11，以及 Esc 暂停、继续、返回主菜单均通过。
原先打开的白块测试场景保持为编辑器当前场景。

完整整理前备份（Assets 目录，含 .meta）：
C:/Users/chenk/Documents/Codex/2026-11-23/ban-2/outputs/asset-organization/Assets-before-cleanup.zip

核对报告与移动/删除清单：
C:/Users/chenk/Documents/Codex/2026-11-23/ban-2/outputs/asset-organization

## 移动清单

| 原位置 | 新位置 |
| --- | --- |
| Assets/Art/AbandonedOutpost/ReadyToPaint/Palettes/AO_00_QuickBlocks.prefab | Assets/Tiles/Palettes/BlockoutPalette.prefab |
| Assets/Art/AbandonedOutpost/ReadyToPaint/Tiles/AO_00_QuickBlocks.asset | Assets/Tiles/Blockout/BlockoutTiles.asset |
| Assets/Art/AbandonedOutpost/tilesets/tileset-platforms-3-16x16.png | Assets/Art/Environment/AbandonedOutpost/Platforms.png |
| Assets/Art/Enemies/Lightning/airbender_s2_lightning_1_0.png | Assets/Art/Characters/Enemies/Lightning/airbender_s2_lightning_1_0.png |
| Assets/Art/Enemies/Lightning/airbender_s2_lightning_1_1.png | Assets/Art/Characters/Enemies/Lightning/airbender_s2_lightning_1_1.png |
| Assets/Art/Enemies/Lightning/airbender_s2_lightning_1_3.png | Assets/Art/Characters/Enemies/Lightning/airbender_s2_lightning_1_3.png |
| Assets/Art/Enemies/Lightning/airbender_s2_lightning_1_4.png | Assets/Art/Characters/Enemies/Lightning/airbender_s2_lightning_1_4.png |
| Assets/Art/Level01/Backgrounds/A_background.png | Assets/Art/Environment/Level01/Backgrounds/A_background.png |
| Assets/Art/Level01/Checkpoints/checkpoint_off_crop.png | Assets/Art/Environment/Level01/Checkpoints/checkpoint_off_crop.png |
| Assets/Art/Level01/Checkpoints/checkpoint_on_crop.png | Assets/Art/Environment/Level01/Checkpoints/checkpoint_on_crop.png |
| Assets/Art/Level01/Gothic/Gothic_PlatformBody.png | Assets/Art/Environment/Level01/Gothic/Gothic_PlatformBody.png |
| Assets/Art/Level01/Gothic/Gothic_Spikes.png | Assets/Art/Environment/Level01/Gothic/Gothic_Spikes.png |
| Assets/Art/Level01/Gothic/Gothic_WallLarge.png | Assets/Art/Environment/Level01/Gothic/Gothic_WallLarge.png |
| Assets/Art/Level01/Gothic/Gothic_Window.png | Assets/Art/Environment/Level01/Gothic/Gothic_Window.png |
| Assets/Art/Level01/LayerTiles/Tiles/tile_0_4.asset | Assets/Tiles/Level01/tile_0_4.asset |
| Assets/Art/Level01/LayerTiles/Tiles/tile_1_0.asset | Assets/Tiles/Level01/tile_1_0.asset |
| Assets/Art/Level01/LayerTiles/Tiles/tile_1_4.asset | Assets/Tiles/Level01/tile_1_4.asset |
| Assets/Art/Level01/LayerTiles/Tiles/tile_2_1.asset | Assets/Tiles/Level01/tile_2_1.asset |
| Assets/Art/Level01/LayerTiles/Tiles/tile_2_3.asset | Assets/Tiles/Level01/tile_2_3.asset |
| Assets/Art/Level01/LayerTiles/Tiles/tile_3_0.asset | Assets/Tiles/Level01/tile_3_0.asset |
| Assets/Art/Level01/LayerTiles/Tiles/tile_4_2.asset | Assets/Tiles/Level01/tile_4_2.asset |
| Assets/Art/Level01/LayerTiles/tile_0_4.png | Assets/Art/Environment/Level01/LayerTiles/tile_0_4.png |
| Assets/Art/Level01/LayerTiles/tile_1_0.png | Assets/Art/Environment/Level01/LayerTiles/tile_1_0.png |
| Assets/Art/Level01/LayerTiles/tile_1_0_seamless.png | Assets/Art/Environment/Level01/LayerTiles/tile_1_0_seamless.png |
| Assets/Art/Level01/LayerTiles/tile_1_4.png | Assets/Art/Environment/Level01/LayerTiles/tile_1_4.png |
| Assets/Art/Level01/LayerTiles/tile_2_1.png | Assets/Art/Environment/Level01/LayerTiles/tile_2_1.png |
| Assets/Art/Level01/LayerTiles/tile_2_3.png | Assets/Art/Environment/Level01/LayerTiles/tile_2_3.png |
| Assets/Art/Level01/LayerTiles/tile_3_0.png | Assets/Art/Environment/Level01/LayerTiles/tile_3_0.png |
| Assets/Art/Level01/LayerTiles/tile_3_0_seamless.png | Assets/Art/Environment/Level01/LayerTiles/tile_3_0_seamless.png |
| Assets/Art/Level01/LayerTiles/tile_4_2.png | Assets/Art/Environment/Level01/LayerTiles/tile_4_2.png |
| Assets/Art/Level01/LayerTiles/tile_4_2_seamless.png | Assets/Art/Environment/Level01/LayerTiles/tile_4_2_seamless.png |
| Assets/Art/Level01/Materials/UnlitSprite.mat | Assets/Materials/Environment/Level01/UnlitSprite.mat |
| Assets/Art/Level01/Painted/arch_bridge_crop.png | Assets/Art/Environment/Level01/Painted/arch_bridge_crop.png |
| Assets/Art/Level01/Painted/arch_pieces/arch_piece_0.png | Assets/Art/Environment/Level01/Painted/arch_pieces/arch_piece_0.png |
| Assets/Art/Level01/Painted/arch_pieces/arch_piece_1.png | Assets/Art/Environment/Level01/Painted/arch_pieces/arch_piece_1.png |
| Assets/Art/Level01/Painted/arch_pieces/arch_piece_2.png | Assets/Art/Environment/Level01/Painted/arch_pieces/arch_piece_2.png |
| Assets/Art/Level01/Painted/arch_pieces/arch_piece_3.png | Assets/Art/Environment/Level01/Painted/arch_pieces/arch_piece_3.png |
| Assets/Art/Level01/Painted/arch_pieces/arch_piece_4.png | Assets/Art/Environment/Level01/Painted/arch_pieces/arch_piece_4.png |
| Assets/Art/Level01/Painted/arch_pieces/arch_piece_5.png | Assets/Art/Environment/Level01/Painted/arch_pieces/arch_piece_5.png |
| Assets/Art/Level01/Painted/arch_pieces/arch_piece_6.png | Assets/Art/Environment/Level01/Painted/arch_pieces/arch_piece_6.png |
| Assets/Art/Level01/Painted/column_a.png | Assets/Art/Environment/Level01/Painted/column_a.png |
| Assets/Art/Level01/Painted/f_upper_rock_wall_tall_crop.png | Assets/Art/Environment/Level01/Painted/f_upper_rock_wall_tall_crop.png |
| Assets/Art/Level01/Painted/platform_a.png | Assets/Art/Environment/Level01/Painted/platform_a.png |
| Assets/Art/Level01/Painted/platform_rightcap.png | Assets/Art/Environment/Level01/Painted/platform_rightcap.png |
| Assets/Art/Level01/Painted/platform_short.png | Assets/Art/Environment/Level01/Painted/platform_short.png |
| Assets/Art/Level01/Painted/platform_thick_long_crop.png | Assets/Art/Environment/Level01/Painted/platform_thick_long_crop.png |
| Assets/Art/Level01/Painted/platform_tile_stone_crop.png | Assets/Art/Environment/Level01/Painted/platform_tile_stone_crop.png |
| Assets/Art/Level01/Painted/spike_frames/spike_4.png | Assets/Art/Environment/Level01/Painted/spike_frames/spike_4.png |
| Assets/Art/Level01/Painted/spike_tops/spike_top_0.png | Assets/Art/Environment/Level01/Painted/spike_tops/spike_top_0.png |
| Assets/Art/Level01/Painted/spike_tops/spike_top_1.png | Assets/Art/Environment/Level01/Painted/spike_tops/spike_top_1.png |
| Assets/Art/Level01/Painted/spike_tops/spike_top_2.png | Assets/Art/Environment/Level01/Painted/spike_tops/spike_top_2.png |
| Assets/Art/Level01/Painted/spike_tops/spike_top_3.png | Assets/Art/Environment/Level01/Painted/spike_tops/spike_top_3.png |
| Assets/Art/Level01/Painted/spike_tops/spike_top_4.png | Assets/Art/Environment/Level01/Painted/spike_tops/spike_top_4.png |
| Assets/Art/Level01/Painted/spike_tops/spike_top_5.png | Assets/Art/Environment/Level01/Painted/spike_tops/spike_top_5.png |
| Assets/Art/Level01/Painted/spike_tops/spike_top_6.png | Assets/Art/Environment/Level01/Painted/spike_tops/spike_top_6.png |
| Assets/Art/Level01/Painted/spike_tops/spike_top_7.png | Assets/Art/Environment/Level01/Painted/spike_tops/spike_top_7.png |
| Assets/Art/Level01/Painted/walkway_a_crop.png | Assets/Art/Environment/Level01/Painted/walkway_a_crop.png |
| Assets/Art/Level01/Switches/switch_off_crop.png | Assets/Art/Environment/Level01/Switches/switch_off_crop.png |
| Assets/Art/Level01/Switches/switch_on_crop.png | Assets/Art/Environment/Level01/Switches/switch_on_crop.png |
| Assets/Art/Level01/Switches/time_switch_hourglass_crop.png | Assets/Art/Environment/Level01/Switches/time_switch_hourglass_crop.png |
| Assets/Art/Level01/TimeGate/f_door_surround_crop.png | Assets/Art/Environment/Level01/TimeGate/f_door_surround_crop.png |
| Assets/Art/Level01/TimeGate/TG_Arch.png | Assets/Art/Environment/Level01/TimeGate/TG_Arch.png |
| Assets/Art/Level01/TimeGate/TG_Chain.png | Assets/Art/Environment/Level01/TimeGate/TG_Chain.png |
| Assets/Art/Level01/TimeGate/TG_Clock.png | Assets/Art/Environment/Level01/TimeGate/TG_Clock.png |
| Assets/Art/Level01/TimeGate/TG_Crystal.png | Assets/Art/Environment/Level01/TimeGate/TG_Crystal.png |
| Assets/Art/Level01/TimeGate/TG_Gear.png | Assets/Art/Environment/Level01/TimeGate/TG_Gear.png |
| Assets/Art/Level01/TimeGate/TG_Glow.png | Assets/Art/Environment/Level01/TimeGate/TG_Glow.png |
| Assets/Art/Level01/TimeGate/TG_Hourglass.png | Assets/Art/Environment/Level01/TimeGate/TG_Hourglass.png |
| Assets/Art/Level01/TimeGate/TG_Mount.png | Assets/Art/Environment/Level01/TimeGate/TG_Mount.png |
| Assets/Art/Level01/TimeGate/TG_Target.png | Assets/Art/Environment/Level01/TimeGate/TG_Target.png |
| Assets/Art/Level01/TimeGate/time_door_crop.png | Assets/Art/Environment/Level01/TimeGate/time_door_crop.png |
| Assets/Art/Level01/Whitebox/CBlockTile.asset | Assets/Tiles/Blockout/CBlockTile.asset |
| Assets/Art/Player/Assassin/assassin_2_0.png | Assets/Art/Characters/Player/Assassin/assassin_2_0.png |
| Assets/Art/Player/Assassin/assassin_2_1.png | Assets/Art/Characters/Player/Assassin/assassin_2_1.png |
| Assets/Art/Player/Assassin/assassin_2_10.png | Assets/Art/Characters/Player/Assassin/assassin_2_10.png |
| Assets/Art/Player/Assassin/assassin_2_11.png | Assets/Art/Characters/Player/Assassin/assassin_2_11.png |
| Assets/Art/Player/Assassin/assassin_2_12.png | Assets/Art/Characters/Player/Assassin/assassin_2_12.png |
| Assets/Art/Player/Assassin/assassin_2_13.png | Assets/Art/Characters/Player/Assassin/assassin_2_13.png |
| Assets/Art/Player/Assassin/assassin_2_14.png | Assets/Art/Characters/Player/Assassin/assassin_2_14.png |
| Assets/Art/Player/Assassin/assassin_2_15.png | Assets/Art/Characters/Player/Assassin/assassin_2_15.png |
| Assets/Art/Player/Assassin/assassin_2_16.png | Assets/Art/Characters/Player/Assassin/assassin_2_16.png |
| Assets/Art/Player/Assassin/assassin_2_2.png | Assets/Art/Characters/Player/Assassin/assassin_2_2.png |
| Assets/Art/Player/Assassin/assassin_2_3.png | Assets/Art/Characters/Player/Assassin/assassin_2_3.png |
| Assets/Art/Player/Assassin/assassin_2_4.png | Assets/Art/Characters/Player/Assassin/assassin_2_4.png |
| Assets/Art/Player/Assassin/assassin_2_5.png | Assets/Art/Characters/Player/Assassin/assassin_2_5.png |
| Assets/Art/Player/Assassin/assassin_2_6.png | Assets/Art/Characters/Player/Assassin/assassin_2_6.png |
| Assets/Art/Player/Assassin/assassin_2_7.png | Assets/Art/Characters/Player/Assassin/assassin_2_7.png |
| Assets/Art/Player/Assassin/assassin_2_8.png | Assets/Art/Characters/Player/Assassin/assassin_2_8.png |
| Assets/Art/Player/Assassin/assassin_2_9.png | Assets/Art/Characters/Player/Assassin/assassin_2_9.png |
| Assets/Art/Player/Assassin/walljump.png | Assets/Art/Characters/Player/Assassin/walljump.png |
| Assets/Art/Player/Druid/druid_7.png | Assets/Art/Characters/Enemies/Druid/druid_7.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_0.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_0.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_1.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_1.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_10.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_10.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_11.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_11.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_12.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_12.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_13.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_13.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_14.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_14.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_15.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_15.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_2.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_2.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_20.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_20.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_21 (1).gif | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_21 (1).gif |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_21 (2).gif | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_21 (2).gif |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_21 (3).gif | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_21 (3).gif |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_21 (4).gif | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_21 (4).gif |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_21 (5).gif | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_21 (5).gif |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_21.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_21.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_22.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_22.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_23.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_23.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_28.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_28.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_29.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_29.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_3.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_3.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_30.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_30.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_31.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_31.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_4.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_4.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_5.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_5.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_6.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_6.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_7.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_7.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_8.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_8.png |
| Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_9.png | Assets/Art/Effects/Ghost/assassin_2_skill_1_effect_0_9.png |
| Assets/Art/Player/Ghost/necromancer_8_skill_0_effect_9_1.png | Assets/Art/Effects/Ghost/necromancer_8_skill_0_effect_9_1.png |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_10.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_10.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_11.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_11.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_12.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_12.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_13.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_13.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_14.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_14.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_15.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_15.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_16.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_16.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_17.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_17.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_18.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_18.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_19.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_19.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_20.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_20.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_21.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_21.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_22.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_22.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_23.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_23.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_24.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_24.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_25.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_25.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_26.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_26.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_27.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_27.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_28.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_28.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_29.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_29.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_3.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_3.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_30.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_30.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_31.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_31.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_32.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_32.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_33.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_33.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_34.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_34.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_35.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_35.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_36.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_36.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_37.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_37.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_38.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_38.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_39.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_39.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_40.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_40.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_41.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_41.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_42.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_42.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_43.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_43.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_44.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_44.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_45.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_45.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_46.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_46.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_47.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_47.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_48.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_48.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_49.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_49.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_5.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_5.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_50.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_50.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_51.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_51.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_6.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_6.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_61.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_61.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_62.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_62.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_63.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_63.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_64.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_64.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_65.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_65.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_66.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_66.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_67.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_67.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_68.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_68.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_69.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_69.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_7.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_7.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_8.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_8.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_9.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Detail_Ground_9.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Lab_0.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Lab_0.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Lab_1.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Lab_1.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Lab_2.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Lab_2.asset |
| Assets/Art/SciFiLab/Geometry/Mapx11_Lab_3.asset | Assets/Meshes/Environment/SciFiLab/Mapx11_Lab_3.asset |
| Assets/Art/SciFiLab/level_tileset.png | Assets/Art/Environment/SciFiLab/level_tileset.png |
| Assets/Art/ShotPressureSwitch/button_machanics_0.png | Assets/Art/Traps/ShotPressureSwitch/button_machanics_0.png |
| Assets/Art/ShotPressureSwitch/button_machanics_1.png | Assets/Art/Traps/ShotPressureSwitch/button_machanics_1.png |
| Assets/Art/ShotPressureSwitch/button_machanics_unlock_1.png | Assets/Art/Traps/ShotPressureSwitch/button_machanics_unlock_1.png |
| Assets/Art/ShotPressureSwitch/pressure-switch-side-view-detailed.png | Assets/Art/Traps/ShotPressureSwitch/pressure-switch-side-view-detailed.png |
| Assets/Art/Traps/RotatingLaser/LaserSprite.mat | Assets/Materials/Traps/LaserSprite.mat |
| Assets/Art/UI/Aftertime/SpaceMono-Bold.ttf | Assets/Fonts/SpaceMono/SpaceMono-Bold.ttf |
| Assets/Art/UI/ChronoEcho/Chrono Ignition.mat | Assets/Materials/UI/ChronoIgnition.mat |
| Assets/Art/UI/ChronoEcho/ChronoEcho.shader | Assets/Shaders/UI/ChronoEcho.shader |
| Assets/Art/UI/ChronoEcho/Echo Interference.mat | Assets/Materials/UI/EchoInterference.mat |
| Assets/Art/Whitebox/WhiteSquare.png | Assets/Art/Blockout/WhiteSquare.png |
| Assets/Editor/LevelArt/LevelArtPreview.cs | Assets/Scripts/Editor/LevelArt/LevelArtPreview.cs |
| Assets/Editor/LevelArt/LevelArtPreview.shader | Assets/Scripts/Editor/LevelArt/LevelArtPreview.shader |
| Assets/Editor/LevelArt/Mapx11CameraBoundsSetup.cs | Assets/Scripts/Editor/LevelArt/Mapx11CameraBoundsSetup.cs |
| Assets/Editor/LevelArt/Mapx11LabSetup.cs | Assets/Scripts/Editor/LevelArt/Mapx11LabSetup.cs |
| Assets/Editor/PlayerMovement/PlayerWallFrictionSetup.cs | Assets/Scripts/Editor/PlayerMovement/PlayerWallFrictionSetup.cs |
| Assets/Editor/ShotPressureSwitch/ShotPressureSwitchCheck.cs | Assets/Scripts/Editor/ShotPressureSwitch/ShotPressureSwitchCheck.cs |
| Assets/Editor/ShotPressureSwitch/ShotPressureSwitchGhostColorSetup.cs | Assets/Scripts/Editor/ShotPressureSwitch/ShotPressureSwitchGhostColorSetup.cs |
| Assets/Editor/ShotPressureSwitch/ShotPressureSwitchGhostOnlySetup.cs | Assets/Scripts/Editor/ShotPressureSwitch/ShotPressureSwitchGhostOnlySetup.cs |
| Assets/Editor/ShotPressureSwitch/ShotPressureSwitchHeadSetup.cs | Assets/Scripts/Editor/ShotPressureSwitch/ShotPressureSwitchHeadSetup.cs |
| Assets/Editor/ShotPressureSwitch/ShotPressureSwitchSetup.cs | Assets/Scripts/Editor/ShotPressureSwitch/ShotPressureSwitchSetup.cs |
| Assets/Editor/ShotPressureSwitch/ShotPressureSwitchSideViewSetup.cs | Assets/Scripts/Editor/ShotPressureSwitch/ShotPressureSwitchSideViewSetup.cs |
| Assets/Input/InputSystem_Actions.inputactions | Assets/Settings/Input/InputSystem_Actions.inputactions |
| Assets/PhysicsMaterials/Player_NoFriction.physicsMaterial2D | Assets/Materials/Physics/Player_NoFriction.physicsMaterial2D |
| Assets/Prefabs/Checkpoints复活锚点/Checkpoint.prefab | Assets/Prefabs/Level/Checkpoints/Checkpoint.prefab |
| Assets/Prefabs/Ghost/Ghost.prefab | Assets/Prefabs/Player/Ghost/Ghost.prefab |
| Assets/Prefabs/Ghost/GunPivot.prefab | Assets/Prefabs/Player/Ghost/GunPivot.prefab |
| Assets/Prefabs/Ghost/Player.prefab | Assets/Prefabs/Player/Player.prefab |
| Assets/Prefabs/Player/Player.prefab | Assets/Prefabs/Player/TowerPrototype/Player.prefab |
| Assets/Scenes/MainMenu.unity | Assets/Scenes/Menu/MainMenu.unity |
| Assets/Scenes/PlayerTest.unity | Assets/Scenes/Tests/PlayerMovementTest.unity |
| Assets/Scenes/SaveSelect.unity | Assets/Scenes/Menu/SaveSelect.unity |
| Assets/Scenes/Traps/CheckpointTest.unity | Assets/Scenes/Tests/Traps/CheckpointTest.unity |
| Assets/Scenes/Traps/Concepts/EchoPlatformTest.unity | Assets/Scenes/Tests/Traps/Concepts/EchoPlatformTest.unity |
| Assets/Scenes/Traps/DoorTrap.unity | Assets/Scenes/Tests/Traps/DoorTrap.unity |
| Assets/Scenes/Traps/RotatingLaserTrap.unity | Assets/Scenes/Tests/Traps/RotatingLaserTrap.unity |
| Assets/Scenes/Traps/TurretTrap.unity | Assets/Scenes/Tests/Traps/TurretTrap.unity |
| Assets/Scripts/UI/AftertimeMenuButton.cs | Assets/Scripts/UI/MenuArtworkButton.cs |
| Assets/Settings/Rendering/New ShaderGraph Fullscreen.mat | Assets/Materials/PostProcessing/FullscreenDesaturation.mat |
| Assets/Settings/Rendering/New ShaderGraph Fullscreen.shadergraph | Assets/Shaders/PostProcessing/FullscreenDesaturation.shadergraph |
| Assets/Shaders/GhostLaserBolt.mat | Assets/Materials/Projectiles/GhostLaserBolt.mat |
| Assets/Shaders/LaserBolt.mat | Assets/Materials/Projectiles/LaserBolt.mat |
| Assets/Shaders/LaserBolt.shader | Assets/Shaders/Projectiles/LaserBolt.shader |
| Assets/Shaders/ProjectileSparks.mat | Assets/Materials/Projectiles/ProjectileSparks.mat |
| Assets/Shaders/ProjectileSparks.shader | Assets/Shaders/Projectiles/ProjectileSparks.shader |
| Assets/Shaders/ShotPressureSwitch/PressureSwitchFlat2D.mat | Assets/Materials/Level/ShotPressureSwitch.mat |
| Assets/Shaders/ShotPressureSwitch/PressureSwitchFlat2D.shader | Assets/Shaders/Traps/ShotPressureSwitch/PressureSwitchFlat2D.shader |
| Assets/Shaders/ShotPressureSwitch/PressureSwitchGhostFlat2D.shader | Assets/Shaders/Traps/ShotPressureSwitch/PressureSwitchGhostFlat2D.shader |
| Assets/Shaders/TimeSlow/TimeSlowFullscreen.shadergraph | Assets/Shaders/PostProcessing/TimeSlow/TimeSlowFullscreen.shadergraph |
| Assets/Shaders/TimeSlow/TimeSlowGray.shader | Assets/Shaders/PostProcessing/TimeSlow/TimeSlowGray.shader |
| Assets/Shaders/VerticalLaser.mat | Assets/Materials/Traps/VerticalLaser.mat |
| Assets/Shaders/VerticalLaser.shader | Assets/Shaders/Traps/VerticalLaser.shader |
| Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt | Assets/ThirdParty/TextMeshPro/Fonts/LiberationSans - OFL.txt |
| Assets/TextMesh Pro/Fonts/LiberationSans.ttf | Assets/ThirdParty/TextMeshPro/Fonts/LiberationSans.ttf |
| Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Drop Shadow.mat | Assets/ThirdParty/TextMeshPro/Resources/Fonts & Materials/LiberationSans SDF - Drop Shadow.mat |
| Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset | Assets/ThirdParty/TextMeshPro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset |
| Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Outline.mat | Assets/ThirdParty/TextMeshPro/Resources/Fonts & Materials/LiberationSans SDF - Outline.mat |
| Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset | Assets/ThirdParty/TextMeshPro/Resources/Fonts & Materials/LiberationSans SDF.asset |
| Assets/TextMesh Pro/Resources/LineBreaking Following Characters.txt | Assets/ThirdParty/TextMeshPro/Resources/LineBreaking Following Characters.txt |
| Assets/TextMesh Pro/Resources/LineBreaking Leading Characters.txt | Assets/ThirdParty/TextMeshPro/Resources/LineBreaking Leading Characters.txt |
| Assets/TextMesh Pro/Resources/Style Sheets/Default Style Sheet.asset | Assets/ThirdParty/TextMeshPro/Resources/Style Sheets/Default Style Sheet.asset |
| Assets/TextMesh Pro/Resources/TMP Settings.asset | Assets/ThirdParty/TextMeshPro/Resources/TMP Settings.asset |
| Assets/TextMesh Pro/Shaders/SDFFunctions.hlsl | Assets/ThirdParty/TextMeshPro/Shaders/SDFFunctions.hlsl |
| Assets/TextMesh Pro/Shaders/TMPro.cginc | Assets/ThirdParty/TextMeshPro/Shaders/TMPro.cginc |
| Assets/TextMesh Pro/Shaders/TMPro_Mobile.cginc | Assets/ThirdParty/TextMeshPro/Shaders/TMPro_Mobile.cginc |
| Assets/TextMesh Pro/Shaders/TMPro_Properties.cginc | Assets/ThirdParty/TextMeshPro/Shaders/TMPro_Properties.cginc |
| Assets/TextMesh Pro/Shaders/TMPro_Surface.cginc | Assets/ThirdParty/TextMeshPro/Shaders/TMPro_Surface.cginc |
| Assets/TextMesh Pro/Shaders/TMP_Bitmap-Custom-Atlas.shader | Assets/ThirdParty/TextMeshPro/Shaders/TMP_Bitmap-Custom-Atlas.shader |
| Assets/TextMesh Pro/Shaders/TMP_Bitmap-Mobile.shader | Assets/ThirdParty/TextMeshPro/Shaders/TMP_Bitmap-Mobile.shader |
| Assets/TextMesh Pro/Shaders/TMP_Bitmap.shader | Assets/ThirdParty/TextMeshPro/Shaders/TMP_Bitmap.shader |
| Assets/TextMesh Pro/Shaders/TMP_SDF Overlay.shader | Assets/ThirdParty/TextMeshPro/Shaders/TMP_SDF Overlay.shader |
| Assets/TextMesh Pro/Shaders/TMP_SDF SSD.shader | Assets/ThirdParty/TextMeshPro/Shaders/TMP_SDF SSD.shader |
| Assets/TextMesh Pro/Shaders/TMP_SDF-HDRP LIT.shadergraph | Assets/ThirdParty/TextMeshPro/Shaders/TMP_SDF-HDRP LIT.shadergraph |
| Assets/TextMesh Pro/Shaders/TMP_SDF-HDRP UNLIT.shadergraph | Assets/ThirdParty/TextMeshPro/Shaders/TMP_SDF-HDRP UNLIT.shadergraph |
| Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile Masking.shader | Assets/ThirdParty/TextMeshPro/Shaders/TMP_SDF-Mobile Masking.shader |
| Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile Overlay.shader | Assets/ThirdParty/TextMeshPro/Shaders/TMP_SDF-Mobile Overlay.shader |
| Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile SSD.shader | Assets/ThirdParty/TextMeshPro/Shaders/TMP_SDF-Mobile SSD.shader |
| Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile-2-Pass.shader | Assets/ThirdParty/TextMeshPro/Shaders/TMP_SDF-Mobile-2-Pass.shader |
| Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader | Assets/ThirdParty/TextMeshPro/Shaders/TMP_SDF-Mobile.shader |
| Assets/TextMesh Pro/Shaders/TMP_SDF-Surface-Mobile.shader | Assets/ThirdParty/TextMeshPro/Shaders/TMP_SDF-Surface-Mobile.shader |
| Assets/TextMesh Pro/Shaders/TMP_SDF-Surface.shader | Assets/ThirdParty/TextMeshPro/Shaders/TMP_SDF-Surface.shader |
| Assets/TextMesh Pro/Shaders/TMP_SDF-URP Lit.shadergraph | Assets/ThirdParty/TextMeshPro/Shaders/TMP_SDF-URP Lit.shadergraph |
| Assets/TextMesh Pro/Shaders/TMP_SDF-URP Unlit.shadergraph | Assets/ThirdParty/TextMeshPro/Shaders/TMP_SDF-URP Unlit.shadergraph |
| Assets/TextMesh Pro/Shaders/TMP_SDF.shader | Assets/ThirdParty/TextMeshPro/Shaders/TMP_SDF.shader |
| Assets/TextMesh Pro/Shaders/TMP_Sprite.shader | Assets/ThirdParty/TextMeshPro/Shaders/TMP_Sprite.shader |
| Assets/_Recovery/0 (2).unity | Assets/Scenes/Tests/PlayerSandbox.unity |
| Assets/_Recovery/Mapx11.unity | Assets/Scenes/Levels/Mapx11.unity |
| Assets/_Recovery/TrapTesting.unity | Assets/Scenes/Tests/TrapTesting.unity |

## 清出 Assets 的清单

- Assets/Art/AbandonedOutpost/character/character animations.txt
- Assets/Art/AbandonedOutpost/character/character-64x64.png
- Assets/Art/AbandonedOutpost/character/character-armed-64x64.png
- Assets/Art/AbandonedOutpost/Editor/CameraSetup.request.txt
- Assets/Art/AbandonedOutpost/Editor/OutpostCameraComfort.cs
- Assets/Art/AbandonedOutpost/Editor/OutpostCameraSetup.cs
- Assets/Art/AbandonedOutpost/Editor/OutpostMotionClarity.cs
- Assets/Art/AbandonedOutpost/Editor/OutpostPaintingSetup.cs
- Assets/Art/AbandonedOutpost/Editor/OutpostQuickStart.cs
- Assets/Art/AbandonedOutpost/Editor/OutpostWallLayers.cs
- Assets/Art/AbandonedOutpost/Editor/QuickStart.request.txt
- Assets/Art/AbandonedOutpost/Editor/Setup.request.txt
- Assets/Art/AbandonedOutpost/Editor/WallLayers.request.txt
- Assets/Art/AbandonedOutpost/enemies/enemy animations.txt
- Assets/Art/AbandonedOutpost/enemies/enemy-01-62x33.png
- Assets/Art/AbandonedOutpost/enemies/enemy-02-20x16.png
- Assets/Art/AbandonedOutpost/enemies/enemy-03-32x32.png
- Assets/Art/AbandonedOutpost/items/item1.png
- Assets/Art/AbandonedOutpost/items/item2.png
- Assets/Art/AbandonedOutpost/items/item3.png
- Assets/Art/AbandonedOutpost/items/item4.png
- Assets/Art/AbandonedOutpost/items/item5.png
- Assets/Art/AbandonedOutpost/items/item6.png
- Assets/Art/AbandonedOutpost/items/item7.png
- Assets/Art/AbandonedOutpost/license.txt
- Assets/Art/AbandonedOutpost/props/locker-1.png
- Assets/Art/AbandonedOutpost/props/locker-2.png
- Assets/Art/AbandonedOutpost/props/locker-3.png
- Assets/Art/AbandonedOutpost/props/locker-4.png
- Assets/Art/AbandonedOutpost/props/npc-dead-1.png
- Assets/Art/AbandonedOutpost/props/npc-dead-2.png
- Assets/Art/AbandonedOutpost/props/npc-head.png
- Assets/Art/AbandonedOutpost/props/npc-headless.png
- Assets/Art/AbandonedOutpost/ReadyToPaint/Backups/Mapx11_BeforeCamera_20261123_224731.unity
- Assets/Art/AbandonedOutpost/ReadyToPaint/Backups/Mapx11_BeforeCamera_20261123_224841.unity
- Assets/Art/AbandonedOutpost/ReadyToPaint/Backups/Mapx11_BeforeClarity_20261123_230011.unity
- Assets/Art/AbandonedOutpost/ReadyToPaint/Backups/Mapx11_BeforeComfort_20261123_230538.unity
- Assets/Art/AbandonedOutpost/ReadyToPaint/Backups/Mapx11_BeforeQuickStart_20261123_200049.unity
- Assets/Art/AbandonedOutpost/ReadyToPaint/Backups/Mapx11_BeforeWallLayers_20261123_201430.unity
- Assets/Art/AbandonedOutpost/ReadyToPaint/Backups/Mapx11_BeforeWallLayers_20261123_201507.unity
- Assets/Art/AbandonedOutpost/ReadyToPaint/Backups/OutpostPlayerCamera_BeforeComfort_20261123_230519.txt
- Assets/Art/AbandonedOutpost/ReadyToPaint/Backups/PlayerColorRT_BeforeClarity_20261123_230011.txt
- Assets/Art/AbandonedOutpost/ReadyToPaint/CameraComfortCheck.json
- Assets/Art/AbandonedOutpost/ReadyToPaint/CameraRuntimeCheck.json
- Assets/Art/AbandonedOutpost/ReadyToPaint/CameraSetupReport.json
- Assets/Art/AbandonedOutpost/ReadyToPaint/Maps/Mapx11_PaintReady.unity
- Assets/Art/AbandonedOutpost/ReadyToPaint/MotionClarityCheck.json
- Assets/Art/AbandonedOutpost/ReadyToPaint/Palettes/AO_01_Terrain.prefab
- Assets/Art/AbandonedOutpost/ReadyToPaint/Palettes/AO_02_Backgrounds.prefab
- Assets/Art/AbandonedOutpost/ReadyToPaint/Palettes/AO_03_Structures.prefab
- Assets/Art/AbandonedOutpost/ReadyToPaint/Palettes/AO_04_Props.prefab
- Assets/Art/AbandonedOutpost/ReadyToPaint/Prefabs/Outpost_MapGrid.prefab
- Assets/Art/AbandonedOutpost/ReadyToPaint/QuickStartReport.json
- Assets/Art/AbandonedOutpost/ReadyToPaint/RuntimeCheck.json
- Assets/Art/AbandonedOutpost/ReadyToPaint/SetupReport.json
- Assets/Art/AbandonedOutpost/ReadyToPaint/Tiles/AO_01_Terrain.asset
- Assets/Art/AbandonedOutpost/ReadyToPaint/Tiles/AO_02_Backgrounds.asset
- Assets/Art/AbandonedOutpost/ReadyToPaint/Tiles/AO_03_Structures.asset
- Assets/Art/AbandonedOutpost/ReadyToPaint/Tiles/AO_04_Props.asset
- Assets/Art/AbandonedOutpost/ReadyToPaint/WallLayersRuntimeCheck.json
- Assets/Art/AbandonedOutpost/ReadyToPaint/WallLayersSetup.json
- Assets/Art/AbandonedOutpost/resources/resources1.png
- Assets/Art/AbandonedOutpost/resources/resources2.png
- Assets/Art/AbandonedOutpost/resources/resources3.png
- Assets/Art/AbandonedOutpost/resources/resources4.png
- Assets/Art/AbandonedOutpost/resources/resources5.png
- Assets/Art/AbandonedOutpost/resources/resources6.png
- Assets/Art/AbandonedOutpost/resources/resources7.png
- Assets/Art/AbandonedOutpost/spritesheets/animated-laser-rail-horizontal-16x16.png
- Assets/Art/AbandonedOutpost/spritesheets/animated-laser-rail-vertical-16x16.png
- Assets/Art/AbandonedOutpost/spritesheets/animated-laser-wall-horizontal-16x16.png
- Assets/Art/AbandonedOutpost/spritesheets/animated-laser-wall-vertical-16x16.png
- Assets/Art/AbandonedOutpost/spritesheets/bullet fire effect-Sheet - 16x16.png
- Assets/Art/AbandonedOutpost/spritesheets/bullet.png
- Assets/Art/AbandonedOutpost/spritesheets/chest-18x15.png
- Assets/Art/AbandonedOutpost/spritesheets/chest-small-15x14.png
- Assets/Art/AbandonedOutpost/spritesheets/door front-48x48.png
- Assets/Art/AbandonedOutpost/spritesheets/door side-32x32.png
- Assets/Art/AbandonedOutpost/spritesheets/door tiles-Sheet-16x16.png
- Assets/Art/AbandonedOutpost/spritesheets/elevator platform-1-34x8.png
- Assets/Art/AbandonedOutpost/spritesheets/elevator platform-2-32x23.png
- Assets/Art/AbandonedOutpost/spritesheets/impact-27x26.png
- Assets/Art/AbandonedOutpost/spritesheets/moving platform-1-48x16.png
- Assets/Art/AbandonedOutpost/spritesheets/moving platform-2-48x32.png
- Assets/Art/AbandonedOutpost/spritesheets/pc-20x29.png
- Assets/Art/AbandonedOutpost/spritesheets/platform-trapdoor-sheet-16x16.png
- Assets/Art/AbandonedOutpost/spritesheets/switch-21x18.png
- Assets/Art/AbandonedOutpost/tilesets/animated tiles-lamp-vertical16x16.png
- Assets/Art/AbandonedOutpost/tilesets/animated tiles-lamps-horizontal-16x16.png
- Assets/Art/AbandonedOutpost/tilesets/animated tiles-water-blue-16x16.png
- Assets/Art/AbandonedOutpost/tilesets/animated tiles-water-red-16x16.png
- Assets/Art/AbandonedOutpost/tilesets/tiles-fan-large-16x16.png
- Assets/Art/AbandonedOutpost/tilesets/tiles-fan-small-16x16.png
- Assets/Art/AbandonedOutpost/tilesets/tiles-pipes-16x16.png
- Assets/Art/AbandonedOutpost/tilesets/tiles-props-16x16.png
- Assets/Art/AbandonedOutpost/tilesets/tiles-rail(laser)-16x16.png
- Assets/Art/AbandonedOutpost/tilesets/tiles-rail(platform)-16x16.png
- Assets/Art/AbandonedOutpost/tilesets/tiles-window-16x16.png
- Assets/Art/AbandonedOutpost/tilesets/tileset-bg-1-16x16.png
- Assets/Art/AbandonedOutpost/tilesets/tileset-bg-2-16x16.png
- Assets/Art/AbandonedOutpost/tilesets/tileset-elevator-16x16.png
- Assets/Art/AbandonedOutpost/tilesets/tileset-platforms-1-16x16.png
- Assets/Art/AbandonedOutpost/tilesets/tileset-platforms-2-16x16.png
- Assets/Art/AbandonedOutpost/tilesets/tilesets-platforms-16x16.png
- Assets/Art/AbandonedOutpost/UNITY-IMPORT.txt
- Assets/Art/UI/Aftertime/OFL.txt
- Assets/Art/UI/Aftertime/AftertimeMenu.shader
- Assets/Art/UI/Aftertime/Architecture.png
- Assets/Art/UI/Aftertime/Backdrop.mat
- Assets/Art/UI/Aftertime/Buttons.png
- Assets/Art/UI/Aftertime/CRT Overlay.mat
- Assets/Art/UI/Aftertime/CyanCrystal.png
- Assets/Art/UI/Aftertime/VioletCrystal.png
- Assets/_Recovery/0.unity
- Assets/_Recovery/0 (1).unity
- Assets/Scenes/Recovery/PlayerTest_Recovery.unity
- Assets/Scenes/SampleScene.unity
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_10.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_11.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_12.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_13.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_14.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_15.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_16.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_17.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_18.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_19.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_20.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_21.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_22.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_23.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_24.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_25.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_26.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_27.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_28.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_29.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_3.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_30.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_31.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_32.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_33.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_34.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_35.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_36.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_37.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_38.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_39.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_40.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_41.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_42.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_43.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_44.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_45.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_46.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_47.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_48.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_49.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_5.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_50.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_51.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_52.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_53.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_54.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_55.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_56.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_57.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_58.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_59.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_6.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_60.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_61.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_62.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_63.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_64.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_65.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_66.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_67.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_68.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_69.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_7.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_8.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_All_Ground_9.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_52.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_53.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_54.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_55.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_56.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_57.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_58.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_59.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_Detail_Ground_60.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_Fix_Ground_15.asset
- Assets/Art/SciFiLab/Geometry/Mapx11_Fix_Ground_8.asset
- Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_16.png
- Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_17.png
- Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_26.png
- Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_27.png
- Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_18.png
- Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_19.png
- Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_24.png
- Assets/Art/Player/Ghost/assassin_2_skill_1_effect_0_25.png
