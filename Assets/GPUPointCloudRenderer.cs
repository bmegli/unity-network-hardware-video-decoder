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
using UnityEngine.Experimental.Rendering;

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
		Application.targetFrameRate = 300;

		if(!CheckRequirements())
		{
			gameObject.SetActive (false);
			return;
		}

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

		vertexBuffer = new ComputeBuffer(848*480, 2 * sizeof(float)*4, ComputeBufferType.Append);
	
		argsBuffer = new ComputeBuffer( 4, sizeof( int ), ComputeBufferType.IndirectArguments );
		//vertex count per instance, instance count, start vertex location, start instance location
		argsBuffer.SetData( new int[] { 0, 1, 0, 0 } );

		unprojectionShader.SetBuffer(0, "vertices", vertexBuffer);

		float depthUnit = 0.0000390625f;
		float maxDistance = depthUnit * 0xffff;
		float minValidDistance = 0.19f / maxDistance;
		float maxValidDistance = (depthUnit * 0xffc0 - 0.01f) / maxDistance;
		float fx = 470.941f, fy = 470.762f;
		float ppx = 358.781f, ppy = 246.297f;
		float[] unprojectionMultiplier = {maxDistance / fx, maxDistance / fy, maxDistance};

		unprojectionShader.SetFloats("UnprojectionMultiplier", unprojectionMultiplier);
		unprojectionShader.SetFloat("PPX", ppx);
		unprojectionShader.SetFloat("PPY", ppy);
		unprojectionShader.SetFloat("MinDistance", minValidDistance);
		unprojectionShader.SetFloat("MaxDistance", maxValidDistance);
	}

	bool CheckRequirements()
	{
		if(!SystemInfo.SupportsTextureFormat(TextureFormat.R16))
		{
			Debug.Log("R16 texture format not supported");
			return false;
		}

		if(!SystemInfo.supportsComputeShaders)
		{
			Debug.Log("Compute shaders not supported");
			return false;
		}

		return true;
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

	private void AdaptTexture()
	{
		if(depthTexture == null || depthTexture.width != frame[0].width || depthTexture.height != frame[0].height)
		{
			depthTexture = new Texture2D (frame[0].width, frame[0].height, TextureFormat.R16, false);
			unprojectionShader.SetTexture(0, "depthTexture", depthTexture);
		}

		if(colorTexture == null || colorTexture.width != frame[1].width || colorTexture.height != frame[1].height)
		{
			if(frame[1].data[0] != IntPtr.Zero)
				colorTexture = new Texture2D (frame[1].width, frame[1].height, TextureFormat.BGRA32, false);
			else
			{
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

	void LateUpdate ()
	{
		bool updateNeeded = false;

		if (UNHVD.unhvd_get_frame_begin(unhvd, frame) == 0)
		{
			if(frame[0].data[0] != IntPtr.Zero)
			{
				AdaptTexture();
				depthTexture.LoadRawTextureData (frame[0].data[0], frame[0].linesize[0] * frame[0].height);
				depthTexture.Apply (false);

				if(frame[1].data[0] != IntPtr.Zero)
				{				
					colorTexture.LoadRawTextureData (frame[1].data[0], frame[1].linesize[0] * frame[1].height);
					colorTexture.Apply (false);
				}

				updateNeeded = true;
				
			}
		}

		if (UNHVD.unhvd_get_frame_end (unhvd) != 0)
		{
			Debug.LogWarning ("Failed to get UNHVD frame data");
			return;
		}

		if(!updateNeeded)
			return;

		vertexBuffer.SetCounterValue(0);
		unprojectionShader.Dispatch(0, 848/8, 480/8, 1);
		ComputeBuffer.CopyCount(vertexBuffer, argsBuffer, 0);
	}
 
	void OnRenderObject()
	{	
		if (material == null)
		{
			material = new Material(pointCloudShader);
			material.hideFlags = HideFlags.DontSave;
			material.SetBuffer("vertices", vertexBuffer);
		}

		material.SetPass(0);
		Graphics.DrawProceduralIndirectNow(MeshTopology.Points, argsBuffer);		
	}
}
