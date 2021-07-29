using System;
using UnityEngine;

public enum HexagonType
{
    FLAT_TOPPED,
    POINTY_TOPPED
}

public enum HexType
{
    LAND,
    MOUNTAIN,
    LAVA,
    WHATER,
    DEFAULT,
}

public enum HexObsticle
{
    OBSTICLE,
    NONE,
}

[Serializable]
public struct HexPosition
{
    [SerializeField]
    private bool _isInitialize;
    public bool IsInitialize => _isInitialize;

    [SerializeField]
    private int _xIndex;

    public int X => _xIndex;

    [SerializeField]
    private int _yIndex;
    public int Y => _yIndex;

    public HexPosition(int x, int y)
    {
        _xIndex = x;
        _yIndex = y;
        _isInitialize = true;
    }
}

public struct HexCell
{
    public Vector3 position;
    public float height;
    public HexType hexType;
    public Transform transform;

    public HexCell(Vector3 position, Transform cellTransform, float height = 1, HexType hexType = HexType.DEFAULT)
    {
        this.position = position;
        this.height = height;
        this.hexType = hexType;
        this.transform = cellTransform;
    }
}

public struct HexCellDescriptor
{
    private bool _obsticle;
    public bool Obsticle => _obsticle;

    private HexType _hexType;
    public HexType HexType => _hexType;

    public HexCellDescriptor(HexType hexType, bool obsticle)
    {
        _obsticle = obsticle;
        _hexType = hexType;
    }
}

public struct HexNode
{
    private HexPosition[] _links;
    public HexPosition[] Links => _links;

    private HexPosition _node;
    public HexPosition Node => _node;

    public HexNode (HexPosition nodePose, HexPosition[] nodeLinks)
    {
        _node = nodePose;
        _links = nodeLinks;
    }
}
