using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State
{
    NONE,
    START,
    END,
}


public class Controller : MonoBehaviour
{
    [SerializeField] private HexagonMapController mapController = null;
    [SerializeField] private Camera _camera = null;
    
    [SerializeField] private State _state = State.NONE;

    [SerializeField] private HexPosition _startCoords;
    [SerializeField] private HexPosition _endCoords;


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

    }


}
