using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public class MessageConstants
{
    public const string SUCCESS = "The action was successful.";
    public const string FAILURE = "The action was not successful: ";
    public const string NOT_ALLOWED = "You are not allowed to do this action.";
    public const string FORMAT_NOT_SUPPORTED = "That format is not supported.";
    public const string NO_MATCHES_FOUND = "No matches were found for the value submitted.";
}
