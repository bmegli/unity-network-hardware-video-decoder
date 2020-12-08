/*
 * Unity Network Hardware Video Decoder
 * 
 * Copyright 2020 (C) Bartosz Meglicki <meglickib@gmail.com>
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 *
 */

using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Physics), typeof(Limits), typeof(UserInput))]
public class Drive : MonoBehaviour
{
	public string ip = "192.168.0.30";
	public ushort port = 10000;
	private IntPtr rc;

	public int packetDelayMs=50;
	private float timeSinceLastPacketMs;

	private Physics physics;
	private Limits limits;
	private UserInput input;

	void Awake()
	{
		RC.rc_net_config net_config = new RC.rc_net_config{ip=this.ip, port=this.port};

		rc = RC.rc_init (ref net_config);

		if (rc == IntPtr.Zero)
		{
			Debug.Log ("failed to initialize RC");
			gameObject.SetActive (false);
			return;
		}

		physics = GetComponent<Physics>().DeepCopy();
		limits = GetComponent<Limits>().DeepCopy();
		input = GetComponent<UserInput>().DeepCopy();
	}
	void OnDestroy()
	{
		RC.rc_close (rc);
	}

	void Update ()
	{
		timeSinceLastPacketMs += Time.deltaTime*1000.0f;

		if (timeSinceLastPacketMs < packetDelayMs)
			return;

		short left, right;
		InputToEngineSpeeds (Input.GetAxis(input.horizontal), Input.GetAxis(input.vertical), 1.0f,
							 out left,out right);
		
		RC.rc_command(rc, 1, left, right);
		timeSinceLastPacketMs = 0.0f;
	}
	
	public void InputToEngineSpeeds(float in_hor, float in_ver, float in_scale,out short left_counts_s,out short right_counts_s)
	{
		float maxAngularSpeedContributionMmPerS = limits.MaxAngularSpeedRadPerS() * physics.wheelbaseMm / 2.0f;
		float countsPerMM = physics.CountsPerMM ();

		float V_mm_s = in_ver * limits.MaxLinearSpeedMmPerSec;
		float angular_speed_contrib_mm_s = maxAngularSpeedContributionMmPerS * in_hor;

		float VL_mm_s = V_mm_s + angular_speed_contrib_mm_s;
		float VR_mm_s = V_mm_s - angular_speed_contrib_mm_s;

		float VL_counts_s = VL_mm_s * countsPerMM;
		float VR_counts_s = VR_mm_s * countsPerMM;

		float scale = in_scale * ( (physics.reverseMotorPolarity) ? -1.0f : 1.0f );

		left_counts_s = (short)(VL_counts_s * scale);
		right_counts_s = (short)(VR_counts_s * scale);

		Clamp(ref left_counts_s, (short)-physics.maxEncoderCountsPerSecond, (short)physics.maxEncoderCountsPerSecond);
		Clamp(ref right_counts_s, (short)-physics.maxEncoderCountsPerSecond, (short)physics.maxEncoderCountsPerSecond);
	}
	private void Clamp(ref short value, short min, short max)
	{
		if (value < min)
		{
			Debug.LogWarning ("Limits of differential drive exceed physical capabilities: leads to " + value + " speed where theorethical limit is " + min);	
			value = min;
		}
		else if (value > max)
		{
			Debug.LogWarning ("Limits of differential drive exceed physical capabilities: leads to " + value + " speed where theorethical limit is " + max);	
			value = max;
		}
	}
}
