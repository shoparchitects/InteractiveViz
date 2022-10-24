using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lean.Touch
{
	/// <summary>
	/// Based off of modified lean touch examples. Will move the gameObjects position to the hit location. Main for use with an orbiting camera attached to a rotating camera root object.
	/// Not super well tested yet.
	/// </summary>
	public class cwmDoubleTapMoveCameraAndGetClick : MonoBehaviour
	{
		[Tooltip("The layers you want the raycast/overlap to hit")]
		public LayerMask LayerMask = Physics.DefaultRaycastLayers;

		[Tooltip("The camera used to calculate the ray (None = MainCamera)")]
		public Camera Camera;

        [Tooltip("Rotate the camera to look at the target")]
		public bool LookAtTarget = true;

		public float SecondsBetweenTaps = 0.4f;

		private float TimeElapsedBetweenTaps = 0;

		private GameObject _LastHitGO = null;

		private RaycastHit GlobalRaycast_Hit;


        protected virtual void LateUpdate()
		{
			// Get the fingers we want to use
			var fingers = LeanTouch.GetFingers(true, true, 1);

			if (fingers.Count > 0)
			{
				if (fingers[0].TapCount == 2)
				{
					//raycast					
					var camera = LeanTouch.GetCamera(Camera, gameObject);

					if (camera != null)
					{
						Ray ray = camera.ScreenPointToRay(fingers[0].ScreenPosition);
						RaycastHit hit;

						if (Physics.Raycast(ray, out hit, float.PositiveInfinity, LayerMask) == true)
						{
							transform.position = hit.point;
							GlobalRaycast_Hit = hit;


							//if (LookAtTarget)
							//{
							//	camera.transform.LookAt(hit.point);
							//}
						}
					}

					fingers[0].TapCount = 0; //<--ADDED THIS LINE TO CLEAR THE TAP COUNT
					TimeElapsedBetweenTaps = 0;
					_LastHitGO = null;
				}
				
				
			}



		}
	}
}
