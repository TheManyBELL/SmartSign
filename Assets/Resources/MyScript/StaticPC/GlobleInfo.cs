using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

/// <summary>
/// 手部模式，right表示左手，left表示右手
/// </summary>
public enum HandMode { right, left }

/// <summary>
/// 射线信息
/// </summary>
public struct RayInfo
{
    public Vector3 startPoint;
    public Vector3 endPoint;
    public bool isActive;
    public HandMode handMode;

}

/// <summary>
/// 智能标识：线段，用于静态点云场景
/// </summary>
public struct SegmentInfo
{
    public Vector3 startPoint;
    public Vector3 endPoint;
}


/// <summary>
/// 智能标识：旋转标识和按压标识，用于静态点云场景
/// </summary>
public struct SymbolInfo
{
    public Vector3 up;
    public Vector3 position;
}

/// <summary>
/// 创造自定义角色 信息结构体，目前只能选择角色绑定的相机类型
/// </summary>
public struct CreateMMOCharacterMessage : NetworkMessage
{
    public CameraMode mode;
}

/// <summary>
/// 创建场景物体，只能由 AR端创建，方便拖动场景与真实场景匹配
/// </summary>
public struct CreateEnvironmentMessage : NetworkMessage
{
    public int startNumber;
    public int endNumber;
}

/// <summary>
/// 创建SmartSign物体
/// </summary>
public struct CreateSmartSignMessage : NetworkMessage
{
    public int smartSignNumber;
}

/// <summary>
/// 相机模式，AR相机或者VR相机
/// </summary>
public enum CameraMode { AR, VR }

public enum SymbolMode { ARROW=0,ROTATE,PRESS}

/// <summary>
/// [VR端处理所有计算]完整的标识信息数据结构
/// DPC:dynamic point cloud VR calculate only
/// </summary>
public struct DPCArrow
{
    public int index; // 此Arrow在同步列表中的下标
    // 初始数据
    public Vector3 startPoint;// 线段起点
    public Vector3 endPoint; // 线段终点
    // 根据遮挡重计算
    public List<Vector3> curvePointList; // 实时曲线
};


public struct DPCSymbol
{
    public int index;
    // 初始数据
    public Vector3 up;
    public Vector3 position;
    //  根据遮挡重计算
    public Vector3 up_new;
    public Vector3 position_new;
}

public static class GlobleInfo
{
    public static CameraMode ClientMode;

}
