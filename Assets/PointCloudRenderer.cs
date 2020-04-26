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

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PointCloudRenderer : MonoBehaviour
{
	public string device = "/dev/dri/renderD128";
	public string ip = "";
	public ushort port = 9768;

	private IntPtr nhvd;
	private NHVD.nhvd_frame frame = new NHVD.nhvd_frame{ data=new System.IntPtr[3], linesize=new int[3] };
	private NHVD.nhvd_point_cloud point_cloud = new NHVD.nhvd_point_cloud {data = System.IntPtr.Zero, size=0, used=0};

	private Mesh mesh;

	void Awake()
	{
		NHVD.nhvd_net_config net_config = new NHVD.nhvd_net_config{ip=this.ip, port=this.port, timeout_ms=500 };
		NHVD.nhvd_hw_config[] hw_config = new NHVD.nhvd_hw_config[]
		{
			new NHVD.nhvd_hw_config{hardware="vaapi", codec="hevc", device=this.device, pixel_format="p010le", width=848, height=480, profile=2},
			new NHVD.nhvd_hw_config{hardware="vaapi", codec="hevc", device=this.device, pixel_format="rgb0", width=848, height=480, profile=1}
		};

		//For depth units explanation see:
		//https://github.com/bmegli/realsense-depth-to-vaapi-hevc10/wiki/How-it-works#depth-units
		NHVD.nhvd_depth_config depth_config = new NHVD.nhvd_depth_config{ppx = 421.353f, ppy=240.93f, fx=426.768f, fy=426.768f, depth_unit = 0.0001f};
		//NHVD.nhvd_depth_config depth_config = new NHVD.nhvd_depth_config{ppx = 421.353f, ppy=240.93f, fx=426.768f, fy=426.768f, depth_unit = 0.00003125f};

		nhvd=NHVD.nhvd_init (ref net_config, hw_config, hw_config.Length, ref depth_config);

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

	private void PrepareMesh(int size)
	{
		if(mesh == null)
			mesh = new Mesh();

		if(mesh.vertexCount == size)
			return;

		mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
		Vector3[] vertices = new Vector3[size];
		for(int i=0;i<size;i++)
			vertices[i] = new Vector3();
		mesh.vertices = vertices;

		int[] indices = new int[size];
		for(int i=0;i<size;++i)
			indices[i] = i;
		mesh.SetIndices(indices, MeshTopology.Points,0);

		//we don't want to recalculate bounds for half million dynamic points so just set wide bounds
		mesh.bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(10, 10, 10));

		Color32[] colors = new Color32[size];
		for(int i=0;i<size;++i)
			colors[i] = new Color32(255, 0, 0, 255);

		mesh.SetColors(colors);

		GetComponent<MeshFilter>().mesh = mesh;
	}

	void LateUpdate ()
	{
		if (NHVD.nhvd_get_point_cloud_begin(nhvd, ref point_cloud) == 0)
		{
			PrepareMesh(point_cloud.size);

			//possible optimization - only render non-zero points (point_cloud.used)
			unsafe
			{
				NativeArray<Vector3> pc = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Vector3>(point_cloud.data.ToPointer(), point_cloud.size, Allocator.None);
				NativeArray<Color32> colors = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Color32>(point_cloud.colors.ToPointer(), point_cloud.size, Allocator.None);
				#if ENABLE_UNITY_COLLECTIONS_CHECKS
				NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref pc, AtomicSafetyHandle.Create());
				NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref colors, AtomicSafetyHandle.Create());
				#endif
				mesh.SetVertices(pc, 0, point_cloud.size);
				mesh.SetColors(colors, 0, point_cloud.size);
			}
		}

		if (NHVD.nhvd_get_point_cloud_end (nhvd) != 0)
			Debug.LogWarning ("Failed to get NHVD point cloud data");
	}
}
