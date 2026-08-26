<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="Default.aspx.cs" Inherits="_Default" %>

<%@ Register Assembly="CrystalDecisions.Web, Version=14.0.2000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304"
    Namespace="CrystalDecisions.Web" TagPrefix="CR" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
    <link href="/aspnet_client/System_Web/2_0_50701/CrystalReportWebFormViewer3/css/default.css"
        rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <table style="width: 100%">
            <tr>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                    <asp:Label ID="Label1" runat="server" Text="Select report elements to display"></asp:Label></td>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                    <asp:ListBox ID="listCRVReport" runat="server" SelectionMode="Multiple"></asp:ListBox></td>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                    <asp:Label ID="Label2" runat="server" Text="Select toolbar elements to display"></asp:Label></td>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                    <asp:ListBox ID="listCRVToolbar" runat="server" SelectionMode="Multiple"></asp:ListBox></td>
            </tr>
            <tr>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                    <asp:Label ID="Label3" runat="server" Text="Select background color"></asp:Label></td>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                    <asp:DropDownList ID="selectBackColor" runat="server">
                    </asp:DropDownList></td>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                </td>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                </td>
            </tr>
            <tr>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                    <asp:Button ID="redisplay" runat="server" OnClick="redisplay_Click" Text="Redisplay Report" /></td>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                </td>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                </td>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                </td>
            </tr>
            <tr>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                    <asp:TextBox ID="pageNumber" runat="server"></asp:TextBox></td>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                    <asp:Button ID="goToPage" runat="server" OnClick="goToPage_Click" Text="Go to Page" /></td>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                    <asp:TextBox ID="zoomFactor" runat="server"></asp:TextBox></td>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                    <asp:Button ID="updateZoomFactor" runat="server" OnClick="updateZoomFactor_Click"
                        Text="% Zoom" /></td>
            </tr>
            <tr>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                    <asp:TextBox ID="searchText" runat="server"></asp:TextBox></td>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                    <asp:Button ID="search" runat="server" OnClick="search_Click" Text="Search For Text" /></td>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                    <asp:Label ID="message" runat="server" ForeColor="Red"></asp:Label></td>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                </td>
            </tr>
            <tr>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                    <asp:Label ID="Label4" runat="server" Text="Border Width"></asp:Label>
                    <asp:TextBox ID="borderWidth" runat="server"></asp:TextBox></td>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                    <asp:Label ID="Label5" runat="server" Text="Border Style"></asp:Label>
                    <asp:DropDownList ID="selectBorderStyle" runat="server">
                    </asp:DropDownList></td>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                    <asp:Label ID="Label6" runat="server" Text="Border Color"></asp:Label>
                    <asp:DropDownList ID="selectBorderColor" runat="server">
                    </asp:DropDownList></td>
                <td nowrap="nowrap" style="width: 100px" valign="top">
                    <asp:Button ID="drawBorder" runat="server" OnClick="drawBorder_Click" Text="Draw Border" /></td>
            </tr>
        </table>
        <br />
        <CR:CrystalReportViewer ID="crystalReportViewer" runat="server" AutoDataBind="true" />
    
    </div>
    </form>
</body>
</html>
