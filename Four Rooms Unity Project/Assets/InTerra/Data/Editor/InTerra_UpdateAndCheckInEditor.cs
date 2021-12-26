using UnityEditor;
using UnityEditor.SceneManagement;

namespace InTerra
{
	public class InTerra_UpdateAndCheckInEditor : UnityEditor.AssetModificationProcessor
	{
		[InitializeOnLoadMethod]
		static void InTerra_InitializeTerrainDataLoading()
		{
			EditorSceneManager.sceneOpened += SceneOpened;
			EditorApplication.update += InTerra_Data.CheckAndUpdateNormalMapRenderTextures;
			Undo.undoRedoPerformed += InTerra_Data.UpdateHeightMapTextures; //when Undo is performed on Terrain changes the Heightmap of Terrain goes black, so there is just quick update
		}

		static void SceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
		{
			EditorApplication.delayCall += InTerra_Data.UpdateTerrainData;
		}

		static string[] OnWillSaveAssets(string[] paths)
		{
			EditorApplication.delayCall += InTerra_Data.UpdateTerrainData;
			return paths;
		}	
	}
}