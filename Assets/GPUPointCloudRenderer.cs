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
using Unity.Collections.LowLevel.Unsafe;

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

	private Texture2D depthTexture;
	private Texture2D colorTexture;
	private Texture2D unprojectionTexture;

	private ComputeBuffer vertexBuffer;
	private ComputeBuffer countBuffer;
	private ComputeBuffer argsBuffer;
	
	private Material material;

	void Awake()
	{
		Debug.Log("Supports R16 " + SystemInfo.SupportsTextureFormat(TextureFormat.R16));
		Debug.Log("Supports RFloat " + SystemInfo.SupportsTextureFormat(TextureFormat.RFloat));
		Debug.Log("Supports RGBAFloat " + SystemInfo.SupportsTextureFormat(TextureFormat.RGBAFloat));

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

		vertexBuffer = new ComputeBuffer(848*480, 2 * sizeof(float)*4, ComputeBufferType.Append);
		countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);

		argsBuffer = new ComputeBuffer( 4, sizeof( int ), ComputeBufferType.IndirectArguments );
		//vertex count per instance, instance count, start vertex location, start instance location
		argsBuffer.SetData( new int[] { 0, 1, 0, 0 } );

		unprojectionShader.SetBuffer(0, "vertices", vertexBuffer);
	}

	private int getVertexCount()
	{
	    ComputeBuffer.CopyCount(vertexBuffer, countBuffer, 0);

    	int[] counter = new int[1] { 0 };
    	countBuffer.GetData(counter);
    	return counter[0];
	}

	void OnDestroy()
	{
		UNHVD.unhvd_close (unhvd);

		if(vertexBuffer != null)
			vertexBuffer.Release();

		if(countBuffer != null)
			countBuffer.Release();

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
			colorTexture = new Texture2D (frame[1].width, frame[1].height, TextureFormat.BGRA32, false);
			unprojectionShader.SetTexture(0, "colorTexture", colorTexture);
		}
 
		if(unprojectionTexture == null || unprojectionTexture.width != frame[0].width || unprojectionTexture.height != frame[0].height)
		{
			unprojectionTexture = new Texture2D(frame[0].width, frame[0].height, TextureFormat.RGBAFloat, false);
			unprojectionShader.SetTexture(0, "unprojectionTexture", unprojectionTexture);

			int width = frame[0].width;
			int height = frame[0].height;
			float ppx = 358.781f;
			float ppy = 246.297f;
			float fx = 470.941f;    
			float fy = 470.762f;
			float depth_unit = 0.0000390625f * 65472.0f;
			float min_margin = 0.19f;
			float max_margin = 0.01f;

			//[d, d, d, 1] * [(id.x - ppx)/fx * DU, (ppy - id.y) / fy * DU, DU, 1]
			float[] data = new float[width*height*4];
			for(int h = 0; h < height; h++)
				for(int w = 0; w < width; w++)
				{
					int i = 4*(h * width + w);
					data[i] = (w - ppx) / fx * depth_unit;
					data[i+1] = -(h - ppy) / fy * depth_unit;
					data[i+2] = depth_unit;
					data[i+3] = 1;
				}

			unprojectionTexture.SetPixelData(data, 0, 0);
			unprojectionTexture.Apply();
		}

	}

	void LateUpdate ()
	{
		bool updateNeeded = false;

		if (UNHVD.unhvd_get_frame_begin(unhvd, frame) == 0)
		{
			AdaptTexture();
			depthTexture.LoadRawTextureData (frame[0].data[0], frame[0].linesize[0] * frame[0].height);
			depthTexture.Apply (false);

			colorTexture.LoadRawTextureData (frame[1].data[0], frame[1].linesize[0] * frame[1].height);
			colorTexture.Apply (false);

			updateNeeded = true;
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
		// Lazy initialization
		if (material == null)
		{
			material = new Material(pointCloudShader);
			material.hideFlags = HideFlags.DontSave;
			material.SetBuffer("vertices", vertexBuffer);
		}

		material.SetPass(0);

		//int vertices = getVertexCount();
		//Debug.Log("vertices count from buffer" + vertices);
		Graphics.DrawProceduralIndirectNow(MeshTopology.Points, argsBuffer);
		
	}

}
