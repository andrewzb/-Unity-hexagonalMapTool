using System;
using System.Text;
using UnityEngine;
using TMPro;

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

    public static bool operator == (HexPosition c1, HexPosition c2)
    {
        return c1.Equals(c2);
    }

    public static bool operator != (HexPosition c1, HexPosition c2)
    {
        return !c1.Equals(c2);
    }

    public override bool Equals(object other)
    {
        if (other == null)
        {
            return false;
        }
        HexPosition objAsPart = (HexPosition)other;
        return Equals(objAsPart);
    }

    public bool Equals(HexPosition other)
    {
        if (!_isInitialize || !other.IsInitialize)
            return false;
        if (_xIndex == other.X && _yIndex == other.Y)
            return true;
        return false;
    }

    public override string ToString()
    {
        return $"x: {_xIndex}; y: {_yIndex}; isInitialize: {_isInitialize}";
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

    public TextMeshPro TMP;

    public HexCellDescriptor(HexType hexType, bool obsticle, TextMeshPro tmp)
    {
        _obsticle = obsticle;
        _hexType = hexType;
        TMP = tmp;
    }
}

public struct HexNode
{
    private HexPosition[] _links;
    public HexPosition[] Links => _links;

    private HexPosition _node;
    public HexPosition Node => _node;

    private float _distnce;
    public float Distance => _distnce;

    public HexNode (HexPosition nodePose, HexPosition[] nodeLinks)
    {
        _node = nodePose;
        _links = nodeLinks;
        _distnce = 1;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"pos -> x: {_node.X}; y: {_node.Y}");
        sb.AppendLine($"links => count: {_links.Length}");
        foreach (var link in _links)
            sb.AppendLine($"link => x: {link.X}; y: {link.Y}; init: {link.IsInitialize}");
        return sb.ToString();
    }
}
