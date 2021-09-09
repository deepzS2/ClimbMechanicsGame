using UnityEngine;

public class EdgeHanging : MonoBehaviour
{
    private bool _canHang = false;
    public bool isHanging { get; private set; }
    public bool isHangingOnFur { get; private set; }

    private MeshRenderer _meshRenderer;

    private Player _player;

    private Vector3 _newPosition;
    private Quaternion _newRotation;

    #region Chest raycast variables
    [Header("Chest raycast")]
    [SerializeField, Range(0, 10)]
    private float _maxHangDistance = 1f;

    [SerializeField, Range(0, 1)]
    private float _chestVerticalOffset = .5f;

    [SerializeField]
    private LayerMask _canHangLayers;
    #endregion

    #region Vertical raycast variables
    [Header("Vertical raycast")]
    [SerializeField, Range(0, 10)]
    private float _upVerticalOffset = 1f;
    #endregion

    #region Raycast variables
    // [A, B]
    private Vector3[] _raycastLine;

    private RaycastHit _hitHorizontal, _hitVertical;
    #endregion

    void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _player = GetComponent<Player>();
    }

    void Start()
    {
        _raycastLine = CalculateLinePoints();
    }

    void Update()
    {
        CheckForHanggableObject();

        if (_canHang && !isHanging)
        {
            HangOnObject();
        }
        else if (!_canHang && isHanging || isHanging && !_player.walkPressed)
        {
            isHanging = false;
        }

        if (isHanging)
        {
            Climb();
        }
    }

    void LateUpdate()
    {
        if (_newPosition != Vector3.zero)
        {
            transform.position = _newPosition;
            _newPosition = Vector3.zero;
        }

        if (_newRotation != Quaternion.identity)
        {
            transform.rotation = _newRotation;
            _newRotation = Quaternion.identity;
        }
    }

    // Helper to check the raycast :D
    void OnDrawGizmos()
    {
        Vector3 startPoint = transform.position + transform.up * _chestVerticalOffset;
        RaycastHit hit;

        if (Physics.Raycast(startPoint, transform.forward, out hit, _maxHangDistance, _canHangLayers))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(startPoint, -hit.normal * hit.distance);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(startPoint, transform.forward * _maxHangDistance);
        }
    }

    #region Methods
    private void CheckForHanggableObject()
    {
        _raycastLine = CalculateLinePoints();

        if (Physics.Raycast(_raycastLine[0], transform.forward, out _hitHorizontal, _maxHangDistance, _canHangLayers))
        {
            Vector3 verticalRaycastPos = _hitHorizontal.point;
            verticalRaycastPos += transform.forward * .1f;
            verticalRaycastPos.y += _upVerticalOffset;

            // Fur is a surface we can hang/grab!
            if (Physics.Raycast(verticalRaycastPos, Vector3.down, out _hitVertical, _upVerticalOffset, _canHangLayers) || _hitHorizontal.transform.CompareTag("Fur"))
            {
                _canHang = true;

                return;
            }
        }

        _canHang = false;

        return;
    }

    private void HangOnObject()
    {
        if (_player.walkPressed)
        {
            isHanging = true;

            // TODO: Slice of the player inside of the object
            if (!_hitHorizontal.transform.CompareTag("Fur"))
            {
                Vector3 bounds = _meshRenderer.bounds.extents;

                isHanging = true;

                Vector3 hangPosition = new Vector3(_hitHorizontal.point.x, _hitVertical.point.y - bounds.y, _hitHorizontal.point.z) - Vector3.Scale(transform.forward, bounds);

                _newPosition = hangPosition;
            }
            else
            {
                isHangingOnFur = true;

                _newPosition = _hitHorizontal.point;

                // Rotate the player as well!
                _newRotation = Quaternion.LookRotation(-_hitHorizontal.normal);
            }
        }
    }

    private void Climb()
    {
        if (_player.jumpPressed)
        {
            Vector3 size = _meshRenderer.bounds.extents;

            Vector3 climbedPosition = _hitVertical.point + Vector3.Scale(size, transform.forward) + Vector3.Scale(size, transform.up);

            _newPosition = climbedPosition;
        }
    }

    Vector3[] CalculateLinePoints()
    {
        Vector3[] returnArray = new Vector3[2];
        Vector3 pointA, pointB;

        // transform.up * _rayOffset = transform.position.y + _rayOffsetY
        pointA = pointB = transform.position + transform.up * _chestVerticalOffset;

        pointB += transform.forward * _maxHangDistance;

        returnArray.SetValue(pointA, 0);
        returnArray.SetValue(pointB, 1);

        return returnArray;
    }
    #endregion
}