using System;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    [Header("레퍼런스")]
    [SerializeField] GameObject cardPrefab; // 카드 프리팹
    [SerializeField] Transform deckParent;  // 하이어라키 정리용

    // 덱
    private List<Card> _cards = new List<Card>();

    void Start()
    {
        GenerateDeck();
        Shuffle();
    }

    /** 덱 생성 함수 **/
    void GenerateDeck()
    {
        // Resources/Cards 폴더의 모든 스프라이트 로드
        Sprite[] allSprites = Resources.LoadAll<Sprite>("Cards");

        foreach (Sprite sprite in allSprites)
        {
            // 파일 이름 형식: 1_Gwang, 5_Pee_1
            string[] parts = sprite.name.Split('_');
            
            // 에러 처리
            if (parts.Length < 2)
            {
                Debug.LogWarning($"[Deck] 잘못된 파일명 감지: {sprite.name}");
                continue;
            }

            // 월(Month) 파싱 (1 -> Jan)
            int monthInt = int.Parse(parts[0]);
            CardMonth month = (CardMonth)monthInt;

            // 타입(Type) 파싱 (Gwang -> CardType.Gwang)
            string typeStr = parts[1];
            CardType type = (CardType)Enum.Parse(typeof(CardType), typeStr);

            // 특수 타입 판별
            SpecialFeature feature = DetermineFeature(month, type);

            // 카드 생성 및 초기화
            CreateCardObject(month, type, feature, sprite);
        }

        Debug.Log($"[Deck] 총 {_cards.Count}장의 화투패가 생성되었습니다.");
    }

    /** 특수 타입(고도리, 홍단 등) 판별 함수 **/
    SpecialFeature DetermineFeature(CardMonth month, CardType type)
    {
        // 광/열끗/띠/피 조합에 따른 속성 정의
        if (type == CardType.Ddee)
        {
            if (month == CardMonth.Jan || month == CardMonth.Feb || month == CardMonth.Mar) return SpecialFeature.HongDan;
            if (month == CardMonth.Apr || month == CardMonth.May || month == CardMonth.Jul) return SpecialFeature.ChoDan;
            if (month == CardMonth.Jun || month == CardMonth.Sep || month == CardMonth.Oct) return SpecialFeature.ChungDan;
        }
        else if (type == CardType.Yeolggeut)
        {
            // 2, 4, 8월 열끗 = 고도리
            if (month == CardMonth.Feb || month == CardMonth.Apr || month == CardMonth.Aug) return SpecialFeature.Godori;
        }

        // 12월 = 비
        if (month == CardMonth.Dec) return SpecialFeature.Bee;

        return SpecialFeature.None;
    }

    /** 작성 완료된 카드를 Instantiate하는 함수 **/
    void CreateCardObject(CardMonth month, CardType type, SpecialFeature feature, Sprite sprite)
    {
        // 오브젝트 생성 및 하이어라키 정리
        GameObject go = Instantiate(cardPrefab, deckParent, false);
        go.name = $"{(int)month}_{type}";

        // 카드 초기화, 뒷면
        Card card = go.GetComponent<Card>();
        card.Initialize(month, type, sprite, feature);
        card.FlipInstant(false);

        _cards.Add(card);
    }

    /** 덱 셔플 함수 **/
    void Shuffle()
    {
        // 덱 개수
        int n = _cards.Count;

        while (n > 1)
        {
            n--;

            // 0부터 n 사이의 무작위 인덱스 선택
            int k = UnityEngine.Random.Range(0, n + 1);

            // 스왑
            (_cards[k], _cards[n]) = (_cards[n], _cards[k]);

            // 하이어라키 순서 변경
            _cards[n].transform.SetSiblingIndex(k);
        }
    }
}
