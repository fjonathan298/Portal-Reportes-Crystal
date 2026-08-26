using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;


namespace CRUFL_CS_ExchangeRate
{
    [ComVisible(true), ClassInterface(ClassInterfaceType.None), Guid("E82DD73A-B76D-4C69-8C67-CD135C5111A3")]
    public class ExchangeUfl : IExchangeUfl
    {
        public double ConvertUSDollarsToCDN(double usd)
        {
            if (usd > Double.MaxValue)
            {
                throw new Exception("Value submitted is larger than the maximum value allowed for a double.");
            }
            return (usd * 1.45);
        }

    }


}
