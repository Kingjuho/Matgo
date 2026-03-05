using System.Collections;
using UnityEngine;

public class AITurnState : GameStateBase
{
    public AITurnState(GameManager manager) : base(manager) { }

    public override IEnumerator Execute()
    {

        // 플레이어 초기화
        GameManager.currentPlayer = GameManager.computerPlayer;
        GameManager.currentPlayer.StartTurn();

        // 1초 대기(고민하는 척)
        yield return new WaitForSeconds(1.0f);

        // TODO: AI 로직 구현
        if (GameManager.currentPlayer.handCards.Count > 0)
        {
            // 자신이 가진 첫 번째 카드 제출
            GameManager.currentPlayer.selectedCard = GameManager.currentPlayer.handCards[0];
            GameManager.currentPlayer.hasPlayed = true;
        }

        GameManager.ChangeState(GameManager.StatePlayHandCard);
    }
}
