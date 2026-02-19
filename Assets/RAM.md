# Chain-Reaction Conveyor Studio Template

## Responsibility Assignment Matrix (RAM)

---

## Proje Genel Bakış

**Proje Adı:** Chain-Reaction Conveyor - HyperCasual Puzzle Engine  
**Hedef:** 300+ level üretilebilir, reusable puzzle framework  
**Platform:** Unity 2D (iOS + Android)  
**Tip:** Studio-level production template

---

## Roller ve Sorumluluklar

| Rol | Sorumluluk | Öncelik |
|-----|------------|----------|
| **Game Designer** | Mechanic tasarımı, level template, difficulty curve | Critical |
| **Unity Developer** | Core engine, mechanic layer, gameplay systems | Critical |
| **Retention Designer** | NearMissEngine, AdaptiveLayer, monetization anchors | High |
| **Visual Artist** | Theme system, UI layout, particle effects | Medium |
| **Data Analyst** | KPI tracking, analytics implementation, A/B testing | Medium |
| **QA Engineer** | Deterministic replay testing, edge case testing | High |

---

## Doküman Sorumlulukları

| Doküman | Sorumlu | Reviewer |
|---------|---------|----------|
| STEERING.md | Product Lead | CEO / Studio Head |
| REQUIREMENTS.md | Game Designer + Unity Developer | Retention Designer |
| DESIGN.md | Unity Developer Lead | Tech Lead |
| TASKS.md | Project Manager | Team Lead |
| RETENTION_ENGINEERING.md | Retention Designer | Data Analyst |
| RAM.md | Project Manager | HR / Studio Lead |

---

## Karar Alma Yetkileri

| Karar Türü | Yetkili | Escalation |
|------------|---------|------------|
| Core mechanic değişikliği | Game Designer | Studio Head |
| Difficulty parametre değişikliği | Retention Designer | Data Analyst |
| UI/UX değişikliği | Visual Artist | Game Designer |
| Milestone değişikliği | Project Manager | Studio Head |
| Teknoloji seçimi | Unity Developer Lead | Tech Lead |

---

## Phase Bazlı Kaynak Tahsisi

| Phase | Süre | Developer | Designer | Artist | Tester |
|-------|------|-----------|----------|--------|--------|
| Phase 0 - Foundation | 1 hafta | 2 | 1 | 0 | 0 |
| Phase 1 - Core Engine | 3 hafta | 2 | 1 | 1 | 1 |
| Phase 2 - Retention | 2 hafta | 1 | 2 | 0 | 1 |
| Phase 3 - Gameplay | 3 hafta | 2 | 1 | 1 | 1 |
| Phase 4 - Monetization | 2 hafta | 1 | 1 | 1 | 1 |
| Phase 5 - Analytics | 1 hafta | 1 | 0 | 0 | 1 |
| Phase 6 - 300 Level | 3 hafta | 1 | 2 | 1 | 2 |
| Phase 7 - Visual | 2 hafta | 0 | 1 | 2 | 1 |
| Phase 8 - Soft Launch | 2 hafta | 1 | 1 | 1 | 2 |

**Toplam:** ~19 hafta

---

## Milestone Bağımlılıkları

```
M0 (Foundation)
    ↓
M1 (Core MVP) ← Phase 1-5 tamamlanmalı
    ↓
M2 (Soft Launch) ← Phase 6 tamamlanmalı
    ↓
M3 (Production) ← Phase 7-8 tamamlanmalı
```

---

## Critical Path

1. **LevelManager + DifficultyEngine** → NearMissEngine → AdaptiveLayer
2. **Conveyor + Board + ChainResolver** → ComboBar → BoosterSystem
3. **Determinism Layer** → 300 Level Pipeline → Validation

---

## Risk ve Önceliklendirme

| Risk | Etki | Öncelik |
|------|------|----------|
| Mechanic重复性 | Yüksek | 1 - Theme system öncelikli |
| 300 level üretim zorluğu | Orta | 2 - Parametric builder öncelikli |
| Retention curve tuning | Yüksek | 3 - Early playtesting |
| Performance (low-end) | Orta | 4 - Optimization sprint |

---

## İletişim Kanalları

| Kanal | Amaç | Frekans |
|-------|------|---------|
| Daily Standup | Progress sync | Her gün |
| Sprint Review | Phase tamamlama | Haftalık |
| Design Sync | Mechanic/Retention kararları | Haftalık |
| Retrospective | Öğrenilen dersler | 2 haftada |

---

## Done Kriterleri

| Milestone | Done Kriteri |
|----------|--------------|
| M0 | Unity proje çalışır, architecture skeleton hazır |
| M1 | 30 playable level, retention systems aktif |
| M2 | 120 level, soft launch ready |
| M3 | 300 level, full production pipeline |
