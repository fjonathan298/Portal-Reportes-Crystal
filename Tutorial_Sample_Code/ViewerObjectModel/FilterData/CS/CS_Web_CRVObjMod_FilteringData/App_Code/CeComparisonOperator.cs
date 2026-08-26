using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public enum  CeComparisonOperator
{
    EqualTo,
    LessThan,
    GreaterThan,
    LessThan_or_EqualTo,
    GreaterThan_or_EqualTo,
    Not_EqualTo
}