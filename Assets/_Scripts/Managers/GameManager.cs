using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [Header("게임 판정용 데이터")]
    public GameState currentState;
    public Dictionary<CardMonth, Player> bbuckRecords = new Dictionary<CardMonth, Player>();

    // 해당 턴 판정용 데이터
    Card lastPlayerCard;            // 이번 턴에 손에서 낸 카드
    Card lastDeckCard;              // 이번 턴에 덱에서 깐 카드
    bool isBombThisTurn = false;    // 이번 턴에 폭탄 터트렸는지 여부
    bool isShakeThisTurn = false;   // 이번 턴에 흔들었는지 여부

    // 선택 대기 상태
    bool isChoosingCard = false;            // 선택 모드 활성화 여부
    Card selectedChoiceCard = null;         // 최종 선택 카드


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
                StartCoroutine(CheckScoreRoutine());
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
        // 디버깅용 치트 활성화
        yield return StartCoroutine(Cheat());

        // 패 분배
        yield return StartCoroutine(CardDealer.DistributeCardsSequence());

        // 데이터 초기화
        bbuckRecords.Clear();

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

        lastPlayerCard = currentPlayer.selectedCard;

        // 더미 패 예외 처리
        if (lastPlayerCard.Type == CardType.Dummy)
        {
            // 소멸
            currentPlayer.handCards.Remove(lastPlayerCard);
            Destroy(lastPlayerCard.gameObject);
            lastPlayerCard = null;

            // 손패 재정렬
            currentPlayer.SortHandCards();
            Transform[] d_anchors = (currentPlayer == humanPlayer) ? CardDealer.playerHandAnchors : CardDealer.aiHandAnchors;
            CardDealer.RearrangeHand(currentPlayer, d_anchors);

            ChangeState(GameState.FlipDeckCard);
            yield break;
        }

        // 유저가 선택한 패가 폭탄이나 흔들기가 되는지 검사
        HintType hintType = currentPlayer.CheckSpecialMoveCondition(lastPlayerCard);
        isBombThisTurn = false;
        isShakeThisTurn = false;

        // 폭탄
        if (hintType == HintType.Bomb)
        {
            isBombThisTurn = true;
            currentPlayer.bombCount++;

            // 소지패에서 같은 월 3장을 모두 바닥패로 던짐
            CardMonth bombMonth = lastPlayerCard.Month;
            List<Card> bombCards = currentPlayer.handCards.FindAll(c => c.Month == bombMonth);
            foreach (Card card in bombCards)
            {
                // 유저가 선택한 카드를 손패에서 제거
                currentPlayer.handCards.Remove(card);

                // 바닥패 목적지 탐색
                int orderInLayer;
                Vector3 targetPos = CardDealer.CalculateTablePosition(card, out orderInLayer);

                // 애니메이션 재생
                StartCoroutine(AnimationManager.Instance?.PlayDropCardToTable(card, targetPos, orderInLayer, true));
            }

            // 더미 패 2장 추가
            for (int i = 0; i < 2; i++)
            {
                Card dummy = CardDealer.CreateDummyCard();
                currentPlayer.handCards.Add(dummy);
            }
        }
        else
        {
            // 흔들기
            if (hintType == HintType.Shake)
            {
                isShakeThisTurn = true;
                currentPlayer.shakeCount++;

                // 소지패에서 같은 월 3장 검색
                CardMonth shakeMonth = lastPlayerCard.Month;
                List<Card> shakeCards = currentPlayer.handCards.FindAll(c => c.Month == shakeMonth);
                
                // 2초간 흔들기 팝업 표시
                UIManager.Instance?.ShowShakePopup(shakeCards);
                yield return new WaitForSeconds(2.0f);
                UIManager.Instance?.HideShakePopup();
            }

            // 유저가 선택한 카드를 손패에서 제거
            currentPlayer.handCards.Remove(lastPlayerCard);

            // 바닥패 목적지 탐색
            int orderInLayer;
            Vector3 targetPos = CardDealer.CalculateTablePosition(lastPlayerCard, out orderInLayer);

            // 애니메이션 재생
            yield return StartCoroutine(AnimationManager.Instance?.PlayDropCardToTable(lastPlayerCard, targetPos, orderInLayer, true));
        }

        // 손패 재정렬
        currentPlayer.SortHandCards();
        Transform[] anchors = (currentPlayer == humanPlayer) ? CardDealer.playerHandAnchors : CardDealer.aiHandAnchors;
        CardDealer.RearrangeHand(currentPlayer, anchors);

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
        yield return StartCoroutine(AnimationManager.Instance?.PlayDropCardToTable(lastDeckCard, targetPos, orderInLayer, false));


        ChangeState(GameState.ResolveMatch);
    }

    /** 공통: 판정 루틴 (쪽, 따닥, 뻑 등) **/
    private IEnumerator ResolveMatchRoutine()
    {
        Debug.Log("ResolveMatch 시작");

        CardMonth playedMonth = lastPlayerCard?.Month ?? (CardMonth)0;
        CardMonth deckMonth = lastDeckCard.Month;

        // 더미 패
        if (lastPlayerCard == null)
        {
            yield return StartCoroutine(ProcessSingleMatchRoutine(deckMonth, lastDeckCard, false));
        }
        else
        {
            // 특수 판정 (낸 패 == 뒤집은 패)
            if (playedMonth == deckMonth)
            {
                yield return StartCoroutine(ProcessSpecialMatchRoutine(playedMonth));
            }
            // 일반 판정 (낸 패 != 뒤집은 패)
            else
            {
                // 낸 패 처리
                yield return StartCoroutine(ProcessSingleMatchRoutine(playedMonth, lastPlayerCard, true));
                // 덱 패 처리
                yield return StartCoroutine(ProcessSingleMatchRoutine(deckMonth, lastDeckCard, false));
            }
        }

        // 싹쓸 판정
        if (CardDealer.GetTotalTableCardCount() == 0)
            yield return StartCoroutine(StealOpponentPeeRoutine(1));

        // 해당 턴의 유저가 획득한 패 정렬
        currentPlayer.OrganizeCapturedCards();
        yield return new WaitForSeconds(0.4f);

        // 판정 시간
        yield return new WaitForSeconds(1f);

        ChangeState(GameState.CheckScore);
    }

    /** 공통: 점수 체크 루틴 **/
    private IEnumerator CheckScoreRoutine()
    {
        Debug.Log("CheckScore 시작");

        // 점수 계산
        int score = currentPlayer.CalculateScore();
        Debug.Log($"[{currentPlayer.playerName}] 현재 점수: {score}점");

        // 났을 경우
        if (score >= 7 && score > currentPlayer.lastGoScore)
        {
            // ~박 판정
            Player opponent = (currentPlayer == humanPlayer) ? computerPlayer : humanPlayer;
            EvaluatePenalty(currentPlayer, opponent);

            // 금액 계산
            long estimatedMoney = Utils.CalculateFinalMoney(score, currentPlayer, opponent, 500);

            bool isGo = false;
            bool isDecisionMade = false;

            // AI가 났을 경우
            if (currentPlayer == computerPlayer)
            {
                // TODO: AI가 현재 필드를 보고 판단하도록. 현재는 무조건 스톱
                yield return new WaitForSeconds(1.0f);
                isGo = false;
                isDecisionMade = true;
            }
            // 플레이어가 났을 경우
            else
            {
                // GoStop 팝업 표시
                UIManager.Instance?.ShowGoStopPopup(estimatedMoney, (decision) =>
                {
                    isGo = decision;
                    isDecisionMade = true;
                });
                yield return new WaitUntil(() => isDecisionMade);

                // 고/스톱
                if (isGo)
                {
                    currentPlayer.goCount++;
                    score++;
                    currentPlayer.lastGoScore = score;

                    // 3고 이상: 2배씩 증가
                    if (currentPlayer.goCount >= 3) currentPlayer.DoubleMultiplier();
                }
                else
                {
                    ChangeState(GameState.GameOver);
                    yield break;
                }
            }
        }

        ChangeState(GameState.TurnEnd);
        yield return null;
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
            UIManager.Instance?.ShowChoicePopup(options, (chosenCard) =>
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

        // 피가 없으면 continue
        List<Card> stolenCards = opponent.LosePees(count);
        if (stolenCards.Count == 0) yield break;

        foreach (Card stolenCard in stolenCards)
        {
            // 현재 유저의 피에 추가
            currentPlayer.CaptureCard(stolenCard);

            // 현재 유저의 피 앵커를 타겟으로 설정 및 레이어 정렬
            Vector3 targetPos = currentPlayer.peeAnchor.position;
            stolenCard.SetSortingOrder("TableCards", 100);

            // 애니메이션 재생
            AnimationManager.Instance?.MoveCard(stolenCard, targetPos, Quaternion.identity, 0.4f);
        }

        // 대기
        yield return new WaitForSeconds(0.4f);
    }

    /** 낸 패와 뒤집은 패가 같은 월일 경우 특수 판정 루틴 **/
    private IEnumerator ProcessSpecialMatchRoutine(CardMonth month)
    {
        int totalCount = CardDealer.TableCards[month].Count;
        // 쪽 or 따닥
        if (totalCount == 2 || totalCount == 4)
        {
            CaptureAllCardsOfMonth(month);
            yield return StartCoroutine(StealOpponentPeeRoutine(1));
        }
        // 뻑
        else if (totalCount == 3)
        {
            currentPlayer.BbuckCount++;

            // 첫뻑 확인
            if (currentPlayer.currentTurnCount == 1)
            {
                // TODO: 즉시 7점 상당의 돈을 뺏음
            }

            // 쓰리뻑 확인
            if (currentPlayer.BbuckCount >= 3)
            {
                // TODO: 즉시 7점으로 승리
            }

            // 뻑 장부에 기록(자뻑 판정을 위함)
            bbuckRecords[month] = currentPlayer;
        }
    }

    /** 낸 패와 뒤집은 패가 다른 월일 경우 일반 판정 루틴 **/
    private IEnumerator ProcessSingleMatchRoutine(CardMonth month, Card triggerCard, bool checkBomb = false)
    {
        int count = CardDealer.TableCards[month].Count;

        // 단순 획득
        if (count == 2)
        {
            CaptureAllCardsOfMonth(month);
        }
        // 1장 선택
        else if (count == 3)
        {
            yield return StartCoroutine(HandleChoiceRoutine(month, triggerCard));
        }
        // 뻑 먹기 or 폭탄
        else if (count == 4)
        {
            CaptureAllCardsOfMonth(month);

            int stealCount = 1;
            if (!checkBomb || !isBombThisTurn)
            {
                // 뻑 장부 검사
                if (bbuckRecords.ContainsKey(month))
                {
                    // 자뻑일 경우 2장
                    if (bbuckRecords[month] == currentPlayer) stealCount = 2;
                    bbuckRecords.Remove(month);
                }
            }

            // 피 뺏기
            yield return StartCoroutine(StealOpponentPeeRoutine(stealCount));
        }
    }

    /** 특정 월의 바닥패를 먹는 헬퍼 함수 **/
    private void CaptureAllCardsOfMonth(CardMonth month)
    {
        // 해당 월의 바닥패가 없으면 종료
        var table = CardDealer.TableCards;
        if (!table.ContainsKey(month) || table[month].Count == 0) return;

        // 리스트에 저장 후 해당 바닥패 청소
        List<Card> cardsToCapture = new List<Card>(table[month]);
        table[month].Clear();

        // 해당 플레이어에게 지급
        foreach (Card c in cardsToCapture) currentPlayer.CaptureCard(c);
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
            CardDealer.deck.StackDeckForPlayer((CardMonth)cheatMonth);
    }

    #endregion
}
