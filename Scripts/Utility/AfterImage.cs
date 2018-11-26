using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImage : MonoBehaviour
{
	struct ImageData
	{
		public Mesh mesh;
		public Material material;
		public float showStartTime;
		public float duration;
		public float alpha;
		public bool needRemove;
		public Quaternion quat;
		public Vector3 pos;
	}
    
    private SkinnedMeshRenderer[] _renderers;
    private Material[] _mats;
	private List<ImageData> _imageList = new List<ImageData>();

	void Awake()
	{
        _renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        _mats = new Material[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
        {
            _mats[i] = new Material(_renderers[i].sharedMaterial);
            _mats[i].SetFloat("_Mode", 3);
            _mats[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            _mats[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _mats[i].SetInt("_ZWrite", 0);
            _mats[i].DisableKeyword("_ALPHATEST_ON");
            _mats[i].DisableKeyword("_ALPHABLEND_ON");
            _mats[i].EnableKeyword("_ALPHAPREMULTIPLY_ON");
            _mats[i].renderQueue = 3000;
        }
    }

    public void Play(float fadeOut, Color color)
    {
        CreateImage(fadeOut, color);
    }

    public void Play(float interval, float fadeout, Color color)
	{
        Stop();
		StartCoroutine(DoAddImage(interval, fadeout, color));
	}

    public void Stop()
    {
        StopAllCoroutines();
    }

	IEnumerator DoAddImage(float interval, float fadeTime, Color color)
	{
        yield return null;

        float elapsedTime = 0f;
		while (true)
		{
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= interval)
            {
                CreateImage(fadeTime, color);
                elapsedTime = 0f;
            }

            yield return null;
        }
	}

	private void CreateImage(float fadeTime, Color color)
	{
		Transform t = transform;
		Material mat = null;
		for (int i = 0; i < _renderers.Length; ++i)
		{
			var item = _renderers[i];
			var tK = item.transform;
            var mesh = new Mesh();

			mat = _mats[i];
            mat.color = color;

            item.BakeMesh(mesh);
            
			var count = mesh.vertexCount;
			var tmp = mesh.vertices;
			var baseScalex = tK.lossyScale.x;
			for (int j = 0; j < count; j++)
			{
				tmp[j].x *= baseScalex;
			}
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            
			_imageList.Add(new ImageData
			{
				mesh = mesh,
				material = mat,
				showStartTime = Time.realtimeSinceStartup,
				duration = fadeTime,
				quat = tK.transform.rotation,
				pos = tK.transform.position
			});

		}
	}

    private void LateUpdate()
    {
        bool hasRemove = false;
        for (int i = 0; i < _imageList.Count; i++)
        {
            var item = _imageList[i];
            float time = Time.realtimeSinceStartup - item.showStartTime;

            if (time > item.duration)
            {
                item.needRemove = true;
                hasRemove = true;
                continue;
            }

            Graphics.DrawMesh(item.mesh, item.pos, item.quat, item.material, gameObject.layer);
        }

        if (hasRemove)
        {
            _imageList.RemoveAll(x => x.needRemove);
        }
    }
}