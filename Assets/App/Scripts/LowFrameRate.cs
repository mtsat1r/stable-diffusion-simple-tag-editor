using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LowFrameRate : MonoBehaviour
{
    [System.Serializable]
    class Settings
    {
        public bool enable = true;
        public float threshold_sec = 5f;
        public int low_fps = 5;
        public int low_fps_vsync = 2;
        public int high_fps = 30;
        public int high_fps_vsync = 1;

        public static string Path { get {
            string tmp = Application.persistentDataPath;//Application.dataPath;
			return tmp.Substring(0, tmp.LastIndexOf(@"/") + 1) + "low_frame_rate.json";
        }}
    }
    Settings _settings;

    float _countdown = 5f;
    Text _text;

    public static bool RequestGC { set; private get; }

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

		QualitySettings.vSyncCount = _settings.high_fps_vsync;
		Application.targetFrameRate = _settings.high_fps;
        _countdown = _settings.threshold_sec;

        _text = transform.Find("Canvas/Image/Text").GetComponent<Text>();
    }

    void Update()
    {
        if ( !_settings.enable ) return;

        var last = _countdown;
        _countdown -= Time.deltaTime;
        _text.text = UnityEngine.Rendering.OnDemandRendering.effectiveRenderFrameRate + ", " + _countdown.ToString("F3");

        if ( (_countdown < 0f) && (0f <= last) ) {
#if old
		    QualitySettings.vSyncCount = _settings.low_fps_vsync;
		    Application.targetFrameRate = _settings.low_fps;
#else
		    UnityEngine.Rendering.OnDemandRendering.renderFrameInterval = 10;
#endif
            if ( RequestGC ) {
                RequestGC = false;
			    System.GC.Collect();
			    Resources.UnloadUnusedAssets();
            }
        }
        else if ( Input.GetMouseButton(0) || 0f != Input.mouseScrollDelta.y || 0f != Input.mouseScrollDelta.x ) {
#if old
		    QualitySettings.vSyncCount = _settings.high_fps_vsync;
		    Application.targetFrameRate = _settings.high_fps;
#else
		    UnityEngine.Rendering.OnDemandRendering.renderFrameInterval = 1;
#endif
            _countdown = _settings.threshold_sec;
        }
        
    }
}
