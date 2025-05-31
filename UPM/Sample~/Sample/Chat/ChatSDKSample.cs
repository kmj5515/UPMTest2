using MiniJSON;
using PPool.ChatSDK;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatSDKSample : MonoBehaviour
{
	[SerializeField] Button btnLogin;

	[SerializeField] ChattingList chattingList;

	StoveLogin stoveLogin = new StoveLogin();

	// Start is called before the first frame update
	void Start()
    {
		ChatData.Instance.Init();

		string target = "stove_bubblyz";
		ChatSDK.Initialize(ChatConfig.Dev(target));
	}

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Login()
	{
		stoveLogin.Login((isSuccess, id, token) =>
		{
			if (isSuccess)
			{
				ChatData.Instance.UserId = id;
				ChatData.Instance.Token = token;

				ChatCredential credential = new ChatCredential(id, token);
				ChatSDK.Connect(credential, (result) =>
				{
					if(result.IsSuccess)
					{
						btnLogin.gameObject.SetActive(false);
						chattingList.Init();
						chattingList.Show();
					}
					else
					{
						Debug.Log($"Connect Fail {result.Error?.Message}");
					}
				});	
			}
			else
			{
				Debug.Log($"Stove Login Fail");
			}
		});
	}

	public void Logout()
	{
		ChatSDK.Disconnect();
        stoveLogin.Logout();
		ChatSDK.Logout();

		btnLogin.gameObject.SetActive(true);
	}
}
