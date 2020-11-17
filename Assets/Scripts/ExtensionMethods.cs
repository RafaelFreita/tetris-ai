using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    public static void Round(this Vector2 v)
	{
		v.x = Mathf.RoundToInt(v.x);
		v.y = Mathf.RoundToInt(v.x);
	}
}
