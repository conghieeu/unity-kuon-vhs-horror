using System.Linq;
using UnityEngine;

namespace UHFPS.Scriptable
{
    [CreateAssetMenu(fileName = "Surface Set", menuName = "UHFPS/Surface/Surface Definition Set")]
    public class SurfaceDefinitionSet : ScriptableObject
    {
        public enum SurfaceDetection { Tag, Texture, Both }

        public SurfaceDefinition[] Surfaces;

        /// <summary>
        /// Get Surface Definition using the selected detection type.
        /// </summary>
        public SurfaceDefinition GetSurface(GameObject surfaceUnder, Vector3 hitPosition, SurfaceDetection surfaceDetection)
        {
            SurfaceDefinition surfaceDefinition = null;

            if (surfaceUnder != null)
            {
                if (surfaceUnder.TryGetComponent(out Terrain terrain))
                {
                    surfaceDefinition = GetTerrainSurface(terrain, hitPosition);
                }
                else if (surfaceDetection == SurfaceDetection.Tag)
                {
                    surfaceDefinition = GetTagSurface(surfaceUnder);
                }
                else if (surfaceDetection == SurfaceDetection.Texture)
                {
                    surfaceDefinition = GetTextureSurface(surfaceUnder);
                }
                else if (surfaceDetection == SurfaceDetection.Both)
                {
                    surfaceDefinition = GetAnySurface(surfaceUnder);
                }
            }

            // get the first surface, which should always be the default surface
            if (surfaceDefinition == null && Surfaces.Length > 0)
                surfaceDefinition = Surfaces[0];

            return surfaceDefinition;
        }

        /// <summary>
        /// Get Surface Definition which contains a specified Tag.
        /// </summary>
        public SurfaceDefinition GetSurface(string tag)
        {
            foreach (var surface in Surfaces)
            {
                if (surface.SurfaceTag == tag)
                    return surface;
            }

            return null;
        }

        /// <summary>
        /// Get Surface Definition which contains a specified Texture.
        /// </summary>
        public SurfaceDefinition GetSurface(Texture2D texture)
        {
            foreach (var surface in Surfaces)
            {
                if (surface.SurfaceTextures.Any(x => x == texture))
                    return surface;
            }

            return null;
        }

        /// <summary>
        /// Get Surface Definition which contains a specified Textures.
        /// </summary>
        public SurfaceDefinition GetSurface(Texture2D[] textures)
        {
            foreach (var surface in Surfaces)
            {
                if (surface.SurfaceTextures.Any(x => textures.Any(y => x == y)))
                    return surface;
            }

            return null;
        }

        /// <summary>
        /// Get Surface Definition by a Tag from the GameObject.
        /// </summary>
        public SurfaceDefinition GetTagSurface(GameObject gameObject)
        {
            return GetSurface(gameObject.tag);
        }

        /// <summary>
        /// Get Surface Definition by a Texture from the GameObject.
        /// </summary>
        public SurfaceDefinition GetTextureSurface(GameObject gameObject)
        {
            if (gameObject.TryGetComponent(out MeshRenderer renderer))
            {
                Texture texture = renderer.material.mainTexture;
                return GetSurface((Texture2D)texture);
            }

            return null;
        }

        /// <summary>
        /// Get Surface Definition by a Texture or Tag from the GameObject.
        /// </summary>
        public SurfaceDefinition GetAnySurface(GameObject gameObject)
        {
            if (gameObject.TryGetComponent(out MeshRenderer renderer))
            {
                Texture texture = renderer.material.mainTexture;
                SurfaceDefinition surface = GetSurface((Texture2D)texture);
                if (surface != null) return surface;
            }

            return GetSurface(gameObject.tag);
        }

        /// <summary>
        /// Get Surface Definition at the world Terrain position.
        /// </summary>
        public SurfaceDefinition GetTerrainSurface(Terrain terrain, Vector3 worldPos)
        {
            Texture2D terrainTexture = TerrainPosToTex(terrain, worldPos);

            if (terrainTexture != null)
            {
                var surfaceDetails = GetSurface(terrainTexture);
                if (surfaceDetails != null) return surfaceDetails;
            }

            return null;
        }

        private Texture2D TerrainPosToTex(Terrain terrain, Vector3 worldPos)
        {
            float[] mix = TerrainTextureMix(terrain, worldPos);
            TerrainLayer[] terrainLayers = terrain.terrainData.terrainLayers;

            float maxMix = 0;
            int maxIndex = 0;

            for (int n = 0; n < mix.Length; n++)
            {
                if (mix[n] > maxMix)
                {
                    maxIndex = n;
                    maxMix = mix[n];
                }
            }

            if (terrainLayers.Length > 0 && terrainLayers.Length >= maxIndex)
                return terrainLayers[maxIndex].diffuseTexture;

            return null;
        }

        private float[] TerrainTextureMix(Terrain terrain, Vector3 worldPos)
        {
            TerrainData terrainData = terrain.terrainData;
            Vector3 terrainPos = terrain.transform.position;

            int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
            int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

            float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);
            float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];

            for (int n = 0; n < cellMix.Length; n++)
            {
                cellMix[n] = splatmapData[0, 0, n];
            }

            return cellMix;
        }
    }
}