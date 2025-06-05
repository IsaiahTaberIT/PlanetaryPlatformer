using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;

[ExecuteInEditMode]

public class AutoProfileScript : MonoBehaviour
{
    private GameLogicScript GameLogicScript;
    private SpriteShapeController ShapeController;
    private SpriteShape Shape;
    private Spline MySpline;

    [SerializeField] private bool _Horizontal = true;
    [SerializeField] private bool _TrueLengthMode = false;
    [SerializeField] private int _Points = 20;

    [SerializeField] private float _OffsetPrecision = 1f;

    [SerializeField] private float _Offset = 20;
    [SerializeField] private float _RotationRate = 10;
    [SerializeField] private float _Radius = 20;
     

    [ContextMenuItem(name: "AddRadius", function: nameof(AddRadius))]
   
    [SerializeField] private float _IncremantRadius = 15;
    const float Shift = 57.3f;

    [SerializeField] private float _ColliderOffset = 1f;
    private Vector2[] ColliderPoints;
    private PolygonCollider2D _Collider;





    // Start is called before the first frame update
    void Start()
    {
        ShapeController = GetComponent<SpriteShapeController>();
        MySpline = ShapeController.spline;
        GameLogicScript = GameObject.FindGameObjectWithTag("Logic").GetComponent<GameLogicScript>();
        _Collider = GetComponent<PolygonCollider2D>();
        Repair();
        _Collider.points = ColliderPoints;
    }

    [ContextMenu("Repair")]
    public void Repair()
    {
        ShapeController = GetComponent<SpriteShapeController>();
        MySpline = ShapeController.spline;
        MySpline.Clear();
        _Collider = GetComponent<PolygonCollider2D>();
       // Debug.Log("Repaired");

        RecalculateSpline();
        //Debug.Log(_Collider.points.Length);
       

    }


    // Update is called once per frame


    void RecalculateSpline()
    {
        if (_Horizontal)
        {
            HorizontalSpline();
        }
        else
        {
            VerticalSpline();
        }


    }


    void VerticalSpline()
    {
        MySpline.Clear();
        _Radius = (_Radius < 10) ? 10 : _Radius;

        float consistantSizeModifier = (_TrueLengthMode) ? _Radius / 100 : 1;
        Vector3 pointPosition;
        ColliderPoints = new Vector2[4];

        for (int i = 0; i < 2; i++)
        {
            ColliderPoints[i] = new Vector3(Mathf.Cos((_Offset / _OffsetPrecision) / 36 + _ColliderOffset / _Radius), Mathf.Sin((_Offset / _OffsetPrecision) / 36 + _ColliderOffset /_Radius), 0) * ((_Radius - 1) + (i * _RotationRate)) / 2;
            ColliderPoints[3-(i)] = new Vector3(Mathf.Cos((_Offset / _OffsetPrecision) / 36 - _ColliderOffset / _Radius), Mathf.Sin((_Offset / _OffsetPrecision) / 36 - _ColliderOffset / _Radius), 0) * ((_Radius - 1) + (i * _RotationRate)) / 2;

                

            pointPosition = new Vector3(Mathf.Cos((_Offset / _OffsetPrecision) / 36), Mathf.Sin((_Offset / _OffsetPrecision) / 36), 0) * ((_Radius-1) + (i* _RotationRate)) / 2;

            MySpline.InsertPointAt(i, pointPosition);

            MySpline.SetTangentMode(i, (ShapeTangentMode)1);
        }
        _Collider.points = ColliderPoints;
        ShapeController.enabled = false;
        ShapeController.enabled = true;
    }

    void HorizontalSpline()
    {

        MySpline.Clear();
        _Radius = (_Radius < 10) ? 10 : _Radius;


        ColliderPoints = new Vector2[_Points * 2];
 
        float consistantSizeModifier = (_TrueLengthMode) ? _Radius / 100 : 1;

        for (int i = 0; i < _Points; i++)
        {
            ColliderPoints[i] = new Vector3(Mathf.Cos((_Offset / _OffsetPrecision) / 36 + ((i - _Points / 2) / ((Shift / (-1 * _RotationRate))) / (_Points - 1)) / consistantSizeModifier), Mathf.Sin((_Offset / _OffsetPrecision) / 36 + ((i - _Points / 2) / ((Shift / (-1 * _RotationRate))) / (_Points - 1)) / consistantSizeModifier), 0) * (_Radius + _ColliderOffset) / 2;
            ColliderPoints[(2 *  _Points) - i - 1] = new Vector3(Mathf.Cos((_Offset / _OffsetPrecision) / 36 + ((i - _Points / 2) / ((Shift / (-1 * _RotationRate))) / (_Points - 1)) / consistantSizeModifier), Mathf.Sin((_Offset / _OffsetPrecision) / 36 + ((i - _Points / 2) / ((Shift / (-1 * _RotationRate))) / (_Points - 1)) / consistantSizeModifier), 0) * (_Radius - _ColliderOffset)/ 2;

            Vector3 pointPosition = new Vector3(Mathf.Cos((_Offset / _OffsetPrecision) / 36 + ((i - _Points / 2) / ((Shift / (-1 * _RotationRate))) / (_Points - 1)) / consistantSizeModifier), Mathf.Sin((_Offset / _OffsetPrecision) / 36 + ((i - _Points / 2) / ((Shift / (-1 * _RotationRate))) / (_Points - 1)) / consistantSizeModifier), 0) * _Radius / 2;

            MySpline.InsertPointAt(i, pointPosition);
            MySpline.SetTangentMode(i, (ShapeTangentMode)1);
        }

        //Debug.Log("Repaired2");
        _Collider.points = ColliderPoints;

        ShapeController.enabled = false;
        ShapeController.enabled = true;
    }

    public void AddRadius()
    {
        ShapeController = GetComponent<SpriteShapeController>();
        MySpline = ShapeController.spline;
        MySpline.Clear();


       
        _Radius += _IncremantRadius;
        RecalculateSpline();
    }
}
