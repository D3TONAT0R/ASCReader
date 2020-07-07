using System;
using ASCReader.Export.Exporters;

public class OrePostProcessor : IMinecraftTerrainPostProcessor
{

	public class Ore {

		public string block;
		public int veinSizeMax;
		public float spawnsPerBlock;
		public int heightMin;
		public int heightMax;

		public Ore(string b, int v, float r, int min, int max) {
			block = "minecraft:"+b;
			veinSizeMax = v;
			spawnsPerBlock = r;
			heightMin = min;
			heightMax = max;
		}
	}

	public OrePostProcessor(float totalRarityMul) {
		random = new Random();
		rarityMul = totalRarityMul;
	}

	public Random random;
	public static readonly Ore[] ores = new Ore[] {
		new Ore("iron_ore", 9, 1f/2500, 2, 66),
		new Ore("coal_ore", 11, 1f/1500, 10, 120),
		new Ore("gold_ore", 8, 1f/6000, 2, 32)
	};
	public float rarityMul = 1;

	public void ProcessBlock(MinecraftRegionExporter region, int x, int y, int z) {
		foreach(Ore o in ores) {
			if(random.NextDouble()*rarityMul < o.spawnsPerBlock) SpawnOre(region, o, x, y, z);
		}
	}

	private void SpawnOre(MinecraftRegionExporter region, Ore ore, int x, int y, int z) {
		for(int i = 0; i < ore.veinSizeMax; i++) {
			int x1 = x + RandomRange(-1,1);
			int y1 = y + RandomRange(-1,1);
			int z1 = z + RandomRange(-1,1);
			if(region.IsDefaultBlock(x1,y1,z1)) region.SetBlock(x1,y1,z1,ore.block);
		}
	}

	public void ProcessSurface(MinecraftRegionExporter region, int x, int y, int z)	{

	}

	private int RandomRange(int min, int max) {
		return random.Next(min,max+1);
	}

	public void OnFinish(MinecraftRegionExporter region) {

	}
}