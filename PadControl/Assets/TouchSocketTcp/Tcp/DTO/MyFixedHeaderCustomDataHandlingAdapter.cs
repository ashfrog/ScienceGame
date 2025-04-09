using System;
using System.Collections.Generic;
using System.Text;
using TouchSocket.Sockets;
using TouchSocket.Core.Config;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Plugins;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Core;

/// <summary>
/// 模板解析“固定包头”数据适配器
/// </summary>
public class MyFixedHeaderCustomDataHandlingAdapter : CustomFixedHeaderDataHandlingAdapter<DTOInfo>
{
    public MyFixedHeaderCustomDataHandlingAdapter()
    {
        this.MaxPackageSize = 1024*1024;
    }

    /// <summary>
    /// 接口实现，指示固定包头长度
    /// </summary>
    public override int HeaderLength => 12;

    /// <summary>
    /// 获取新实例
    /// </summary>
    /// <returns></returns>
    protected override DTOInfo GetInstance()
    {
        return new DTOInfo();
    }
}

public class DTOInfo : IFixedHeaderRequestInfo
{
    private int bodyLength;
    /// <summary>
    /// 接口实现，标识数据长度
    /// </summary>
    public int BodyLength
    {
        get { return bodyLength; }
    }

    private int dataType;
    /// <summary>
    /// 自定义属性，标识数据类型
    /// </summary>
    public int DataType
    {
        get { return dataType; }
    }

    private int orderType;
    /// <summary>
    /// 自定义属性，标识指令类型
    /// </summary>
    public int OrderType
    {
        get { return orderType; }
    }

    private byte[] body;
    /// <summary>
    /// 自定义属性，标识实际数据
    /// </summary>
    public byte[] Body
    {
        get { return body; }
    }


    public bool OnParsingBody(byte[] body)
    {
        if (body.Length == this.bodyLength)
        {
            this.body = body;
            return true;
        }
        return false;
    }


    public bool OnParsingHeader(byte[] header)
    {
        //在该示例中，第一个字节表示后续的所有数据长度，但是header设置的是12，所以后续还应当接收length-12个长度。
        this.bodyLength = TouchSocketBitConverter.Default.ToInt32(header, 0) - 12;
        this.dataType = TouchSocketBitConverter.Default.ToInt32(header, 4);
        this.orderType = TouchSocketBitConverter.Default.ToInt32(header, 8);
        return true;
    }
}
