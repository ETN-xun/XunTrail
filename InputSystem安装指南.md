# Unity Input System 安装指南

## 问题描述

项目中使用了 `UnityEngine.InputSystem` 命名空间，但未安装 Input System 包，导致编译错误：

```
Assets\Scripts\ToneGenerator.cs(4,19): error CS0234: The type or namespace name 'InputSystem' does not exist in the namespace 'UnityEngine' (are you missing an assembly reference?)
```

## 解决方案

### 1. 已添加 Input System 包

我已经在项目的 `manifest.json` 文件中添加了 Input System 包的引用：

```json
"com.unity.inputsystem": "1.7.0",
```

### 2. 启用新的输入系统

要完全解决问题，您需要在 Unity 编辑器中启用新的输入系统：

1. 打开 Unity 编辑器
2. 进入菜单：**Edit > Project Settings**
3. 在左侧选择 **Player**
4. 在右侧面板中找到 **Other Settings** 部分
5. 找到 **Active Input Handling** 设置
6. 将其从 **Input Manager (Old)** 更改为 **Both** 或 **Input System Package (New)**
7. Unity 会提示重启编辑器，点击 **Restart** 按钮

### 3. 重新编译项目

重启 Unity 编辑器后，项目应该能够正确识别 `UnityEngine.InputSystem` 命名空间，编译错误应该消失。

## 注意事项

- 如果选择 **Both** 选项，可以同时使用旧的和新的输入系统，这对于逐步迁移很有帮助
- 如果选择 **Input System Package (New)**，则只能使用新的输入系统，旧的输入系统代码将不再工作
- 更改输入系统设置后，可能需要更新现有的输入处理代码