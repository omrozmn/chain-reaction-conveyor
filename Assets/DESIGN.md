# Chain-Reaction Conveyor

## HyperCasual Puzzle Engine — Production Architecture v1

---

## 1) Mimari Genel Bakış

### 1.1 Katmanlı Mimari

```
┌──────────────────────────────┐
│        UI Layer              │
├──────────────────────────────┤
│      Game Flow Layer         │
├──────────────────────────────┤
│    Core Systems Layer        │
│   - LevelManager             │
│   - DifficultyEngine          │
│   - MonetizationEngine       │
│   - AdaptiveLayer            │
│   - NearMissEngine           │
│   - Analytics                │
├──────────────────────────────┤
│   Mechanic Layer (Plugin)    │
│   - Conveyor                 │
│   - Board                    │
│   - ChainResolver            │
│   - Spawner                  │
└──────────────────────────────┘
```

---

## 2) Runtime Flow

```
Boot → Load Core Config → Load LevelDef → Apply Difficulty Profile → 
Init Mechanic Layer → Gameplay Loop → Resolve Win/Fail → 
Monetization Layer → Save Progress
```

---

## 3) Core Systems Tasarımı

### 3.1 LevelManager

**Sorumluluk**
- Level yükleme
- Seed üretme
- DifficultyProfile bağlama
- Level state yönetimi

**Veri Akışı**
```
LevelDef → DifficultyEngine.Apply() → Mechanic.Init()
```

### 3.2 DifficultyEngine

**Amaç**
Her level için:
- target_win_rate
- spike_flag
- recovery_flag
- anchor_flag

parametrelerini yönetir.

**Çalışma**
```csharp
Level load edilirken:
if (level.isSpike) ApplySpikeModifiers()
elif (level.isRecovery) ApplyRecoveryModifiers()
```

### 3.3 AdaptiveLayer

**Amaç**
Fail sonrası easing uygular.

**State**
- fail_count
- last_fail_reason

**Kural**
```csharp
if (fail_count >= 3):
    spawn_weight[target] += 15%
    conveyor_speed *= 0.9
    bonus_fill_rate *= 1.1
```

**Reset**
- Level win olunca reset.

### 3.4 NearMissEngine

Bu sistem retention için kritiktir.

**Amaç**
- Son %20 hedefte manipülasyon
- Fail'e yakın dramatik durum üretme
- Continue sonrası kazandırma bias

#### 3.4.1 Activation Koşulu
```csharp
if (target_progress >= 80%): ActivateNearMiss()
```

#### 3.4.2 Spawn Manipülasyonu
```csharp
target_weight -= X%
non_target_weight += Y%
```

**Ama:**
- Oyuncuya fark ettirmemeli
- Determinism korunmalı

#### 3.4.3 Continue Bias
```csharp
Continue sonrası:
GuaranteeSpawn(target_type, next 2 spawns)
```

---

## 4) Mechanic Layer

### 4.1 Conveyor System

**Veri Yapısı**
```csharp
List<Item> conveyorQueue
List<Item> pocketSlots
```

**Update Loop**
```
Tick() → MoveItems() → CheckCapacity() → SpawnIfNeeded()
```

**Spawn**
Spawner seed bazlı çalışır:
```csharp
Random(seed)
```

### 4.2 Board System

**Veri Modeli**
```csharp
Cell[,] grid

Cell:
  - type
  - state
  - lock_hp
```

### 4.3 ChainResolver

**Süreç**
```csharp
onItemPlaced(cell):
    BFS cluster search
    if (cluster >= MIN_CLUSTER):
        resolve(cluster)
        chainCount++
        re-check neighbors
```

**Determinism**
- BFS queue sırası sabit
- Resolve order sabit

---

## 5) ComboBar System

```csharp
// Dolum
combo_points += cluster.size * chain_depth

// Tetik
if (combo_points >= threshold):
    ActivateBonus()
```

---

## 6) MonetizationEngine

**Sorumluluk**
- Continue
- Booster teklifleri
- Interstitial frekans kontrol

**Continue Flow**
```
Fail → ShowContinue() → If accepted: ApplyContinueBias() → ResumeLevel()
```

---

## 7) Analytics Architecture

**Event Queue**
```csharp
GameEventBus.Publish(event)
AnalyticsManager.Listen(event)
```

**Zorunlu Eventler:**
- level_start
- level_fail
- fail_reason
- rewarded_accepted
- combo_max
- continue_used

---

## 8) Level Data Pipeline (300+ Ölçek)

### 8.1 Parametrik Level Template

```csharp
LevelTemplate:
  - base_board
  - difficulty_profile
  - spawn_profile
  - target_profile
```

Batch üretim mümkün olmalı.

---

## 9) Difficulty Scaling Architecture

**Global Config**
```csharp
DifficultyConfig:
  - base_min_cluster
  - spike_multiplier
  - recovery_multiplier
  - anchor_bias
```

Level override desteklenmeli.

---

## 10) Performance Tasarımı

- ObjectPool zorunlu
- BFS reuse arrays
- No LINQ runtime
- FixedUpdate kullanılmaz (Update tick)

---

## 11) Edge Case Handling

- Simultaneous cluster resolve
- Bomb + chain overlap
- Continue sırasında resolve state
- Pocket enqueue race condition

---

## 12) Deterministic Replay System (Zorunlu)

**Amaç:**
- Aynı seed + input → aynı replay

**Kaydedilecek:**
- seed
- input_timestamps
- tap_targets

Debug için gereklidir.

---

## 13) Theme Abstraction Layer

Skin değiştirilebilir olmalı:

```csharp
ThemeConfig:
  - primary_color
  - accent_color
  - particle_style
  - sound_pack
```

Mechanic değişmeden tema değişebilir.

---

## 14) Studio-Level Production Capability

Bu mimari:
- Yeni puzzle 2 haftada çıkarabilmeli
- Sadece Mechanic Layer değiştirilmeli
- Core sabit kalmalı

---

## 15) Design Done Kriteri

- 30 level deterministik
- NearMissEngine çalışıyor
- AdaptiveLayer aktif
- Continue bias doğru
- 120 level üretilebilir
- 300 level parametre uyumlu
