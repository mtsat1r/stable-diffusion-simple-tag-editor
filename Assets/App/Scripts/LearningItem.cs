using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class LearningItem : MonoBehaviour
{
	public static UnityAction<LearningItem> onValueChanged = null; 
	
	public string Label => transform.Find("Label").GetComponent<UnityEngine.UI.Text>().text;
	public bool isOn {
		set { GetComponent<UnityEngine.UI.Toggle>().isOn = value; }
		get { return GetComponent<UnityEngine.UI.Toggle>().isOn; }
	}
	
	public LearningTool.Data data { get; private set; }
	public Texture texture { get; private set; }

	public void Init(LearningTool.Data data, bool loadImage, bool makeImageCache)
	{
		this.data = data;
		
		var path = data.path;
		transform.Find("Label").GetComponent<UnityEngine.UI.Text>().text = System.IO.Path.GetFileName(data.path);

		var raw = transform.Find("RawImage")?.GetComponent<UnityEngine.UI.RawImage>();
		if ( null != raw && loadImage ) {
			if ( System.IO.File.Exists(path) ) {
				var extension = System.IO.Path.GetExtension(path);
				var cache_path = path.Replace(extension,".cache");

				Texture2D tex = null;
				if ( !System.IO.File.Exists(cache_path) ) {
					var org = new Texture2D(1,1);
					org.LoadImage(System.IO.File.ReadAllBytes(path));
					org.Apply();
					
					ResizeTextureBlit(org, 1024>>2, 1024>>2, null);
					tex = org;

					if ( makeImageCache ) {
						byte[] bin = tex.EncodeToJPG();
						System.IO.File.WriteAllBytes(cache_path, bin);
					}
				}
				else {
					tex = new Texture2D(1,1);
					tex.LoadImage(System.IO.File.ReadAllBytes(cache_path));
					tex.Apply();
				}

				raw.texture =
				this.texture = tex;
			}
		}
	}

	void Awake()
	{
		GetComponent<UnityEngine.UI.Toggle>().onValueChanged.AddListener(x => onValueChanged?.Invoke(this));
	}

	void OnDestroy()
	{
		if ( null != this.texture ) {
			GameObject.Destroy(this.texture);
			this.texture = null;
		}
	}

	public static void ResizeTextureBlit(Texture2D src, int targetX, int targetY, Material mat = null,  bool mipmap = true, FilterMode filter = FilterMode.Bilinear)
	{
		RenderTexture rt = RenderTexture.GetTemporary(targetX, targetY, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
		RenderTexture.active = rt;
		if ( null != mat ) {
			Graphics.Blit(src, rt, mat);
		}
		else {
			Graphics.Blit(src, rt);
		}
		src.Resize(targetX, targetY, src.format, mipmap);
		src.filterMode = filter;

		try
		{
			src.ReadPixels(new Rect(0.0f, 0.0f, targetX, targetY), 0, 0);
			src.Apply();
		}
		catch
		{
			//Trace("Read/Write is not enabled on texture "+ src.name, true);
		}
		RenderTexture.ReleaseTemporary(rt);
	}
}
