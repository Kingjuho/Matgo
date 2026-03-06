using System.Collections;
using System.Linq;
using UnityEngine;

public class CheckPresidentState : GameStateBase
{
    public CheckPresidentState(GameManager manager) : base(manager) { }

    public override IEnumerator Execute()
    {
        // 총통 체크를 위한 데이터
        var playerChongtongGroup = GameManager.humanPlayer.handCards
        .GroupBy(c => c.Month)
        .FirstOrDefault(g => g.Count() == 4);

        var aiChongtongGroup = GameManager.computerPlayer.handCards
            .GroupBy(c => c.Month)
            .FirstOrDefault(g => g.Count() == 4);

        bool isPlayerChongtong = playerChongtongGroup != null;
        bool isAIChongtong = aiChongtongGroup != null;
        bool isTableChongtong = GameManager.CardDealer.TableCards.Values.Any(list => list.Count == 4);

        // 바닥 총통, 동시 총통 -> 나가리
        if (isTableChongtong || (isPlayerChongtong && isAIChongtong))
        {
            GameManager.isNagari = true;
            GameManager.ChangeState(GameManager.StateGameOver);
            yield break;
        }

        // 단일 총통
        Player chongtongPlayer = null;
        if (isPlayerChongtong) chongtongPlayer = GameManager.humanPlayer;
        else if (isAIChongtong) chongtongPlayer = GameManager.computerPlayer;

        if (chongtongPlayer != null)
        {
            // 게임 지속 여부 판정
            bool shouldStop = true;
            bool isDecisionMade = false;

            // AI 총통
            if (chongtongPlayer == GameManager.computerPlayer)
            {
                // AI는 일단 즉시 승리
                shouldStop = true;
                isDecisionMade = true;
            }
            // 플레이어 총통
            else
            {
                UIManager.Instance?.ShowPresidentPopup(playerChongtongGroup.ToList(), (decision) =>
                {
                    shouldStop = decision;
                    isDecisionMade = true;
                });
            }

            yield return new WaitUntil(() => isDecisionMade);

            if (shouldStop)
            {
                // 10점으로 즉시 승리
                GameManager.finalWinner = chongtongPlayer;

                // 총통은 기본 10점에 판 배당 곱함
                // 흔들기, 뻑 등은 없으므로 다이렉트 계산
                long baseStake = 500;
                GameManager.finalAmount = 10 * baseStake * GameManager.gameMultiplier;

                GameManager.isNagari = false;
                GameManager.ChangeState(GameManager.StateGameOver);
                yield break;
            }
        }

        // TODO: 선 결정해서 선한테 넘겨줘야 함
        GameManager.ChangeState(GameManager.StatePlayerTurn);
    }
}
