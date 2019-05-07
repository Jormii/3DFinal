using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public abstract class DotMeshField { }

public class DotMeshVertex : DotMeshField {

	public float v1Pos;
	public float v2Pos;
	public float v3Pos;
	public int reference;

	public DotMeshVertex (string line) {
		string[] values = line.Split (new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		v1Pos = float.Parse (values[0], System.Globalization.CultureInfo.InvariantCulture);
		v2Pos = float.Parse (values[1], System.Globalization.CultureInfo.InvariantCulture);
		v3Pos = float.Parse (values[2], System.Globalization.CultureInfo.InvariantCulture);
		reference = int.Parse (values[3]);
	}

	public override string ToString () {
		return string.Format ("VERTEX ({0}, {1}, {2}). Reference: {3}", v1Pos, v2Pos, v3Pos, reference);
	}
}

public class DotMeshTriangle : DotMeshField {

	public int v1;
	public int v2;
	public int v3;
	public int reference;

	public DotMeshTriangle (string line) {
		string[] values = line.Split (new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		v1 = int.Parse (values[0]);
		v2 = int.Parse (values[1]);
		v3 = int.Parse (values[2]);
		reference = int.Parse (values[3]);
	}

	public override string ToString () {
		return string.Format ("TRIANGLE ({0}, {1}, {2}). Reference: {3}", v1, v2, v3, reference);
	}

}

public class DotMeshTetrahedron : DotMeshField {

	public int v1;
	public int v2;
	public int v3;
	public int v4;
	public int reference;

	public DotMeshTetrahedron (string line) {
		string[] values = line.Split (new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		v1 = int.Parse (values[0]);
		v2 = int.Parse (values[1]);
		v3 = int.Parse (values[2]);
		v4 = int.Parse (values[3]);
		reference = int.Parse (values[4]);
	}

	public override string ToString () {
		return string.Format ("Tetrahedron ({0}, {1}, {2}, {3}). Reference: {4}", v1, v2, v3, v4, reference);
	}
}

public class DotMeshCorner : DotMeshField {

	public int id;

	public DotMeshCorner (string line) {
		id = int.Parse (line.Split (new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) [0]);
	}

	public override string ToString () {
		return string.Format ("CORNER {0}", id);
	}
}

public class DotMeshEdge : DotMeshField {

	public int v1;
	public int v2;
	public int reference;

	public DotMeshEdge (string line) {
		string[] values = line.Split (new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		v1 = int.Parse (values[0]);
		v2 = int.Parse (values[1]);
		reference = int.Parse (values[2]);
	}

	public override string ToString () {
		return string.Format ("EDGE ({0}, {1}). Reference: {2}", v1, v2, reference);
	}

}

/// <summary>
/// Class that parses .mesh files. Only contains one static method.
/// </summary>
/// <remarks>
/// .mesh file format: https://www.ljll.math.upmc.fr/frey/logiciels/Docmedit.dir/Docmedit.html#SECTION00031000000000000000
/// </remarks>
public class DotMeshParser {

	public const string verticesField = "Vertices";
	public const string trianglesField = "Triangles";
	public const string tetrahedronField = "Tetrahedra";
	public const string cornersField = "Corners";
	public const string edgesField = "Edges";

	/// <summary>
	/// Parses a .mesh file.
	/// </summary>
	/// <param name="dotMeshFilePath">
	/// The path to the .mesh file to be parsed.
	/// </param>
	/// <returns>
	/// A dictionary that keys a field of the .mesh file to a list with its correspondent elements.
	/// </returns>
	/// <remarks>
	/// As of now, the parser only returns the information of the vertices, triangles and tetrahedrons.
	/// </remarks>
	public static Dictionary<string, List<DotMeshField>> Parse (string dotMeshFile) {
		List<DotMeshField> vertices = null;
		List<DotMeshField> triangles = null;
		List<DotMeshField> tetrahedrons = null;
		List<DotMeshField> corners = null;
		List<DotMeshField> edges = null;

		System.IO.StreamReader inputStream = new System.IO.StreamReader (dotMeshFile);
		string line;
		string currentField = "";
		List<DotMeshField> ds = null;

		do {
			line = inputStream.ReadLine ();

			switch (line) {
				case verticesField:
					int nVertices = int.Parse (inputStream.ReadLine ());
					vertices = new List<DotMeshField> (nVertices);
					ds = vertices;
					currentField = line;
					break;
				case trianglesField:
					int nTriangles = int.Parse (inputStream.ReadLine ());
					triangles = new List<DotMeshField> (nTriangles);
					ds = triangles;
					currentField = line;
					break;
				case tetrahedronField:
					int nTetrahedrons = int.Parse (inputStream.ReadLine ());
					tetrahedrons = new List<DotMeshField> (nTetrahedrons);
					ds = tetrahedrons;
					currentField = line;
					break;
				case cornersField:
					int nCorners = int.Parse (inputStream.ReadLine ());
					corners = new List<DotMeshField> (nCorners);
					ds = corners;
					currentField = line;
					break;
				case edgesField:
					int nEdges = int.Parse (inputStream.ReadLine ());
					edges = new List<DotMeshField> (nEdges);
					ds = edges;
					currentField = line;
					break;
				default:
					if (ds != null && line.Length != 0 && !line.Contains ("#") && !line.Contains ("End")) {
						switch (currentField) {
							case verticesField:
								ds.Add (new DotMeshVertex (line));
								break;
							case trianglesField:
								ds.Add (new DotMeshTriangle (line));
								break;
							case tetrahedronField:
								ds.Add (new DotMeshTetrahedron (line));
								break;
							case cornersField:
								ds.Add (new DotMeshCorner (line));
								break;
							case edgesField:
								ds.Add (new DotMeshEdge (line));
								break;
							default:
								break;
						}
					}
					break;
			}
		} while (inputStream.Peek () != -1);

		return new Dictionary<string, List<DotMeshField>> { { verticesField, vertices },
			{ trianglesField, triangles },
			{ tetrahedronField, tetrahedrons }
		};
	}

}