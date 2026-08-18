using Godot;

public partial class Main : Control
{
	private readonly Color _pageBackground = Color.FromHtml("#101820");
	private readonly Color _panelBackground = Color.FromHtml("#f7f9fb");
	private readonly Color _panelBorder = Color.FromHtml("#d8e1ea");
	private readonly Color _primary = Color.FromHtml("#2364aa");
	private readonly Color _primaryHover = Color.FromHtml("#1b4f86");
	private readonly Color _secondary = Color.FromHtml("#eef3f8");
	private readonly Color _secondaryHover = Color.FromHtml("#dde8f2");
	private readonly Color _danger = Color.FromHtml("#b3261e");
	private readonly Color _text = Colors.White;
	private readonly Color _mutedText = Colors.White;
	private readonly Color _success = Color.FromHtml("#13795b");

	private ApiClient _api;

	private string _playerId = "";
	private string _profileName = "";
	private int _score = 0;
	private bool _passed = false;
	private int _currentQuestionIndex = 0;

	private readonly string[] _questions =
	{
		"Question 1: What color is the sky on a clear day?",
		"Question 2: How many days are in a week?",
		"Question 3: Which animal says meow?"
	};

	private readonly string[,] _answers =
	{
		{ "Blue", "Green" },
		{ "7", "10" },
		{ "Cat", "Dog" }
	};

	private readonly int[] _correctAnswerIndexes =
	{
		0,
		0,
		0
	};

	private VBoxContainer _screen;
	private LineEdit _usernameInput;
	private LineEdit _passwordInput;
	private Label _loginStatusLabel;
	private Label _questionLabel;
	private Label _quizProgressLabel;
	private Label _scoreLabel;
	private Label _statusLabel;
	private Button _firstAnswerButton;
	private Button _secondAnswerButton;
	private Button _submitButton;
	private AcceptDialog _quizCompleteDialog;

	public override void _Ready()
	{
		_api = new ApiClient();
		AddChild(_api);

		_api.PlayerLoaded += OnPlayerLoaded;
		_api.PlayerLoadFailed += OnPlayerLoadFailed;

		_screen = GetNode<VBoxContainer>(
			"VBoxContainer"
		);

		ConfigureRoot();

		_quizCompleteDialog = new AcceptDialog
		{
			Title = "Quiz Completed",
			DialogText = "Nice work. Review your score, then submit it to the API."
		};
		AddChild(_quizCompleteDialog);

		ShowLoginScreen();

		GD.Print("Godot Security Lab Started");
	}

	private void ConfigureRoot()
	{
		SetAnchorsPreset(LayoutPreset.FullRect);
		OffsetLeft = 0;
		OffsetTop = 0;
		OffsetRight = 0;
		OffsetBottom = 0;

		ColorRect background = new ColorRect
		{
			Color = _pageBackground
		};
		background.SetAnchorsPreset(LayoutPreset.FullRect);
		AddChild(background);
		MoveChild(background, 0);

		_screen.SetAnchorsPreset(LayoutPreset.FullRect);
		_screen.OffsetLeft = 0;
		_screen.OffsetTop = 0;
		_screen.OffsetRight = 0;
		_screen.OffsetBottom = 0;
		_screen.Alignment = BoxContainer.AlignmentMode.Center;
		_screen.AddThemeConstantOverride("separation", 0);
	}

	private void ShowLoginScreen()
	{
		ClearScreen();

		VBoxContainer panel = CreatePanel(480, false);
		panel.AddChild(CreateTitle("Corpo Office Quiz"));
		panel.AddChild(CreateBodyText("sign in to view your dashboard, take the trivia quiz and submit your score"));
		panel.AddChild(CreateSpacer(8));

		_usernameInput = CreateInput("Username");
		_usernameInput.TextSubmitted += _ => Login();
		panel.AddChild(_usernameInput);
		panel.AddChild(CreateSpacer(4));

		_passwordInput = CreateInput("Password");
		_passwordInput.Secret = true;
		_passwordInput.TextSubmitted += _ => Login();
		panel.AddChild(_passwordInput);

		Button loginButton = CreateButton("Login", true);
		loginButton.Pressed += Login;
		panel.AddChild(loginButton);

		_loginStatusLabel = CreateStatusLabel("");
		panel.AddChild(_loginStatusLabel);

		CenterPanel(panel);
		_usernameInput.GrabFocus();
	}

	private void Login()
	{
		string username = _usernameInput.Text.Trim();
		string password = _passwordInput.Text.Trim();

		if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
		{
			_loginStatusLabel.Text = "Enter both username and password.";
			_loginStatusLabel.AddThemeColorOverride("font_color", _danger);
			return;
		}

		_playerId = username;
		_loginStatusLabel.Text = "Checking profile...";
		_loginStatusLabel.AddThemeColorOverride("font_color", _mutedText);

		_api.GetPlayer(_playerId);
	}

	private void OnPlayerLoaded(Godot.Collections.Dictionary player)
	{
		_playerId = player["username"].AsString();
		_score = player["score"].AsInt32();
		_passed = player["passed"].AsBool();

		var profile = player["profile"].AsGodotDictionary();
		_profileName = profile["name"].AsString();

		ShowDashboard();
	}

	private void OnPlayerLoadFailed(string message)
	{
		if (_loginStatusLabel == null)
		{
			return;
		}

		_loginStatusLabel.Text = message;
		_loginStatusLabel.AddThemeColorOverride("font_color", _danger);
	}

	private void ShowDashboard()
	{
		ClearScreen();

		MarginContainer page = CreatePageMargin();
		VBoxContainer content = new VBoxContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill
		};
		content.AddThemeConstantOverride("separation", 22);
		page.AddChild(content);

		HBoxContainer topBar = new HBoxContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		topBar.AddThemeConstantOverride("separation", 16);

		VBoxContainer identity = new VBoxContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		identity.AddThemeConstantOverride("separation", 4);
		identity.AddChild(CreateEyebrow("Dashboard"));
		identity.AddChild(CreateTitle("Corpo Office Quiz"));
		topBar.AddChild(identity);

		Button logoutButton = CreateButton("Logout", false, _danger);
		logoutButton.CustomMinimumSize = new Vector2(120, 44);
		logoutButton.Pressed += Logout;
		topBar.AddChild(logoutButton);

		content.AddChild(topBar);

		HBoxContainer cards = new HBoxContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			CustomMinimumSize = new Vector2(0, 112)
		};
		cards.AddThemeConstantOverride("separation", 16);
		cards.AddChild(CreateInfoPanel("Welcome", $"{_profileName}\n({_playerId})"));
		cards.AddChild(CreateInfoPanel("Current Score", _score.ToString()));
		content.AddChild(cards);

		VBoxContainer actions = CreatePanel(0, false);
		actions.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		actions.AddChild(CreateSectionTitle("Available Actions"));

		Button takeQuizButton = CreateButton("Take the Quiz", true);
		takeQuizButton.Pressed += ShowQuizScreen;
		actions.AddChild(takeQuizButton);

		Button leaderboardButton = CreateButton("Leaderboard", false);
		leaderboardButton.Pressed += ShowLeaderboard;
		actions.AddChild(leaderboardButton);

		content.AddChild(actions);

		_screen.AddChild(page);
	}

	private void ShowLeaderboard()
	{
		ClearScreen();

		VBoxContainer panel = CreatePanel(700, false);
		HBoxContainer topBar = new HBoxContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		topBar.AddThemeConstantOverride("separation", 12);

		VBoxContainer heading = new VBoxContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		heading.AddThemeConstantOverride("separation", 6);
		heading.AddChild(CreateEyebrow("Leaderboard"));
		heading.AddChild(CreateTitle("Top Quiz Scores"));
		topBar.AddChild(heading);

		Button backButton = CreateButton("Back", false);
		backButton.CustomMinimumSize = new Vector2(110, 44);
		backButton.Pressed += ShowDashboard;
		topBar.AddChild(backButton);

		panel.AddChild(topBar);

		VBoxContainer table = new VBoxContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		table.AddThemeConstantOverride("separation", 8);
		table.AddChild(CreateLeaderboardHeader());

		string[] names =
		{
			"player010",
			"player009",
			"player008",
			"player007",
			"player006",
			"player005",
			"player004",
			"player003",
			"player002"
		};

		int[] scores =
		{
			93,
			85,
			77,
			69,
			61,
			53,
			45,
			37,
			29
		};

		int rank = 1;
		bool playerAdded = false;

		for (int i = 0; i < names.Length; i++)
		{
			if (!playerAdded && _score > scores[i])
			{
				table.AddChild(CreateLeaderboardRow(
					rank,
					_playerId,
					_score,
					true
				));
				rank++;
				playerAdded = true;
			}

			table.AddChild(CreateLeaderboardRow(
				rank,
				names[i],
				scores[i],
				false
			));
			rank++;
		}

		if (!playerAdded)
		{
			table.AddChild(CreateLeaderboardRow(
				rank,
				_playerId,
				_score,
				true
			));
		}

		panel.AddChild(table);

		HBoxContainer actions = new HBoxContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		actions.AddThemeConstantOverride("separation", 12);

		Button logoutButton = CreateButton("Logout", false, _danger);
		logoutButton.Pressed += Logout;
		actions.AddChild(logoutButton);

		panel.AddChild(actions);
		CenterPanel(panel);
	}

	private void ShowQuizScreen()
	{
		ClearScreen();

		_currentQuestionIndex = 0;
		_score = 0;
		_passed = false;

		VBoxContainer panel = CreatePanel(620, false);
		panel.AddChild(CreateEyebrow("Trivia Challenge"));
		panel.AddChild(CreateTitle("Answer 3 Questions"));

		_quizProgressLabel = CreateStatusLabel("");
		panel.AddChild(_quizProgressLabel);

		_questionLabel = CreateSectionTitle("");
		panel.AddChild(_questionLabel);

		_firstAnswerButton = CreateButton("", true);
		_firstAnswerButton.Pressed += () => AnswerQuestion(0);
		panel.AddChild(_firstAnswerButton);

		_secondAnswerButton = CreateButton("", false);
		_secondAnswerButton.Pressed += () => AnswerQuestion(1);
		panel.AddChild(_secondAnswerButton);

		HBoxContainer quizMeta = new HBoxContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		quizMeta.AddThemeConstantOverride("separation", 12);

		_scoreLabel = CreateStatusLabel("");
		_scoreLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		quizMeta.AddChild(_scoreLabel);

		_statusLabel = CreateStatusLabel("");
		_statusLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		quizMeta.AddChild(_statusLabel);
		panel.AddChild(quizMeta);

		_submitButton = CreateButton("Submit Score", true, _success);
		_submitButton.Pressed += SubmitScore;
		_submitButton.Visible = false;
		panel.AddChild(_submitButton);

		ShowCurrentQuestion();
		UpdateScoreUI();
		UpdateStatusUI();

		CenterPanel(panel);
	}

	private void AnswerQuestion(int selectedAnswerIndex)
	{
		if (_passed)
		{
			return;
		}

		bool isCorrect = selectedAnswerIndex == _correctAnswerIndexes[
			_currentQuestionIndex
		];

		if (isCorrect)
		{
			_score++;
		}

		GD.Print(
			$"Question {_currentQuestionIndex + 1} answered. Correct: {isCorrect}. Score: {_score}"
		);

		_currentQuestionIndex++;

		UpdateScoreUI();

		if (_currentQuestionIndex >= _questions.Length)
		{
			CompleteQuiz();
			return;
		}

		ShowCurrentQuestion();
	}

	private void ShowCurrentQuestion()
	{
		_quizProgressLabel.Text = $"Question {_currentQuestionIndex + 1} of {_questions.Length}";

		_questionLabel.Text = _questions[
			_currentQuestionIndex
		];

		_firstAnswerButton.Text = _answers[
			_currentQuestionIndex,
			0
		];

		_secondAnswerButton.Text = _answers[
			_currentQuestionIndex,
			1
		];
	}

	private void UpdateScoreUI()
	{
		_scoreLabel.Text = $"Score: {_score}";
	}

	private void UpdateStatusUI()
	{
		if (_passed)
		{
			_statusLabel.Text = "Status: Completed";
			_statusLabel.AddThemeColorOverride("font_color", _success);
		}
		else
		{
			_statusLabel.Text = "Status: In progress";
			_statusLabel.AddThemeColorOverride("font_color", _mutedText);
		}
	}

	private void CompleteQuiz()
	{
		_passed = true;

		GD.Print("QUIZ COMPLETED");

		UpdateStatusUI();

		_quizProgressLabel.Text = "All questions answered";
		_questionLabel.Text = "Quiz completed";
		_firstAnswerButton.Disabled = true;
		_secondAnswerButton.Disabled = true;
		_submitButton.Visible = true;
		_quizCompleteDialog.PopupCentered();
	}

	private void SubmitScore()
	{
		if (!_passed)
		{
			GD.PrintErr("Quiz must be completed before submitting.");
			return;
		}

		GD.Print(
			$"Submitting score: {_score}, passed: {_passed}"
		);

		_api.UpdateScore(
			_playerId,
			_score
		);

		ShowDashboard();
	}

	private void Logout()
	{
		_playerId = "";
		_profileName = "";
		_score = 0;
		_passed = false;
		ShowLoginScreen();
	}

	private void ClearScreen()
	{
		foreach (Node child in _screen.GetChildren())
		{
			_screen.RemoveChild(child);
			child.QueueFree();
		}
	}

	private void CenterPanel(Control panel)
	{
		MarginContainer page = CreatePageMargin();
		CenterContainer center = new CenterContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill
		};
		center.AddChild(panel);
		page.AddChild(center);
		_screen.AddChild(page);
	}

	private MarginContainer CreatePageMargin()
	{
		MarginContainer page = new MarginContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill
		};
		page.AddThemeConstantOverride("margin_left", 40);
		page.AddThemeConstantOverride("margin_top", 32);
		page.AddThemeConstantOverride("margin_right", 40);
		page.AddThemeConstantOverride("margin_bottom", 32);
		return page;
	}

	private VBoxContainer CreatePanel(float width, bool framed = true)
	{
		VBoxContainer content = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(width, 0),
			SizeFlagsHorizontal = width > 0 ? SizeFlags.ShrinkCenter : SizeFlags.ExpandFill
		};
		content.AddThemeConstantOverride("separation", 14);

		if (!framed)
		{
			return content;
		}

		PanelContainer panel = new PanelContainer();
		panel.AddThemeStyleboxOverride(
			"panel",
			CreatePanelStyle(_panelBackground, _panelBorder)
		);

		MarginContainer margin = new MarginContainer();
		margin.AddThemeConstantOverride("margin_left", 28);
		margin.AddThemeConstantOverride("margin_top", 28);
		margin.AddThemeConstantOverride("margin_right", 28);
		margin.AddThemeConstantOverride("margin_bottom", 28);

		margin.AddChild(content);
		panel.AddChild(margin);

		VBoxContainer wrapper = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(width, 0),
			SizeFlagsHorizontal = SizeFlags.ShrinkCenter
		};
		wrapper.AddChild(panel);
		return wrapper;
	}

	private VBoxContainer CreateInfoPanel(string title, string value)
	{
		VBoxContainer panel = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(0, 96),
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		panel.AddThemeConstantOverride("separation", 8);

		Label titleLabel = CreateEyebrow(title);
		titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		panel.AddChild(titleLabel);

		Label valueLabel = CreateSectionTitle(value);
		valueLabel.HorizontalAlignment = HorizontalAlignment.Center;
		panel.AddChild(valueLabel);
		return panel;
	}

	private Control CreateLeaderboardHeader()
	{
		return CreateLeaderboardVisualRow(
			"Rank",
			"Player",
			"Score",
			Color.FromHtml("#243447"),
			Colors.White,
			15
		);
	}

	private Control CreateLeaderboardRow(
		int rank,
		string playerName,
		int score,
		bool isCurrentPlayer)
	{
		Color background = isCurrentPlayer
			? Color.FromHtml("#d9f2ea")
			: Color.FromHtml("#eef3f8");
		Color color = Colors.Black;

		return CreateLeaderboardVisualRow(
			$"#{rank}",
			playerName,
			score.ToString(),
			background,
			color,
			15
		);
	}

	private Control CreateLeaderboardVisualRow(
		string rank,
		string playerName,
		string score,
		Color background,
		Color color,
		int fontSize)
	{
		HBoxContainer content = new HBoxContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		content.AddThemeConstantOverride("separation", 12);
		content.AddChild(CreateLeaderboardCell(rank, 80, HorizontalAlignment.Center, color, fontSize));
		content.AddChild(CreateLeaderboardCell(playerName, 1, HorizontalAlignment.Left, color, fontSize));
		content.AddChild(CreateLeaderboardCell(score, 100, HorizontalAlignment.Right, color, fontSize));

		PanelContainer panel = new PanelContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		panel.AddThemeStyleboxOverride(
			"panel",
			CreatePanelStyle(background, Color.FromHtml("#c9d4df"), 6)
		);

		MarginContainer margin = new MarginContainer();
		margin.AddThemeConstantOverride("margin_left", 14);
		margin.AddThemeConstantOverride("margin_top", 8);
		margin.AddThemeConstantOverride("margin_right", 14);
		margin.AddThemeConstantOverride("margin_bottom", 8);
		margin.AddChild(content);
		panel.AddChild(margin);
		return panel;
	}

	private Label CreateLeaderboardCell(
		string text,
		float width,
		HorizontalAlignment alignment,
		Color color,
		int fontSize)
	{
		Label label = new Label
		{
			Text = text,
			HorizontalAlignment = alignment,
			VerticalAlignment = VerticalAlignment.Center,
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};

		if (width == 1)
		{
			label.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		}
		else
		{
			label.CustomMinimumSize = new Vector2(width, 24);
		}

		label.AddThemeFontSizeOverride("font_size", fontSize);
		label.AddThemeColorOverride("font_color", color);
		return label;
	}

	private Label CreateEyebrow(string text)
	{
		Label label = CreateLabel(text, 14, _mutedText);
		label.AddThemeConstantOverride("line_spacing", 2);
		return label;
	}

	private Label CreateTitle(string text)
	{
		return CreateLabel(text, 28, _text);
	}

	private Label CreateSectionTitle(string text)
	{
		return CreateLabel(text, 20, _text);
	}

	private Label CreateBodyText(string text)
	{
		return CreateLabel(text, 16, _mutedText);
	}

	private Label CreateStatusLabel(string text)
	{
		return CreateLabel(text, 15, _mutedText);
	}

	private Label CreateLabel(string text, int fontSize, Color color)
	{
		Label label = new Label
		{
			Text = text,
			HorizontalAlignment = HorizontalAlignment.Center,
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};
		label.AddThemeFontSizeOverride("font_size", fontSize);
		label.AddThemeColorOverride("font_color", color);
		return label;
	}

	private LineEdit CreateInput(string placeholder)
	{
		LineEdit input = new LineEdit
		{
			PlaceholderText = placeholder,
			CustomMinimumSize = new Vector2(0, 46)
		};
		input.FocusEntered += () => input.PlaceholderText = "";
		input.FocusExited += () =>
		{
			if (string.IsNullOrEmpty(input.Text))
			{
				input.PlaceholderText = placeholder;
			}
		};
		input.AddThemeFontSizeOverride("font_size", 16);
		input.AddThemeColorOverride("font_color", Colors.Black);
		input.AddThemeColorOverride("font_placeholder_color", Color.FromHtml("#5f6368"));
		input.AddThemeColorOverride("caret_color", Colors.Black);
		input.AddThemeStyleboxOverride(
			"normal",
			CreateInputStyle(Color.FromHtml("#ffffff"), _panelBorder)
		);
		input.AddThemeStyleboxOverride(
			"focus",
			CreateInputStyle(Color.FromHtml("#ffffff"), _primary)
		);
		return input;
	}

	private Button CreateButton(string text, bool primary, Color? overrideColor = null)
	{
		Color baseColor = overrideColor ?? (primary ? _primary : _secondary);
		Color hoverColor = overrideColor ?? (primary ? _primaryHover : _secondaryHover);
		Color fontColor = primary || overrideColor != null ? Colors.White : Colors.Black;

		Button button = new Button
		{
			Text = text,
			CustomMinimumSize = new Vector2(0, 48),
			MouseDefaultCursorShape = CursorShape.PointingHand
		};
		button.AddThemeFontSizeOverride("font_size", 16);
		button.AddThemeColorOverride("font_color", fontColor);
		button.AddThemeColorOverride("font_hover_color", fontColor);
		button.AddThemeColorOverride("font_pressed_color", fontColor);
		button.AddThemeColorOverride("font_focus_color", fontColor);
		button.AddThemeStyleboxOverride(
			"normal",
			CreateButtonStyle(baseColor)
		);
		button.AddThemeStyleboxOverride(
			"hover",
			CreateButtonStyle(hoverColor)
		);
		button.AddThemeStyleboxOverride(
			"pressed",
			CreateButtonStyle(hoverColor.Darkened(0.1f))
		);
		button.AddThemeStyleboxOverride(
			"disabled",
			CreateButtonStyle(Color.FromHtml("#d7dde3"))
		);
		button.AddThemeColorOverride("font_disabled_color", Color.FromHtml("#7b8792"));
		return button;
	}

	private Control CreateSpacer(float height)
	{
		return new Control
		{
			CustomMinimumSize = new Vector2(0, height)
		};
	}

	private StyleBoxFlat CreatePanelStyle(Color background, Color border, int radius = 8)
	{
		StyleBoxFlat style = new StyleBoxFlat
		{
			BgColor = background,
			BorderColor = border,
			CornerRadiusTopLeft = radius,
			CornerRadiusTopRight = radius,
			CornerRadiusBottomRight = radius,
			CornerRadiusBottomLeft = radius,
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1
		};
		return style;
	}

	private StyleBoxFlat CreateInputStyle(Color background, Color border)
	{
		StyleBoxFlat style = CreatePanelStyle(background, border, 6);
		style.ContentMarginLeft = 14;
		style.ContentMarginRight = 14;
		style.ContentMarginTop = 10;
		style.ContentMarginBottom = 10;
		return style;
	}

	private StyleBoxFlat CreateButtonStyle(Color color)
	{
		StyleBoxFlat style = new StyleBoxFlat
		{
			BgColor = color,
			CornerRadiusTopLeft = 6,
			CornerRadiusTopRight = 6,
			CornerRadiusBottomRight = 6,
			CornerRadiusBottomLeft = 6
		};
		style.ContentMarginLeft = 16;
		style.ContentMarginRight = 16;
		style.ContentMarginTop = 10;
		style.ContentMarginBottom = 10;
		return style;
	}
}
