using System.Collections;
using UnityEngine;

public class GameOverState : GameStateBase
{
    public GameOverState(GameManager manager) : base(manager) { }

    public override IEnumerator Execute()
    {
        // 해당 판이 나가리였을 경우
        if (GameManager.isNagari)
        {
            // 판 배당 2배
            GameManager.gameMultiplier *= 2;

            // TODO: 나가리 팝업
            // UIManager.Instance.ShowNagariPopup()

            // 2초 대기
            yield return new WaitForSeconds(2.0f);
        }
        // 정상 진행
        else
        {
            Player winner = GameManager.finalWinner;
            Player loser = (winner == GameManager.humanPlayer) ? GameManager.computerPlayer : GameManager.humanPlayer;

            // 금액 계산
            winner.money += GameManager.finalAmount;
            loser.money -= GameManager.finalAmount;

            // 판 배당 리셋
            GameManager.gameMultiplier = 1;

            // TODO: 승패 팝업, 확인 누를 때까지 대기
            // UIManager.Instance.ShowGameOverPopup(winner, finalAmount)

            // TODO: 승패 팝업 확인 누를 때까지 대기로 변경
            yield return new WaitForSeconds(2.0f);
        }

        GameManager.ChangeState(GameManager.StateInit);
    }
}
