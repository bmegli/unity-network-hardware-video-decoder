using UnityEngine;
using System.Collections;

public class UserInput : MonoBehaviour
{
	public string horizontal="Horizontal";
	public string vertical="Vertical";
	public string acceleration="Acceleration";
	public float accelerationPower = 0.5f;

	private void CheckSanity()
	{
		if (accelerationPower < 0f || accelerationPower > 1f)
		{
			Debug.LogError("UserInput - accelaration power has to be between <0, 1>");
			accelerationPower = 0.5f;
		}
	}

	public UserInput DeepCopy()
	{
		CheckSanity();
		UserInput other = (UserInput) this.MemberwiseClone();
		other.horizontal = string.Copy(horizontal);
		other.vertical = string.Copy(vertical);
		other.acceleration = string.Copy(acceleration);
		return other;
	}
}
