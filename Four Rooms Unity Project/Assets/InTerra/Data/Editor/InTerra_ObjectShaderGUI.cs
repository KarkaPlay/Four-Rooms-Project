using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEditor;

namespace InTerra
{
	public class InTerra_ObjectShaderGUI : ShaderGUI
	{	
		bool terrainInfo = false;
		bool objectInfo = false;
		bool minmax1 = false;
		bool minmax2 = false;
		bool minmaxNi = false;
		bool nIntersect = false;

		MaterialProperty[] properties;
		string shaderName = " ";
		Object selectedObject;
		Vector2 ScrollPos;

		List<Renderer> okTerrain = new List<Renderer>();
		List<Renderer> noTerrain = new List<Renderer>();
		List<Renderer> wrongTerrain = new List<Renderer>();
		enum NumberOfLayers
		{
			[Description("One Pass")] OnePass,
			[Description("One Layer")] OneLayer,
			[Description("Two Layers")] TwoLayers
		}
		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			this.properties = properties;
			Material targetMat = materialEditor.target as Material;
			Rect textureRect;


			//===================== TERRAIN & OBJECTS DATA =========================
			Terrain[] terrains = Terrain.activeTerrains;
			Terrain terrain = null;
			TerrainLayer[] tLayers = null;
			
			bool isOnTerrain = false;

			if (InTerra_Data.materialsTerrain.ContainsKey(targetMat))
			{
				terrain = InTerra_Data.materialsTerrain[targetMat];
				isOnTerrain = true;
				tLayers = terrain.terrainData.terrainLayers;
			}

			if (targetMat.shader.name != shaderName || Selection.activeGameObject != selectedObject)
			{
				EditorApplication.delayCall += () =>
				{
					InTerra_Data.UpdateTerrainData();
				};
				if (terrain != null) CreateObjectsLists(targetMat, terrain);
				shaderName = targetMat.shader.name;
				selectedObject = Selection.activeGameObject;
			}
			//======================================================================
		
			//-------------------------- FONT STYLES -------------------------------
			var styleButtonBold = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold };
			var styleBoldLeft = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft };
			var styleLeft = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
			var styBoldCenter = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
			var styleMiniBold = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold };
			var styleMini = new GUIStyle(EditorStyles.miniLabel);
			//----------------------------------------------------------------------


			//=======================================================================
			//----------------------|   OBJECT TEXTURES    |-------------------------
			//=======================================================================

			//------------------------ ALBEDO ----------------------------
			TextureSingleLine("_MainTex", "_Color", "Albedo", "Albedo(RGB)");
			if (targetMat.shader.name == InTerra_Data.DiffuseObjectShaderName)
			{
				TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(FindProperty("_MainTex").textureValue)) as TextureImporter;
				if (importer && importer.DoesSourceTextureHaveAlpha())
				{
					using (new GUILayout.VerticalScope(EditorStyles.helpBox))
					{
						EditorGUILayout.LabelField("Channel Remapping:");
						Vector4 offset = targetMat.GetVector("_MaskMapRemapOffset");
						Vector4 scale = targetMat.GetVector("_MaskMapRemapScale");
						RemapMask(ref offset.z, ref scale.z, "A: Heightmap", "Remap Heightmap in Albedo Alpha Channel");
						if (EditorGUI.EndChangeCheck())
						{
							materialEditor.RegisterPropertyChangeUndo("InTerra Object Channel Remapping");
							targetMat.SetVector("_MaskMapRemapOffset", offset);
							targetMat.SetVector("_MaskMapRemapScale", scale);
						}
					}
					EditorGUILayout.Space();
				}
			}

			//------------------------ NORMAL MAP ------------------------
			EditorGUI.BeginChangeCheck();
			TextureSingleLine("_BumpMap", "_BumpScale", "Normal Map", "Normal map");
			if (EditorGUI.EndChangeCheck() && targetMat.shader.name == InTerra_Data.DiffuseObjectShaderName)
			{
				materialEditor.RegisterPropertyChangeUndo("InTerra Object Normal Texture Keywodr");
				SetKeyword("_OBJECT_NORMALMAP", FindProperty("_BumpMap").textureValue != null);
			}
			
			//------------------------ MASK MAP --------------------------
			if (targetMat.shader.name != InTerra_Data.DiffuseObjectShaderName)
			{ 
				using (new GUILayout.VerticalScope(EditorStyles.helpBox))
				{
					EditorGUI.BeginChangeCheck();
					MaterialProperty maskTex = FindProperty("_MainMask");
					targetMat.SetFloat("_HasMask", maskTex.textureValue ? 1.0f : 0.0f);

					using (new GUILayout.HorizontalScope())
					{
						textureRect = EditorGUILayout.GetControlRect(GUILayout.MinWidth(50));
						materialEditor.TexturePropertyMiniThumbnail(textureRect, maskTex, "Mask Map", "Mask Map Channels: \n R:Metallic \n G:A.Occlusion  \n B:Heightmap \n A:Smoothness ");
						if (EditorGUI.EndChangeCheck())
						{
							materialEditor.RegisterPropertyChangeUndo("InTerra Object Mask");
							targetMat.SetFloat("_HasMask", maskTex.textureValue ? 1.0f : 0.0f);						
						}
						if (GUILayout.Button(LabelAndTooltip("Mask Map Creator", "Open window for creating Mask Map")))
						{
							InTerra_MaskCreator.OpenWindow(false);
						}
					}

					EditorGUILayout.Space();
					if (maskTex.textureValue == null)
					{
						PropertyLine("_Metallic", "Metallic");
						PropertyLine("_Glossiness", "Smoothness");

						using (new GUILayout.HorizontalScope())
						{
							GUILayout.Label("A. Occlusion", GUILayout.MinWidth(118));
							float ao = targetMat.GetFloat("_Ao");

							EditorGUI.BeginChangeCheck();

							ao = EditorGUILayout.Slider(1 - ao, 0, 1);

							if (EditorGUI.EndChangeCheck())
							{
								materialEditor.RegisterPropertyChangeUndo("InTerra Object AO");
								targetMat.SetFloat("_Ao", 1 - ao);
							}
						}
					}
					else
					{				
						EditorGUILayout.LabelField("Channels Remapping:");
						EditorGUILayout.Space();

						Vector4 offset = targetMat.GetVector("_MaskMapRemapOffset");
						Vector4 scale = targetMat.GetVector("_MaskMapRemapScale");

						EditorGUI.BeginChangeCheck();					
						RemapMask(ref offset.x, ref scale.x, "R: Metallic", "Remap Metallic Map in Red Channel");
						RemapMask(ref offset.y, ref scale.y, "G: A. Occlusion", "Remap A. Occlusion Map in Green Channel");
						RemapMask(ref offset.z, ref scale.z, "B: Heightmap", "Remap Heightmap in Blue Channel");
						RemapMask(ref offset.w, ref scale.w, "A: Smoothness", "Remap Smoothness Map in Alpha Channel");

						if (EditorGUI.EndChangeCheck())
						{
							materialEditor.RegisterPropertyChangeUndo("InTerra Object Channel Remapping");
							targetMat.SetVector("_MaskMapRemapOffset", offset);
							targetMat.SetVector("_MaskMapRemapScale", scale);
						}
					}		
				}
			}
			//--------------------- TEXTURES PROPERTY -------------------------
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				materialEditor.TextureScaleOffsetProperty(FindProperty("_MainTex"));
			}
			EditorGUILayout.Space();

			//------------------------- DETAIL -------------------------
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorStyles.label.fontStyle = FontStyle.Bold;
				bool detail = targetMat.IsKeywordEnabled("_OBJECT_DETAIL");

				EditorGUI.BeginChangeCheck();
				detail = EditorGUILayout.ToggleLeft(LabelAndTooltip("Detail Map", "Secondary textures"), detail);
				if (EditorGUI.EndChangeCheck())
				{
					materialEditor.RegisterPropertyChangeUndo("InTerra Object DetailMap");
					SetKeyword("_OBJECT_DETAIL", detail);
				}

				EditorStyles.label.fontStyle = FontStyle.Normal;

				if (detail)
				{
					materialEditor.TexturePropertySingleLine(new GUIContent("Detail Albedo"), FindProperty("_DetailAlbedoMap"));
					TextureSingleLine("_DetailNormalMap", "_DetailNormalMapScale", "Normal Map", "Detail Normal Map");

					materialEditor.ShaderProperty(FindProperty("_DetailStrenght"), new GUIContent("Detail Strenght"));

					using (new GUILayout.VerticalScope(EditorStyles.helpBox))
					{
						materialEditor.TextureScaleOffsetProperty(FindProperty("_DetailAlbedoMap"));
					}
				}
			}						
			EditorGUILayout.Space();

			//========================================================================
			//---------------------------|   WARNINGS   |-----------------------------
			//========================================================================
			if (noTerrain.Count > 0 && noTerrain.Count < 2)
			{			
				EditorGUILayout.HelpBox("The Object " + noTerrain[0].name + " with this material is outside of any Terrain!", MessageType.Warning);
			}

			if (noTerrain.Count > 1)
			{
				EditorGUILayout.HelpBox("Some Objects with this material are outside of any Terrain!", MessageType.Warning);
			}

			if (wrongTerrain.Count > 0 && wrongTerrain.Count < 2)
			{
				EditorGUILayout.HelpBox("The Object " + wrongTerrain[0].name + " with this material is not on correct Terrain!", MessageType.Warning);
			}

			if (wrongTerrain.Count > 1)
			{
				EditorGUILayout.HelpBox("Some Objects with this material are not on correct Terrain!", MessageType.Warning);
			}

			if (!isOnTerrain)
			{			
				if (okTerrain.Count + wrongTerrain.Count + noTerrain.Count == 0)
				{
					EditorGUILayout.HelpBox("This Material is not assigned to any Object in this Scene.", MessageType.Info);
				}
				else
				{
					EditorGUILayout.HelpBox("The avarge position of the Objects using this Material is outside of any Terrain!", MessageType.Warning);
				}
				GUI.enabled = false;
			}

			//================================================================
			//-------------------|   TERRAIN LAYERS    |----------------------
			//================================================================
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUILayout.LabelField("TERRAIN LAYERS", styBoldCenter);


				NumberOfLayers layers = NumberOfLayers.OnePass;
				if (targetMat.IsKeywordEnabled("ONE_LAYER"))
				{
					layers = NumberOfLayers.OneLayer;
				}
				else if (targetMat.IsKeywordEnabled("TWO_LAYERS"))
				{
					layers = NumberOfLayers.TwoLayers;
				}
				EditorGUI.BeginChangeCheck();
				layers = (NumberOfLayers)EditorGUILayout.EnumPopup(layers);
				if (EditorGUI.EndChangeCheck())
				{
					materialEditor.RegisterPropertyChangeUndo("InTerra Shader Variant");
					SetKeyword("ONE_LAYER", layers == NumberOfLayers.OneLayer);
					SetKeyword("TWO_LAYERS", layers == NumberOfLayers.TwoLayers);
					InTerra_Data.UpdateTerrainData();
				}

				//----------------------	ONE LAYER   ----------------------
				if (isOnTerrain && layers == NumberOfLayers.OneLayer)
				{
					SelectTerrainLayer(1, "Terrain Layer:");
				}

				//----------------------   TWO LAYERS   -----------------------
				if (isOnTerrain && layers == NumberOfLayers.TwoLayers)
				{
					SelectTerrainLayer(1, "Terrain Layer 1:");
					SelectTerrainLayer(2, "Terrain Layer 2:");
				}

				//----------------------   ONE PASS   -----------------------
				if (isOnTerrain && layers == NumberOfLayers.OnePass)
				{
					List<string> passes = new List<string>();

					int passNumber = (int)targetMat.GetFloat("_PassNumber");
					int passesList = passNumber + 1;
					if (terrain.terrainData.alphamapTextureCount <= passNumber)
					{
						EditorGUILayout.HelpBox("The Terrain do not have pass " + ( passNumber + 1 ) + ".", MessageType.Warning);
					}
					else
					{
						passesList = terrain.terrainData.alphamapTextureCount;
					}

					for (int i = 0; i < (passesList) ; i++)
					{
						passes.Add("Pass " + (i + 1).ToString() + " - Layers  "  + (i * 4 + 1).ToString() + " - " + (i * 4 + 4).ToString());
					}

					EditorGUI.BeginChangeCheck();
					passNumber = EditorGUILayout.Popup(passNumber, passes.ToArray(), GUILayout.MinWidth(150));

					if (EditorGUI.EndChangeCheck())
					{
						materialEditor.RegisterPropertyChangeUndo("InTerra LayerNumber1");
						targetMat.SetFloat("_PassNumber", passNumber);
						InTerra_Data.UpdateTerrainData();
					}

					GUILayout.BeginHorizontal();
					for (int i = passNumber * 4; i < (passNumber * 4 + 4); i++)
					{
						string layerName = "Empty";
						Texture2D layerTexture = null;

						if (i < terrain.terrainData.alphamapLayers)
						{
							TerrainLayer tl = terrain.terrainData.terrainLayers[i];
							if (tl)
							{
								layerName = tl.name;
								layerTexture = AssetPreview.GetAssetPreview(tl.diffuseTexture);
							}
							else
							{
								layerName = "Missing";
							}
						}

						using (new GUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Width(50)))
						{
							if (layerTexture)
							{ 
								GUI.DrawTexture(EditorGUILayout.GetControlRect(GUILayout.Width(48), GUILayout.Height(48)), layerTexture, ScaleMode.ScaleAndCrop); 
							}
							else
							{
								EditorGUILayout.GetControlRect(GUILayout.Width(48), GUILayout.Height(48)); 
							}
							EditorGUILayout.LabelField(layerName, styleMini, GUILayout.Width(48), GUILayout.Height(12));
						}
					}
					GUILayout.EndHorizontal();
				}
			}

			//============================================================================
			//-----------------------|  TERRAIN INTERSECTION  |---------------------------
			//============================================================================
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUILayout.LabelField("TERRAIN INTERSECTION", styBoldCenter);
				using (new GUILayout.VerticalScope(EditorStyles.helpBox))
				{
					EditorGUI.BeginChangeCheck();

					Vector4 intersection = MinMaxValues(targetMat.GetVector("_Intersection"), ref minmax1);

					if (EditorGUI.EndChangeCheck())
					{
						materialEditor.RegisterPropertyChangeUndo("InTerra BlendingIntersection");
						targetMat.SetVector("_Intersection", intersection);
					}

					EditorGUILayout.Space();

					PropertyLine("_Sharpness", "Sharpness", "Sharpness of blending");

					EditorGUI.BeginChangeCheck();

					EditorGUI.indentLevel = 1;
					nIntersect = EditorGUILayout.Foldout(nIntersect, LabelAndTooltip("Mesh Normals Intersection", "The height of intersection of terrain's and object's mesh normals. This value is calculated per vertex and it always affects the whole polygon!"));
					EditorGUI.indentLevel = 0;
					if (nIntersect)
					{
						Vector4 normalIntersect = MinMaxValues(targetMat.GetVector("_NormIntersect"), ref minmaxNi);

						if (EditorGUI.EndChangeCheck())
						{
							materialEditor.RegisterPropertyChangeUndo("InTerra NormalIntersection");
							targetMat.SetVector("_NormIntersect", normalIntersect);
						}

					}
				}


				//============================= STEEP SLOPES =============================
				using (new GUILayout.VerticalScope(EditorStyles.helpBox))
				{
					EditorGUILayout.LabelField("Steep slopes", styBoldCenter);

					//------------------------- SECONDARY INTERSECTION  -------------------------------
					bool steepIntersect = targetMat.IsKeywordEnabled("_OBJECT_STEEP_INTERSECTION");

					EditorGUI.BeginChangeCheck();
					steepIntersect = EditorGUILayout.ToggleLeft(LabelAndTooltip("Secondary Intersection", "Separated intersection for steep slopes."), steepIntersect);
					Vector4 intersection2 = targetMat.GetVector("_Intersection2");

					if (steepIntersect)
					{
						intersection2 = MinMaxValues(intersection2, ref minmax2);
						PropertyLine("_Steepness", "Steepness adjust", "This value adjusts the angle that will be considered as steep.");
					}
					if (EditorGUI.EndChangeCheck())
					{
						materialEditor.RegisterPropertyChangeUndo("InTerra Secondary Intersection");
						SetKeyword("_OBJECT_STEEP_INTERSECTION", steepIntersect);
						targetMat.SetVector("_Intersection2", intersection2);
					}

					//------------------------------ TRIPLANAR -------------------------------
					bool triplanar = targetMat.IsKeywordEnabled("_OBJECT_TRIPLANAR");
					bool disOffset = targetMat.GetFloat("_DisableOffsetY") == 1;

					EditorGUI.BeginChangeCheck();
					triplanar = EditorGUILayout.ToggleLeft(LabelAndTooltip("Triplanar Mapping", "The Texture on steep slopes of Object will not be stretched."), triplanar);

					if (triplanar)
					{
						EditorGUI.indentLevel = 1;
						EditorStyles.label.fontSize = 10;
						disOffset = EditorGUILayout.ToggleLeft(LabelAndTooltip("Disable Height and Position Offset", "Front and Side projection of texture is offsetting by position and height to fit the Terrain texture as much as possible, but in some cases, if there is too steep slope of terrain, it can get stretched and it is better to disable the offsetting."), disOffset, GUILayout.Width(200));
						EditorStyles.label.fontSize = 12;
						EditorGUI.indentLevel = 0;
					}
					if (EditorGUI.EndChangeCheck())
					{
						materialEditor.RegisterPropertyChangeUndo("InTerra Object Triplanar");
						SetKeyword("_OBJECT_TRIPLANAR", triplanar);
						if (disOffset) targetMat.SetFloat("_DisableOffsetY", 1); else targetMat.SetFloat("_DisableOffsetY", 0);
					}

					//------------------------------ DISTORTION -------------------------------
					if (!triplanar)
					{
						EditorStyles.label.fontSize = 11;
						materialEditor.ShaderProperty(FindProperty("_SteepDistortion"), LabelAndTooltip("Distortion (by Albedo)", "This value distorts stretched texture on Steep slopes, this is useful if you don't want to use triplanar - which is more performance heavy. Distortion is calculated by Albedo Texture and doesn't work with a single color."));
						EditorStyles.label.fontSize = 12;
					}
				}

				//------------------------------ DISABLE HIDE TILING -------------------------------
				if (terrain != null && terrain.materialTemplate.IsKeywordEnabled("_TERRAIN_DISTANCEBLEND"))
				{
					using (new GUILayout.VerticalScope(EditorStyles.helpBox))
					{
						bool distanceBlend = targetMat.GetFloat("_DisableDistanceBlending") == 1;

						EditorGUI.BeginChangeCheck();
						distanceBlend = EditorGUILayout.ToggleLeft(LabelAndTooltip("Disable Hide Tiling", "If Terrain \"Hide Tiling\" is set on, this option will turn it off only for this Material to prevent additional samplings and calculations. This may cause some more or less visible seams in distance."), distanceBlend);

						if (EditorGUI.EndChangeCheck())
						{
							materialEditor.RegisterPropertyChangeUndo("InTerra Disable Hide Tiling");

							if (distanceBlend)
							{
								targetMat.SetFloat("_DisableDistanceBlending", 1);
								targetMat.DisableKeyword("_TERRAIN_DISTANCEBLEND");
							}
							else
							{
								targetMat.SetFloat("_DisableDistanceBlending", 0);
								SetKeyword("_TERRAIN_DISTANCEBLEND", terrain.materialTemplate.IsKeywordEnabled("_TERRAIN_DISTANCEBLEND"));
							}
						}
					}
				}
			}

			//================= TERRAIN INFO ================
			EditorGUI.indentLevel = 1;
			terrainInfo = EditorGUILayout.Foldout(terrainInfo, "Terrain info");
			EditorGUI.indentLevel = 0;
			if (terrainInfo && isOnTerrain)
			{
				using (new GUILayout.VerticalScope(EditorStyles.helpBox))
				{
					GUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("Name:", styleBoldLeft, GUILayout.Width(60));
					EditorGUILayout.LabelField(terrain.name, styleLeft, GUILayout.MinWidth(50));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("Position:", styleBoldLeft, GUILayout.Width(60));
					Vector3 tp = terrain.GetPosition();

					EditorGUILayout.LabelField("X: " + tp.x.ToString(), styleLeft, GUILayout.MinWidth(50));
					EditorGUILayout.LabelField("Y: " + tp.y.ToString(), styleLeft, GUILayout.MinWidth(50));
					EditorGUILayout.LabelField("Z: " + tp.z.ToString(), styleLeft, GUILayout.MinWidth(50));
					GUILayout.EndHorizontal();
				}
				EditorGUI.indentLevel = 0;
			}
			GUI.enabled = true;
		

			//================= OBJECT INFO ================
			EditorGUI.indentLevel = 1;
			objectInfo = EditorGUILayout.Foldout(objectInfo, "Objects info");
			EditorGUI.indentLevel = 0;
			if (objectInfo)
			{
				using (new GUILayout.VerticalScope(EditorStyles.helpBox))
				{
					using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
					{					
						GUILayout.Label("Name", styleMiniBold, GUILayout.MinWidth(60));
						GUILayout.Label("position (x,y,z)", styleMiniBold, GUILayout.MinWidth(40));
						GUILayout.Label("Go to Object", styleMiniBold, GUILayout.Width(65));		
					}

					ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos, GUILayout.Height(100));
				
					ObjectsList(noTerrain, Color.red);
					ObjectsList(wrongTerrain, new Color(1.0f, 0.5f, 0.0f));
					ObjectsList(okTerrain, Color.black);

					EditorGUILayout.EndScrollView();
				}
				EditorGUI.indentLevel = 0;
			}
			GUI.enabled = true;

			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				if (GUILayout.Button("Update Terrain Data", styleButtonBold))
				{
					InTerra_Data.UpdateTerrainData();
					foreach (Terrain ter in terrains)
					{
						if (InTerra_Data.materialsTerrain.ContainsKey(targetMat))
						{
							terrain = InTerra_Data.materialsTerrain[targetMat];
						}
					}
					
				}
			}

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			//-------------------------------------------------------------
			materialEditor.RenderQueueField();
			materialEditor.EnableInstancingField();
			materialEditor.DoubleSidedGIField();
			//-------------------------------------------------------------

			//=====================================================================================
			 
			void PropertyLine(string property, string label, string tooltip = null)
			{
				materialEditor.ShaderProperty(FindProperty(property), new GUIContent() { text = label, tooltip = tooltip });
			}

			void TextureSingleLine(string property1, string property2, string label, string tooltip = null)
			{
				materialEditor.TexturePropertySingleLine(new GUIContent() { text = label, tooltip = tooltip }, FindProperty(property1), FindProperty(property2) );
			}

			GUIContent LabelAndTooltip(string label, string tooltip)
			{
				return new GUIContent() { text = label, tooltip = tooltip };
			}

			void SetKeyword(string name, bool set)
			{
				if (set) targetMat.EnableKeyword(name); else targetMat.DisableKeyword(name);
			}

			void RemapMask(ref float offset, ref float scale, string label, string tooltip = null)
			{
				using (new GUILayout.HorizontalScope())
				{
					scale += offset;
					EditorGUILayout.LabelField(new GUIContent() { text = label, tooltip = tooltip }, GUILayout.Width(100));
					EditorGUILayout.LabelField(" ", GUILayout.Width(3));
					EditorGUILayout.MinMaxSlider(ref offset, ref scale, 0, 1);
					scale -= offset;
				}
			}

			void SelectTerrainLayer(int layerNumber, string label)
			{ 				
				string tagName ="TerrainLayerGUID_" + layerNumber.ToString();
				TerrainLayer terainLayer = InTerra_Data.TerrainLayerFromGUID(targetMat, tagName); 

				EditorGUI.BeginChangeCheck();

				using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
				{
					EditorGUILayout.LabelField(LabelAndTooltip(label, "The Terrain Layer the Material will be blended with"), styleLeft, GUILayout.MaxWidth(100));
					Rect rt = GUILayoutUtility.GetLastRect();
					if (terainLayer && AssetPreview.GetAssetPreview(terainLayer.diffuseTexture))
					{
						GUI.DrawTexture(new Rect(rt.x + 103, rt.y, 21, 21), AssetPreview.GetAssetPreview(terainLayer.diffuseTexture), ScaleMode.ScaleToFit, true);
					}

					EditorGUILayout.GetControlRect(GUILayout.Width(20));					
					terainLayer = (TerrainLayer)EditorGUILayout.ObjectField(terainLayer, typeof(TerrainLayer), false, GUILayout.MinWidth(100), GUILayout.Height(22));

					if (EditorGUI.EndChangeCheck())
					{
						materialEditor.RegisterPropertyChangeUndo("InTerra TerrainLayer");
						targetMat.SetOverrideTag(tagName, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(terainLayer)));

						InTerra_Data.UpdateTerrainData();
					}
				}
			}		
		}		
		//=====================================================================================

		MaterialProperty FindProperty(string name)
		{
			return FindProperty(name, properties);
		}

		Vector4 MinMaxValues(Vector4 intersection, ref bool minMax)
		{
			GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(intersection.x.ToString("0.0"), GUILayout.Width(33));
				EditorGUILayout.MinMaxSlider(ref intersection.x, ref intersection.y, intersection.z, intersection.w);
				EditorGUILayout.LabelField(intersection.y.ToString("0.0"), GUILayout.Width(33));

			GUILayout.EndHorizontal();

			EditorGUI.indentLevel = 2;
			minMax = EditorGUILayout.Foldout(minMax, "Adjust range");
			EditorGUI.indentLevel = 0;
			if (minMax)
			{
				GUILayout.BeginHorizontal();

					GUIStyle rightAlignment = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };
					EditorGUILayout.LabelField("Min:", rightAlignment, GUILayout.Width(45));
					intersection.z = EditorGUILayout.DelayedFloatField(intersection.z, GUILayout.MinWidth(50));

					EditorGUILayout.LabelField("Max:", rightAlignment, GUILayout.Width(45));
					intersection.w = EditorGUILayout.DelayedFloatField(intersection.w, GUILayout.MinWidth(50));

				GUILayout.EndHorizontal();
			}

			intersection.x = Mathf.Clamp(intersection.x, intersection.z, intersection.w);
			intersection.y = Mathf.Clamp(intersection.y, intersection.z, intersection.w);

			intersection.y = intersection.x + (float)0.001 >= intersection.y ? intersection.y + (float)0.001 : intersection.y;

			return intersection;
		}


		void ObjectsList(List<Renderer> rend, Color color)
		{
		
			var style = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleLeft };
			if (color != Color.black)
			{
				style.normal.textColor = color;
			}

			for (int i = 0; i< rend.Count; i++)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(rend[i].name, style, GUILayout.MinWidth(60));
				GUILayout.Label(rend[i].transform.position.x.ToString() + ", " + rend[i].transform.position.y.ToString() + ", " + rend[i].transform.position.z.ToString(), style, GUILayout.MinWidth(40));

				if (GUILayout.Button("  -->  ", EditorStyles.miniButton, GUILayout.Width(50)))
				{
					Selection.activeGameObject = rend[i].gameObject;
					SceneView.lastActiveSceneView.Frame(rend[i].bounds, false);
				}
				GUILayout.EndHorizontal();
			}		
		}


		void CreateObjectsLists(Material targetMat, Terrain terain)
		{
			Terrain[] terrains = Terrain.activeTerrains;
			MeshRenderer[] renderers = Object.FindObjectsOfType<MeshRenderer>();

			okTerrain.Clear();
			noTerrain.Clear();
			wrongTerrain.Clear();

			foreach (Renderer rend in renderers)
			{
				if (rend != null && rend.transform.position != null)
				{
					foreach (Material mat in rend.sharedMaterials)
					{
						if (mat != null && mat.shader != null && mat.shader.name != null && (mat == targetMat))
						{
							noTerrain.Add(rend); //it is easier to check if the renderer is on Terrain, so all renderes will be add to this list and if it is on terrain, it will be removed 
							wrongTerrain.Add(rend);
						
							Vector2 pos = new Vector2(rend.transform.position.x, rend.transform.position.z);

							if (InTerra_Data.CheckPosition(terain, pos))
							{
								okTerrain.Add(rend);
								wrongTerrain.Remove(rend);
							}

							foreach (Terrain ter in terrains)
							{
								if (InTerra_Data.CheckPosition(ter, pos))
								{								
									noTerrain.Remove(rend);																	
								}
							}
						}
					}
				}
			}

			foreach (Renderer nt in noTerrain)
			{
				wrongTerrain.Remove(nt);
			}

		}
	
	}
}
