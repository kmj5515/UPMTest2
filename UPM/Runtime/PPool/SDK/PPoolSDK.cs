using System;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;

namespace PPool
{
	internal class PPoolSDK : MonoBehaviour
    {
		static PPoolSDK instance;
		public static PPoolSDK Instance
		{
			get
			{
				if (instance == null)
				{
					GameObject gameObject = new GameObject("PPoolSDK");
					instance = gameObject.AddComponent<PPoolSDK>();
				}
				return instance;
			}
		}

		public const string IdentifierKey = "identifier";
		public const string ValueKey = "value";

		private Dictionary<string, Action<Dictionary<string, object>>> actionDic = new Dictionary<string, Action<Dictionary<string, object>>>();

		private void Awake()
		{
			if (instance == null)
			{
				instance = this;
			}
			else if (instance != this)
			{
				Destroy(gameObject);
			}
			DontDestroyOnLoad(gameObject);
		}
		
		internal string AddAction(Action<Dictionary<string, object>> action)
        {
            string identifier = Guid.NewGuid().ToString();
            actionDic.Add(identifier, action);

            return identifier;
        }

		internal void RemoveAction(string identifier)
		{
			if (actionDic.ContainsKey(identifier))
			{
				actionDic.Remove(identifier);
			}
		}

		public void CallMessage(string message)
        {
			Debug.Log($"@@@ [Unity-SDK] CallMessage : {message}");
            var payload = Json.Deserialize(message) as Dictionary<string, object>;

            string identifier = payload[IdentifierKey] as string;
            var value = payload[ValueKey] as Dictionary<string, object>;

			Action<Dictionary<string, object>> callback = null;
            if (actionDic.TryGetValue(identifier, out callback))
            {
                actionDic.Remove(identifier);
            }

            if (callback != null)
            {
                callback.Invoke(value);
            }
        }

        public void NotifyMessage(string message)
        {
			Debug.Log($"@@@ [Unity-SDK] NotifyMessage : {message}");
			var payload = Json.Deserialize(message) as Dictionary<string, object>;

            string identifier = payload[IdentifierKey] as string;
            var value = payload[ValueKey] as Dictionary<string, object>;

            Action<Dictionary<string, object>> callback = actionDic[identifier] as Action<Dictionary<string, object>>;
            
            if (callback != null)
            {
				callback.Invoke(value);
            }
        }
	}
}
