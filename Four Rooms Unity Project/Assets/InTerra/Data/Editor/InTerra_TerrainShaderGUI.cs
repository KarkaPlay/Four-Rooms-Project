using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InTerra
{
	public class InTerra_TerrainShaderGUI : ShaderGUI
	{
		bool setMinMax = false;
		bool setNormScale = false;
		bool moveLayer = false;
		int layerToFirst = 0;
		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			Material targetMat = materialEditor.target as Material;
			bool normInMask = targetMat.IsKeywordEnabled("_TERRAIN_NORMAL_IN_MASK");

			//----------------------------- FONT STYLES ----------------------------------
			var styleButtonBold = new GUIStyle(GUI.skin.button) {fontStyle = FontStyle.Bold};
			var styleBoldCenter = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
			var styleRight = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };
			var styleMini = new GUIStyle(EditorStyles.miniLabel);
			//----------------------------------------------------------------------------

			Terrain terrain = null;
			if (Selection.activeGameObject != null)
			{
				terrain = Selection.activeGameObject.GetComponent<Terrain>();
			}


			if (targetMat.shader.name == InTerra_Data.DiffuseTerrainShaderName)
			{
				using (new GUILayout.VerticalScope(EditorStyles.helpBox))
				{
					HeightmapBlending("Heightmap blending", "Heightmap based texture transition.");					
				}
			}
			else
			{
				//=============================  MASK MAP FEATURES  =============================
				using (new GUILayout.VerticalScope(EditorStyles.helpBox)) 
				{
					EditorGUILayout.LabelField("Mask Map Features", styleBoldCenter);
					using (new GUILayout.VerticalScope(EditorStyles.helpBox)) 
					{
						//------------------------ HEIGHTMAP BLENDING ------------------------
						HeightmapBlending("Heightmap blending", "Heightmap based texture transition.");

						//------------------------ OCCLUSION, METALLIC, SMOOTHNESS ------------------------
						bool maskMaps = targetMat.IsKeywordEnabled("_TERRAIN_MASK_MAPS") && !normInMask;
						string omLabel = "A.Occlusion, Metallic, Smoothness";
						string omTooltip = "This option applies the Ambient occlusion, Metallic and Smoothness maps from Mask Map.";
						EditorGUI.BeginChangeCheck();

						EditorStyles.label.fontStyle = FontStyle.Bold;	
						maskMaps = normInMask ? false : EditorGUILayout.ToggleLeft(LabelAndTooltip(omLabel, omTooltip), maskMaps);

						EditorStyles.label.fontStyle = FontStyle.Normal;
						if (EditorGUI.EndChangeCheck())
						{
							materialEditor.RegisterPropertyChangeUndo("InTerra Terrain Mask maps");
							SetKeyword(targetMat, maskMaps, "_TERRAIN_MASK_MAPS");
							InTerra_Data.UpdateTerrainData();
						}
				
						//-------------------------------- NORMAL IN MASK -------------------------------- 				
						EditorGUI.BeginChangeCheck();
						EditorStyles.label.fontStyle = FontStyle.Bold;
						normInMask = EditorGUILayout.ToggleLeft(LabelAndTooltip("Normal map in Mask map", "The Normal map will be taken from Mask map Green and Alpha channel, A.Occlusion from Red channel and Heightmap from Blue."), normInMask);
						EditorStyles.label.fontStyle = FontStyle.Normal;

						if (EditorGUI.EndChangeCheck())
						{
							materialEditor.RegisterPropertyChangeUndo("InTerra Normal To Mask");
							SetKeyword(targetMat, normInMask, "_TERRAIN_NORMAL_IN_MASK");
							if (normInMask) SetKeyword(targetMat, false, "_TERRAIN_MASK_MAPS");
							InTerra_Data.UpdateTerrainData();
						}

						if (!normInMask)
						{
							if (GUILayout.Button(LabelAndTooltip("Mask Map Creator", "Open window for creating Mask Map"), styleButtonBold))
							{
								InTerra_MaskCreator.OpenWindow(false);
							}
						}
						else
						{
							using (new GUILayout.VerticalScope(EditorStyles.helpBox))
							{
								if (GUILayout.Button(LabelAndTooltip("Normal-Mask Map Creator", "Open window for creating Mask Map including Normal map."), styleButtonBold))
								{
									InTerra_MaskCreator.OpenWindow(true);
								}

								EditorGUI.indentLevel = 1;
								setNormScale = EditorGUILayout.Foldout(setNormScale, "Normal Scales");

								if (setNormScale && terrain != null)
								{
									for (int i = 0; i < terrain.terrainData.alphamapLayers; i++)
									{
										TerrainLayer tl = terrain.terrainData.terrainLayers[i];
										if (tl)
										{										
											float nScale = tl.normalScale;
											EditorGUI.BeginChangeCheck();
											nScale = EditorGUILayout.FloatField(i.ToString() + ". " + tl.name + " :", nScale);
											if (EditorGUI.EndChangeCheck())
											{
												Undo.RecordObject(terrain.terrainData.terrainLayers[i], "InTerra TerrainLayer Normal Scale");
												tl.normalScale = nScale;
											}
										}
									}
								}
							}
							EditorGUI.indentLevel = 0;
						}
					}
				}
			}

			//========================= HIDE TILING (DISTANCE BLENDING) ========================
			bool distanceBlending = targetMat.IsKeywordEnabled("_TERRAIN_DISTANCEBLEND");
			Vector4 distance = targetMat.GetVector("_HT_distance");
		
		
			using (new GUILayout.VerticalScope(EditorStyles.helpBox)) 
			{
				EditorGUI.BeginChangeCheck();
				EditorStyles.label.fontStyle = FontStyle.Bold;
				distanceBlending = EditorGUILayout.ToggleLeft(LabelAndTooltip("Hide Tiling", "Hides tiling by covering the texture by its scaled up version in the given distance from the camera."), distanceBlending);
				EditorStyles.label.fontStyle = FontStyle.Normal;

				if (EditorGUI.EndChangeCheck())
				{
					materialEditor.RegisterPropertyChangeUndo("InTerra HideTiling Keyword");
					SetKeyword(targetMat, distanceBlending, "_TERRAIN_DISTANCEBLEND");
					InTerra_Data.UpdateTerrainData();
				}

				EditorGUI.BeginChangeCheck();
				if (distanceBlending)
				{
					using (new GUILayout.VerticalScope(EditorStyles.helpBox)) 
					{					
						PropertyLine("_HT_distance_scale", "Scale", "This value is multiplying the scale of the Texture of a distant area.");
						PropertyLine("_HT_cover", "Cover strenght", "Strength of covering the Terrain textures in the distant area.");

						using (new GUILayout.HorizontalScope())
						{
							EditorGUILayout.LabelField(LabelAndTooltip("Distance", "The distance where the covering will start. The closer the sliders are, the sharper is the transition."), GUILayout.Width(70));
							EditorGUILayout.LabelField(distance.x.ToString("0.0"), GUILayout.Width(30));
							EditorGUILayout.MinMaxSlider(ref distance.x, ref distance.y, distance.z, distance.w);
							EditorGUILayout.LabelField(distance.y.ToString("0.0"), GUILayout.Width(30));
						}

						EditorGUI.indentLevel = 1;
						setMinMax = EditorGUILayout.Foldout(setMinMax, "Adjust Distance range");
						EditorGUI.indentLevel = 0;
						if (setMinMax)
						{
							using (new GUILayout.HorizontalScope()) 
							{
								EditorGUILayout.LabelField("Min:", styleRight, GUILayout.Width(45));
								distance.z = EditorGUILayout.DelayedFloatField(distance.z, GUILayout.MinWidth(50));

								EditorGUILayout.LabelField("Max:", styleRight, GUILayout.Width(45));
								distance.w = EditorGUILayout.DelayedFloatField(distance.w, GUILayout.MinWidth(50));
							}
						}

						distance.x = Mathf.Clamp(distance.x, distance.z, distance.w);
						distance.y = Mathf.Clamp(distance.y, distance.z, distance.w);

					}
				
				}

				if (EditorGUI.EndChangeCheck())
				{
					materialEditor.RegisterPropertyChangeUndo("InTerra HideTiling Value");
					targetMat.SetVector("_HT_distance", distance);
				}
			}

			//============================= TRIPLANAR ============================= 
			bool triplanar = targetMat.IsKeywordEnabled("_TERRAIN_TRIPLANAR");
			bool triplanarOneLayer = targetMat.IsKeywordEnabled("_TERRAIN_TRIPLANAR_ONE");
			bool applyFirstLayer = targetMat.GetFloat("_TriplanarOneToAllSteep") == 1;

			using (new GUILayout.VerticalScope(EditorStyles.helpBox)) 
			{
				EditorGUI.BeginChangeCheck();

				EditorStyles.label.fontStyle = FontStyle.Bold;
				triplanar = EditorGUILayout.ToggleLeft(LabelAndTooltip("Triplanar Maping", "The Texture on steep slopes of Terrain will not be stretched."), triplanar);
				EditorStyles.label.fontStyle = FontStyle.Normal;
				if (triplanar && terrain != null)				
				{
					targetMat.SetVector("_TerrainSize", terrain.terrainData.size); //needed for triplanar UV
				}

				if (triplanar)
				{					
					using (new GUILayout.VerticalScope(EditorStyles.helpBox))
					{
						PropertyLine("_TriplanarSharpness", "Sharpness", "Sharpness of the textures transitions between planar projections.");
						triplanarOneLayer = EditorGUILayout.ToggleLeft(LabelAndTooltip("First Layer Only", "Only the first Terrain Layer will be triplanared - this option is for performance reasons."), triplanarOneLayer, GUILayout.MaxWidth(115));

						if (triplanarOneLayer)
						{							
							EditorGUI.indentLevel = 1;
							EditorStyles.label.fontSize = 11;
							applyFirstLayer = EditorGUILayout.ToggleLeft(LabelAndTooltip("Apply first Layer to all steep slopes", "The first Terrain Layer will be automaticly applied to all steep slopes."), applyFirstLayer);
							EditorStyles.label.fontSize = 12;

							if (terrain && terrain.terrainData.alphamapLayers > 1)
							{
								moveLayer = EditorGUILayout.Foldout(moveLayer, "Move Layer To First Position");
								EditorGUI.indentLevel = 0;
								if (moveLayer)
								{
									List<string> tl = new List<string>();
									for (int i = 1; i < terrain.terrainData.alphamapLayers; i++)
									{
										tl.Add((i + 1).ToString() + ". " + terrain.terrainData.terrainLayers[i].name.ToString()); 
									}
									if ((layerToFirst + 1) >= terrain.terrainData.alphamapLayers) layerToFirst = 0;
									TerrainLayer terainLayer = terrain.terrainData.terrainLayers[layerToFirst + 1];

									using (new GUILayout.HorizontalScope())
									{									
										if (terainLayer && AssetPreview.GetAssetPreview(terainLayer.diffuseTexture))
										{
											GUI.Box(EditorGUILayout.GetControlRect(GUILayout.Width(50),  GUILayout.Height(50)), AssetPreview.GetAssetPreview(terainLayer.diffuseTexture));		
										}
										else
										{
											EditorGUILayout.GetControlRect(GUILayout.Width(50), GUILayout.Height(50));
										}
										using (new GUILayout.VerticalScope())
										{										
											layerToFirst = EditorGUILayout.Popup("", layerToFirst, tl.ToArray(), GUILayout.MinWidth(170));
											if (GUILayout.Button("Move Layer to First Position", GUILayout.MinWidth(170), GUILayout.Height(27)))
											{											
												MoveLayerToFirstPosition(terrain, layerToFirst + 1);
												InTerra_Data.UpdateTerrainData();
											}
										}
										EditorGUILayout.GetControlRect(GUILayout.MinWidth(10));
									}
								}
							}							
						}
					}
				}

				if (EditorGUI.EndChangeCheck())
				{
					materialEditor.RegisterPropertyChangeUndo("InTerra Triplanar Terrain");
					SetKeyword(targetMat, triplanar && triplanarOneLayer, "_TERRAIN_TRIPLANAR_ONE");
					SetKeyword(targetMat, triplanar, "_TERRAIN_TRIPLANAR");
					if (applyFirstLayer && triplanar && triplanarOneLayer) targetMat.SetFloat("_TriplanarOneToAllSteep", 1); else targetMat.SetFloat("_TriplanarOneToAllSteep", 0);
				}

				if (targetMat.GetFloat("_NumLayersCount") > 4)
				{
					EditorGUILayout.HelpBox("Triplanar Features will be applied on Terrain Base Map only if \"First Layer only\" is checked and there are not more than four Layers.", MessageType.Info);
				}

			}
			//========================= TWO LAYERS ONLY ===========================
			bool twoLayers = targetMat.IsKeywordEnabled("TWO_LAYERS");

			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUI.BeginChangeCheck();

				EditorStyles.label.fontStyle = FontStyle.Bold;
				twoLayers = EditorGUILayout.ToggleLeft(LabelAndTooltip("First Two Layers Only", "The shader will sample only first twoo layers."), twoLayers);
				EditorStyles.label.fontStyle = FontStyle.Normal;
				if (EditorGUI.EndChangeCheck())
				{
					materialEditor.RegisterPropertyChangeUndo("InTerra Terrain Two Layers");
					SetKeyword(targetMat, twoLayers, "TWO_LAYERS");
				}
			}

			EditorGUILayout.Space();


			using (new GUILayout.VerticalScope(EditorStyles.helpBox)) 
			{
				if (GUILayout.Button(LabelAndTooltip("Update Terrain Data For Objects", "Send updated data from Terrain to Objects integrated to Terrain."), styleButtonBold))
				{
					InTerra_Data.UpdateTerrainData();
				}
			}
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			materialEditor.RenderQueueField();
			materialEditor.EnableInstancingField();


			void HeightmapBlending(string name, string tooltip)
			{
				
				bool heightBlending = targetMat.IsKeywordEnabled("_TERRAIN_BLEND_HEIGHT");
				EditorGUI.BeginChangeCheck();
				EditorStyles.label.fontStyle = FontStyle.Bold;
				heightBlending = EditorGUILayout.ToggleLeft(LabelAndTooltip(name, tooltip), heightBlending, GUILayout.MinWidth(120));
				
				EditorStyles.label.fontStyle = FontStyle.Normal;

				if (EditorGUI.EndChangeCheck())
				{
					materialEditor.RegisterPropertyChangeUndo("InTerra HeightBlend");
					SetKeyword(targetMat, heightBlending, "_TERRAIN_BLEND_HEIGHT");
					InTerra_Data.UpdateTerrainData();
				}

				if (heightBlending)
				{
					using (new GUILayout.VerticalScope(EditorStyles.helpBox))
					{
						PropertyLine("_HeightTransition", "Sharpness", "Sharpness of the textures transitions");

						if (targetMat.IsKeywordEnabled("_TERRAIN_DISTANCEBLEND"))
						{
							PropertyLine("_Distance_HeightTransition", "Distant Sharpness", "Sharpness of the textures transitions for distant area setted in Hide Tiling.");
						}
					}
				}
				else
                {
					if (targetMat.shader.name == InTerra_Data.DiffuseTerrainShaderName) EditorGUILayout.LabelField("(Heightmap will be taken from Diffuse Alpha cahnnel)", styleMini);
				}

				if (targetMat.GetFloat("_NumLayersCount") > 4)
				{
					EditorGUILayout.HelpBox("The Heightmap blending will not be applied on Terrain Base Map if there are more than four Layers.", MessageType.Info);
				}
			}

			void PropertyLine(string property, string label, string tooltip = null)
			{
				materialEditor.ShaderProperty(FindProperty(property, properties), new GUIContent() { text = label, tooltip = tooltip });
			}

			GUIContent LabelAndTooltip(string label, string tooltip)
			{
				return new GUIContent() { text = label, tooltip = tooltip };
			}

		
		}

		static void SetKeyword(Material targetMat, bool set, string keyword)
		{
			if (set)
			{
				targetMat.EnableKeyword(keyword);
			}
			else
			{
				targetMat.DisableKeyword(keyword);
			}
		}

		static void MoveLayerToFirstPosition(Terrain terrain, int indexToFirst)
		{
			float[,,] alphaMaps = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);

			for (int y = 0; y < terrain.terrainData.alphamapHeight; y++)
			{
				for (int x = 0; x < terrain.terrainData.alphamapWidth; x++)
				{
					float a0 = alphaMaps[x, y, 0];
					float a1 = alphaMaps[x, y, indexToFirst];

					alphaMaps[x, y, 0] = a1;
					alphaMaps[x, y, indexToFirst] = a0;

				}
			}
			TerrainLayer[] origLayers = terrain.terrainData.terrainLayers;
			TerrainLayer[] movedLayers = terrain.terrainData.terrainLayers;

			TerrainLayer firstLayer = terrain.terrainData.terrainLayers[0];
			TerrainLayer movingLayer = terrain.terrainData.terrainLayers[indexToFirst];

			movedLayers[0] = movingLayer;
			movedLayers[indexToFirst] = firstLayer;

			terrain.terrainData.SetTerrainLayersRegisterUndo(origLayers, "InTerra Move Terrain Layer");			
			terrain.terrainData.terrainLayers = movedLayers;
			
			Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "InTerra Move Terrain Layer");
			terrain.terrainData.SetAlphamaps(0, 0, alphaMaps);
		}
	}
}
