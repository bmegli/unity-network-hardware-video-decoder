# unity-network-hardware-video-decoder

Unity video streaming over custom [MLSP](https://github.com/bmegli/minimal-latency-streaming-protocol) protocol with hardware decoding.

There are two examples:
- streaming to UI element (RawImage)
- streaming to scene element (anything with texture)

This project contains native library plugin.

See also [hardware-video-streaming](https://github.com/bmegli/hardware-video-streaming) for related projects and video sources:
- [NHVE](https://github.com/bmegli/network-hardware-video-encoder) (currently Unix-like platforms only)
- [RNHVE](https://github.com/bmegli/realsense-network-hardware-video-encoder) (currently Unix-like platforms only)

## Platforms 

Unix-like operating systems (e.g. Linux).

The dependency is through [MLSP](https://github.com/bmegli/minimal-latency-streaming-protocol) socket use (easily portable).

Tested on Ubuntu 18.04.

## Hardware

Tested with:
- Intel VAAPI compatible hardware decoders ([Quick Sync Video](https://ark.intel.com/Search/FeatureFilter?productType=processors&QuickSyncVideo=true))

Also implemented (but not tested):
- AMD/ATI VAAPI compatible hardware decoders
- VDPAU compatible hardware decoders (e.g. Nvidia GPU) 
- DirectX 9 Video Acceleration (dxva2)
- DirectX 11 Video Acceleration (d3d11va)
- VideoToolbox

Hardware dependencies are introduced through [HVD](https://github.com/bmegli/hardware-video-decoder) library.

## Dependencies

Library depends on:
- [NHVD Network Hardware Video Decoder](https://github.com/bmegli/network-hardware-video-decoder)
	- [HVD Hardware Video Decoder](https://github.com/bmegli/hardware-video-decoder)
		- FFmpeg `avcodec` and `avutil` (at least 3.4 version)
	- [MLSP Minimal Latency Streaming Protocol](https://github.com/bmegli/minimal-latency-streaming-protocol)

NHVD and its dependencies are included as submodules so you only need to satifisy HVD dependencies.

Works with system FFmpeg on Ubuntu 18.04 and doesn't on 16.04 (outdated FFmpeg).

## Building Instructions

Tested on Ubuntu 18.04.

``` bash
# update package repositories
sudo apt-get update 
# get avcodec and avutil
sudo apt-get install ffmpeg libavcodec-dev libavutil-dev
# get compilers and make 
sudo apt-get install build-essential
# get cmake - we need to specify libcurl4 for Ubuntu 18.04 dependencies problem
sudo apt-get install libcurl4 cmake
# get git
sudo apt-get install git
# clone the repository with *RECURSIVE* for submodules
git clone --recursive https://github.com/bmegli/unity-network-hardware-video-decoder.git

# build the plugin shared library
cd unity-network-hardware-video-decoder
cd PluginsSource
cd network-hardware-video-decoder
mkdir build
cd build
cmake ..
make

# finally copy the native plugin library to Unity project
cp libnhvd.so ../../../Assets/Plugins/x86_64/libnhvd.so
```

## Testing

It's easiest to check first with [HVD](https://github.com/bmegli/hardware-video-decoder) what 
parameters you need to setup your hardware. Assuming you are using VAAPI device:

### Receiving (Unity) side

- Open the project in Unity
- Choose `Canvas` -> `CameraView` -> `RawImage`
- For `RawImageVideoRenderer` component define
	- `Device`
	- note the `Port`

Alternatively configure `VideoQuad` `VideoRenderer` componenent (scene, not UI streaming).

### Sending side

For a quick test you may use [NHVE](https://github.com/bmegli/network-hardware-video-encoder) procedurally generated H.264 video (recommended).

```bash
# assuming you build NHVE, port is 9766, VAAPI device is /dev/dri/renderD128
# in NHVE build directory
./nhve-stream-h264 127.0.0.1 9766 10 /dev/dri/renderD128
```

If everything went well you will see 10 seconds video (moving through grayscale).

If you have Realsense camera you may use [realsense-network-hardware-video-encoder](https://github.com/bmegli/realsense-network-hardware-video-encoder).

```bash
# assuming you build RNHVE, port is 9766, VAAPI device is /dev/dri/renderD128
# in RNHVE build directory
./realsense-nhve 127.0.0.1 9766 color 640 360 30 10 /dev/dri/renderD128
```

If everything went well you will see 10 seconds video streamed from Realsense camera.

## License

Code in this repository and my dependencies are licensed under Mozilla Public License, v. 2.0

This is similiar to LGPL but more permissive:
- you can use it as LGPL in prioprietrary software
- unlike LGPL you may compile it statically with your code

Like in LGPL, if you modify the code, you have to make your changes available.
Making a github fork with your changes satisfies those requirements perfectly.

Since you are linking to FFmpeg libraries consider also `avcodec` and `avutil` licensing.

## Additional information

### Understanding this project

- native plugin (NHVD) is responsible for:
	- receiving video data on UDP port
	- hardware video decoding
	- serving lastest video frame through easy interface
- Unity side:
	- `NHVD` script is a wrapper around native library
	- `RawImageVideoRenderer` script may be used for streaming to UI
	- `VideoRenderer` script may be used for streaming to scene object 
	- in `LateUpdate` script gets pointer to latest video frame
	- and fills the texture with video data

