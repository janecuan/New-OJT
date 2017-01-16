<%@ Page Language="VB" MasterPageFile="~/MasterPage.master" AutoEventWireup="false"
    CodeFile="OJT.aspx.vb" Inherits="OJTWebPage" Title="OJT 2.0" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <title>OJT 2.0</title>
    <style type="text/css">
        .PromptCSS
        {
            color: Black;
            font-size: medium;
            font-style: italic;
            font-weight: normal;
            background-color: White;
            font-family: Courier New;
            border: solid 1px Black;
            height: 12px;
        }
        .ModalBackground
        {
            background-color: #006B54;
            filter: alpha(opacity=80);
            opacity: 0.8;
        }
        .GVFixedHeader {
            position: relative;
            top: expression(this.parentNode.parentNode.parentNode.scrollTop-1);
        }
    </style>

    <script language="javascript" type="text/javascript">
        // <![CDATA[
        // ************************************************************************
        //    Name: ValidateEmpTrainDisplayFormRequest
        //
        // Purpose: Helper method to open Training Form in a new window if a
        //          valid job, machine, and category have been selected
        //
        // ************************************************************************

        function ValidateEmpTrainDisplayFormRequest() {
            var noJobTitle = this.theForm.ctl00$Content_RIGHT$ddlEmpTrainJobTitle[0].selected
            var noMachine = this.theForm.ctl00$Content_RIGHT$ddlEmpTrainMachine[0].selected
            var noCategory = this.theForm.ctl00$Content_RIGHT$ddlEmpTrainCategory[0].selected

            if ((noMachine == false) && (noJobTitle == false) && (noCategory == false)) {
                window.open('TrainingForm.aspx', '', '')
            }
        }

        // ************************************************************************
        //    Name: ValidateByMachineDisplayFormRequest
        //
        // Purpose: Helper method to open Training Form in a new window if a
        //          valid job, machine, and category have been selected
        //
        // ************************************************************************

        function ValidateByMachineDisplayFormRequest() {
            var noJobTitle = this.theForm.ctl00$Content_RIGHT$ddlByMachineJobTitle[0].selected
            var noMachine = this.theForm.ctl00$Content_RIGHT$ddlByMachineMachine[0].selected
            var noCategory = this.theForm.ctl00$Content_RIGHT$ddlByMachineCategory[0].selected

            if ((noMachine == false) && (noJobTitle == false) && (noCategory == false)) {
                window.open('TrainingForm.aspx', '', '')
            }
        }
        // ]]>
    </script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="Content_LEFT" runat="Server">
    <asp:Label ID="lblOjtTitle" runat="server" Font-Bold="True" Font-Size="XX-Large"
        Text="OJT 2.0"></asp:Label>
    <asp:TreeView ID="menuSimple" runat="server" Font-Bold="True" ForeColor="Yellow"
        ShowExpandCollapse="False">
        <Nodes>
            <asp:TreeNode Text="Employee Training" Value="Employee Training"></asp:TreeNode>
            <asp:TreeNode SelectAction="None" Text="Reports" Value="Reports">
                <asp:TreeNode Text="Incomplete Training" Value="Incomplete Training"></asp:TreeNode>
                <asp:TreeNode Text="Training Completed" Value="Training Completed" SelectAction="None">
                    <asp:TreeNode Text="By Job" Value="By Job"></asp:TreeNode>
                    <asp:TreeNode Text="By Employee" Value="By Employee"></asp:TreeNode>
                    <asp:TreeNode Text="Waiting for Supervisor" Value="Waiting for Supervisor"></asp:TreeNode>
                </asp:TreeNode>
            </asp:TreeNode>
        </Nodes>
    </asp:TreeView>
    <asp:TreeView ID="menuFull" runat="server" Font-Bold="True" ForeColor="Yellow" ShowExpandCollapse="False">
        <Nodes>
            <asp:TreeNode Text="Employee Training" Value="Employee Training"></asp:TreeNode>
            <asp:TreeNode SelectAction="None" Text="Maintenance" Value="Maintenance">
                <asp:TreeNode Text="Training Codes" Value="Training Codes"></asp:TreeNode>
                <asp:TreeNode Text="Job Titles" Value="Job Titles"></asp:TreeNode>
                <asp:TreeNode Text="Machine Codes" Value="Machine Codes"></asp:TreeNode>
                <asp:TreeNode Text="Categories" Value="Categories"></asp:TreeNode>
                <asp:TreeNode Text="Employees" Value="Employees"></asp:TreeNode>
                <asp:TreeNode Text="Administrators" Value="Administrators"></asp:TreeNode>
            </asp:TreeNode>
            <asp:TreeNode SelectAction="None" Text="Configuration" Value="Configuration">
                <asp:TreeNode Text="Approvals" Value="Approvals"></asp:TreeNode>
                <asp:TreeNode Text="Trainers" Value="Trainers"></asp:TreeNode>
                <asp:TreeNode SelectAction="None" Text="Position" Value="Position">
                    <asp:TreeNode Text="By Machine" Value="By Machine"></asp:TreeNode>
                    <asp:TreeNode Text="By Training Code" Value="By Training Code"></asp:TreeNode>
                </asp:TreeNode>
                
            </asp:TreeNode>
            <asp:TreeNode SelectAction="None" Text="Reports" Value="Reports">
                <asp:TreeNode Text="Incomplete Training" Value="Incomplete Training"></asp:TreeNode>
                <asp:TreeNode Text="Training Completed" Value="Training Completed" SelectAction="None">
                    <asp:TreeNode Text="By Job" Value="By Job"></asp:TreeNode>
                    <asp:TreeNode Text="By Employee" Value="By Employee"></asp:TreeNode>
                    <asp:TreeNode Text="Waiting for Supervisor" Value="Waiting for Supervisor"></asp:TreeNode>
                </asp:TreeNode>
                <asp:TreeNode Text="Trainer Report" Value="Trainer Report"></asp:TreeNode> 
            </asp:TreeNode>
        </Nodes>
    </asp:TreeView>
    <br />
    <asp:Panel ID="pnlLOGIN" runat="server" DefaultButton="Login1$LoginButton">
        <asp:Login ID="Login1" runat="server" BackColor="LightGreen" BorderColor="Black"
            BorderStyle="Solid" BorderWidth="1px" DisplayRememberMe="False" Font-Size="Small"
            Width="200px">
            <TextBoxStyle Width="100px" />
        </asp:Login>
    </asp:Panel>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="Content_RIGHT" runat="Server">
    <asp:UpdatePanel ID="udpRight" runat="server">
        <ContentTemplate>
            <asp:Button ID="btnZero" runat="server" Text="Button" BackColor="Yellow" BorderStyle="None"
                Font-Bold="True" Height="33px" Visible="False" Width="100%" />
            <asp:MultiView ID="MultiView1" runat="server">
                <asp:View ID="viewTrainingCodes" runat="server">
                    <table width="100%">
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpTrainingCodesTop" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td style="width: 100%;" colspan="2">
                                                    <asp:Label ID="ttlTrainingCodeMaintenance" runat="server" Font-Bold="True" Font-Size="X-Large"
                                                        Text="Training Code Maintenance" Style="text-align: center" Width="100%"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="width: 80%;">
                                                    <br />
                                                    <br />
                                                    <asp:ListBox ID="lstTrainingCodes" runat="server" Font-Names="Courier New" Rows="10"
                                                        Width="100%"></asp:ListBox>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:Button ID="btnNewTrainingCodes" runat="server" Text="New" Width="100px" /><br />
                                                    <br />
                                                    <asp:Button ID="btnEditTrainingCodes" runat="server" Text="Edit" Width="100px" /><br />
                                                    <br />
                                                    <ajax:ConfirmButtonExtender ID="cbeDeleteTrainingCodes" runat="server" TargetControlID="btnDeleteTrainingCodes"
                                                        ConfirmText="Are you sure you want to delete this Training Code?&#10;&#10;This will also delete all JOB and EMPLOYEE training&#10;records associated with this training code." />
                                                    <asp:Button ID="btnDeleteTrainingCodes" runat="server" Text="Delete" Width="100px" />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpTrainingCodesBottom" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td align="right" style="width: 50%;">
                                                    <asp:Label ID="lblTrainingCodesTitle" runat="server" Text="Title:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txtTrainingCodeTitle" runat="server" Columns="50" Font-Names="Courier New"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 50%;">
                                                    <asp:Label ID="lblTrainingCodeDependent" runat="server" Text="Machine Dependent:"
                                                        Visible="False"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:CheckBox ID="chbTrainingCodeDependent" runat="server" Text=" " Visible="False" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 50%;" valign="top">
                                                    <asp:Label ID="lblTrainingCodeDescription" runat="server" Text="Description:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txtTrainingCodeDescription" runat="server" Columns="50" Font-Names="Courier New"
                                                        Rows="3" TextMode="MultiLine"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2" style="width: 100%; text-align: center;">
                                                    <asp:Button ID="btnApplyTrainingCodes" runat="server" Text="Add" Width="100px" />&nbsp;
                                                    <asp:Button ID="btnClearTrainingCodes" runat="server" Text="Clear" Width="100px" />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                    </table>
                </asp:View>
                <asp:View ID="viewJobTitles" runat="server">
                    <table width="100%">
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpJobTitlesTop" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td style="width: 100%;" colspan="2">
                                                    <asp:Label ID="ttlJobTitleMaintenance" runat="server" Font-Bold="True" Font-Size="X-Large"
                                                        Text="Job Title Maintenance" Style="text-align: center" Width="100%"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="width: 80%;">
                                                    <br />
                                                    <br />
                                                    <asp:ListBox ID="lstJobTitles" runat="server" Font-Names="Courier New" Rows="10"
                                                        Width="100%"></asp:ListBox>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:Button ID="btnNewJobTitles" runat="server" Text="New" Width="100px" /><br />
                                                    <br />
                                                    <asp:Button ID="btnEditJobTitles" runat="server" Text="Edit" Width="100px" /><br />
                                                    <br />
                                                    <ajax:ConfirmButtonExtender ID="cbeDeleteJobTitles" runat="server" TargetControlID="btnDeleteJobTitles"
                                                        ConfirmText="Are you sure you want to delete this Job Title?&#10;&#10;This will also delete all JOB and EMPLOYEE training&#10;records associated with this job title." />
                                                    <asp:Button ID="btnDeleteJobTitles" runat="server" Text="Delete" Width="100px" />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpJobTitlesBottom" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td align="right" style="width: 50%;">
                                                    <asp:Label ID="lblJobTitleTitle" runat="server" Text="Title:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txtJobTitleTitle" runat="server" Columns="50" Font-Names="Courier New"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 50%;" valign="top">
                                                    <asp:Label ID="lblJobTitleDescription" runat="server" Text="Description:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txtJobTitleDescription" runat="server" Columns="50" Font-Names="Courier New"
                                                        Rows="3" TextMode="MultiLine"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 50%;">
                                                    <asp:Label ID="lblJobTitleDocument" runat="server" Text="Document:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txtJobTitleDocument" runat="server" Columns="50" Font-Names="Courier New"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2" style="width: 100%; text-align: center;">
                                                    <asp:Button ID="btnApplyJobTitles" runat="server" Text="Add" Width="100px" />&nbsp;
                                                    <asp:Button ID="btnClearJobTitles" runat="server" Text="Clear" Width="100px" />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                    </table>
                </asp:View>
                <asp:View ID="viewMachineCodes" runat="server">
                    <table width="100%">
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpMachineCodesTop" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td style="width: 100%;" colspan="2">
                                                    <asp:Label ID="ttlMachineCodeMaintenance" runat="server" Font-Bold="True" Font-Size="X-Large"
                                                        Text="Machine Code Maintenance" Style="text-align: center" Width="100%"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="width: 80%;">
                                                    <br />
                                                    <br />
                                                    <asp:ListBox ID="lstMachineCodes" runat="server" Font-Names="Courier New" Rows="10"
                                                        Width="100%"></asp:ListBox>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:Button ID="btnNewMachineCodes" runat="server" Text="New" Width="100px" /><br />
                                                    <br />
                                                    <asp:Button ID="btnEditMachineCodes" runat="server" Text="Edit" Width="100px" /><br />
                                                    <br />
                                                    <ajax:ConfirmButtonExtender ID="cbeDeleteMachineCodes" runat="server" TargetControlID="btnDeleteMachineCodes"
                                                        ConfirmText="Are you sure you want to delete this Machine Code?&#10;&#10;This will also delete all JOB and EMPLOYEE training&#10;records associated with this machine code." />
                                                    <asp:Button ID="btnDeleteMachineCodes" runat="server" Text="Delete" Width="100px" />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpMachineCodesBottom" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td align="right" style="width: 50%;">
                                                    <asp:Label ID="lblMachineCodesCode" runat="server" Text="Code:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txtMachineCodeCode" runat="server" Columns="50" Font-Names="Courier New"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 50%;" valign="top">
                                                    <asp:Label ID="lblMachineCodeDescription" runat="server" Text="Description:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txtMachineCodeDescription" runat="server" Columns="50" Font-Names="Courier New"
                                                        Rows="3" TextMode="MultiLine"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2" style="width: 100%; text-align: center;">
                                                    <asp:Button ID="btnApplyMachineCodes" runat="server" Text="Add" Width="100px" />&nbsp;
                                                    <asp:Button ID="btnClearMachineCodes" runat="server" Text="Clear" Width="100px" />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                    </table>
                    </table>
                </asp:View>
                <asp:View ID="viewCategories" runat="server">
                    <table width="100%">
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpCategoriesTop" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td style="width: 100%;" colspan="2">
                                                    <asp:Label ID="ttlCategoriesMaintenance" runat="server" Font-Bold="True" Font-Size="X-Large"
                                                        Text="Category Maintenance" Style="text-align: center" Width="100%"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="width: 80%;">
                                                    <br />
                                                    <br />
                                                    <asp:ListBox ID="lstCategories" runat="server" Font-Names="Courier New" Rows="10"
                                                        Width="100%"></asp:ListBox>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:Button ID="btnNewCategories" runat="server" Text="New" Width="100px" /><br />
                                                    <br />
                                                    <asp:Button ID="btnEditCategories" runat="server" Text="Edit" Width="100px" /><br />
                                                    <br />
                                                    <ajax:ConfirmButtonExtender ID="cbeDeleteCategories" runat="server" TargetControlID="btnDeleteCategories"
                                                        ConfirmText="Are you sure you want to delete this Category?&#10;&#10;This will also delete all JOB and EMPLOYEE training&#10;records associated with this category." />
                                                    <asp:Button ID="btnDeleteCategories" runat="server" Text="Delete" Width="100px" />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpCategoriesBottom" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td align="right" style="width: 50%;">
                                                    <asp:Label ID="lblCategoriesTitle" runat="server" Text="Title:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txtCategoriesTitle" runat="server" Columns="50" Font-Names="Courier New"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 50%;">
                                                    <asp:Label ID="lblCategoriesType" runat="server" Text="Category Type:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:DropDownList ID="ddlCategoriesType" runat="server" Font-Names="Courier New">
                                                        <asp:ListItem Value="O">Operational</asp:ListItem>
                                                        <asp:ListItem Value="S">Safety</asp:ListItem>
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 50%;" valign="top">
                                                    <asp:Label ID="lblCategoriesDescription" runat="server" Text="Description:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txtCategoriesDescription" runat="server" Columns="50" Font-Names="Courier New"
                                                        Rows="3" TextMode="MultiLine"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2" style="width: 100%; text-align: center;">
                                                    <asp:Button ID="btnApplyCategories" runat="server" Text="Add" Width="100px" />&nbsp;
                                                    <asp:Button ID="btnClearCategories" runat="server" Text="Clear" Width="100px" />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                    </table>
                </asp:View>
                <asp:View ID="viewApprovals" runat="server">
                    <table width="100%">
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpApprovalsTop" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td colspan="2" style="width: 100%;">
                                                    <asp:Label ID="ttlApprovalsMaintenance" runat="server" Text="Approval Configuration"
                                                        Font-Bold="True" Font-Size="X-Large" Style="text-align: center" Width="100%"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 25%;">
                                                    <br />
                                                    <asp:Label ID="lblApprovalJobTitle" runat="server" Text="Job Title:"></asp:Label>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:DropDownList ID="ddlApprovalJobTitle" runat="server" Font-Names="Courier New"
                                                        AutoPostBack="True">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="top" style="width: 25%;">
                                                    <br />
                                                    <asp:Label ID="lblApprovalMachine" runat="server" Text="Machine/Location:"></asp:Label>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:DropDownList ID="ddlApprovalMachine" runat="server" Font-Names="Courier New"
                                                        AutoPostBack="True">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="top" style="width: 25%;">
                                                    <br />
                                                    <asp:Label ID="lblApprovalCategory" runat="server" Text="Category:"></asp:Label>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:DropDownList ID="ddlApprovalCategory" runat="server" Font-Names="Courier New"
                                                        AutoPostBack="True">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2" style="width: 100%; text-align: center;">
                                                    <asp:Button ID="btnApprovalLookup" runat="server" Text="Lookup" Width="100px" />&nbsp;
                                                    <asp:Button ID="btnApprovalClear" runat="server" Text="Clear" Width="100px" />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpApprovalsBottom" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td colspan="4" style="width: 100%; text-align: center;">
                                                    <br />
                                                    <center>
                                                        <asp:GridView ID="gridApproval" runat="server" AutoGenerateColumns="False" BackColor="Silver">
                                                            <Columns>
                                                                <asp:BoundField DataField="MACHINE_DEPENDENT" HeaderText="Machine Dependent" ItemStyle-HorizontalAlign="Center">
                                                                    <HeaderStyle Width="100px" />
                                                                    <ItemStyle HorizontalAlign="Center" />
                                                                </asp:BoundField>
                                                                <asp:BoundField DataField="TITLE" HeaderText="Title" ItemStyle-HorizontalAlign="Left">
                                                                    <ItemStyle HorizontalAlign="Left" />
                                                                </asp:BoundField>
                                                                <asp:BoundField DataField="DESCRIPTION" HeaderText="Description" ItemStyle-HorizontalAlign="Left">
                                                                    <ItemStyle HorizontalAlign="Left" />
                                                                </asp:BoundField>
                                                            </Columns>
                                                            <HeaderStyle BackColor="Gray" />
                                                            <AlternatingRowStyle BackColor="White" />
                                                        </asp:GridView>
                                                    </center>
                                                    <br />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 20%;">
                                                    <asp:Label ID="lblApprovalReviewedBy" runat="server" Text="Reviewed By:"></asp:Label>
                                                </td>
                                                <td style="width: 30%;">
                                                    <asp:TextBox ID="txtApprovalReviewedBy" runat="server" Columns="20"></asp:TextBox>
                                                    <asp:TextBox ID="txtApprovalReviewDate" runat="server" Columns="10" ReadOnly="True"
                                                        BackColor="LightGreen" BorderStyle="None" TabIndex="-1"></asp:TextBox>
                                                </td>
                                                <td align="right" style="width: 20%;">
                                                    <asp:Label ID="lblApprovalQcfNumber" runat="server" Text="QCF Number:"></asp:Label>
                                                </td>
                                                <td style="width: 20%;">
                                                    <asp:TextBox ID="txtApprovalQcfNumber" runat="server" Columns="20"></asp:TextBox>
                                                </td>
                                                <tr>
                                                    <td colspan="4" style="width: 100%; text-align: center;">
                                                        <asp:Button ID="btnApprovalSubmit" runat="server" Text="Submit" Width="100px" />
                                                    </td>
                                                </tr>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                    </table>
                </asp:View>
                <asp:View ID="viewByMachine" runat="server">
                    <table width="100%">
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpByMachineTop" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td colspan="2" style="width: 100%;">
                                                    <asp:Label ID="ttlByMachineMaintenance" runat="server" Text="Position Configuration - By Machine"
                                                        Font-Bold="True" Font-Size="X-Large" Style="text-align: center" Width="100%"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="top" style="width: 25%;">
                                                    <br />
                                                    <asp:Label ID="lblByMachineJobTitle" runat="server" Text="Job Title:"></asp:Label>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:DropDownList ID="ddlByMachineJobTitle" runat="server" Font-Names="Courier New"
                                                        AutoPostBack="True">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 25%;">
                                                    <br />
                                                    <asp:Label ID="lblByMachineMachine" runat="server" Text="Machine:"></asp:Label>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:DropDownList ID="ddlByMachineMachine" runat="server" Font-Names="Courier New"
                                                        AutoPostBack="True">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="top" style="width: 25%;">
                                                    <br />
                                                    <asp:Label ID="lblByMachineCategory" runat="server" Text="Category:"></asp:Label>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:DropDownList ID="ddlByMachineCategory" runat="server" Font-Names="Courier New"
                                                        AutoPostBack="True">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2" style="width: 100%; text-align: center;">
                                                    <asp:Button ID="btnByMachineLookup" runat="server" Text="Lookup" Width="100px" />&nbsp;
                                                    <asp:Button ID="btnByMachineDisplayForm" runat="server" Text="Display Form" Width="100px"
                                                        OnClientClick="return ValidateByMachineDisplayFormRequest()" />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpByMachineBottom" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td align="center" style="width: 40%;">
                                                    <b>Available Codes</b>
                                                </td>
                                                <td align="center" style="width: 20%;">
                                                </td>
                                                <td align="center" style="width: 40%;">
                                                    <b>Selected Codes</b>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="center" style="width: 40%;">
                                                    <asp:ListBox ID="lstByMachineAvailableCodes" runat="server" Rows="10" SelectionMode="Multiple">
                                                    </asp:ListBox>
                                                </td>
                                                <td align="center" style="width: 20%;">
                                                    <asp:Button ID="btnByMachineAddSelected" runat="server" Text="Add Selected  &gt;"
                                                        Width="150px" /><br />
                                                    <br />
                                                    <asp:Button ID="btnByMachineAddAll" runat="server" Text="Add All  &gt;&gt;" Width="150px" /><br />
                                                    <ajax:ConfirmButtonExtender ID="cbeByMachineRemoveAll" runat="server" TargetControlID="btnByMachineRemoveAll"
                                                        ConfirmText="Are you sure you want to remove all Training Codes?&#10;&#10;This will also delete all EMPLOYEE training&#10;records associated with these training codes." />
                                                    <asp:Button ID="btnByMachineRemoveAll" runat="server" Text="&lt;&lt;  Remove All"
                                                        Width="150px" /><br />
                                                    <br />
                                                    <ajax:ConfirmButtonExtender ID="cbeByMachineRemoveSelected" runat="server" TargetControlID="btnByMachineRemoveSelected"
                                                        ConfirmText="Are you sure you want to remove this Training Code?&#10;&#10;This will also delete all EMPLOYEE training&#10;records associated with this training code." />
                                                    <asp:Button ID="btnByMachineRemoveSelected" runat="server" Text="&lt;  Remove Selected"
                                                        Width="150px" />
                                                </td>
                                                <td align="center" style="width: 40%;">
                                                    <asp:ListBox ID="lstByMachineSelectedCodes" runat="server" Rows="10" SelectionMode="Multiple">
                                                    </asp:ListBox>
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                    </table>
                </asp:View>
                <asp:View ID="viewByTrainingCode" runat="server">
                    <table width="100%">
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpByTrainingCodeTop" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td colspan="2" style="width: 100%;">
                                                    <asp:Label ID="ttlByTrainingCodeMaintenance" runat="server" Text="Position Configuration - By Training Code"
                                                        Font-Bold="True" Font-Size="X-Large" Style="text-align: center" Width="100%"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="top" style="width: 25%;">
                                                    <br />
                                                    <asp:Label ID="lblByTrainingCodeTrainingCode" runat="server" Text="Training Code:"></asp:Label>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:DropDownList ID="ddlByTrainingCodeTrainingCode" runat="server" Font-Names="Courier New"
                                                        AutoPostBack="True">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 25%;">
                                                    <br />
                                                    <asp:Label ID="lblByTrainingCodeMachine" runat="server" Text="Machine:"></asp:Label>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:DropDownList ID="ddlByTrainingCodeMachine" runat="server" Font-Names="Courier New"
                                                        AutoPostBack="True">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="top" style="width: 25%;">
                                                    <br />
                                                    <asp:Label ID="lblByTrainingCodeCategory" runat="server" Text="Category:"></asp:Label>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:DropDownList ID="ddlByTrainingCodeCategory" runat="server" Font-Names="Courier New"
                                                        AutoPostBack="True">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2" style="width: 100%; text-align: center;">
                                                    <asp:Button ID="btnByTrainingCodeLookup" runat="server" Text="Lookup" Width="100px" />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpByTrainingCodeBottom" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td align="center" style="width: 40%;">
                                                    <b>Available Job Titles</b>
                                                </td>
                                                <td align="center" style="width: 20%;">
                                                </td>
                                                <td align="center" style="width: 40%;">
                                                    <b>Selected Job Titles</b>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="center" style="width: 40%;">
                                                    <asp:ListBox ID="lstByTrainingCodeAvailableJobs" runat="server" Rows="10" SelectionMode="Multiple">
                                                    </asp:ListBox>
                                                </td>
                                                <td align="center" style="width: 20%;">
                                                    <asp:Button ID="btnByTrainingCodeAddSelected" runat="server" Text="Add Selected  &gt;"
                                                        Width="150px" /><br />
                                                    <br />
                                                    <asp:Button ID="btnByTrainingCodeAddAll" runat="server" Text="Add All  &gt;&gt;"
                                                        Width="150px" /><br />
                                                    <ajax:ConfirmButtonExtender ID="cbeByTrainingCodeRemoveAll" runat="server" TargetControlID="btnByTrainingCodeRemoveAll"
                                                        ConfirmText="Are you sure you want to remove this Training Code for all job titles?&#10;&#10;This will also delete all EMPLOYEE training&#10;records associated with this training code." />
                                                    <asp:Button ID="btnByTrainingCodeRemoveAll" runat="server" Text="&lt;&lt;  Remove All"
                                                        Width="150px" /><br />
                                                    <br />
                                                    <ajax:ConfirmButtonExtender ID="cbeByTrainingCodeRemoveSelected" runat="server" TargetControlID="btnByTrainingCodeRemoveSelected"
                                                        ConfirmText="Are you sure you want to remove this Training Code for this job titles?&#10;&#10;This will also delete all EMPLOYEE training&#10;records associated with this training code." />
                                                    <asp:Button ID="btnByTrainingCodeRemoveSelected" runat="server" Text="&lt;  Remove Selected"
                                                        Width="150px" />
                                                </td>
                                                <td align="center" style="width: 40%;">
                                                    <asp:ListBox ID="lstByTrainingCodeSelectedJobs" runat="server" Rows="10" SelectionMode="Multiple">
                                                    </asp:ListBox>
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                    </table>
                </asp:View>
                <asp:View ID="viewTrainer" runat="server">
                    <table width="100%">
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpTrainerTop" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td colspan="2" style="width: 100%;">
                                                    <asp:Label ID="ttlTrainerMaintenance" runat="server" Text="Trainer Configuration"
                                                        Font-Bold="True" Font-Size="X-Large" Style="text-align: center" Width="100%"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 25%;">
                                                    <br />
                                                    <asp:Label ID="lblTrainerEmployee" runat="server" Text="Employee:"></asp:Label>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:DropDownList ID="ddlTrainerEmployee" runat="server" Font-Names="Courier New"
                                                        AutoPostBack="True">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="top" style="width: 25%;">
                                                    <br />
                                                    <asp:Label ID="lblTrainerJobTitle" runat="server" Text="Job Title:"></asp:Label>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:DropDownList ID="ddlTrainerJobTitle" runat="server" Font-Names="Courier New"
                                                        AutoPostBack="True">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="top" style="width: 25%;">
                                                    <br />
                                                    <asp:Label ID="lblTrainerCategory" runat="server" Text="Category:"></asp:Label>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:DropDownList ID="ddlTrainerCategory" runat="server" Font-Names="Courier New"
                                                        AutoPostBack="True">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2" style="width: 100%; text-align: center;">
                                                    <asp:Button ID="btnTrainerLookup" runat="server" Text="Lookup" Width="100px" />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpTrainerBottom" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td align="center" style="width: 40%;">
                                                    <b>Available<br />
                                                        Machines/Locations</b>
                                                </td>
                                                <td align="center" style="width: 20%;">
                                                </td>
                                                <td align="center" style="width: 40%;">
                                                    <b>Selected<br />
                                                        Machines/Locations</b>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="center" style="width: 40%;">
                                                    <asp:ListBox ID="lstTrainerAvailableMachines" runat="server" Rows="10" SelectionMode="Multiple">
                                                    </asp:ListBox>
                                                </td>
                                                <td align="center" style="width: 20%;">
                                                    <asp:Button ID="btnTrainerAddSelected" runat="server" Text="Add Selected  &gt;" Width="150px" /><br />
                                                    <br />
                                                    <asp:Button ID="btnTrainerAddAll" runat="server" Text="Add All  &gt;&gt;" Width="150px" /><br />
                                                    <asp:Button ID="btnTrainerRemoveAll" runat="server" Text="&lt;&lt;  Remove All" Width="150px" /><br />
                                                    <br />
                                                    <asp:Button ID="btnTrainerRemoveSelected" runat="server" Text="&lt;  Remove Selected"
                                                        Width="150px" />
                                                </td>
                                                <td align="center" style="width: 40%;">
                                                    <asp:ListBox ID="lstTrainerSelectedMachines" runat="server" Rows="10" SelectionMode="Multiple">
                                                    </asp:ListBox>
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                    </table>
                </asp:View>
                <asp:View ID="viewEmployees" runat="server">
                    <table width="100%">
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpEmployeesTop" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td style="width: 100%;">
                                                    <asp:Label ID="ttlEmployeeMaintenance" runat="server" Font-Bold="True" Font-Size="X-Large"
                                                        Text="Employee Maintenance" Style="text-align: center" Width="100%"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <center>
                                                        <br />
                                                        <div style="height: 200px; overflow-y: scroll; overflow-x: hidden; width: 75%;">
                                                            <asp:GridView ID="grdEmployees" runat="server" AutoGenerateColumns="False"
                                                                BackColor="Silver" Width="95%" HeaderStyle-CssClass="GVFixedHeader">
                                                                <Columns>
                                                                    <asp:TemplateField>
                                                                        <ItemTemplate>
                                                                            <asp:LinkButton ID="editButton" runat="server" CommandName="Edit" Text="Edit" />
                                                                        </ItemTemplate>
                                                                        <ControlStyle Width="50px" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField>
                                                                        <ItemTemplate>
                                                                            <asp:LinkButton ID="deleteButton" runat="server" CommandName="Delete" Text="Delete"
                                                                                OnClientClick="return confirm('Are you sure you want to delete this employee?');" />
                                                                        </ItemTemplate>
                                                                        <ControlStyle Width="50px" />
                                                                    </asp:TemplateField>
                                                                    <asp:BoundField DataField="LAST_NAME" HeaderText="Last Name" ItemStyle-HorizontalAlign="Left">
                                                                        <ItemStyle HorizontalAlign="Left" />
                                                                    </asp:BoundField>
                                                                    <asp:BoundField DataField="FIRST_NAME" HeaderText="First Name" ItemStyle-HorizontalAlign="Left">
                                                                        <ItemStyle HorizontalAlign="Left" />
                                                                    </asp:BoundField>
                                                                    <asp:BoundField DataField="EMPLOYEE_NUMBER" HeaderText="Employee Number" ItemStyle-HorizontalAlign="Left">
                                                                        <ItemStyle HorizontalAlign="Left" />
                                                                    </asp:BoundField>
                                                                    <asp:BoundField DataField="ACTIVE_STATUS" HeaderText="Active" ItemStyle-HorizontalAlign="Left">
                                                                        <ItemStyle HorizontalAlign="Left" />
                                                                    </asp:BoundField>
                                                                </Columns>
                                                                <HeaderStyle BackColor="Gray" />
                                                                <AlternatingRowStyle BackColor="White" />
                                                            </asp:GridView>
                                                        </div>
                                                        <br />
                                                        <asp:Button ID="btnNewEmployees" runat="server" Text="New" Width="100px" /><br />
                                                    </center>
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpEmployeesBottom" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td align="right" style="width: 50%;">
                                                    <asp:Label ID="lblEmployeesNumber" runat="server" Text="Employee Number:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txtEmployeeNumber" runat="server" Columns="10" Font-Names="Courier New"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 50%;">
                                                    <asp:Label ID="lblEmployeeFirst" runat="server" Text="First Name:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txtEmployeeFirst" runat="server" Columns="20" Font-Names="Courier New"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 50%;">
                                                    <asp:Label ID="lblEmployeeLast" runat="server" Text="Last Name:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txtEmployeeLast" runat="server" Columns="20" Font-Names="Courier New"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 50%;">
                                                    <asp:Label ID="lblEmployeeActive" runat="server" Text="Active:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:CheckBox ID="chbEmployeeActive" runat="server" Text=" " />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 50%;" valign="top">
                                                    <asp:Label ID="lblEmployeeComments" runat="server" Text="Comments:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txtEmployeeComments" runat="server" Columns="50" Font-Names="Courier New"
                                                        Rows="3" TextMode="MultiLine"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 50%;">
                                                    <asp:Label ID="lblEmployeeTrainer" runat="server" Text="Is a Trainer?:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:CheckBox ID="chbEmployeeTrainer" runat="server" Text=" " />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 50%;" valign="top">
                                                    <asp:Label ID="lblEmployeePassword" runat="server" Text="Trainer Password:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txtEmployeePassword" runat="server" Columns="20" Font-Names="Courier New"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 50%;">
                                                    <asp:Label ID="lblEmployeeIsSupervisor" runat="server" Text="Is a Supervisor?:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:CheckBox ID="chbEmployeeIsSupervisor" runat="server" Text=" " />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 50%;" valign="top">
                                                    <asp:Label ID="lblSupervisorPassword" runat="server" Text="Supervisor Password:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txtSupervisorPassword" runat="server" Columns="20" Font-Names="Courier New"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 50%;" valign="top">
                                                    <asp:Label ID="lblEmployeeEmail" runat="server" Text="E-Mail Address:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txtEmployeeEmail" runat="server" Columns="20" Font-Names="Courier New"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 50%;" valign="top">
                                                    <asp:Label ID="lblEmployeeSupervisor" runat="server" Text="Supervisor:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:DropDownList ID="ddlEmployeeSupervisor" runat="server" Font-Names="Courier New">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2" style="width: 100%; text-align: center;">
                                                    <asp:Button ID="btnApplyEmployees" runat="server" Text="Add" Width="100px" />&nbsp;
                                                    <asp:Button ID="btnClearEmployees" runat="server" Text="Clear" Width="100px" />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                    </table>
                </asp:View>
                <asp:View ID="viewEmployeeTraining" runat="server">
                    <table width="100%">
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpEmployeeTrainingTop" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td colspan="2" style="width: 100%;">
                                                    <asp:Label ID="ttlEmpTrain" runat="server" Text="Employee Training" Font-Bold="True"
                                                        Font-Size="X-Large" Style="text-align: center" Width="100%"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 25%;">
                                                    <br />
                                                    <asp:Label ID="lblEmpTrainEmployee" runat="server" Text="Employee:"></asp:Label>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:DropDownList ID="ddlEmpTrainEmployee" runat="server" Font-Names="Courier New"
                                                        AutoPostBack="True">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="top" style="width: 25%;">
                                                    <br />
                                                    <asp:Label ID="lblEmpTrainJobTitle" runat="server" Text="Job Title:"></asp:Label>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:DropDownList ID="ddlEmpTrainJobTitle" runat="server" Font-Names="Courier New"
                                                        AutoPostBack="True">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="top" style="width: 25%;">
                                                    <br />
                                                    <asp:Label ID="lblEmpTrainMachine" runat="server" Text="Machine:"></asp:Label>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:DropDownList ID="ddlEmpTrainMachine" runat="server" Font-Names="Courier New"
                                                        AutoPostBack="True">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="top" style="width: 25%;">
                                                    <br />
                                                    <asp:Label ID="lblEmpTrainCategory" runat="server" Text="Category:"></asp:Label>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:DropDownList ID="ddlEmpTrainCategory" runat="server" Font-Names="Courier New"
                                                        AutoPostBack="True">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2" style="width: 100%; text-align: center;">
                                                    <asp:Button ID="btnEmpTrainLookup" runat="server" Text="Lookup" Width="100px" />&nbsp;
                                                    <asp:Button ID="btnEmpTrainDisplayForm" runat="server" Text="Display Form" Width="100px"
                                                        OnClientClick="return ValidateEmpTrainDisplayFormRequest()" />&nbsp;
                                                    <asp:Button ID="btnEmpTrainClear" runat="server" Text="Clear" Width="100px" />&nbsp;
                                                    <asp:Button ID="btnEmpTrainBack" runat="server" Text="Back" Width="100px" />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpEmployeeTrainingBottom" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td colspan="4" style="width: 100%; text-align: center;">
                                                    <br />
                                                    <asp:Label ID="lblEmpTrainDocument" runat="server" Text="Job Description:"></asp:Label>
                                                    <asp:HyperLink ID="lnkEmpTrainDocument" runat="server"></asp:HyperLink>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="4" style="width: 100%; text-align: center;">
                                                    <br />
                                                    <center>
                                                        <asp:GridView ID="gridEmployeeTraining" runat="server" AutoGenerateColumns="False"
                                                            BackColor="Silver">
                                                            <Columns>
                                                                <asp:TemplateField HeaderText="Step Completed" ItemStyle-HorizontalAlign="Center">
                                                                    <ItemTemplate>
                                                                        <asp:CheckBox ID="chbStepCompleted" runat="server" /></ItemTemplate>
                                                                    <HeaderStyle Width="100px" />
                                                                    <ItemStyle HorizontalAlign="Center" />
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="Training Number" Visible="False" ItemStyle-HorizontalAlign="Center">
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="lblTrainingNumber" runat="server" Text="Label"></asp:Label></ItemTemplate>
                                                                    <HeaderStyle Width="100px" />
                                                                    <ItemStyle HorizontalAlign="Center" />
                                                                </asp:TemplateField>
                                                                <asp:BoundField DataField="DESCRIPTION" HeaderText="Description" ItemStyle-HorizontalAlign="Left">
                                                                    <ItemStyle HorizontalAlign="Left" />
                                                                </asp:BoundField>
                                                                <asp:BoundField DataField="DATE_APPROVED" HeaderText="Date Completed" ItemStyle-HorizontalAlign="Left">
                                                                    <ItemStyle HorizontalAlign="Left" />
                                                                </asp:BoundField>
                                                            </Columns>
                                                            <HeaderStyle BackColor="Gray" />
                                                            <AlternatingRowStyle BackColor="White" />
                                                        </asp:GridView>
                                                        <asp:Button ID="btnEmpTrainSelectAll" runat="server" Text="Select All" />
                                                    </center>
                                                    <br />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 20%;">
                                                    <asp:Label ID="lblEmpTrainTrainer" runat="server" Text="Trainer:"></asp:Label>
                                                </td>
                                                <td style="width: 30%;">
                                                    <asp:DropDownList ID="ddlEmpTrainTrainer" runat="server">
                                                    </asp:DropDownList>
                                                    <asp:TextBox ID="txtEmpTrainTrainerDate" runat="server" Columns="10" ReadOnly="True"
                                                        BackColor="LightGreen" BorderStyle="None" TabIndex="-1"></asp:TextBox>
                                                </td>
                                                <td align="right" style="width: 20%;">
                                                    <asp:Label ID="lblEmpTrainTrainerPassword" runat="server" Text="Password:"></asp:Label>
                                                </td>
                                                <td style="width: 30%;">
                                                    <asp:TextBox ID="txtEmpTrainTrainerPassword" runat="server" Columns="25" TextMode="Password"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="4" style="width: 100%; text-align: center;">
                                                    <asp:Button ID="btnEmpTrainTrainerSignoff" runat="server" Text="Trainer Signoff"
                                                        Width="150px" /><br />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 20%;">
                                                    <asp:Label ID="lblEmpTrainSupervisor" runat="server" Text="Supervisor:"></asp:Label>
                                                </td>
                                                <td style="width: 30%;">
                                                    <asp:DropDownList ID="ddlEmpTrainSupervisor" runat="server">
                                                    </asp:DropDownList>
                                                    <asp:TextBox ID="txtEmpTrainSupervisorDate" runat="server" Columns="10" ReadOnly="True"
                                                        BackColor="LightGreen" BorderStyle="None" TabIndex="-1"></asp:TextBox>
                                                </td>
                                                <td align="right" style="width: 20%;">
                                                    <asp:Label ID="lblEmpTrainSupervisorPassword" runat="server" Text="Password:"></asp:Label>
                                                </td>
                                                <td style="width: 30%;">
                                                    <asp:TextBox ID="txtEmpTrainSupervisorPassword" runat="server" Columns="25" TextMode="Password"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="4" style="width: 100%; text-align: center;">
                                                    <asp:Button ID="btnEmpTrainSupervisorSignoff" runat="server" Text="Supervisor Signoff"
                                                        Width="150px" />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                    </table>
                </asp:View>
                <asp:View ID="viewAdministrators" runat="server">
                    <table width="100%">
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpAdministratorsTop" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td style="width: 100%;" colspan="2">
                                                    <asp:Label ID="ttlAdministratorMaintenance" runat="server" Font-Bold="True" Font-Size="X-Large"
                                                        Text="Administrator Maintenance" Style="text-align: center" Width="100%"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="width: 80%;">
                                                    <br />
                                                    <br />
                                                    <asp:ListBox ID="lstAdministrators" runat="server" Font-Names="Courier New" Rows="10"
                                                        Width="100%"></asp:ListBox>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:Button ID="btnNewAdministrators" runat="server" Text="New" Width="100px" /><br />
                                                    <br />
                                                    <asp:Button ID="btnEditAdministrators" runat="server" Text="Edit" Width="100px" /><br />
                                                    <br />
                                                    <ajax:ConfirmButtonExtender ID="cbeDeleteAdministrators" runat="server" TargetControlID="btnDeleteAdministrators"
                                                        ConfirmText="Are you sure you want to delete this Administrator?" />
                                                    <asp:Button ID="btnDeleteAdministrators" runat="server" Text="Delete" Width="100px" />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpAdministratorsBottom" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td align="right" style="width: 50%;">
                                                    <asp:Label ID="lblAdministratorLogin" runat="server" Text="Login:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txtAdministratorLogin" runat="server" Columns="20" Font-Names="Courier New"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 50%;">
                                                    <asp:Label ID="lblAdministratorPassword" runat="server" Text="Password:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txtAdministratorPassword" runat="server" Columns="20" Font-Names="Courier New"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 50%;">
                                                    <asp:Label ID="lblAdministratorFirst" runat="server" Text="First Name:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txtAdministratorFirst" runat="server" Columns="20" Font-Names="Courier New"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" style="width: 50%;">
                                                    <asp:Label ID="lblAdministratorLast" runat="server" Text="Last Name:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:TextBox ID="txtAdministratorLast" runat="server" Columns="20" Font-Names="Courier New"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2" style="width: 100%; text-align: center;">
                                                    <asp:Button ID="btnApplyAdministrators" runat="server" Text="Add" Width="100px" />&nbsp;
                                                    <asp:Button ID="btnClearAdministrators" runat="server" Text="Clear" Width="100px" />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                    </table>
                </asp:View>
                <asp:View ID="viewReportCompleteByJob" runat="server">
                    <table width="100%">
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpReportCompleteByJobTop" runat="server">
                                    <Triggers>
                                        <asp:PostBackTrigger ControlID="btnReportCompleteByJobPrint" />
                                    </Triggers>
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td colspan="2" style="width: 100%;">
                                                    <asp:Label ID="ttlReportCompleteByJob" runat="server" Text="Report - Training Completed By Job"
                                                        Font-Bold="True" Font-Size="X-Large" Style="text-align: center" Width="100%"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="top" style="width: 40%;">
                                                    <br />
                                                    <asp:Label ID="lblReportCompleteJobTitle" runat="server" Text="Job Title:"></asp:Label>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:DropDownList ID="ddlReportCompleteJobTitle" runat="server" Font-Names="Courier New"
                                                        AutoPostBack="True">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="top" style="width: 40%;">
                                                    <br />
                                                    <asp:Label ID="lblReportCompleteMachine" runat="server" Text="Machine:"></asp:Label>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:DropDownList ID="ddlReportCompleteMachine" runat="server" Font-Names="Courier New"
                                                        AutoPostBack="True">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="top" style="width: 40%;">
                                                    <br />
                                                    <asp:Label ID="lblReportCompleteInactive" runat="server" Text="Include Inactive Employees?:"></asp:Label>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:CheckBox ID="chbReportCompleteByJobInactive" runat="server" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2" style="width: 100%; text-align: center;">
                                                    <asp:Button ID="btnReportCompleteByJobGenerate" runat="server" Text="Generate" Width="100px" />&nbsp;
                                                    <asp:Button ID="btnReportCompleteByJobClear" runat="server" Text="Clear" Width="100px" />&nbsp;
                                                    <asp:Button ID="btnReportCompleteByJobPrint" runat="server" Text="Print" Width="100px" />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpReportCompleteByJobBottom" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td style="width: 100%; text-align: center;">
                                                    <br />
                                                    <center>
                                                        <asp:GridView ID="gridReportCompleteByJob" runat="server" AutoGenerateColumns="False"
                                                            BackColor="Silver">
                                                            <Columns>
                                                                <asp:HyperLinkField DataTextField="Employee" HeaderText="Employee" ItemStyle-HorizontalAlign="Left"
                                                                    DataNavigateUrlFields="Address" />
                                                                <asp:BoundField DataField="Category" HeaderText="Category" ItemStyle-HorizontalAlign="Left">
                                                                    <ItemStyle HorizontalAlign="Left" />
                                                                </asp:BoundField>
                                                                <asp:BoundField DataField="Status" HeaderText="Status" ItemStyle-HorizontalAlign="Left">
                                                                    <ItemStyle HorizontalAlign="Left" />
                                                                </asp:BoundField>
                                                            </Columns>
                                                            <HeaderStyle BackColor="Gray" />
                                                            <AlternatingRowStyle BackColor="White" />
                                                        </asp:GridView>
                                                    </center>
                                                    <br />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                    </table>
                </asp:View>
                <asp:View ID="viewReportCompleteByEmployee" runat="server">
                    <table width="100%">
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpReportCompleteByEmployeeTop" runat="server">
                                    <Triggers>
                                        <asp:PostBackTrigger ControlID="btnReportCompleteByEmployeePrint" />
                                    </Triggers>
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td colspan="2" style="width: 100%;">
                                                    <asp:Label ID="ttlReportCompleteByEmployee" runat="server" Text="Report - Training Completed By Employee"
                                                        Font-Bold="True" Font-Size="X-Large" Style="text-align: center" Width="100%"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="top" style="width: 25%;">
                                                    <br />
                                                    <asp:Label ID="lblReportCompleteEmployee" runat="server" Text="Employee:"></asp:Label>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:DropDownList ID="ddlReportCompleteEmployee" runat="server" Font-Names="Courier New"
                                                        AutoPostBack="True">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2" style="width: 100%; text-align: center;">
                                                    <asp:Button ID="btnReportCompleteByEmployeeGenerate" runat="server" Text="Generate"
                                                        Width="100px" />&nbsp;
                                                    <asp:Button ID="btnReportCompleteByEmployeeClear" runat="server" Text="Clear" Width="100px" />&nbsp;
                                                    <asp:Button ID="btnReportCompleteByEmployeePrint" runat="server" Text="Print" Width="100px" />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpReportCompleteByEmployeeBottom" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td style="width: 100%; text-align: center;">
                                                    <br />
                                                    <center>
                                                        <asp:GridView ID="gridReportCompleteByEmployee" runat="server" AutoGenerateColumns="False"
                                                            BackColor="Silver" EmptyDataText="The specified employee has not been trained for any jobs.">
                                                            <EmptyDataRowStyle BackColor="White" BorderColor="Black" BorderStyle="Solid" ForeColor="Black"
                                                                HorizontalAlign="Center" VerticalAlign="Middle" />
                                                            <Columns>
                                                                <asp:HyperLinkField DataTextField="Job" HeaderText="Job" ItemStyle-HorizontalAlign="Left"
                                                                    DataNavigateUrlFields="Address">
                                                                    <ItemStyle HorizontalAlign="Left" />
                                                                </asp:HyperLinkField>
                                                                <asp:BoundField DataField="Machine" HeaderText="Machine" ItemStyle-HorizontalAlign="Left">
                                                                    <ItemStyle HorizontalAlign="Left" />
                                                                </asp:BoundField>
                                                                <asp:BoundField DataField="Category" HeaderText="Category" ItemStyle-HorizontalAlign="Left">
                                                                    <ItemStyle HorizontalAlign="Left" />
                                                                </asp:BoundField>
                                                                <asp:BoundField DataField="Status" HeaderText="Status" ItemStyle-HorizontalAlign="Left">
                                                                    <ItemStyle HorizontalAlign="Left" />
                                                                </asp:BoundField>
                                                            </Columns>
                                                            <HeaderStyle BackColor="Gray" />
                                                            <AlternatingRowStyle BackColor="White" />
                                                        </asp:GridView>
                                                    </center>
                                                    <br />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                    </table>
                </asp:View>
                <asp:View ID="viewReportIncompleteTraining" runat="server">
                    <table width="100%">
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpReportIncompleteTrainingTop" runat="server">
                                    <Triggers>
                                        <asp:PostBackTrigger ControlID="btnReportIncompleteTrainingPrint" />
                                    </Triggers>
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td colspan="2" style="width: 100%;">
                                                    <asp:Label ID="ttlReportIncompleteTraining" runat="server" Text="Report - Incomplete Training"
                                                        Font-Bold="True" Font-Size="X-Large" Style="text-align: center" Width="100%"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="top" style="width: 25%;">
                                                    <br />
                                                    <asp:Label ID="lblReportIncompleteSupervisor" runat="server" Text="Supervisor:"></asp:Label>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:DropDownList ID="ddlReportIncompleteSupervisor" runat="server" Font-Names="Courier New"
                                                        AutoPostBack="True" CausesValidation="True">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="top" style="width: 25%;">
                                                    <br />
                                                    <asp:Label ID="lblReportIncompleteReport" runat="server" Text="Report:"></asp:Label>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:DropDownList ID="ddlReportIncompleteReport" runat="server" Font-Names="Courier New"
                                                        AutoPostBack="True" CausesValidation="True">
                                                        <asp:ListItem>Make A Selection</asp:ListItem>
                                                        <asp:ListItem>No Training</asp:ListItem>
                                                        <asp:ListItem>Safety Incomplete</asp:ListItem>
                                                        <asp:ListItem>Operational Incomplete</asp:ListItem>
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2" style="width: 100%; text-align: center;">
                                                    <asp:Button ID="btnReportIncompleteTrainingGenerate" runat="server" Text="Generate"
                                                        Width="100px" CausesValidation="False" />&nbsp;
                                                    <asp:Button ID="btnReportIncompleteTrainingClear" runat="server" Text="Clear" Width="100px" />&nbsp;
                                                    <asp:Button ID="btnReportIncompleteTrainingPrint" runat="server" Text="Print" Width="100px" />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpReportIncompleteTrainingBottom" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td style="width: 100%; text-align: center;">
                                                    <br />
                                                    <center>
                                                        <asp:Label ID="lblReportIncompleteTrainingHeading" runat="server" Text="Label" Font-Bold="True"
                                                            Font-Size="Large"></asp:Label>
                                                        <br />
                                                        <br />
                                                        <asp:GridView ID="gridReportIncompleteTraining" runat="server" AutoGenerateColumns="False"
                                                            BackColor="Silver">
                                                            <Columns>
                                                                <asp:BoundField DataField="EMPLOYEE" HeaderText="Employee" ItemStyle-HorizontalAlign="Left">
                                                                    <ItemStyle HorizontalAlign="Left" />
                                                                </asp:BoundField>
                                                                <asp:BoundField DataField="SUPERVISOR" HeaderText="Supervisor" ItemStyle-HorizontalAlign="Left">
                                                                    <ItemStyle HorizontalAlign="Left" />
                                                                </asp:BoundField>
                                                            </Columns>
                                                            <HeaderStyle BackColor="Gray" />
                                                            <AlternatingRowStyle BackColor="White" />
                                                        </asp:GridView>
                                                    </center>
                                                    <asp:Label ID="lblReportIncompleteTrainingNoEmp" runat="server" Text="No records exist for this report."
                                                        Font-Bold="True"></asp:Label>
                                                    <br />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                    </table>
                </asp:View>
                <asp:View ID="viewReportWaitingForSupervisor" runat="server">
                    <table width="100%">
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpReportWaitingForSupervisorTop" runat="server">
                                    <Triggers>
                                        <asp:PostBackTrigger ControlID="btnReportWaitingForSupervisorPrint" />
                                    </Triggers>
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td colspan="2" style="width: 100%;">
                                                    <asp:Label ID="ttlReportWaitingForSupervisor" runat="server" Text="Report - Training Waiting for Supervisor Signature"
                                                        Font-Bold="True" Font-Size="X-Large" Style="text-align: center" Width="100%"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="top" style="width: 25%;">
                                                    <br />
                                                    <asp:Label ID="lblReportWaitingForSupervisor" runat="server" Text="Supervisor:"></asp:Label>
                                                </td>
                                                <td>
                                                    <br />
                                                    <asp:DropDownList ID="ddlReportWaitingForSupervisor" runat="server" Font-Names="Courier New"
                                                        AutoPostBack="True" CausesValidation="True">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2" style="width: 100%; text-align: center;">
                                                    <asp:Button ID="btnReportWaitingForSupervisorGenerate" runat="server" Text="Generate"
                                                        Width="100px" />&nbsp;
                                                    <asp:Button ID="btnReportWaitingForSupervisorClear" runat="server" Text="Clear" Width="100px" />&nbsp;
                                                    <asp:Button ID="btnReportWaitingForSupervisorPrint" runat="server" Text="Print" Width="100px" />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="udpReportWaitingForSupervisorBottom" runat="server">
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td style="width: 100%; text-align: center;">
                                                    <br />
                                                    <center>
                                                        <asp:GridView ID="gridReportWaitingForSupervisor" runat="server" AutoGenerateColumns="False"
                                                            BackColor="Silver">
                                                            <EmptyDataRowStyle BackColor="White" BorderColor="Black" BorderStyle="Solid" ForeColor="Black"
                                                                HorizontalAlign="Center" VerticalAlign="Middle" />
                                                            <Columns>
                                                                <asp:HyperLinkField DataTextField="Employee" HeaderText="Employee" ItemStyle-HorizontalAlign="Left"
                                                                    DataNavigateUrlFields="Address" />
                                                                <asp:BoundField DataField="Job" HeaderText="Job" ItemStyle-HorizontalAlign="Left">
                                                                </asp:BoundField>
                                                                <asp:BoundField DataField="Machine" HeaderText="Machine" ItemStyle-HorizontalAlign="Left">
                                                                    <ItemStyle HorizontalAlign="Left" />
                                                                </asp:BoundField>
                                                                <asp:BoundField DataField="Category" HeaderText="Category" ItemStyle-HorizontalAlign="Left">
                                                                    <ItemStyle HorizontalAlign="Left" />
                                                                </asp:BoundField>
                                                                <asp:BoundField DataField="SUPERVISOR" HeaderText="Supervisor" ItemStyle-HorizontalAlign="Left">
                                                                    <ItemStyle HorizontalAlign="Left" />
                                                                </asp:BoundField>
                                                            </Columns>
                                                            <HeaderStyle BackColor="Gray" />
                                                            <AlternatingRowStyle BackColor="White" />
                                                        </asp:GridView>
                                                    </center>
                                                    <asp:Label ID="lblReportWaitingForSupervisorNoEmp" runat="server" Text="No records exist for this report."
                                                        Font-Bold="True"></asp:Label>
                                                    <br />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                    </table>
                </asp:View>

                <asp:View ID="viewReportTrainerReport" runat="server">
                    <table width="100%">
                        <tr>
                            <td>
                                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                                    <Triggers>
                                        <asp:PostBackTrigger ControlID="ddlTRAINER_REPORT" />
                                        <asp:PostBackTrigger ControlID="btnTrainerReportPrint" />
                                    </Triggers>
                                    <ContentTemplate>
                                        <table width="100%">
                                            <tr>
                                                <td colspan="2" style="width: 100%;">
                                                    <asp:Label ID="ttlTrainerReport" runat="server" Text="Trainer Report"
                                                        Font-Bold="True" Font-Size="X-Large" Style="text-align: center" Width="100%"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="top" style="width: 25%;">
                                                    <br />
                                                    <br />
                                                </td>
                                                <td>
                                                    &nbsp;&nbsp;<br />&nbsp;
                                                    <asp:Label ID="lblTrainer" runat="server" Text="Trainer Name:   "></asp:Label>
                                                    <asp:DropDownList ID="ddlTRAINER_REPORT" runat="server" AutoPostBack="True">
                                                    </asp:DropDownList>
                                                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                                    <asp:Button ID="btnTrainerReportPrint" runat="server" Text="Print" Width="100px" />
                                                    <br />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2" style="width: 100%; text-align: center;">

                                                    &nbsp;<br /> &nbsp;<center>
                                                        <asp:GridView ID="gridTrainerReport" runat="server" BackColor="Silver" AutoGenerateColumns="False" >
                                                            <EmptyDataRowStyle BackColor="White" BorderColor="Black" BorderStyle="Solid" ForeColor="Black" HorizontalAlign="Center" VerticalAlign="Middle" />
                                                            <HeaderStyle BackColor="Gray" />
                                                            <AlternatingRowStyle BackColor="White" />
                                                            <Columns>
                                                                <asp:BoundField DataField="EMPLOYEE" HeaderText="Trainer Name" ItemStyle-HorizontalAlign="Left">
                                                                </asp:BoundField>
                                                                <asp:BoundField DataField="Machine" HeaderText="Machine Code" ItemStyle-HorizontalAlign="Left">
                                                                    <ItemStyle HorizontalAlign="Left" />
                                                                </asp:BoundField>
                                                                <asp:BoundField DataField="MASTER" HeaderText="Master Job Title" ItemStyle-HorizontalAlign="Left">
                                                                    <ItemStyle HorizontalAlign="Left" />
                                                                </asp:BoundField>
                                                                <asp:BoundField DataField="Job" HeaderText="Job Title" ItemStyle-HorizontalAlign="Left">
                                                                    <ItemStyle HorizontalAlign="Left" />
                                                                </asp:BoundField>
                                                            </Columns>
                                                        </asp:GridView>
                                                        <br />
<asp:SqlDataSource ID="SqlDataSourcetblTrainerQualifications" runat="server" ConnectionString="<%$ ConnectionStrings:OJTConnectionString %>" SelectCommand="SELECT tblTRAINER_QUALIFICATIONS.EMPLOYEE_ID, tblTRAINER_QUALIFICATIONS.MACHINE_CODE, tblEMPLOYEES.FIRST_NAME, tblEMPLOYEES.LAST_NAME, tblMASTER_JOB_TITLES.MASTER_JOB_TITLE, tblJOB_TITLES.JOB_TITLE FROM tblTRAINER_QUALIFICATIONS INNER JOIN tblEMPLOYEES ON tblTRAINER_QUALIFICATIONS.EMPLOYEE_ID = tblEMPLOYEES.EMPLOYEE_ID INNER JOIN tblMASTER_JOB_TITLES ON tblTRAINER_QUALIFICATIONS.MASTER_JOB_NUMBER = tblMASTER_JOB_TITLES.MASTER_JOB_NUMBER INNER JOIN tblJOB_TITLES ON tblTRAINER_QUALIFICATIONS.JOB_TITLE_NUMBER = tblJOB_TITLES.JOB_TITLE_NUMBER WHERE (tblTRAINER_QUALIFICATIONS.EMPLOYEE_ID IN (SELECT EMPLOYEE_ID FROM tblEMPLOYEES AS tblEMPLOYEES_1 WHERE (TRAINER = 'Y') AND (ACTIVE_STATUS = 'A')))"></asp:SqlDataSource>
                                                        </center>
                                                    <br />
                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                    </table>
                </asp:View>
            </asp:MultiView>
        </ContentTemplate>
    </asp:UpdatePanel>
    <div style="width: 500px; height: 140px; z-index: auto;">
        <asp:UpdatePanel ID="udpMessage" runat="server">
            <ContentTemplate>
                <ajax:ModalPopupExtender ID="mpeMessage" runat="server" BackgroundCssClass="ModalBackground"
                    PopupControlID="pnlMessage" TargetControlID="pnlMessage" DropShadow="True" OkControlID="btnMessageOK">
                </ajax:ModalPopupExtender>
                <asp:Panel ID="pnlMessage" runat="server" Height="140px" Width="500px" BackColor="#FFFFFF"
                    BorderStyle="Double" DefaultButton="btnMessageOK" Style="display: none">
                    <table width="100%">
                        <tr>
                            <td id="ttlMessageTitle" style="background-color: #FFFF00; border-style: none none solid none; 
                                border-width: thin; border-color: #000000">
                                <center>
                                    <asp:Label ID="lblMessageTitle" runat="server" Text="Put message title here..." Font-Bold="True"></asp:Label>
                                </center>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <br />
                                <asp:Label ID="lblMessageBody" runat="server" Text="Put message text here..."></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <br />
                                <center>
                                    <asp:Button ID="btnMessageOK" runat="server" Text="OK" Width="100px" />
                                </center>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>

        <script language="javascript" type="text/javascript">
            // <![CDATA[
            function fnClickOK(sender, e) {
                __doPostBack(sender, e);
            }
            // ]]>
        </script>

    </div>
</asp:Content>
