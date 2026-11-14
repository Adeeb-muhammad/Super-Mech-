using Godot;

/// <summary>
/// Full combat HUD that listens to TurnManager signals and updates the display.
/// 
/// Expected scene structure (all children of this Control node):
///   CombatUI (Control)  ← attach this script
///     ├── PlayerHPBar (ProgressBar)
///     ├── PlayerHPLabel (Label)
///     ├── EnemyHPBar (ProgressBar)
///     ├── EnemyHPLabel (Label)
///     ├── CombatLog (RichTextLabel)
///     ├── TurnIndicator (Label)
///     └── AttackButton (Button)
///
/// Assign your TurnManager node via the export in the Inspector.
/// </summary>
public partial class CombatUI : Control
{
	[Export] public TurnManager TurnManager { get; set; }

	// --- HP display nodes ---
	[Export] public ProgressBar PlayerHPBar { get; set; }
	[Export] public Label PlayerHPLabel { get; set; }
	[Export] public ProgressBar EnemyHPBar { get; set; }
	[Export] public Label EnemyHPLabel { get; set; }

	// --- Combat log ---
	[Export] public RichTextLabel CombatLog { get; set; }

	// --- Turn indicator & attack button ---
	[Export] public Label TurnIndicator { get; set; }
	[Export] public Button AttackButton { get; set; }

	public override void _Ready()
	{
		if (TurnManager == null)
		{
			GD.PrintErr("CombatUI: TurnManager export is not assigned!");
			return;
		}

		// Connect to all TurnManager signals
		TurnManager.CombatLogUpdated += OnCombatLogUpdated;
		TurnManager.HPUpdated += OnHPUpdated;
		TurnManager.CombatEnded += OnCombatEnded;

		// Wire the attack button to fire the player's attack
		if (AttackButton != null)
		{
			AttackButton.Pressed += OnAttackButtonPressed;
		}

		// TurnManager may have emitted its initial signals before this UI connected.
		TurnManager.EmitCurrentStatus();
		UpdateTurnIndicator();
	}

	/// <summary>
	/// Appends a timestamped message to the combat log.
	/// </summary>
	private void OnCombatLogUpdated(string message)
	{
		if (CombatLog == null) return;

		CombatLog.AppendText($"[color=gray]> [/color]{message}\n");

		// Auto-scroll to the bottom so the latest message is always visible
		CombatLog.ScrollToLine(CombatLog.GetLineCount() - 1);

		// Update the turn indicator and button state
		UpdateTurnIndicator();
	}

	/// <summary>
	/// Updates both HP bars and labels when HP values change.
	/// </summary>
	private void OnHPUpdated(int playerHP, int playerMaxHP, int enemyHP, int enemyMaxHP)
	{
		if (PlayerHPBar != null)
		{
			PlayerHPBar.MaxValue = playerMaxHP;
			PlayerHPBar.Value = playerHP;
		}
		if (PlayerHPLabel != null)
		{
			PlayerHPLabel.Text = $"MECH  {playerHP} / {playerMaxHP}";
		}

		if (EnemyHPBar != null)
		{
			EnemyHPBar.MaxValue = enemyMaxHP;
			EnemyHPBar.Value = enemyHP;
		}
		if (EnemyHPLabel != null)
		{
			EnemyHPLabel.Text = $"ENEMY  {enemyHP} / {enemyMaxHP}";
		}
	}

	/// <summary>
	/// Handles the end of combat — disables input and shows the result.
	/// </summary>
	private void OnCombatEnded(bool playerWon)
	{
		if (AttackButton != null)
		{
			AttackButton.Disabled = true;
			AttackButton.Text = playerWon ? "VICTORY" : "DEFEATED";
		}

		if (TurnIndicator != null)
		{
			TurnIndicator.Text = playerWon ? "YOU WIN!" : "GAME OVER";
		}
	}

	/// <summary>
	/// Fires the player's attack through TurnManager.
	/// TurnManager internally ignores this if it's not the player's turn.
	/// </summary>
	private void OnAttackButtonPressed()
	{
		TurnManager.ExecutePlayerAttack();
	}

	/// <summary>
	/// Updates the turn indicator label and enables/disables the attack button
	/// based on whose turn it currently is.
	/// </summary>
	private void UpdateTurnIndicator()
	{
		if (TurnManager.IsCombatOver)
		{
			string result = TurnManager.PlayerWon == true
				? "YOU WIN!"
				: TurnManager.PlayerWon == false ? "GAME OVER" : "COMBAT UNAVAILABLE";

			if (TurnIndicator != null)
				TurnIndicator.Text = result;

			if (AttackButton != null)
			{
				AttackButton.Disabled = true;
				AttackButton.Text = result;
			}
			return;
		}

		bool playerTurn = TurnManager.IsPlayerInputAllowed;
		if (TurnIndicator != null)
			TurnIndicator.Text = playerTurn ? "YOUR TURN" : "ENEMY TURN";

		if (AttackButton != null)
			AttackButton.Disabled = !playerTurn;
	}

	public override void _ExitTree()
	{
		// Clean up signal connections to prevent memory leaks
		if (TurnManager != null)
		{
			TurnManager.CombatLogUpdated -= OnCombatLogUpdated;
			TurnManager.HPUpdated -= OnHPUpdated;
			TurnManager.CombatEnded -= OnCombatEnded;
		}

		if (AttackButton != null)
		{
			AttackButton.Pressed -= OnAttackButtonPressed;
		}
	}
}
