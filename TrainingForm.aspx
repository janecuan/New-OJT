<%@ Page Language="VB" AutoEventWireup="false" CodeFile="TrainingForm.aspx.vb" Inherits="TrainingForm"
    Title="Training Form" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<html>
<head>
    <title>Training Form</title>
</head>
<body>
    <table style="width: 100%">
        <tr>
            <td style="width: 50%; text-align: left;">
                <asp:Image ID="imgHaartz" runat="server" ImageUrl="~/haartz.jpg" ImageAlign="Left"
                    Width="200px" />
            </td>
            <td style="width: 50%; text-align: right;">
                <asp:Label ID="lblFormChange" runat="server"></asp:Label>
            </td>
        </tr>
    </table>
    <asp:Label ID="lblError" runat="server" Text="" Visible="False" ForeColor="Red" Font-Bold="True"
        BackColor="Black"></asp:Label>
    <br />
    <br />
    <center>
        <table>
            <tr>
                <td colspan="2" align="center">
                    <asp:Label ID="lblTrainingFormTitle" runat="server" Font-Bold="True" Font-Size="Large"
                        Style="text-align: center" Text="Employee Training"></asp:Label>
                </td>
            </tr>
            <tr>
                <td align="right" style="width: 50%">
                    <asp:Label ID="lblHeaderEmp1" runat="server" Text="Employee Name:" Font-Bold="True"
                        Font-Size="X-Small"></asp:Label>
                </td>
                <td>
                    <asp:Label ID="lblHeaderEmp2" runat="server" Text="n/a" Font-Size="X-Small"></asp:Label>
                </td>
            </tr>
            <tr>
                <td align="right" style="width: 50%">
                    <asp:Label ID="lblHeaderJob1" runat="server" Text="Job Title:" Font-Bold="True" Font-Size="X-Small"></asp:Label>
                </td>
                <td>
                    <asp:Label ID="lblHeaderJob2" runat="server" Text="n/a" Font-Size="X-Small"></asp:Label>
                </td>
            </tr>
            <tr>
                <td align="right" style="width: 50%">
                    <asp:Label ID="lblHeaderMachine1" runat="server" Text="Machine:" Font-Bold="True"
                        Font-Size="X-Small"></asp:Label>
                </td>
                <td>
                    <asp:Label ID="lblHeaderMachine2" runat="server" Text="n/a" Font-Size="X-Small"></asp:Label>
                </td>
            </tr>
            <tr>
                <td valign="top" align="right" style="width: 50%">
                    <asp:Label ID="lblHeaderCategory1" runat="server" Text="Category:" Font-Bold="True"
                        Font-Size="X-Small"></asp:Label>
                </td>
                <td valign="top">
                    <asp:Label ID="lblHeaderCategory2" runat="server" Text="n/a" Font-Size="X-Small"></asp:Label>
                    <br />
                    <br />
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <form runat="server">
                    <center>
                        <asp:GridView ID="gridTraining" runat="server" AutoGenerateColumns="False" BorderColor="Black"
                            BorderStyle="Double" BackColor="Silver">
                            <Columns>
                                <asp:BoundField DataField="STEP_COMPLETE" HeaderText="Step Completed" ItemStyle-HorizontalAlign="Center">
                                    <HeaderStyle Width="100px" />
                                    <ItemStyle HorizontalAlign="Center"></ItemStyle>
                                </asp:BoundField>
                                <asp:BoundField DataField="MACHINE_DEPENDENT" HeaderText="Machine Dependent" ItemStyle-HorizontalAlign="Center">
                                    <HeaderStyle Width="100px" />
                                    <ItemStyle HorizontalAlign="Center"></ItemStyle>
                                </asp:BoundField>
                                <asp:BoundField DataField="TITLE" HeaderText="Title" />
                                <asp:BoundField DataField="DESCRIPTION" HeaderText="Description" />
                            </Columns>
                            <HeaderStyle BackColor="Gray" />
                            <AlternatingRowStyle BackColor="White" />
                        </asp:GridView>
                    </center>
                    </form>
                </td>
            </tr>
        </table>
    </center>
    <table>
        <tr>
            <td colspan="5">
                <br />
                <asp:Label ID="lblCompletionType" runat="server" Font-Size="X-Small" Text="I have completed the above TYPE training."></asp:Label>
                <br />
                <br />
            </td>
        </tr>
        <tr>
            <td style="width: 50px">
            </td>
            <td>
                <asp:Label ID="lblEmployeeSigned" runat="server" Font-Size="X-Small" Text="Signed:"></asp:Label>
            </td>
            <td>
                <asp:Label ID="lblEmployeeSignedSpace" runat="server" Font-Size="X-Small" Font-Underline="True"></asp:Label>
            </td>
            <td>
                <asp:Label ID="lblEmployeeDate" runat="server" Font-Size="X-Small" Text="Date:"></asp:Label>
            </td>
            <td>
                <asp:Label ID="lblEmployeeDateSpace" runat="server" Font-Size="X-Small" Font-Underline="True"></asp:Label>
            </td>
        </tr>
        <tr>
            <td>
            </td>
            <td>
            </td>
            <td>
                <asp:Label ID="lblEmployeeSignedName" runat="server" Font-Size="X-Small" Text="Trainee"></asp:Label>
            </td>
            <td>
            </td>
            <td>
            </td>
        </tr>
        <tr>
            <td colspan="5">
                <br />
                <asp:Label ID="lblCompletionTrainer" runat="server" Font-Size="X-Small" Text="EMPLOYEE NAME has completed the above TYPE training."></asp:Label>
                <br />
                <br />
            </td>
        </tr>
        <tr>
            <td>
            </td>
            <td>
                <asp:Label ID="lblTrainerSigned" runat="server" Font-Size="X-Small" Text="Signed:"></asp:Label>
            </td>
            <td>
                <asp:Label ID="lblTrainerSignedSpace" runat="server" Font-Size="X-Small" Font-Underline="True"></asp:Label>
            </td>
            <td>
                <asp:Label ID="lblTrainerDate" runat="server" Font-Size="X-Small" Text="Date:"></asp:Label>
            </td>
            <td>
                <asp:Label ID="lblTrainerDateSpace" runat="server" Font-Size="X-Small" Font-Underline="True"></asp:Label>
            </td>
        </tr>
        <tr>
            <td>
            </td>
            <td>
            </td>
            <td>
                <asp:Label ID="lblTrainerSignedName" runat="server" Font-Size="X-Small" Text="Operator/Trainer"></asp:Label>
            </td>
            <td>
            </td>
            <td>
            </td>
        </tr>
        <tr>
            <td colspan="5">
                <br />
                <asp:Label ID="lblCompletionSupervisor" runat="server" Font-Size="X-Small" Text="EMPLOYEE NAME has completed the above TYPE training."></asp:Label>
                <br />
                <br />
            </td>
        </tr>
        <tr>
            <td>
            </td>
            <td>
                <asp:Label ID="lblSupervisorSigned" runat="server" Font-Size="X-Small" Text="Signed:"></asp:Label>
            </td>
            <td>
                <asp:Label ID="lblSupervisorSignedSpace" runat="server" Font-Size="X-Small" Font-Underline="True"></asp:Label>
            </td>
            <td>
                <asp:Label ID="lblSupervisorDate" runat="server" Font-Size="X-Small" Text="Date:"></asp:Label>
            </td>
            <td>
                <asp:Label ID="lblSupervisorDateSpace" runat="server" Font-Size="X-Small" Font-Underline="True"></asp:Label>
            </td>
        </tr>
        <tr>
            <td>
            </td>
            <td>
            </td>
            <td>
                <asp:Label ID="lblSupervisorSignedName" runat="server" Font-Size="X-Small" Text="Supervisor"></asp:Label>
            </td>
            <td>
            </td>
            <td>
            </td>
        </tr>
    </table>
</body>
</html>
