using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;


namespace Lean.Touch
{
	//COPIED FROM LEAN FINGER TAP AND MODIFIED - cwm

	// This script fires events if a finger has been held for a certain amount of time without moving
	public class cwmDoubleTapHoldZoom : MonoBehaviour
	{
		// Event signature
		[System.Serializable] public class FingerEvent : UnityEvent<LeanFinger> { }

		// This class will store extra Finger data
		[System.Serializable]
		public class Link
		{
			public LeanFinger Finger; // The finger associated with this link
			public bool LastSet; // Was this finger held?
			public Vector2 TotalScaledDelta; // The total movement so we can ignore it if it gets too high
		}

		[Tooltip("Ignore fingers with StartedOverGui?")]
		public bool IgnoreStartedOverGui = true;

		[Tooltip("Ignore fingers with IsOverGui?")]
		public bool IgnoreIsOverGui;

		[Tooltip("Do nothing if this LeanSelectable isn't selected?")]
		public LeanSelectable RequiredSelectable;

		[Tooltip("The finger must be held for this many seconds")]
		public float MinimumAge = 1.0f;

		[Tooltip("The finger cannot move more than this many pixels relative to the reference DPI")]
		public float MaximumMovement = 5.0f;


		[Tooltip("The camera that will be zoomed (None = MainCamera)")]
		public Camera Camera;

		[Tooltip("Should the scaling be performanced relative to the finger center?")]
		public bool Relative;

		[Tooltip("How fast to move the camera dolly")]
		public float Speed = 0.005f;


		[Tooltip("The maximum distance allowed from the camera to the rotate root")]
		public float MaxDistance = 1000;

		// Called on the first frame the conditions are met
		public FingerEvent OnHeldDown;

		// Called on every frame the conditions are met
		public FingerEvent OnHeldSet;

		// Called on the last frame the conditions are met
		public FingerEvent OnHeldUp;

		// This stores all finger links
		private List<Link> links = new List<Link>();


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

		protected virtual void OnEnable()
		{
			// Hook events
			LeanTouch.OnFingerDown += OnFingerDown;
			LeanTouch.OnFingerSet += OnFingerSet;
			LeanTouch.OnFingerUp += OnFingerUp;
		
		}

		protected virtual void OnDisable()
		{
			// Unhook events
			LeanTouch.OnFingerDown -= OnFingerDown;
			LeanTouch.OnFingerSet -= OnFingerSet;
			LeanTouch.OnFingerUp -= OnFingerUp;
			
		}

		private void OnFingerDown(LeanFinger finger)
		{
			// Ignore?
			if (IgnoreStartedOverGui == true && finger.StartedOverGui == true)
			{
				return;
			}
			if (IgnoreIsOverGui == true && finger.IsOverGui == true)
			{
				return;
			}

			if (RequiredSelectable != null && RequiredSelectable.IsSelected == false)
			{
				return;
			}

			// Get link for this finger and reset
			var link = FindLink(finger, true);

			link.LastSet = false;
			link.TotalScaledDelta = Vector2.zero;
		}

		private void OnFingerSet(LeanFinger finger)
		{
			// Try and find the link for this finger
			var link = FindLink(finger, false);

			if (link != null)
			{
				// Has this finger been held for more than MinimumAge without moving more than MaximumMovement?
				//var set = finger.Age >= MinimumAge && link.TotalScaledDelta.magnitude < MaximumMovement;
				bool set;

				if (link.LastSet)
				{
					set = finger.Age >= MinimumAge && finger.TapCount == 1;
				}
				else
				{
					set = finger.Age >= MinimumAge && link.TotalScaledDelta.magnitude < MaximumMovement && finger.TapCount == 1;
				}

				link.TotalScaledDelta += finger.ScaledDelta;

				if (set == true && link.LastSet == false)
				{
					if (OnHeldDown != null)
					{					
						OnHeldDown.Invoke(finger);
					}
				}

				if (set == true)
				{
					if (OnHeldSet != null)
					{						
						OnHeldSet.Invoke(finger);
						SetDolly(finger);
					}
				}

				if (set == false && link.LastSet == true)
				{
					if (OnHeldUp != null)
					{						
						OnHeldUp.Invoke(finger);
					}
				}

				// Store last value
				link.LastSet = set;
			}
		}

		private void OnFingerUp(LeanFinger finger)
		{
			// Find link for this finger, and clear it
			var link = FindLink(finger, false);

			if (link != null)
			{
				links.Remove(link);

				if (link.LastSet == true)
				{
					if (OnHeldUp != null)
					{
						OnHeldUp.Invoke(finger);
					}
				}
			}
		}

		private Link FindLink(LeanFinger finger, bool createIfNull)
		{
			// Find existing link?
			for (var i = 0; i < links.Count; i++)
			{
				var link = links[i];

				if (link.Finger == finger)
				{
					return link;
				}
			}

			// Make new link?
			if (createIfNull == true)
			{
				var link = new Link();

				link.Finger = finger;

				links.Add(link);

				return link;
			}

			return null;
		}

		

		//cwm
		/// <summary>
        /// Sets the maximum allowed finger movement during the 'hold' period; if the finger moves more than this, it won't be detected as a tap and hold gesture
        /// </summary>
        /// <param name="newMaximumMovement"></param>
		public void SetMaximumMovement(float newMaximumMovement)
		{
			MaximumMovement = newMaximumMovement;
		}


		/// <summary>
        /// Moves the camera forward or backward
        /// TODO - implement maximum zoom?
        /// TODO - make percentage of distance so it's faster when zoomed out and slower when zoomed in
        /// </summary>
        /// <param name="finger"></param>
		protected void SetDolly(LeanFinger finger)
		{
			// Make sure the camera exists
			var camera = LeanTouch.GetCamera(Camera, gameObject);
			
			
			Vector3 dollyDirection = camera.transform.forward;
			if (Relative)
			{
				//TODO - clean up, maybe moe into some update since it already calculates the centerscreen there
				var pinchScreenCenter = finger.ScreenPosition;
				dollyDirection = camera.ScreenPointToRay(pinchScreenCenter).direction;
			}

			if (camera != null)
			{
				float DistanceFromTarget = Vector3.Distance(this.transform.position, camera.transform.position);
				float DistanceToMove = (DistanceFromTarget * (1 + finger.ScreenDelta.y) - DistanceFromTarget) * Speed * -1; //-1 so moving finger down zooms in (like google maps)

				var newPosition = camera.transform.position + dollyDirection * DistanceToMove;

				float PotentialNewDistance = Vector3.Distance(this.transform.position, newPosition);
				if(PotentialNewDistance < MaxDistance)
                {
					//Move camera
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