using UnityEngine;
using System.Collections;

public class Limits : MonoBehaviour
{
	public float MaxLinearSpeedMmPerSec=200;
	public float MaxAngularSpeedDegPerSec=45;

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

