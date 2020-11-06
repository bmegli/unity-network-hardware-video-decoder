using UnityEngine;
using System.Collections;

public class Physics : MonoBehaviour
{
	public float wheelDiameterMm=43.2f;
	public float wheelbaseMm=250.0f;
	public float encoderCountsPerRotation=360.0f;
	public int maxEncoderCountsPerSecond=1000;
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
