TransUnity

About:
TransUnity is a simple C# .NET implementation of Erik Lengyel's Transvoxel algorithm in Unity. It uses a heightmap to create a smooth voxel world.

Features:
- Quick world generation
- Chunks
- Easy implementation

NOTE: TransUnity does not support LOD yet, just voxel terrain creation.

IMPORTANT NOTE: THE HEIGHTMAP MUST ALWAYS HAVE AN EQUAL OR GREATER SIZE THAN: width: worldSize * chunkSize, height: worldSize * chunkSize.

Future plans:
- Texture atlas usage
- Editable terrain
- Foliage and decoration generation
- LOD support
