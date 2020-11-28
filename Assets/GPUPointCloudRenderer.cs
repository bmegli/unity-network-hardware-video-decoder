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

	private ComputeBuffer vertexBuffer;
	private ComputeBuffer countBuffer;
	
	private Material material;

	void Awake()
	{
		Debug.Log("Supports R16 " + SystemInfo.SupportsTextureFormat(TextureFormat.R16));

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
			Debug.Log ("failed to initialize UNHVD");
			gameObject.SetActive (false);
			return;
		}

		vertexBuffer = new ComputeBuffer(848*480, 2 * sizeof(float)*4, ComputeBufferType.Append);
		countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
/* 
		Vector4[] positions = new Vector4[848*480];
		for(int i=0;i<positions.Length;++i)
			positions[i] = new Vector4(0, 0, 0, 1);//i/100.0f, i/100.0f, i/100.0f);

		vertexBuffer.SetData(positions);
*/
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
		Debug.Log("vertices count from buffer" + getVertexCount());
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

		//Graphics.DrawProceduralNow(MeshTopology.Points, 848*480, 1);
		Graphics.DrawProceduralNow(MeshTopology.Points, getVertexCount(), 1);
	}

}
