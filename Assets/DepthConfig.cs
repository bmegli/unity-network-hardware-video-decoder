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
using UnityEngine;

public class DepthConfig
{
	public float ppx;
	public float ppy;
	public float fx;    
	public float fy;
	public float depth_unit;
	public float min_margin;
	public float max_margin;
}
