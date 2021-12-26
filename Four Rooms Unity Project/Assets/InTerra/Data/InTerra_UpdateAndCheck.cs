using UnityEngine;

namespace InTerra
{
	public class InTerra_UpdateAndCheck : MonoBehaviour
	{
		void Start()
		{
			InTerra_Data.UpdateTerrainData();
		}

		void Update()
		{
			InTerra_Data.CheckAndUpdateNormalMapRenderTextures();
		}
	}
}
