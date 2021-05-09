using System;
using System.Collections;
using System.Collections.Generic;
using MatchCube.Scripts.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Cube : MonoBehaviour
{
    #region Public Variables
    
    public Text[] numberTexts;
    public float force = 10f;
    [FormerlySerializedAs("EnteredCube")] public Cube enteredCube;
    public LayerMask RayMask;
    public bool isDestroying = false;
    #endregion

    #region Private Variables

    private bool _canMove = false;
    private Rigidbody _rigidbody;
    private Vector2 _deltaMousePosition;
    private ulong _number;

    #endregion

    #region Properties

    public bool CanMove
    {
        set { _canMove = value; }
    }
    public ulong Number
    {
        get => _number;
        set
        {
            _number = value;
            foreach (var t in numberTexts)
            {
                t.text = _number.ToString();
            }
        }
    }

    #endregion

    #region Unity Methods

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!_canMove) return;
        CheckTouchInput();
        CheckMouseInput();
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (!other.collider.CompareTag("NumberBox")) return;
        enteredCube = other.collider.GetComponent<Cube>();
    }

    private void OnCollisionExit(Collision other)
    {
        if(!other.collider.CompareTag("NumberBox")) return;
        var cube = other.collider.GetComponent<Cube>();
        if (cube == enteredCube)
        {
            enteredCube = null;
        }
    }
    #endregion

    #region Private Methods

    private void CheckTouchInput()
    {
        if (Input.touchCount <= 1) return;
        var t = Input.GetTouch(0);
        switch (t.phase)
        {
            case TouchPhase.Moved:
                if (Camera.main != null)
                {
                    var ray = Camera.main.ScreenPointToRay(t.position);
                    if (!Physics.Raycast(ray, out var hit, 100, RayMask)) return;
                    if (hit.collider == null) return;
                    var x = hit.point.x;
                    x = Mathf.Clamp(x, -3.9f, 3.9f);
                    _rigidbody.transform.position = new Vector3(x, 1, -2);
                }
                break;
            case TouchPhase.Canceled:
            case TouchPhase.Ended:
                _canMove = false;
                _rigidbody.AddForce(new Vector3(0,0,force),ForceMode.Impulse);
                GameManager.Instance.SetNextTime();
                break;
            case TouchPhase.Began:
                break;
            case TouchPhase.Stationary:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void CheckMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _deltaMousePosition = Input.mousePosition;
            GameManager.Instance.aimLineTransform.gameObject.SetActive(true);
            var pos = GameManager.Instance.aimLineTransform.position;
            pos.x = transform.position.x;
            GameManager.Instance.aimLineTransform.position = pos;
        }
        else if (Input.GetMouseButton(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, 100, RayMask)) return;
            if(hit.collider == null) return;
            var x = hit.point.x;
            x = Mathf.Clamp(x, -3.9f, 3.9f); 
            _rigidbody.transform.position = new Vector3(x, 1, -2);
            var pos = GameManager.Instance.aimLineTransform.position;
            pos.x = x;
            GameManager.Instance.aimLineTransform.position = pos;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            _canMove = false;
            _rigidbody.AddForce(new Vector3(0,0,force),ForceMode.Impulse);
            GameManager.Instance.SetNextTime();
            GameManager.Instance.aimLineTransform.gameObject.SetActive(false);
        }
    }

    #endregion
}
