using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCube : MonoBehaviour
{
public Transform Square;
[Range(0, 1)]
public float RotateTime = 2.5f;

public Material[] materials;

internal int color = 0;
internal int col;
internal int row;

public Color[] colors = new[]
{
new Color(0x00 / 255f, 0x37 / 255f, 0xFF / 255f, 1),
new Color(0x09 / 255f, 0xFF / 255f, 0x00 / 255f, 1),
new Color(0xFF / 255f, 0xEF / 255f, 0x05 / 255f, 1),
new Color(0xF6 / 255f, 0xEF / 255f, 0x05 / 255f, 1),
new Color(0x00 / 255f, 0xFF / 255f, 0xEA / 255f, 1),
};

private bool working;
// Use this for initialization
void Start()
{
//StartCoroutine(Test());
}

// Update is called once per frame
void Update()
{

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

Renderer render = Square.GetComponent<Renderer>();
var mat = render.materials;

//Find cube side
int a = direction == MoveDirection.Left ? 1 : direction == MoveDirection.Right ? 3 : direction == MoveDirection.Up ? 2 : 0;

mat[4] = materials[0]; //Set default material
mat[a] = materials[this.color]; //Set new material

render.materials = mat;//Assign materials array

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

public override string ToString()
{
return " cube col:" + col + " row:" + row + " color:" + color;
}
}