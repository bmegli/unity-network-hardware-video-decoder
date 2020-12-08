using UnityEngine;
using System.Collections;

public class Physics : MonoBehaviour
{
	public float wheelDiameterMm=120.0f;
	public float wheelbaseMm=400.0f;
	public float encoderCountsPerRotation=1196.8f;
	public int maxEncoderCountsPerSecond=2000;
	public bool reverseMotorPolarity=false;

	public float MMPerCount()
	{
		return Mathf.PI * wheelDiameterMm / encoderCountsPerRotation;
	}
	public float CountsPerMM()
	{
		return encoderCountsPerRotation / (Mathf.PI * wheelDiameterMm);
	}
		
	public Physics DeepCopy()
	{
		Physics other = (Physics) this.MemberwiseClone();
		return other;
	}
}
