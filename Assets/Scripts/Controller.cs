using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum State
{
    NONE,
    START,
    END,
}

public struct DijkstrasCellDescriptor
{
    private bool _isInitialize;
    public bool IsInitialize => _isInitialize;

    private bool _isFinal;
    public bool IsFinal => _isFinal;

    private float _distance;
    public float Distance => _distance;

    private HexPosition _position;
    public HexPosition Position => _position;

    private HexPosition _fromPosition;
    public HexPosition FromPosition => _fromPosition;

    public DijkstrasCellDescriptor(HexPosition position, HexPosition fromPosition, float distance = float.MaxValue, bool isFinal = false)
    {
        _distance = distance;
        _fromPosition = fromPosition;
        _position = position;
        _isInitialize = true;
        _isFinal = false;
    }

    public void MarkAsFinal()
    {
        _isFinal = true;
    }

    public void Update(HexPosition fromPosition, float distance, bool isFinal = false)
    {
        if (_isFinal || !_isInitialize)
            return;
        _distance = distance;
        _fromPosition = fromPosition;
        _isFinal = isFinal;
    }

    public bool TryUpdate(HexPosition fromPosition, float distance, bool isFinal = false)
    {
        if (_isFinal || !_isInitialize)
            return false;
        _distance = distance;
        _fromPosition = fromPosition;
        _isFinal = isFinal;
        return true;
    }
}

public class Controller : MonoBehaviour
{
    [SerializeField] private HexagonMapController mapController = null;
    [SerializeField] private Camera _camera = null;
    
    [SerializeField] private State _state = State.NONE;

    [SerializeField] private HexPosition _startCoords;
    [SerializeField] private HexPosition _endCoords;

    private Dictionary<HexPosition, DijkstrasCellDescriptor> _dijkstrasCellDict = null;
    private List<DijkstrasCellDescriptor> _dijkstrasCellList = null;

    #region LifySycle
    private void Update()
    {
        var mousePos = Input.mousePosition;

        if (!Input.GetMouseButtonDown(0))
            return;

        if (_state == State.NONE)
            return;

        if (_state == State.START)
        {
            Ray endRay = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(endRay, out RaycastHit endHit))
            {
                if (mapController.TryGetHex(endHit.point, out HexPosition result))
                {
                    {
                        var Cell = mapController.HexCells[_startCoords.X,_startCoords.Y];
                        var cellDiscriptor = mapController.MapCellDescriptor[_startCoords.X,_startCoords.Y];
                        var mr = Cell.transform.GetComponent<MeshRenderer>();
                        if (cellDiscriptor.Obsticle)
                            mr.material.SetColor("_Color", Color.gray);
                        else if (cellDiscriptor.HexType == HexType.LAND)
                            mr.material.SetColor("_Color", Color.green);
                    }
                    {
                        _startCoords = result;
                        var Cell = mapController.HexCells[_startCoords.X,_startCoords.Y];
                        var cellDiscriptor = mapController.MapCellDescriptor[_startCoords.X,_startCoords.Y];
                        var mr = Cell.transform.GetComponent<MeshRenderer>();
                        mr.material.SetColor("_Color", Color.cyan);
                    }
                }
            }
        }
        
        if (_state == State.END)
        {
            Ray startRay = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(startRay, out RaycastHit startHit))
            {
                if (mapController.TryGetHex(startHit.point, out HexPosition result))
                {
                    {
                        var Cell = mapController.HexCells[_endCoords.X,_endCoords.Y];
                        var cellDiscriptor = mapController.MapCellDescriptor[_endCoords.X,_endCoords.Y];
                        var mr = Cell.transform.GetComponent<MeshRenderer>();
                        if (cellDiscriptor.Obsticle)
                            mr.material.SetColor("_Color", Color.gray);
                        else if (cellDiscriptor.HexType == HexType.LAND)
                            mr.material.SetColor("_Color", Color.green);
                    }
                    {
                        _endCoords = result;
                        var Cell = mapController.HexCells[_endCoords.X,_endCoords.Y];
                        var cellDiscriptor = mapController.MapCellDescriptor[_endCoords.X,_endCoords.Y];
                        var mr = Cell.transform.GetComponent<MeshRenderer>();
                        mr.material.SetColor("_Color", Color.blue);
                    }
                }
            }
        }

    }

    #endregion


    #region UNILS

    private int GetSearchIndex(List<DijkstrasCellDescriptor> list, Func<DijkstrasCellDescriptor, bool> handler)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var result = handler(list[i]);
            if (result)
                return i;
        }
        return -1;
    }

    private int GetMinDistance(List<DijkstrasCellDescriptor> list)
    {
        var index = -1;
        var tempResult = float.MaxValue;
        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            if (!item.IsInitialize || item.IsFinal)
                continue;

            var localResult = item.Distance;
            if (tempResult > localResult)
            {
                index = i;
                tempResult = localResult;
            }
        }
        return index;
    }

    #endregion

    #region ContexMenuFunc
    [ContextMenu("RefreshHex")]
    private void Refresh()
    {
        for (int i = 0; i < mapController.GridHeight; i++)
        {
            for (int j = 0; j < mapController.GridWidth; j++)
            {
                var cell = mapController.HexCells[i,j];
                var cellDiscriptor = mapController.MapCellDescriptor[i,j];
                var mr = cell.transform.GetComponent<MeshRenderer>();

                if (cellDiscriptor.Obsticle)
                {
                    mr.material.SetColor("_Color", Color.gray);
                    continue;
                }
                if (cellDiscriptor.HexType == HexType.LAND)
                {
                    mr.material.SetColor("_Color", Color.green);
                    continue;
                }
            }
        }
    }

    [ContextMenu("FindPath")]
    private void FindPath()
    {
        if (!(_startCoords.IsInitialize && _endCoords.IsInitialize))
            throw new Exception("uncorect arguments");

        var relationNodesMap = mapController.HexCellRelationMap;

        // Generate dijkstars distance list 
        //_dijkstrasCellDict = new Dictionary<HexPosition, DijkstrasCellDescriptor>();
        _dijkstrasCellList = new List<DijkstrasCellDescriptor>();
        for (int i = 0; i < mapController.GridHeight; i++)
        {
            for (int j = 0; j < mapController.GridWidth; j++)
            {
                var hexPos = new HexPosition(i, j);
                var _dijkstrasCell = new DijkstrasCellDescriptor(hexPos, _startCoords);
                //_dijkstrasCellDict.Add(hexPos, _dijkstrasCell);
                _dijkstrasCellList.Add(_dijkstrasCell);
            }
        }
        //var keys = _dijkstrasCellDict.Keys;

        for (int i = 0; i < _dijkstrasCellList.Count; i++)
        {
            Debug.Log("---------------------");
            Debug.Log($"item {i}");
            int currenNodeIndex;
            if (i == 0)
            {
                currenNodeIndex = GetSearchIndex(_dijkstrasCellList, el => el.Position == _startCoords);
            }
            else
            {
                currenNodeIndex = GetMinDistance(_dijkstrasCellList);
                var node = _dijkstrasCellList[currenNodeIndex];
                node.MarkAsFinal();
                _dijkstrasCellList[currenNodeIndex] = node;
            }

            var currenNode = _dijkstrasCellList[currenNodeIndex];

            var relationNode = relationNodesMap[currenNode.Position.X, currenNode.Position.Y];
            var links = relationNode.Links;
            Debug.Log($"item {i} stage Links");
            for (int j = 0; j < links.Length; j++)
            {
                Debug.Log($"item {i} stage Links link {j}");
                var linkPos = links[j];
                if (!linkPos.IsInitialize)
                {
                    continue;
                }
                var linkNodeIndex = GetSearchIndex(_dijkstrasCellList, el => el.Position == linkPos);
                var linkNode = _dijkstrasCellList[linkNodeIndex];
                if (linkNode.IsFinal)
                {
                    continue;
                }
                // TODO relate to type set Distance By default 1;
                var currentDist = currenNode.Distance == float.MaxValue ? 0f : currenNode.Distance;

                if (currentDist + relationNode.Distance < linkNode.Distance)
                {
                    linkNode.Update(currenNode.Position, currentDist + relationNode.Distance);
                    _dijkstrasCellList[linkNodeIndex] = linkNode;
                }
            }
            Debug.Log($"item {i} stage 3");
        }

        // DEBUG
        Debug.Log("---------------------");
        var sb = new StringBuilder();
        foreach (var item in _dijkstrasCellList)
        {
            sb.AppendLine($"hex with position -> x: {item.Position.X}; t: {item.Position.Y};");
            sb.AppendLine($"hex from position -> x: {item.FromPosition.X}; t: {item.FromPosition.Y};");
            sb.AppendLine($"hex with distance from start Distance = {item.Distance}");
        }
        Debug.Log(sb);
    }


    [ContextMenu("Show Distance")]
    private void ShowDistance()
    {
        var cellMap = mapController.MapCellDescriptor;
        for (int i = 0; i < mapController.GridHeight; i++)
        {
            for (int j = 0; j < mapController.GridWidth; j++)
            {
                var cell = cellMap[i, j];
                var dijasCellIndex = GetSearchIndex(_dijkstrasCellList, el => el.Position.X == i && el.Position.Y == j);
                var dijasCell = _dijkstrasCellList[dijasCellIndex];
                cell.TMP.text = dijasCell.Distance.ToString();
            }
        }
    }

    [ContextMenu("HideDistance")]
    private void HideDistance()
    {
        var cellMap = mapController.MapCellDescriptor;
        foreach (var item in cellMap)
        {
            item.TMP.text = "";
        }
    }
    #endregion
}
