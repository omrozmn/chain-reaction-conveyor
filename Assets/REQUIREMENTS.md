# Chain-Reaction Conveyor Studio Template

## Requirements — 300+ Level Scalable Puzzle Framework

---

## 1) Amaç

Bu doküman:
- Chain-Reaction Conveyor oyununun
- Studio-level reusable puzzle framework'ünün
- 300+ level üretilebilir sistem gereksinimlerini tanımlar.

Bu sistem:
- Tek oyun değil
- Bir "Puzzle Engine" altyapısıdır.

---

## 2) Genel Ürün Gereksinimleri

### 2.1 Platform

- Unity 2D
- iOS + Android
- Portrait orientation

### 2.2 Oynanış Gereksinimleri

- Tek parmak kontrol (tap)
- 3 saniyede anlaşılabilir mekanik
- Ortalama level süresi: 30–90 sn
- Her 15–25 saniyede bir görsel tatmin (combo, chain, bonus)

---

## 3) Core Oyun Döngüsü Gereksinimleri

1. Objeler conveyor üzerinde akar.
2. Oyuncu tap ile objeyi route eder.
3. Obje board/gate üzerinde aktivasyon başlatır.
4. Zincir reaksiyon tetiklenir.
5. Hedef ilerlemesi hesaplanır.
6. Win veya fail kontrolü yapılır.
7. Continue/Restart/Next flow çalışır.

Bu döngü deterministik olmalıdır.

---

## 4) Conveyor Sistemi Gereksinimleri

### 4.1 Spawn Sistemi

Her level için:
- spawn_interval
- spawn_weights (type bazlı)
- max_spawn
- burst_pattern (opsiyonel)

tanımlanabilir olmalıdır.

Spawn sistemi:
- Seed bazlı deterministik çalışmalıdır.
- Spawn manipülasyonuna izin vermelidir (Near-Miss Engine için).

### 4.2 Kapasite ve Pocket

- conveyor_capacity tanımlı olmalı.
- pocket_count default 5.
- pocket_capacity parametreli olmalı.
- pocket_overflow fail condition desteklenmeli.

Pocket davranışı:
- Manuel re-enqueue desteklenmeli.
- UI feedback zorunlu.

---

## 5) Board Sistemi Gereksinimleri

### 5.1 Grid Yapısı

- width x height parametreli
- Hücre tipleri:
  - Empty
  - TargetSlot
  - Locked
  - Special
  - Obstacle

### 5.2 Hücre Özellikleri

**Locked hücre:**
- hp parametresi
- chain damage alabilir

**TargetSlot:**
- accept_types
- progress tracking

---

## 6) Zincir Reaksiyon Sistemi Gereksinimleri

### 6.1 Activation Kuralları

- 4-direction adjacency (genişletilebilir)
- min_cluster parametreli
- Deterministik cluster çözümleme

### 6.2 Resolve Kuralları

- Cluster hücreleri temizlenmeli
- Locked hücreler damage almalı
- Yeni oluşan cluster'lar otomatik kontrol edilmeli
- Combo sayacı artmalı

### 6.3 Determinism

- Aynı seed + aynı input → aynı sonuç
- Resolve sırası sabit olmalı

---

## 7) Combo & Bonus Gereksinimleri

### 7.1 ComboBar

- Dolum formülü parametrik
- Threshold parametreli
- Bonus duration parametreli

### 7.2 Bonus Mode

MVP için en az 1 bonus türü:
- Conveyor slow VEYA
- MIN_CLUSTER düşüşü VEYA
- Radius patlama

Bonus mode parametreli ve devre dışı bırakılabilir olmalıdır.

---

## 8) Booster Sistemi Gereksinimleri

Zorunlu boosterlar:
1. Swap
2. Bomb
3. Slow

Booster gereksinimleri:
- Level başında verilebilir
- Rewarded ile kazanılabilir
- Continue sonrası verilebilir
- Kullanım analytics'i tutulmalı

---

## 9) Level Sistemi Gereksinimleri

### 9.1 Level Definition

Her level:
- level_id
- seed
- board_size
- cell_layout
- target_defs
- spawner_defs
- conveyor_defs
- fail_rule
- difficulty_profile
- monetization_anchor_flag

içermelidir.

### 9.2 Hedef Türleri

MVP zorunlu:
- FillSlots
- ClearLocked
- DeliverToGate

Sistem hybrid hedefi desteklemelidir.

---

## 10) Difficulty Engineering Gereksinimleri

Bu bölüm zorunludur.

### 10.1 Win Rate Hedefi

Her level:
- target_win_rate
- target_fail_rate

parametrelerine sahip olmalıdır.

### 10.2 Roller-Coaster Pattern

Sistem şunu desteklemelidir:
- Spike level flag
- Recovery level flag
- Anchor level flag

### 10.3 Spike Level Özellikleri

Spike level:
- Spawn zorlaştırılmış
- Target daha karmaşık
- Near-miss manipülasyonu aktif

---

## 11) Near-Miss Engine Gereksinimleri

Sistem şunları desteklemelidir:
- Son %20 hedefte spawn weight adjust
- Fail'e 1 hamle kala çözülebilir board bırakma
- Continue sonrası 1–2 kolay spawn garantisi

NearMissEngine:
- UI'dan bağımsız
- Config bazlı

---

## 12) Adaptive Difficulty Gereksinimleri

Sistem:
- fail_count tracking
- fail_count >= 3 ise easing uygulama
- spawn weight +X%
- conveyor speed -Y%
- bonus fill +Z%

desteklemelidir.

Adaptive layer:
- Level override edilebilir
- Default global config kullanılabilir

---

## 13) Monetization Gereksinimleri

### 13.1 Continue

- Fail ekranında primary CTA
- 1 continue per level
- Continue sonrası kazandırma bias uygulanmalı

### 13.2 Rewarded

Context bazlı:
- Continue
- Extra booster
- Pre-level bonus

### 13.3 Interstitial

- Win sonrası
- Frequency cap parametreli

---

## 14) Analytics Gereksinimleri

Zorunlu eventler:
- level_start
- level_end
- fail_reason
- combo_max
- booster_used
- rewarded_shown
- rewarded_accepted
- rewarded_completed
- continue_used

Ek olarak:
- win_rate_per_level
- fail_cluster_distribution
- abandon_rate

---

## 15) Performans Gereksinimleri

- 60 FPS hedef
- Object pooling zorunlu
- Chain resolve GC spike yapmamalı
- Low-end Android test zorunlu

---

## 16) 300+ Level Ölçeklenebilirlik Gereksinimleri

Sistem:
- Level template varyasyonu desteklemeli
- Procedural parametre üretimine izin vermeli
- Batch level generation mümkün olmalı
- Level parametreleri export/import edilebilir olmalı

---

## 17) Visual & UX Template Gereksinimleri

### 17.1 Renk Sistemi

- 4 ana renk
- Parametrik tema sistemi

### 17.2 UI Layout

- Booster sabit alt bölüm
- Hedef sabit üst bölüm
- Combo merkezde dramatik

### 17.3 Animasyon Süreleri

- Spawn < 0.25 sn
- Chain resolve < 0.1 sn adım
- Fail slow-motion 0.5 sn max

---

## 18) Test Gereksinimleri

- Deterministic replay test
- Fail condition test
- Booster interaction test
- Near-miss activation test
- Adaptive easing test

---

## 19) MVP Done (Studio-Level)

- 30 playable level
- Difficulty curve uygulanmış
- NearMissEngine aktif
- AdaptiveLayer aktif
- Monetization anchor uygulanmış
- Analytics verileri geliyor
- Crash-free build
