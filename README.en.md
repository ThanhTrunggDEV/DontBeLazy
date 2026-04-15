<div align="center">

# 🧱 Don't Be Lazy

**Force yourself to get things done by whitelisting only what you need to work.**

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)
[![Status](https://img.shields.io/badge/status-planning-blue)]()
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](./CONTRIBUTING.md)
[![Sponsor](https://img.shields.io/badge/Sponsor-PayPal-00457C?logo=paypal)](https://paypal.me/ntt68)

🌏 **[Tiếng Việt](./README.md)** | **English**

</div>

---

## 🎯 The Problem

You sit down to work, open your laptop, and two hours later you realize you've been scrolling through social media and watching YouTube without getting anything done.

**Don't Be Lazy** was built to solve exactly that.

Unlike typical website blockers that are trivially easy to disable, **Don't Be Lazy** operates on a **Whitelist-first** principle: everything is blocked by default, and you explicitly allow only what you need to work.

---

## ✨ Core Features

| Feature | Description |
|---|---|
| 📋 **Task Management** | Create a to-do list with Pomodoro-style time estimates |
| 🔒 **Whitelist per Task** | Each task has its own Whitelist Profile — "Code" task allows VS Code, "Study" task allows dictionary app |
| 🛡️ **Focus Mode** | Activates a system-level shield blocking all internet and apps outside the Whitelist |
| ⚔️ **Strict Mode** | Cannot Stop, cannot edit Whitelist, cannot kill app via Task Manager |
| 🧠 **Psychological Tricks** | Friction, Guilt-tripping, Loss Aversion, Implementation Intention |
| 💬 **Motivation Quotes** | Auto-displays inspiring quotes at 4 strategic moments during your session |
| 📊 **Analytics** | Daily Streak, focus time charts, and "blocked attempt" counters |

---

## 🧠 Behavioral Psychology by Design

> *Don't Be Lazy doesn't just block websites — it trains your brain to stay committed.*

- **Implementation Intention:** Before starting, you must manually type your goal commitment (e.g. *"I will finish this feature in 25 minutes"*). Writing it down increases follow-through by ~40%.
- **Friction:** Want to quit early? You have to manually type: *"I am lazy and I accept giving up"* before the Stop button works.
- **Loss Aversion:** Your Streak counter is always visible. Quitting = Streak resets to 0. The brain hates loss more than it loves gain.
- **Guilt-tripping:** A red warning screen guilt-trips you with *"You only have 10 minutes left — do you really want to throw away all that effort?"* before allowing exit.

---

## 🖥️ Platform

- Windows (primary target)
- macOS (planned)

---

## 🏗️ Architecture Philosophy

- **Single-user, Offline-first:** No accounts, no cloud sync. Your data stays on your machine.
- **Privacy by design:** Zero network connections made by the app itself. All task data, whitelist configs, and session history stored in a local SQLite/JSON database.
- **System-level blocking:** Modifies the OS `hosts` file and/or uses a local proxy to block sites across all browsers (requires Admin/Root privilege).
- **Tamper-proof:** In Strict Mode, the app resists being killed via Task Manager and prevents uninstallation mid-session.

---

## 📊 How It Works

```
[User creates Task] → [Assigns Whitelist Profile] → [Types commitment]
         ↓
[Start Focus Mode]
         ↓
[System blocks all apps/websites not in the Profile]
         ↓
[Psychological tricks fire at strategic moments]
         ↓
[Session ends] → [Streak +1, Analytics updated]
```

---

## 🗂️ Documentation

- [📄 BA Document](./docs/ba_document.md) — Business analysis, features, system requirements (Vietnamese)
- [📋 Use Cases](./docs/use_cases.md) — Detailed use case flows UC01–UC06 (Vietnamese)
- [📝 Changelog](./CHANGELOG.md) — Release history
- [🤝 Contributing](./CONTRIBUTING.md) — How to contribute
- [🔒 Security](./SECURITY.md) — Security policy and vulnerability reporting

---

## 🚀 Project Status

Currently in **Planning & Design** phase. All BA documentation and Use Cases are finalized. Development begins next.

**Roadmap:**
- [x] Business Analysis document
- [x] Use Case documentation (UC01–UC06)
- [ ] Tech stack decision (leaning towards Tauri + Rust)
- [ ] UI/UX wireframes
- [ ] Core engine: system-level blocker
- [ ] MVP release

---

## 🤝 Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](./CONTRIBUTING.md) before submitting a Pull Request.

---

## 💖 Support the Project

If you find this project useful and want to support its development, feel free to buy me a coffee!

[![Sponsor via PayPal](https://img.shields.io/badge/Donate-PayPal-00457C?style=for-the-badge&logo=paypal)](https://paypal.me/ntt68)

---

## 📜 License

Distributed under the [MIT License](./LICENSE). © 2026 ThanhTrunggDEV
