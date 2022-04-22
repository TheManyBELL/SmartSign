using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

/// <summary>
/// �ֲ�ģʽ��right��ʾ���֣�left��ʾ����
/// </summary>
public enum HandMode { right, left }

/// <summary>
/// ������Ϣ
/// </summary>
public struct RayInfo
{
    public Vector3 startPoint;
    public Vector3 endPoint;
    public bool isActive;
    public HandMode handMode;

}

/// <summary>
/// ���ܱ�ʶ���߶Σ����ھ�̬���Ƴ���
/// </summary>
public struct SegmentInfo
{
    public Vector3 startPoint;
    public Vector3 endPoint;
}


/// <summary>
/// ���ܱ�ʶ����ת��ʶ�Ͱ�ѹ��ʶ�����ھ�̬���Ƴ���
/// </summary>
public struct SymbolInfo
{
    public Vector3 up;
    public Vector3 position;
}

/// <summary>
/// �����Զ����ɫ ��Ϣ�ṹ�壬Ŀǰֻ��ѡ���ɫ�󶨵��������
/// </summary>
public struct CreateMMOCharacterMessage : NetworkMessage
{
    public CameraMode mode;
}

/// <summary>
/// �����������壬ֻ���� AR�˴����������϶���������ʵ����ƥ��
/// </summary>
public struct CreateEnvironmentMessage : NetworkMessage
{
    public int startNumber;
    public int endNumber;
}

/// <summary>
/// ����SmartSign����
/// </summary>
public struct CreateSmartSignMessage : NetworkMessage
{
    public int smartSignNumber;
}

/// <summary>
/// ���ģʽ��AR�������VR���
/// </summary>
public enum CameraMode { AR, VR }

public enum SymbolMode { ARROW=0,ROTATE,PRESS}

public static class GlobleInfo
{
    public static CameraMode ClientMode;

}
