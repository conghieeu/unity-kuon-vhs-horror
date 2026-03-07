using System.Collections.Generic;
using UnityEngine;
using ch.sycoforge.Decal;
using ch.sycoforge.Decal.Projectors.Geometry;

public class RuntimeDecalCombiner
{
	public static List<GameObject> Combine(IList<EasyDecal> decals)
	{
		Dictionary<DecalTextureAtlas, List<EasyDecal>> dictionary = new Dictionary<DecalTextureAtlas, List<EasyDecal>>();
		foreach (EasyDecal decal in decals)
		{
			if (decal.Source == SourceMode.Atlas && decal.Projector != null)
			{
				if (!dictionary.ContainsKey(decal.Atlas))
				{
					dictionary.Add(decal.Atlas, new List<EasyDecal>());
				}
				dictionary[decal.Atlas].Add(decal);
			}
		}
		return Combine(dictionary);
	}

	private static List<GameObject> Combine(Dictionary<DecalTextureAtlas, List<EasyDecal>> mappings)
	{
		List<GameObject> list = new List<GameObject>();
		if (mappings.Count > 0)
		{
			foreach (DecalTextureAtlas key in mappings.Keys)
			{
				IList<EasyDecal> list2 = mappings[key];
				foreach (EasyDecal item in list2)
				{
					_ = item;
					GameObject gameObject = Combine(list2, key);
					if (gameObject != null)
					{
						list.Add(gameObject);
					}
				}
			}
		}
		return list;
	}

	private static GameObject Combine(IList<EasyDecal> decals, DecalTextureAtlas atlas)
	{
		if (decals.Count > 0)
		{
			DynamicMesh dynamicMesh = new DynamicMesh(DecalBase.DecalRoot, RecreationMode.Always);
			GameObject gameObject = new GameObject($"Combined Decals Root [{atlas.name}]");
			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			foreach (EasyDecal decal in decals)
			{
				if (decal.Source == SourceMode.Atlas && decal.Projector != null)
				{
					dynamicMesh.Add(decal.Projector.Mesh, decal.LocalToWorldMatrix, gameObject.transform.worldToLocalMatrix);
					decal.gameObject.SetActive(value: false);
				}
			}
			meshRenderer.material = atlas.Material;
			meshFilter.sharedMesh = dynamicMesh.ConvertToMesh(null);
		}
		return null;
	}
}
