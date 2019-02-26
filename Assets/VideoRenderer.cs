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

public class VideoRenderer : MonoBehaviour
{
	public string device = "/dev/dri/renderD128";
	public string ip = "";
	public ushort port = 9766;

	private IntPtr nhvd;
	private NHVD.nhvd_frame frame = new NHVD.nhvd_frame{ data=new System.IntPtr[8], linesize=new int[8] };
	private Texture2D Y, U, V;


	void Awake()
	{
		NHVD.nhvd_hw_config hw_config = new NHVD.nhvd_hw_config{hardware="vaapi", codec="h264", device=this.device, pixel_format="yuv420p"};
		NHVD.nhvd_net_config net_config = new NHVD.nhvd_net_config{ip=this.ip, port=this.port, timeout_ms=500 };

		nhvd=NHVD.nhvd_init (ref net_config, ref hw_config);

		if (nhvd == IntPtr.Zero)
		{
			Debug.Log ("failed to initialize NHVD");
			gameObject.SetActive (false);
		}
		/*
		Vector2[] uv = GetComponent<MeshFilter>().mesh.uv;
		for (int i = 0; i < uv.Length; ++i)
			uv [i][1] = -uv [i][1];
		GetComponent<MeshFilter> ().mesh.uv = uv;
		*/
	}
	void OnDestroy()
	{
		NHVD.nhvd_close (nhvd);
	}

	private void AdaptTexture()
	{
		if(Y== null || Y.width != frame.width || Y.height != frame.height)
		{
			Y = new Texture2D (frame.width, frame.height, TextureFormat.R8, false);
			U = new Texture2D (frame.width/2, frame.height/2, TextureFormat.R8, false);
			V = new Texture2D (frame.width/2, frame.height/2, TextureFormat.R8, false);
			GetComponent<MeshRenderer> ().material.mainTexture = Y;
			GetComponent<MeshRenderer> ().material.SetTexture ("_U", U);
			GetComponent<MeshRenderer> ().material.SetTexture ("_V", V);
		}
	}

	// Update is called once per frame
	void LateUpdate ()
	{
		if (NHVD.nhvd_get_frame_begin(nhvd, ref frame) == 0)
		{
			AdaptTexture ();
			Y.LoadRawTextureData (frame.data[0], frame.width*frame.height);
			Y.Apply (false);
			U.LoadRawTextureData (frame.data [1], frame.width * frame.height / 4);
			U.Apply (false);
			V.LoadRawTextureData (frame.data [2], frame.width * frame.height / 4);
			V.Apply (false);	
		}

		if (NHVD.nhvd_get_frame_end (nhvd) != 0)
			Debug.LogWarning ("Failed to get NHVD frame data");

	}
}
