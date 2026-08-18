extends Control

const ApiClientScript := preload("res://scripts/api_client.gd")

const PAGE_BACKGROUND := Color("#101820")
const PANEL_BACKGROUND := Color("#f7f9fb")
const PANEL_BORDER := Color("#d8e1ea")
const PRIMARY := Color("#2364aa")
const PRIMARY_HOVER := Color("#1b4f86")
const SECONDARY := Color("#eef3f8")
const SECONDARY_HOVER := Color("#dde8f2")
const DANGER := Color("#b3261e")
const TEXT := Color.WHITE
const MUTED_TEXT := Color.WHITE
const SUCCESS := Color("#13795b")

var _api: Node
var _player_id := ""
var _profile_name := ""
var _score := 0
var _passed := false
var _current_question_index := 0

var _questions := [
	"Question 1: What color is the sky on a clear day?",
	"Question 2: How many days are in a week?",
	"Question 3: Which animal says meow?"
]

var _answers := [
	["Blue", "Green"],
	["7", "10"],
	["Cat", "Dog"]
]

var _correct_answer_indexes: Array[int] = [0, 0, 0]

var _screen: VBoxContainer
var _username_input: LineEdit
var _password_input: LineEdit
var _login_status_label: Label
var _question_label: Label
var _quiz_progress_label: Label
var _score_label: Label
var _status_label: Label
var _first_answer_button: Button
var _second_answer_button: Button
var _submit_button: Button
var _quiz_complete_dialog: AcceptDialog


func _ready() -> void:
	_api = ApiClientScript.new()
	add_child(_api)
	_api.player_loaded.connect(_on_player_loaded)
	_api.player_load_failed.connect(_on_player_load_failed)

	_screen = get_node("VBoxContainer")
	_configure_root()

	_quiz_complete_dialog = AcceptDialog.new()
	_quiz_complete_dialog.title = "Quiz Completed"
	_quiz_complete_dialog.dialog_text = "Nice work. Review your score, then submit it to the API."
	add_child(_quiz_complete_dialog)

	_show_login_screen()
	print("Godot Security Lab Started")


func _configure_root() -> void:
	set_anchors_preset(Control.PRESET_FULL_RECT)
	offset_left = 0
	offset_top = 0
	offset_right = 0
	offset_bottom = 0

	var background := ColorRect.new()
	background.color = PAGE_BACKGROUND
	background.set_anchors_preset(Control.PRESET_FULL_RECT)
	add_child(background)
	move_child(background, 0)

	_screen.set_anchors_preset(Control.PRESET_FULL_RECT)
	_screen.offset_left = 0
	_screen.offset_top = 0
	_screen.offset_right = 0
	_screen.offset_bottom = 0
	_screen.alignment = BoxContainer.ALIGNMENT_CENTER
	_screen.add_theme_constant_override("separation", 0)


func _show_login_screen() -> void:
	_clear_screen()

	var panel := _create_panel(480, false)
	panel.add_child(_create_title("Corpo Office Quiz"))
	panel.add_child(_create_body_text("sign in to view your dashboard, take the trivia quiz and submit your score"))
	panel.add_child(_create_spacer(8))

	_username_input = _create_input("Username")
	_username_input.text_submitted.connect(func(_text): _login())
	panel.add_child(_username_input)
	panel.add_child(_create_spacer(4))

	_password_input = _create_input("Password")
	_password_input.secret = true
	_password_input.text_submitted.connect(func(_text): _login())
	panel.add_child(_password_input)

	var login_button := _create_button("Login", true)
	login_button.pressed.connect(_login)
	panel.add_child(login_button)

	_login_status_label = _create_status_label("")
	panel.add_child(_login_status_label)

	_center_panel(panel)
	_username_input.grab_focus()


func _login() -> void:
	var username := _username_input.text.strip_edges()
	var password := _password_input.text.strip_edges()

	if username.is_empty() or password.is_empty():
		_login_status_label.text = "Enter both username and password."
		_login_status_label.add_theme_color_override("font_color", DANGER)
		return

	_player_id = username
	_login_status_label.text = "Checking profile..."
	_login_status_label.add_theme_color_override("font_color", MUTED_TEXT)
	_api.get_player(_player_id)


func _on_player_loaded(player: Dictionary) -> void:
	_player_id = str(player["username"])
	_score = int(player["score"])
	_passed = bool(player["passed"])

	var profile: Dictionary = player["profile"]
	_profile_name = str(profile["name"])

	_show_dashboard()


func _on_player_load_failed(message: String) -> void:
	if _login_status_label == null:
		return

	_login_status_label.text = message
	_login_status_label.add_theme_color_override("font_color", DANGER)


func _show_dashboard() -> void:
	_clear_screen()

	var page := _create_page_margin()
	var content := VBoxContainer.new()
	content.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	content.size_flags_vertical = Control.SIZE_EXPAND_FILL
	content.add_theme_constant_override("separation", 22)
	page.add_child(content)

	var top_bar := HBoxContainer.new()
	top_bar.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	top_bar.add_theme_constant_override("separation", 16)

	var identity := VBoxContainer.new()
	identity.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	identity.add_theme_constant_override("separation", 4)
	identity.add_child(_create_eyebrow("Dashboard"))
	identity.add_child(_create_title("Corpo Office Quiz"))
	top_bar.add_child(identity)

	var logout_button := _create_button("Logout", false, DANGER)
	logout_button.custom_minimum_size = Vector2(120, 44)
	logout_button.pressed.connect(_logout)
	top_bar.add_child(logout_button)

	content.add_child(top_bar)

	var cards := HBoxContainer.new()
	cards.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	cards.custom_minimum_size = Vector2(0, 112)
	cards.add_theme_constant_override("separation", 16)
	cards.add_child(_create_info_panel("Welcome", "%s\n(%s)" % [_profile_name, _player_id]))
	cards.add_child(_create_info_panel("Current Score", str(_score)))
	content.add_child(cards)

	var actions := _create_panel(0, false)
	actions.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	actions.add_child(_create_section_title("Available Actions"))

	var take_quiz_button := _create_button("Take the Quiz", true)
	take_quiz_button.pressed.connect(_show_quiz_screen)
	actions.add_child(take_quiz_button)

	var leaderboard_button := _create_button("Leaderboard", false)
	leaderboard_button.pressed.connect(_show_leaderboard)
	actions.add_child(leaderboard_button)

	content.add_child(actions)
	_screen.add_child(page)


func _show_leaderboard() -> void:
	_clear_screen()

	var panel := _create_panel(700, false)
	var top_bar := HBoxContainer.new()
	top_bar.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	top_bar.add_theme_constant_override("separation", 12)

	var heading := VBoxContainer.new()
	heading.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	heading.add_theme_constant_override("separation", 6)
	heading.add_child(_create_eyebrow("Leaderboard"))
	heading.add_child(_create_title("Top Quiz Scores"))
	top_bar.add_child(heading)

	var back_button := _create_button("Back", false)
	back_button.custom_minimum_size = Vector2(110, 44)
	back_button.pressed.connect(_show_dashboard)
	top_bar.add_child(back_button)
	panel.add_child(top_bar)

	var table := VBoxContainer.new()
	table.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	table.add_theme_constant_override("separation", 8)
	table.add_child(_create_leaderboard_header())

	var names := [
		"player010",
		"player009",
		"player008",
		"player007",
		"player006",
		"player005",
		"player004",
		"player003",
		"player002"
	]
	var scores := [93, 85, 77, 69, 61, 53, 45, 37, 29]
	var rank := 1
	var player_added := false

	for i in range(names.size()):
		if not player_added and _score > scores[i]:
			table.add_child(_create_leaderboard_row(rank, _player_id, _score, true))
			rank += 1
			player_added = true

		table.add_child(_create_leaderboard_row(rank, names[i], scores[i], false))
		rank += 1

	if not player_added:
		table.add_child(_create_leaderboard_row(rank, _player_id, _score, true))

	panel.add_child(table)

	var actions := HBoxContainer.new()
	actions.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	actions.add_theme_constant_override("separation", 12)

	var logout_button := _create_button("Logout", false, DANGER)
	logout_button.pressed.connect(_logout)
	actions.add_child(logout_button)

	panel.add_child(actions)
	_center_panel(panel)


func _show_quiz_screen() -> void:
	_clear_screen()

	_current_question_index = 0
	_score = 0
	_passed = false

	var panel := _create_panel(620, false)
	panel.add_child(_create_eyebrow("Trivia Challenge"))
	panel.add_child(_create_title("Answer 3 Questions"))

	_quiz_progress_label = _create_status_label("")
	panel.add_child(_quiz_progress_label)

	_question_label = _create_section_title("")
	panel.add_child(_question_label)

	_first_answer_button = _create_button("", true)
	_first_answer_button.pressed.connect(func(): _answer_question(0))
	panel.add_child(_first_answer_button)

	_second_answer_button = _create_button("", false)
	_second_answer_button.pressed.connect(func(): _answer_question(1))
	panel.add_child(_second_answer_button)

	var quiz_meta := HBoxContainer.new()
	quiz_meta.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	quiz_meta.add_theme_constant_override("separation", 12)

	_score_label = _create_status_label("")
	_score_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	quiz_meta.add_child(_score_label)

	_status_label = _create_status_label("")
	_status_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	quiz_meta.add_child(_status_label)
	panel.add_child(quiz_meta)

	_submit_button = _create_button("Submit Score", true, SUCCESS)
	_submit_button.pressed.connect(_submit_score)
	_submit_button.visible = false
	panel.add_child(_submit_button)

	_show_current_question()
	_update_score_ui()
	_update_status_ui()
	_center_panel(panel)


func _answer_question(selected_answer_index: int) -> void:
	if _passed:
		return

	var is_correct: bool = selected_answer_index == _correct_answer_indexes[_current_question_index]
	if is_correct:
		_score += 1

	print("Question %s answered. Correct: %s. Score: %s" % [
		_current_question_index + 1,
		is_correct,
		_score
	])

	_current_question_index += 1
	_update_score_ui()

	if _current_question_index >= _questions.size():
		_complete_quiz()
		return

	_show_current_question()


func _show_current_question() -> void:
	_quiz_progress_label.text = "Question %s of %s" % [
		_current_question_index + 1,
		_questions.size()
	]
	_question_label.text = _questions[_current_question_index]
	_first_answer_button.text = _answers[_current_question_index][0]
	_second_answer_button.text = _answers[_current_question_index][1]


func _update_score_ui() -> void:
	_score_label.text = "Score: %s" % _score


func _update_status_ui() -> void:
	if _passed:
		_status_label.text = "Status: Completed"
		_status_label.add_theme_color_override("font_color", SUCCESS)
	else:
		_status_label.text = "Status: In progress"
		_status_label.add_theme_color_override("font_color", MUTED_TEXT)


func _complete_quiz() -> void:
	_passed = true
	print("QUIZ COMPLETED")
	_update_status_ui()

	_quiz_progress_label.text = "All questions answered"
	_question_label.text = "Quiz completed"
	_first_answer_button.disabled = true
	_second_answer_button.disabled = true
	_submit_button.visible = true
	_quiz_complete_dialog.popup_centered()


func _submit_score() -> void:
	if not _passed:
		push_error("Quiz must be completed before submitting.")
		return

	print("Submitting score: %s, passed: %s" % [_score, _passed])
	_api.update_score(_player_id, _score)
	_show_dashboard()


func _logout() -> void:
	_player_id = ""
	_profile_name = ""
	_score = 0
	_passed = false
	_show_login_screen()


func _clear_screen() -> void:
	for child in _screen.get_children():
		_screen.remove_child(child)
		child.queue_free()


func _center_panel(panel: Control) -> void:
	var page := _create_page_margin()
	var center := CenterContainer.new()
	center.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	center.size_flags_vertical = Control.SIZE_EXPAND_FILL
	center.add_child(panel)
	page.add_child(center)
	_screen.add_child(page)


func _create_page_margin() -> MarginContainer:
	var page := MarginContainer.new()
	page.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	page.size_flags_vertical = Control.SIZE_EXPAND_FILL
	page.add_theme_constant_override("margin_left", 40)
	page.add_theme_constant_override("margin_top", 32)
	page.add_theme_constant_override("margin_right", 40)
	page.add_theme_constant_override("margin_bottom", 32)
	return page


func _create_panel(width: float, framed := true) -> VBoxContainer:
	var content := VBoxContainer.new()
	content.custom_minimum_size = Vector2(width, 0)
	content.size_flags_horizontal = Control.SIZE_SHRINK_CENTER if width > 0 else Control.SIZE_EXPAND_FILL
	content.add_theme_constant_override("separation", 14)

	if not framed:
		return content

	var panel := PanelContainer.new()
	panel.add_theme_stylebox_override("panel", _create_panel_style(PANEL_BACKGROUND, PANEL_BORDER))

	var margin := MarginContainer.new()
	margin.add_theme_constant_override("margin_left", 28)
	margin.add_theme_constant_override("margin_top", 28)
	margin.add_theme_constant_override("margin_right", 28)
	margin.add_theme_constant_override("margin_bottom", 28)
	margin.add_child(content)
	panel.add_child(margin)

	var wrapper := VBoxContainer.new()
	wrapper.custom_minimum_size = Vector2(width, 0)
	wrapper.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	wrapper.add_child(panel)
	return wrapper


func _create_info_panel(title: String, value: String) -> VBoxContainer:
	var panel := VBoxContainer.new()
	panel.custom_minimum_size = Vector2(0, 96)
	panel.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	panel.add_theme_constant_override("separation", 8)

	var title_label := _create_eyebrow(title)
	title_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	panel.add_child(title_label)

	var value_label := _create_section_title(value)
	value_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	panel.add_child(value_label)
	return panel


func _create_leaderboard_header() -> Control:
	return _create_leaderboard_visual_row(
		"Rank",
		"Player",
		"Score",
		Color("#243447"),
		Color.WHITE,
		15
	)


func _create_leaderboard_row(rank: int, player_name: String, score: int, is_current_player: bool) -> Control:
	var background := Color("#d9f2ea") if is_current_player else Color("#eef3f8")
	return _create_leaderboard_visual_row(
		"#%s" % rank,
		player_name,
		str(score),
		background,
		Color.BLACK,
		15
	)


func _create_leaderboard_visual_row(
	rank: String,
	player_name: String,
	score: String,
	background: Color,
	color: Color,
	font_size: int
) -> Control:
	var content := HBoxContainer.new()
	content.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	content.add_theme_constant_override("separation", 12)
	content.add_child(_create_leaderboard_cell(rank, 80, HORIZONTAL_ALIGNMENT_CENTER, color, font_size))
	content.add_child(_create_leaderboard_cell(player_name, 1, HORIZONTAL_ALIGNMENT_LEFT, color, font_size))
	content.add_child(_create_leaderboard_cell(score, 100, HORIZONTAL_ALIGNMENT_RIGHT, color, font_size))

	var panel := PanelContainer.new()
	panel.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	panel.add_theme_stylebox_override("panel", _create_panel_style(background, Color("#c9d4df"), 6))

	var margin := MarginContainer.new()
	margin.add_theme_constant_override("margin_left", 14)
	margin.add_theme_constant_override("margin_top", 8)
	margin.add_theme_constant_override("margin_right", 14)
	margin.add_theme_constant_override("margin_bottom", 8)
	margin.add_child(content)
	panel.add_child(margin)
	return panel


func _create_leaderboard_cell(
	text: String,
	width: float,
	alignment: HorizontalAlignment,
	color: Color,
	font_size: int
) -> Label:
	var label := Label.new()
	label.text = text
	label.horizontal_alignment = alignment
	label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART

	if width == 1:
		label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	else:
		label.custom_minimum_size = Vector2(width, 24)

	label.add_theme_font_size_override("font_size", font_size)
	label.add_theme_color_override("font_color", color)
	return label


func _create_eyebrow(text: String) -> Label:
	var label := _create_label(text, 14, MUTED_TEXT)
	label.add_theme_constant_override("line_spacing", 2)
	return label


func _create_title(text: String) -> Label:
	return _create_label(text, 28, TEXT)


func _create_section_title(text: String) -> Label:
	return _create_label(text, 20, TEXT)


func _create_body_text(text: String) -> Label:
	return _create_label(text, 16, MUTED_TEXT)


func _create_status_label(text: String) -> Label:
	return _create_label(text, 15, MUTED_TEXT)


func _create_label(text: String, font_size: int, color: Color) -> Label:
	var label := Label.new()
	label.text = text
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	label.add_theme_font_size_override("font_size", font_size)
	label.add_theme_color_override("font_color", color)
	return label


func _create_input(placeholder: String) -> LineEdit:
	var input := LineEdit.new()
	input.placeholder_text = placeholder
	input.custom_minimum_size = Vector2(0, 46)
	input.focus_entered.connect(func(): input.placeholder_text = "")
	input.focus_exited.connect(func():
		if input.text.is_empty():
			input.placeholder_text = placeholder
	)
	input.add_theme_font_size_override("font_size", 16)
	input.add_theme_color_override("font_color", Color.BLACK)
	input.add_theme_color_override("font_placeholder_color", Color("#5f6368"))
	input.add_theme_color_override("caret_color", Color.BLACK)
	input.add_theme_stylebox_override("normal", _create_input_style(Color.WHITE, PANEL_BORDER))
	input.add_theme_stylebox_override("focus", _create_input_style(Color.WHITE, PRIMARY))
	return input


func _create_button(text: String, primary: bool, override_color = null) -> Button:
	var base_color: Color = override_color if override_color != null else (PRIMARY if primary else SECONDARY)
	var hover_color: Color = override_color if override_color != null else (PRIMARY_HOVER if primary else SECONDARY_HOVER)
	var font_color := Color.WHITE if primary or override_color != null else Color.BLACK

	var button := Button.new()
	button.text = text
	button.custom_minimum_size = Vector2(0, 48)
	button.mouse_default_cursor_shape = Control.CURSOR_POINTING_HAND
	button.add_theme_font_size_override("font_size", 16)
	button.add_theme_color_override("font_color", font_color)
	button.add_theme_color_override("font_hover_color", font_color)
	button.add_theme_color_override("font_pressed_color", font_color)
	button.add_theme_color_override("font_focus_color", font_color)
	button.add_theme_stylebox_override("normal", _create_button_style(base_color))
	button.add_theme_stylebox_override("hover", _create_button_style(hover_color))
	button.add_theme_stylebox_override("pressed", _create_button_style(hover_color.darkened(0.1)))
	button.add_theme_stylebox_override("disabled", _create_button_style(Color("#d7dde3")))
	button.add_theme_color_override("font_disabled_color", Color("#7b8792"))
	return button


func _create_spacer(height: float) -> Control:
	var spacer := Control.new()
	spacer.custom_minimum_size = Vector2(0, height)
	return spacer


func _create_panel_style(background: Color, border: Color, radius := 8) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = background
	style.border_color = border
	style.corner_radius_top_left = radius
	style.corner_radius_top_right = radius
	style.corner_radius_bottom_right = radius
	style.corner_radius_bottom_left = radius
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	return style


func _create_input_style(background: Color, border: Color) -> StyleBoxFlat:
	var style := _create_panel_style(background, border, 6)
	style.content_margin_left = 14
	style.content_margin_right = 14
	style.content_margin_top = 10
	style.content_margin_bottom = 10
	return style


func _create_button_style(color: Color) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = color
	style.corner_radius_top_left = 6
	style.corner_radius_top_right = 6
	style.corner_radius_bottom_right = 6
	style.corner_radius_bottom_left = 6
	style.content_margin_left = 16
	style.content_margin_right = 16
	style.content_margin_top = 10
	style.content_margin_bottom = 10
	return style
