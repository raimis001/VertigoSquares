using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public static class Helper
{
	public static T GetRandomEnum<T>()
	{
		System.Array A = System.Enum.GetValues(typeof(T));
		T V = (T) A.GetValue(UnityEngine.Random.Range(0, A.Length));
		return V;
	}

	public static IEnumerable GetEnumList<T>()
	{
		System.Array A = System.Enum.GetValues(typeof(T));
		foreach (var V in A)
		{
			yield return (T) V;
		}
	}

	public static float AngleInRad(Vector3 vec1, Vector3 vec2)
	{
		return Mathf.Atan2(vec2.x - vec1.x, vec2.z - vec1.z);
	}

	public static float AngleInDeg(Vector3 vec1, Vector3 vec2)
	{
		return AngleInRad(vec1, vec2) * 180f / Mathf.PI;
	}


	public static bool isPointerOverUI()
	{
		//For Android input ower UI elements

		if (!EventSystem.current) return false;

		PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
		return results.Count > 0;
	}

}

public class WeightClass<T>
{
	public float Weight { get; set; }
	public T Value { get; set; }
}

public static class Weighted
{
	public static float TotalWeight = 0;

	public static WeightClass<T> Create<T>(float weight, T value)
	{
		TotalWeight += weight;
		return new WeightClass<T> { Weight = weight, Value = value };
	}

	public static T GetWeighted<T>(this IEnumerable<WeightClass<T>> collection)
	{
		float total = 0;

		float random = UnityEngine.Random.Range(0, TotalWeight + 0.00f);
		foreach (var item in collection)
		{
			total += item.Weight;
			if (total >= random)
			{
				return item.Value;
			}
		}
		throw new InvalidOperationException("The proportions in the collection do not add up to 1.");
	}
}


public class Timer
{
	private float endTime;
	private float deltaTime;

	public Timer(float time)
	{
		Reset(time);
	}

	public bool IsEnded()
	{
		return endTime < Time.time;
	}
	public void Reset(float time)
	{
		deltaTime = time;
		endTime = Time.time + time;
	}

	public float Progress()
	{
		return 1f - (endTime - Time.time) / deltaTime;
	}
}
