using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace SparksToothChart.Wpf3D {
	///<summary>The winding rule of the WPF chart's meshes (docs/specs/wpf-tooth-chart-plan.md, section 5.2), with no WPF object. WPF draws only the
	///front faces of a mesh: triangles that run counter-clockwise as seen from the camera. The models' triangles are the DirectX chart's (the fan of
	///ToothGroup.GetTriangleIndices), and teeth 9-24 are the models of 1-8 and 25-32 mirrored in X when they are loaded, with their normals mirrored
	///but their winding not reversed (ToothGraphic.ImportObj). So each triangle is turned to agree with its vertex normals: when its geometric normal
	///((b-a) x (c-a)) points away from the sum of its three vertex normals, b and c swap. That reverses the mirrored teeth, and a triangle whose
	///normals face the camera then runs counter-clockwise and is kept. A mirroring model transform (negative determinant: the lingual perio rows)
	///reverses the winding on screen again, so such an instance draws the reversed list.</summary>
	public static class ToothMeshWinding {
		///<summary>The group's triangles as three indices into the tooth's VertexNormals each, every triangle turned to agree with its vertex normals.
		///A triangle with no area keeps its order.</summary>
		public static int[] GetWoundTriangles(ToothGraphic toothGraphic,int indexGroup) {
			List<int> listTriangles=toothGraphic.Groups[indexGroup].GetTriangleIndices();
			int[] arrayTriangles=listTriangles.ToArray();
			for(int i=0;i+2<arrayTriangles.Length;i+=3) {
				if(IsAgainstNormals(toothGraphic,arrayTriangles[i],arrayTriangles[i+1],arrayTriangles[i+2])) {
					int b=arrayTriangles[i+1];
					arrayTriangles[i+1]=arrayTriangles[i+2];
					arrayTriangles[i+2]=b;
				}
			}
			return arrayTriangles;
		}

		///<summary>Every triangle with its last two corners swapped: the same triangles, wound the other way.</summary>
		public static int[] GetReversedTriangles(int[] arrayTriangles) {
			int[] arrayReversed=(int[])arrayTriangles.Clone();
			for(int i=0;i+2<arrayReversed.Length;i+=3) {
				arrayReversed[i+1]=arrayTriangles[i+2];
				arrayReversed[i+2]=arrayTriangles[i+1];
			}
			return arrayReversed;
		}

		///<summary>True when the triangle's geometric normal points away from the sum of its vertex normals (negative dot product).</summary>
		public static bool IsAgainstNormals(ToothGraphic toothGraphic,int a,int b,int c) {
			double dot=GetNormalAgreement(toothGraphic,a,b,c);
			return dot<0;
		}

		///<summary>The dot product of the triangle's geometric normal ((b-a) x (c-a)) and the sum of its three vertex normals: positive when they
		///agree, zero for a triangle with no area.</summary>
		public static double GetNormalAgreement(ToothGraphic toothGraphic,int a,int b,int c) {
			Vertex3f pa=toothGraphic.VertexNormals[a].Vertex;
			Vertex3f pb=toothGraphic.VertexNormals[b].Vertex;
			Vertex3f pc=toothGraphic.VertexNormals[c].Vertex;
			double abx=(double)pb.X-pa.X;
			double aby=(double)pb.Y-pa.Y;
			double abz=(double)pb.Z-pa.Z;
			double acx=(double)pc.X-pa.X;
			double acy=(double)pc.Y-pa.Y;
			double acz=(double)pc.Z-pa.Z;
			double nx=aby*acz-abz*acy;
			double ny=abz*acx-abx*acz;
			double nz=abx*acy-aby*acx;
			Vertex3f na=toothGraphic.VertexNormals[a].Normal;
			Vertex3f nb=toothGraphic.VertexNormals[b].Normal;
			Vertex3f nc=toothGraphic.VertexNormals[c].Normal;
			double sx=(double)na.X+nb.X+nc.X;
			double sy=(double)na.Y+nb.Y+nc.Y;
			double sz=(double)na.Z+nb.Z+nc.Z;
			return nx*sx+ny*sy+nz*sz;
		}
	}

	///<summary>One group of a model as frozen WPF meshes: the triangles of ToothMeshWinding, and the same triangles reversed, with the normals
	///negated, for mirroring transforms. Each mesh holds only the vertices its triangles use (position and normal of VertexNormals), because WPF
	///lights every vertex of a mesh for every material pass; ArraySourceIndices maps a mesh vertex back to its VertexNormals index.</summary>
	public sealed class ToothGroupMesh {
		public readonly ToothGroupType GroupType;
		///<summary>For each mesh vertex, its index in the tooth's VertexNormals.</summary>
		public readonly int[] ArraySourceIndices;
		///<summary>The triangles wound to agree with the vertex normals.</summary>
		public readonly MeshGeometry3D Mesh;
		///<summary>The same positions with every triangle reversed and every normal negated, for a transform that mirrors (negative determinant: only
		///the lingual perio rows and their implants). WPF lights such a model as if its normals pointed the other way (TC-7 found the lingual rows lit
		///by the emissive part alone, with the normals unchanged), so the negated normals give it DirectX's light: DirectX transforms the normals by
		///the world matrix and lights the mirrored rows as the others.</summary>
		public readonly MeshGeometry3D MeshReversed;

		public ToothGroupMesh(ToothGroupType groupType,int[] arraySourceIndices,MeshGeometry3D mesh,MeshGeometry3D meshReversed) {
			GroupType=groupType;
			ArraySourceIndices=arraySourceIndices;
			Mesh=mesh;
			MeshReversed=meshReversed;
		}
	}

	///<summary>The frozen meshes of one model (a tooth ID or "implant").</summary>
	public sealed class ToothMeshSet {
		public readonly string ToothId;
		///<summary>The number of the model's VertexNormals: the positions the meshes take their vertices from, one for one.</summary>
		public readonly int CountVertexNormals;
		///<summary>One per group, in the order of ToothGraphic.Groups.</summary>
		public readonly ToothGroupMesh[] ArrayGroups;

		public ToothMeshSet(string toothId,int countVertexNormals,ToothGroupMesh[] arrayGroups) {
			ToothId=toothId;
			CountVertexNormals=countVertexNormals;
			ArrayGroups=arrayGroups;
		}
	}

	///<summary>The WPF chart's meshes, built from the embedded .obj models once per process on the render thread and frozen, so they can be used
	///from any thread (docs/specs/wpf-tooth-chart-plan.md, sections 5.2 and 5.4). Each model is loaded by its own ToothGraphic, made here, so the
	///render thread never reads a chart's ToothChartData; the scene's 3D items name a model by tooth ID and group index, and the groups come in the
	///same order from the same file. TC-6 adds one shape that is no tooth: ToothSceneShapes.Box, a unit box made with Helix Toolkit's MeshBuilder
	///(the brackets and the space maintainers' bars, each item scaling it by its transform). Not thread-safe: one cache belongs to one thread (the
	///renderer's).</summary>
	public sealed class ToothMeshCache {
		private readonly Dictionary<string,ToothMeshSet> _dictMeshSets=new Dictionary<string,ToothMeshSet>();

		///<summary>The meshes of the model of toothId (1-32, A-T or "implant"), loaded and frozen the first time.</summary>
		public ToothMeshSet GetMeshSet(string toothId) {
			ToothMeshSet toothMeshSet;
			if(_dictMeshSets.TryGetValue(toothId,out toothMeshSet)) {
				return toothMeshSet;
			}
			if(toothId==ToothSceneShapes.Box) {
				toothMeshSet=BuildBoxMeshSet();
			}
			else {
				toothMeshSet=BuildMeshSet(new ToothGraphic(toothId));
			}
			_dictMeshSets[toothId]=toothMeshSet;
			return toothMeshSet;
		}

		///<summary>The mesh of one group, reversed or not. Null when the model has no such group.</summary>
		public MeshGeometry3D GetMesh(string toothId,int indexGroup,bool isReversed) {
			ToothMeshSet toothMeshSet=GetMeshSet(toothId);
			if(indexGroup<0 || indexGroup>=toothMeshSet.ArrayGroups.Length) {
				return null;
			}
			ToothGroupMesh toothGroupMesh=toothMeshSet.ArrayGroups[indexGroup];
			return isReversed?toothGroupMesh.MeshReversed:toothGroupMesh.Mesh;
		}

		///<summary>The number of models loaded so far.</summary>
		public int CountMeshSets {
			get {
				return _dictMeshSets.Count;
			}
		}

		///<summary>The frozen meshes of every group of the model.</summary>
		public static ToothMeshSet BuildMeshSet(ToothGraphic toothGraphic) {
			ToothGroupMesh[] arrayGroups=new ToothGroupMesh[toothGraphic.Groups.Count];
			for(int g=0;g<toothGraphic.Groups.Count;g++) {
				arrayGroups[g]=BuildGroupMesh(toothGraphic,g);
			}
			return new ToothMeshSet(toothGraphic.ToothID,toothGraphic.VertexNormals.Count,arrayGroups);
		}

		///<summary>TC-6: the unit box (1 x 1 x 1 mm, centred on the origin) of ToothSceneShapes.Box, made with Helix Toolkit's MeshBuilder.AddBox and
		///ToMesh().ToWndMeshGeometry3D(true), the calls docs/port-notes/TC-4.md (section 13) allows. Each face has its own four vertices with the face's
		///normal, and Helix winds every triangle counter-clockwise seen from outside, so the faces toward the camera are the front faces of section 5.2
		///(WpfMeshTests checks it). As for a tooth, the reversed mesh has every triangle reversed and every normal negated, for a mirroring transform
		///(the chart has none for the box). One group, ToothGroupType.None, with no VertexNormals behind it.</summary>
		public static ToothMeshSet BuildBoxMeshSet() {
			HelixToolkit.Geometry.MeshBuilder meshBuilder=new HelixToolkit.Geometry.MeshBuilder();
			meshBuilder.AddBox(new System.Numerics.Vector3(0f,0f,0f),1f,1f,1f);
			MeshGeometry3D meshHelix=HelixToolkit.Wpf.ConverterExtensions.ToWndMeshGeometry3D(meshBuilder.ToMesh(),true);
			Point3DCollection point3DCollection=new Point3DCollection(meshHelix.Positions);
			point3DCollection.Freeze();
			Vector3DCollection vector3DCollection=new Vector3DCollection(meshHelix.Normals);
			vector3DCollection.Freeze();
			Vector3DCollection vector3DCollectionNegated=new Vector3DCollection(vector3DCollection.Count);
			for(int i=0;i<vector3DCollection.Count;i++) {
				vector3DCollectionNegated.Add(-vector3DCollection[i]);
			}
			vector3DCollectionNegated.Freeze();
			int[] arrayTriangles=new int[meshHelix.TriangleIndices.Count];
			meshHelix.TriangleIndices.CopyTo(arrayTriangles,0);
			MeshGeometry3D mesh=CreateMesh(point3DCollection,vector3DCollection,arrayTriangles);
			MeshGeometry3D meshReversed=CreateMesh(point3DCollection,vector3DCollectionNegated,ToothMeshWinding.GetReversedTriangles(arrayTriangles));
			ToothGroupMesh toothGroupMesh=new ToothGroupMesh(ToothGroupType.None,new int[0],mesh,meshReversed);
			return new ToothMeshSet(ToothSceneShapes.Box,0,new ToothGroupMesh[] { toothGroupMesh });
		}

		private static ToothGroupMesh BuildGroupMesh(ToothGraphic toothGraphic,int indexGroup) {
			int[] arrayWound=ToothMeshWinding.GetWoundTriangles(toothGraphic,indexGroup);
			//The vertices the triangles use, in the order of first use.
			Dictionary<int,int> dictLocal=new Dictionary<int,int>();
			List<int> listSource=new List<int>();
			int[] arrayLocal=new int[arrayWound.Length];
			for(int i=0;i<arrayWound.Length;i++) {
				int indexLocal;
				if(!dictLocal.TryGetValue(arrayWound[i],out indexLocal)) {
					indexLocal=listSource.Count;
					dictLocal[arrayWound[i]]=indexLocal;
					listSource.Add(arrayWound[i]);
				}
				arrayLocal[i]=indexLocal;
			}
			Point3DCollection point3DCollection=new Point3DCollection(listSource.Count);
			Vector3DCollection vector3DCollection=new Vector3DCollection(listSource.Count);
			for(int i=0;i<listSource.Count;i++) {
				VertexNormal vertexNormal=toothGraphic.VertexNormals[listSource[i]];
				point3DCollection.Add(new Point3D(vertexNormal.Vertex.X,vertexNormal.Vertex.Y,vertexNormal.Vertex.Z));
				vector3DCollection.Add(new Vector3D(vertexNormal.Normal.X,vertexNormal.Normal.Y,vertexNormal.Normal.Z));
			}
			point3DCollection.Freeze();
			vector3DCollection.Freeze();
			Vector3DCollection vector3DCollectionNegated=new Vector3DCollection(listSource.Count);
			for(int i=0;i<vector3DCollection.Count;i++) {
				vector3DCollectionNegated.Add(-vector3DCollection[i]);
			}
			vector3DCollectionNegated.Freeze();
			MeshGeometry3D mesh=CreateMesh(point3DCollection,vector3DCollection,arrayLocal);
			MeshGeometry3D meshReversed=CreateMesh(point3DCollection,vector3DCollectionNegated,ToothMeshWinding.GetReversedTriangles(arrayLocal));
			return new ToothGroupMesh(toothGraphic.Groups[indexGroup].GroupType,listSource.ToArray(),mesh,meshReversed);
		}

		private static MeshGeometry3D CreateMesh(Point3DCollection point3DCollection,Vector3DCollection vector3DCollection,int[] arrayTriangles) {
			MeshGeometry3D mesh=new MeshGeometry3D();
			mesh.Positions=point3DCollection;
			mesh.Normals=vector3DCollection;
			Int32Collection int32Collection=new Int32Collection(arrayTriangles);
			int32Collection.Freeze();
			mesh.TriangleIndices=int32Collection;
			mesh.Freeze();
			return mesh;
		}
	}
}
