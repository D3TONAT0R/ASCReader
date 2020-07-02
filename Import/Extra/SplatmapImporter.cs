using System;
using System.Drawing;
using System.IO;
using ASCReader;

public static class SplatmapImporter {

	public static Random random = new Random();

	public static byte[,] GetFixedSplatmap(string path, SplatmapMapping[] mappings, int ditherLimit) {
		var splat = GetBitmap(path);
		byte[,] map = new byte[splat.Width, splat.Height];
		for(int x = 0; x < splat.Width; x++) {
			for(int y = 0; y < splat.Height; y++) {
				Color c = splat.GetPixel(x,y);
				SplatmapMapping mapping;
				if(ditherLimit > 1) {
					mapping = GetDitheredMapping(c, mappings, ditherLimit);
				} else {
					mapping = GetClosestMapping(c, mappings);
				}
				map[x,y] = (byte)mapping.value;
			}
		}
		return map;
	}


	static Bitmap GetBitmap(string path) {
		FileStream stream = File.Open(path, FileMode.Open);
		return new Bitmap(stream);
	}

	static SplatmapMapping GetClosestMapping(Color c, SplatmapMapping[] mappings) {
		int[] deviations = new int[mappings.Length];
		for(int i = 0; i < mappings.Length; i++) {
			deviations[i] += Math.Abs(c.R-mappings[i].color.R);
			deviations[i] += Math.Abs(c.G-mappings[i].color.G);
			deviations[i] += Math.Abs(c.B-mappings[i].color.B);
		}
		int index = -1;
		int closest = 999;
		for(int i = 0; i < mappings.Length; i++) {
			if(deviations[i] < closest) {
				index = i;
				closest = deviations[i];
			}
		}
		return mappings[index];
	}

	static SplatmapMapping GetDitheredMapping(Color c, SplatmapMapping[] mappings, int ditherLimit) {
		float[] probs = new float[mappings.Length];
		for(int i = 0; i < mappings.Length; i++) {
			int deviation = 0;
			deviation += Math.Abs(c.R-mappings[i].color.R);
			deviation += Math.Abs(c.G-mappings[i].color.G);
			deviation += Math.Abs(c.B-mappings[i].color.B);
			if(deviation >= ditherLimit) {
				probs[i] = 0;
			} else {
				probs[i] = 1-(deviation/(float)ditherLimit);
			}
		}
		float max = 0;
		foreach(float p in probs) max += p;
		double d = random.NextDouble()*max;
		double v = 0;
		for(int i = 0; i < probs.Length; i++) {
			v += probs[i];
			if(d < v) return mappings[i];
		}
		return mappings[0];
	}
}