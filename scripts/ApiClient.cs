using Godot;
using System;
using System.Text;

public partial class ApiClient : Node
{
	public event Action<Godot.Collections.Dictionary> PlayerLoaded;
	public event Action<string> PlayerLoadFailed;

	private const string ApiUrl = "http://127.0.0.1:8000";

	private HttpRequest _httpRequest;

	public override void _Ready()
	{
		_httpRequest = new HttpRequest();
		AddChild(_httpRequest);

		_httpRequest.RequestCompleted += OnRequestCompleted;

		GD.Print("API Client initialized.");
		UpdateScore("player001",0);
	}

	public void GetPlayer(string playerId)
	{
		string url = $"{ApiUrl}/api/player/{playerId}";

		Error error = _httpRequest.Request(url);

		if (error != Error.Ok)
		{
			GD.PrintErr($"GET request failed: {error}");
		}
	}

	public void UpdateScore(string playerId, int score)
	{
		string url = $"{ApiUrl}/api/score";

		var data = new Godot.Collections.Dictionary
		{
			{ "player_id", playerId },
			{ "score", score }
		};

		string json = Json.Stringify(data);

		string[] headers =
		{
            "Content-Type: application/json"
		};

		Error error = _httpRequest.Request(
			url,
			headers,
			HttpClient.Method.Post,
			json
		);

		if (error != Error.Ok)
		{
			GD.PrintErr($"Score request failed: {error}");
		}
	}

	public void UpdateUser(
		string playerId,
		string name,
		string department)
	{
		string url = $"{ApiUrl}/api/user";

		var data = new Godot.Collections.Dictionary
		{
			{ "player_id", playerId },
			{ "name", name },
			{ "department", department }
		};

		string json = Json.Stringify(data);

		string[] headers =
		{
            "Content-Type: application/json"
		};

		Error error = _httpRequest.Request(
			url,
			headers,
			HttpClient.Method.Post,
			json
		);

		if (error != Error.Ok)
		{
			GD.PrintErr($"User request failed: {error}");
		}
	}

	private void OnRequestCompleted(
		long result,
		long responseCode,
		string[] headers,
		byte[] body)
	{
		GD.Print($"HTTP Status: {responseCode}");

		string response = Encoding.UTF8.GetString(body);

		GD.Print("Server Response:");
		GD.Print(response);

		Variant parsedResponse = Json.ParseString(response);

		if (parsedResponse.VariantType != Variant.Type.Dictionary)
		{
			return;
		}

		var data = parsedResponse.AsGodotDictionary();

		if (data.ContainsKey("error"))
		{
			PlayerLoadFailed?.Invoke(
				data["error"].AsString()
			);
			return;
		}

		if (data.ContainsKey("username") && data.ContainsKey("profile"))
		{
			PlayerLoaded?.Invoke(data);
		}
	}
}
