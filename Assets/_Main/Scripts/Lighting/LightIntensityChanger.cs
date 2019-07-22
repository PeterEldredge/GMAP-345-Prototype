using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightIntensityChanger : MonoBehaviour {

	[SerializeField, Range(0, 5)] private float _minIntensity;
	[SerializeField, Range(0, 5)] private float _maxIntensity;

	[SerializeField, Range(0, 5)] private float _changeSpeed;

	private Light _light;

	private void Awake()
	{
		_light = GetComponent<Light>();
		_light.intensity = 0f;

		StartCoroutine(IntensityChanger());
	}

	private IEnumerator IntensityChanger()
	{
		float intensityDifference = _maxIntensity - _minIntensity;
		float randomY = Random.Range(0, 100);

		while(true)
		{
			float perlinNoise = Mathf.PerlinNoise(Time.time * _changeSpeed, randomY);
			
			_light.intensity = _minIntensity + perlinNoise * intensityDifference;

			yield return 0f;
		}
	}
}
