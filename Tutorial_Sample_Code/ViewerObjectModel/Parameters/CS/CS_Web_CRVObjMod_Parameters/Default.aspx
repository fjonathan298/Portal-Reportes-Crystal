<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="Default.aspx.cs" Inherits="_Default" %>

<%@ Register Assembly="CrystalDecisions.Web, Version=14.0.2000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304"
    Namespace="CrystalDecisions.Web" TagPrefix="CR" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <br />
        <asp:ListBox ID="defaultParameterValuesList" runat="server" SelectionMode="Multiple">
        </asp:ListBox>
        &nbsp; &nbsp; &nbsp;
        <asp:Button ID="redisplay" runat="server" OnClick="redisplay_Click" Text="Redisplay Report" /><br />
        <br />
        <CR:CrystalReportViewer ID="crystalReportViewer" runat="server" AutoDataBind="true" />
    
    </div>
    </form>
</body>
</html>
