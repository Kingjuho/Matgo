using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Player : MonoBehaviour
{
    [Header("기본 정보")]
    public string playerName;
    public long money = 500000;

    [Header("화투 목록")]
    public List<Card> handCards = new List<Card>();     // 들고 있는 패
    public List<Card> capturedCards = new List<Card>(); // 먹은 패

    [Header("게임 상태")]
    public int currentScore = 0;    // 현재 점수
    public int multiplier = 1;      // 배당
    public int goCount = 0;         // 고 횟수

    /** 카드 획득 **/
    public virtual void CaptureCard(Card card)
    {
        capturedCards.Add(card);
    }

    /** 배당 증가 **/
    public void addMultiplier()
    {
        // 변칙적인 배당 증가는 없으므로 2배로 고정
        multiplier *= 2;
        Debug.Log($"[{playerName}] 배당이 {multiplier}배로 증가");
    }

    /** 패 지우기 (턴 시작 시 등) **/
    public void RemoveHandCard(Card card)
    {
        if (handCards.Contains(card))
            handCards.Remove(card);
    }

    /** 패 정렬 **/
    public void SortHandCards()
    {
        handCards = handCards.OrderBy(card => (int)card.Month).ToList();
    }
}
