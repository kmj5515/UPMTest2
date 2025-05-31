using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NoticePopup : BaseUI
{
    [SerializeField] TMP_Text title;
    [SerializeField] TMP_Text description;
    [SerializeField] Button button;

    private void Start()
    {
        button.onClick.AddListener(ButtonClicked);
    }

    public override void Init()
    {
        
    }

    public override void Deinit()
    {
        
    }

    public override void Show()
    {
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
    }

    public void SetInfo(string title = "", string description = "")
    {
        this.title.text = title;
        this.description.text = description;
    }

    private void ButtonClicked()
    {
        this.Hide();
    }

    private void OnDestroy()
    {
       button.onClick.RemoveListener(ButtonClicked);
    }
}
