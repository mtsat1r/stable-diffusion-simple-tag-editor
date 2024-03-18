using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SFB;

public class LearningTool : MonoBehaviour
{
	[SerializeField] GameObject _groupImage;
	[SerializeField] GameObject _groupWords;
		
	public class Data
	{
		public string path;
		public string path_text;
		public List<string> words = new List<string>();
	}
	List<Data> _dataset = new List<Data>();
	
	List<LearningItem> _allImage = new List<LearningItem>();
	List<LearningItemWord> _allWords = new List<LearningItemWord>();
	
    [System.Serializable]
    class Settings
    {
        public string lastDir = string.Empty;
        public bool load_image = false;
        public bool make_image_cache = false;

        public static string Path { get {
            string tmp = Application.persistentDataPath;//Application.dataPath;
			return tmp.Substring(0, tmp.LastIndexOf(@"/") + 1) + "config.json";
        }}
    }
    Settings _settings;
	
	List<string> MakeFileList(string dir)
	{
		List<string> ret = new List<string>();

		if ( string.IsNullOrEmpty(dir) ) return ret;
		if ( !Directory.Exists(dir) ) return ret;

		var paths = Directory.GetFiles(dir, "*.*", SearchOption.TopDirectoryOnly).OrderBy(f => f);
		foreach ( var path in paths ) {
			if ( string.IsNullOrEmpty(path) ) { continue; }
			string _extension = System.IO.Path.GetExtension(path)?.ToLower();
			if ( ".png" == _extension || ".jpg" == _extension || ".jpeg" == _extension ) {
				ret.Add(path);
			}
		}
		return ret;
	}

	void Init(string dir)
	{
		_dataset.Clear();
		
		var list = MakeFileList(dir);
		foreach ( var v in list ) {
			var extension = Path.GetExtension(v);
			var path_text = v.Substring(0, v.Length - extension.Length) + ".txt";
			if ( !File.Exists(path_text) ) {
				continue;
			}
			var data = new Data();
			data.path = v;
			data.path_text = path_text;
			
            using(StreamReader reader = new StreamReader(path_text))
            {
                while (!reader.EndOfStream) {
                    var line = reader.ReadLine();
					line = line.Replace(", ",",");
					var spl = line.Split(',');
					foreach ( var s in spl ) {
						data.words.Add(s);
					}
                }
                reader.Close();
            }
			_dataset.Add(data);
		}

		//image
		{
			foreach ( var v in _allImage ) {
				GameObject.Destroy(v.gameObject);
			}
			_allImage.Clear();
		}
		var groupBg = _groupImage.transform.Find("Bg");
		var loadImage = transform.Find("Base/Info/Toggle_load_image").GetComponent<Toggle>().isOn;
		var makeImageCache = transform.Find("Base/Info/Toggle_make_image_cache").GetComponent<Toggle>().isOn;
		{
			var itemPrefab = groupBg.Find("prefab");
			itemPrefab.gameObject.SetActive(false);
			
			foreach ( var data in _dataset ) {
				var toggle = GameObject.Instantiate(itemPrefab, groupBg).GetComponent<LearningItem>();
				toggle.Init(data, loadImage, makeImageCache);
				toggle.gameObject.SetActive(true);
				_allImage.Add(toggle);
			}
		}
		
		ShowAllWords();

		{
			_groupImage.GetComponent<UIGroup>()._updateSize = true;
			_groupWords.GetComponent<UIGroup>()._updateSize = true;
		}

		_settings.lastDir = dir;
	}
	
	void UpdateSelectedText()
	{
		int cnt = 0;
		foreach ( var v in _allImage ) {
			if ( !v.isOn ) continue;
			++cnt;
		}
		transform.Find("Base/Info/text_selected").GetComponent<Text>().text = "selected: " + cnt + " / " + _allImage.Count;
	}
	void UpdateSelectedWordText()
	{
		int cnt = 0;
		foreach ( var v in _allWords ) {
			if ( !v.isOn ) continue;
			++cnt;
		}
		transform.Find("Base/Info/text_selected_word").GetComponent<Text>().text = "selected: " + cnt + " / " + _allWords.Count;
	}

	bool _dataChanged = false;
	void UpdateWriteButtonTextColor(bool dataChanged)
	{
		_dataChanged |= dataChanged;
		transform.Find("Base/Info/write_text/Text").GetComponent<TMPro.TMP_Text>().color = _dataChanged ? Color.red : Color.black;
	}

	void Start()
	{
        _settings = new Settings();
        {
            var path = Settings.Path;
            if ( System.IO.File.Exists(path) ) {
                string strTmp = System.IO.File.ReadAllText(path);
                _settings = JsonUtility.FromJson<Settings>(strTmp);
            }
            else {
                Util.WriteText(path, JsonUtility.ToJson(_settings), false);
            }
        }
		transform.Find("Base/Info/Toggle_load_image").GetComponent<Toggle>().isOn = _settings.load_image;
		transform.Find("Base/Info/Toggle_make_image_cache").GetComponent<Toggle>().isOn = _settings.make_image_cache;

		LearningItem.onValueChanged += x => {
			transform.Find("Base/Info/RawImage").GetComponent<RawImage>().texture = x.texture;
			RefleshWords(false);
			UpdateSelectedText();
		};

		LearningItemWord.onValueChanged += x => {
			UpdateSelectedWordText();
			transform.Find("Base/Info/InputField").GetComponent<TMPro.TMP_InputField>().text = x.Label;
		};
				
		transform.Find("Base/Info/del_selected_word_all").GetComponent<Button>().onClick.AddListener(() => {
			foreach ( var v in _allWords ) {
				if ( !v.isOn ) continue;
				foreach ( var d in _allImage ) {
					d.data.words.Remove(v.Word);
				}
			}
			RefleshWords(true);
			UpdateWriteButtonTextColor(true);
		});

		transform.Find("Base/Info/del_selected_word_this").GetComponent<Button>().onClick.AddListener(() => {
			foreach ( var v in _allWords ) {
				if ( !v.isOn ) continue;
				foreach ( var d in _allImage ) {
					if ( !d.isOn ) continue;
					d.data.words.Remove(v.Word);
				}
			}
			RefleshWords(true);
			UpdateWriteButtonTextColor(true);
		});
		
		transform.Find("Base/Info/write_text").GetComponent<Button>().onClick.AddListener(() => {
			foreach ( var d in _dataset ) {
				var backuppath = d.path_text + ".backup";
				if ( !File.Exists(backuppath) ) {
					File.Copy(d.path_text, backuppath, true);
				}
				string words = string.Empty;
				foreach ( var v in d.words ) words += v + ",";
				words = words.Substring(0, words.Length - 1);
				Util.WriteText(d.path_text, words, false);

				_dataChanged = false;
				UpdateWriteButtonTextColor(false);
			}
		});
		
		transform.Find("Base/Info/select_all").GetComponent<Button>().onClick.AddListener(() => {
			_acceptRefleshWords = false;
			foreach ( var v in _allImage ) {
				v.isOn = true;
			}
			_acceptRefleshWords = true;
			RefleshWords(true);
			UpdateSelectedText();
		});

		transform.Find("Base/Info/select_clear").GetComponent<Button>().onClick.AddListener(() => {
			_acceptRefleshWords = false;
			foreach ( var v in _allImage ) {
				v.isOn = false;
			}
			_acceptRefleshWords = true;
			RefleshWords(true);
			UpdateSelectedText();
		});

		transform.Find("Base/Info/add_word_head").GetComponent<Button>().onClick.AddListener(() => {
			var word = transform.Find("Base/Info/InputField").GetComponent<TMPro.TMP_InputField>().text;
			if ( !string.IsNullOrEmpty(word)) {
				foreach ( var v in _allImage ) {
					if ( !v.isOn ) continue;
					if ( v.data.words.Contains(word) ) {
						v.data.words.Remove(word);
					}
					v.data.words.Insert(0, word);
				}
				RefleshWords(true);
				UpdateWriteButtonTextColor(true);
			}
		});
		
		transform.Find("Base/Info/add_word_tail").GetComponent<Button>().onClick.AddListener(() => {
			var word = transform.Find("Base/Info/InputField").GetComponent<TMPro.TMP_InputField>().text;
			if ( !string.IsNullOrEmpty(word)) {
				foreach ( var v in _allImage ) {
					if ( !v.isOn ) continue;
					if ( v.data.words.Contains(word) ) continue;
					v.data.words.Add(word); //!
				}
				RefleshWords(true);
				UpdateWriteButtonTextColor(true);
			}
		});

		transform.Find("Base/Info/select_image_from_words").GetComponent<Button>().onClick.AddListener(() => {
			List<string> words = new List<string>();
			foreach ( var v in _allWords ) {
				if ( !v.isOn ) continue;
				words.Add(v.Label);
			}
			if ( 0 < words.Count ) {
				_acceptRefleshWords = false;
				foreach ( var v in _allImage ) {
					bool containsAll = true;
					foreach ( var w in words ) {
						containsAll = v.data.words.Contains(w);
						if ( !containsAll ) break;
					}
					v.isOn = containsAll;
				}
				_acceptRefleshWords = true;
				RefleshWords(true);
				UpdateSelectedText();
			}
		});
		
		transform.Find("Base/Info/show_all_words").GetComponent<Button>().onClick.AddListener(() => {
			ShowAllWords();
		});
		

		transform.Find("Base/Info/select_word_all").GetComponent<Button>().onClick.AddListener(() => {
			foreach ( var v in _allWords ) {
				v.isOn = true;
			}
			UpdateSelectedWordText();
		});

		transform.Find("Base/Info/select_word_clear").GetComponent<Button>().onClick.AddListener(() => {
			foreach ( var v in _allWords ) {
				v.isOn = false;
			}
			UpdateSelectedWordText();
		});
		
		transform.Find("Base/Info/open_dir").GetComponent<Button>().onClick.AddListener(() => {
			var paths = StandaloneFileBrowser.OpenFolderPanel("Dir", _settings.lastDir, false);
			if (0 < paths.Length) {
				Init(paths[0]);
				UpdateSelectedText();

				_dataChanged = false;
				UpdateWriteButtonTextColor(false);
			}
		});
	}

	bool _acceptRefleshWords = true;
	void RefleshWords(bool force)
	{
		if ( !_acceptRefleshWords && !force ) return;

		var selectedOld = new List<string>();

		foreach ( var v in _allWords ) {
			if ( v.isOn ) {
				selectedOld.Add(v.Label);
			}
			GameObject.Destroy(v.gameObject);
		}
		_allWords.Clear();

		List<Data> selected = new List<Data>();
		foreach ( var v in _allImage ) {
			if ( !v.isOn ) continue;
			selected.Add(v.data);
		}
		if ( selected.Count <= 0 ) return;
		
		var groupBg = _groupWords.transform.Find("Bg");
		{
			var itemPrefab = groupBg.Find("prefab");
			itemPrefab.gameObject.SetActive(false);

			List<string> words = new List<string>();
			words.AddRange(selected[0].words);
			foreach ( var v in words ) {
				bool notfound = false;
				foreach ( var d in selected ) {
					notfound = !d.words.Contains(v);
					if ( notfound ) break;
				}
				if ( !notfound ) {
					var toggle = GameObject.Instantiate(itemPrefab, groupBg).GetComponent<LearningItemWord>();
					toggle.Init(v);
					toggle.gameObject.SetActive(true);
					toggle.isOn = selectedOld.Contains(v);
					_allWords.Add(toggle);
				}
			}
		}

		UpdateSelectedWordText();
	}

	void ShowAllWords()
	{
		var selectedOld = new List<string>();

		foreach ( var v in _allWords ) {
			if ( v.isOn ) {
				selectedOld.Add(v.Label);
			}
			GameObject.Destroy(v.gameObject);
		}
		_allWords.Clear();

		var all = new List<string>();
		foreach ( var v in _dataset ) {
			var w = (0 < (v.words?.Count ?? 0)) ? v.words[0] : null;
			if ( string.IsNullOrEmpty(w) ) continue;
			if ( all.Contains(w) ) continue;
			all.Add(w);
		}
		foreach ( var v in _dataset ) {
			foreach ( var w in v.words ) {
				if ( all.Contains(w) ) continue;
				all.Add(w);
			}
		}
		
		var groupBg = _groupWords.transform.Find("Bg");
		{
			var itemPrefab = groupBg.Find("prefab");
			itemPrefab.gameObject.SetActive(false);

			foreach ( var v in all ) {
				var toggle = GameObject.Instantiate(itemPrefab, groupBg).GetComponent<LearningItemWord>();
				toggle.Init(v);
				toggle.gameObject.SetActive(true);
				toggle.isOn = selectedOld.Contains(v);
				_allWords.Add(toggle);
			}
		}
	}

	void OnDestroy()
	{
		_settings.load_image = transform.Find("Base/Info/Toggle_load_image").GetComponent<Toggle>().isOn;
		_settings.make_image_cache = transform.Find("Base/Info/Toggle_make_image_cache").GetComponent<Toggle>().isOn;
        Util.WriteText(Settings.Path, JsonUtility.ToJson(_settings), false);
	}
}
