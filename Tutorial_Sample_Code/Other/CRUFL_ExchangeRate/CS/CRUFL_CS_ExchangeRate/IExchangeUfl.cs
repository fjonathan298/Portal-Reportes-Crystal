using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CRUFL_CS_ExchangeRate
{
    [ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("733A2949-F66E-4EF6-8D82-2BA87065DDF5")]
    public interface IExchangeUfl
    {
        double ConvertUSDollarsToCDN(double usd);
    }
}
