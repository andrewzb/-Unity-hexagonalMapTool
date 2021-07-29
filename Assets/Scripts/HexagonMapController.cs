using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;

public enum HexLayoutType
{
    ODD_HORIZONTAL,
    //EVEN_HORIZONTAL,
    //ODD_VERTICAL,
    //EVEN_VERTICAL
    // cube coords
    // Axial coordinates
    //double-width horizontal layout doubles column values
    //double-height horizontal layout doubles row values
    // ...
}

[Serializable]
public struct HexGridSettings
{
    public HexLayoutType hexLayoutType;
    public int hexWidth;
    public int hexHeight;
    public float hexRadius; // also a side a
    public float smallSide;
}

public class HexagonMapController : MonoBehaviour
{
    [SerializeField] private Transform _pointer = null;
    [SerializeField] private Transform _hexContainer = null;
    [SerializeField] private Transform _plane = null;
    [SerializeField] private GameObject _hexCellPrefab = null;
    [SerializeField] private Vector3 _flatTopDirection;
    [SerializeField] private Vector3 _pointyTopDirection;
    [SerializeField] private HexagonType _hexType = HexagonType.POINTY_TOPPED;
    [SerializeField] private HexGridSettings _gridSettings;

    private HexCell[,] _mapCell;
    private HexNode[,] _relationNodeMap;
    private HexCellDescriptor[,] _mapCellDescriptor;

    public HexCell[,] HexCells => _mapCell;
    public HexCellDescriptor[,] MapCellDescriptor => _mapCellDescriptor;
    public HexNode[,] HexCellRelationMap => _relationNodeMap;

    public int GridWidth => _gridSettings.hexWidth;
    public int GridHeight => _gridSettings.hexHeight;

    #region API
    public bool TryGetHex(Vector3 hitPoint, out HexPosition result)
    {
        var indexies = FindAproximateHex(hitPoint);
        if (!(indexies.X < 0 || indexies.X > _gridSettings.hexWidth - 1
        || indexies.Y < 0 || indexies.Y > _gridSettings.hexHeight - 1))
        {
            var cell = _mapCell[indexies.X, indexies.Y];
            _pointer.position = hitPoint;
            if (IsInCell(cell, hitPoint))
            {
                result = new HexPosition(indexies.X, indexies.Y);
                return true;
            }
        }
        result = new HexPosition();
        return false;
    }
    #endregion

    #region UNILS
    private bool IsInCell(HexCell cell, Vector3 hitpoint)
    {
        var point = new Vector3(hitpoint.x, 0, hitpoint.z);
        var points = GetHexagonPoints(cell.transform.position, hitpoint);
        var hexSquare = 3 * Mathf.Sqrt(3) / 2 * _gridSettings.hexRadius * 0.5f * _gridSettings.hexRadius * 0.5f;
        var triangle1 = GetTriangleSquare(point, points[0], points[1]);
        var triangle2 = GetTriangleSquare(point, points[1], points[2]);
        var triangle3 = GetTriangleSquare(point, points[2], points[3]);
        var triangle4 = GetTriangleSquare(point, points[3], points[4]);
        var triangle5 = GetTriangleSquare(point, points[4], points[5]);
        var triangle6 = GetTriangleSquare(point, points[5], points[0]);
        var triangleSumm = triangle1 + triangle2 + triangle3 +triangle4 + triangle5 + triangle6;
        return Mathf.Abs(hexSquare - triangleSumm) < 0.01f;
    }

    private float GetTriangleSquare(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        var a = Vector3.Distance(pointA, pointB);
        var b = Vector3.Distance(pointB, pointC);
        var c = Vector3.Distance(pointC, pointA);
        var p = (a + b + c) / 2;
        return Mathf.Sqrt(p * (p - a) * (p - b) * (p - c));
    }

    private Vector3[] GetHexagonPoints(Vector3 position, Vector3 hit)
    {
        var pointArray = new Vector3[6];
        pointArray[0] = position +  new Vector3(0, 0, 0.5f * _gridSettings.hexRadius); //1
        pointArray[1] = position +  new Vector3(0.5f * _gridSettings.smallSide, 0, 0.25f * _gridSettings.hexRadius); //2
        pointArray[2] = position +  new Vector3(0.5f * _gridSettings.smallSide, 0, - 0.25f * _gridSettings.hexRadius); //3
        pointArray[3] = position +  new Vector3(0, 0, - 0.5f * _gridSettings.hexRadius); //4
        pointArray[4] = position +  new Vector3(- 0.5f * _gridSettings.smallSide, 0, - 0.25f * _gridSettings.hexRadius); //5
        pointArray[5] = position +  new Vector3(- 0.5f * _gridSettings.smallSide, 0, 0.25f * _gridSettings.hexRadius); //6
        // for debug
        var debugPoint1 = position +  new Vector3(0, 0.5f, 0.5f * _gridSettings.hexRadius); //1
        var debugPoint2 = position +  new Vector3(0.5f * _gridSettings.smallSide, 0.5f, 0.25f * _gridSettings.hexRadius); //2
        var debugPoint3 = position +  new Vector3(0.5f * _gridSettings.smallSide, 0.5f, - 0.25f * _gridSettings.hexRadius); //3
        var debugPoint4 = position +  new Vector3(0, 0.5f, - 0.5f * _gridSettings.hexRadius); //4
        var debugPoint5 = position +  new Vector3(- 0.5f * _gridSettings.smallSide, 0.5f, - 0.25f * _gridSettings.hexRadius); //5
        var debugPoint6 = position +  new Vector3(- 0.5f * _gridSettings.smallSide, 0.5f, 0.25f * _gridSettings.hexRadius); //6

        Debug.DrawLine(debugPoint1, debugPoint2, Color.green);
        Debug.DrawLine(debugPoint2, debugPoint3, Color.green);
        Debug.DrawLine(debugPoint3, debugPoint4, Color.green);
        Debug.DrawLine(debugPoint4, debugPoint5, Color.green);
        Debug.DrawLine(debugPoint5, debugPoint6, Color.green);
        Debug.DrawLine(debugPoint6, debugPoint1, Color.green);

        Debug.DrawLine(new Vector3(hit.x, 0.5f, hit.z), debugPoint1, Color.red);
        Debug.DrawLine(new Vector3(hit.x, 0.5f, hit.z), debugPoint2, Color.red);
        Debug.DrawLine(new Vector3(hit.x, 0.5f, hit.z), debugPoint3, Color.red);
        Debug.DrawLine(new Vector3(hit.x, 0.5f, hit.z), debugPoint4, Color.red);
        Debug.DrawLine(new Vector3(hit.x, 0.5f, hit.z), debugPoint5, Color.red);
        Debug.DrawLine(new Vector3(hit.x, 0.5f, hit.z), debugPoint6, Color.red);

        return pointArray;
    }

    private HexPosition FindAproximateHex(Vector3 pos)
    {
        var yclick = pos.z + 0.375f * _gridSettings.hexRadius;
        var xclick = pos.x;
        var y = 0.750f * _gridSettings.hexRadius;
        var row = (int)Mathf.Clamp(Mathf.Floor(yclick / y), -1, _gridSettings.hexHeight - 1);
        var column = (int)Mathf.Clamp(Mathf.Floor(
            row % 2 == 0
            ? (xclick + 0.5f * _gridSettings.smallSide ) / _gridSettings.smallSide
            : xclick / _gridSettings.smallSide), -1, _gridSettings.hexWidth - 1);
        return new HexPosition(row, column);
    }

    private HexPosition[] GetNeighbors(HexPosition hexPos)
    {
        var leftCoordsX = hexPos.X - 1;
        var rightCoordsX = hexPos.X + 1;

        var topRightCoordsX = hexPos.Y % 2 == 0 ? hexPos.X : hexPos.X + 1;
        var topLeftCoordsX = hexPos.Y % 2 == 0 ? hexPos.X - 1 : hexPos.X;

        var bottomRightCoordsX = hexPos.Y % 2 == 0 ? hexPos.X : hexPos.X + 1;
        var bottomLeftCoordsX = hexPos.Y % 2 == 0 ? hexPos.X - 1 : hexPos.X;

        var links = new HexPosition[6];

        if (topRightCoordsX < GridWidth && hexPos.Y + 1 < GridHeight)
            links[0] = new HexPosition(topRightCoordsX, hexPos.Y + 1);
        if (rightCoordsX < GridWidth)
            links[1] = new HexPosition(rightCoordsX, hexPos.Y);
        if (bottomRightCoordsX < GridWidth && hexPos.Y - 1 >= 0)
            links[2] = new HexPosition(bottomRightCoordsX, hexPos.Y - 1);
        if (bottomLeftCoordsX >= 0 && hexPos.Y - 1 >= 0)
            links[3] = new HexPosition(bottomLeftCoordsX, hexPos.Y - 1);
        if (leftCoordsX >= 0)
            links[4] = new HexPosition(leftCoordsX, hexPos.Y);
        if (topLeftCoordsX >= 0 && hexPos.Y + 1 < GridHeight)
            links[5] = new HexPosition(topLeftCoordsX, hexPos.Y + 1);
        /*
        Debug.Log($"links[0] => x: {links[0].X}; y: {links[0].Y}; init: {links[0].IsInitialize} ");
        Debug.Log($"links[1] => x: {links[1].X}; y: {links[1].Y}; init: {links[1].IsInitialize} ");
        Debug.Log($"links[2] => x: {links[2].X}; y: {links[2].Y}; init: {links[2].IsInitialize} ");
        Debug.Log($"links[3] => x: {links[3].X}; y: {links[3].Y}; init: {links[3].IsInitialize} ");
        Debug.Log($"links[4] => x: {links[4].X}; y: {links[4].Y}; init: {links[4].IsInitialize} ");
        Debug.Log($"links[5] => x: {links[5].X}; y: {links[5].Y}; init: {links[5].IsInitialize} ");
        */
        
        return links;
    }

    #endregion

    #region ContexMenuFunc
    [ContextMenu("GenerateHexGrid")]
    private void GenerateHexGrid()
    {
        _mapCell = new HexCell[_gridSettings.hexHeight, _gridSettings.hexWidth];
        _mapCellDescriptor = new HexCellDescriptor[_gridSettings.hexHeight, _gridSettings.hexWidth];
        for (int i = 0; i < _gridSettings.hexHeight; i++)
        {
            for (int j = 0; j < _gridSettings.hexWidth; j++)
            {
                var xPos = i % 2 == 0
                    ? j - j * (_gridSettings.hexRadius - _gridSettings.smallSide)
                    : j - j * (_gridSettings.hexRadius - _gridSettings.smallSide) + _gridSettings.smallSide * 0.5f;
                var yPos = i - i * _gridSettings.hexRadius * 0.25f;
                var spawnHex = Instantiate(_hexCellPrefab, new Vector3(xPos, 0, yPos), Quaternion.identity);
                spawnHex.name = $"hex x: {j}, y: {i} ";
                spawnHex.transform.SetParent(_hexContainer);
                var tmp = spawnHex.GetComponentInChildren<TextMeshPro>();
                _mapCell[i, j] = new HexCell(spawnHex.transform.position, spawnHex.transform);
                var IsObsticle = UnityEngine.Random.Range(0, 10) % 5 == 0;
                _mapCellDescriptor[i, j] = new HexCellDescriptor(HexType.LAND, IsObsticle, tmp);
            }
        }

        var height = (_gridSettings.hexHeight + 0.5f) * _gridSettings.smallSide;
        var width = (_gridSettings.hexWidth - 1) * _gridSettings.hexRadius - (_gridSettings.hexWidth) / 2 * _gridSettings.hexRadius / 4;
        var position = new Vector3(height / 2 - _gridSettings.smallSide / 2, 0, width / 2 - _gridSettings.hexRadius / 2);
        _plane.localScale = new Vector3(height, 0.1f, width);
        _plane.position = position;
        // generate relation nodes
        _relationNodeMap = new HexNode[_gridSettings.hexHeight, _gridSettings.hexWidth];
        for (int i = 0; i < _gridSettings.hexHeight; i++)
        {
            for (int j = 0; j < _gridSettings.hexWidth; j++)
            {
                var hexCell = _mapCell[i,j];
                var pos = new HexPosition(i, j);
                var neighbors = GetNeighbors(pos);
                _relationNodeMap[i, j] = new HexNode(pos, neighbors);
            }
        }
        foreach (var item in _relationNodeMap)
        {
            Debug.Log(item);
        }
    }

    [ContextMenu("RemoveGrid")]
    private void RemoveGrid()
    {
        foreach (var cell in _mapCell)
        {
            DestroyImmediate(cell.transform.gameObject);
        }
        _plane.localScale = new Vector3(0, 0, 0);
        _plane.position = Vector3.zero;
    }

    [ContextMenu("CalculateSmallSide")]
    private void GetSmallSide()
    {
        _gridSettings.smallSide = _gridSettings.hexRadius * Mathf.Sin(60 * Mathf.Deg2Rad);
    }
    #endregion
}
