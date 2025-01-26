using System.Runtime.InteropServices;

namespace SoulEngine.Rendering;

[StructLayout(LayoutKind.Sequential, Pack = 0, Size = 4 * sizeof(uint))]
public unsafe struct JointIndices
{
    private fixed uint data[4];

    public JointIndices(uint a, uint b, uint c, uint d)
    {
        data[0] = a;
        data[1] = b;
        data[2] = c;
        data[3] = d;
    }

    public uint this[int index]
    {
        get
        {
            if (index < 0 || index > 4)
                throw new IndexOutOfRangeException("Joint index must be in range 0-4");
            return data[index];
        }
        set
        {
            if (index < 0 || index > 4)
                throw new IndexOutOfRangeException("Joint index must be in range 0-4");
            data[index] = value;
        }
    }

    public override string ToString()
    {
        return $"JointIndices {{ {data[0]}, {data[1]}, {data[2]}, {data[3]} }}";
    }
}