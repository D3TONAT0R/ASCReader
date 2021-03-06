
using HMCon;
using HMCon.Export;
using HMCon.Util;
using HMConImage;
using HMConMC.PostProcessors;
using HMConMCPlugin;
using Ionic.Zlib;
using MCUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static MCUtils.NBTContent;

namespace HMConMC
{
	public class MCWorldExporter : IExporter
	{

		public static readonly string defaultBlock = "minecraft:stone";

		public World world;
		public byte[,] heightmap;

		public int regionNumX;
		public int regionNumZ;

		public int regionOffsetX;
		public int regionOffsetZ;

		public int heightmapLengthX;
		public int heightmapLengthZ;

		public Bounds worldBounds;

		public SplatmappedSurfacePostProcessor postProcessor = null;

		public MCWorldExporter(ExportJob job)
		{
			regionOffsetX = job.exportNumX + job.settings.GetCustomSetting("mcaOffsetX", 0);
			regionOffsetZ = job.exportNumZ + job.settings.GetCustomSetting("mcaOffsetZ", 0);
			int xmin = regionOffsetX * 512;
			int zmin = regionOffsetZ * 512;
			var hmapFlipped = job.data.GetDataGridFlipped();
			heightmapLengthX = hmapFlipped.GetLength(0);
			heightmapLengthZ = hmapFlipped.GetLength(1);
			worldBounds = new Bounds(xmin, zmin, xmin + heightmapLengthX - 1, zmin + heightmapLengthZ - 1);
			heightmap = new byte[heightmapLengthX, heightmapLengthZ];
			for (int x = 0; x < heightmapLengthX; x++)
			{
				for (int z = 0; z < heightmapLengthZ; z++)
				{
					heightmap[x, z] = (byte)MathUtils.Clamp((float)Math.Round(hmapFlipped[x, z], MidpointRounding.AwayFromZero), 0, 255);
				}
			}
			regionNumX = (int)Math.Ceiling(heightmapLengthX / 512f);
			regionNumZ = (int)Math.Ceiling(heightmapLengthZ / 512f);
			if (heightmapLengthX % 512 > 0 || heightmapLengthZ % 512 > 0)
			{
				ConsoleOutput.WriteWarning("Input heightmap is not a multiple of 512. Void borders will be present in the world.");
			}
		}

		public MCWorldExporter(ExportJob job, bool useDefaultPostProcessors, bool useSplatmaps) : this(job)
		{
			if (useSplatmaps)
			{
				postProcessor = new SplatmappedSurfacePostProcessor(job.data.filename, 255, regionOffsetX * 512, regionOffsetZ * 512, job.data.GridWidth, job.data.GridHeight);
			}
		}

		public bool NeedsFileStream(FileFormat format)
		{
			return format.Identifier.StartsWith("MCR");
		}

		//private void GetMCAOffset(Job job, out int offsetX, out int offsetZ) {

		//}

		private void CreateWorld()
		{
			world = new World(regionOffsetX, regionOffsetZ, regionOffsetX + regionNumX - 1, regionOffsetZ + regionNumZ - 1);
			MakeBaseTerrain();
			DecorateTerrain();
		}

		private void MakeBaseTerrain()
		{
			int progress = 0;
			int iterations = (int)Math.Ceiling(heightmapLengthX / 16f);
			Parallel.For(0, iterations, (int cx) => {
					for (int bx = 0; bx < Math.Min(16, heightmapLengthX - cx * 16); bx++)
					{
						int x = cx * 16 + bx;
						for (int z = 0; z < heightmapLengthZ; z++)
						{
							for (int y = 0; y <= heightmap[x, z]; y++)
							{
								world.SetDefaultBlock(regionOffsetX * 512 + x, y, regionOffsetZ * 512 + z);
							}
						}
					}
					progress++;
					ConsoleOutput.UpdateProgressBar("Generating base terrain", progress / (float)iterations);
				}
			);
		}

		public void DecorateTerrain()
		{
			if(postProcessor != null)
			{
				postProcessor.DecorateTerrain(this);
			}
		}

		public void WriteFile(FileStream stream, string path, FileFormat filetype)
		{
			CreateWorld();
			if (filetype.IsFormat("MCR") || filetype.IsFormat("MCR-RAW"))
			{
				world.WriteRegionFile(stream, regionOffsetX, regionOffsetZ);
			}
			else
			{
				path = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
				Directory.CreateDirectory(path);
				var mapPath = Path.Combine(path, "overviewmap.png");
				using (var mapStream = new FileStream(mapPath, FileMode.Create))
				{
					var mapExporter = new OverviewmapExporter(this, true);
					mapExporter.WriteFile(mapStream, mapPath, null);
				}
				mapPath = Path.Combine(path, "overviewmap_no-water.png");
				using (var mapStream = new FileStream(mapPath, FileMode.Create))
				{
					var mapExporter = new OverviewmapExporter(this, true, HeightmapType.SolidBlocksNoLiquid);
					mapExporter.WriteFile(mapStream, mapPath, null);
				}
				world.WriteWorldSave(path, regionOffsetX * 512 + 50, regionOffsetZ * 512 + 50);
			}
		}

		public short[,] GetHeightmap(HeightmapType type, bool keepFlippedZ)
		{
			var hm = world.GetHeightmap(worldBounds.xMin, worldBounds.yMin, worldBounds.xMax, worldBounds.yMax, type);
			if (!keepFlippedZ)
			{
				hm = ArrayConverter.Flip(hm);
			}
			return hm;
		}
	}
}