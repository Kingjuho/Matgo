using System.Collections;
using UnityEngine;

public class PlayerTurnState : GameStateBase
{
    public PlayerTurnState(GameManager manager) : base(manager) { }

    public override IEnumerator Execute()
    {
        Debug.Log("PlayerTurn 시작");

        // 플레이어 초기화
        GameManager.currentPlayer = GameManager.humanPlayer;
        GameManager.currentPlayer.StartTurn();

        // 카드 선택까지 대기
        yield return new WaitUntil(() => GameManager.currentPlayer.hasPlayed);

        GameManager.ChangeState(GameManager.StatePlayHandCard);
    }
}
