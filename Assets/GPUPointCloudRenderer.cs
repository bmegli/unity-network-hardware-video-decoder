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
using UnityEngine.Rendering;
using Unity.Collections;

public class GPUPointCloudRenderer : MonoBehaviour
{
	public string device = "/dev/dri/renderD128";
	public string ip = "";
	public ushort port = 9768;

	public ComputeShader unprojectionShader;
	public Shader pointCloudShader;

	private IntPtr unhvd;

	private UNHVD.unhvd_frame[] frame = new UNHVD.unhvd_frame[]
	{
		new UNHVD.unhvd_frame{ data=new System.IntPtr[3], linesize=new int[3] },
		new UNHVD.unhvd_frame{ data=new System.IntPtr[3], linesize=new int[3] }
	};

	private Texture2D depthTexture; //uint16 depth map filled with data from native side
	private Texture2D colorTexture; //rgb0 color map filled with data from native side

	private ComputeBuffer vertexBuffer;
	private ComputeBuffer argsBuffer;
	
	private Material material;

	void Awake()
	{
		//Application.targetFrameRate = 30;

		UNHVD.unhvd_net_config net_config = new UNHVD.unhvd_net_config{ip=this.ip, port=this.port, timeout_ms=500 };
		UNHVD.unhvd_hw_config[] hw_config = new UNHVD.unhvd_hw_config[]
		{
			new UNHVD.unhvd_hw_config{hardware="vaapi", codec="hevc", device=this.device, pixel_format="p010le", width=848, height=480, profile=2},
			new UNHVD.unhvd_hw_config{hardware="vaapi", codec="hevc", device=this.device, pixel_format="rgb0", width=848, height=480, profile=1}
		};

		IntPtr nullPtr = IntPtr.Zero;
		
		unhvd = UNHVD.unhvd_init (ref net_config, hw_config, hw_config.Length, IntPtr.Zero);
	
		if (unhvd == IntPtr.Zero)
		{
			Debug.Log("failed to initialize UNHVD");
			gameObject.SetActive (false);
			return;
		}

		argsBuffer = new ComputeBuffer( 4, sizeof( int ), ComputeBufferType.IndirectArguments );
		//vertex count per instance, instance count, start vertex location, start instance location
		//see https://docs.unity3d.com/2019.4/Documentation/ScriptReference/Graphics.DrawProceduralIndirectNow.html
		argsBuffer.SetData( new int[] { 0, 1, 0, 0 } );

		//For depth config explanation see:
		//https://github.com/bmegli/unity-network-hardware-video-decoder/wiki/Point-clouds-configuration

		//For D435 at 848x480 the MinZ is ~16.8cm, in our result unit min_margin is 0.168
		//max_margin is arbitrarilly set
		DepthConfig dc = new DepthConfig {ppx = 421.353f, ppy=240.93f, fx=426.768f, fy=426.768f, depth_unit = 0.0001f, min_margin = 0.168f, max_margin = 0.01f};
		//DepthConfig dc = new DepthConfig {ppx = 421.353f, ppy=240.93f, fx=426.768f, fy=426.768f, depth_unit = 0.0000390625f, min_margin = 0.168f, max_margin = 0.01f};

		SetDepthConfig(dc);
	}

	void SetDepthConfig(DepthConfig dc)
	{
		//The depth texture values will be normalied in the shader, 16 bit to [0, 1]
		float maxDistance = dc.depth_unit * 0xffff;
		//Normalize also valid ranges
		float minValidDistance = dc.min_margin / maxDistance;
		//Our depth encoding uses P010LE format for depth texture which uses 10 MSB of 16 bits (0xffc0 is 10 "1" and 6 "0")
		float maxValidDistance = (dc.depth_unit * 0xffc0 - dc.max_margin) / maxDistance;
		//The multiplier renormalizes [0, 1] to real world units again and is part of unprojection
		float[] unprojectionMultiplier = {maxDistance / dc.fx, maxDistance / dc.fy, maxDistance};

		unprojectionShader.SetFloats("UnprojectionMultiplier", unprojectionMultiplier);
		unprojectionShader.SetFloat("PPX", dc.ppx);
		unprojectionShader.SetFloat("PPY", dc.ppy);
		unprojectionShader.SetFloat("MinDistance", minValidDistance);
		unprojectionShader.SetFloat("MaxDistance", maxValidDistance);
	}

	void OnDestroy()
	{
		UNHVD.unhvd_close (unhvd);

		if(vertexBuffer != null)
			vertexBuffer.Release();

		if(argsBuffer != null)
			argsBuffer.Release();

		if (material != null)
		{
			if (Application.isPlaying)			
				Destroy(material);
			else			
				DestroyImmediate(material);
		}	
	}

	void LateUpdate ()
	{
		bool updateNeeded = false;

		if (UNHVD.unhvd_get_frame_begin(unhvd, frame) == 0)
			updateNeeded = PrepareTextures();

		if (UNHVD.unhvd_get_frame_end (unhvd) != 0)
			Debug.LogWarning ("Failed to get UNHVD frame data");

		if(!updateNeeded)
			return;

		vertexBuffer.SetCounterValue(0);
		unprojectionShader.Dispatch(0, frame[0].width/8, frame[0].height/8, 1);
		ComputeBuffer.CopyCount(vertexBuffer, argsBuffer, 0);
	}

	private bool PrepareTextures()
	{
		if(frame[0].data[0] == IntPtr.Zero)
			return false;

		Adapt();

		depthTexture.LoadRawTextureData (frame[0].data[0], frame[0].linesize[0] * frame[0].height);
		depthTexture.Apply (false);

		if(frame[1].data[0] == IntPtr.Zero)
			return true; //only depth data is also ok

		colorTexture.LoadRawTextureData (frame[1].data[0], frame[1].linesize[0] * frame[1].height);
		colorTexture.Apply (false);

		return true;
	}

	private void Adapt()
	{
		//adapt to incoming stream if something changed
		if(depthTexture == null || depthTexture.width != frame[0].width || depthTexture.height != frame[0].height)
		{
			depthTexture = new Texture2D (frame[0].width, frame[0].height, TextureFormat.R16, false);
			unprojectionShader.SetTexture(0, "depthTexture", depthTexture);

			vertexBuffer = new ComputeBuffer(frame[0].width*frame[0].height, 2 * sizeof(float)*4, ComputeBufferType.Append);
			unprojectionShader.SetBuffer(0, "vertices", vertexBuffer);
		}

		if(colorTexture == null || colorTexture.width != frame[1].width || colorTexture.height != frame[1].height)
		{
			if(frame[1].data[0] != IntPtr.Zero)
				colorTexture = new Texture2D (frame[1].width, frame[1].height, TextureFormat.BGRA32, false);
			else
			{	//in case only depth data is coming prepare dummy color texture
				colorTexture = new Texture2D (frame[0].width, frame[0].height, TextureFormat.BGRA32, false);
				uint[] data = new uint[frame[0].width * frame[0].height];
				for(int i=0;i<data.Length;i++)
				data[i] = 0xFFFFFFFF;
				colorTexture.SetPixelData(data, 0, 0);
				colorTexture.Apply();
			}
			unprojectionShader.SetTexture(0, "colorTexture", colorTexture);
		}
	}

	void OnRenderObject()
	{	
		if(vertexBuffer == null)
			return;

		if (material == null)
		{
			material = new Material(pointCloudShader);
			material.hideFlags = HideFlags.DontSave;
		}

		material.SetPass(0);
		material.SetBuffer("vertices", vertexBuffer);

		Graphics.DrawProceduralIndirectNow(MeshTopology.Points, argsBuffer);
	}
}
