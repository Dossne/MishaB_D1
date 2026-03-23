using System.Collections.Generic;
using TetrisTactic.PlayField;
using TetrisTactic.Units;

namespace TetrisTactic.EnemyTurn
{
    public enum EnemyActionType
    {
        None = 0,
        Move = 1,
        Attack = 2,
        Wait = 3,
    }

    public sealed class EnemyDecisionModel
    {
        private EnemyDecisionModel(UnitRuntimeModel enemy, EnemyActionType actionType)
        {
            Enemy = enemy;
            ActionType = actionType;
        }

        public UnitRuntimeModel Enemy { get; }
        public EnemyActionType ActionType { get; }
        public GridPosition MoveDestination { get; private set; }
        public IReadOnlyList<List<GridPosition>> AttackWaveSteps { get; private set; }

        public static EnemyDecisionModel CreateMove(UnitRuntimeModel enemy, GridPosition destination)
        {
            return new EnemyDecisionModel(enemy, EnemyActionType.Move)
            {
                MoveDestination = destination,
            };
        }

        public static EnemyDecisionModel CreateAttack(UnitRuntimeModel enemy, IReadOnlyList<List<GridPosition>> waveSteps)
        {
            return new EnemyDecisionModel(enemy, EnemyActionType.Attack)
            {
                AttackWaveSteps = waveSteps,
            };
        }

        public static EnemyDecisionModel CreateWait(UnitRuntimeModel enemy)
        {
            return new EnemyDecisionModel(enemy, EnemyActionType.Wait);
        }

        public static EnemyDecisionModel CreateNone(UnitRuntimeModel enemy)
        {
            return new EnemyDecisionModel(enemy, EnemyActionType.None);
        }
    }
}
