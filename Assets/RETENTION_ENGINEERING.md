# Chain-Reaction Conveyor

## HyperCasual Puzzle Engine — Retention Architecture v1

---

## 1) Amaç

Bu doküman:
- Oyuncunun D1, D3, D7 retention'ını artırmak
- Level progression'ı psikolojik olarak optimize etmek
- Monetization ile zorluk eğrisini senkronize etmek
- Near-miss ve adaptive difficulty sistemlerini tanımlamak için yazılmıştır.

---

## 2) Retention Felsefesi

Retention şu üç şeye bağlıdır:
1. Erken başarı hissi
2. Kontrollü sürtünme
3. Az kalmış hissi

Bu üçü matematiksel olarak tasarlanmalıdır.

---

## 3) Onboarding Retention (Level 1–5)

**Hedef**
- Oyuncu 30 saniyede başarı yaşamalı.
- %90+ win rate.

**Kurallar**
- 2 renk
- Düşük spawn karmaşıklığı
- Pocket overflow yok
- Spike yok
- Bonus hızlı dolar

---

## 4) Friction Introduction (Level 6–10)

**Hedef**
- İlk mikro zorluk
- Continue gösterimi

**Kurallar**
- 3 renk
- İlk kilitli hücre
- Pocket overflow aktif
- Win rate ~80%

**Level 8 → Soft spike**

---

## 5) Roller-Coaster Difficulty Model

Her 6–8 levelde bir pattern uygulanır:

```
Easy → Medium → Medium → Spike → Recovery → Medium
```

**Recovery level:**
- Spawn kolay
- Bonus daha hızlı dolar
- Oyuncu "rahatladım" hissi yaşar

---

## 6) Spike Engineering

**Spike level özellikleri:**
- Target karmaşık
- Spawn dengesiz
- Nadir type az
- Pocket baskısı

**Ama:** Win rate 45–55% aralığında kalmalı.

---

## 7) Near-Miss Engineering

Bu sistem retention'ın kalbidir.

### 7.1 Activation Koşulu
```csharp
if (target_progress >= 80%): NearMissMode = true
```

### 7.2 Spawn Manipülasyonu

NearMissMode aktifken:
- Target type spawn weight düşürülür
- Diğer type artırılır
- Ama çözümsüzlük oluşmaz

### 7.3 Psychological Fail

Fail durumu:
- Son 1–2 hedef kalmış
- Pocket dolmak üzere
- Çözüm görünüyor ama gelmiyor

### 7.4 Continue Bias

Continue sonrası:
```csharp
GuaranteeSpawn(target_type, next 2 spawns)
IncreaseBonusFill(temporary)
```

**Ama UI bunu göstermez.**

---

## 8) Adaptive Difficulty Layer

**Amaç:**
- Casual oyuncu drop olmasın
- Hard oyuncu sıkılmasın

### 8.1 Fail Tracking

Her level için:
```csharp
fail_count
```

### 8.2 Easing Koşulu
```csharp
if (fail_count >= 3):
    spawn_weight[target] += 15%
    conveyor_speed *= 0.9
    bonus_fill_rate += 10%
```

### 8.3 Reset

```csharp
Level win olduğunda:
fail_count = 0
adaptive_modifiers reset
```

---

## 9) Monetization Anchors

Retention ve monetization senkronize olmalı.

### Anchor Planı

| Level | Event |
|-------|-------|
| 8     | İlk sürtünme |
| 14    | İlk hard spike |
| 21    | Booster dependency |
| 28    | Controlled frustration |

Bu pattern her 40 levelde tekrar eder.

---

## 10) Continue Acceptance Optimization

Continue teklif:
- Fail sonrası 0.5 sn dramatik slow
- Board freeze
- Hedef gösterimi
- "Just 1 more?" hissi

---

## 11) Bonus Timing Engineering

Bonus çok erken verilirse retention düşer.
Çok geç verilirse frustration artar.

**Hedef:**
- ortalamA levelde 1 bonus
- Spike levelde 0–1

---

## 12) Content Depth Strategy (300 Level)

| Level Aralığı | Odak |
|--------------|------|
| 1–40        | Öğrenme + varyasyon |
| 41–120      | Hybrid hedef + pocket baskısı |
| 121–200     | Multi-target + nadir spawn |
| 201–300     | Kombine spike + düşük tolerans |

Her 40 levelde pattern reset edilir.

---

## 13) KPI Hedefleri

### Level KPI
- Win rate target
- Fail cluster
- Continue acceptance %
- Booster usage %

### Global KPI
- D1 retention
- ortalamA seans süresi
- ortalamA level sayısı
- Rewarded opt-in %

---

## 14) Retention Risk Kontrolü

| Risk | Çözüm |
|------|-------|
| Çok zor → Drop | Adaptive layer |
| Çok kolay → Sıkılma | Spike planı |
| Monetization agresif | Anchor planı + recovery level |

---

## 15) Retention Done Kriteri

- Spike seviyeler planlı
- Recovery seviyeler planlı
- NearMissEngine aktif
- AdaptiveLayer aktif
- Continue bias uygulanıyor
- Win rate ölçümü aktif
