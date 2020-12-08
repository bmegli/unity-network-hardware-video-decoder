using UnityEngine;
using System.Collections;

public class Limits : MonoBehaviour
{
	public float MaxLinearSpeedMmPerSec=400;
	public float MaxAngularSpeedDegPerSec=60;

	public float MaxAngularSpeedRadPerS()
	{
		return Mathf.Deg2Rad * MaxAngularSpeedDegPerSec;
	}
		
	public Limits DeepCopy()
	{
		Limits other = (Limits) this.MemberwiseClone();
		return other;
	}
}

