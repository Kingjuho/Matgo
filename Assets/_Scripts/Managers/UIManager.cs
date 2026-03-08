using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("선택 팝업 UI")]
    // UI
    public GameObject choicePopupPanel;
    public Image choice1_Image;
    public Image choice2_Image;
    public Button choice1_Button;
    public Button choice2_Button;
    // 콜백 함수
    private Action<Card> _onChoiceSelectedCallback;
    private List<Card> _currentOptions;

    [Header("흔들기 팝업 UI")]
    // UI
    public GameObject shakePopupPanel;
    public Image shake1_Image;
    public Image shake2_Image;
    public Image shake3_Image;

    [Header("고/스톱 팝업 UI")]
    // UI
    public GameObject goStopPopupPanel;
    public TextMeshProUGUI txtStopMoney;
    // 콜백 함수
    private Action<bool> _onGoStopMadeCallback;

    [Header("국열끗 팝업 UI")]
    // UI
    public GameObject gukYeolggeutPopupPanel;
    // 콜백 함수
    private Action<bool> _onGukYeolggeutMadeCallback;

    [Header("총통 팝업 UI")]
    // UI
    public GameObject presidentPopupPanel;
    public Image presidentOption0_Image;
    public Image presidentOption1_Image;
    public Image presidentOption2_Image;
    public Image presidentOption3_Image;
    // 콜백 함수
    private Action<bool> _onPresidentDecisionMadeCallback;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /** 2장 중 1장 선택 팝업 **/
    public bool ShowChoicePopup(List<Card> options, Action<Card> callback)
    {
        _onChoiceSelectedCallback = callback;
        _currentOptions = options;

        if (choicePopupPanel == null ||
            choice1_Image == null || choice2_Image == null ||
            choice1_Button == null || choice2_Button == null ||
            options == null || options.Count < 2)
        {
            return false;
        }

        choice1_Image.sprite = options[0].frontSprite;
        choice2_Image.sprite = options[1].frontSprite;

        choice1_Button.onClick.RemoveAllListeners();
        choice1_Button.onClick.AddListener(() => OnOptionClicked(0));

        choice2_Button.onClick.RemoveAllListeners();
        choice2_Button.onClick.AddListener(() => OnOptionClicked(1));

        choicePopupPanel.SetActive(true);
        return true;
    }
    /** 2장 중 1장 선택 팝업 버튼 클릭 이벤트 **/
    private void OnOptionClicked(int index)
    {
        choicePopupPanel?.SetActive(false);
        
        if (_currentOptions == null || index < 0 || index >= _currentOptions.Count)
            return;

        // 선택한 카드를 넘겨주며 콜백
        _onChoiceSelectedCallback?.Invoke(_currentOptions[index]);
    }


    /** 흔들기 팝업 **/
    public void ShowShakePopup(List<Card> options)
    {
        // 화투패 이미지 뒤집어 씌우기
        shake1_Image.sprite = options[0].frontSprite;
        shake2_Image.sprite = options[1].frontSprite;
        shake3_Image.sprite = options[2].frontSprite;

        shakePopupPanel?.SetActive(true);
    }
    /** 흔들기 팝업 닫기 **/
    public void HideShakePopup() => shakePopupPanel.SetActive(false);


    /** 고/스톱 팝업 **/
    public bool ShowGoStopPopup(long calculatedMoney, Action<bool> callback)
    {
        _onGoStopMadeCallback = callback;

        if (goStopPopupPanel == null || txtStopMoney == null)
            return false;

        txtStopMoney.text = Utils.FormatMoney(calculatedMoney);

        goStopPopupPanel.SetActive(true);
        return true;
    }
    /** 고/스톱 팝업 클릭 이벤트 **/
    public void OnGoOrStopClicked(bool isGo)
    {
        // true: 고, false: 스톱
        goStopPopupPanel?.SetActive(false);
        _onGoStopMadeCallback?.Invoke(isGo);
    }


    /** 국열끗 팝업 **/
    public bool ShowGukYeolggeutPopup(Action<bool> callback)
    {
        _onGukYeolggeutMadeCallback = callback;

        if (gukYeolggeutPopupPanel == null)
            return false;

        gukYeolggeutPopupPanel.SetActive(true);
        return true;
    }
    /** 국열끗 팝업 클릭 이벤트 **/
    public void OnGukYeolggeutButtonClicked(bool useAsSsangpee)
    {
        // true: 쌍피, false: 열끗
        gukYeolggeutPopupPanel?.SetActive(false);
        _onGukYeolggeutMadeCallback?.Invoke(useAsSsangpee);
    }


    /** 총통 팝업 **/
    public bool ShowPresidentPopup(List<Card> cards, Action<bool> callback)
    {
        _onPresidentDecisionMadeCallback = callback;

        if (presidentPopupPanel == null)
            return false;

        Image[] optionImages =
        {
        presidentOption0_Image,
        presidentOption1_Image,
        presidentOption2_Image,
        presidentOption3_Image
    };

        for (int i = 0; i < optionImages.Length; i++)
        {
            if (optionImages[i] == null) continue;

            bool hasCard = cards != null && i < cards.Count;
            optionImages[i].sprite = hasCard ? cards[i].frontSprite : null;
            optionImages[i].enabled = hasCard;
        }

        presidentPopupPanel.SetActive(true);
        return true;
    }
    /** 총통 팝업 클릭 이벤트 **/
    public void OnPresidentButtonClicked(bool shouldStop)
    {
        // true: 즉시 10점 승리, false: 정상 진행
        presidentPopupPanel?.SetActive(false);
        _onPresidentDecisionMadeCallback?.Invoke(shouldStop);
    }

    #region 디버깅용

    [Header("치트/디버그 팝업 UI")]
    public GameObject cheatPopupPanel;
    private Action<int> _onCheatSelectedCallback;

    /** 치트 팝업 띄우기 **/
    public bool ShowCheatPopup(Action<int> callback)
    {
        _onCheatSelectedCallback = callback;

        if (cheatPopupPanel == null)
            return false;

        cheatPopupPanel.SetActive(true);
        return true;
    }
    /** 치트 팝업 버튼 클릭 이벤트 **/
    public void OnCheatButtonClicked(int monthValue)
    {
        cheatPopupPanel?.SetActive(false);
        _onCheatSelectedCallback?.Invoke(monthValue);
    }

    #endregion
}
