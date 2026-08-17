# Godot WebGL Security Lab

An educational security lab built with **Godot WebGL** and a locally hosted API server to demonstrate the security risks of trusting client-side game logic.

The project simulates a simple corporate quiz platform where users can log in, take quizzes, update profile data, view their scores, and check a leaderboard.

The main goal is to explore what happens when security-sensitive decisions are handled by an untrusted client.

## 🎯 Learning Objectives

This project focuses on understanding:

* Client-side trust and why it is dangerous
* Reverse engineering Godot WebGL applications
* Discovering hardcoded API endpoints
* Inspecting client-side game logic
* Manipulating client-controlled values
* API request analysis and replay
* Broken authorization / IDOR-style scenarios
* Server-side validation
* Secure client/server architecture
* Why WebGL game logic should not be considered trusted

## 🏗️ Architecture

```text
┌─────────────────────────────┐
│       Godot WebGL Client    │
│                             │
│  Login                      │
│  Dashboard                  │
│  Quiz                       │
│  Profile                    │
│  Leaderboard                │
└──────────────┬──────────────┘
               │
               │ HTTP / JSON
               ▼
┌─────────────────────────────┐
│       Local API Server      │
│                             │
│  POST /login                │
│  GET  /user                 │
│  POST /user/update          │
│  POST /quiz/submit          │
│  GET  /leaderboard          │
└──────────────┬──────────────┘
               │
               ▼
          Database
```

## 🎮 Application Flow

### 1. Login

The user authenticates through the API.

```text
Login
  ↓
API authentication
  ↓
Dashboard
```

### 2. Dashboard

The dashboard displays information retrieved from the server:

* Username
* Profile information
* Score
* Rank
* Quiz button
* Leaderboard button

### 3. Quiz

The player receives five randomly selected questions.

Answers are submitted to the API and the resulting score is displayed to the player.

### 4. Profile

The client can send user profile information to the API through a POST request.

### 5. Leaderboard

The client retrieves leaderboard information from the API and displays the results.

## 🔬 Security Lab

The initial version intentionally contains insecure design decisions.

Examples include:

### Client-controlled score

A vulnerable implementation may allow the client to calculate and submit the final score.

```text
Client
  ↓
Calculates score
  ↓
Sends score to API
  ↓
Server accepts score
```

An attacker who controls the client can potentially modify the value before it reaches the server.

### Client-controlled identity

The application may send a user ID as part of an API request.

If the server blindly trusts that value instead of deriving the identity from an authenticated session, this can lead to unauthorized access or modification of another user's data.

### Client-side game logic

Important decisions such as:

```text
Pass / Fail
Score calculation
Quiz validation
```

should not be trusted simply because they are implemented inside the game.

A WebGL application runs in an environment controlled by the user.

### Hardcoded API endpoint

The client contains an API endpoint used to communicate with the backend.

This provides an opportunity to demonstrate how an analyst can:

1. Obtain the WebGL build
2. Inspect its generated files
3. Search for API-related strings
4. Identify backend endpoints
5. Analyze the API independently of the game's UI

A public API endpoint itself is not necessarily a vulnerability. The security problem occurs when sensitive information or trust assumptions are placed in the client.

## 🔐 Intended Security Lesson

The central concept of this project is:

> **Never trust the client.**

Anything running inside the user's browser should be considered potentially compromised.

The client can be:

* Modified
* Debugged
* Reverse engineered
* Instrumented
* Replayed
* Automated
* Disconnected from its intended UI

Therefore, security-sensitive operations should be enforced by the server.

### Vulnerable architecture

```text
Client
 │
 ├── Calculates score
 ├── Determines pass/fail
 ├── Provides user ID
 └── Sends final result
          │
          ▼
       Server
          │
          └── Trusts client
```

### Preferred architecture

```text
Client
 │
 ├── Sends answers
 └── Sends authenticated request
          │
          ▼
       Server
          │
          ├── Identifies user
          ├── Validates request
          ├── Validates answers
          ├── Calculates score
          ├── Calculates rank
          └── Stores result
```

The client should receive the result rather than determine the authoritative result.

## 🧪 Planned Attack / Analysis Workflow

The lab is intended to be analyzed in stages:

```text
1. Run the application
        ↓
2. Use the normal UI
        ↓
3. Observe API communication
        ↓
4. Export / inspect the WebGL build
        ↓
5. Locate API configuration
        ↓
6. Inspect client-side logic
        ↓
7. Analyze API requests
        ↓
8. Identify trust boundaries
        ↓
9. Demonstrate the weaknesses
        ↓
10. Harden the backend
        ↓
11. Repeat the tests
```

The goal is not simply to modify a game.

The goal is to understand **why the modification was possible in the first place**.

## 🛠️ Technology

* Godot Engine
* Godot WebGL
* GDScript
* Python
* FastAPI
* SQLite
* HTTP / JSON APIs

## 📁 Project Structure

```text
godot-security-lab/
│
├── godot-client/
│   ├── scenes/
│   ├── scripts/
│   ├── assets/
│   └── project.godot
│
├── api-server/
│   ├── main.py
│   ├── database.py
│   ├── models.py
│   └── routes/
│
└── README.md
```

## ⚠️ Disclaimer

This project is intentionally designed as an educational security laboratory.

The vulnerabilities and insecure implementations are included for **authorized testing, reverse-engineering practice, and security education**.

Only test these techniques against applications and systems that you own or have explicit permission to assess.
