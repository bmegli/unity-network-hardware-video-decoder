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

public class UNHVD
{
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct unhvd_net_config
	{
		[System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)]
		public string ip;
		public ushort port;
		public int timeout_ms;
	}

	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct unhvd_hw_config
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
	public struct unhvd_depth_config
	{	
		public float ppx;
		public float ppy;
		public float fx;    
		public float fy;
		public float depth_unit;
		public float min_margin;
		public float max_margin;
	}

	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct unhvd_frame
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
	public struct unhvd_point_cloud
	{
		public System.IntPtr data;
		public System.IntPtr colors;
		public int size;
		public int used;
	}

	/// Return Type: unhvd*
	///net_config: unhvd_net_config*
	///hw_config: unhvd_hw_config*
	///depth_config: unhvd_depth_config*
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("unhvd")]
	#endif
	public static extern System.IntPtr unhvd_init(ref unhvd_net_config net_config, [In]unhvd_hw_config[] hw_configs, int hw_size, ref unhvd_depth_config depth_config);

	///Return Type: unhvd*
	///net_config: unhvd_net_config*
	///hw_config: unhvd_hw_config*
	///depth_config: unhvd_depth_config*
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("unhvd")]
	#endif
	private static extern System.IntPtr unhvd_init(ref unhvd_net_config net_config, ref unhvd_hw_config hw_config, int hw_size, System.IntPtr depth_config) ;

	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("unhvd")]
	#endif
	public static extern System.IntPtr unhvd_init(ref unhvd_net_config net_config, [In]unhvd_hw_config[] hw_configs, int hw_size, System.IntPtr depth_config);

	public static System.IntPtr unhvd_init(ref unhvd_net_config net_config, ref unhvd_hw_config hw_config) 
	{
		return unhvd_init(ref net_config, ref hw_config, 1, System.IntPtr.Zero);
	}

	/// Return Type: void
	///n: unhvd *
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("unhvd")]
	#endif
	public static extern void unhvd_close(System.IntPtr n) ;

	/// Return Type: int
	///n: unhvd*
	///frame: unhvd_frame*
	///pc: unhvd_point_cloud*
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("unhvd")]
	#endif
	public static extern  int unhvd_get_begin(System.IntPtr n, ref unhvd_frame frame, ref unhvd_point_cloud pc) ;

	/// Return Type: int
	///n: unhvd *
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("unhvd")]
	#endif
	public static extern int unhvd_get_end(System.IntPtr n) ;

	/// Return Type: int
	///n: void*
	///frame: unhvd_frame*
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("unhvd")]
	#endif
	public static extern int unhvd_get_frame_begin(System.IntPtr n, ref unhvd_frame frame);

	/// Return Type: int
	///n: void*
	///frame: unhvd_frame*
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("unhvd")]
	#endif
	public static extern int unhvd_get_frame_begin(System.IntPtr n, [In, Out]unhvd_frame[] frames);

	/// Return Type: int
	///n: unhvd *
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("unhvd")]
	#endif
	public static extern int unhvd_get_frame_end(System.IntPtr n) ;

	/// Return Type: int
	///n: void*
	///pc: unhvd_point_cloud*
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("unhvd")]
	#endif
	public static extern int unhvd_get_point_cloud_begin(System.IntPtr n, ref unhvd_point_cloud pc);

	/// Return Type: int
	///n: unhvd *
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("unhvd")]
	#endif
	public static extern int unhvd_get_point_cloud_end(System.IntPtr n) ;
}
