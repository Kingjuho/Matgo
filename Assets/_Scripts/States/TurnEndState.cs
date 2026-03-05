using System.Collections;
using UnityEngine;

public class TurnEndState : GameStateBase
{
    public TurnEndState(GameManager manager) : base(manager) { }

    public override IEnumerator Execute()
    {
        // 나가리 체크
        if (GameManager.humanPlayer.handCards.Count == 0 && GameManager.computerPlayer.handCards.Count == 0)
        {
            // 다음 판 배당 2배
            GameManager.isNagari = true;
            GameManager.ChangeState(GameManager.StateGameOver);
            yield break;
        }

        // 반대쪽으로 턴 넘기기
        GameManager.ChangeState((GameManager.currentPlayer == GameManager.humanPlayer) ? GameManager.StateAITurn : GameManager.StatePlayerTurn);
        yield return null;
    }
}
