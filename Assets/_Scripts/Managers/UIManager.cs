using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("선택 팝업 UI")]
    public GameObject choicePopupPanel;
    // UI에 카드를 그려줄 이미지나 버튼 등 (에셋에 맞게 연결)
    public Image option1_Image;
    public Image option2_Image;
    public Button option1_Button;
    public Button option2_Button;

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
        option1_Image.sprite = options[0].frontSprite;
        option2_Image.sprite = options[1].frontSprite;

        // 버튼 리스너 재설정
        option1_Button.onClick.RemoveAllListeners();
        option2_Button.onClick.RemoveAllListeners();
        option1_Button.onClick.AddListener(() => OnOptionClicked(0));
        option2_Button.onClick.AddListener(() => OnOptionClicked(1));

        choicePopupPanel.SetActive(true);
    }
    /** 2장 중 1장 선택 팝업 버튼 클릭 이벤트 **/
    private void OnOptionClicked(int index)
    {
        choicePopupPanel.SetActive(false);
        // 선택한 카드를 넘겨주며 콜백
        _onChoiceMadeCallback?.Invoke(_currentOptions[index]);
    }
}
