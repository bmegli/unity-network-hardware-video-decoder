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

public class VideoRenderer : MonoBehaviour
{
	public string device = "/dev/dri/renderD128";
	public string ip = "";
	public ushort port = 9766;

	private IntPtr nhvd;
	private NHVD.nhvd_frame frame = new NHVD.nhvd_frame{ data=new System.IntPtr[3], linesize=new int[3] };
	private Texture2D videoTexture;

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

		//flip the texture mapping upside down
		Vector2[] uv = GetComponent<MeshFilter>().mesh.uv;
		for (int i = 0; i < uv.Length; ++i)
			uv [i][1] = -uv [i][1];
		GetComponent<MeshFilter> ().mesh.uv = uv;
	}
	void OnDestroy()
	{
		NHVD.nhvd_close (nhvd);
	}

	private void AdaptTexture()
	{
		if(videoTexture== null || videoTexture.width != frame.width || videoTexture.height != frame.height)
		{
			videoTexture = new Texture2D (frame.width, frame.height, TextureFormat.BGRA32, false);
			GetComponent<Renderer> ().material.mainTexture = videoTexture;
		}
	}

	// Update is called once per frame
	void LateUpdate ()
	{
		if (NHVD.nhvd_get_frame_begin(nhvd, ref frame) == 0)
		{
			AdaptTexture ();
			videoTexture.LoadRawTextureData (frame.data[0], frame.width*frame.height*4);
			videoTexture.Apply (false);
		}

		if (NHVD.nhvd_get_frame_end (nhvd) != 0)
			Debug.LogWarning ("Failed to get NHVD frame data");
	}
}
