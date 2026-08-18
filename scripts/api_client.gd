extends Node

signal player_loaded(player)
signal player_load_failed(message)

const API_URL := "http://127.0.0.1:8000"

var _http_request: HTTPRequest


func _ready() -> void:
	_http_request = HTTPRequest.new()
	add_child(_http_request)
	_http_request.request_completed.connect(_on_request_completed)
	print("API Client initialized.")


func get_player(player_id: String) -> void:
	var url := "%s/api/player/%s" % [API_URL, player_id]
	var error := _http_request.request(url)

	if error != OK:
		push_error("GET request failed: %s" % error)
		player_load_failed.emit("Could not start API request.")


func update_score(player_id: String, score: int) -> void:
	var url := "%s/api/score" % API_URL
	var data := {
		"player_id": player_id,
		"score": score
	}
	var json := JSON.stringify(data)
	var headers := ["Content-Type: application/json"]

	var error := _http_request.request(
		url,
		headers,
		HTTPClient.METHOD_POST,
		json
	)

	if error != OK:
		push_error("Score request failed: %s" % error)


func update_user(player_id: String, player_name: String, department: String) -> void:
	var url := "%s/api/user" % API_URL
	var data := {
		"player_id": player_id,
		"name": player_name,
		"department": department
	}
	var json := JSON.stringify(data)
	var headers := ["Content-Type: application/json"]

	var error := _http_request.request(
		url,
		headers,
		HTTPClient.METHOD_POST,
		json
	)

	if error != OK:
		push_error("User request failed: %s" % error)


func _on_request_completed(
	result: int,
	response_code: int,
	headers: PackedStringArray,
	body: PackedByteArray
) -> void:
	print("HTTP Status: %s" % response_code)

	if result != HTTPRequest.RESULT_SUCCESS:
		player_load_failed.emit("API request failed. Check CORS and server status.")
		return

	if response_code < 200 or response_code >= 300:
		player_load_failed.emit("API returned HTTP %s." % response_code)
		return

	var response := body.get_string_from_utf8()
	print("Server Response:")
	print(response)

	var parsed = JSON.parse_string(response)
	if typeof(parsed) != TYPE_DICTIONARY:
		return

	if parsed.has("error"):
		player_load_failed.emit(str(parsed["error"]))
		return

	if parsed.has("username") and parsed.has("profile"):
		player_loaded.emit(parsed)
