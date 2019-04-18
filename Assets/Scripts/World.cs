using UnityEngine;

public class World : MonoBehaviour
{
    public Chunk[] chunks;
    public int worldSize = 5;
    public Chunk chunkPrefab;
    public Texture2D heightmapRaw;
    public static byte[] heightmap;

    public int smoothingFactor = 5;

    private void Awake()
    {
        DecodeHeightmap();
        chunks = new Chunk[worldSize * worldSize];

        for (int x = 0; x < worldSize; x++)
        {
            for (int z = 0; z < worldSize; z++)
            {
                chunks[x + z * worldSize] = Instantiate(chunkPrefab, new Vector3(x * 16, 0f, z * 16), Quaternion.identity, transform);
            }
        }

        InitializeWorld();
        SmoothWorld();
        RenderWorld();
    }

    public void DecodeHeightmap()
    {
        Color32[] colors = heightmapRaw.GetPixels32();
        heightmap = new byte[colors.Length];

        for (int i = 0; i < colors.Length; i++)
        {
            heightmap[i] = colors[i].r;
        }
    }

    public void InitializeWorld()
    {
        for (int i = 0; i < chunks.Length; i++)
        {
            chunks[i].Initialize();
        }
    }

    public void SmoothWorld()
    {
        for (int i = 0; i < chunks.Length; i++)
        {
            chunks[i].Smooth(smoothingFactor);
        }
    }

    public void RenderWorld()
    {
        for (int i = 0; i < chunks.Length; i++)
        {
            chunks[i].Render();
        }
    }
}