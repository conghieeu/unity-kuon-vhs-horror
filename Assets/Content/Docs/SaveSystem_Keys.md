# Game Save System - ES3 Keys Reference (Tham chiếu Save Game)

## Overview (Tổng quan)
This document defines all keys used in the Easy Save 3 (ES3) save system. Each key corresponds to a specific game data component that needs to be persisted.

---

## 1. Player State (Trạng thái Nhân vật)

### Position & Rotation (Vị trí & Xoay)
| Key | Type | Description | Example |
|-----|------|-------------|---------|
| `PLAYER_POSITION` | `Vector3` | Player XYZ coordinate | `(10.5, 1.0, -25.3)` |
| `PLAYER_ROTATION` | `Quaternion` | Player view direction | `(0.0, 0.7, 0.0, 0.7)` |

### Health & Stamina (Máu & Thể lực)
| Key | Type | Description | Example |
|-----|------|-------------|---------|
| `PLAYER_HEALTH` | `float` | Current HP | `85.0` (0-100) |
| `PLAYER_MAX_HEALTH` | `float` | Maximum HP | `100.0` |
| `PLAYER_STAMINA` | `float` | Current stamina/energy | `75.5` |
| `PLAYER_MAX_STAMINA` | `float` | Maximum stamina | `100.0` |

### Equipment (Trang bị)
| Key | Type | Description | Example |
|-----|------|-------------|---------|
| `PLAYER_EQUIPPED_ITEM_ID` | `string` | ID of currently equipped item | `"flashlight_01"` or `"NONE"` |
| `PLAYER_EQUIPPED_ITEM_SLOT` | `int` | Hotbar slot number | `0`, `1`, `2`... |
| `PLAYER_FLASHLIGHT_BATTERY` | `float` | Remaining battery (0-100) | `45.0` |

---

## 2. Inventory System (Hành trang)

### Item List Structure (Cấu trúc Danh sách Vật phẩm)
```
Key: PLAYER_INVENTORY_ITEMS
Type: List<InventoryItemData>

InventoryItemData = {
  itemID: string,
  quantity: int,
  rarity: string,
  customData: Dictionary<string, object>
}
```

| Key | Type | Description |
|-----|------|-------------|
| `PLAYER_INVENTORY_ITEMS` | `List<InventoryItemData>` | All items in inventory |
| `PLAYER_INVENTORY_MAX_SLOTS` | `int` | Max inventory capacity |
| `PLAYER_INVENTORY_USED_SLOTS` | `int` | Currently used slots |

### Hotbar/Shortcuts (Thanh phím tắt)
```
Key: PLAYER_HOTBAR_SLOT_{N}
Type: string (item ID)

Example: 
PLAYER_HOTBAR_SLOT_0 = "flashlight_01"
PLAYER_HOTBAR_SLOT_1 = "key_room_201"
PLAYER_HOTBAR_SLOT_2 = "healing_kit_01"
```

| Key | Type | Description |
|-----|------|-------------|
| `PLAYER_HOTBAR_SLOT_0` to `PLAYER_HOTBAR_SLOT_9` | `string` | Item ID assigned to slot |

### Weapon-Specific Data (Dữ liệu Vũ khí)
| Key | Type | Description | Example |
|-----|------|-------------|---------|
| `WEAPON_AMMO_{WEAPON_ID}` | `int` | Ammo count for specific weapon | `30` |
| `WEAPON_MAGAZINE_{WEAPON_ID}` | `int` | Currently loaded magazine | `1` |
| `WEAPON_MAGAZINE_AMMO_{WEAPON_ID}_{MAG_NUM}` | `int` | Ammo in specific magazine | `25` |

---

## 3. Narrative & Quests (Cốt truyện & Nhiệm vụ)

### Dialogue System Variables (Biến Hệ thống Đối thoại)
```
Key: DS_LUA_VAR_{VAR_NAME}
Type: Varies (string, int, float, bool)

Stored in ES3: Yes
Original source: Dialogue System (Lua Variables table)
Bridge: DialogueSystemLocalizationBridge.cs
```

| Key | Type | Description | Example |
|-----|------|-------------|---------|
| `DS_LUA_VAR_PLAYER_NAME` | `string` | Player character name | `"Alex"` |
| `DS_LUA_VAR_NPC_MOOD_{NPC_ID}` | `int` | NPC mood/attitude level | `0-100` |
| `DS_LUA_VAR_STORY_PROGRESS` | `int` | Main story progress flag | `1`, `2`, `3`... |

### Quest/Objectives (Nhiệm vụ/Mục tiêu)
```
Key: QUEST_{QUEST_ID}
Type: QuestData

QuestData = {
  questID: string,
  status: QuestState (Unassigned, Active, Success, Failure, Abandoned),
  progress: float (0-100),
  objectives: List<ObjectiveData>
}
```

| Key | Type | Description | Example |
|-----|------|-------------|---------|
| `QUEST_{QUEST_ID}` | `QuestData` | Data for specific quest | `QUEST_ESCAPE_ROOM_01` |
| `QUEST_{QUEST_ID}_STATUS` | `string` | Quest state | `"Active"`, `"Success"` |
| `QUEST_{QUEST_ID}_PROGRESS` | `float` | Completion percentage | `75.0` |
| `QUEST_{QUEST_ID}_OBJECTIVE_{OBJ_NUM}` | `bool` | Objective completion | `true`/`false` |

### Documents/Notes (Tài liệu/Ghi chú)
| Key | Type | Description |
|-----|------|-------------|
| `PLAYER_DOCUMENTS_UNLOCKED` | `List<string>` | List of document IDs player has read |
| `PLAYER_NOTES_{NOTE_ID}_READ` | `bool` | Whether note has been read |
| `PLAYER_MESSAGES_{MESSAGE_ID}` | `string` | Message/letter content seen by player |

---

## 4. World State Persistence (Trạng thái Thế giới)

### Picked-up Items (Vật phẩm đã nhặt)
| Key | Type | Description | Example |
|-----|------|-------------|---------|
| `WORLD_ITEM_PICKED_{ITEM_INSTANCE_ID}` | `bool` | Whether item was collected | `true` |
| `WORLD_ITEM_SPAWNED_{ITEM_INSTANCE_ID}` | `bool` | Whether item exists in world | `false` |

### Doors & Locks (Cửa & Khóa)
```
Key: WORLD_DOOR_{DOOR_ID}
Type: DoorState

DoorState = {
  isOpen: bool,
  isLocked: bool,
  keyUsed: string
}
```

| Key | Type | Description | Example |
|-----|------|-------------|---------|
| `WORLD_DOOR_{DOOR_ID}_OPEN` | `bool` | Door open/closed state | `true` |
| `WORLD_DOOR_{DOOR_ID}_LOCKED` | `bool` | Door locked/unlocked | `false` |
| `WORLD_DOOR_{DOOR_ID}_KEY_USED` | `string` | Key ID that opened it | `"key_master_01"` |

### Puzzles & Mechanisms (Câu đố & Cơ chế)
| Key | Type | Description | Example |
|-----|------|-------------|---------|
| `WORLD_PUZZLE_{PUZZLE_ID}_SOLVED` | `bool` | Puzzle completion status | `true` |
| `WORLD_PUZZLE_{PUZZLE_ID}_ANSWER` | `string` | Answer/solution entered | `"1234"` |
| `WORLD_PUZZLE_{PUZZLE_ID}_STATE` | `Dictionary<string, object>` | Custom puzzle state data | `{piece_0: placed, piece_1: wrong}` |
| `WORLD_SAFE_{SAFE_ID}_CODE_ENTERED` | `string` | Last code entered | `"1234"` |

### Dynamic Objects State (Trạng thái Vật thể Động)
| Key | Type | Description | Example |
|-----|------|-------------|---------|
| `WORLD_OBJECT_{OBJECT_ID}_POSITION` | `Vector3` | Object XYZ position | `(5.0, 2.0, -10.5)` |
| `WORLD_OBJECT_{OBJECT_ID}_ROTATION` | `Quaternion` | Object rotation | `(0, 0, 0, 1)` |
| `WORLD_OBJECT_{OBJECT_ID}_STATE` | `string` | Custom state (opened, moved, etc.) | `"drawer_pulled_out"` |
| `WORLD_DRAWER_{OBJECT_ID}_OPEN` | `bool` | Drawer open state | `true` |
| `WORLD_VALVE_{OBJECT_ID}_TURNED` | `bool` | Valve turned state | `true` |

---

## 5. Global Settings & Statistics (Cài đặt & Thống kê)

### Game Settings (Cài đặt Trò chơi)
| Key | Type | Description | Default |
|-----|------|-------------|---------|
| `SETTING_MASTER_VOLUME` | `float` | Master volume (0-1) | `0.8` |
| `SETTING_MUSIC_VOLUME` | `float` | Music volume (0-1) | `0.6` |
| `SETTING_SFX_VOLUME` | `float` | Sound effects volume (0-1) | `0.8` |
| `SETTING_VOICE_VOLUME` | `float` | Voice volume (0-1) | `1.0` |
| `SETTING_MOUSE_SENSITIVITY` | `float` | Mouse look sensitivity | `1.0` |
| `SETTING_VHS_EFFECT_INTENSITY` | `float` | VHS effect strength (0-1) | `0.5` |
| `SETTING_LANGUAGE` | `string` | Current language code | `"en"`, `"vi"` |
| `SETTING_BRIGHTNESS` | `float` | Screen brightness | `1.0` |
| `SETTING_DIFFICULTY` | `string` | Game difficulty | `"Normal"`, `"Hard"` |

### Game Statistics (Thống kê Trò chơi)
| Key | Type | Description | Purpose |
|-----|------|-------------|---------|
| `STAT_TOTAL_PLAYTIME` | `float` | Total playtime in seconds | Achievements |
| `STAT_DEATHS_COUNT` | `int` | Number of times player died | Achievements |
| `STAT_JUMPSCARE_COUNT` | `int` | Times player triggered jumpscare | Achievements |
| `STAT_QUESTS_COMPLETED` | `int` | Total quests finished | Achievements |
| `STAT_ITEMS_COLLECTED` | `int` | Total items picked up | Achievements |
| `STAT_PUZZLES_SOLVED` | `int` | Puzzles solved | Achievements |

---

## 6. Save Metadata (Siêu dữ liệu Lưu trữ)

### Save Slot Info (Thông tin Slot Save)
| Key | Type | Description | Example |
|-----|------|-------------|---------|
| `SAVE_SLOT_NAME` | `string` | User-defined or auto-generated name | `"Manual Save 1"` |
| `SAVE_TIMESTAMP` | `string` | Date and time of save | `"2026-04-19 14:30:00"` |
| `SAVE_PLAYTIME_DISPLAY` | `string` | Formatted playtime | `"02:15:30"` |
| `SAVE_LOCATION_NAME` | `string` | Name of the area/scene | `"Takayagi Village"` |
| `SAVE_THUMBNAIL_PATH` | `string` | Path to save slot screenshot | `"Thumbnails/save_0.png"` |

### Scene/Checkpoint Data (Dữ liệu Cảnh/Checkpoint)
| Key | Type | Description |
|-----|------|-------------|
| `CURRENT_SCENE_NAME` | `string` | Last scene player was in |
| `LAST_CHECKPOINT_TIME` | `long` | Timestamp of last save |
| `LAST_CHECKPOINT_DATA` | `string` | Custom checkpoint metadata |

---

## File Organization (Tổ chức Tệp)

### Save File Structure (Cấu trúc Tệp Save)
```
SaveFile_{N}.es3 (Binary encrypted file)
├── SAVE_* (Metadata for slot)
├── PLAYER_* (Player state data)
├── PLAYER_INVENTORY_* (Inventory data)
├── QUEST_* (Quest progression)
├── DS_LUA_VAR_* (Dialogue System variables)
├── WORLD_* (World state)
├── SETTING_* (User settings - Global)
└── STAT_* (Statistics)
```

### ES3 Settings (Cài đặt ES3)
```csharp
var settings = new ES3Settings();
settings.compressionType = ES3.CompressionType.Gzip;
settings.encryptionType = ES3.EncryptionType.AES;
settings.encryptionPassword = "your_game_password";
settings.path = "SaveFile_0.es3";  // Slot-based naming
```

---

## Usage Examples (Ví dụ Sử dụng)

### Saving Data with ES3 (Lưu dữ liệu)
```csharp
// Save player health
ES3.Save<float>("PLAYER_HEALTH", playerHealth);

// Save inventory
ES3.Save<List<InventoryItemData>>("PLAYER_INVENTORY_ITEMS", inventoryList);

// Save quest progress
ES3.Save<QuestData>("QUEST_ESCAPE_ROOM_01", questData);
```

### Loading Data with ES3 (Tải dữ liệu)
```csharp
// Load player health
float health = ES3.Load<float>("PLAYER_HEALTH", 100f);

// Load inventory
List<InventoryItemData> inventory = ES3.Load<List<InventoryItemData>>("PLAYER_INVENTORY_ITEMS");

// Load quest progress
QuestData quest = ES3.Load<QuestData>("QUEST_ESCAPE_ROOM_01");
```

---

## Bridge Integration (Tích hợp Cầu nối)

### Dialogue System Bridge
- **Key Pattern**: `DS_LUA_VAR_{VARIABLE_NAME}`
- **Handler**: `DialogueSystemLocalizationBridge.cs`
- **Function**: Intercepts DS variable saves and redirects to ES3

### UHFPS Integration (Frozen)
- Original UHFPS save system is disabled via feature flag.
- UHFPS native components (Inventory, Attributes) data are manually mapped to ES3 keys.
- **Note**: DO NOT use UHFPS `SaveGameManager` for new features.

---

## Migration Notes (Ghi chú Di trú)

- All third-party save systems (UHFPS, Dialogue System) are frozen.
- Single source of truth: **Easy Save 3**.
- Config files (audio, graphics settings) saved separately in `SETTING_*` keys or standard Config location.
- All data structured for easy Steam Cloud sync support.
