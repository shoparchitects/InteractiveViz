using UnityEngine;

namespace Lean.Touch
{
	// This script allows you to zoom a camera in and out based on the pinch gesture
	// This supports both perspective and orthographic cameras
	[ExecuteInEditMode]
	public class LeanCameraDolly : MonoBehaviour
	{
		[Tooltip("The camera that will be zoomed (None = MainCamera)")]
		public Camera Camera;

		[Tooltip("Ignore fingers with StartedOverGui?")]
		public bool IgnoreStartedOverGui = true;

		[Tooltip("Ignore fingers with IsOverGui?")]
		public bool IgnoreIsOverGui;

		[Tooltip("Allows you to force rotation with a specific amount of fingers (0 = any)")]
		public int RequiredFingerCount;

		[Tooltip("If RequiredSelectable.IsSelected is false, ignore?")]
		public LeanSelectable RequiredSelectable;

		[Tooltip("If you want the mouse wheel to simulate pinching then set the strength of it here")]
		[Range(-1.0f, 1.0f)]
		public float WheelSensitivity;

		[Tooltip("Should the scaling be performanced relative to the finger center?")]
		public bool Relative;
        		
		[Tooltip("Speed to Move camera")]
		public float CameraSpeed = 10.0f;

		[Tooltip("The maximum distance allowed from the camera to the rotate root")]
		public float MaxDistance = 1000;


#if UNITY_EDITOR
		protected virtual void Reset()
		{
			Start();
		}
#endif

		protected virtual void Start()
		{
			if (RequiredSelectable == null)
			{
				RequiredSelectable = GetComponent<LeanSelectable>();
			}
		}

		protected virtual void LateUpdate()
		{
			// Get the fingers we want to use
			var fingers = LeanSelectable.GetFingers(IgnoreStartedOverGui, IgnoreIsOverGui, RequiredFingerCount, RequiredSelectable);

			if (fingers.Count > 1)
			{
				// Get the pinch ratio of these fingers
				var pinchRatio = LeanGesture.GetPinchRatio(fingers, WheelSensitivity);
				
				
				// Perform the translation if this is a relative scale
				if (Relative == true)
				{
					var pinchScreenCenter = LeanGesture.GetScreenCenter(fingers);

					//Translate(pinchRatio, pinchScreenCenter);
				}

				// Do the Dolly
				//Debug.Log(pinchRatio);
				SetDolly(1- pinchRatio);
				
				
			}
		}

		// Original copied from Lean Touch Zoom
		protected virtual void Translate(float pinchScale, Vector2 screenCenter)
		{
			// Make sure the camera exists
			var camera = LeanTouch.GetCamera(Camera, gameObject);

			if (camera != null)
			{
				// Screen position of the transform
				var screenPosition = camera.WorldToScreenPoint(transform.position);

				// Push the screen position away from the reference point based on the scale
				screenPosition.x = screenCenter.x + (screenPosition.x - screenCenter.x) * pinchScale;
				screenPosition.y = screenCenter.y + (screenPosition.y - screenCenter.y) * pinchScale;

				// Convert back to world space
				transform.position = camera.ScreenToWorldPoint(screenPosition);
			}
			else
			{
				Debug.LogError("Failed to find camera. Either tag your cameras MainCamera, or set one in this component.", this);
			}
		}

		
        protected void SetDolly(float ratio)
        {
			// Make sure the camera exists
			var camera = LeanTouch.GetCamera(Camera, gameObject);
			var fingers = LeanSelectable.GetFingers(IgnoreStartedOverGui, IgnoreIsOverGui, RequiredFingerCount, RequiredSelectable);

			Vector3 dollyDirection = camera.transform.forward;
			if(Relative)
            {
				//TODO - clean up, maybe moe into some update since it already calculates the centerscreen there
				
				var pinchScreenCenter = LeanGesture.GetScreenCenter(fingers);
				dollyDirection = camera.ScreenPointToRay(pinchScreenCenter).direction;
			}

			if (camera != null)
			{

				float DistanceFromTarget = Vector3.Distance(this.transform.position, camera.transform.position);
				float DistanceToMove = DistanceFromTarget * ratio * CameraSpeed;
				//Debug.Log(DistanceToMove + "; " + DistanceFromTarget);
				var newPosition = camera.transform.position + dollyDirection * DistanceToMove;

				float PotentialNewDistance = Vector3.Distance(this.transform.position, newPosition);
				if (PotentialNewDistance < MaxDistance)
				{
					//Move camera
					//camera.transform.position += dollyDirection * ratio;
					camera.transform.position = newPosition;

					//Move rotate root to center of screen?
					LayerMask LayerMask = Physics.DefaultRaycastLayers; //tODO - make variable?
					Ray ray = camera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
					RaycastHit hit;
					if (Physics.Raycast(ray, out hit, float.PositiveInfinity, LayerMask) == true)
					{
						camera.transform.parent = null; //detach from root so root can move
						transform.position = hit.point; //move rotate root
						camera.transform.SetParent(transform, true);//reattach to rotate root and keep world position
					}
				}



            }
			else
			{
				Debug.LogError("Failed to find camera. Either tag your cameras MainCamera, or set one in this component.", this);
			}

			
		}

	}
}