using System.Collections.Generic;
using UnityEngine;

public class Polygonization
{
    private const float s = 1f / 256f;

    private static int size = 64;
    private static int vcase;

    private static List<Vector3> vertices = new List<Vector3>();
    private static List<Vector3> normals = new List<Vector3>();
    private static List<int> triangles = new List<int>();
    private static List<Vector2> uvs = new List<Vector2>();

    public static readonly Vector2[] uvOffset = new Vector2[4] { new Vector2(0f, 0f), new Vector2(offset, 0f), new Vector2(0f, offset), new Vector2(offset, offset) };
    private static ushort[] localVertexMapping = new ushort[15];

    private const float offset = 1f;//0.23f;

    public static Mesh Polygonize(Chunk chunk)
    {
        vertices.Clear();
        normals.Clear();
        triangles.Clear();
        uvs.Clear();
        Mesh mesh = new Mesh();
        vcase = 0;
        Vector3Int index;

        sbyte[] density = new sbyte[8];
        Vector3[] cornerNormals = new Vector3[8];

        for (int x = 0; x < Chunk.chunkSize; x++)
        {
            for (int y = chunk.lowestPoint; y < chunk.highestPoint; y++)
            {
                for (int z = 0; z < Chunk.chunkSize; z++)
                {
                    for (int i = 0; i < density.Length; i++)
                    {
                        index = cornerIndex[i];
                        density[i] = chunk.GetCell(x + index.x, y + index.y, z + index.z);
                    }

                    byte caseCode = (byte)((density[0] >> 7 & 1)
                                    | (density[1] >> 6 & 2)
                                    | (density[2] >> 5 & 4)
                                    | (density[3] >> 4 & 8)
                                    | (density[4] >> 3 & 16)
                                    | (density[5] >> 2 & 32)
                                    | (density[6] >> 1 & 64)
                                    | (density[7] & 128));

                    if ((caseCode ^ ((density[7] >> 7) & 255)) == 0)
                    {
                        continue;
                    }

                    for (int i = 0; i < cornerNormals.Length; i++)
                    {
                        index = cornerIndex[i];
                        cornerNormals[i] = new Vector3(chunk.GetCell(x + index.x + 1, y + index.y, z + index.z) - chunk.GetCell(x + index.x - 1, y + index.y, z + index.z),
                        chunk.GetCell(x + index.x, y + index.y + 1, z + index.z) - chunk.GetCell(x + index.x, y + index.y - 1, z + index.z),
                        chunk.GetCell(x + index.x, y + index.y, z + index.z + 1) - chunk.GetCell(x + index.x, y + index.y, z + index.z - 1));
                    }

                    byte cellClass = Transvoxel.RegularCellClass[caseCode];
                    ushort[] vertexLocations = Transvoxel.RegularVertexData[caseCode];

                    long vertexCount = Geos[cellClass] >> 4;
                    long triangleCount = Geos[cellClass] & 0x0F;
                    byte[] indexOffset = Indizes[cellClass];

                    for (int i = 0; i < vertexCount; i++)
                    {
                        ushort edge = (ushort)(vertexLocations[i] & 255);

                        byte v0 = (byte)((edge >> 4) & 0x0F); //First Corner Index
                        byte v1 = (byte)(edge & 0x0F); //Second Corner Index

                        int t = (density[v1] << 8) / (density[v1] - density[v0]);
                        int u = 0x0100 - t;

                        Vector3 p0 = new Vector3((x + cornerIndex[v0].x) * t, (y + cornerIndex[v0].y) * t, (z + cornerIndex[v0].z) * t);
                        Vector3 p1 = new Vector3((x + cornerIndex[v1].x) * u, (y + cornerIndex[v1].y) * u, (z + cornerIndex[v1].z) * u);
                        Vector3 n0 = cornerNormals[v0].normalized;
                        Vector3 n1 = cornerNormals[v1].normalized;

                        normals.Add(new Vector3((n0.x * t + n1.x * u) * s, (n0.y * t + n1.y * u) * s, (n0.z * t + n1.z * u) * s));
                        vertices.Add(new Vector3((p0.x + p1.x) * s, (p0.y + p1.y) * s, (p0.z + p1.z) * s));
                        uvs.Add(uvOffset[vcase]);// uvBase + uvOffset[vcase]);

                        vcase = (byte)(vcase == 3 ? 0 : vcase + 1);

                        localVertexMapping[i] = (ushort)(vertices.Count - 1);
                    }

                    for (int t = 0; t < triangleCount; t++)
                    {
                        int tm = t * 3;
                        triangles.Add(localVertexMapping[indexOffset[tm]]);
                        triangles.Add(localVertexMapping[indexOffset[tm + 1]]);
                        triangles.Add(localVertexMapping[indexOffset[tm + 2]]);
                    }
                }
            }
        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        //mesh.RecalculateNormals();
        mesh.uv = uvs.ToArray();
        return mesh;
    }

    private static readonly Vector3Int[] cornerIndex = new Vector3Int[8] {
            new Vector3Int(0, 0, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(1, 1, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(1, 0, 1),
            new Vector3Int(0, 1, 1),
            new Vector3Int(1, 1, 1) };

    private static readonly long[] Geos = new long[16] { 0x00, 0x31, 0x62, 0x42, 0x53, 0x73, 0x93, 0x84, 0x84, 0xC4, 0x64, 0x64, 0x64, 0x64, 0x75, 0x95 };

    public static readonly byte[][] Indizes = new byte[16][] {
           new byte[] { },
           new byte[] { 0, 1, 2 },
           new byte[] { 0, 1, 2, 3, 4, 5 },
           new byte[] { 0, 1, 2, 0, 2, 3 },
           new byte[] { 0, 1, 4, 1, 3, 4, 1, 2, 3 },
           new byte[] { 0, 1, 2, 0, 2, 3, 4, 5, 6 },
           new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
           new byte[] { 0, 1, 4, 1, 3, 4, 1, 2, 3, 5, 6, 7 },
           new byte[] { 0, 1, 2, 0, 2, 3, 4, 5, 6, 4, 6, 7 },
           new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 },
           new byte[] { 0, 4, 5, 0, 1, 4, 1, 3, 4, 1, 2, 3 },
           new byte[] { 0, 5, 4, 0, 4, 1, 1, 4, 3, 1, 3, 2 },
           new byte[] { 0, 4, 5, 0, 3, 4, 0, 1, 3, 1, 2, 3 },
           new byte[] { 0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 5 },
           new byte[] { 0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 5, 0, 5, 6 },
           new byte[] { 0, 4, 5, 0, 3, 4, 0, 1, 3, 1, 2, 3, 6, 7, 8 }
        };
}