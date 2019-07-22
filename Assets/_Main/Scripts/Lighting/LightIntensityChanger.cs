using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightIntensityChanger : MonoBehaviour {
	
	[SerializeField] private float _startTime = 0f;

	[SerializeField] private ParticleSystem _particleSystem;

	[SerializeField, Range(0, 5)] private float _minIntensity;
	[SerializeField, Range(0, 5)] private float _maxIntensity;

	[SerializeField, Range(0, 5)] private float _changeSpeed;

	private Light _light;

	private void Awake()
	{
		_light = GetComponent<Light>();
		_light.intensity = 0f;
		_particleSystem.Stop();

		StartCoroutine(IntensityChanger());
	}

	private IEnumerator IntensityChanger()
	{
		yield return new WaitForSeconds(_startTime);

		_particleSystem.Play();

		float intensityDifference = _maxIntensity - _minIntensity;
		while(true)
		{
			float perlinNoise = Mathf.PerlinNoise(Time.time * _changeSpeed, 0);
			
			_light.intensity = _minIntensity + perlinNoise * intensityDifference;

			yield return 0f;
		}
	}
}
