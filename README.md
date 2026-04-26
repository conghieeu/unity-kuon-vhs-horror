# 🏯 TAKAYAGI — THE AWAKENING

<div align="center">

**Indie Horror | PS1-Style | Slow-burn | First-Person**

_Thị trấn Takayagi, Nhật Bản — Nơi quá khứ linh thiêng không bao giờ thực sự ngủ yên._

[![Unity](https://img.shields.io/badge/Unity-6000.x-000000?logo=unity&logoColor=white)](https://unity.com/)
[![URP](https://img.shields.io/badge/Render_Pipeline-URP_17.3-blue)](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.0/manual/index.html)
[![Platform](https://img.shields.io/badge/Platform-Windows-0078D6?logo=windows)](https://store.steampowered.com/)
[![Steam](https://img.shields.io/badge/Steam-Integrated-1b2838?logo=steam)](https://partner.steamgames.com/)

</div>

---

## 📖 Giới Thiệu

**Takayagi — The Awakening** là một tựa game **kinh dị góc nhìn thứ nhất (FPS Horror)** lấy bối cảnh tại một thị trấn nhỏ ở Nhật Bản. Game mang phong cách **PS1 retro** với hiệu ứng VHS, grain, và low-resolution — tạo nên bầu không khí u ám, hoài cổ đặc trưng của thể loại horror indie.

Người chơi vào vai một người nước ngoài đến Takayagi để bắt đầu cuộc sống mới, nhưng nhanh chóng phát hiện rằng thị trấn này ẩn chứa những bí mật rùng rợn mà không ai dám nói ra.

### 🎮 Đặc Điểm Nổi Bật

- **Slow-burn Horror** — Xây dựng sợ hãi từ từ qua âm thanh, ánh sáng và bầu không khí
- **PS1-Style Visuals** — Hiệu ứng retro: VHS grain, low-resolution, CRT scanlines
- **Spatial Audio (3D Binaural)** — Âm thanh 3D tạo cảm giác ám ảnh trong không gian
- **Dialogue với nhiều lựa chọn** — Tương tác hội thoại ảnh hưởng đến phản hồi NPC
- **Jump Scare có chiều sâu** — Không spam jump scare, mỗi pha đều được dàn dựng kỹ lưỡng
- **Bối cảnh Nhật Bản chân thực** — Thành phố, chung cư, đường phố kiểu Nhật

---

## 🗺️ Cấu Trúc Cảnh (Game Flow)

```
CẢNH 1: Chuyến Taxi Đến Takayagi
  └─ Dialogue Tree với tài xế → Thanh toán → Đến chung cư

CẢNH 2: Căn Hộ Số 302
  └─ Chơi game trên PC → Đi ngủ → Sự kiện 3:00 AM
  └─ Tiếng thì thầm + Ánh sáng kỳ lạ → Van nước → Jump Scare

CẢNH 3: Ngày Làm Việc Đầu Tiên
  └─ Tin nhắn smartphone → Đi làm → Cutscene → Tutorial

CẢNH 4: Đường Về & Bà Lão Vô Minh
  └─ NPC Encounter → Jump Scare kinh hoàng → Chạy trốn vào sảnh
```

---

## 🛠️ Công Nghệ & Framework

### Core Engine

| Thành phần            | Phiên bản               |
| --------------------- | ----------------------- |
| **Unity**             | 6000.x (Unity 6)        |
| **Render Pipeline**   | URP 17.3.0              |
| **Input System**      | New Input System 1.18.0 |
| **Scripting Backend** | IL2CPP                  |
| **Target Platform**   | Windows (Standalone)    |

### Framework Chính

| Framework                                                                            | Vai trò                                                                                       |
| ------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------- |
| **[UHFPS](https://docs.twgamesdev.com/uhfps)**                                       | Core template — Player, Camera, Inventory, Jump Scare, Save/Load, AI, Puzzle, Dynamic Objects |
| **[Pixel Crushers Dialogue System](https://www.pixelcrushers.com/dialogue-system/)** | Hệ thống hội thoại với cây lựa chọn (Dialogue Trees)                                          |
| **[Steamworks.NET](https://steamworks.github.io/)**                                  | Tích hợp Steam (Achievements, Overlay, Cloud Save)                                            |

### Plugin & Tools

| Plugin                     | Mô tả                                      |
| -------------------------- | ------------------------------------------ |
| **VHS Pro URP**            | Hiệu ứng VHS/CRT retro — PS1-style visuals |
| **Odin Inspector**         | Inspector nâng cao cho Unity Editor        |
| **Quantum Console (QFSW)** | In-game debug console                      |
| **Cinemachine 3.1**        | Camera system (cutscenes, virtual cameras) |
| **Timeline**               | Cinematic sequencing                       |
| **ProBuilder**             | 3D prototyping trong editor                |
| **AI Navigation**          | NavMesh cho AI NPC                         |
| **ShaderGraph**            | Custom shader authoring                    |
| **Hot Reload**             | Live code reload (dev only)                |

### Asset Packs

| Asset                      | Mô tả                         |
| -------------------------- | ----------------------------- |
| **Japanese City Megapack** | Môi trường thành phố Nhật Bản |
| **AllSky Free**            | Skybox                        |

---

## 📁 Cấu Trúc Dự Án

```
Assets/
│
├── _Project/                     # ⭐ TẤT CẢ asset do team tạo ra
│   ├── Animations/               #   Animation clips, Animator Controllers
│   ├── Audio/                    #   Nhạc, SFX, Ambience
│   ├── Docs/                     #   Tài liệu nội bộ
│   ├── Fonts/                    #   Font chữ
│   ├── Materials/                #   Material, Shader Graph
│   ├── Models/                   #   Model 3D (FBX, OBJ...)
│   ├── Prefabs/                  #   Prefab gameplay
│   │   ├── GAMEMANAGER.prefab    #     GameManager (UI, Systems, Modules)
│   │   ├── HEROPLAYER.prefab     #     Nhân vật chính (FPS Player)
│   │   └── Interactables/        #     Items cầm tay (đèn pin, rìu, ...)
│   ├── Scenes/                   #   Scenes của game
│   ├── Scripts/                  #   Code C# tự viết
│   │   └── NodeCanvasTasks/      #     Custom NodeCanvas actions
│   ├── Timelines/                #   Timeline .playable assets
│   │   └── Chapter01/            #     TL_CH01_S01_Tunnel.playable, ...
│   └── UI/                       #   Sprite, UI assets
│
│   ── Thư mục do Unity/Plugin tạo — KHÔNG di chuyển ──
│
├── Data/                         # ⚠️ UHFPS save/config (hardcode path)
│   ├── Config/
│   └── SavedGame/
├── Dialogs/                      # ⚠️ Pixel Crushers Dialogue Database
├── Editor Default Resources/     # ⚠️ Unity Special Folder (Editor UI cho plugin)
├── Gizmos/                       # ⚠️ Unity Special Folder (icon Scene View)
├── HierarchyDecorator/           # ⚠️ Settings của plugin HierarchyDecorator
│
│   ── ThirdParty & Unity Folders ──
│
├── ThirdParty/                   # Asset tải về từ Asset Store / GitHub
│   ├── NodeCanvas/
│   ├── UHFPS/
│   └── ...
├── Plugins/                      # DLL, native plugins
├── Resources/                    # Unity Resources.Load() runtime
└── Settings/                     # URP, Input, v.v.

Packages/ (ngang cấp Assets — không nằm trong Assets)
└── com.rlabrecque.steamworks.net/ # Steamworks.NET (Unity Package)
```

> 📄 Xem chi tiết tại: `Assets/_Project/Docs/folder-structure-guide.txt`

---

## 🎯 Hệ Thống Gameplay (UHFPS)

### Player Systems

- **PlayerStateMachine** — FSM: Walk, Sprint, Crouch, Lean, Slide, Climb
- **LookController** — Camera FPS (mouse look, sensitivity, clamping)
- **InteractController** — Raycast interaction (nhấn [E])
- **HeadBobController** — Hiệu ứng lắc đầu khi di chuyển
- **PlayerHealth** — Hệ thống HP + hiệu ứng máu
- **FootstepsSystem** — Âm thanh bước chân theo vật liệu

### Horror Systems

- **JumpscareManager** — Direct (hình ảnh/model), Indirect (animation), Audio
- **JumpscareTrigger** — TriggerEnter/Exit/Event + Wobble + Fear + LookAt
- **FlickeringLight** — Đèn chớp tắt
- **FearTentacles** — Hiệu ứng kinh dị viền màn hình
- **WobbleMotion** — Camera rung lắc

### World Interaction

- **DynamicObject** — Cửa, ngăn kéo, van nước, công tắc
- **InteractableItem** — Nhặt/sử dụng đồ vật
- **Inventory** — Túi đồ + Shortcut slots
- **ExamineController** — Xem xét vật thể 3D

### Narrative

- **Pixel Crushers Dialogue System** — Cây hội thoại đa nhánh
- **ObjectiveManager** — Hệ thống nhiệm vụ
- **CutsceneTrigger + Timeline** — Cắt cảnh

### Technical

- **SaveGameManager** — Save/Load (AES encryption)
- **InputManager** — New Input System wrapper
- **GameManager** — State management, UI panels, freeze player

---

## 🚀 Bắt Đầu

### Yêu Cầu Hệ Thống (Development)

- **Unity** 6000.x trở lên
- **OS**: Windows 10/11
- **RAM**: 16 GB trở lên (khuyến nghị)

### Cài Đặt

1. Clone repository:

   ```bash
   git clone https://github.com/conghieeu/kuon.git
   ```

2. Mở project bằng **Unity Hub** → Add project from disk

3. Chờ Unity import tất cả assets và compile scripts

4. Mở scene chính:

   ```
   Assets/_Project/Scenes/
   ```

5. Nhấn **Play** để test

### Scripting Defines (đã cấu hình)

```
UNITY_POST_PROCESSING_STACK_V2
UHFPS_LOCALIZATION
ODIN_INSPECTOR
STEAMWORKS_NET
TMP_PRESENT
USE_NEW_INPUT
```

---

## 📝 Quy Ước Code & Asset

### Naming Convention

| Loại           | Format                              | Ví dụ                        |
| -------------- | ----------------------------------- | ---------------------------- |
| **Scripts**    | PascalCase                          | `PlayPlayableDirector.cs`    |
| **Prefabs**    | `PFB_` + PascalCase                 | `PFB_EnemyZombie.prefab`     |
| **Materials**  | `MAT_` + PascalCase                 | `MAT_WoodFloor.mat`          |
| **Timelines**  | `TL_CH{n}_S{n}_` + PascalCase       | `TL_CH01_S01_Tunnel.playable`|
| **Audio SFX**  | `SFX_` + PascalCase                 | `SFX_DoorCreak.wav`          |
| **Audio Music**| `MUS_` + PascalCase                 | `MUS_MainTheme.mp3`          |
| **Scenes**     | `SCN_` + PascalCase                 | `SCN_Chapter01_Tunnel.unity` |
| **Animations** | `ANIM_` + PascalCase                | `ANIM_PlayerWalk.anim`       |

### Quy Tắc Code

- **Namespace**: `Kuon` cho scripts tự viết, `UHFPS.Runtime` cho extensions
- **Ngôn ngữ comment**: Tiếng Việt (cho team nội bộ)
- **Console Commands**: Sử dụng `[Command("tên")]` từ Quantum Console để debug
- **Naming**: PascalCase cho class/method, camelCase cho biến local

---

## 🗓️ Roadmap

- [x] Setup UHFPS framework
- [x] Tích hợp Pixel Crushers Dialogue System
- [x] Tích hợp Steamworks.NET
- [x] Cấu hình VHS/PS1 visual effects
- [x] Bridge UHFPS ↔ Dialogue System (UHFPSDialogueBridge)
- [x] QuickItemAdder utility
- [x] Tái cấu trúc thư mục theo chuẩn `_Project/`
- [ ] **Cảnh 2**: Căn hộ 302 — Environment + 3AM Event + Jump Scare
- [ ] **Cảnh 1**: Chuyến Taxi — Cutscene + Dialogue + Payment UI
- [ ] **Cảnh 3**: Ngày làm việc — Smartphone UI + Cutscene + Tutorial
- [ ] **Cảnh 4**: Bà lão — NPC + Jump Scare + Auto Sprint
- [ ] Polish: Audio, Post-processing, Pacing
- [ ] Steam build + Testing

---

## 👥 Team

Dự án phát triển bởi team indie Việt Nam.

---

## 📜 License

Dự án sử dụng các asset có bản quyền (UHFPS, Pixel Crushers, Odin Inspector, etc.).
Không phân phối lại mã nguồn của các plugin thương mại.

---

<div align="center">

_"Thị trấn này... từng là một nơi linh thiêng đấy."_

**🕯️ TAKAYAGI — THE AWAKENING 🕯️**

</div>
