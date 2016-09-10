using UnityEngine;
using System.Collections;
using DragonBone;

public class TestDargonBoneEvent : MonoBehaviour {

	private DragonBoneEvent m_Evt;

	// Use this for initialization
	void Start () {
		m_Evt = GetComponent<DragonBoneEvent>();
		m_Evt.onDragonBoneEvent += delegate(DragonBoneEventData obj) {
			print(obj.eventName+"  "+obj.action+"   "+obj.sound);
		};
	}

}
