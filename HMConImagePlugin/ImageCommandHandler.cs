﻿using ASCReader;
using ASCReader.Export;
using ASCReader.Util;
using System;
using System.Collections.Generic;
using static ASCReader.Program;

namespace ASCReaderImagePlugin {
	public class ImageCommandHandler : ASCReaderCommandHandler {
		public override void AddCommands(List<ConsoleCommand> list) {
			list.Add(new ConsoleCommand("preview", "", "Previews the grid data in an image", this));
			list.Add(new ConsoleCommand("preview-hm", "", "Previews the grid data in a heightmap", this));
		}

		public override void HandleCommand(string cmd, string[] args, ExportOptions options, ASCData data) {
			if(cmd == "preview-hm") {
				WriteLine("Opening preview...");
				Previewer.OpenDataPreview(data, exportOptions, true);
			} else if(cmd == "preview") {
				WriteLine("Opening preview...");
				Previewer.OpenDataPreview(data, exportOptions, false);
			}
		}
	}
}
