# NutriVision 架构说明（v1）

## 目标

构建一个可在 Android 手机模拟器、Android 平板模拟器、Windows 演示的 .NET MAUI 应用，形成完整闭环：

拍照 -> 识别 -> 营养分析 -> 保存历史 -> 统计展示 -> 语音播报 + 位置记录

## 分层

- `Models`
- `ViewModels`
- `Views`
- `Services`
- `Helpers`
- `Resources`

架构模式：`MVVM + Services + DI + SQLite`

## 硬件功能（高分主线）

1. Camera
2. Flash（代码控制）
3. Geolocation/Geocoding
4. Text-to-Speech
5. Vibration/Haptic
6. Shake

## 关键接口

- `IFoodRecognitionService`
- `INutritionService`
- `IHistoryRepository`
- `ILocationService`
- `ISpeechService`
- `IHapticService`
- `IShakeService`
- `ICameraService`

## 可靠性策略

- 云端识别优先，本地识别回退兜底。
- 每个硬件/网络边界都做权限检查 + try/catch + 用户友好提示。
- 关键错误场景必须可演示：相机拒绝、网络失败、API超时/空结果、定位失败、SQLite写入失败。
