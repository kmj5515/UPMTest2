using Stove.StoveSDK;
using System;
using System.Collections.Generic;
using UnityEngine;

public class StoveLogin
{
	const string PACKAGE_NAME = "com.stove.ppool.google.dev";
	const string VERSION = "1.12.0";
	const string ENVIRONMENT = "dev";

	public void Login(Action<bool, string, string> callback)
	{
		Initialize((result) =>
		{
			if (result)
			{
				AuthUILogin(callback);
			}
			else
			{
				callback?.Invoke(false, "", "");
			}
		});
	}

	public void TestLogin(Action<bool, string, string> callback) // 작업 완료후 삭제
	{
		Initialize((result) =>
		{
			if (result)
			{
				AuthLogin(callback);
			}
			else
			{
				callback?.Invoke(false, "", "");
			}
		});
	}

	void Initialize(Action<bool> callback)
	{
		BuildConfiguration buildConfiguration = new BuildConfiguration(PACKAGE_NAME, VERSION, ENVIRONMENT);
		Constants.SetBuildConfiguration(buildConfiguration);

		Auth.Initialize(result =>
		{
			if (result.IsSuccessful)
			{
				Debug.Log("------- [Stove-Sample] Auth.Initialize : " + result + " -------");

				callback?.Invoke(true);
			}
			else
			{
				callback?.Invoke(false);
			}
		});
	}

	void AuthUILogin(Action<bool, string, string> callback)
	{
		AuthUI.Providers = new List<Provider>() { new EmailProvider() };

		AuthUI.Login((Result result, AccessToken accessToken) =>
		{
			Debug.Log("------- [Stove-Sample] AuthUI.Login : " + result + " -------");

			if (result.IsSuccessful)
			{
				string id = accessToken.User.UserId;
				string token = accessToken.Token;
				callback?.Invoke(true, id, token);
			}
			else
			{
				callback?.Invoke(false, "", "");
			}
		});
	}

	void AuthLogin(Action<bool, string, string> callback)
	{
		Auth.Login(new EmailProvider(), (Result result, AccessToken accessToken) =>
		{
			Debug.Log("------- [Stove-Sample] Auth.Login : " + result + " -------");

			if (result.IsSuccessful)
			{
				string id = accessToken.User.UserId;
				string token = accessToken.Token;
				callback?.Invoke(true, id, token);
			}
			else
			{
				callback?.Invoke(false, "", "");
			}
		});
	}

	public void Logout()
	{
		Auth.Logout();
	}

}
