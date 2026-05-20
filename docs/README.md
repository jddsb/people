# testA Wheel Runner 项目文档

## 项目概述

这是一个 Unity 跑酷原型项目，核心玩法是控制角色骑着彩色轮子在无尽循环跑道上前进。玩家通过左右移动选择不同颜色的地块和颜色挡板，轮子的颜色和高度会随交互结果变化。

项目当前启动场景为 `Assets/testA/Scenes/testA.scene`，启动脚本为 `Assets/testA/Scripts/WheelRunnerBootstrap.cs`。

## 启动入口

### 启动场景

- 场景路径：`Assets/testA/Scenes/testA.scene`
- Build Settings 中已配置该场景为启用场景。
- 场景中挂载了 `WheelRunnerBootstrap` 组件，运行后由脚本动态生成游戏所需对象。

### 启动脚本

- 脚本路径：`Assets/testA/Scripts/WheelRunnerBootstrap.cs`
- 组件名称：`WheelRunnerBootstrap`
- 生命周期入口：
  - `Awake()`：初始化颜色、轮子半径、材质、场景世界和 UI。
  - `Update()`：处理输入、移动、地块检测、挡板检测和 UI 刷新。
  - `LateUpdate()`：跟随角色更新摄像机位置。

## 运行方式

1. 使用 Unity 打开项目根目录。
2. 打开场景 `Assets/testA/Scenes/testA.scene`。
3. 点击 Play 运行。

如果需要重新生成场景，可在 Unity 菜单中执行：

`Tools/testA/Build Wheel Runner Scene`

该菜单由 `Assets/testA/Scripts/Editor/TestAWheelRunnerSceneBuilder.cs` 提供，会创建材质、生成启动场景，并把场景写入 Build Settings。

## 操作说明

- 鼠标左键按住并左右拖动：控制角色横向移动。
- 键盘方向键或 `A` / `D`：控制角色横向移动。
- `1`：切换轮子为绿色。
- `2`：切换轮子为蓝色。
- `3`：切换轮子为黄色。
- `R`：重新加载当前场景。

## 核心玩法

### 轮子颜色

轮子颜色由 `WheelRunnerColor` 枚举表示，支持三种颜色：

- `Green`
- `Blue`
- `Yellow`

颜色会影响玩家经过颜色地块时的结果，也会被颜色挡板改变。

### 颜色地块

跑道上分布多组颜色地块：

- 轮子颜色与地块颜色相同：轮子高度增加，分数增加。
- 轮子颜色与地块颜色不同：轮子高度降低，速度立即下降，之后继续按原有加速逻辑越跑越快，分数减少但不会低于 0。

轮子高度由 `initialWheelRadius`、`radiusStep`、`minWheelRadius`、`maxWheelRadius` 控制。
踩错颜色地块的降速由 `mismatchSlowdownMultiplier` 控制，降速后的重新加速由 `postSlowdownAcceleration` 控制。

### 颜色挡板

跑道上定距生成颜色挡板。角色通过挡板后：

- 轮子切换为挡板颜色。
- 分数增加。
- UI 显示 `Very Good!`。

### 无尽循环跑道

跑道由多个循环段组成。角色向前移动时，后方跑道段会被移动到前方继续复用，形成无尽跑道效果。

相关常量位于 `WheelRunnerTrack.cs`：

- `TrackWidth`：跑道宽度。
- `TrackLoopStartZ`：循环跑道起始 Z 坐标。
- `TrackLoopLength`：单段循环跑道长度。
- `LoopSegmentCount`：循环段数量。
- `LoopSegmentRecycleMargin`：回收跑道段的缓冲距离。

## 主要目录结构

```text
Assets/
  testA/
    Arts/
      Pad_Blue.mat
      Pad_Yellow.mat
      Track_Pastel.mat
      Wheel_Green.mat
    Scenes/
      testA.scene
    Scripts/
      WheelRunnerBootstrap.cs
      WheelRunnerCharacter.cs
      WheelRunnerGameplay.cs
      WheelRunnerMaterials.cs
      WheelRunnerTrack.cs
      WheelRunnerUi.cs
      WheelRunnerWorld.cs
      Editor/
        TestAWheelRunnerSceneBuilder.cs
  CrowdRunnerPrototype.cs
Packages/
  manifest.json
ProjectSettings/
  EditorBuildSettings.asset
```

说明：

- `Assets/testA/Scenes/testA.scene` 是当前主场景。
- `Assets/testA/Scripts/WheelRunnerBootstrap.cs` 是当前主入口脚本。
- `Assets/testA/Scripts/WheelRunner*.cs` 按职责拆分跑道、角色、UI、玩法和材质逻辑。
- `Assets/testA/Scripts/Editor/TestAWheelRunnerSceneBuilder.cs` 是编辑器工具脚本，用于生成或修复 testA 场景。
- `Assets/testA/Arts` 存放当前场景引用的基础材质。
- `Assets/CrowdRunnerPrototype.cs` 是另一个跑酷原型脚本，目前其自动启动逻辑已被注释，不是当前 testA 启动入口。

## 脚本职责

### WheelRunnerBootstrap 与 WheelRunner 拆分文件

入口组件 `WheelRunnerBootstrap` 负责运行时生命周期和序列化配置，其余 `WheelRunner*.cs` 文件按职责拆分运行时逻辑：

- 创建灯光和主摄像机。
- 创建循环跑道、墙体、条纹、颜色地块和颜色挡板。
- 创建轮子和角色模型。
- 创建分数、高度、提示文本和进度条 UI。
- 处理鼠标、键盘输入。
- 管理角色前进、横向移动和摄像机跟随。
- 判断颜色地块和颜色挡板交互。
- 刷新分数、圈数、颜色、高度和进度显示。

### TestAWheelRunnerSceneBuilder

该脚本仅在 Unity Editor 中使用：

- 确保 `Assets/testA/Arts`、`Assets/testA/Scripts`、`Assets/testA/Scenes` 目录存在。
- 创建或更新项目材质。
- 新建空场景并挂载 `WheelRunnerBootstrap`。
- 将材质引用写入组件序列化字段。
- 保存场景到 `Assets/testA/Scenes/testA.scene`。
- 将该场景设置到 Build Settings。

## 可调参数

在 `WheelRunnerBootstrap` 组件 Inspector 中可以调整以下玩法参数：

- `Initial Wheel Color`：初始轮子颜色。
- `Forward Speed`：初始前进速度。
- `Max Forward Speed`：最高前进速度。
- `Speed Ramp Distance`：速度爬升距离。
- `Mismatch Slowdown Multiplier`：踩到不同颜色地块时当前速度的保留倍率，数值越小减速越明显。
- `Post Slowdown Acceleration`：踩错减速后重新追赶目标速度的加速度。
- `Horizontal Speed`：键盘横向移动速度。
- `Drag Sensitivity`：鼠标拖动灵敏度。
- `Initial Wheel Radius`：初始轮子半径。
- `Radius Step`：每次地块交互改变的轮子半径。
- `Min Wheel Radius`：最小轮子半径。
- `Max Wheel Radius`：最大轮子半径。

材质字段也可在 Inspector 中替换。如果材质未绑定，脚本会在运行时创建备用材质，保证原型可运行。

## UI 显示

运行时会动态生成 `Game UI`，包含：

- 左上角分数和圈数。
- 右上角当前颜色和轮子高度。
- 顶部进度条。
- 底部提示文本。

进度条显示当前角色在本圈循环跑道中的位置，填充颜色会跟随轮子颜色变化。

## 维护建议

- 如果只调整玩法数值，优先修改 `WheelRunnerBootstrap` 的序列化字段或相关常量。
- 如果调整跑道内容，重点修改 `BuildColorPadsForSegment()` 和 `BuildColorBafflesForSegment()`。
- 如果调整角色外观，重点修改 `BuildCharacter()`。
- 如果调整 UI，重点修改 `BuildUi()`、`CreateText()` 和 `UpdateUi()`。
- 如果场景丢失组件或材质引用，可重新执行 `Tools/testA/Build Wheel Runner Scene`。

## 常见问题

### 运行后看不到内容

确认打开的是 `Assets/testA/Scenes/testA.scene`，并且场景中存在挂载 `WheelRunnerBootstrap` 的对象。

### 材质缺失或颜色不正确

可执行 `Tools/testA/Build Wheel Runner Scene` 重新生成材质和场景引用。脚本本身也包含运行时备用材质逻辑。

### 修改场景后运行时对象被覆盖

`WheelRunnerBootstrap` 会在 `Awake()` 中动态生成灯光、摄像机、跑道、角色和 UI。若需要保留手工摆放对象，需要先调整脚本的生成逻辑。
