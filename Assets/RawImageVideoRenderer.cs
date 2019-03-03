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

public class RawImageVideoRenderer : MonoBehaviour
{
	public string device = "/dev/dri/renderD128";
	public string ip = "";
	public ushort port = 9766;

	private IntPtr nhvd;
	private NHVD.nhvd_frame frame = new NHVD.nhvd_frame{ data=new System.IntPtr[8], linesize=new int[8] };
	private Texture2D Y, UV;


	void Awake()
	{
        NHVD.nhvd_hw_config hw_config = new NHVD.nhvd_hw_config{hardware="mediacodec", codec="h264_mediacodec", device=Application.persistentDataPath + "/" + "testfile.mp4", pixel_format="yuv420p"};
		NHVD.nhvd_net_config net_config = new NHVD.nhvd_net_config{ip=this.ip, port=this.port, timeout_ms=500 };

		nhvd=NHVD.nhvd_init (ref net_config, ref hw_config);

		if (nhvd == IntPtr.Zero)
		{
			Debug.Log ("failed to initialize NHVD");
			gameObject.SetActive (false);
		}
			
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
            UV = new Texture2D (frame.width/2, frame.height/2, TextureFormat.RG16, false);
			GetComponent<RawImage> ().texture = Y;
			GetComponent<RawImage> ().material.SetTexture ("_UV", UV);
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
			UV.LoadRawTextureData (frame.data [1], frame.width * frame.height / 2);
			UV.Apply (false);
		}
			
		if (NHVD.nhvd_get_frame_end (nhvd) != 0)
			Debug.LogWarning ("Failed to get NHVD frame data");
	}
}
