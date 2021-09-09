using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    private GameObject _player;
    private Inputs _playerInputs;
    private float _FOV;

    #region Camera
    [Header("Common settings")]
    [SerializeField]
    private float _sensitivity = 3f;

    [SerializeField]
    private CinemachineFreeLook _cinemachineFreeLook;

    [SerializeField]
    private CinemachineVirtualCamera _cinemachineVirtualCamera;

    [SerializeField, Range(0, 5)]
    private float _recenterTime = 1.5f;
    #endregion

    #region Zoom
    [Header("Zoom")]
    [SerializeField]
    private float _zoomSpeed = 5f;

    [SerializeField]
    private float _zoomFOV = 5f;

    [SerializeField]
    private float _normalFOV = 40f;
    #endregion

    #region Focus on enemy
    private GameObject _closeEnemy;
    private bool _focusOnEnemy;
    #endregion

    void Awake()
    {
        _playerInputs = new Inputs();

        _playerInputs.Camera.Enable();
        _playerInputs.Camera.Zoom.started += HandleZoomButton;
        _playerInputs.Camera.Zoom.canceled += HandleZoomButton;

        _playerInputs.Camera.Reset.started += HandleResetButton;

        _playerInputs.Camera.Focus.started += HandleFocusButton;
        _playerInputs.Camera.Focus.canceled += HandleFocusButton;

        _player = GameObject.FindGameObjectWithTag("Player");
    }

    void Start()
    {
        _cinemachineFreeLook.m_CommonLens = true;
        _cinemachineFreeLook.m_Lens.FieldOfView = _FOV = _normalFOV;
        _cinemachineFreeLook.m_RecenterToTargetHeading.m_RecenteringTime = _recenterTime;
        _cinemachineFreeLook.m_YAxisRecentering.m_RecenteringTime = _recenterTime;

        _cinemachineFreeLook.LookAt = _player.transform;
        _cinemachineFreeLook.Follow = _player.transform;
    }

    void Update()
    {
        MoveCamera();
        HandleFocusOnEnemy();
    }

    void LateUpdate()
    {
        float currentFOV = _cinemachineFreeLook.m_Lens.FieldOfView;

        if (currentFOV != _FOV) _cinemachineFreeLook.m_Lens.FieldOfView = Mathf.Lerp(_cinemachineFreeLook.m_Lens.FieldOfView, _FOV, Time.deltaTime * _zoomSpeed);
    }

    #region Methods
    void MoveCamera()
    {
        Vector2 mouseInput = _playerInputs.Camera.Move.ReadValue<Vector2>().normalized;

        if  (mouseInput.magnitude > 0)
        {
            mouseInput.x *= 180f;

            _cinemachineFreeLook.m_XAxis.Value += mouseInput.x * _sensitivity * Time.deltaTime;
            _cinemachineFreeLook.m_YAxis.Value += mouseInput.y * _sensitivity * Time.deltaTime;
        }
    }

    void HandleFocusOnEnemy()
    {
        if (_focusOnEnemy)
        {
            _cinemachineFreeLook.gameObject.SetActive(false);
            _cinemachineVirtualCamera.gameObject.SetActive(true);

            _closeEnemy = GameManager.FindClosestWithTag("Enemy", _player.transform.position);

            _cinemachineVirtualCamera.LookAt = _closeEnemy.transform;
        }
        else
        {
            _cinemachineFreeLook.gameObject.SetActive(true);
            _cinemachineVirtualCamera.gameObject.SetActive(false);

            _cinemachineFreeLook.m_LookAt = _player.transform;

            _cinemachineFreeLook.LookAt = _player.transform;
        }
    }

    void HandleZoomButton(InputAction.CallbackContext context)
    {
        bool isZooming = context.ReadValueAsButton();

        _FOV = isZooming ? _zoomFOV : _normalFOV;
    }

    void HandleResetButton(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            StartCoroutine(ResetCameraRoutine());
        }
    }

    void HandleFocusButton(InputAction.CallbackContext context) => _focusOnEnemy = context.ReadValueAsButton();
    #endregion

    #region Routines
    IEnumerator ResetCameraRoutine()
    {
        _cinemachineFreeLook.m_RecenterToTargetHeading.m_enabled = true;
        _cinemachineFreeLook.m_YAxisRecentering.m_enabled = true;

        _cinemachineFreeLook.m_RecenterToTargetHeading.RecenterNow();
        _cinemachineFreeLook.m_YAxisRecentering.RecenterNow();

        yield return new WaitForSeconds(_cinemachineFreeLook.m_RecenterToTargetHeading.m_RecenteringTime + .5f);

        _cinemachineFreeLook.m_RecenterToTargetHeading.CancelRecentering();
        _cinemachineFreeLook.m_YAxisRecentering.CancelRecentering();

        _cinemachineFreeLook.m_RecenterToTargetHeading.m_enabled = false;
        _cinemachineFreeLook.m_YAxisRecentering.m_enabled = false;

    }
    #endregion
}