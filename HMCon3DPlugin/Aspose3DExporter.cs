using HMCon;
using HMCon.Export;
using Aspose.ThreeD;
using Aspose.ThreeD.Entities;
//using Aspose.ThreeD.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace HMCon3D {
	public class Aspose3DExporter : IExporter {

		private readonly Scene scene;

		public Aspose3DExporter(List<(List<Vector3> verts, List<int> tris, List<Vector2> uvs)> meshInfo) {
			try {
				bool makeChildNodes = meshInfo.Count > 1;
				scene = new Scene();
				for(int i = 0; i < meshInfo.Count; i++) {
					var (verts, tris, uvs) = meshInfo[i];
					Mesh m = new Mesh();
					foreach(Vector3 v in meshInfo[i].verts) m.ControlPoints.Add(new Aspose.ThreeD.Utilities.Vector4(v.X, v.Y, v.Z, 1));
					for(int j = 0; j < tris.Count; j += 3) {
						m.CreatePolygon(tris[j], tris[j + 1], tris[j + 2]);
					}
					var elem = m.CreateElementUV(TextureMapping.Diffuse, MappingMode.PolygonVertex, ReferenceMode.Direct);
					var uv = new List<Aspose.ThreeD.Utilities.Vector4>();
					for(int k = 0; k < meshInfo[i].uvs.Count; k++) {
						uv.Add(new Aspose.ThreeD.Utilities.Vector4(meshInfo[i].uvs[k].X, meshInfo[i].uvs[k].Y, 0, 1));
					}
					elem.Data.AddRange(uv);
					elem.Indices.AddRange(meshInfo[i].tris);
					if(makeChildNodes) {
						Node n = new Node("mesh" + (i + 1), m);
						scene.RootNode.ChildNodes.Add(n);
					} else {
						Node n = new Node("mesh", m);
						scene.RootNode.ChildNodes.Add(n);
					}
				}
			} catch(Exception e) {
				ConsoleOutput.WriteError("ERROR while creating 3D data for Aspose3D:");
				ConsoleOutput.WriteLine(e.ToString());
				throw e;
			}
		}

		public bool NeedsFileStream(HMCon.FileFormat format) {
			return true;
		}

		public void WriteFile(FileStream stream, string path, HMCon.FileFormat filetype) {
			if(filetype.IsFormat("3DM_3DS")) {
				scene.Save(stream, Aspose.ThreeD.FileFormat.Discreet3DS);
			} else if(filetype.IsFormat("3DM_FBX")) {
				scene.Save(stream, Aspose.ThreeD.FileFormat.FBX7300ASCII);
			}
		}
	}
}