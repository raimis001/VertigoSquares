using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemCube : MonoBehaviour
{
	public Transform Square;
	public Text LabelText;
	public ParticleSystem Effect;

	[Range(0, 1)]
	public float RotateTime = 2.5f;

	public Material[] materials;

	internal int color = 0;
	internal int col;
	internal int row;

	private int points;

	internal int Points
	{
		get { return points; }
		set
		{
			points = value;
			Label = points > 2 ? points.ToString() : "";
		}
	}
	private string Label
	{
		set
		{
			if (!LabelText) return;
			LabelText.text = value;
		}
	}

	private bool working;


	void Start()
	{
		Label = "";

		//StartCoroutine(Test());
	}

	public void Rotate(MoveDirection direction, int color)
	{
		this.color = color;
		StartCoroutine(RotateCube(direction, color));
	}

	IEnumerator RotateCube(MoveDirection direction, int color)
	{
		while (working)
		{
			yield return null;
		}
		working = true;
		if (LabelText) LabelText.gameObject.SetActive(false);

		//Find cube side
		int a = direction == MoveDirection.Left ? 1 : direction == MoveDirection.Right ? 3 : direction == MoveDirection.Up ? 2 : 0;
		SetMaterials(a);

		//Debug.Log("Set color:" + color + " mat:" + materials[this.color].name);

		//Set cube rotation to zero
		Square.localEulerAngles = Vector3.zero;
		Quaternion start = Square.localRotation;

		start.y = 0;

		//Find rotation angle
		float x = direction == MoveDirection.Up ? start.x + 90f : direction == MoveDirection.Down ? start.x - 90f : start.x;
		float z = direction == MoveDirection.Left ? start.z + 90f : direction == MoveDirection.Right ? start.z - 90f : start.z;
		Quaternion end = Quaternion.Euler(new Vector3(x, 0, z));

		//Debug.Log("Rotate to:" + direction + " " + start + ":" + end);

		float t = RotateTime;
		while (t > 0)
		{
			float d = 1 - t / RotateTime;

			//Rotate by lerp
			Square.localRotation = Quaternion.Lerp(start, end, d);

			t -= Time.deltaTime;
			yield return null;
		}

		//Set final angle
		Square.localRotation = end;
		if (LabelText) LabelText.gameObject.SetActive(true);

		working = false;
	}

	IEnumerator Test()
	{
		foreach (MoveDirection dir in Helper.GetEnumList<MoveDirection>())
		{
			yield return RotateCube(dir, 1);
			yield return new WaitForSeconds(0.5f);
		}

		StartCoroutine(Test());
	}

	public void ClearScore()
	{
		//Clear label
		Label = "";


		if (points < 3)
		{
			points = 0;
			return;
		}

		//Set non color material
		SetMaterials();

		//Set start rotation
		Square.localEulerAngles = Vector3.zero;

		color = 0;
		points = 0;

		if (Effect) Effect.Play(true);
	}

	void SetMaterials(int face = -1)
	{
		Renderer render = Square.GetComponent<Renderer>();
		var mat = render.sharedMaterials;

		for (int i = 0; i < mat.Length; i++)
		{
			mat[i] = materials[0];
		}

		if (face > -1)
		{
			mat[face] = materials[color + 1];
		}

		render.sharedMaterials = mat;
	}
	public override string ToString()
	{
		return " cube col:" + col + " row:" + row + " color:" + color;
	}
}