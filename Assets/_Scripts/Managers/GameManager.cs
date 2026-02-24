using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 싱글톤
    public static GameManager Instance { get; private set; }

    // 컴포넌트
    public CardDealer CardDealer;

    [Header("플레이어 객체")]
    public HumanPlayer humanPlayer;         // 유저
    public ComputerPlayer computerPlayer;   // AI
    Player currentPlayer;                   // 현재 턴을 진행한 플레이어

    // 해당 턴 판정용 데이터
    Card lastPlayerCard;     // 이번 턴에 손에서 낸 카드
    Card lastDeckCard;       // 이번 턴에 덱에서 깐 카드

    // 선택 대기 상태
    bool isChoosingCard = false;            // 선택 모드 활성화 여부
    Card selectedChoiceCard = null;         // 최종 선택 카드

    [Header("현재 상태")]
    public GameState currentState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ChangeState(GameState.Init);
    }

    /** FSM **/
    public void ChangeState(GameState newState)
    {
        currentState = newState;
        switch (currentState)
        {
            case GameState.Init:
                StartCoroutine(InitRoutine());
                break;
            case GameState.CheckPresident:
                ChangeState(GameState.PlayerTurn);
                break;
            case GameState.PlayerTurn:
                StartCoroutine(PlayerTurnRoutine());
                break;
            case GameState.AITurn:
                // TODO: AI 로직 개발 후 추가
                ChangeState(GameState.PlayerTurn);
                break;
            case GameState.PlayHandCard:
                StartCoroutine(PlayHandCardRoutine());
                break;
            case GameState.FlipDeckCard:
                StartCoroutine(FlipDeckCardRoutine());
                break;
            case GameState.ResolveMatch:
                StartCoroutine(ResolveMatchRoutine());
                break;
            case GameState.CheckScore:
                ChangeState(GameState.TurnEnd);
                break;
            case GameState.TurnEnd:
                StartCoroutine(TurnEndRoutine());
                break;
            case GameState.GameOver:
                break;
        }
    }

    /** 초기화 루틴 **/
    private IEnumerator InitRoutine()
    {
        //// 테스트
        //for(int i = 0; i < 10; i++)
        //{
        //    Card card = CardDealer.deck.Draw();
        //    card.FlipInstant(true);
        //    computerPlayer.CaptureCard(card);
        //}
        //computerPlayer.OrganizeCapturedCards();

        // 패 분배
        yield return StartCoroutine(CardDealer.DistributeCardsSequence());

        // 손패 정렬
        humanPlayer.SortHandCards();
        CardDealer.RearrangeHand(humanPlayer, CardDealer.playerHandAnchors);

        // 애니메이션 재생 동안 대기
        yield return new WaitForSeconds(0.4f);

        ChangeState(GameState.CheckPresident);
    }

    /** 플레이어 턴 루틴 **/
    private IEnumerator PlayerTurnRoutine()
    {
        Debug.Log("PlayerTurn 시작");

        // 플레이어 초기화
        currentPlayer = humanPlayer;
        currentPlayer.StartTurn();

        // 카드 선택까지 대기
        yield return new WaitUntil(() => currentPlayer.hasPlayed);

        ChangeState(GameState.PlayHandCard);
    }

    /** 공통: 손패 처리 루틴 **/
    private IEnumerator PlayHandCardRoutine()
    {
        Debug.Log("PlayHandCard 시작");

        // 유저가 선택한 카드를 손패에서 제거
        lastPlayerCard = currentPlayer.selectedCard;
        currentPlayer.handCards.Remove(lastPlayerCard);

        // 손패 재정렬
        CardDealer.RearrangeHand(currentPlayer, CardDealer.playerHandAnchors);

        // 바닥패 목적지 탐색
        int orderInLayer;
        Vector3 targetPos = CardDealer.CalculateTablePosition(lastPlayerCard, out orderInLayer);

        // 애니메이션 재생
        yield return StartCoroutine(AnimationManager.Instance.PlayDropCardToTable(lastPlayerCard, targetPos, orderInLayer, true));

        ChangeState(GameState.FlipDeckCard);
    }

    /** 공통: 덱에서 화투를 1장 뽑은 후 처리 루틴 **/
    private IEnumerator FlipDeckCardRoutine()
    {
        Debug.Log("FlipDeckCard 시작");

        // 덱에서 1장 드로우
        lastDeckCard = CardDealer.deck.Draw();
        // 덱이 텅 비었다면 바로 판정 시작
        if (lastDeckCard == null)
        {
            ChangeState(GameState.ResolveMatch);
            yield break;
        }

        // 잠시 HandCards로 설정
        lastDeckCard.SetSortingOrder("HandCards", 100);

        // 카드의 출발 위치를 덱 앵커로 세팅
        lastDeckCard.transform.position = CardDealer.deckAnchor.position;
        lastDeckCard.transform.rotation = CardDealer.deckAnchor.rotation;

        // 카드를 화면 중앙으로 살짝 띄우면서 앞면으로 뒤집음
        Vector3 flipPos = CardDealer.deckAnchor.position + new Vector3(1.5f, 0.5f, 0);
        lastDeckCard.transform.DOMove(flipPos, 0.3f).SetEase(Ease.OutBack);
        lastDeckCard.Flip(true);

        // 대기
        yield return new WaitForSeconds(0.5f);

        // 바닥패 목적지 탐색
        int orderInLayer;
        Vector3 targetPos = CardDealer.CalculateTablePosition(lastDeckCard, out orderInLayer);

        // 애니메이션 재생
        yield return StartCoroutine(AnimationManager.Instance.PlayDropCardToTable(lastDeckCard, targetPos, orderInLayer, false));


        ChangeState(GameState.ResolveMatch);
    }

    /** 공통: 판정 루틴 (쪽, 따닥, 뻑 등) **/
    private IEnumerator ResolveMatchRoutine()
    {
        Debug.Log("ResolveMatch 시작");

        CardMonth playedMonth = lastPlayerCard.Month;
        CardMonth deckMonth = lastDeckCard.Month;
        var table = CardDealer.TableCards;

        // 낸 패와 덱에서 뽑은 패가 같은 월인 경우
        if (playedMonth == deckMonth)
        {
            // 바닥패에 같은 월이 몇 개 있는 지 확인
            int totalCount = table[playedMonth].Count;
            // 쪽
            if (totalCount == 2)
            {
                // 바닥패에서 제거
                List<Card> cardsToCapture = new List<Card>(table[playedMonth]);
                table[playedMonth].Clear();

                // 해당 턴의 유저에게 삽입
                foreach (Card c in cardsToCapture) currentPlayer.CaptureCard(c);

                // 피 1장 뺏기
                yield return StartCoroutine(StealOpponentPeeRoutine(1));
            }
            // 뻑
            else if (totalCount == 3)
            {
                // TODO: 뻑
            }
            // 따닥
            else if (totalCount == 4)
            {
                // 바닥패에서 제거
                List<Card> cardsToCapture = new List<Card>(table[playedMonth]);
                table[playedMonth].Clear();

                // 해당 턴의 유저에게 삽입
                foreach (Card c in cardsToCapture) currentPlayer.CaptureCard(c);

                // 피 1장 뺏기
                yield return StartCoroutine(StealOpponentPeeRoutine(1));
            }
        }
        // 낸 패와 덱에서 뽑은 패가 다른 월인 경우
        else
        {
            // 낸 패 판정
            int playedCount = table[playedMonth].Count;
            // 단순 획득
            if (playedCount == 2)
            {
                // 바닥패에서 제거
                List<Card> cardsToCapture = new List<Card>(table[playedMonth]);
                table[playedMonth].Clear();

                // 해당 턴의 유저에게 삽입
                foreach (Card c in cardsToCapture) currentPlayer.CaptureCard(c);
            }
            // 1장 선택하여 획득
            else if (playedCount == 3)
            {
                yield return StartCoroutine(HandleChoiceRoutine(playedMonth, lastPlayerCard));
            }
            else if (playedCount == 4)
            {
                // TODO: 뻑 먹기 or 폭탄(전부 획득 + 1장 뺏기, 자뻑은 2장 뺏기)
            }

            // 덱에서 뽑은 패 판정
            int deckCount = table[deckMonth].Count;
            // 단순 획득
            if (deckCount == 2)
            {
                // 바닥패에서 제거
                List<Card> cardsToCapture = new List<Card>(table[deckMonth]);
                table[deckMonth].Clear();

                // 해당 턴의 유저에게 삽입
                foreach (Card c in cardsToCapture) currentPlayer.CaptureCard(c);
            }
            // 1장 선택하여 획득
            else if (deckCount == 3)
            {
                yield return StartCoroutine(HandleChoiceRoutine(deckMonth, lastDeckCard));
            }
            else if (deckCount == 4)
            {
                // TODO: 뻑 먹기(전부 획득 + 1장 뺏기, 자뻑은 2장 뺏기)
            }

            // 싹쓸이 판정
            bool hasCapturedAnything = (playedCount == 2 || deckCount == 2);
            if (CardDealer.GetTotalTableCardCount() == 0)
                yield return StartCoroutine(StealOpponentPeeRoutine(1));
        }

        // 해당 턴의 유저가 획득한 패 정렬
        currentPlayer.OrganizeCapturedCards();
        yield return new WaitForSeconds(0.4f);

        // 판정 시간
        yield return new WaitForSeconds(1f);

        ChangeState(GameState.CheckScore);
    }

    /** 공통: 턴 종료 루틴 **/
    private IEnumerator TurnEndRoutine()
    {
        Debug.Log("TurnEnd 시작");

        // 반대쪽으로 턴 넘기기
        ChangeState((currentPlayer == humanPlayer) ? GameState.AITurn : GameState.PlayerTurn);
        yield return null;
    }

    /** 2장 중 1장 선택 루틴 **/
    private IEnumerator HandleChoiceRoutine(CardMonth month, Card triggerCard)
    {
        var table = CardDealer.TableCards;
        List<Card> options = new List<Card>(table[month]);
        options.Remove(triggerCard);    // 플레이어가 낸 카드는 무조건 먹음

        // 상태 변경
        isChoosingCard = true;
        selectedChoiceCard = null;

        // 컴퓨터 턴엔 UI 띄우지 않음
        if (currentPlayer == computerPlayer)
        {
            yield return new WaitForSeconds(0.8f);
            // TODO: AI가 자신에게 유리한 패를 고르도록
            selectedChoiceCard = options[0];
            isChoosingCard = false;
        }
        else
        {
            // UIManager 호출, 콜백
            UIManager.Instance.ShowChoicePopup(options, (chosenCard) =>
            {
                selectedChoiceCard = chosenCard;
                isChoosingCard = false;
            });

            // 선택될 때까지 대기
            yield return new WaitUntil(() => !isChoosingCard);
        }

        // 낸 패 획득
        currentPlayer.CaptureCard(triggerCard);
        table[month].Remove(triggerCard);

        // 선택한 패 획득
        currentPlayer.CaptureCard(selectedChoiceCard);
        table[month].Remove(selectedChoiceCard);
    }

    /** 상대의 피를 뺏는 루틴 **/
    private IEnumerator StealOpponentPeeRoutine(int count)
    {
        Player opponent = (currentPlayer == humanPlayer) ? computerPlayer : humanPlayer;

        for (int i = 0; i < count; i++)
        {
            // 피가 없으면 continue
            Card stolenCard = opponent.LosePee();
            if (stolenCard == null) continue;

            // 현재 유저의 피에 추가
            currentPlayer.CaptureCard(stolenCard);

            // 현재 유저의 피 앵커를 타겟으로 설정 및 레이어 정렬
            Vector3 targetPos = currentPlayer.peeAnchor.position;
            stolenCard.SetSortingOrder("TableCards", 100);

            // 애니메이션 재생
            AnimationManager.Instance.MoveCard(stolenCard, targetPos, Quaternion.identity, 0.4f);
            yield return new WaitForSeconds(0.4f);
        }

        // 먹은 패 재정렬
        // currentPlayer.OrganizeCapturedCards();
    }
}
