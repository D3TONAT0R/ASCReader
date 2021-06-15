using MCUtils;
using System;

namespace HMConMC.PostProcessors {
	public class BedrockPostProcessor : PostProcessor {

		public bool flatBedrock = false;
		Random random;

		public override Priority OrderPriority => Priority.First;

		public override PostProcessType PostProcessorType => PostProcessType.Block;

		public override int BlockProcessYMin => 0;
		public override int BlockProcessYMax => flatBedrock ? 0 : 3;

		public BedrockPostProcessor() {
			random = new Random();
		}

		protected override void OnProcessBlock(MCUtils.World world, int x, int y, int z, int pass, float mask)
		{
			if(random.NextDouble() < 1f - y / 4f && !world.IsAir(x,y,z)) world.SetBlock(x, 0, z, "minecraft:bedrock");
		}
	}
}