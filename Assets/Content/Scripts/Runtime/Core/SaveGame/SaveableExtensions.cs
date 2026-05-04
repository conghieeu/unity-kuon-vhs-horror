using UnityEngine;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    public struct SaveableVector2
    {
        public float x;
        public float y;

        public SaveableVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public SaveableVector2(Vector2 vector)
        {
            x = vector.x;
            y = vector.y;
        }
    }

    public struct SaveableVector2Int
    {
        public int x;
        public int y;

        public SaveableVector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public SaveableVector2Int(Vector2Int vector)
        {
            x = vector.x;
            y = vector.y;
        }
    }

    public struct SaveableVector3
    {
        public float x;
        public float y;
        public float z;

        public SaveableVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public SaveableVector3(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }
    }

    public struct SaveableVector3Int
    {
        public int x;
        public int y;
        public int z;

        public SaveableVector3Int(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public SaveableVector3Int(Vector3Int vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }
    }

    public static class SaveableExtensions
    {
        public static SaveableVector2 ToSaveable(this Vector2 vector)
        {
            return new SaveableVector2(vector);
        }

        public static SaveableVector2Int ToSaveable(this Vector2Int vector)
        {
            return new SaveableVector2Int(vector);
        }

        public static SaveableVector3 ToSaveable(this Vector3 vector)
        {
            return new SaveableVector3(vector);
        }

        public static SaveableVector3Int ToSaveable(this Vector3Int vector)
        {
            return new SaveableVector3Int(vector);
        }

        /// <summary>
        /// Add basic transform properties to a storable collection. (position, rotation, scale)
        /// </summary>
        public static StorableCollection WithTransform(this StorableCollection storableCollection, Transform transform, bool includeScale = false)
        {
            storableCollection.Add("position", transform.position.ToSaveable());
            storableCollection.Add("rotation", transform.eulerAngles.ToSaveable());
            if(includeScale) storableCollection.Add("scale", transform.localScale.ToSaveable());
            return storableCollection;
        }

        /// <summary>
        /// Load basic transform properties. (position, rotation, scale)
        /// </summary>
        public static void LoadTransform(this JToken token, Transform transform)
        {
            Vector3 position = token["position"].ToObject<Vector3>();
            transform.position = position;

            Vector3 rotation = token["rotation"].ToObject<Vector3>();
            transform.eulerAngles = rotation;

            if (token["scale"] != null)
            {
                Vector3 scale = token["scale"].ToObject<Vector3>();
                transform.localScale = scale;
            }
        }
    }
}