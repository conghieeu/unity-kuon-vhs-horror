# Project Context

## 1. Stack & Versions
Dự án được xây dựng trên Unity Game Engine, sử dụng kiến trúc **UHFPS (Ultimate Horror FPS - ThunderWire Studio)** làm nền tảng cốt lõi.

**Các package và thư viện chính (từ `manifest.json`):**
- **Core Framework:** ThunderWire Studio (UHFPS)
- **Universal Render Pipeline (URP):** `17.3.0`
- **Cinemachine:** `3.1.6` (Quản lý Camera)
- **Input System:** `1.19.0`
- **Newtonsoft JSON:** `3.2.2` (Dùng nhiều trong hệ thống Save/Load)
- **Localization:** `com.picoshot.localization`
- **Unity UI (uGUI):** `2.0.0`
- **ProBuilder:** `6.0.9`

## 2. Folder Structure
Cấu trúc dự án nằm gọn trong `Assets/_Project/`.

- `Animations/`, `Audio/`, `Fonts/`, `Materials/`, `Models/`: Các asset cơ bản (không chứa logic code).
- `Docs/`: Tài liệu dự án (chứa prompt, note).
- `Prefabs/`: Chứa các prefab đã setup sẵn component.
- `Scenes/`: Chứa scene game.
- `Scriptables/`: ScriptableObject data (Rất quan trọng, cấu hình Inventory, Dialogue, StateMachine, Localization).
- **`Scripts/`** (Nơi chứa toàn bộ code):
  - `Editor/`: Custom Inspector, Drawer, Window Editor.
  - `Runtime/`: Code chạy lúc chơi.
    - `Controllers/`: Điều khiển Camera, Items, Motion, Player.
    - `Core/`: Logic nền tảng (GameManager, Inventory, SaveGame, Objectives, Options, Puzzle, Dialogue).
    - `Interact/`: Mọi thứ tương tác được (VHS, Radio, Safe, Keypad, Elevator...).
    - `Trigger/`: Các trigger sự kiện (Cutscenes, GhostHunting...).
    - `UI/`: Script điều khiển giao diện.

⚠️ **Lưu ý:** KHÔNG được sửa code trong các thư mục nằm ngoài `Assets/_Project/` (ví dụ `Packages/` hay thư mục gốc của framework nếu có).

## 3. Conventions
- **Naming:**
  - File/Class/Interface: `PascalCase` (e.g., `SaveGameManager`, `ISaveable`).
  - Constant/Readonly: `UPPER_SNAKE_CASE` (e.g., `TOKEN_SEPARATOR`).
  - Public property/field: `PascalCase` (e.g., `GameLoadType`).
  - Private field: `camelCase` (e.g., `syncLoadingDone`, `objectiveManager`).
- **Import Order:** `System.*` -> `UnityEngine.*` -> `Newtonsoft.Json.*` -> `UHFPS.*` -> `ThunderWire.*`.
- **Error Handling:** 
  - Ném lỗi rõ ràng bằng `throw new FileNotFoundException(...)` hoặc `DataException`.
  - Dùng `Debug.LogError` hoặc `Debug.Log` kèm context (VD: `Debug.LogError($"[SaveGameManager] Could not find saveable...");`).
- **Asynchronous:** Dùng `Coroutine` cho UI/Gameplay flow (fading, sequence) và `async/await Task` cho File I/O (Lưu/Tải file Json).
- **UI:** Dùng `CanvasGroup` để fade in/out (thông qua `CanvasGroupFader`), dùng `TextMeshPro` để render text (Rich text).

## 4. Core Modules
- **`SaveGameManager`**: Quản lý lưu/tải game state, player data, world state bằng JSON.
- **`ObjectiveManager`**: Hệ thống theo dõi Quest/Objective (kết nối trực tiếp với UI và Save/Load).
- **`NPCQuestController`**: Chịu trách nhiệm quản lý NPC, liên kết giữa Dialogue và Objective, ngăn ngừa memory leak do subscription event.
- **`DialogueSystem`**: Xử lý rẽ nhánh hội thoại, Localization, skip text, và kích hoạt objective dựa trên sự lựa chọn.
- **`Inventory`**: Quản lý item, slot, phím tắt.
- **`Puzzle`**: Hệ thống base cực lớn cho hàng loạt puzzle (Keypad, Fusebox, Safe, Lockpick...).

## 5. Database / Data Layer
- **Không dùng SQL hay ORM truyền thống.** Toàn bộ dữ liệu được lưu dưới dạng file **JSON** thông qua `Newtonsoft.Json`.
- Dữ liệu cấu hình (Item data, Dialogue Tree, NPC Stats) được lưu cứng bằng **`ScriptableObject`**.
- **Save/Load Pattern:** Sử dụng Interface `ISaveable` (hoặc `ISaveableCustom`). Bất cứ object nào muốn lưu trạng thái đều phải implement `ISaveable`, trả về `StorableCollection` (một wrapper của Json Object) trong hàm `OnSave()`, và parse data trong hàm `OnLoad(JToken)`.
- File lưu trữ được gom thành 2 file chính trong một folder: `[SaveInfoName].json` và `[SaveDataName].json`.

## 6. Các điều AI hay làm sai với codebase này
Dựa trên lịch sử phát triển, đây là những lỗi phổ biến dễ mắc phải:
- **Tạo logic mới thay vì dùng UHFPS:** AI có xu hướng tự viết hệ thống tương tác hoặc raycast mới thay vì implement các interface có sẵn của UHFPS (như `IInteractable`, `ISaveable`).
- **Lỗi Coroutine trong Dialogue:** Khi xử lý chức năng "Skip" (bỏ qua thoại), AI hay làm sai logic vòng lặp khiến game skip toàn bộ sequence hội thoại thay vì chỉ hiện đầy đủ dòng text hiện tại.
- **Memory Leak ở Localization:** Quên unsubscribe các event của `Localization` khi destroy object UI hoặc chuyển scene.
- **Lỗi Softlock ở NPC Quest:** Quên đồng bộ trạng thái của NPC với `ObjectiveManager` (ví dụ quest đã xong nhưng NPC vẫn nói thoại giao quest), dẫn đến kẹt tiến trình game.
- **Quên lưu trạng thái (Save State):** Thêm biến mới vào script nhưng quên khai báo nó vào dictionary của `OnSave()` và `OnLoad()`, khiến game load lại bị mất dữ liệu.
