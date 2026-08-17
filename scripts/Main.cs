using Godot;

public partial class Main : Control
{
	private ApiClient _api;

	private string _playerId = "player001";

	private int _score = 0;

	private Label _scoreLabel;

	public override void _Ready()
	{
		_api = new ApiClient();

		AddChild(_api);

		_scoreLabel = GetNode<Label>(
            "VBoxContainer/ScoreLabel"
		);

		Button correctButton = GetNode<Button>(
            "VBoxContainer/CorrectButton"
		);

		Button submitButton = GetNode<Button>(
            "VBoxContainer/SubmitButton"
		);

		correctButton.Pressed += AnswerCorrect;
		submitButton.Pressed += SubmitScore;

		GD.Print("Godot Security Lab Started");
	}

	private void AnswerCorrect()
	{
		_score++;

		UpdateScoreUI();

		GD.Print($"Score: {_score}");

		if (_score >= 3)
		{
			PassQuiz();
		}
	}

	private void UpdateScoreUI()
	{
		_scoreLabel.Text = $"Score: {_score}";
	}

	private void PassQuiz()
	{
		GD.Print("PLAYER PASSED");

		_api.UpdateScore(
			_playerId,
			_score
		);
	}

	private void SubmitScore()
	{
		GD.Print($"Submitting score: {_score}");

		_api.UpdateScore(
			_playerId,
			_score
		);
	}
}
