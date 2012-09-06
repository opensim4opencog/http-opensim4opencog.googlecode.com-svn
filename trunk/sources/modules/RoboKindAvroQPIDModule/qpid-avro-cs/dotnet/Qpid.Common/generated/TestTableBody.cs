
using Apache.Qpid.Buffer;
using System.Text;

namespace Apache.Qpid.Framing
{
  ///
  /// This class is autogenerated
  /// Do not modify.
  ///
  /// @author Code Generator Script by robert.j.greig@jpmorgan.com
  public class TestTableBody : AMQMethodBody , IEncodableAMQDataBlock
  {
    public const int CLASS_ID = 120; 	
    public const int METHOD_ID = 30; 	

    public FieldTable Table;    
    public byte IntegerOp;    
    public byte StringOp;    
     

    protected override ushort Clazz
    {
        get
        {
            return 120;
        }
    }
   
    protected override ushort Method
    {
        get
        {
            return 30;
        }
    }

    protected override uint BodySize
    {
    get
    {
        
        return (uint)
        (uint)EncodingUtils.EncodedFieldTableLength(Table)+
            1 /*IntegerOp*/+
            1 /*StringOp*/		 
        ;
         
    }
    }

    protected override void WriteMethodPayload(ByteBuffer buffer)
    {
        EncodingUtils.WriteFieldTableBytes(buffer, Table);
            buffer.Put(IntegerOp);
            buffer.Put(StringOp);
            		 
    }

    protected override void PopulateMethodBodyFromBuffer(ByteBuffer buffer)
    {
        Table = EncodingUtils.ReadFieldTable(buffer);
        IntegerOp = buffer.GetByte();
        StringOp = buffer.GetByte();
        		 
    }

    public override string ToString()
    {
        StringBuilder buf = new StringBuilder(base.ToString());
        buf.Append(" Table: ").Append(Table);
        buf.Append(" IntegerOp: ").Append(IntegerOp);
        buf.Append(" StringOp: ").Append(StringOp);
         
        return buf.ToString();
    }

    public static AMQFrame CreateAMQFrame(ushort channelId, FieldTable Table, byte IntegerOp, byte StringOp)
    {
        TestTableBody body = new TestTableBody();
        body.Table = Table;
        body.IntegerOp = IntegerOp;
        body.StringOp = StringOp;
        		 
        AMQFrame frame = new AMQFrame();
        frame.Channel = channelId;
        frame.BodyFrame = body;
        return frame;
    }
} 
}