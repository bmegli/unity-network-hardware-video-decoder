/*
 * Unity Network Hardware Video Decoder
 * 
 * Copyright 2019-2020 (C) Bartosz Meglicki <meglickib@gmail.com>
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 *
 */

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class NHVD
{
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct nhvd_net_config
	{
		[System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)]
		public string ip;
		public ushort port;
		public int timeout_ms;
	}

	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct nhvd_hw_config
	{
		[System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)]
		public string hardware;

		[System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)]
		public string codec;

		[System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)]
		public string device;

		[System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)]
		public string pixel_format;

		public int width;
		public int height;
		public int profile;
	}

	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct nhvd_depth_config
	{	
		public float ppx;
		public float ppy;
		public float fx;    
		public float fy;
		public float depth_unit;
	}

	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct nhvd_frame
	{
		public int width;
		public int height;
		public int format;

		/// uint8t *[3]
		[System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=3, ArraySubType=System.Runtime.InteropServices.UnmanagedType.SysUInt)]
		public System.IntPtr[] data;

		/// int[3]
		[System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=3, ArraySubType=System.Runtime.InteropServices.UnmanagedType.I4)]
		public int[] linesize;
	}

	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct nhvd_point_cloud
	{
		public System.IntPtr data;
		public System.IntPtr colors;
		public int size;
		public int used;
	}

	/// Return Type: nhvd*
	///net_config: nhvd_net_config*
	///hw_config: nhvd_hw_config*
	///depth_config: nhvd_depth_config*
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("nhvd")]
	#endif
	public static extern System.IntPtr nhvd_init(ref nhvd_net_config net_config, ref nhvd_hw_config hw_config, ref nhvd_depth_config depth_config) ;

	///Return Type: nhvd*
	///net_config: nhvd_net_config*
	///hw_config: nhvd_hw_config*
	///depth_config: nhvd_depth_config*
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("nhvd")]
	#endif
	private static extern System.IntPtr nhvd_init(ref nhvd_net_config net_config, ref nhvd_hw_config hw_config, System.	IntPtr depth_config) ;

	public static System.IntPtr nhvd_init(ref nhvd_net_config net_config, ref nhvd_hw_config hw_config) 
	{
		return nhvd_init(ref net_config, ref hw_config, System.IntPtr.Zero);
	}

	/// Return Type: void
	///n: nhvd *
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("nhvd")]
	#endif
	public static extern void nhvd_close(System.IntPtr n) ;

	/// Return Type: int
	///n: nhvd*
	///frame: nhvd_frame*
	///pc: nhvd_point_cloud*
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("nhvd")]
	#endif
	public static extern  int nhvd_get_begin(System.IntPtr n, ref nhvd_frame frame, ref nhvd_point_cloud pc) ;

	/// Return Type: int
	///n: nhvd *
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("nhvd")]
	#endif
	public static extern int nhvd_get_end(System.IntPtr n) ;

	/// Return Type: int
	///n: void*
	///frame: nhvd_frame*
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("nhvd")]
	#endif
	public static extern int nhvd_get_frame_begin(System.IntPtr n, ref nhvd_frame frame);

	/// Return Type: int
	///n: nhvd *
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("nhvd")]
	#endif
	public static extern int nhvd_get_frame_end(System.IntPtr n) ;

	/// Return Type: int
	///n: void*
	///pc: nhvd_point_cloud*
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("nhvd")]
	#endif
	public static extern int nhvd_get_point_cloud_begin(System.IntPtr n, ref nhvd_point_cloud pc);

	/// Return Type: int
	///n: nhvd *
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("nhvd")]
	#endif
	public static extern int nhvd_get_point_cloud_end(System.IntPtr n) ;
}
