using System.Collections;
using System.Linq;
using UnityEngine;

public class CheckScoreState : GameStateBase
{
    public CheckScoreState(GameManager manager) : base(manager) { }

    public override IEnumerator Execute()
    {
        // 점수 계산
        int score = GameManager.currentPlayer.CalculateScore();
        Debug.Log($"[{GameManager.currentPlayer.playerName}] 현재 점수: {score}점");

        // 났을 경우
        if (score >= 7 && score > GameManager.currentPlayer.lastGoScore)
        {
            Player opponent = (GameManager.currentPlayer == GameManager.humanPlayer) 
                ? GameManager.computerPlayer 
                : GameManager.humanPlayer;

            // ~박 판정
            EvaluatePenalty(GameManager.currentPlayer, opponent);

            // 금액 계산
            long estimatedMoney = Utils.CalculateFinalMoney(score, GameManager.currentPlayer, opponent, 500);

            bool isGo = false;

            // AI가 났을 경우
            if (GameManager.currentPlayer == GameManager.computerPlayer)
            {
                // TODO: AI 판단 로직 붙이기 전까지는 무조건 스톱
                yield return new WaitForSeconds(1.0f);
                isGo = false;
            }
            // 플레이어가 났을 경우
            else
            {
                // GoStop 팝업 표시
                yield return WaitForPopupResult
                (
                    callback => UIManager.Instance != null &&
                                UIManager.Instance.ShowGoStopPopup(estimatedMoney, callback),
                    decision => isGo = decision,
                    false
                );
            }

            // 고
            if (isGo)
            {
                GameManager.currentPlayer.goCount++;
                score++;
                GameManager.currentPlayer.currentScore = score;
                GameManager.currentPlayer.lastGoScore = score;

                // 3고 이상: 2배씩 증가
                if (GameManager.currentPlayer.goCount >= 3) 
                    GameManager.currentPlayer.DoubleMultiplier();
            }
            // 스톱
            else
            {
                GameManager.finalWinner = GameManager.currentPlayer;
                GameManager.finalAmount = estimatedMoney;

                GameManager.ChangeState(GameManager.StateGameOver);
                yield break;
            }
        }

        GameManager.ChangeState(GameManager.StateTurnEnd);
        yield return null;
    }

    /** ~박 판정 헬퍼 함수 **/
    private void EvaluatePenalty(Player winner, Player loser)
    {
        // 고박
        if (loser.goCount > 0) loser.isGobak = true;

        // 피박, 광박, 멍박 판정을 위한 데이터 수집
        int myPeeCount = winner.capturedCards.Sum(c => c.Type == CardType.Pee ? 1 : (c.Type == CardType.Ssangpee ? 2 : (c.Type == CardType.Threepee ? 3 : 0)));
        int oppPeeCount = loser.capturedCards.Sum(c => c.Type == CardType.Pee ? 1 : (c.Type == CardType.Ssangpee ? 2 : (c.Type == CardType.Threepee ? 3 : 0)));
        int myGwangCount = winner.capturedCards.Count(c => c.Type == CardType.Gwang);
        int oppGwangCount = loser.capturedCards.Count(c => c.Type == CardType.Gwang);
        int myYeolCount = winner.capturedCards.Count(c => c.Type == CardType.Yeolggeut);

        // 피박
        loser.isPeebak = (myPeeCount >= 10 && oppPeeCount <= 7);
        // 광박
        loser.isGwangbak = (myGwangCount >= 3 && oppGwangCount == 0);
        // 멍박
        winner.isMeongbak = (myYeolCount >= 7);
    }
}