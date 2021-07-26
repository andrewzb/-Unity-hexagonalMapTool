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

public struct HexCell
{
    public Vector3 position;
    //public Vector3 flatToppedDirection;
    public Vector3 pointyToppedDirection;
    public float height;
    public HexType hexType;

    public HexCell(Vector3 position, Vector3 pointyToppedDirection, float height = 1, HexType hexType = HexType.DEFAULT)
    {
        this.position = position;
        this.pointyToppedDirection = pointyToppedDirection;
        this.height = height;
        this.hexType = hexType;
    }
}
