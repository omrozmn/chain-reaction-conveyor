# Chain-Reaction Conveyor Studio Template

## Master Template (Top 20 Target Architecture)

---

## 0) Amaç

Bu doküman:
- Tek oyun için değil
- Reusable hyper-casual puzzle framework için yazılmıştır
- En az 300 level üretilebilir altyapıyı hedefler
- Top 20 chart mantığına göre tasarlanır

Bu bir oyun dokümanı değil. Bu bir studio production blueprint'tir.

---

## 1) Ürün Vizyonu

### 1.1 Konumlandırma

Chain-Reaction Conveyor:
- Aşina akış (conveyor + queue + combo + booster)
- Farklı mekanik (yönlendirme + domino chain)
- Düşük öğrenme bariyeri
- Yüksek tatmin yoğunluğu

Ama asıl hedef: Aynı iskeletle farklı mekanikler çıkarabilmek.

---

## 2) Studio Template Felsefesi

Bu proje tek oyun değildir. Bu: **HyperCasual Puzzle Engine v1** dır.

### 2.1 Sabit Katman (Değişmeyecek)

- UI Flow
- Monetization pacing
- Continue sistemi
- Booster yerleşimi
- ComboBar mantığı
- Difficulty ramp eğrisi
- Analytics event seti
- Fail UX
- Win UX

### 2.2 Değişebilir Katman

- Mekanik modül
- Tema / skin
- Obje tipi
- Chain tetikleme kuralı
- Level içi hedef kombinasyonu

---

## 3) 300+ Level Üretim Hedefi

Bu sistem:
- 30 MVP
- 120 Soft Launch
- 300 Production
- 600+ long tail

üretebilecek parametre mimarisine sahip olmalıdır.

---

## 4) Difficulty Engineering (Template Seviyesi)

Bu bölüm zorunlu ve sabittir.

### 4.1 Win Rate Hedef Eğrisi

| Level Range | Target Win Rate |
|-------------|-----------------|
| 1–5         | 90%             |
| 6–10        | 80%             |
| 11–20       | 70%             |
| 21–40       | 65%             |
| Spike Levels | 45–55%         |

### 4.2 Roller-Coaster Pattern

Her 6–8 levelde:
- 1 spike
- 1 recovery
- 1 monetization anchor zorunlu

### 4.3 Monetization Anchor Levels

Sabit kural:
- L8 → İlk sürtünme
- L14 → İlk near-miss spike
- L21 → Booster dependency
- L30+ → Controlled frustration

Bu kalıp 40 level sonra tekrar eder.

---

## 5) Psychological Design Template

Top 20 oyunların ortak noktası:

### 5.1 Near-Miss Engine

- Son %20 hedefte spawn manipülasyonu
- Fail'e 1 hamle kala çözülebilir board
- Continue sonrası garanti çözüm penceresi

### 5.2 Dopamine Loop

Her 15–25 saniyede:
- Görsel patlama
- Combo animasyonu
- Yavaşlatılmış dramatik an
- Haptic feedback

---

## 6) Core Gameplay İlkeleri

1. 1 parmak kontrol
2. 3 saniyede anlaşılır
3. 30–90 saniye seviye süresi
4. Her level bir "mini problem"

---

## 7) Visual Template (Top 20 Standardı)

### 7.1 Renk Kuralları

- 1 Primary
- 1 Secondary
- 1 Accent
- 1 Background tone

Toplam 4 ana renk.

### 7.2 UI Kuralları

- Büyük dokunma alanı
- Minimal text
- Booster alt bölüm sabit
- Hedef üst bölüm sabit
- Combo ortada dramatik

### 7.3 Animasyon Ritim

- Spawn: 0.15–0.25 sn
- Chain resolve: 0.1 sn adım
- Win animasyonu: 1.2 sn max
- Fail dramatik slow: 0.5 sn

---

## 8) Core Framework Mimarisi

### 8.1 Katmanlar

**CORE (Sabit)**
- LevelManager
- DifficultyEngine
- MonetizationEngine
- ContinueSystem
- BoosterSystem
- AnalyticsLayer
- AdaptiveLayer
- NearMissController

**MECHANIC (Plugin)**
- Conveyor
- Board
- Chain rules
- Spawn rules

---

## 9) Adaptive Difficulty Template

Her level için:
- fail_count tracking
- 3 fail sonrası easing
- spawn weight adjust
- conveyor speed adjust
- bonus fill boost

Bu sistem UI'dan görünmez.

---

## 10) 300 Level İçerik Planı (Yüksek Seviye)

**Phase 1 — Foundation (1–40)**
- Mekanik öğrenme
- 2–3 varyasyon

**Phase 2 — Complexity (41–120)**
- Hybrid target
- Pocket pressure
- Nadir spawn

**Phase 3 — Depth (121–200)**
- Çift hedef
- Çapraz engel
- Booster dependency

**Phase 4 — Expert Casual (201–300)**
- Multi-layer hedef
- Sınırlı move
- Düşük spawn toleransı

---

## 11) KPI Odaklı Tasarım

Her level:
- target_win_rate
- target_fail_rate
- rewarded_offer_rate
- continue_accept_rate

parametrelerine sahip olmalı.

---

## 12) Monetization Template

### 12.1 Continue

Continue:
- 1 per level
- Rewarded zorunlu
- Continue sonrası "kazandırma bias"

### 12.2 Interstitial

- Win sonrası
- 2–3 level frekans

### 12.3 Booster Habit

Level 7–9 arası ilk booster baskısı.

---

## 13) Risk Yönetimi

**Risk:** Çok benzer hissetme

**Çözüm:**
- Tema farkı
- Etkileşim farkı
- Chain animasyonu özgün

**Risk:** 300 level üretim zor

**Çözüm:**
- Parametrik level builder
- Procedural template varyasyonu

---

## 14) Milestone (Studio Seviyesi)

- M0 → Core engine
- M1 → 30 level MVP
- M2 → 120 level soft launch
- M3 → 300 level production

---

## 15) Çıkış Kriteri (Studio)

Bu proje başarı sayılır:
- Aynı core ile yeni puzzle 2 haftada çıkabiliyorsa
- 300 level üretebiliyorsak
- Retention curve ölçülebiliyorsa
- Monetization anchor çalışıyorsa

---

## Önemli Not

Bu doküman artık tek oyun steering'i değil. Bu: **HyperCasual Puzzle Production System v1** dır.

Tamam. Bu artık MVP requirements değil. Bu Studio-Level, 300+ Level üretilebilir, Top 20 hedefli Requirements dokümanı olacak.

Aşağıdaki doküman:
- Reusable template mantığına göre
- Retention engineering dahil
- Monetization anchor dahil
- Adaptive difficulty dahil
- Near-miss dahil
- 300+ level ölçeklenebilir şekilde yazılmıştır.
