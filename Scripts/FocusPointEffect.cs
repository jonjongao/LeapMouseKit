using UnityEngine;
using System.Collections;

public class FocusPointEffect : MonoBehaviour
{
    public bool allToggle = false;
    private Vector3 focusPoint;
    public FocusTarget[] focus;
    [System.Serializable]
    public struct FocusTarget
    {
        public GameObject target;
        public Vector2 range;
        public float time;
        public bool reverse;
        public bool x;
        public bool y;
    }
    private Vector3[] _defaultPosition;

    void Awake()
    {
        _defaultPosition = new Vector3[focus.Length];
        for (int i = 0; i < focus.Length; i++)
        {
            _defaultPosition[i] = focus[i].target.transform.position;
        }
    }

    void Update()
    {
        if (allToggle)
        {
            #region 偽3D效果
            foreach (FocusTarget f in focus)
            {
                Vector3 _point = new Vector3(f.range.x, f.range.y, f.target.transform.position.z) - new Vector3(focusPoint.x * f.range.x, focusPoint.y * f.range.y, 0);
                Vector3 _velocity = Vector3.zero;
                int _face = -1;
                if (f.reverse)
                    _face = 1;
                else
                    _face = -1;


                if (f.x)
                    f.target.transform.position = Vector3.SmoothDamp(f.target.transform.position, new Vector3(_point.x * _face, f.target.transform.position.y * _face, _point.z), ref _velocity, f.time);
                if (f.y)
                    f.target.transform.position = Vector3.SmoothDamp(f.target.transform.position, new Vector3(f.target.transform.position.x * _face, _point.y * _face, _point.z), ref _velocity, f.time);

            }
            #endregion
        }
        else
        {
            for (int i = 0; i < focus.Length; i++)
            {
                focus[i].target.transform.position = _defaultPosition[i];
            }
        }

    }

    public void SetFocusPoint(Vector3 pointer)
    {
        focusPoint = pointer;
    }

    public void ToggleFocusEffect(bool on)
    {
        allToggle = on;
    }
}
