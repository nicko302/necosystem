using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotion : MonoBehaviour
{
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _smoothing = 5f;
    [SerializeField] private Vector2 _range = new Vector2(100, 100);
    
    private Vector3 _targetPosition;
    private Vector3 _input;

    public bool enableFreecam;

    private float _initialSpeed;
    private Vector3 _lastPosition;

    public CameraZoom cameraZoom;
    public GameObject mainCam;


    private void Awake()
    {
        _targetPosition = transform.position;
        _initialSpeed = _speed;
        _lastPosition = mainCam.transform.position;
    }

    private void Start()
    {
        enableFreecam = false;
    }
    private void HandleInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 right = transform.right * x;
        Vector3 forward = transform.forward * z;

        _input = (forward + right).normalized;
    }

    private void Move()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
            _speed = _speed + 0.75f;
        else if (Input.GetKeyDown(KeyCode.LeftControl))
            _speed = _speed - 0.75f;
        else if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.LeftShift))
            _speed = _initialSpeed;

        Vector3 nextTargetPosition = _targetPosition + _input * _speed;
        if (IsInBounds(nextTargetPosition)) _targetPosition = nextTargetPosition;
        transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * _smoothing);

        //Debug.Log(_speed);
    }

    private bool IsInBounds(Vector3 position)
    {
        return position.x > -_range.x &&
               position.x < _range.x &&
               position.z > -_range.y &&
               position.z < _range.y;
    }

    private void CheckForFreecam()
    {
        if (enableFreecam)
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                //
                //+++++++++++++++++++++++mainCam.transform.position = _lastPosition;
                //mainCam.transform.rotation = Quaternion.Euler(Vector3.zero);
                mainCam.transform.localEulerAngles = Vector3.zero;
                mainCam.transform.position = _lastPosition;

                enableFreecam = false;
                Debug.Log("V unpressed");
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                _lastPosition = mainCam.transform.position;
                enableFreecam = true;
                Debug.Log("V pressed");
            }
        }
    }

    private void Update()
    {
        CheckForFreecam();

        if (!enableFreecam)
        {
            HandleInput();
            Move();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 5f);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(_range.x * 2f, 5f, _range.y * 2f));
    }
}
