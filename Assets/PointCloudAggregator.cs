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

public class PointCloudAggregator : MonoBehaviour
{
	public ComputeShader aggregationShader;
	public Shader pointCloudShader;

    public int points = 2000000;

	private ComputeBuffer aggregationBuffer;
	private ComputeBuffer aggregationShaderArgsBuffer;
	
	private Material material;

	void Awake()
	{
		aggregationShaderArgsBuffer = new ComputeBuffer( 3, sizeof( int ), ComputeBufferType.IndirectArguments );
		//number of work groups in X, Y, Z dimensions
		//see https://docs.unity3d.com/560/Documentation/ScriptReference/ComputeShader.DispatchIndirect.html
		aggregationShaderArgsBuffer.SetData(new int[] {1, 1, 1}) ;

		aggregationBuffer = new ComputeBuffer(points, 2 * sizeof(float)*4, ComputeBufferType.Counter);
		aggregationShader.SetBuffer(0, "aggregatedVertices", aggregationBuffer);
        aggregationShader.SetFloat("BaselineM", 0.095f);
	}

	void OnDestroy()
	{
		if(aggregationBuffer != null)
			aggregationBuffer.Release();

		if(aggregationShaderArgsBuffer != null)
			aggregationShaderArgsBuffer.Release();

		if (material != null)
		{
			if (Application.isPlaying)			
				Destroy(material);
			else			
				DestroyImmediate(material);
		}	
	}

	public void UpdateMap (ComputeBuffer vertexBuffer, Matrix4x4 toWorld, float zDisplacement)
	{
		ComputeBuffer.CopyCount(vertexBuffer, aggregationShaderArgsBuffer, 0);
        aggregationShader.SetBuffer(0, "vertices", vertexBuffer);
        aggregationShader.SetMatrix("transform", toWorld);
        aggregationShader.SetFloat("ZMoveM", zDisplacement);
		aggregationShader.DispatchIndirect(0, aggregationShaderArgsBuffer, 0);
	}

	void OnRenderObject()
	{
		if (material == null)
		{
			material = new Material(pointCloudShader);
			material.hideFlags = HideFlags.DontSave;
		}

		material.SetPass(0);
		material.SetMatrix("transform", transform.localToWorldMatrix);
		material.SetBuffer("vertices", aggregationBuffer);
		Graphics.DrawProceduralNow(MeshTopology.Points, points, 1);
	}
}
