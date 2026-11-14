using Godot;
using System;

/// <summary>
/// Manages a simple turn-based combat loop between a Player (MechCombatant)
/// and an Enemy (EnemyCombatant). Player input is blocked during the enemy's turn.
/// Attach this script to a dedicated "TurnManager" node in your combat scene.
/// </summary>
public partial class TurnManager : Node
{
	public enum CombatState
	{
		PlayerTurn,
		EnemyTurn,
		GameOver
	}

	[Export] public NodePath PlayerPath { get; set; }
	[Export] public NodePath EnemyPath { get; set; }
	[Export] public NodePath AIControllerPath { get; set; }

	private MechCombatant _player;
	private EnemyCombatant _enemy;
	private AIController _aiController;
	private CombatManager _combatManager;
	private CombatState _currentState = CombatState.GameOver;

	/// <summary>
	/// Emitted when a combatant takes damage. Useful for updating UI.
	/// Parameters: targetName (string), damage (int), remainingHP (int).
	/// </summary>
	[Signal]
	public delegate void CombatLogUpdatedEventHandler(string message);

	/// <summary>
	/// Emitted when combat ends (one side reaches 0 HP).
	/// Parameter: true if the player won, false if the enemy won.
	/// </summary>
	[Signal]
	public delegate void CombatEndedEventHandler(bool playerWon);

	/// <summary>
	/// Emitted after every attack with the current HP of both combatants.
	/// Connect this to your UI to update HP bars.
	/// </summary>
	[Signal]
	public delegate void HPUpdatedEventHandler(int playerHP, int playerMaxHP, int enemyHP, int enemyMaxHP);

	/// <summary>
	/// Emitted when the player's energy changes (after using an ability).
	/// Connect this to your UI to update an energy bar.
	/// </summary>
	[Signal]
	public delegate void EnergyUpdatedEventHandler(int currentEnergy, int maxEnergy);

	public override void _Ready()
	{
		if (PlayerPath == null || PlayerPath.IsEmpty ||
			EnemyPath == null || EnemyPath.IsEmpty)
		{
			GD.PushError("TurnManager: PlayerPath and EnemyPath must both be assigned.");
			_currentState = CombatState.GameOver;
			return;
		}

		_player = GetNodeOrNull<MechCombatant>(PlayerPath);
		_enemy = GetNodeOrNull<EnemyCombatant>(EnemyPath);
		if (_player == null || _enemy == null)
		{
			GD.PushError("TurnManager: PlayerPath must point to MechCombatant and EnemyPath to EnemyCombatant.");
			_currentState = CombatState.GameOver;
			return;
		}

		if (AIControllerPath != null && !AIControllerPath.IsEmpty)
			_aiController = GetNodeOrNull<AIController>(AIControllerPath);
		_combatManager = new CombatManager();

		_player.InitializeForCombat();
		_enemy.MaxHP = Math.Max(0, _enemy.MaxHP);
		_enemy.CurrentHP = Math.Clamp(_enemy.CurrentHP, 0, _enemy.MaxHP);
		_enemy.MaxEnergy = Math.Max(0, _enemy.MaxEnergy);
		_enemy.CurrentEnergy = Math.Clamp(_enemy.CurrentEnergy, 0, _enemy.MaxEnergy);

		_currentState = CombatState.PlayerTurn;
		if (CheckVictoryCondition())
			return;

		EmitSignal(SignalName.CombatLogUpdated, "Combat started — your turn!");
		EmitSignal(SignalName.HPUpdated, _player.CurrentHP, _player.MaxHP, _enemy.CurrentHP, _enemy.MaxHP);
		EmitSignal(SignalName.EnergyUpdated, _player.CurrentEnergy, _player.MaxEnergy);
	}

	/// <summary>
	/// Returns true if it is currently the player's turn and input is allowed.
	/// </summary>
	public bool IsPlayerInputAllowed => _currentState == CombatState.PlayerTurn;

	public bool IsCombatOver => _currentState == CombatState.GameOver;

	public bool? PlayerWon { get; private set; }

	public void EmitCurrentStatus()
	{
		if (_player == null || _enemy == null ||
			!GodotObject.IsInstanceValid(_player) ||
			!GodotObject.IsInstanceValid(_enemy))
			return;

		EmitSignal(SignalName.HPUpdated, _player.CurrentHP, _player.MaxHP, _enemy.CurrentHP, _enemy.MaxHP);
		EmitSignal(SignalName.EnergyUpdated, _player.CurrentEnergy, _player.MaxEnergy);
	}

	/// <summary>
	/// Call this from your UI button or input handler when the player chooses to attack.
	/// Does nothing if it is not the player's turn.
	/// </summary>
	public void ExecutePlayerAttack()
	{
		if (_currentState != CombatState.PlayerTurn)
		{
			GD.Print("Input blocked — it is the enemy's turn.");
			return;
		}

		var playerPosComp = _player.GetNodeOrNull<PositionComponent>("PositionComponent");
		var enemyPosComp = _enemy.GetNodeOrNull<PositionComponent>("PositionComponent");
		if (playerPosComp != null && enemyPosComp != null &&
			CombatRules.Distance(playerPosComp.Position, enemyPosComp.Position) > CombatConstants.DefaultAttackRange)
		{
			EmitSignal(SignalName.CombatLogUpdated, "Target out of range!");
			return;
		}

		int damage = _combatManager.CalculateDamage(_player, _enemy);
		_enemy.CurrentHP -= damage;
		_enemy.CurrentHP = Math.Max(0, _enemy.CurrentHP);

		string message = $"Player attacks for {damage} damage! Enemy HP: {_enemy.CurrentHP}/{_enemy.MaxHP}";
		GD.Print(message);
		EmitSignal(SignalName.CombatLogUpdated, message);
		EmitSignal(SignalName.HPUpdated, _player.CurrentHP, _player.MaxHP, _enemy.CurrentHP, _enemy.MaxHP);

		if (CheckVictoryCondition()) return;

		// Hand the turn over to the enemy after a short delay
		StartEnemyTurn();
	}

	/// <summary>
	/// Executes a player ability against the enemy. Returns false if energy is insufficient
	/// or if it is not the player's turn.
	/// </summary>
	public bool ExecutePlayerAbility(Ability ability)
	{
		if (_currentState != CombatState.PlayerTurn)
		{
			GD.Print("Input blocked — it is the enemy's turn.");
			return false;
		}

		if (ability == null)
		{
			EmitSignal(SignalName.CombatLogUpdated, "No ability selected!");
			return false;
		}

		if (!CombatRules.IsValidAbility(
			ability.EnergyCost,
			ability.DamageMultiplier,
			ability.MinRange,
			ability.MaxRange))
		{
			EmitSignal(SignalName.CombatLogUpdated, "This ability has invalid configuration.");
			return false;
		}

		// Verify distance between Attacker and Defender
		var playerPosComp = _player.GetNodeOrNull<PositionComponent>("PositionComponent");
		var enemyPosComp = _enemy.GetNodeOrNull<PositionComponent>("PositionComponent");

		if (playerPosComp != null && enemyPosComp != null)
		{
			long distance = CombatRules.Distance(playerPosComp.Position, enemyPosComp.Position);
			if (distance < ability.MinRange || distance > ability.MaxRange)
			{
				string rangeMsg = "Target out of range!";
				GD.Print(rangeMsg);
				EmitSignal(SignalName.CombatLogUpdated, rangeMsg);
				return false;
			}
		}

		// Check if the player has enough energy
		if (_player.CurrentEnergy < ability.EnergyCost)
		{
			string failMsg = $"Not enough energy for {ability.Name}! (Need {ability.EnergyCost}, have {_player.CurrentEnergy})";
			GD.Print(failMsg);
			EmitSignal(SignalName.CombatLogUpdated, failMsg);
			return false;
		}

		// Subtract the energy cost
		_player.CurrentEnergy -= ability.EnergyCost;
		EmitSignal(SignalName.EnergyUpdated, _player.CurrentEnergy, _player.MaxEnergy);

		// Calculate ability damage
		int damage = _combatManager.CalculateAbilityDamage(_player, _enemy, ability);
		_enemy.CurrentHP -= damage;
		_enemy.CurrentHP = Math.Max(0, _enemy.CurrentHP);

		string message = $"Player used {ability.Name} for {damage} damage! (Energy: {_player.CurrentEnergy}/{_player.MaxEnergy}) Enemy HP: {_enemy.CurrentHP}/{_enemy.MaxHP}";
		GD.Print(message);
		EmitSignal(SignalName.CombatLogUpdated, message);
		EmitSignal(SignalName.HPUpdated, _player.CurrentHP, _player.MaxHP, _enemy.CurrentHP, _enemy.MaxHP);

		if (CheckVictoryCondition()) return true;

		// Hand the turn over to the enemy
		StartEnemyTurn();
		return true;
	}

	/// <summary>
	/// Executes a player move action. Costs 5 energy.
	/// </summary>
	public void ExecutePlayerMove(int targetPosition)
	{
		if (_currentState != CombatState.PlayerTurn)
		{
			GD.Print("Input blocked — it is the enemy's turn.");
			return;
		}

		var playerPosComp = _player.GetNodeOrNull<PositionComponent>("PositionComponent");
		if (playerPosComp == null)
		{
			EmitSignal(SignalName.CombatLogUpdated, "Movement is unavailable: the player has no PositionComponent.");
			return;
		}

		if (targetPosition == playerPosComp.Position ||
			CombatRules.Distance(playerPosComp.Position, targetPosition) > CombatConstants.MoveRange)
		{
			EmitSignal(SignalName.CombatLogUpdated, "Choose an adjacent cell to move one space.");
			return;
		}

		var enemyPosComp = _enemy.GetNodeOrNull<PositionComponent>("PositionComponent");
		if (enemyPosComp != null && targetPosition == enemyPosComp.Position)
		{
			EmitSignal(SignalName.CombatLogUpdated, "Cell blocked!");
			return;
		}

		int cost = CombatConstants.MoveEnergyCost;
		if (_player.CurrentEnergy < cost)
		{
			string failMsg = $"Not enough energy to move! (Need {cost}, have {_player.CurrentEnergy})";
			EmitSignal(SignalName.CombatLogUpdated, failMsg);
			return;
		}

		playerPosComp.Position = targetPosition;
		_player.CurrentEnergy -= cost;
		EmitSignal(SignalName.EnergyUpdated, _player.CurrentEnergy, _player.MaxEnergy);
		EmitSignal(SignalName.CombatLogUpdated, $"Player moved to position {targetPosition}.");

		StartEnemyTurn();
	}

	/// <summary>
	/// Executes an enemy move action. Called by AIController.
	/// </summary>
	public void ExecuteEnemyMove(int targetPosition)
	{
		if (_currentState != CombatState.EnemyTurn)
			return;

		var enemyPosComp = _enemy.GetNodeOrNull<PositionComponent>("PositionComponent");
		if (enemyPosComp == null)
		{
			EmitSignal(SignalName.CombatLogUpdated, "Enemy movement is unavailable; the enemy attacks instead.");
			ExecuteEnemyAttack();
			return;
		}

		var playerPosComp = _player.GetNodeOrNull<PositionComponent>("PositionComponent");
		if (targetPosition == enemyPosComp.Position ||
			CombatRules.Distance(enemyPosComp.Position, targetPosition) > CombatConstants.MoveRange ||
			(playerPosComp != null && targetPosition == playerPosComp.Position))
		{
			EmitSignal(SignalName.CombatLogUpdated, "Enemy could not move to that cell; the enemy attacks instead.");
			ExecuteEnemyAttack();
			return;
		}

		int cost = CombatConstants.MoveEnergyCost;
		if (_enemy.CurrentEnergy < cost)
		{
			EmitSignal(SignalName.CombatLogUpdated, "Enemy does not have enough energy to move; turn skipped.");
			StartPlayerTurn();
			return;
		}

		enemyPosComp.Position = targetPosition;
		_enemy.CurrentEnergy -= cost;
		EmitSignal(SignalName.CombatLogUpdated, $"Enemy moved to position {targetPosition}.");

		StartPlayerTurn();
	}

	/// <summary>
	/// Transitions to the enemy's turn, waits 1 second, then executes the enemy's action.
	/// </summary>
	private async void StartEnemyTurn()
	{
		_currentState = CombatState.EnemyTurn;
		_enemy.CurrentEnergy = CombatRules.RegenerateEnergy(
			_enemy.CurrentEnergy,
			_enemy.MaxEnergy,
			CombatConstants.EnergyRegenAmount);
		EmitSignal(SignalName.CombatLogUpdated,
			$"Enemy is preparing to act... (Energy: {_enemy.CurrentEnergy}/{_enemy.MaxEnergy})");

		// Wait before the enemy acts
		await ToSignal(GetTree().CreateTimer(CombatConstants.TurnDelay), SceneTreeTimer.SignalName.Timeout);

		// Guard: if the node was freed during the wait (e.g., scene change), bail out
		if (!IsInsideTree()) return;

		if (_aiController != null && _aiController.IsConfiguredFor(this, _enemy, _player))
			_aiController.DecideAction();
		else
			ExecuteEnemyAttack();
	}

	/// <summary>
	/// The enemy automatically attacks the player, then hands the turn back.
	/// </summary>
	public void ExecuteEnemyAttack()
	{
		if (_currentState != CombatState.EnemyTurn)
			return;

		int damage = _combatManager.CalculateDamage(_enemy, _player);
		_player.CurrentHP -= damage;
		_player.CurrentHP = Math.Max(0, _player.CurrentHP);

		string message = $"Enemy attacks for {damage} damage! Player HP: {_player.CurrentHP}/{_player.MaxHP}";
		GD.Print(message);
		EmitSignal(SignalName.CombatLogUpdated, message);
		EmitSignal(SignalName.HPUpdated, _player.CurrentHP, _player.MaxHP, _enemy.CurrentHP, _enemy.MaxHP);

		if (CheckVictoryCondition()) return;

		// Return control to the player
		StartPlayerTurn();
	}

	/// <summary>
	/// Starts the player's turn and regenerates energy.
	/// </summary>
	private void StartPlayerTurn()
	{
		if (_currentState == CombatState.GameOver) return;
		
		_currentState = CombatState.PlayerTurn;
		
		_player.CurrentEnergy = CombatRules.RegenerateEnergy(
			_player.CurrentEnergy,
			_player.MaxEnergy,
			CombatConstants.EnergyRegenAmount);
		EmitSignal(SignalName.EnergyUpdated, _player.CurrentEnergy, _player.MaxEnergy);
		
		EmitSignal(SignalName.CombatLogUpdated, $"Your turn! (+{CombatConstants.EnergyRegenAmount} Energy)");
	}

	/// <summary>
	/// Checks if either combatant's HP has reached 0. Returns true if the game is over.
	/// </summary>
	private bool CheckVictoryCondition()
	{
		if (_enemy.CurrentHP <= 0)
		{
			EndCombat(playerWon: true);
			return true;
		}
		else if (_player.CurrentHP <= 0)
		{
			EndCombat(playerWon: false);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Ends the combat loop and emits the result signal.
	/// </summary>
	private void EndCombat(bool playerWon)
	{
		PlayerWon = playerWon;
		string result = playerWon ? "Victory! The enemy has been destroyed." : "Defeat... your mech has been destroyed.";
		GD.Print(result);
		EmitSignal(SignalName.CombatLogUpdated, result);
		EmitSignal(SignalName.CombatEnded, playerWon);

		// Freeze the state so no further input is processed
		_currentState = CombatState.GameOver;
	}
}
