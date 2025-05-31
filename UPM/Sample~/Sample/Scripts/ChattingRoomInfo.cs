using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PPool.ChatSDK;

public class ChattingRoomInfo: MonoBehaviour
{
    // DATA
    public string roomId;
    public string roomName;
    public string roomLastMessage;

    // UI
    public TMP_Text roomNameText;
    public TMP_Text roomLastMessageText;
    public Image roomImage;

    // Profile Grid
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private Image[] profileImages = new Image[4];
    [SerializeField] private Sprite defaultProfileSprite;

    List<ChatUser> roomUsers;

	private void Start()
	{
		if (roomUsers != null && roomUsers.Count > 0)
		{
			int userCount = Mathf.Min(roomUsers.Count, 4);

			if (userCount == 1)
			{
				gridLayoutGroup.cellSize = new Vector2(250, 250);
				StartCoroutine(LoadProfileImage(0, roomUsers[0].ProfileUrl));
			}
			else if (userCount == 2)
			{
                gridLayoutGroup.cellSize = new Vector2(200, 200);
				StartCoroutine(LoadProfileImage(0, roomUsers[0].ProfileUrl));
				StartCoroutine(LoadProfileImage(2, roomUsers[1].ProfileUrl));
			}
			else
			{
                gridLayoutGroup.cellSize = new Vector2(100, 100);
				for (int i = 0; i < userCount; i++)
				{
					StartCoroutine(LoadProfileImage(i, roomUsers[i].ProfileUrl));
				}
			}
		}
	}

	public void Init(string roomId, string roomName, string roomLastMessage, List<ChatUser> users)
    {
        this.roomId = roomId;
        this.roomName = roomName;
        this.roomLastMessage = roomLastMessage;

        roomNameText.text = roomName;
        roomLastMessageText.text = roomLastMessage;

        gridLayoutGroup.cellSize = new Vector2(100, 100);
        for (int i = 0; i < profileImages.Length; i++)
        {
            if (profileImages[i] != null)
            {
                profileImages[i].gameObject.SetActive(false);
                profileImages[i].sprite = defaultProfileSprite;
            }
        }

        roomUsers = users;
	}

    public void UpdateRooom(ChatRoom room)
    {
		this.roomLastMessage = room.Preview;
		roomLastMessageText.text = room.Preview;
	}

    private IEnumerator LoadProfileImage(int index, string imageUrl)
    {
        if (index < 0 || index >= profileImages.Length || profileImages[index] == null)
            yield break;

        profileImages[index].gameObject.SetActive(true);

        if (string.IsNullOrEmpty(imageUrl))
        {
            profileImages[index].sprite = defaultProfileSprite;
            yield break;
        }

        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Texture2D texture = ((UnityEngine.Networking.DownloadHandlerTexture)www.downloadHandler).texture;
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                if (profileImages[index] != null)
                {
                    profileImages[index].sprite = sprite;
                }
            }
            else
            {
                Debug.LogError($"Failed to load profile image: {www.error}");
                if (profileImages[index] != null)
                {
                    profileImages[index].sprite = defaultProfileSprite;
                }
            }
        }
    }
}

   
