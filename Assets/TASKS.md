# Chain-Reaction Conveyor

## HyperCasual Puzzle Engine — Production Plan v1

---

## PHASE 0 — Studio Foundation

**Amaç:** Tek oyun değil, Engine kurmak.

### 0.1 Repository Setup

- [ ] Unity 2D URP proje oluştur
- [ ] iOS + Android build support
- [ ] Git repo + branch stratejisi
- [ ] Core / Mechanic klasör ayrımı
- [ ] GlobalConfig sistemi oluştur

### 0.2 Architecture Skeleton

- [ ] AppState enum
- [ ] GameFlowController
- [ ] ServiceLocator / DI container
- [ ] EventBus sistemi
- [ ] ConfigLoader

**Çıktı:** Engine boş ama çalışır durumda.

---

## PHASE 1 — Core Engine Development

### 1.1 Level System

- [ ] LevelDef model oluştur
- [ ] DifficultyProfile modeli
- [ ] LevelLoader (JSON + ScriptableObject)
- [ ] Seed sistemi
- [ ] Deterministic random wrapper

### 1.2 Mechanic Base Layer

- [ ] IMechanic interface
- [ ] ConveyorMechanic implementasyonu
- [ ] Board grid sistemi
- [ ] ChainResolver
- [ ] SpawnController

### 1.3 Determinism Layer

- [ ] Custom Random sınıfı
- [ ] Seed injection
- [ ] Replay recorder sistemi
- [ ] Replay runner test aracı

---

## PHASE 2 — Retention Systems

### 2.1 DifficultyEngine

- [ ] Win rate hedef parametreleri
- [ ] Spike flag sistemi
- [ ] Recovery flag sistemi
- [ ] Anchor level flag sistemi
- [ ] Difficulty modifier apply logic

### 2.2 NearMissEngine

- [ ] Target progress tracker
- [ ] 80% threshold check
- [ ] Spawn weight manipülasyonu
- [ ] Continue sonrası guarantee spawn
- [ ] Near-miss disable condition

**Test:** Simüle edilmiş near-miss senaryosu

### 2.3 AdaptiveLayer

- [ ] Fail counter
- [ ] Fail reason tracking
- [ ] Spawn easing logic
- [ ] Conveyor speed easing
- [ ] Bonus fill easing
- [ ] Reset logic

---

## PHASE 3 — Gameplay Systems

### 3.1 Conveyor

- [ ] Conveyor queue
- [ ] Pocket system
- [ ] Manual re-enqueue
- [ ] Capacity logic
- [ ] Overflow fail trigger

### 3.2 Board

- [ ] Cell model
- [ ] Locked HP logic
- [ ] Special cell placeholder
- [ ] TargetSlot logic

### 3.3 ChainResolver

- [ ] BFS cluster detection
- [ ] Resolve order stabilization
- [ ] Multi-cluster resolution
- [ ] Combo depth tracking
- [ ] Damage propagation

**Test:**
- Multi-chain scenario
- Bomb overlap scenario

### 3.4 ComboBar

- [ ] Combo point accumulation
- [ ] Threshold trigger
- [ ] Bonus activation
- [ ] [ ] Bonus de Bonus timer
-activation

---

## PHASE 4 — Monetization Layer

### 4.1 Continue System

- [ ] Continue UI
- [ ] Rewarded integration
- [ ] Continue bias injection
- [ ] Continue limit (1 per level)
- [ ] Resume gameplay state

### 4.2 Booster System

- [ ] BoosterManager
- [ ] Swap
- [ ] Bomb
- [ ] Slow
- [ ] Booster inventory tracking
- [ ] Booster analytics event

### 4.3 Ads Integration

- [ ] AdManager abstraction
- [ ] Rewarded
- [ ] Interstitial
- [ ] Frequency cap logic
- [ ] Remote config ready

---

## PHASE 5 — Analytics & Telemetry

### 5.1 Event Layer

- [ ] EventBus → Analytics bridge
- [ ] level_start
- [ ] level_fail
- [ ] level_win
- [ ] fail_reason
- [ ] continue_used
- [ ] rewarded_completed
- [ ] combo_max

### 5.2 KPI Tracking

- [ ] Win rate per level
- [ ] Fail heatmap (cell-based)
- [ ] Continue accept rate
- [ ] Booster usage frequency
- [ ] Level abandon rate

---

## PHASE 6 — 300 Level Production Pipeline

**Bu bölüm kritik.**

### 6.1 Level Template System

- [ ] LevelTemplate class
- [ ] Difficulty bucket system (Easy/Medium/Spike/Recovery)
- [ ] Parametric spawn profile
- [ ] Parametric board layout generator

### 6.2 Batch Level Generator

- [ ] Scriptable level batch builder
- [ ] 1–40 foundation pack üret
- [ ] 41–120 complexity pack üret
- [ ] 121–200 hybrid pack
- [ ] 201–300 advanced pack

### 6.3 Validation Tool

- [ ] Simulated auto-play test
- [ ] Win probability estimator
- [ ] Spawn sanity checker
- [ ] Pocket overflow frequency checker

---

## PHASE 7 — Visual & UX Polish

### 7.1 Theme System

- [ ] ThemeConfig
- [ ] Color injection system
- [ ] Particle profile
- [ ] Sound pack abstraction

### 7.2 UX Polish

- [ ] Fail slow-motion
- [ ] Combo shake
- [ ] Haptic integration
- [ ] Button feedback animation

---

## PHASE 8 — Soft Launch Preparation

### 8.1 Difficulty Tuning

- [ ] Level 1–30 manual test
- [ ] Win rate simulation
- [ ] Spike verify
- [ ] Near-miss verify

### 8.2 Store Assets

- [ ] Gameplay capture
- [ ] Hook video
- [ ] Store screenshots
- [ ] ASO keywords

---

## PHASE 9 — Automation (Studio Capability)

### 9.1 OpenClaw Automation

- [ ] Module-by-module code generation
- [ ] Test file generation
- [ ] CI pipeline
- [ ] Crash log parser

### 9.2 Future Game Reuse

- [ ] Mechanic plugin swap test
- [ ] New skin injection test
- [ ] Core unchanged validation

---

## Milestone Plan

| Milestone | Açıklama |
|-----------|----------|
| M0        | Core engine skeleton - çalışır ama boş |
| M1        | 30 level + retention layer aktif |
| M2        | 120 level + soft launch tuning |
| M3        | 300 level + full automation |

---

## Final Production Checklist

- [ ] Deterministic replay çalışıyor
- [ ] NearMissEngine aktif
- [ ] AdaptiveLayer aktif
- [ ] Continue bias test edildi
- [ ] 300 level üretildi
- [ ] KPI ölçümü aktif
- [ ] Low-end test yapıldı
