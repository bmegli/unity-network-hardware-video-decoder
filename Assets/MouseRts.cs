// A modified version of script from:
// http://www.andrejeworutzki.de/game-developement/unity-realtime-strategy-camera/

using UnityEngine;
using UnityEngine.EventSystems;

namespace Ev3devMapping
{

public class MouseRts : MonoBehaviour
{
	public int LevelArea = 100;

	public int ScrollArea = 25;
	public int ScrollSpeed = 25;
	public int DragSpeed = 25;

	public int ZoomSpeed = 25;
	public float ZoomMin = 0.5f;
	public int ZoomMax = 100;

	public int PanSpeed = 50;
	public int PanAngleMin = 25;
	public int PanAngleMax = 80;

	public int RotationSpeed=100;

	public float TouchRotationMinMagSquared = 1f; 
	public float TouchRotationMinAngle = 0.1f;

    private bool touchRotating = false;
    private Vector2 touchRotationStart = Vector2.zero;

	public float UpDownScale = 0.03f;
	public float RotateScale = 0.5f;
	public float MoveScale = 0.5f;

	public float TouchUpDownScale = 0.15f;
	public float TouchRotateScale = 0.015f;
	public float TouchMoveScale = 1f;


	private float yrotation;

	public void Start()
	{
		Cursor.visible=false;
		yrotation = transform.eulerAngles.y;
	}

	void Update()
	{
		Vector3 translation = Vector3.zero;

		// Zoom in or out
		float zoomDelta = CameraUpDown ();

		if (zoomDelta!=0.0f)
			translation -= Vector3.up * ZoomSpeed * zoomDelta;
		
		float xrotation = transform.eulerAngles.x - zoomDelta * PanSpeed;
		xrotation = Mathf.Clamp(xrotation, PanAngleMin, PanAngleMax);

		yrotation += CameraRotation() * RotationSpeed;

		transform.eulerAngles = new Vector3(0, yrotation, 0);

		translation += CameraMovement() * DragSpeed;

		// Keep camera within level and zoom area
		Vector3 desiredPosition = transform.position + transform.TransformDirection(translation);

		desiredPosition.x = Mathf.Clamp(desiredPosition.x, -LevelArea, LevelArea);
		desiredPosition.y = Mathf.Clamp(desiredPosition.y, ZoomMin, ZoomMax);
		desiredPosition.z = Mathf.Clamp(desiredPosition.z, -LevelArea, LevelArea);

		// Finally move camera parallel to world axis
		transform.position = desiredPosition;
		transform.eulerAngles = new Vector3(xrotation, yrotation, 0);
	}

	private float CameraUpDown()
	{
		float delta=0f;

		#if UNITY_STANDALONE
		delta = -Input.GetAxis("Mouse ScrollWheel")*Time.deltaTime;
		#else
		if (Input.touchCount == 2)
		{
			if (EventSystem.current.IsPointerOverGameObject (0) || EventSystem.current.IsPointerOverGameObject (1))
				return delta;

			Touch touchZero = Input.GetTouch(0);
			Touch touchOne = Input.GetTouch(1);

			Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
			Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

			float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

			float deltaMagnitudeDiff = (touchDeltaMag - prevTouchDeltaMag)/Mathf.Sqrt(Screen.width*Screen.width + Screen.height*Screen.height);
				 
			delta += deltaMagnitudeDiff * TouchUpDownScale;
		}

		#endif

		//delta += Input.GetAxis("CameraUpDown") * UpDownScale * Time.deltaTime;

		return delta;
	}

	private float CameraRotation()
	{
		float yrotation = 0.0f;
		#if UNITY_STANDALONE
		if (Input.GetMouseButton(1) && !EventSystem.current.IsPointerOverGameObject()) // RMB
			yrotation = Input.GetAxis("Mouse X") * Time.deltaTime;
		#else
		if (Input.touchCount == 2)
		{
			if (EventSystem.current.IsPointerOverGameObject (0) || EventSystem.current.IsPointerOverGameObject (1))
				return yrotation;

			if (!touchRotating)
			{
				touchRotationStart = Input.touches [1].position - Input.touches [0].position;
				touchRotating = touchRotationStart.sqrMagnitude > TouchRotationMinMagSquared;
			} 
			else
			{
				Vector2 currVector = Input.touches [1].position - Input.touches [0].position;
				float angleOffset = Vector2.Angle(touchRotationStart, currVector);

				if (angleOffset > TouchRotationMinAngle)
				{
					Vector3 LR = Vector3.Cross(touchRotationStart, currVector);
					// z > 0 left rotation, z < 0 right rotation
					yrotation -= Mathf.Sign(LR.z) * angleOffset * TouchRotateScale;

					touchRotationStart = currVector;
				}
			}
		}
		else
			touchRotating = false;

		#endif

		//yrotation += Input.GetAxis("CameraRotate") * RotateScale * Time.deltaTime;

		return yrotation;
	}

	private bool mouseDragging = false;
	private Vector3 CameraMovement()
	{
		Vector3 move = Vector3.zero;

		#if UNITY_STANDALONE

		if(mouseDragging)
		{
			move += new Vector3(Input.GetAxis("Mouse X") * Time.deltaTime, 0, 
			Input.GetAxis("Mouse Y") * Time.deltaTime);
		}

		mouseDragging = Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject();

		#else
		if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
		{
			if (EventSystem.current.IsPointerOverGameObject (0))
				return move;

			Touch touch = Input.GetTouch (0);

			move += new Vector3(touch.deltaPosition.x / Screen.width, 0, 
					touch.deltaPosition.y / Screen.height) * TouchMoveScale;				
		}

		#endif

	//	move += new Vector3(Input.GetAxis("CameraX"), 0,
	//		-Input.GetAxis("CameraY")) * Time.deltaTime * MoveScale;

		return move;
	}
}

} //namespace