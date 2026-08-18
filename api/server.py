from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from datetime import datetime

app = FastAPI(title="Godot Security Lab API")

app.add_middleware(
    CORSMiddleware,
    allow_origins=[
        "http://127.0.0.1:8001",
        "http://localhost:8001",
    ],
    allow_credentials=False,
    allow_methods=["*"],
    allow_headers=["*"],
)


# --------------------------------------------------
# Temporary in-memory database
# --------------------------------------------------

players = {
    "player001": {
        "username": "player001",
        "score": 0,
        "passed": False,
        "profile": {
            "name": "Test Player",
            "department": "Security Lab"
        }
    }
}


# --------------------------------------------------
# Request models
# --------------------------------------------------

class ScoreUpdate(BaseModel):
    player_id: str
    score: int


class UserUpdate(BaseModel):
    player_id: str
    name: str
    department: str


# --------------------------------------------------
# Health check
# --------------------------------------------------

@app.get("/")
def root():
    return {
        "status": "online",
        "service": "Godot Security Lab API"
    }


# --------------------------------------------------
# Get player data
# --------------------------------------------------

@app.get("/api/player/{player_id}")
def get_player(player_id: str):

    if player_id not in players:
        return {
            "error": "Player not found"
        }

    return players[player_id]


# --------------------------------------------------
# Update score
# --------------------------------------------------

@app.post("/api/score")
def update_score(data: ScoreUpdate):

    if data.player_id not in players:
        return {
            "error": "Player not found"
        }

    # INTENTIONALLY VULNERABLE
    #
    # The server trusts whatever score
    # the client sends.

    players[data.player_id]["score"] = data.score

    players[data.player_id]["passed"] = data.score >= 3

    return {
        "success": True,
        "player_id": data.player_id,
        "score": data.score,
        "passed": data.score >= 3,
        "updated_at": datetime.now().isoformat()
    }


# --------------------------------------------------
# Update user profile
# --------------------------------------------------

@app.post("/api/user")
def update_user(data: UserUpdate):

    if data.player_id not in players:
        return {
            "error": "Player not found"
        }

    # INTENTIONALLY VULNERABLE
    #
    # Server accepts arbitrary profile information
    # from the client.

    players[data.player_id]["profile"]["name"] = data.name
    players[data.player_id]["profile"]["department"] = data.department

    return {
        "success": True,
        "player_id": data.player_id,
        "profile": players[data.player_id]["profile"]
    }
