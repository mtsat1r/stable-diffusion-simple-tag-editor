using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGroup : MonoBehaviour
{
	RectTransform _rt;

	public bool _updateSize;

	void Awake()
	{
		_rt = GetComponent<RectTransform>();
	}

	void Reflesh()
	{
		_rt.sizeDelta = transform.GetChild(0).GetComponent<RectTransform>().sizeDelta;
	}

	int _wait = 4;
	void LateUpdate()
	{
		if ( _updateSize ) {
			if ( --_wait < 0 ) {
				_wait = 4;
				_updateSize = false;
				Reflesh();
			}
		}
	}
}
