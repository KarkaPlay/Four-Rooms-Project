//========================================================
//--------------|         INTERRA         |---------------
//========================================================
//--------------| ©  INEFFABILIS ARCANUM  |---------------
//========================================================


using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace InTerra
{
	public static class InTerra_Data
	{		
		static public Dictionary<Material, Terrain> materialsTerrain; //Dictionary of Materials and Terrains where are placed
		static Dictionary<Terrain, RenderTexture> normalMaps; //Render textures of Terrains mesh normals
		static Material normalsCreatorMat;

		public const string ObjectShaderName = "InTerra/Object into Terrain Integration";
		public const string terrainShaderName = "InTerra/Terrain (Standard With Features)";

		public const string DiffuseObjectShaderName = "InTerra/Diffuse/Object into Terrain Integration (Diffuse)";
		public const string DiffuseTerrainShaderName = "InTerra/Diffuse/Terrain (Diffuse With Features)";

		public static void UpdateTerrainData()
		{
			//======= DICTIONARY OF MATERIALS WITH INTERRA SHADERS AND SUM POSITIONS OF RENDERERS WITH THAT MATERIAL =========
			Dictionary<Material, Vector3> matPos = new Dictionary<Material, Vector3>();
			MeshRenderer[] renderers = Object.FindObjectsOfType<MeshRenderer>();

			foreach (Renderer rend in renderers)
			{
				if (rend != null && rend.transform.position != null)
				{
					foreach (Material mat in rend.sharedMaterials)
					{
						if (mat && mat.shader && mat.shader.name != null && (mat.shader.name == ObjectShaderName || mat.shader.name == DiffuseObjectShaderName))
						{
							if (!matPos.ContainsKey(mat))
							{
								matPos.Add(mat, new Vector3(rend.transform.position.x, rend.transform.position.z, 1));
							}
							else
							{
								Vector3 sumPos = matPos[mat];
								sumPos.x += rend.transform.position.x;
								sumPos.y += rend.transform.position.z;
								sumPos.z += 1;
								matPos[mat] = sumPos;
							}
						}
					}
				}
			}
			
			Terrain[] terrains = Terrain.activeTerrains;
			normalMaps = new Dictionary<Terrain, RenderTexture>();
			normalsCreatorMat = GetUpdaterObject().GetComponent<MeshRenderer>().sharedMaterial;

			//===================== DICTIONARY OF MATERIALS AND TERRAINS WHERE ARE PLACED =========================
			materialsTerrain = new Dictionary<Material, Terrain>();
			
			foreach (Material mat in matPos.Keys)
			{
				Vector2 averagePos = matPos[mat] / matPos[mat].z;

				foreach (Terrain terrain in terrains)
				{
					if (CheckPosition(terrain, averagePos))
					{
						materialsTerrain.Add(mat, terrain);
						if (!normalMaps.ContainsKey(terrain))
						{
							normalMaps.Add(terrain, CreateNormalsRenderTexture(terrain));							
						} 
					}
				}				
			}

			//================================================================================
			//--------------------|    SET TERRAINS DATA TO MATERIALS    |--------------------
			//================================================================================
			foreach (Material mat in materialsTerrain.Keys)
			{
				Terrain terrain = materialsTerrain[mat];

				mat.SetVector("_TerrainSize", terrain.terrainData.size); 
				mat.SetVector("_TerrainPosition", terrain.transform.position); 
				mat.SetVector("_HeightmapScale", new Vector3 (terrain.terrainData.heightmapScale.x, terrain.terrainData.heightmapScale.y / (32766.0f / 65535.0f), terrain.terrainData.heightmapScale.z));
				mat.SetTexture("_TerrainHeightmapTexture", terrain.terrainData.heightmapTexture);
				if (normalMaps.ContainsKey(terrain)) mat.SetTexture("_TerrainNormalmapTexture", normalMaps[terrain]);

				//-------------------|  InTerra Keywords  |------------------
				string[] keywords = new string[] 
				{   "_TERRAIN_MASK_MAPS",
					"_TERRAIN_BLEND_HEIGHT",
					"_TERRAIN_DISTANCEBLEND", 
					"_TERRAIN_NORMAL_IN_MASK",					
				};

				if (terrain.materialTemplate.shader == Shader.Find(terrainShaderName) || terrain.materialTemplate.shader == Shader.Find(DiffuseTerrainShaderName))
				{					
					TerrainKeywordsToMaterial(terrain, mat, keywords);

					//------------------|  InTerra Properties  |------------------
					string[] floatProperties = new string[] 
					{	"_HT_distance_scale", 
						"_HT_cover",
						"_HeightTransition",
						"_Distance_HeightTransition",
						"_TriplanarOneToAllSteep",
						"_TriplanarSharpness",
					};
					SetTerrainFloatsToMaterial(terrain, mat, floatProperties);
					SetTerrainVectorToMaterial(terrain, mat, "_HT_distance");
				}
				else
				{					
					DisableKeywords(mat, keywords); //Disable Keywords when Terrain is not using Interra Shader
				}

				bool hasNormalMap = false;
				//----------- ONE PASS ------------
				if (!mat.IsKeywordEnabled("TWO_LAYERS") && !mat.IsKeywordEnabled("ONE_LAYER"))
				{
					int passNumber = (int)mat.GetFloat("_PassNumber");

					for (int i = 0; (i + (passNumber * 4)) < terrain.terrainData.alphamapLayers && i < 4; i++)
					{
						TerrainLaeyrDataToMaterial(terrain.terrainData.terrainLayers[i + ( passNumber * 4 )], i, mat);
						hasNormalMap = terrain.terrainData.terrainLayers[i + (passNumber * 4)].normalMapTexture || hasNormalMap;
					}

					if (terrain.terrainData.alphamapTextureCount > passNumber) mat.SetTexture("_Control", terrain.terrainData.alphamapTextures[passNumber]);
					if (passNumber > 0) mat.DisableKeyword("_TERRAIN_BLEND_HEIGHT");
				}

				//----------- ONE LAYER ------------
				if (mat.IsKeywordEnabled("ONE_LAYER"))
				{
					#if UNITY_EDITOR //The TerrainLayers in Editor are referenced by GUID, in Build by TerrainLayers array index
						TerrainLayer terainLayer = TerrainLayerFromGUID(mat, "TerrainLayerGUID_1");
						TerrainLaeyrDataToMaterial(terainLayer, 0, mat);
						hasNormalMap = terainLayer.normalMapTexture;
					#else
						int layerIndex1 = (int)mat.GetFloat("_LayerIndex1");
						CheckLayerIndex(terrain, 0, mat, ref layerIndex1);
						TerrainLaeyrDataToMaterial(terrain.terrainData.terrainLayers[layerIndex1], 0, mat);	
						hasNormalMap = terrain.terrainData.terrainLayers[layerIndex1].normalMapTexture;
					#endif
				}

				//----------- TWO LAYERS ------------
				if (mat.IsKeywordEnabled("TWO_LAYERS"))
				{
					#if UNITY_EDITOR
						TerrainLayer terainLayer1 = TerrainLayerFromGUID(mat, "TerrainLayerGUID_1");
						TerrainLayer terainLayer2 = TerrainLayerFromGUID(mat, "TerrainLayerGUID_2");
						TerrainLaeyrDataToMaterial(terainLayer1, 0, mat);
						TerrainLaeyrDataToMaterial(terainLayer2, 1, mat);
						int layerIndex1 = terrain.terrainData.terrainLayers.ToList().IndexOf(terainLayer1);
						int layerIndex2 = terrain.terrainData.terrainLayers.ToList().IndexOf(terainLayer2);
						hasNormalMap = terainLayer1.normalMapTexture || terainLayer2.normalMapTexture;
					#else
						int layerIndex1 = (int)mat.GetFloat("_LayerIndex1"); 
						int layerIndex2 = (int)mat.GetFloat("_LayerIndex2");
						CheckLayerIndex(terrain, 0, mat, ref layerIndex1);
						CheckLayerIndex(terrain, 1, mat, ref layerIndex2);
						TerrainLaeyrDataToMaterial(terrain.terrainData.terrainLayers[layerIndex1], 0, mat);
						TerrainLaeyrDataToMaterial(terrain.terrainData.terrainLayers[layerIndex2], 1, mat);	
						hasNormalMap = terrain.terrainData.terrainLayers[layerIndex1].normalMapTexture || terrain.terrainData.terrainLayers[layerIndex2].normalMapTexture;
					#endif

					mat.SetFloat("_ControlNumber", layerIndex1 % 4); 

					if (terrain.terrainData.alphamapTextureCount > layerIndex1 / 4) mat.SetTexture("_Control", terrain.terrainData.alphamapTextures[layerIndex1 / 4]);
					if (layerIndex1 > 3 || layerIndex2 > 3) mat.DisableKeyword("_TERRAIN_BLEND_HEIGHT");
				}
				
				if (mat.GetFloat("_DisableDistanceBlending") == 1)
				{
					mat.DisableKeyword("_TERRAIN_DISTANCEBLEND");
				}

				if (hasNormalMap) { mat.EnableKeyword("_NORMALMAP"); } else { mat.DisableKeyword("_NORMALMAP"); }

				if (mat.shader.name == DiffuseObjectShaderName)
                {
					if (mat.GetTexture("_BumpMap")) { mat.EnableKeyword("_OBJECT_NORMALMAP"); } else { mat.DisableKeyword("_OBJECT_NORMALMAP"); }
				}					
			}

			//--------- Updating the Materials outside of active Scene ---------
			#if UNITY_EDITOR
			string[] matGUIDS = UnityEditor.AssetDatabase.FindAssets("t:Material", null);

				foreach (string guid in matGUIDS)
				{
					Material mat = (Material)UnityEditor.AssetDatabase.LoadAssetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(guid), typeof(Material));
					if (mat && mat.shader && mat.shader.name != null && !materialsTerrain.ContainsKey(mat) && (mat.shader.name == ObjectShaderName || mat.shader.name == DiffuseObjectShaderName))
					{
						if (mat.IsKeywordEnabled("ONE_LAYER"))
						{
							TerrainLaeyrDataToMaterial(TerrainLayerFromGUID(mat, "TerrainLayerGUID_1"), 0, mat);
						}
						if (mat.IsKeywordEnabled("TWO_LAYERS"))
						{
							TerrainLaeyrDataToMaterial(TerrainLayerFromGUID(mat, "TerrainLayerGUID_1"), 0, mat);
							TerrainLaeyrDataToMaterial(TerrainLayerFromGUID(mat, "TerrainLayerGUID_2"), 1, mat);
						}
					}
				}
			#endif
			TriplanarDataUpdate();
		}

		//============================================================================
		//-------------------------|		FUNCTIONS		|-------------------------
		//============================================================================
		public static bool CheckPosition(Terrain terrain, Vector2 position)
		{
			return terrain != null && terrain.terrainData != null
			&& terrain.GetPosition().x <= position.x && (terrain.GetPosition().x + terrain.terrainData.size.x) > position.x
			&& terrain.GetPosition().z <= position.y && (terrain.GetPosition().z + terrain.terrainData.size.z) > position.y;
		}

		#if UNITY_EDITOR
			public static TerrainLayer TerrainLayerFromGUID(Material mat, string tag)
			{
				return (TerrainLayer)UnityEditor.AssetDatabase.LoadAssetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(mat.GetTag(tag, false)), typeof(TerrainLayer));
			}
		#endif

		public static void TerrainLaeyrDataToMaterial(TerrainLayer tl, int n, Material mat)
		{
			bool diffuse = mat.shader.name == DiffuseObjectShaderName;
			Vector4 normScale = mat.GetVector("_TerrainNormalScale"); normScale[n] = tl ? tl.normalScale : 1;
			if (!diffuse)
			{
				#if UNITY_EDITOR
					if (tl)
					{
						UnityEditor.TextureImporter importer = UnityEditor.AssetImporter.GetAtPath(UnityEditor.AssetDatabase.GetAssetPath(tl.diffuseTexture)) as UnityEditor.TextureImporter;
						if (importer && importer.DoesSourceTextureHaveAlpha())
						{
							tl.smoothness = 1;
						}
					}
				#endif
				Vector4 smoothness = mat.GetVector("_TerrainSmoothness"); smoothness[n] = tl ? tl.smoothness : 0;
				Vector4 metallic = mat.GetVector("_TerrainMetallic"); metallic[n] = tl ? tl.metallic : 0;
				mat.SetVector("_TerrainSmoothness", smoothness);
				mat.SetVector("_TerrainMetallic", metallic);
			}
			
			mat.SetTexture("_Splat" + n.ToString(), tl ? tl.diffuseTexture : null);
			mat.SetTexture("_Normal" + n.ToString(), tl ? tl.normalMapTexture : null);			
			mat.SetVector("_TerrainNormalScale", normScale);
			mat.SetTexture("_Mask" + n.ToString(), tl ? tl.maskMapTexture : null);
			mat.SetVector("_SplatUV" + n.ToString(), tl ? new Vector4(tl.tileSize.x, tl.tileSize.y, tl.tileOffset.x, tl.tileOffset.y) : new Vector4(1,1,0,0));
			mat.SetVector("_MaskMapRemapScale" + n.ToString(), tl ? tl.maskMapRemapMax - tl.maskMapRemapMin : new Vector4(1, 1, 1, 1));
			mat.SetVector("_MaskMapRemapOffset" + n.ToString(), tl ? tl.maskMapRemapMin : new Vector4(0, 0, 0, 0));
		}
		 
		public static void CheckLayerIndex(Terrain terrain, int n, Material mat, ref int layerIndex)
		{
			bool diffuse = mat.shader.name == DiffuseObjectShaderName;
			foreach (TerrainLayer tl in terrain.terrainData.terrainLayers)
			{
				bool equal = tl && mat.GetTexture("_Splat" + n.ToString()) == tl.diffuseTexture
				&& mat.GetTexture("_Normal" + n.ToString()) == tl.normalMapTexture
				&& mat.GetVector("_TerrainNormalScale")[n] == tl.normalScale
				&& mat.GetTexture("_Mask" + n.ToString()) == tl.maskMapTexture
				&& mat.GetVector("_SplatUV" + n.ToString()) == new Vector4(tl.tileSize.x, tl.tileSize.y, tl.tileOffset.x, tl.tileOffset.y)
				&& mat.GetVector("_MaskMapRemapScale" + n.ToString()) == tl.maskMapRemapMax - tl.maskMapRemapMin
				&& mat.GetVector("_MaskMapRemapOffset" + n.ToString()) == tl.maskMapRemapMin;

				bool equalMetallicSmooth = diffuse || tl && mat.GetVector("_TerrainMetallic")[n] == tl.metallic
				&& mat.GetVector("_TerrainSmoothness")[n] == tl.smoothness;

				if (equal && equalMetallicSmooth)
				{
					layerIndex = terrain.terrainData.terrainLayers.ToList().IndexOf(tl);
					mat.SetFloat("_LayerIndex" + (n + 1).ToString(), layerIndex);
				}
			}
		}

		static void SetTerrainFloatsToMaterial(Terrain terrain, Material mat, string[] properties)
		{
			foreach (string prop in properties)
			{
				mat.SetFloat(prop, terrain.materialTemplate.GetFloat(prop));
			}
		}

		static void SetTerrainVectorToMaterial(Terrain terrain, Material mat, string value)
		{
			mat.SetVector(value, terrain.materialTemplate.GetVector(value));
		}

		static void TerrainKeywordsToMaterial(Terrain terrain, Material mat,  string[] keywords)
		{
			foreach (string keyword in keywords)
			{
				if (terrain.materialTemplate.IsKeywordEnabled(keyword))
				{
					mat.EnableKeyword(keyword);
				}
				else
				{
					mat.DisableKeyword(keyword);
				}
			}
		}

		static void DisableKeywords(Material mat, string[] keywords)
		{
			foreach (string keyword in keywords)
			{
				mat.DisableKeyword(keyword);
			}
		}

		public static GameObject GetUpdaterObject()
		{
			string name = "InTerra_UpdateAndCheck";
			GameObject updater = GameObject.Find(name);

			if (!updater)
			{
				updater = new GameObject(name);
				Material NormalMap = new Material(Shader.Find("Hidden/InTerra/CalculateNormal"));

				updater.AddComponent<MeshRenderer>();
				updater.GetComponent<MeshRenderer>().sharedMaterial = NormalMap;
				updater.AddComponent<InTerra_UpdateAndCheck>();
			
				updater.hideFlags = HideFlags.HideInInspector;
				updater.hideFlags = HideFlags.HideInHierarchy; 
			}
			return (updater);
		}


		public static void CheckAndUpdateNormalMapRenderTextures()
		{
			if (normalMaps == null)
			{
				UpdateTerrainData();
			}
			else
			{
				bool updated = false;

				foreach (Terrain terrain in normalMaps.Keys.ToList())
				{ 
					if (!terrain)
					{
						UpdateTerrainData();
						break;
					}
					else
					{
						if (!normalMaps[terrain] || !normalMaps[terrain].IsCreated() && terrain.terrainData.heightmapTexture.IsCreated())
						{							
							normalMaps[terrain] = CreateNormalsRenderTexture(terrain);						
							updated = true;
						}					
					}
				}

				if (updated)
				{
					foreach (Material mat in materialsTerrain.Keys)
					{
						mat.SetTexture("_TerrainNormalmapTexture", normalMaps[materialsTerrain[mat]]);
						mat.SetTexture("_TerrainHeightmapTexture", materialsTerrain[mat].terrainData.heightmapTexture);
					}
				}
			}
		#if UNITY_EDITOR
			TriplanarDataUpdate();
		#endif
		}

		static void TriplanarDataUpdate()
		{
			Terrain[] terrains = Terrain.activeTerrains;
			foreach (Terrain terrain in terrains)
			{
				if (terrain && terrain.materialTemplate.IsKeywordEnabled("_TERRAIN_TRIPLANAR"))
				{
					terrain.materialTemplate.SetVector("_TerrainSizeXZPosY", new Vector3(terrain.terrainData.size.x, terrain.terrainData.size.z, terrain.transform.position.y));
				} 
			}
		}

		public static void UpdateHeightMapTextures()
		{
			foreach (Material mat in materialsTerrain.Keys)
			{
				mat.SetTexture("_TerrainHeightmapTexture", materialsTerrain[mat].terrainData.heightmapTexture);
			}
		}

		static RenderTexture CreateNormalsRenderTexture(Terrain terrain)
		{
			normalsCreatorMat.SetTexture("_TerrainHeightmapTexture", terrain.terrainData.heightmapTexture);
			normalsCreatorMat.SetVector("_HeightmapScale", terrain.terrainData.heightmapScale);
			int resolution = terrain.terrainData.heightmapResolution;
			RenderTexture rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBHalf) { name = terrain.GetInstanceID().ToString() };
			Graphics.Blit(null, rt, normalsCreatorMat);
			return rt;
		}

		//Folowing code is from Bunny83 and is needed for centering the window position even if multiple displayes are used
		#if UNITY_EDITOR
			public static System.Type[] GetAllDerivedTypes(this System.AppDomain aAppDomain, System.Type aType)
			{
				var result = new List<System.Type>();
				var assemblies = aAppDomain.GetAssemblies();
				foreach (var assembly in assemblies)
				{
					var types = assembly.GetTypes();
					foreach (var type in types)
					{
						if (type.IsSubclassOf(aType))
							result.Add(type);
					}
				}
				return result.ToArray();
			}

			public static Rect GetEditorMainWindowPos()
			{
				var containerWinType = System.AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(ScriptableObject)).Where(t => t.Name == "ContainerWindow").FirstOrDefault();
				if (containerWinType == null)
					throw new System.MissingMemberException("Can't find internal type ContainerWindow. Maybe something has changed inside Unity");
				var showModeField = containerWinType.GetField("m_ShowMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var positionProperty = containerWinType.GetProperty("position", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
				if (showModeField == null || positionProperty == null)
					throw new System.MissingFieldException("Can't find internal fields 'm_ShowMode' or 'position'. Maybe something has changed inside Unity");
				var windows = Resources.FindObjectsOfTypeAll(containerWinType);
				foreach (var win in windows)
				{
					var showmode = (int)showModeField.GetValue(win);
					if (showmode == 4) // main window
					{
						var pos = (Rect)positionProperty.GetValue(win, null);
						return pos;
					}
				}
				throw new System.NotSupportedException("Can't find internal main window. Maybe something has changed inside Unity");
			}

			public static void CenterOnMainWin(this UnityEditor.EditorWindow aWin)
			{
				var main = GetEditorMainWindowPos();
				var pos = aWin.position;
				float w = (main.width - pos.width) * 0.5f;
				float h = (main.height - pos.height) * 0.5f;
				pos.x = main.x + w;
				pos.y = main.y + h;
				aWin.position = pos;
			}
		#endif
		//==============================================================================================================================

	}
}
