using UnityEngine;

public class EdgeHanging : MonoBehaviour
{
    private bool _canHang = false;
    public bool isHanging { get; private set; }

    private MeshRenderer _meshRenderer;

    private Player _player;

    private Vector3 _newPosition;

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

    //[SerializeField, Range(0, 10)]
    //private int _qualityCheck = 1;

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
    }

    void OnDrawGizmosSelected()
    {
        Vector3[] linePoints = CalculateLinePoints();

        Gizmos.color = Color.black;
        Gizmos.DrawLine(linePoints[0], linePoints[1]);
    }

    #region Methods
    // TODO: If is a surface that we can grab
    private void CheckForHanggableObject()
    {
        _raycastLine = CalculateLinePoints();

        if (Physics.Raycast(_raycastLine[0], transform.forward, out _hitHorizontal, _maxHangDistance, _canHangLayers))
        {
            Vector3 verticalRaycastPos = _hitHorizontal.point;
            verticalRaycastPos += transform.forward * .1f;
            verticalRaycastPos.y += _upVerticalOffset;

            if (Physics.Raycast(verticalRaycastPos, Vector3.down, out _hitVertical, _upVerticalOffset, _canHangLayers))
            {
                Debug.DrawLine(verticalRaycastPos, _hitVertical.point, Color.green);

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
            Vector3 bounds = _meshRenderer.bounds.extents;

            isHanging = true;

            Vector3 hangPosition = new Vector3(_hitHorizontal.point.x, _hitVertical.point.y - bounds.y, _hitHorizontal.point.z) - Vector3.Scale(transform.forward, bounds);

            _newPosition = hangPosition;
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