using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class LearningItemWord : MonoBehaviour
{
	public static UnityAction<LearningItemWord> onValueChanged = null; 
	
	public string Label => transform.Find("Label").GetComponent<UnityEngine.UI.Text>().text;
	public bool isOn {
		set { GetComponent<UnityEngine.UI.Toggle>().isOn = value; }
		get { return GetComponent<UnityEngine.UI.Toggle>().isOn; }
	}
	
	public string Word { get; private set; }

	public void Init(string word)
	{
		Word = word;
		
		transform.Find("Label").GetComponent<UnityEngine.UI.Text>().text = word;
	}

	void Awake()
	{
		GetComponent<UnityEngine.UI.Toggle>().onValueChanged.AddListener(x => onValueChanged?.Invoke(this));
	}
}
