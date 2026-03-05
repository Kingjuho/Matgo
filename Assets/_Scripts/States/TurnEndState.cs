using System.Collections;
using UnityEngine;

public class TurnEndState : GameStateBase
{
    public TurnEndState(GameManager manager) : base(manager) { }

    public override IEnumerator Execute()
    {
        // 반대쪽으로 턴 넘기기
        GameManager.ChangeState((GameManager.currentPlayer == GameManager.humanPlayer) ? GameManager.StateAITurn : GameManager.StatePlayerTurn);
        yield return null;
    }
}