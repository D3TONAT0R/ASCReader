using System;
using System.Drawing;
using System.IO;

public static class SplatmapImporter {

	public static Random random = new Random();

	public static byte[,] GetFixedSplatmap(string path, Color[] mappings, int ditherLimit, int offsetX, int offsetZ, int sizeX, int sizeZ) {
		var splat = GetBitmap(path);
		byte[,] map = new byte[sizeX, sizeZ];
		for(int x = 0; x < sizeX; x++) {
			for(int y = 0; y < sizeZ; y++) {
				Color c = splat.GetPixel(offsetX + x, offsetZ + y);
				byte mapping;
				if(ditherLimit > 1) {
					mapping = GetDitheredMapping(c, mappings, ditherLimit);
				} else {
					mapping = GetClosestMapping(c, mappings);
				}
				map[x, y] = mapping;
			}
		}
		return map;
	}

	static Bitmap GetBitmap(string path) {
		FileStream stream = File.Open(path, FileMode.Open);
		return new Bitmap(stream);
	}

	static byte GetClosestMapping(Color c, Color[] mappings) {
		int[] deviations = new int[mappings.Length];
		for(int i = 0; i < mappings.Length; i++) {
			deviations[i] += Math.Abs(c.R - mappings[i].R);
			deviations[i] += Math.Abs(c.G - mappings[i].G);
			deviations[i] += Math.Abs(c.B - mappings[i].B);
		}
		byte index = 255;
		int closest = 999;
		for(byte i = 0; i < mappings.Length; i++) {
			if(deviations[i] < closest) {
				index = i;
				closest = deviations[i];
			}
		}
		return index;
	}

	static byte GetDitheredMapping(Color c, Color[] mappings, int ditherLimit) {
		float[] probs = new float[mappings.Length];
		for(int i = 0; i < mappings.Length; i++) {
			int deviation = 0;
			deviation += Math.Abs(c.R - mappings[i].R);
			deviation += Math.Abs(c.G - mappings[i].G);
			deviation += Math.Abs(c.B - mappings[i].B);
			if(deviation >= ditherLimit) {
				probs[i] = 0;
			} else {
				probs[i] = 1 - (deviation / (float)ditherLimit);
			}
		}
		float max = 0;
		foreach(float p in probs) max += p;
		double d = random.NextDouble() * max;
		double v = 0;
		for(byte i = 0; i < probs.Length; i++) {
			v += probs[i];
			if(d < v) return i;
		}
		return 255;
	}
}