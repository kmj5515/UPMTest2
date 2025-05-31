using PPool.ChatSDK;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InvitePopup : BaseUI
{
	[SerializeField] TMP_InputField inviteUserInput;
	[SerializeField] Button cancelBt;
    [SerializeField] Button inviteBt;
    [SerializeField] Button addBt;
    [SerializeField] RectTransform content;
    [SerializeField] GameObject userButtonPrefab;

    private List<string> inviteUserIds = new List<string>();
    private List<GameObject> inviteUserItems = new List<GameObject>();

    public override void Init()
    {
        base.Init();

        ClearUserButtons();

        inviteUserInput.text = "";

        cancelBt.onClick.AddListener(OnCancelClicked);
        inviteBt.onClick.AddListener(OnInviteUserClicked);
        addBt.onClick.AddListener(OnAddClicked);
    }

    public override void Deinit()
    {
        base.Deinit();

        cancelBt.onClick.RemoveListener(OnCancelClicked);
        inviteBt.onClick.RemoveListener(OnInviteUserClicked);
        addBt.onClick.RemoveListener(OnAddClicked);
    }

    public override void Show()
    {
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();

        Deinit();
    }

    private void OnCancelClicked()
    {
        Debug.Log("@@@ [Unity-Sample] ChattingPopup OnCancelClicked");
        
        this.Hide();
    }

    private void OnInviteUserClicked()
    {
        Debug.Log("@@@ [Unity-Sample] ChattingPopup OnInviteUserClicked");

        if (inviteUserIds.Count == 0)
        {
            SSTools.ShowMessage("Please add users to invite", SSTools.Position.bottom, SSTools.Time.oneSecond);
            return;
        }

		ChatSDK.Invite(ChatData.Instance.RoomId, inviteUserIds, (result) =>
        {
            if (result.IsSuccess)
            {
                Debug.Log($"@@@ [Unity-Sample] Invite success");
                this.Hide();
            }
            else
            {
                SSTools.ShowMessage("Invite failed", SSTools.Position.bottom, SSTools.Time.oneSecond);
            }
        });
    }

    private void OnAddClicked()
    {
        Debug.Log("@@@ [Unity-Sample] ChattingPopup OnAddClicked");

        var userInput = inviteUserInput.text;

        if (!string.IsNullOrEmpty(userInput))
        {
            inviteUserIds.Add(userInput);
            CreateUserButton(userInput);

            inviteUserInput.text = "";
        }
    }

    private void CreateUserButton(string userId)
    {
        GameObject itemObject = Instantiate(userButtonPrefab);
        inviteUserItems.Add(itemObject);
        
        InviteItem inviteItem = itemObject.GetComponent<InviteItem>();
        inviteItem.userName.text = userId;
        inviteItem.deleteBt.onClick.AddListener(() => {
            inviteUserIds.Remove(userId);
            inviteUserItems.Remove(itemObject);
            Destroy(itemObject);
        });

        itemObject.transform.SetParent(content, false);
        itemObject.SetActive(true);
    }

    private void ClearUserButtons()
    {
        foreach (var item in inviteUserItems)
        {
            Destroy(item);
        }

        inviteUserIds.Clear();
        inviteUserItems.Clear();
    }
}
