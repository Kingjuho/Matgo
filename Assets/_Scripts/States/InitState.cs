using System.Collections;
using UnityEngine;

public class InitState : GameStateBase
{
    public InitState(GameManager manager) : base(manager) { }

    public override IEnumerator Execute()
    {
        // 디버깅용 치트 활성화
        yield return Cheat();

        // 패 분배
        yield return GameManager.CardDealer.DistributeCardsSequence();

        // 데이터 초기화
        GameManager.bbuckRecords.Clear();

        // 손패 정렬
        GameManager.humanPlayer.SortHandCards();
        GameManager.CardDealer.RearrangeHand(GameManager.humanPlayer, GameManager.CardDealer.playerHandAnchors);

        // 애니메이션 재생 동안 대기
        yield return new WaitForSeconds(0.4f);

        // 상태 전이
        GameManager.ChangeState(GameManager.StateCheckPresident);
    }

    #region 디버깅용

    private IEnumerator Cheat()
    {
        int cheatMonth = -1;
        bool isCheatSelected = false;

        UIManager.Instance?.ShowCheatPopup((selectedMonth) =>
        {
            cheatMonth = selectedMonth;
            isCheatSelected = true;
        });

        yield return new WaitUntil(() => isCheatSelected);

        if (cheatMonth >= 1 && cheatMonth <= 12)
            GameManager.CardDealer.deck.StackDeckForPlayer((CardMonth)cheatMonth);
    }

    #endregion
}
