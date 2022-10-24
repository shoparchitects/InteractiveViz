using UnityEngine;

namespace Lean.Touch
{
	// This script allows you to translate the current GameObject relative to the camera
    //CWM - fixed the y-value so the point only moves in the xz plane
	public class LeanTranslate_XZ : MonoBehaviour
	{
		[Tooltip("Ignore fingers with StartedOverGui?")]
		public bool IgnoreStartedOverGui = true;

		[Tooltip("Ignore fingers with IsOverGui?")]
		public bool IgnoreIsOverGui;

		[Tooltip("Ignore fingers if the finger count doesn't match? (0 = any)")]
		public int RequiredFingerCount;

		[Tooltip("Does translation require an object to be selected?")]
		public LeanSelectable RequiredSelectable;

		[Tooltip("The camera the translation will be calculated using (None = MainCamera)")]
		public Camera Camera;

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

		protected virtual void Update()
		{
			// Get the fingers we want to use
			var fingers = LeanSelectable.GetFingers(IgnoreStartedOverGui, IgnoreIsOverGui, RequiredFingerCount, RequiredSelectable);
            
			// Calculate the screenDelta value based on these fingers
			var screenDelta = LeanGesture.GetScreenDelta(fingers);

			if (fingers.Count > 0)
			{
				var screenPoint = fingers[0].ScreenPosition; //cwm

				if (screenDelta != Vector2.zero)
				{
					// Perform the translation
					if (transform is RectTransform)
					{
						TranslateUI(screenDelta);
					}
					else
					{
						//Translate(screenDelta);
						SetToPoint(screenPoint); //cwm
					}
				}
			}
		}

		protected virtual void TranslateUI(Vector2 screenDelta)
		{
			// Screen position of the transform
			var screenPoint = RectTransformUtility.WorldToScreenPoint(Camera, transform.position);

			// Add the deltaPosition
			screenPoint += screenDelta;

			// Convert back to world space
			var worldPoint = default(Vector3);

			if (RectTransformUtility.ScreenPointToWorldPointInRectangle(transform.parent as RectTransform, screenPoint, Camera, out worldPoint) == true)
			{
				transform.position = worldPoint;
			}
		}

		protected virtual void Translate(Vector2 screenDelta)
		{

			// Make sure the camera exists
			var camera = LeanTouch.GetCamera(Camera, gameObject);

			if (camera != null)
			{
                
                //save current y value
				float y = transform.position.y;

				// Screen position of the transform
				var screenPoint = camera.WorldToScreenPoint(transform.position);

				// Add the deltaPosition
				screenPoint += (Vector3)screenDelta;

				// Convert back to world space
				Vector3 newPos = camera.ScreenToWorldPoint(screenPoint);

                //set back to original y value
				newPos.y = y;

                //apply to transform
				transform.position = newPos;
			}
			else
			{
				Debug.LogError("Failed to find camera. Either tag your cameras MainCamera, or set one in this component.", this);
			}
		}

        //TESTING cwm
        private void SetToPoint(Vector2 screenPoint)
        {
			// Make sure the camera exists
			var camera = LeanTouch.GetCamera(Camera, gameObject);

			if (camera != null)
			{
				Plane xz = new Plane(Vector3.up, Vector3.zero);

				Ray ray = camera.ScreenPointToRay((Vector3)screenPoint);

				float enter = 0f;
                if(xz.Raycast(ray, out enter))
                {
					Vector3 hitPoint = ray.GetPoint(enter);
					transform.position = hitPoint;
                }
			}
			else
			{
				Debug.LogError("Failed to find camera. Either tag your cameras MainCamera, or set one in this component.", this);
			}
		}

	}
}