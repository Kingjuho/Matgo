using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("선택 팝업 UI")]
    public GameObject choicePopupPanel;
    public Image choice1_Image;
    public Image choice2_Image;
    public Button choice1_Button;
    public Button choice2_Button;

    [Header("흔들기 팝업 UI")]
    public GameObject shakePopupPanel;
    public Image shake1_Image;
    public Image shake2_Image;
    public Image shake3_Image;

    // 누가 결과를 기다리고 있는지 기억할 콜백 함수
    private Action<Card> _onChoiceMadeCallback;
    private List<Card> _currentOptions;

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
    public void ShowChoicePopup(List<Card> options, Action<Card> callback)
    {
        _currentOptions = options;
        _onChoiceMadeCallback = callback;

        // 화투패 이미지 뒤집어 씌우기
        choice1_Image.sprite = options[0].frontSprite;
        choice2_Image.sprite = options[1].frontSprite;

        // 버튼 리스너 재설정
        choice1_Button.onClick.RemoveAllListeners();
        choice2_Button.onClick.RemoveAllListeners();
        choice1_Button.onClick.AddListener(() => OnOptionClicked(0));
        choice2_Button.onClick.AddListener(() => OnOptionClicked(1));

        choicePopupPanel.SetActive(true);
    }
    /** 2장 중 1장 선택 팝업 버튼 클릭 이벤트 **/
    private void OnOptionClicked(int index)
    {
        choicePopupPanel.SetActive(false);
        // 선택한 카드를 넘겨주며 콜백
        _onChoiceMadeCallback?.Invoke(_currentOptions[index]);
    }

    /** 흔들기 팝업 **/
    public void ShowShakePopup(List<Card> options)
    {
        // 화투패 이미지 뒤집어 씌우기
        shake1_Image.sprite = options[0].frontSprite;
        shake2_Image.sprite = options[1].frontSprite;
        shake3_Image.sprite = options[2].frontSprite;

        shakePopupPanel.SetActive(true);
    }
    /** 흔들기 팝업 닫기 **/
    public void HideShakePopup() => shakePopupPanel.SetActive(false);


    #region 디버깅용

    [Header("치트/디버그 팝업 UI")]
    public GameObject cheatPopupPanel;
    private Action<int> _onCheatSelectedCallback;

    /** 치트 팝업 띄우기 **/
    public void ShowCheatPopup(Action<int> callback)
    {
        _onCheatSelectedCallback = callback;
        cheatPopupPanel.SetActive(true);
    }
    /** 치트 팝업 버튼 클릭 이벤트 **/
    public void OnCheatButtonClicked(int monthValue)
    {
        cheatPopupPanel.SetActive(false);
        _onCheatSelectedCallback?.Invoke(monthValue);
    }

    #endregion
}
