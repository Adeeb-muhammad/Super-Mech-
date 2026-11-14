using Godot;

/// <summary>
/// Basic Tactical AI for the Enemy.
/// Calculates distance and decides whether to attack or move closer.
/// </summary>
public partial class AIController : Node
{
    [Export] public TurnManager TurnManager { get; set; }
    [Export] public EnemyCombatant Enemy { get; set; }
    [Export] public MechCombatant Player { get; set; }

    public bool IsConfiguredFor(TurnManager expectedManager, EnemyCombatant expectedEnemy, MechCombatant expectedPlayer)
    {
        return TurnManager != null && Enemy != null && Player != null &&
               GodotObject.IsInstanceValid(TurnManager) &&
               GodotObject.IsInstanceValid(Enemy) &&
               GodotObject.IsInstanceValid(Player) &&
               TurnManager == expectedManager &&
               Enemy == expectedEnemy &&
               Player == expectedPlayer;
    }

    /// <summary>
    /// Evaluates combat state and executes an action.
    /// </summary>
    public void DecideAction()
    {
        if (!IsConfiguredFor(TurnManager, Enemy, Player))
        {
            GD.PrintErr("AIController missing references!");
            return;
        }

        var playerPosComp = Player.GetNodeOrNull<PositionComponent>("PositionComponent");
        var enemyPosComp = Enemy.GetNodeOrNull<PositionComponent>("PositionComponent");

        if (playerPosComp != null && enemyPosComp != null)
        {
            long distance = CombatRules.Distance(playerPosComp.Position, enemyPosComp.Position);
            
            int attackRange = CombatConstants.DefaultAttackRange;

            if (distance <= attackRange)
            {
                // In range: Attack
                TurnManager.ExecuteEnemyAttack();
            }
            else
            {
                // Out of range: Move 1 step closer
                int direction = playerPosComp.Position > enemyPosComp.Position ? 1 : -1;
                int targetPosition = enemyPosComp.Position + direction;
                
                // Collision check
                if (targetPosition == playerPosComp.Position)
                {
                    // Fallback to attack if we are right next to them but somehow still considered out of range
                    TurnManager.ExecuteEnemyAttack();
                }
                else
                {
                    TurnManager.ExecuteEnemyMove(targetPosition);
                }
            }
        }
        else
        {
            // Fallback if no positioning system is found
            TurnManager.ExecuteEnemyAttack();
        }
    }
}
