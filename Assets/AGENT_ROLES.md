# Chain-Reaction Conveyor - Agent Rolleri ve Yetkinlikler

## Proje Özeti
- **Proje Tipi:** Unity 2D HyperCasual Puzzle Game
- **Hedef:** 300+ Level Üretilebilir Puzzle Engine
- **Platform:** iOS + Android
- **Teknoloji:** Unity C#, URP

---

## Agent Rol Matrisi

### 1. Game Designer (Mechanic Designer)

| Alan | Detay |
|------|-------|
| **Sorumluluk** | Mechanic tasarımı, level template, difficulty curve, combo system |
| **Öncelik** | 🔴 Critical |

**Görevler:**
- Conveyor, Board, ChainResolver mechanic tasarımı
- Level template varyasyonları oluşturma
- Difficulty curve parametreleri belirleme
- ComboBar ve bonus system tasarımı
- NearMissEngine parametrelerini tasarlama

**Gerekli Yetenekler:**
- HyperCasual oyun mechanic anlayışı
- Level design pattern'leri (spike, recovery, anchor)
- Win rate engineering
- Retention mechanics (near-miss, adaptive difficulty)

**Kullanılacak Tool'lar:**
- `exec` - Unity project structure oluşturma
- `read/write` - DESIGN.md, TASKS.md güncelleme
- `browser` - Rival games analizi

---

### 2. Unity Developer (Builder Agent)

| Alan | Detay |
|------|-------|
| **Sorumluluk** | Core engine, mechanic layer, gameplay systems kodlama |
| **Öncelik** | 🔴 Critical |

**Görevler:**
- Unity 2D URP proje setup
- Core Systems implementation (LevelManager, GameFlowController)
- Mechanic Layer implementation (Conveyor, Board, ChainResolver)
- Spawn system ve deterministic random wrapper
- Monetization layer (Continue, Booster)
- Analytics event implementation

**Gerekli Yetenekler:**
- Unity C# programlama
- Unity 2D/URP deneyimi
- Object pooling ve performance optimization
- Event-driven architecture
- ScriptableObject kullanımı

**Kullanılacak Tool'lar:**
- `exec` - Unity CLI, git, build commands
- `read/write/edit` - C# script dosyaları
- `github` - Repo yönetimi, PR workflow
- `test-patterns` - Unit test yazımı

**MCP/Tools Gerekli:**
- Unity Editor CLI (headless build)
- Git CLI
- File system access

---

### 3. Retention Designer

| Alan | Detay |
|------|-------|
| **Sorumluluk** | NearMissEngine, AdaptiveLayer, monetization anchors |
| **Öncelik** | 🟠 High |

**Görevler:**
- NearMissEngine implementasyonu (80% threshold, spawn manipulation)
- AdaptiveLayer logic (fail count → easing)
- Monetization anchor placement (L8, L14, L21, L30+)
- Win rate target curve uygulama
- Continue bias system

**Gerekli Yetenekler:**
- Retention mechanics anlayışı
- Psychology-based game design
- A/B testing temelleri
- KPI metrics (win rate, continue rate, etc.)

**Kullanılacak Tool'lar:**
- `read/write` - Config dosyaları, steering documents
- `exec` - Simulation/test runs

---

### 4. Visual Artist

| Alan | Detay |
|------|-------|
| **Sorumluluk** | Theme system, UI layout, particle effects, sprite management |
| **Öncelik** | 🟡 Medium |

**Görevler:**
- Theme abstraction layer oluşturma
- Color palette ve visual style belirleme
- UI layout (booster placement, target display, combo bar)
- Particle effect placeholder'ları
- Sprite atlas organization

**Gerekli Yetenekler:**
- Unity 2D visual systems
- UI/UX design principles
- Color theory
- Particle system basics

**Kullanılacak Tool'lar:**
- `read/write` - ThemeConfig, UI prefabs
- `exec` - Asset organization

---

### 5. Data Analyst

| Alan | Detay |
|------|-------|
| **Sorumluluk** | KPI tracking, analytics implementation, A/B testing |
| **Öncelik** | 🟡 Medium |

**Görevler:**
- Analytics event structure oluşturma
- Event tracking (level_start, level_end, fail_reason, etc.)
- KPI dashboard requirements belirleme
- A/B test framework tasarımı
- Win rate / fail rate tracking

**Gerekli Yetenekler:**
- Analytics implementation
- Data pipeline design
- KPI definition
- Unity analytics platforms

**Kullanılacak Tool'lar:**
- `read/write` - Analytics event scripts, config
- `exec` - Data validation scripts

---

### 6. QA Engineer

| Alan | Detay |
|------|-------|
| **Sorumluluk** | Deterministic replay testing, edge case testing, bug finding |
| **Öncelik** | 🟠 High |

**Görevler:**
- Deterministic replay test framework
- Edge case testing (bomb + chain, pocket overflow, etc.)
- Fail condition validation
- Near-miss activation testing
- Adaptive easing test
- Build stability testing

**Gerekli Yetenekler:**
- Game testing methodologies
- Edge case identification
- Reproducible test scenarios
- Unity test frameworks (NUnit, PlayMode)

**Kullanılacak Tool'lar:**
- `test-patterns` - Test file generation, Unity test setup
- `exec` - Build testing, Unity test runner
- `read` - Code review for test coverage

---

## OpenClaw Agent Mapping

| OpenClaw Agent | Rol | Mapping Nedeni |
|----------------|-----|----------------|
| **builder** | Unity Developer | Kod yazma, Unity CLI, file operations |
| **qa** | QA Engineer | Test yazımı, edge case finding |
| **researcher** | Visual Artist + Data Analyst | Araştırma, best practices, market analizi |
| **planner** | Game Designer + Retention Designer | Task planning, steering docs |

---

## Required Tools & Configuration

### Mevcut Tool'lar (Aktif)
```json
{
  "tools": {
    "exec": { "security": "full", "ask": "off" },
    "web.fetch": { "enabled": true },
    "agentToAgent": { "enabled": true },
    "elevated": { "enabled": true }
  }
}
```

### Gerekli Ek Tool'lar
1. **Unity CLI** - `exec` ile mevcut
2. **GitHub** - `github` skill mevcut
3. **Test Patterns** - `test-patterns` skill mevcut

### Eksik Olanlar (Opsiyonel)
- Unity-specific MCP -manuel exec ile çözülür
- Game analytics MCP -manuel implementation

---

## Başlangıç Task'ları

### Builder (Unity Developer)
1. Unity 2D URP proje oluşturma
2. Core folder structure kurma
3. LevelDef model yazma
4. Deterministic Random wrapper

### QA
1. Test folder structure
2. Deterministic replay test template
3. Edge case checklist oluşturma

### Researcher
1. Unity 2D best practices araştırma
2. Similar hyper-casual games analizi
3. Performance optimization araştırma

---

## Notlar
- Unity Editor bu makinede kurulu: `/Applications/Unity/Hub/Editor/6000.3.9f1/`
- Proje path: `~/Desktop/Games/`
- 6 rol + OpenClaw agent mapping tamamlandı
