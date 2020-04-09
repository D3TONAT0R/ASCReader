﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Text;

namespace ASCReader.Export.Exporters {
	public class ImageExporter : IExporter {

		public Bitmap image;

		ImageType imageType;
		float[,] grid;
		float gridSpacing;
		float lowValue;
		float highValue;

		Vector3[,] normals;

		public ImageExporter(float[,] cells, float cellsize, ImageType type, float blackValue, float whiteValue) {
			grid = cells;
			gridSpacing = cellsize;
			imageType = type;
			lowValue = blackValue;
			highValue = whiteValue;
			if(type == ImageType.Heightmap) MakeHeightmap();
			else if(type == ImageType.Normalmap) MakeNormalmap(true);
			else if(type == ImageType.Hillshade) MakeHillshademap();
		}

		private void MakeHeightmap() {
			image = new Bitmap(grid.GetLength(0), grid.GetLength(1));
			for(int x = 0; x < image.Width; x++) {
				for(int y = 0; y < image.Height; y++) {
					float v = (grid[x, y] - lowValue) / (highValue - lowValue);
					image.SetPixel(x, image.Height-y-1, CreateColorGrayscale(v));
				}
			}
		}

		private void CalculateNormals(bool sharpMode) {
			if(sharpMode) {
				normals = new Vector3[grid.GetLength(0), grid.GetLength(1)];
				for(int x = 0; x < image.Width; x++) {
					for(int y = 0; y < image.Height; y++) {
						float ll = GetValueAt(x,y);
						float lr = GetValueAt(x+1,y);
						float ul = GetValueAt(x,y+1);
						float ur = GetValueAt(x+1,y+1);
						float nrmX = (GetSlope(lr,ll) + GetSlope(ur,ul)) / 2f;
						float nrmY = (GetSlope(ul,ll) + GetSlope(ur,lr)) / 2f;
						float power = Math.Abs(nrmX) + Math.Abs(nrmY);
						if(power > 1) {
							nrmX /= power;
							nrmY /= power;
						}
						float nrmZ = 1f - power;
						normals[x, y] = Normalize(new Vector3(nrmX, nrmY, nrmZ));
					}
				}
			} else {
				normals = new Vector3[grid.GetLength(0)-1, grid.GetLength(1)-1];
				for(int x = 0; x < image.Width; x++) {
					for(int y = 0; y < image.Height; y++) {
						float m = GetValueAt(x, y);
						float r = GetSlope(GetValueAt(x + 1, y), m);
						float l = GetSlope(m, GetValueAt(x - 1, y));
						float u = GetSlope(GetValueAt(x, y + 1), m);
						float d = GetSlope(m, GetValueAt(x, y - 1));
						float nrmX = (r + l) / 2f;
						float nrmY = (u + d) / 2f;
						float power = Math.Abs(nrmX) + Math.Abs(nrmY);
						if(power > 1) {
							nrmX /= power;
							nrmY /= power;
						}
						float nrmZ = 1f - power;
						normals[x, y] = Normalize(new Vector3(nrmX, nrmY, nrmZ));
					}
				}
			}
		}

		private void MakeNormalmap(bool sharp) {
			if(sharp) {
				image = new Bitmap(grid.GetLength(0)-1, grid.GetLength(1)-1);
			} else {
				image = new Bitmap(grid.GetLength(0), grid.GetLength(1));
			}
			CalculateNormals(sharp);
			for(int x = 0; x < image.Width; x++) {
				for(int y = 0; y < image.Height; y++) {
					Vector3 nrm = normals[x, y];
					float r = 0.5f + nrm.X / 2f;
					float g = 0.5f + nrm.Y / 2f;
					float b = 0.5f + nrm.Z / 2f;
					image.SetPixel(x, image.Height-y-1, CreateColor(r, g, b, 1));
				}
			}
		}

		private void MakeHillshademap() {
			image = new Bitmap(grid.GetLength(0)-1, grid.GetLength(1)-1);
			CalculateNormals(true);
			for(int x = 0; x < image.Width; x++) {
				for(int y = 0; y < image.Height; y++) {
					Vector3 nrm = normals[x, y];
					float light = 0.5f + nrm.X / 2f;
					light += 0.5f + nrm.Y / 2f;
					light /= 1.4f;
					image.SetPixel(x, image.Height - y - 1, CreateColorGrayscale(light));
				}
			}
		}

		private float GetValueAt(int x, int y) {
			x = Math.Clamp(x, 0, image.Width - 1);
			y = Math.Clamp(y, 0, image.Height - 1);
			return grid[x, y];
		}

		private float GetSlope(float from, float to) {
			float hdiff = to - from;
			return (float)(Rad2Deg(Math.Atan(hdiff / gridSpacing )) / 90f);
		}

		private Vector3 Normalize(Vector3 src) {
			float power = Math.Abs(src.X) + Math.Abs(src.Y) + Math.Abs(src.Z);
			return src / power;
		}

		private Color CreateColor(float r, float g, float b, float a) {
			try {
				return Color.FromArgb(ToColorByte(a), ToColorByte(r), ToColorByte(g), ToColorByte(b));
			} catch {
				return Color.FromArgb(0,0,0,0);
			}
		}

		private int ToColorByte(float f) {
			return (int)(Math.Clamp(f, 0d, 1d) * 255d);
		}

		private Color CreateColorGrayscale(float b) {
			return CreateColor(b,b,b,1);
		}

		public void WriteFile(FileStream stream, FileFormat filetype) {
			image.Save(stream, ImageFormat.Png);
		}

		private double Rad2Deg(double rad) {
			return rad * 180f / Math.PI;
		}
	}
}
