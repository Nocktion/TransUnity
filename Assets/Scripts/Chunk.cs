using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public sbyte[] cells;
    public const int chunkHeight = 256;
    public const int chunkSize = 16;
    public bool initialized;
    public World world;
    public int lowestPoint, highestPoint;

    public int posX, posZ;
    public int gposX, gposZ;

    private void Awake()
    {
        world = GetComponentInParent<World>();
        gposX = (int)transform.position.x;
        gposZ = (int)transform.position.z;
        posX = gposX / 16;
        posZ = gposZ / 16;
    }

    public void Initialize()
    {
        if (!initialized)
        {
            cells = new sbyte[chunkSize * chunkHeight * chunkSize];

            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    int height = World.heightmap[(gposX + x) + (gposZ + z) * world.worldSize];
                    if (lowestPoint + 3 > height)
                    {
                        lowestPoint = height - 3;
                    }

                    if (highestPoint - 5 < height)
                    {
                        highestPoint = height + 5;
                    }

                    for (int y = 0; y < chunkHeight; y++)
                    {
                        if (y < height)
                        {
                            cells[x + (y * chunkSize) + (z * chunkSize * chunkHeight)] = (sbyte)-height;
                        }
                        else if (y == height)
                        {
                            cells[x + (y * chunkSize) + (z * chunkSize * chunkHeight)] = 0;
                        }
                        else
                        {
                            cells[x + (y * chunkSize) + (z * chunkSize * chunkHeight)] = (sbyte)height;
                        }
                    }
                }
            }
            initialized = true;
        }
    }

    public void Smooth(int smoothingFactor)
    {
        for (int i = 0; i < smoothingFactor; i++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    for (int y = lowestPoint; y < highestPoint; y++)
                    {
                        cells[x + (y * chunkSize) + (z * chunkSize * chunkHeight)] = (sbyte)((GetCell(x, y, z) + GetCell(x - 1, y, z) + GetCell(x + 1, y, z) + GetCell(x, y, z - 1) + GetCell(x, y, z + 1)) / 5);
                    }
                }
            }
        }
    }

    public void Render()
    {
        GetComponent<MeshFilter>().mesh = GetComponent<MeshCollider>().sharedMesh = Polygonization.Polygonize(this);
    }

    public void Clear()
    {
        if (initialized)
        {
            cells = null;
            GetComponent<MeshFilter>().mesh = GetComponent<MeshCollider>().sharedMesh = null;
            initialized = false;
        }
    }

    public sbyte GetCell(int x, int y, int z)
    {
        if (x >= 0 && x < chunkSize && y >= 0 && y < chunkHeight && z >= 0 && z < chunkSize)
        {
            return cells[x + (y * chunkSize) + (z * chunkSize * chunkHeight)];
        }
        else
        {
            return GetNeighborCell(x, y, z);
        }
    }

    private sbyte GetNeighborCell(int x, int y, int z)
    {
        int cx = posX;
        int cz = posZ;
        y = y < chunkHeight - 1 ? y : chunkHeight - 1;
        y = y > 0 ? y : 0;

        if (x >= chunkSize)
        {
            cx = (byte)(cx >= world.worldSize - 1 ? world.worldSize - 1 : cx + 1);
            x = 0;
        }

        if (z >= chunkSize)
        {
            cz = (byte)(cz >= world.worldSize - 1 ? world.worldSize - 1 : cz + 1);
            z = 0;
        }

        if (x < 0)
        {
            cx = (byte)(cx <= 0 ? 0 : cx - 1);
            x = chunkSize - 1;
        }

        if (z < 0)
        {
            cz = (byte)(cz <= 0 ? 0 : cz - 1);
            z = chunkSize - 1;
        }

        Chunk chunk = world.chunks[cx + cz * world.worldSize];
        return chunk.GetCell(x, y, z);
    }
}
