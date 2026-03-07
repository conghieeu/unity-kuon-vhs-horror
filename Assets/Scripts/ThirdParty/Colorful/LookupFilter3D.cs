using UnityEngine;

namespace Colorful
{
	[HelpURL("http://www.thomashourdel.com/colorful/doc/color-correction/lookup-filter-3d.html")]
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Colorful FX/Color Correction/Lookup Filter 3D")]
	public class LookupFilter3D : MonoBehaviour
	{
		[Tooltip("The lookup texture to apply. Read the documentation to learn how to create one.")]
		public Texture2D LookupTexture;

		[Range(0f, 1f)]
		[Tooltip("Blending factor.")]
		public float Amount = 1f;

		[Tooltip("The effect will automatically detect the correct shader to use for the device but you can force it to only use the compatibility shader.")]
		public bool ForceCompatibility;

		protected Texture3D m_Lut3D;

		protected string m_BaseTextureName;

		protected bool m_Use2DLut;

		public Shader Shader2D;

		public Shader Shader3D;

		protected Material m_Material2D;

		protected Material m_Material3D;

		public Shader Shader2DSafe
		{
			get
			{
				if (Shader2D == null)
				{
					Shader2D = Shader.Find("Hidden/Colorful/Lookup Filter 2D");
				}
				return Shader2D;
			}
		}

		public Shader Shader3DSafe
		{
			get
			{
				if (Shader3D == null)
				{
					Shader3D = Shader.Find("Hidden/Colorful/Lookup Filter 3D");
				}
				return Shader3D;
			}
		}

		public Material Material
		{
			get
			{
				if (m_Use2DLut || ForceCompatibility)
				{
					if (m_Material2D == null)
					{
						m_Material2D = new Material(Shader2DSafe)
						{
							hideFlags = HideFlags.HideAndDontSave
						};
					}
					return m_Material2D;
				}
				if (m_Material3D == null)
				{
					m_Material3D = new Material(Shader3DSafe)
					{
						hideFlags = HideFlags.HideAndDontSave
					};
				}
				return m_Material3D;
			}
		}

		protected virtual void Start()
		{
			if (!SystemInfo.supports3DTextures)
			{
				m_Use2DLut = true;
			}
			if (!Shader2DSafe || !Shader2D.isSupported)
			{
				Debug.LogWarning("The shader is null or unsupported on this device");
				base.enabled = false;
			}
			else if (!m_Use2DLut && !ForceCompatibility && (!Shader3DSafe || !Shader3D.isSupported))
			{
				m_Use2DLut = true;
			}
		}

		protected virtual void OnDisable()
		{
			if ((bool)m_Material2D)
			{
				Object.DestroyImmediate(m_Material2D);
			}
			if ((bool)m_Material3D)
			{
				Object.DestroyImmediate(m_Material3D);
			}
			if ((bool)m_Lut3D)
			{
				Object.DestroyImmediate(m_Lut3D);
			}
			m_BaseTextureName = "";
		}

		protected virtual void Reset()
		{
			m_BaseTextureName = "";
		}

		protected void SetIdentityLut()
		{
			int num = 16;
			Color[] array = new Color[num * num * num];
			float num2 = 1f / (1f * (float)num - 1f);
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num; j++)
				{
					for (int k = 0; k < num; k++)
					{
						array[i + j * num + k * num * num] = new Color((float)i * 1f * num2, (float)j * 1f * num2, (float)k * 1f * num2, 1f);
					}
				}
			}
			if ((bool)m_Lut3D)
			{
				Object.DestroyImmediate(m_Lut3D);
			}
			m_Lut3D = new Texture3D(num, num, num, TextureFormat.ARGB32, mipChain: false)
			{
				hideFlags = HideFlags.HideAndDontSave
			};
			m_Lut3D.SetPixels(array);
			m_Lut3D.Apply();
			m_BaseTextureName = "";
		}

		public bool ValidDimensions(Texture2D tex2D)
		{
			if (tex2D == null || tex2D.height != Mathf.FloorToInt(Mathf.Sqrt(tex2D.width)))
			{
				return false;
			}
			return true;
		}

		protected void ConvertBaseTexture()
		{
			if (!ValidDimensions(LookupTexture))
			{
				Debug.LogWarning("The given 2D texture " + LookupTexture.name + " cannot be used as a 3D LUT. Pick another texture or adjust dimension to e.g. 256x16.");
				return;
			}
			m_BaseTextureName = LookupTexture.name;
			int height = LookupTexture.height;
			Color[] pixels = LookupTexture.GetPixels();
			Color[] array = new Color[pixels.Length];
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < height; j++)
				{
					for (int k = 0; k < height; k++)
					{
						int num = height - j - 1;
						array[i + j * height + k * height * height] = pixels[k * height + i + num * height * height];
					}
				}
			}
			if ((bool)m_Lut3D)
			{
				Object.DestroyImmediate(m_Lut3D);
			}
			m_Lut3D = new Texture3D(height, height, height, TextureFormat.ARGB32, mipChain: false)
			{
				hideFlags = HideFlags.HideAndDontSave,
				wrapMode = TextureWrapMode.Clamp
			};
			m_Lut3D.SetPixels(array);
			m_Lut3D.Apply();
		}

		public void Apply(Texture source, RenderTexture destination)
		{
			if (source is RenderTexture)
			{
				OnRenderImage(source as RenderTexture, destination);
				return;
			}
			RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height);
			Graphics.Blit(source, temporary);
			OnRenderImage(temporary, destination);
			RenderTexture.ReleaseTemporary(temporary);
		}

		protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (LookupTexture == null || Amount <= 0f)
			{
				Graphics.Blit(source, destination);
			}
			else if (m_Use2DLut || ForceCompatibility)
			{
				RenderLut2D(source, destination);
			}
			else
			{
				RenderLut3D(source, destination);
			}
		}

		protected virtual void RenderLut2D(RenderTexture source, RenderTexture destination)
		{
			float num = Mathf.Sqrt(LookupTexture.width);
			Material.SetTexture("_LookupTex", LookupTexture);
			Material.SetVector("_Params1", new Vector3(1f / (float)LookupTexture.width, 1f / (float)LookupTexture.height, num - 1f));
			Material.SetVector("_Params2", new Vector2(Amount, 0f));
			Graphics.Blit(source, destination, Material, CLib.IsLinearColorSpace() ? 1 : 0);
		}

		protected virtual void RenderLut3D(RenderTexture source, RenderTexture destination)
		{
			if (LookupTexture.name != m_BaseTextureName)
			{
				ConvertBaseTexture();
			}
			if (m_Lut3D == null)
			{
				SetIdentityLut();
			}
			Material.SetTexture("_LookupTex", m_Lut3D);
			float num = m_Lut3D.width;
			Material.SetVector("_Params", new Vector3((num - 1f) / (1f * num), 1f / (2f * num), Amount));
			Graphics.Blit(source, destination, Material, CLib.IsLinearColorSpace() ? 1 : 0);
		}
	}
}
