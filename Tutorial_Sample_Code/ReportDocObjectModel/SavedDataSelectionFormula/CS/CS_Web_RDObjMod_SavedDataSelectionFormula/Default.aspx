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
        <asp:Label ID="Label1" runat="server" Text="Display the following customers:"></asp:Label><br />
        <asp:Label ID="Label2" runat="server" Text="- last year's sales > $"></asp:Label>
        <asp:TextBox ID="lastYearsSales" runat="server"></asp:TextBox>&nbsp;<br />
        <asp:Label ID="Label3" runat="server" Text="- first letter of name is"></asp:Label>&nbsp;<asp:DropDownList
            ID="operatorValueList" runat="server">
        </asp:DropDownList>
        <asp:TextBox ID="letterOfName" runat="server" Height="16px" Width="98px"></asp:TextBox><br />
        <asp:Button ID="redisplay" runat="server" OnClick="redisplay_Click" Text="Redisplay Report" /><br />
        <asp:Label ID="formula" runat="server"></asp:Label><br />
        <CR:CrystalReportViewer ID="crystalReportViewer" runat="server" AutoDataBind="true" />
    
    </div>
    </form>
</body>
</html>
