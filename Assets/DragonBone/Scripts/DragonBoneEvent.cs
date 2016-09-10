using UnityEngine;
using System.Collections;

namespace DragonBone
{
	[System.Serializable]
	public class DragonBoneEventData
	{
		public string eventName;
		public string action;
		public string sound;
	}

	[DisallowMultipleComponent]
	public class DragonBoneEvent : MonoBehaviour {

		public event System.Action<DragonBoneEventData> onDragonBoneEvent;

		/// <summary>
		/// Animation Callback
		/// </summary>
		/// <param name="data">Data.</param>
		public void OnAnimEvent(string data){
			if(onDragonBoneEvent!=null && !string.IsNullOrEmpty(data))
			{
				string[] param =  data.Split('$');
				DragonBoneEventData obj = new DragonBoneEventData();
				obj.eventName = param[0];
				if(!string.IsNullOrEmpty(param[1])){
					obj.action = param[1];
				}
				if(!string.IsNullOrEmpty(param[2])){
					obj.action = param[2];
				}
				onDragonBoneEvent (obj);
			}
		}

	}

}