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
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
public class PointCloudRenderer : MonoBehaviour
{
	public string device = "/dev/dri/renderD128";
	public string ip = "";
	public ushort port = 9766;

	private IntPtr nhvd;
	private NHVD.nhvd_frame frame = new NHVD.nhvd_frame{ data=new System.IntPtr[3], linesize=new int[3] };
	private NHVD.nhvd_point_cloud point_cloud = new NHVD.nhvd_point_cloud {data = System.IntPtr.Zero, size=0, used=0};

	const int MAX_VERTICES=848*480;
	void Awake()
	{
		NHVD.nhvd_hw_config hw_config = new NHVD.nhvd_hw_config{hardware="vaapi", codec="hevc", device=this.device, pixel_format="p010le", width=848, height=480, profile=2};
		NHVD.nhvd_net_config net_config = new NHVD.nhvd_net_config{ip=this.ip, port=this.port, timeout_ms=500 };

		nhvd=NHVD.nhvd_init (ref net_config, ref hw_config);

		if (nhvd == IntPtr.Zero)
		{
			Debug.Log ("failed to initialize NHVD");
			gameObject.SetActive (false);
		}

		Mesh mesh = GetComponent<MeshFilter>().mesh;
		mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
	
		Vector3[] vertices = new Vector3[MAX_VERTICES];

		for(int i=0;i<MAX_VERTICES;i++)
			vertices[i] = new Vector3(i/1000f, i/1000f, i/1000f);
		mesh.vertices = vertices;


		int[] indices = new int[MAX_VERTICES];
		for(int i=0;i<MAX_VERTICES;++i)
			indices[i] = i;
		mesh.SetIndices(indices, MeshTopology.Points,0);

	}
	void OnDestroy()
	{
		NHVD.nhvd_close (nhvd);
	}
	NativeArray<Vector3> pc;
	void LateUpdate ()
	{
		if (NHVD.nhvd_get_point_cloud_begin(nhvd, ref point_cloud) == 0)
		{
			unsafe
			{
				pc = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Vector3>(point_cloud.data.ToPointer(), point_cloud.size, Allocator.None);
				NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref pc, AtomicSafetyHandle.Create());
				int points = Math.Min(MAX_VERTICES, point_cloud.size);

				GetComponent<MeshFilter>().mesh.SetVertices(pc, 0, points);
			}
		}

		if (NHVD.nhvd_get_point_cloud_end (nhvd) != 0)
			Debug.LogWarning ("Failed to get NHVD point cloud data");
	}
}
