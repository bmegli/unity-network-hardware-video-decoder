using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class NHVD
{
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct nhvd_hw_config {

		[System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)]
		public string hardware;

		[System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)]
		public string codec;

		[System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)]
		public string device;

		[System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)]
		public string pixel_format;
	}

	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct nhvd_net_config {

		[System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)]
		public string ip;

		public ushort port;
		public int timeout_ms;
	}
		
	/// Return Type: nhvd*
	///net_config: nhvd_net_config*
	///hw_config: nhvd_hw_config*
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("nhvd")]
	#endif
	public static extern System.IntPtr nhvd_init(ref nhvd_net_config net_config, ref nhvd_hw_config hw_config) ;

	/// Return Type: void
	///n: nhvd *
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("nhvd")]
	#endif
	public static extern void nhvd_close(System.IntPtr n) ;

	/// Return Type: void*
	///n: uint8_t*
	///w: int*
	///h: int*
	///s: int*
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("nhvd")]
	#endif
	public static extern System.IntPtr nhvd_get_frame_begin(System.IntPtr n, ref int w, ref int h, ref int s) ;

	/// Return Type: int
	///n: nhvd *
	#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
	#else
	[DllImport ("nhvd")]
	#endif
	public static extern int nhvd_get_frame_end(System.IntPtr n) ;
}
