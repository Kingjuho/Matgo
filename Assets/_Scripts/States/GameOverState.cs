using System.Collections;
using UnityEngine;

public class GameOverState : GameStateBase
{
    public GameOverState(GameManager manager) : base(manager) { }

    public override IEnumerator Execute()
    {
        // 나가리
        if (GameManager.isNagari)
        {
            GameManager.gameMultiplier *= 2;

            // TODO: 나가리 팝업
            yield return new WaitForSeconds(2.0f);

            GameManager.ChangeState(GameManager.StateInit);
            yield break;
        }

        Player winner = GameManager.finalWinner;
        if (winner == null)
        {
            GameManager.ChangeState(GameManager.StateInit);
            yield break;
        }

        Player loser = (winner == GameManager.humanPlayer)
        ? GameManager.computerPlayer
        : GameManager.humanPlayer;

        yield return WaitForPopupResult
        (
            callback => UIManager.Instance != null &&
                        UIManager.Instance.ShowGameOverPopup
                        (
                            winner,
                            GameManager.finalAmount,
                            GameManager.finalScore,
                            callback
                        ),
            _ => { },
            true
        );

        // 정산
        winner.money += GameManager.finalAmount;
        loser.money -= GameManager.finalAmount;
        GameManager.gameMultiplier = 1;

        // 돈 UI 갱신
        UIManager.Instance?.RefreshMoneyUI(GameManager.humanPlayer, GameManager.computerPlayer);

        yield return null;
        GameManager.ChangeState(GameManager.StateInit);
    }
}
