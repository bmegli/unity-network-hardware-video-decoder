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

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class RC
{
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct rc_net_config
	{
		[System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)]
		public string ip;
		public ushort port;
	}

	/// Return Type: rc*
	///net_config: rc_net_config*
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("rc")]
	#endif
	public static extern System.IntPtr rc_init(ref rc_net_config net_config);


	/// Return Type: void
	///r: rc *
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("rc")]
	#endif
	public static extern void rc_close(System.IntPtr r) ;

	/// Return Type: int
	///r: rc*
	///frame: unhvd_frame*
	///pc: unhvd_point_cloud*
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("rc")]
	#endif
	public static extern  int rc_command(System.IntPtr n, short command, short arg1, short arg2);
}
