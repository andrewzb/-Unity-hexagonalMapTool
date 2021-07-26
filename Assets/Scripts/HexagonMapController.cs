using System;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum HexLayoutType
{
    ODD_HORIZONTAL,
    //EVEN_HORIZONTAL,
    //ODD_VERTICAL,
    //EVEN_VERTICAL
    // cube coords
    // Axial coordinates
    //double-width” horizontal layout doubles column values
    //double-height” horizontal layout doubles row values
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
    [SerializeField] private Transform _plane = null;
    [SerializeField] private Camera _camera = null;
    [SerializeField] private GameObject _hexCellPrefab = null;
    [SerializeField] private Vector3 _flatTopDirection;
    [SerializeField] private Vector3 _pointyTopDirection;
    [SerializeField] private HexagonType _hexType = HexagonType.POINTY_TOPPED;
    [SerializeField] private HexGridSettings _gridSettings;

    private Transform[,] _mapCell;

    private void Update()
    {
        var mousePos = Input.mousePosition;

        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var hitpoint = hit.point;
            Debug.Log(hitpoint);
        }
    }

    #region ContexMenuFunc
    [ContextMenu("GenerateHexGrid")]
    private void GenerateHexGrid()
    {
        _mapCell = new Transform[_gridSettings.hexHeight, _gridSettings.hexWidth];
        //var hexList = new List<Transform>();
        for (int i = 0; i < _gridSettings.hexHeight; i++)
        {
            for (int j = 0; j < _gridSettings.hexWidth; j++)
            {
                var xPos = i % 2 == 0
                    ? j - j * (_gridSettings.hexRadius - _gridSettings.smallSide)
                    : j - j * (_gridSettings.hexRadius - _gridSettings.smallSide) + _gridSettings.smallSide * 0.5f;
                var yPos = i - i  * _gridSettings.hexRadius * 0.25f;
                var spawnHex = Instantiate(_hexCellPrefab, new Vector3(xPos, 0, yPos), Quaternion.identity);
                spawnHex.name = $"hex x: {j}, y: {i} ";
                spawnHex.transform.SetParent(transform);
                //hexList.Add(spawnHex.transform);
                _mapCell[i,j] = spawnHex.transform;
            }
        }

        var height = (_gridSettings.hexHeight + 0.5f) * _gridSettings.smallSide;
        var width = (_gridSettings.hexWidth - 1) * _gridSettings.hexRadius - (_gridSettings.hexWidth) / 2 * _gridSettings.hexRadius / 4;
        var position = new Vector3(height / 2 - _gridSettings.smallSide / 2, 0, width / 2 - _gridSettings.hexRadius / 2);
        _plane.localScale = new Vector3(height, 0, width);
        _plane.position = position;
    }

    [ContextMenu("GetSmallSide")]
    private void GetSmallSide()
    {
        _gridSettings.smallSide = _gridSettings.hexRadius * Mathf.Sin(60 * Mathf.Deg2Rad);
    }
    #endregion
}
