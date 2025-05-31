using PPool.ChatSDK;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CreateRoomPopup : BaseUI
{
	[SerializeField] ChattingList chattingList;
	[SerializeField] TMP_InputField titleInput;
	[SerializeField] TMP_InputField inviteUserInput;
	[SerializeField] Button cancelBt;
    [SerializeField] Button createRoomBt;
    [SerializeField] Button addBt;
    [SerializeField] RectTransform content;
    [SerializeField] GameObject userButtonPrefab;

    [SerializeField] Button personalRoomTypeBt;
    [SerializeField] Button groupRoomTypeBt;

    private List<string> inviteUserIds = new List<string>();
    private List<GameObject> inviteUserItems = new List<GameObject>();
    private ChatRoomType roomType = ChatRoomType.GROUP;

    private Image personalRoomTypeImage;
    private Image groupRoomTypeImage;

    public override void Init()
    {
        base.Init();

        ClearUserButtons();

        titleInput.text = "";
        inviteUserInput.text = "";

        cancelBt.onClick.AddListener(OnCancelClicked);
        createRoomBt.onClick.AddListener(OnCreateRoomClicked);
        addBt.onClick.AddListener(OnAddClicked);

        personalRoomTypeBt.onClick.AddListener(OnPersonalRoomTypeClicked);
        groupRoomTypeBt.onClick.AddListener(OnGroupRoomTypeClicked);

        personalRoomTypeImage = personalRoomTypeBt.gameObject.GetComponent<Image>();
        groupRoomTypeImage = groupRoomTypeBt.gameObject.GetComponent<Image>();
    }

    public override void Deinit()
    {
        base.Deinit();

        cancelBt.onClick.RemoveListener(OnCancelClicked);
        createRoomBt.onClick.RemoveAllListeners();
        addBt.onClick.RemoveListener(OnAddClicked);

        personalRoomTypeBt.onClick.RemoveListener(OnPersonalRoomTypeClicked);
        groupRoomTypeBt.onClick.RemoveListener(OnGroupRoomTypeClicked);
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

    private void OnCreateRoomClicked()
    {
        Debug.Log("@@@ [Unity-Sample] ChattingPopup OnCreateRoomClicked");

        string title = titleInput.text;

		if (string.IsNullOrEmpty(title))
            return;

		ChatRoomOption option = new ChatRoomOption(inviteUserIds, title, "", roomType);

		ChatSDK.CreateRoom(option, (result) =>
        {
            if (result.IsSuccess)
            {
                ChatData.Instance.AddRoom(result.Value);
                chattingList.FetchRooms();
                this.Hide();

                chattingList.EnterRoom(result.Value.Id, result.Value.Title);
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

    private void OnPersonalRoomTypeClicked()
    {
        roomType = ChatRoomType.PERSONAL;

        personalRoomTypeImage.color = new Color(0, 1, 0, 1);
        groupRoomTypeImage.color = new Color(1, 1, 1, 0.5f);
    }

    private void OnGroupRoomTypeClicked()
    {
        roomType = ChatRoomType.GROUP;

        personalRoomTypeImage.color = new Color(1, 1, 1, 0.5f);
        groupRoomTypeImage.color = new Color(0, 1, 0, 1);
    }
}
