/*
 * Unity Network Hardware Video Decoder
 * 
 * Copyright 2019 (C) Bartosz Meglicki <meglickib@gmail.com>
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 *
 */

using System;
using UnityEngine;
using UnityEngine.UI; //RawImage
using UnityEngine.Rendering; //CommandBuffer

public class RawImageVideoRenderer : MonoBehaviour
{
	public string device = "/dev/dri/renderD128";
	public string ip = "";
	public ushort port = 9766;

	private IntPtr nhvd;
	private NHVD.nhvd_frame frame = new NHVD.nhvd_frame{ data=new System.IntPtr[3], linesize=new int[3] };
	private Texture2D videoTexture;
	private CommandBuffer commandBuffer;

	void Awake()
	{
		NHVD.nhvd_hw_config hw_config = new NHVD.nhvd_hw_config{hardware="vaapi", codec="h264", device=this.device, pixel_format="bgr0"};
		NHVD.nhvd_net_config net_config = new NHVD.nhvd_net_config{ip=this.ip, port=this.port, timeout_ms=500 };

		nhvd=NHVD.nhvd_init (ref net_config, ref hw_config);

		if (nhvd == IntPtr.Zero)
		{
			Debug.Log ("failed to initialize NHVD");
			gameObject.SetActive (false);
		}

		commandBuffer = new CommandBuffer();

		videoTexture = new Texture2D (640, 360, TextureFormat.BGRA32, false);
		GetComponent<RawImage> ().texture = videoTexture;
			
	}
	void OnDestroy()
	{
		NHVD.nhvd_close (nhvd);
	}

	 void Update()
    {
        commandBuffer.IssuePluginCustomTextureUpdateV2( NHVD.GetUnityTextureUpdateCallback(), videoTexture, 0);
        Graphics.ExecuteCommandBuffer(commandBuffer);
        commandBuffer.Clear();
 	}
}