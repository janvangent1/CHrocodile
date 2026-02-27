using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CHRocodileLib;

public static class CHRLibHelper
{
    public static void ThrowIfError(Res_t res)
    {
        if (res == 0)
            return;

        throw new Exception(GetErrorCode(res));
    }

    public static string GetErrorCode(Res_t res)
    {
        long size = 0;
        Lib.ErrorCodeToString(res, null, ref size);

        var builder = new StringBuilder((int)size);
        var internalRes = Lib.ErrorCodeToString(res, builder, ref size);
        if (internalRes != 0)
            throw new Exception(GetErrorCode(internalRes));

        return builder.ToString();
    }
}

