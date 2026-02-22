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
    public HumanPlayer humanPlayer;
    public ComputerPlayer computerPlayer;

    public GameState currentState;          // 현재 상태
    private bool _isPlayerTurn = true;      // 플레이어 턴 여부

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
                break;
            case GameState.PlayHandCard:
                StartCoroutine(PlayHandCardRoutine());
                break;
            case GameState.FlipDeckCard:
                break;
            case GameState.ResolveMatch:
                break;
            case GameState.CheckScore:
                break;
            case GameState.TurnEnd:
                break;
            case GameState.GameOver:
                break;
        }
    }

    /** 초기화 루틴 **/
    private IEnumerator InitRoutine()
    {
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
        humanPlayer.StartTurn();

        // 카드 선택까지 대기
        yield return new WaitUntil(() => humanPlayer.hasPlayed);

        ChangeState(GameState.PlayHandCard);
    }

    ///** 공통: 손패 처리 루틴 **/
    private IEnumerator PlayHandCardRoutine()
    {
        Debug.Log("PlayHandCard 시작");

        // 유저가 선택한 카드를 손패에서 제거
        Card playerCard = humanPlayer.selectedCard;
        humanPlayer.handCards.Remove(playerCard);

        // 손패가 빠졌으니 재정렬
        CardDealer.RearrangeHand(humanPlayer, CardDealer.playerHandAnchors);

        // 레이어 확인
        int orderInLayer;
        Vector3 targetPos = CardDealer.CalculateTablePosition(playerCard, out orderInLayer);

        // 렌더링 순서 적용(바닥에 깔린 애들보다 위로 오게)
        UnityEngine.Rendering.SortingGroup sg = playerCard.GetComponent<UnityEngine.Rendering.SortingGroup>();
        if (sg != null)
        {
            sg.sortingLayerName = "TableCards";
            sg.sortingOrder = orderInLayer;
        }

        // 애니메이션 재생
        Vector3 baseScale = playerCard.transform.localScale;
        playerCard.transform.DOScale(baseScale * 2.0f, 0.1f).OnComplete(() =>
        {
            playerCard.transform.DOScale(baseScale, 0.15f);
        });

        // 애니메이션 재생 (이동)
        playerCard.transform.DOMove(targetPos, 0.15f).SetEase(Ease.OutQuad);
        playerCard.transform.DORotateQuaternion(Quaternion.identity, 0.15f);

        // 애니메이션 종료까지 대기
        yield return new WaitForSeconds(0.5f);

        ChangeState(GameState.FlipDeckCard);
    }

    ///** 공통: 덱에서 화투를 1장 뽑은 후 처리 루틴 **/
    //private IEnumerator FlipDeckCardRoutine()
    //{
    //    ChangeState(GameState.ResolveMatch);
    //}

    ///** 공통: 판정 루틴 (쪽, 따닥, 뻑 등) **/
    //private IEnumerator ResolveMatchRoutine()
    //{
    //    ChangeState(GameState.CheckScore);
    //}
}
