Option Strict On
Option Explicit On

Imports System.Data.Linq
Imports System.Net.Mail
Imports OfficeOpenXml
Imports System.Data

' ****************************************************************************
'    Name: OJTWebPage class
'
' Purpose: Provide the capabilityes to manage employee training data via
'          a web based tool
'
' ****************************************************************************
Partial Class OJTWebPage
    Inherits System.Web.UI.Page

    Private Const ALL_SUPERVISORS_ID As Integer = -99
    Private Const MAKE_A_SELECTION As String = "Make A Selection"

    Private db As OJT_DB_ClassesDataContext      ' Reference to the database


#Region "Main, Page, and MessageBox Methods"

    ' ************************************************************************
    '    The methods below are for the Main Form, Main Menu, and MessageBox
    ' ************************************************************************

    ' ************************************************************************
    '    Name: Page_Load
    '
    ' Purpose: When the page loads, open a connection to the database and
    '          detrmine if anyone is logged in
    '
    ' ************************************************************************
    Protected Sub Page_Load(ByVal Sender As System.Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            ' Connect to the database
            db = New OJT_DB_ClassesDataContext(ConfigurationManager.ConnectionStrings.Item("OJTConnectionString").ConnectionString)

            ' Register the message dialog's callback
            btnMessageOK.OnClientClick = "fnClickOK('" & btnMessageOK.UniqueID & "','')"

            ' Remove the Maintenance and Report menus if the current user is not an administrator
            If (Session("CURRENT_LOGIN_ID") Is Nothing) Then
                menuFull.Visible = False
                menuSimple.Visible = True
                Login1.Visible = True
            Else
                menuFull.Visible = True
                menuSimple.Visible = False
                Login1.Visible = False
            End If

            ' If Query Strings are appended to the web address, process them
            Dim qStrings As NameValueCollection = Request.QueryString()
            If ((qStrings.Count <> 0) And (Not IsPostBack)) Then
                ClearEmployeeTrainingFields()
                ProcessQueryStrings()
            End If
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ProcessQueryStrings
    '
    ' Purpose: Process the query string and display the proper screen based on
    '          the information in the fields
    '
    ' ************************************************************************
    Protected Sub ProcessQueryStrings()
        Dim qStrings As NameValueCollection = Request.QueryString()
        Dim screen As String = qStrings.Get("sc")
        Dim value As String

        Select Case screen

            Case "et"
                ' Employee Training
                MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(viewEmployeeTraining)

                ' Get the employee ID and query the database to identify the selected index
                value = qStrings.Get("ed")
                Dim empID As Integer = Integer.Parse(value)
                Session("SELECTED_EMPLOYEE_ID") = empID
                Dim empQuery As String = _
                    (From Emps In db.tblEMPLOYEEs() _
                     Order By Emps.LAST_NAME, Emps.FIRST_NAME _
                     Where Emps.EMPLOYEE_ID = empID _
                     Select FULL_NAME = Emps.LAST_NAME + ", " + Emps.FIRST_NAME).FirstOrDefault()
                Session("SELECTED_EMPLOYEE") = empQuery

                ' Get the job number and query the database to identify the selected index
                value = qStrings.Get("jn")
                Dim jNum As Integer = Integer.Parse(value)
                Session("SELECTED_JOB_NUMBER") = jNum
                Dim jobTitleQuery As String = _
                    (From Titles In db.tblMASTER_JOB_TITLEs() _
                     Order By Titles.MASTER_JOB_TITLE _
                     Where Titles.MASTER_JOB_NUMBER = jNum _
                     Select Titles.MASTER_JOB_TITLE).FirstOrDefault()
                Session("SELECTED_JOB") = jobTitleQuery

                ' Get the machine code and query the database to identify the selected index
                value = qStrings.Get("mc")
                Dim mCode As String = value
                Session("SELECTED_MACHINE_CODE") = value
                Session("SELECTED_MACHINE") = value

                ' Get the category number and query the database to identify the selected index
                value = qStrings.Get("cn")
                Dim cNum As Integer = Integer.Parse(value)
                Session("SELECTED_CATEGORY_NUMBER") = Integer.Parse(value)
                Dim categoryQuery As String = _
                    (From cats In db.tblJOB_TITLEs() _
                     Order By cats.JOB_TITLE _
                     Where cats.JOB_TITLE_NUMBER = cNum _
                     Select cats.JOB_TITLE).FirstOrDefault()
                Session("SELECTED_CATEGORY") = categoryQuery

                ' Fake a button push to populate the display
                Session("INCLUDE_INACTIVE_EMPLOYEES") = True
                btnEmpTrainLookup_Click(Nothing, Nothing)

            Case Else
                MultiView1.ActiveViewIndex = -1

        End Select
    End Sub

    ' ************************************************************************
    '    Name: Login_Authenticate
    '
    ' Purpose: When a user tries to log in see if they are an administrator
    '
    ' ************************************************************************
    Protected Sub Login_Authenticate(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.AuthenticateEventArgs) Handles Login1.Authenticate
        Try
            Session("CURRENT_LOGIN_ID") = Nothing

            ' See if the specified user name is in the database
            Dim loginQuery As tblADMINISTRATOR = _
                (From admins In db.tblADMINISTRATORs _
                 Where admins.LOGIN = Login1.UserName _
                 Select admins).FirstOrDefault()

            ' See if the specified password matches the database
            If Not (loginQuery Is Nothing) Then
                If Login1.Password = loginQuery.PASSWORD Then
                    Session("CURRENT_LOGIN_ID") = loginQuery.LOGIN
                    e.Authenticated = True
                End If
            End If
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: MAIN_MENU_SelectedNodeChanged
    '
    ' Purpose: When an item is selected on the main menu activate the 
    '          associated view
    '
    ' ************************************************************************
    Protected Sub MAIN_MENU_SelectedNodeChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles menuSimple.SelectedNodeChanged, menuFull.SelectedNodeChanged
        Try
            Dim menu As System.Web.UI.WebControls.TreeView = CType(sender, System.Web.UI.WebControls.TreeView)

            Session("OJT_BACK") = Nothing

            Select Case menu.SelectedValue
                Case "Training Codes"
                    ClearTrainingCodesFields()
                    MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(viewTrainingCodes)

                Case "Job Titles"
                    ClearJobTitlesFields()
                    MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(viewJobTitles)

                Case "Machine Codes"
                    ClearMachineCodesFields()
                    MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(viewMachineCodes)

                Case "Categories"
                    ClearCategoriesFields()
                    MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(viewCategories)

                Case "Approvals"
                    ClearApprovalFields()
                    MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(viewApprovals)

                Case "By Machine"
                    ClearByMachineFields()
                    MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(viewByMachine)

                Case "By Training Code"
                    ClearByTrainingCodeFields()
                    MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(viewByTrainingCode)

                Case "Trainers"
                    ClearTrainerFields()
                    MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(viewTrainer)

                Case "Employees"
                    ClearEmployeesFields()
                    MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(viewEmployees)

                Case "Employee Training"
                    ClearEmployeeTrainingFields()
                    MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(viewEmployeeTraining)

                Case "Administrators"
                    ClearAdministratorsFields()
                    MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(viewAdministrators)

                Case "By Job"
                    ClearReportCompleteByJobFields()
                    MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(viewReportCompleteByJob)

                Case "By Employee"
                    ClearReportCompleteByEmployeeFields()
                    MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(viewReportCompleteByEmployee)

                Case "Incomplete Training"
                    ClearReportIncompleteTrainingFields()
                    MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(viewReportIncompleteTraining)

                Case "Waiting for Supervisor"
                    ClearReportWaitingForSupervisorFields()
                    MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(viewReportWaitingForSupervisor)

                Case "Trainer Report"
                    'ClearTrainerReportForSupervisorFields()
                    TrainerReportSetUp()
                    MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(viewReportTrainerReport)

                Case Else
                    MultiView1.ActiveViewIndex = -1
            End Select
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: MessageBox
    '
    ' Purpose: Helper method to open a message dialog on the client to
    '          display the specified title and message text
    '
    ' ************************************************************************
    Public Sub MessageBox(ByVal messageText As String, ByVal messageTitle As String)
        Try
            lblMessageTitle.Text = messageTitle
            lblMessageBody.Text = messageText

            pnlMessage.Visible = True
            mpeMessage.Show()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnMessageOK_Click
    '
    ' Purpose: When the message box's OK button is clicked, close the message box
    '
    ' ************************************************************************
    Protected Sub btnMessageOK_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnMessageOK.Click
        Try
            lblMessageTitle.Text = ""
            lblMessageBody.Text = ""

            mpeMessage.Hide()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

#End Region

#Region "Training Code Maintenance Methods"

    ' ************************************************************************
    '           The methods below are for TRAINING CODE MAINTENANCE 
    ' ************************************************************************



    ' ************************************************************************
    '    Name: viewTrainingCodes_PreRender
    '
    ' Purpose: Update the list of training codes before the view is displayed
    '
    ' ************************************************************************
    Protected Sub viewTrainingCodes_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles viewTrainingCodes.PreRender
        Try
            ' Query the database to retrieve the training codes and bind them to the widget
            Dim populateQuery As List(Of String) = _
                (From tCodes In db.tblTRAINING_CODEs() _
                 Select tCodes.TITLE _
                 Order By TITLE).ToList()
            lstTrainingCodes.DataSource = populateQuery
            lstTrainingCodes.DataBind()
            lstTrainingCodes.SelectedIndex = populateQuery.IndexOf(Session("SELECTED_TRAINING_CODE").ToString())
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnNewTrainingCodes_Click
    '
    ' Purpose: When the training code view's NEW button is clicked,
    '          show the detail widgets
    '
    ' ************************************************************************
    Protected Sub btnNewTrainingCodes_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnNewTrainingCodes.Click
        Try
            ClearTrainingCodesFields()
            btnApplyTrainingCodes.Text = "Add"
            txtTrainingCodeTitle.Focus()
            udpTrainingCodesBottom.Visible = True
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnEditTrainingCodes_Click
    '
    ' Purpose: When the training code view's EDIT button is clicked, update
    '          the detail widgets with all the info for the selected entry
    '
    ' ************************************************************************
    Protected Sub btnEditTrainingCodes_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnEditTrainingCodes.Click
        Try
            If (lstTrainingCodes.SelectedIndex = -1) Then
                MessageBox("You must select an entry in the list before you can edit.", "USER ERROR")
                Exit Sub
            End If

            ' Retrieve the database record for the selected entry
            Dim editQuery As tblTRAINING_CODE = _
                (From tCodes In db.tblTRAINING_CODEs() _
                 Select tCodes _
                 Where tCodes.TITLE = lstTrainingCodes.SelectedValue).FirstOrDefault()

            ' Display the record's information in the detail widgets
            Session("SELECTED_TRAINING_CODE_ID") = editQuery.TRAINING_NUMBER
            Session("SELECTED_TRAINING_CODE") = lstTrainingCodes.SelectedValue
            txtTrainingCodeTitle.Text = editQuery.TITLE
            chbTrainingCodeDependent.Checked = editQuery.MACHINE_DEPENDENT.Equals("Y")
            txtTrainingCodeDescription.Text = editQuery.DESCRIPTION
            txtTrainingCodeTitle.Focus()

            btnApplyTrainingCodes.Text = "Update"
            udpTrainingCodesBottom.Visible = True
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnDeleteTrainingCodes_Click
    '
    ' Purpose: When the training code view's DELETE button is clicked,
    '          remove the info for the selected entry from the database
    '
    ' ************************************************************************
    Protected Sub btnDeleteTrainingCodes_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnDeleteTrainingCodes.Click
        Try
            If (lstTrainingCodes.SelectedIndex = -1) Then
                MessageBox("You must select an entry in the list before you can delete.", "USER ERROR")
                Exit Sub
            End If

            ' Retrieve the database record(s) to be deleted
            Dim delTcQuery As tblTRAINING_CODE = _
                (From tCodes In db.tblTRAINING_CODEs() _
                 Select tCodes _
                 Where tCodes.TITLE = lstTrainingCodes.SelectedValue).FirstOrDefault()

            Dim delJmmQuery As List(Of tblJOB_MACHINE_MASTER) = _
                (From jmm In db.tblJOB_MACHINE_MASTERs() _
                 Select jmm _
                 Where jmm.TRAINING_NUMBER = delTcQuery.TRAINING_NUMBER).ToList()

            Dim delEqQuery As List(Of tblEMPLOYEE_QUALIFICATION) = _
                (From Eq In db.tblEMPLOYEE_QUALIFICATIONs() _
                 Select Eq _
                 Where Eq.TRAINING_NUMBER = delTcQuery.TRAINING_NUMBER).ToList()

            ' Delete the selected records from the database
            db.tblTRAINING_CODEs().DeleteOnSubmit(delTcQuery)
            db.tblJOB_MACHINE_MASTERs().DeleteAllOnSubmit(delJmmQuery)
            db.tblEMPLOYEE_QUALIFICATIONs().DeleteAllOnSubmit(delEqQuery)
            db.SubmitChanges()

            ' Clear the selection and the detail widgets
            lstTrainingCodes.ClearSelection()
            ClearTrainingCodesFields()
            btnApplyTrainingCodes.Text = "Add"
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnApplyTrainingCodes_Click
    '
    ' Purpose: When the training code view's APPLY button is clicked,
    '          add or update the info in the database
    '
    ' ************************************************************************
    Protected Sub btnApplyTrainingCodes_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnApplyTrainingCodes.Click
        Try
            If ((txtTrainingCodeTitle.Text.Trim.Length = 0) Or (txtTrainingCodeDescription.Text.Trim.Length = 0)) Then
                MessageBox("You must specify a Title and Description before you can save.", "USER ERROR")
                Exit Sub
            End If

            ' Convert the check box to a string
            Dim machineDependent As String = "Y"
            ' TPL - 20-NOV-2014 - All training is now machine dependent
            'If (chbTrainingCodeDependent.Checked = True) Then
            '    machineDependent = "Y"
            'Else
            '    machineDependent = "N"
            'End If

            ' If the ID isn't 0 then it is the key to the entry we're updating
            Dim tCode As Integer = Integer.Parse(Session("SELECTED_TRAINING_CODE_ID").ToString())
            If (tCode <> 0) Then
                Dim updateQuery As tblTRAINING_CODE = _
                    (From tCodes In db.tblTRAINING_CODEs() _
                     Select tCodes _
                     Where tCodes.TRAINING_NUMBER = tCode).FirstOrDefault()

                If updateQuery Is Nothing Then
                    MessageBox("Unable to update record #" + Session("SELECTED_TRAINING_CODE_ID").ToString(), "SYSTEM ERROR")
                Else
                    updateQuery.DESCRIPTION = txtTrainingCodeDescription.Text.Trim
                    updateQuery.TITLE = txtTrainingCodeTitle.Text.Trim
                    updateQuery.MACHINE_DEPENDENT = machineDependent
                    db.SubmitChanges()
                End If

                ClearTrainingCodesFields()
            Else
                ' Otherwise, we're adding a new record
                Dim number As Integer = GetNextTrainingCodeNumber()
                Dim newCode As New tblTRAINING_CODE With { _
                    .TRAINING_NUMBER = number, _
                    .TITLE = txtTrainingCodeTitle.Text.Trim, _
                    .MACHINE_DEPENDENT = machineDependent, _
                    .DESCRIPTION = txtTrainingCodeDescription.Text.Trim}
                db.tblTRAINING_CODEs().InsertOnSubmit(newCode)
                db.SubmitChanges()
            End If

            btnApplyTrainingCodes.Text = "Add"
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnClearTrainingCodes_Click
    '
    ' Purpose: When the training code view's CLEAR button is clicked,
    '          blank the detail widgets
    '
    ' ************************************************************************
    Protected Sub btnClearTrainingCodes_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnClearTrainingCodes.Click
        Try
            ClearTrainingCodesFields()
            btnApplyTrainingCodes.Text = "Add"
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ClearTrainingCodesFields
    '
    ' Purpose: Helper method to blank the training code view's detail widgets
    '
    ' ************************************************************************
    Private Sub ClearTrainingCodesFields()
        lstTrainingCodes.ClearSelection()
        txtTrainingCodeTitle.Text = ""
        chbTrainingCodeDependent.Checked = False
        txtTrainingCodeDescription.Text = ""

        Session("SELECTED_TRAINING_CODE_ID") = 0
        Session("SELECTED_TRAINING_CODE") = ""

        udpTrainingCodesBottom.Visible = False
    End Sub

    ' ************************************************************************
    '    Name: GetNextTrainingCodeNumber
    '
    ' Purpose: Generate a unique ID for a new entry in the TRAINING_CODES table
    '
    ' ************************************************************************
    Private Function GetNextTrainingCodeNumber() As Integer
        Dim rtnValue As Integer = 0

        ' Lookup the last training code number, store it as the return value,
        ' increment the training code number, and update the database
        Dim controlQuery As tblOJT_CONTROL = _
            (From controls In db.tblOJT_CONTROLs() _
             Select controls _
             Where controls.RECORD_ID = "A").FirstOrDefault()

        If controlQuery Is Nothing Then
            MessageBox("Unable to retrieve Training Code control number.", "SYSTEM ERROR")
        Else
            rtnValue = controlQuery.LAST_TRAINING_NUMBER.Value
            controlQuery.LAST_TRAINING_NUMBER = rtnValue + 1
            db.SubmitChanges()
        End If

        Return rtnValue
    End Function

#End Region

#Region "Job Title Maintenance Methods"

    ' ************************************************************************
    '           The methods below are for JOB TITLE MAINTENANCE 
    ' ************************************************************************



    ' ************************************************************************
    '    Name: viewJobTitles_PreRender
    '
    ' Purpose: Update the list of job titles before the view is displayed
    '
    ' ************************************************************************
    Protected Sub viewJobTitles_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles viewJobTitles.PreRender
        Try
            ' Query the database to retrieve the job titles and bind them to the widget
            Dim populateQuery As List(Of String) = _
                (From Titles In db.tblMASTER_JOB_TITLEs() _
                 Select Titles.MASTER_JOB_TITLE _
                 Order By MASTER_JOB_TITLE).ToList()
            lstJobTitles.DataSource = populateQuery
            lstJobTitles.DataBind()
            lstJobTitles.ClearSelection()
            lstJobTitles.SelectedIndex = populateQuery.IndexOf(Session("SELECTED_JOB").ToString())
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnNewJobTitles_Click
    '
    ' Purpose: When the job title view's NEW button is clicked,
    '          show the detail widgets
    '
    ' ************************************************************************
    Protected Sub btnNewJobTitles_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnNewJobTitles.Click
        Try
            ClearJobTitlesFields()
            btnApplyJobTitles.Text = "Add"
            txtJobTitleTitle.Focus()
            udpJobTitlesBottom.Visible = True
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnEditJobTitles_Click
    '
    ' Purpose: When the job title view's EDIT button is clicked, update
    '          the detail widgets with all the info for the selected entry
    '
    ' ************************************************************************
    Protected Sub btnEditJobTitles_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnEditJobTitles.Click
        Try
            If (lstJobTitles.SelectedIndex = -1) Then
                MessageBox("You must select an entry in the list before you can edit.", "USER ERROR")
                Exit Sub
            End If

            ' Retrieve the database record for the selected entry
            Dim editQuery As tblMASTER_JOB_TITLE = _
                (From Titles In db.tblMASTER_JOB_TITLEs() _
                 Select Titles _
                 Where Titles.MASTER_JOB_TITLE = lstJobTitles.SelectedValue).FirstOrDefault()

            ' Display the record's information in the detail widgets
            Session("SELECTED_JOB_ID") = editQuery.MASTER_JOB_NUMBER
            Session("SELECTED_JOB") = lstJobTitles.SelectedValue
            txtJobTitleTitle.Text = editQuery.MASTER_JOB_TITLE
            txtJobTitleDescription.Text = editQuery.DESCRIPTION
            txtJobTitleDocument.Text = editQuery.DOCUMENT
            txtJobTitleTitle.Focus()
            btnApplyJobTitles.Text = "Update"
            udpJobTitlesBottom.Visible = True
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnDeleteJobTitles_Click
    '
    ' Purpose: When the job title view's DELETE button is clicked,
    '          remove the info for the selected entry from the database
    '
    ' ************************************************************************
    Protected Sub btnDeleteJobTitles_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnDeleteJobTitles.Click
        Try
            If (lstJobTitles.SelectedIndex = -1) Then
                MessageBox("You must select an entry in the list before you can delete.", "USER ERROR")
                Exit Sub
            End If

            ' Retrieve the database record(s) to be deleted
            Dim delJtQuery As tblMASTER_JOB_TITLE = _
                (From Titles In db.tblMASTER_JOB_TITLEs() _
                 Select Titles _
                 Where Titles.MASTER_JOB_TITLE = lstJobTitles.SelectedValue).FirstOrDefault()

            Dim delTqQuery As List(Of tblTRAINER_QUALIFICATION) = _
                (From tq In db.tblTRAINER_QUALIFICATIONs() _
                 Select tq _
                 Where tq.MASTER_JOB_NUMBER = delJtQuery.MASTER_JOB_NUMBER).ToList()

            Dim delEqQuery As List(Of tblEMPLOYEE_QUALIFICATION) = _
                (From eq In db.tblEMPLOYEE_QUALIFICATIONs() _
                 Select eq _
                 Where eq.MASTER_JOB_NUMBER = delJtQuery.MASTER_JOB_NUMBER).ToList()

            Dim delEq2Query As List(Of tblEMPLOYEES_QUALIFIED) = _
                (From eq In db.tblEMPLOYEES_QUALIFIEDs() _
                 Select eq _
                 Where eq.MASTER_JOB_NUMBER = delJtQuery.MASTER_JOB_NUMBER).ToList()

            Dim delJmmQuery As List(Of tblJOB_MACHINE_MASTER) = _
                (From jmm In db.tblJOB_MACHINE_MASTERs() _
                 Select jmm _
                 Where jmm.MASTER_JOB_NUMBER = delJtQuery.MASTER_JOB_NUMBER).ToList()

            Dim delJtaQuery As List(Of tblJOB_TITLE_APPROVAL) = _
                (From jta In db.tblJOB_TITLE_APPROVALs() _
                 Select jta _
                 Where jta.MASTER_JOB_NUMBER = delJtQuery.MASTER_JOB_NUMBER).ToList()

            ' Delete the selected records from the database
            db.tblMASTER_JOB_TITLEs().DeleteOnSubmit(delJtQuery)
            db.tblTRAINER_QUALIFICATIONs().DeleteAllOnSubmit(delTqQuery)
            db.tblEMPLOYEE_QUALIFICATIONs().DeleteAllOnSubmit(delEqQuery)
            db.tblEMPLOYEES_QUALIFIEDs().DeleteAllOnSubmit(delEq2Query)
            db.tblJOB_MACHINE_MASTERs().DeleteAllOnSubmit(delJmmQuery)
            db.tblJOB_TITLE_APPROVALs().DeleteAllOnSubmit(delJtaQuery)
            db.SubmitChanges()

            ' Clear the selection and the detail widgets
            lstJobTitles.ClearSelection()
            ClearJobTitlesFields()
            btnApplyJobTitles.Text = "Add"
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnApplyJobTitles_Click
    '
    ' Purpose: When the job title view's APPLY button is clicked,
    '          add or update the info in the database
    '
    ' ************************************************************************
    Protected Sub btnApplyJobTitles_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnApplyJobTitles.Click
        Try
            If ((txtJobTitleTitle.Text.Trim.Length = 0) Or (txtJobTitleDescription.Text.Trim.Length = 0)) Then
                MessageBox("You must specify a Title and Description before you can save.", "USER ERROR")
                Exit Sub
            End If

            ' If the ID isn't 0 then it is the key to the entry we're updating
            Dim jtID As Integer = Integer.Parse(Session("SELECTED_JOB_ID").ToString())
            If (jtID <> 0) Then
                Dim updateQuery As tblMASTER_JOB_TITLE = _
                    (From Titles In db.tblMASTER_JOB_TITLEs() _
                     Select Titles _
                     Where Titles.MASTER_JOB_NUMBER = jtID).FirstOrDefault()

                If updateQuery Is Nothing Then
                    MessageBox("Unable to update record #" + Session("SELECTED_JOB_ID").ToString(), "SYSTEM ERROR")
                Else
                    updateQuery.MASTER_JOB_TITLE = txtJobTitleTitle.Text.Trim
                    updateQuery.DESCRIPTION = txtJobTitleDescription.Text.Trim
                    updateQuery.DOCUMENT = txtJobTitleDocument.Text.Trim
                    db.SubmitChanges()
                End If

                ClearJobTitlesFields()
            Else
                ' Otherwise, we're adding a new record
                Dim number As Integer = GetNextJobTitleNumber()
                Dim newTitle As New tblMASTER_JOB_TITLE With { _
                    .MASTER_JOB_NUMBER = number, _
                    .MASTER_JOB_TITLE = txtJobTitleTitle.Text.Trim, _
                    .DESCRIPTION = txtJobTitleDescription.Text.Trim, _
                    .DOCUMENT = txtJobTitleDocument.Text.Trim}
                db.tblMASTER_JOB_TITLEs().InsertOnSubmit(newTitle)
                db.SubmitChanges()
            End If

            btnApplyJobTitles.Text = "Add"
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnClearJobTitles_Click
    '
    ' Purpose: When the job title view's CLEAR button is clicked,
    '          blank the detail widgets
    '
    ' ************************************************************************
    Protected Sub btnClearJobTitles_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnClearJobTitles.Click
        Try
            ClearJobTitlesFields()
            btnApplyJobTitles.Text = "Add"
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ClearJobTitlesFields
    '
    ' Purpose: Helper method to blank the job title view's detail widgets
    '
    ' ************************************************************************
    Private Sub ClearJobTitlesFields()
        lstJobTitles.ClearSelection()
        txtJobTitleTitle.Text = ""
        txtJobTitleDescription.Text = ""
        txtJobTitleDocument.Text = ""
        Session("SELECTED_JOB_ID") = 0
        Session("SELECTED_JOB") = ""

        udpJobTitlesBottom.Visible = False
    End Sub

    ' ************************************************************************
    '    Name: GetNextJobTitleNumber
    '
    ' Purpose: Generate a unique ID for a new entry in the MASTER_JOB_TITLES table
    '
    ' ************************************************************************
    Private Function GetNextJobTitleNumber() As Integer
        Dim rtnValue As Integer = 0

        ' Lookup the last job title number, store it as the return value,
        ' increment the job title number, and update the database
        Dim controlQuery As tblOJT_CONTROL = _
            (From controls In db.tblOJT_CONTROLs() _
             Select controls _
             Where controls.RECORD_ID = "A").FirstOrDefault()

        If controlQuery Is Nothing Then
            MessageBox("Unable to retrieve Job Title control number.", "SYSTEM ERROR")
        Else
            rtnValue = controlQuery.LAST_JOB_TITLE.Value
            controlQuery.LAST_JOB_TITLE = rtnValue + 1
            db.SubmitChanges()
        End If

        Return rtnValue
    End Function

#End Region

#Region "Machine Code Maintenance Methods"

    ' ************************************************************************
    '           The methods below are for MACHINE CODE MAINTENANCE 
    ' ************************************************************************



    ' ************************************************************************
    '    Name: viewMachineCodes_PreRender
    '
    ' Purpose: Update the list of machine codes before the view is displayed
    '
    ' ************************************************************************
    Protected Sub viewMachineCodes_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles viewMachineCodes.PreRender
        Try
            ' Query the database to retrieve the machine codes and bind them to the widget
            Dim populateQuery As List(Of String) = _
                (From mCodes In db.tblMACHINEs() _
                 Select mCodes.MACHINE_CODE _
                 Order By MACHINE_CODE).ToList()
            lstMachineCodes.DataSource = populateQuery
            lstMachineCodes.DataBind()
            lstMachineCodes.SelectedIndex = populateQuery.IndexOf(Session("SELECTED_MACHINE").ToString())
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnNewMachineCodes_Click
    '
    ' Purpose: When the machine code view's NEW button is clicked,
    '          show the detail widgets
    '
    ' ************************************************************************
    Protected Sub btnNewMachineCodes_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnNewMachineCodes.Click
        Try
            ClearMachineCodesFields()
            btnApplyMachineCodes.Text = "Add"
            txtMachineCodeCode.Focus()
            udpMachineCodesBottom.Visible = True
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnEditMachineCodes_Click
    '
    ' Purpose: When the machine code view's EDIT button is clicked, update
    '          the detail widgets with all the info for the selected entry
    '
    ' ************************************************************************
    Protected Sub btnEditMachineCodes_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnEditMachineCodes.Click
        Try
            If (lstMachineCodes.SelectedIndex = -1) Then
                MessageBox("You must select an entry in the list before you can edit.", "USER ERROR")
                Exit Sub
            End If

            ' Retrieve the database record for the selected entry
            Dim editQuery As tblMACHINE = _
                (From mCodes In db.tblMACHINEs() _
                 Select mCodes _
                 Where mCodes.MACHINE_CODE = lstMachineCodes.SelectedValue).FirstOrDefault()

            ' Display the record's information in the detail widgets
            Session("SELECTED_MACHINE_CODE") = editQuery.MACHINE_CODE
            Session("SELECTED_MACHINE") = lstMachineCodes.SelectedValue
            txtMachineCodeCode.Text = editQuery.MACHINE_CODE
            txtMachineCodeDescription.Text = editQuery.DESCRIPTION
            txtMachineCodeCode.Focus()
            btnApplyMachineCodes.Text = "Update"
            udpMachineCodesBottom.Visible = True
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnDeleteMachineCodes_Click
    '
    ' Purpose: When the machine code view's DELETE button is clicked,
    '          remove the info for the selected entry from the database
    '
    ' ************************************************************************
    Protected Sub btnDeleteMachineCodes_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnDeleteMachineCodes.Click
        Try
            If (lstMachineCodes.SelectedIndex = -1) Then
                MessageBox("You must select an entry in the list before you can delete.", "USER ERROR")
                Exit Sub
            End If

            ' Retrieve the database record(s) to be deleted
            Dim delMQuery As tblMACHINE = _
                (From mCodes In db.tblMACHINEs() _
                 Select mCodes _
                 Where mCodes.MACHINE_CODE = lstMachineCodes.SelectedValue).FirstOrDefault()

            Dim delTqQuery As List(Of tblTRAINER_QUALIFICATION) = _
                (From tq In db.tblTRAINER_QUALIFICATIONs() _
                 Select tq _
                 Where tq.MACHINE_CODE = delMQuery.MACHINE_CODE).ToList()

            Dim delEqQuery As List(Of tblEMPLOYEE_QUALIFICATION) = _
                (From eq In db.tblEMPLOYEE_QUALIFICATIONs() _
                 Select eq _
                 Where eq.MACHINE_CODE = delMQuery.MACHINE_CODE).ToList()

            Dim delEq2Query As List(Of tblEMPLOYEES_QUALIFIED) = _
                (From eq In db.tblEMPLOYEES_QUALIFIEDs() _
                 Select eq _
                 Where eq.MACHINE_CODE = delMQuery.MACHINE_CODE).ToList()

            Dim delJmmQuery As List(Of tblJOB_MACHINE_MASTER) = _
                (From jmm In db.tblJOB_MACHINE_MASTERs() _
                 Select jmm _
                 Where jmm.MACHINE_CODE = delMQuery.MACHINE_CODE).ToList()

            Dim delJtaQuery As List(Of tblJOB_TITLE_APPROVAL) = _
                (From jta In db.tblJOB_TITLE_APPROVALs() _
                 Select jta _
                 Where jta.MACHINE_CODE = delMQuery.MACHINE_CODE).ToList()

            ' Delete the selected records from the database
            db.tblMACHINEs().DeleteOnSubmit(delMQuery)
            db.tblTRAINER_QUALIFICATIONs().DeleteAllOnSubmit(delTqQuery)
            db.tblEMPLOYEE_QUALIFICATIONs().DeleteAllOnSubmit(delEqQuery)
            db.tblEMPLOYEES_QUALIFIEDs().DeleteAllOnSubmit(delEq2Query)
            db.tblJOB_MACHINE_MASTERs().DeleteAllOnSubmit(delJmmQuery)
            db.tblJOB_TITLE_APPROVALs().DeleteAllOnSubmit(delJtaQuery)
            db.SubmitChanges()

            ' Clear the selection and the detail widgets
            lstMachineCodes.ClearSelection()
            ClearMachineCodesFields()
            btnApplyMachineCodes.Text = "Add"
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnApplyMachineCodes_Click
    '
    ' Purpose: When the machine code view's APPLY button is clicked,
    '          add or update the info in the database
    '
    ' ************************************************************************
    Protected Sub btnApplyMachineCodes_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnApplyMachineCodes.Click
        Try
            If ((txtMachineCodeCode.Text.Trim.Length = 0) Or (txtMachineCodeDescription.Text.Trim.Length = 0)) Then
                MessageBox("You must specify a Code and Description before you can save.", "USER ERROR")
                Exit Sub
            End If

            ' If the ID isn't 0 then it is the key to the entry we're updating
            Dim mCode As String = Session("SELECTED_MACHINE_CODE").ToString()
            If (mCode.Length <> 0) Then
                Dim updateQuery As tblMACHINE = _
                    (From mCodes In db.tblMACHINEs() _
                     Select mCodes _
                     Where mCodes.MACHINE_CODE = mCode).FirstOrDefault()

                If updateQuery Is Nothing Then
                    MessageBox("Unable to update record #" + Session("SELECTED_MACHINE_CODE").ToString(), "SYSTEM ERROR")
                Else
                    ' The index can not change so updating the machine code means delete the existing record & add a new record
                    db.tblMACHINEs().DeleteOnSubmit(updateQuery)
                    Dim newCode As New tblMACHINE With { _
                        .MACHINE_CODE = txtMachineCodeCode.Text.Trim, _
                        .DESCRIPTION = txtMachineCodeDescription.Text.Trim}
                    db.tblMACHINEs().InsertOnSubmit(newCode)
                    db.SubmitChanges()
                End If

                ClearMachineCodesFields()
            Else
                ' Otherwise, we're adding a new record
                Dim newCode As New tblMACHINE With { _
                    .MACHINE_CODE = txtMachineCodeCode.Text.Trim, _
                    .DESCRIPTION = txtMachineCodeDescription.Text.Trim}
                db.tblMACHINEs().InsertOnSubmit(newCode)
                db.SubmitChanges()
            End If

            btnApplyMachineCodes.Text = "Add"
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnClearMachineCodes_Click
    '
    ' Purpose: When the machine code view's CLEAR button is clicked,
    '          blank the detail widgets
    '
    ' ************************************************************************
    Protected Sub btnClearMachineCodes_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnClearMachineCodes.Click
        Try
            ClearMachineCodesFields()
            btnApplyMachineCodes.Text = "Add"
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ClearMachineCodesFields
    '
    ' Purpose: Helper method to blank the machine code view's detail widgets
    '
    ' ************************************************************************
    Private Sub ClearMachineCodesFields()
        lstMachineCodes.ClearSelection()
        txtMachineCodeCode.Text = ""
        txtMachineCodeDescription.Text = ""
        Session("SELECTED_MACHINE_CODE") = ""
        Session("SELECTED_MACHINE") = ""

        udpMachineCodesBottom.Visible = False
    End Sub

#End Region

#Region "Employee Maintenance Methods"

    ' ************************************************************************
    '           The methods below are for EMPLOYEE MAINTENANCE 
    ' ************************************************************************



    ' ************************************************************************
    '    Name: viewEmployees_PreRender
    '
    ' Purpose: Update the list of employees before the view is displayed
    '
    ' ************************************************************************
    Protected Sub viewEmployees_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles viewEmployees.PreRender
        Try
            ' Query the database to retrieve the employee names and bind them to the widget
            Dim populateQuery As List(Of tblEMPLOYEE) = _
                (From emps In db.tblEMPLOYEEs() _
                 Select emps _
                 Order By emps.LAST_NAME, emps.FIRST_NAME).ToList()
            grdEmployees.DataSource = populateQuery
            grdEmployees.DataBind()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnNewEmployees_Click
    '
    ' Purpose: When the employee view's NEW button is clicked,
    '          show the detail widgets
    '
    ' ************************************************************************
    Protected Sub btnNewEmployees_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnNewEmployees.Click
        Try
            ClearEmployeesFields()
            btnApplyEmployees.Text = "Add"

            ' Retrieve the available supervisors
            Dim superQuery As List(Of String) = _
                (From supers In db.tblEMPLOYEEs() _
                 Where supers.IS_SUPERVISOR = "Y" _
                 Order By supers.LAST_NAME, supers.FIRST_NAME _
                 Select FULL_NAME = supers.LAST_NAME + ", " + supers.FIRST_NAME).ToList()
            superQuery.Insert(0, MAKE_A_SELECTION)
            ddlEmployeeSupervisor.DataSource = superQuery
            ddlEmployeeSupervisor.DataBind()

            txtEmployeeNumber.Focus()
            udpEmployeesBottom.Visible = True
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnEditEmployees_Click
    '
    ' Purpose: When the employee view's EDIT button is clicked, update
    '          the detail widgets with all the info for the selected entry
    '
    ' ************************************************************************
    Protected Sub grdEmployees_RowEditing(sender As Object, e As GridViewEditEventArgs) Handles grdEmployees.RowEditing
        Try
            ' Get the original list of records
            Dim editQuery As List(Of tblEMPLOYEE) = _
                (From emps In db.tblEMPLOYEEs() _
                 Select emps _
                 Order By emps.LAST_NAME, emps.FIRST_NAME).ToList()

            ' Display the record's information in the detail widgets
            Session("SELECTED_EMPLOYEE_ID") = editQuery(e.NewEditIndex).EMPLOYEE_ID
            txtEmployeeNumber.Text = editQuery(e.NewEditIndex).EMPLOYEE_NUMBER
            txtEmployeeFirst.Text = editQuery(e.NewEditIndex).FIRST_NAME
            txtEmployeeLast.Text = editQuery(e.NewEditIndex).LAST_NAME
            chbEmployeeActive.Checked = editQuery(e.NewEditIndex).ACTIVE_STATUS.Equals("A")
            txtEmployeeComments.Text = editQuery(e.NewEditIndex).COMMENTS
            chbEmployeeTrainer.Checked = editQuery(e.NewEditIndex).TRAINER.Equals("Y")
            txtEmployeePassword.Text = editQuery(e.NewEditIndex).PASSWORD
            If (editQuery(e.NewEditIndex).IS_SUPERVISOR Is Nothing) Then
                chbEmployeeIsSupervisor.Checked = False
            Else
                chbEmployeeIsSupervisor.Checked = editQuery(e.NewEditIndex).IS_SUPERVISOR.Equals("Y")
            End If
            txtSupervisorPassword.Text = editQuery(e.NewEditIndex).SUPERVISOR_PASSWORD
            txtEmployeeEmail.Text = editQuery(e.NewEditIndex).EMAIL

            ' Retrieve the available supervisors
            Dim superQuery As List(Of String) = _
                (From supers In db.tblEMPLOYEEs() _
                 Where supers.IS_SUPERVISOR = "Y" _
                 Order By supers.LAST_NAME, supers.FIRST_NAME _
                 Select FULL_NAME = supers.LAST_NAME + ", " + supers.FIRST_NAME).ToList()
            superQuery.Insert(0, MAKE_A_SELECTION)
            ddlEmployeeSupervisor.DataSource = superQuery
            ddlEmployeeSupervisor.DataBind()

            ' Retrieve this employee's supervisor
            Dim superQuery2 As String = _
                (From supers In db.tblEMPLOYEEs() _
                 Where supers.EMPLOYEE_ID = editQuery(e.NewEditIndex).SUPERVISOR_ID _
                 Select FULL_NAME = supers.LAST_NAME + ", " + supers.FIRST_NAME).FirstOrDefault()
            If (Not (superQuery2 Is Nothing) AndAlso superQuery.Contains(superQuery2)) Then
                ddlEmployeeSupervisor.SelectedValue = superQuery2
            End If

            txtEmployeeNumber.Focus()
            btnApplyEmployees.Text = "Update"
            udpEmployeesBottom.Visible = True
            e.Cancel = True
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnDeleteEmployees_Click
    '
    ' Purpose: When the employee view's DELETE button is clicked,
    '          remove the info for the selected entry from the database
    '
    ' ************************************************************************
    Protected Sub grdEmployees_RowDeleting(sender As Object, e As GridViewDeleteEventArgs) Handles grdEmployees.RowDeleting
        Try
            ' Get the original list of records
            Dim deleteQuery As List(Of tblEMPLOYEE) = _
                (From emps In db.tblEMPLOYEEs() _
                 Select emps _
                 Order By emps.LAST_NAME, emps.FIRST_NAME).ToList()

            Dim delTqQuery As List(Of tblTRAINER_QUALIFICATION) = _
                (From tq In db.tblTRAINER_QUALIFICATIONs() _
                 Select tq _
                 Where tq.EMPLOYEE_ID = deleteQuery(e.RowIndex).EMPLOYEE_ID).ToList()

            Dim delEqQuery As List(Of tblEMPLOYEE_QUALIFICATION) = _
                (From eq In db.tblEMPLOYEE_QUALIFICATIONs() _
                 Select eq _
                 Where eq.EMPLOYEE_ID = deleteQuery(e.RowIndex).EMPLOYEE_ID).ToList()

            Dim delEq2Query As List(Of tblEMPLOYEES_QUALIFIED) = _
                (From eq In db.tblEMPLOYEES_QUALIFIEDs() _
                 Select eq _
                 Where eq.EMPLOYEE_ID = deleteQuery(e.RowIndex).EMPLOYEE_ID).ToList()

            ' Delete the selected records from the database only if there are no training records
            If ((delTqQuery.Count = 0) And (delEqQuery.Count = 0) And (delEq2Query.Count = 0)) Then
                db.tblEMPLOYEEs().DeleteOnSubmit(deleteQuery(e.RowIndex))
            Else
                deleteQuery(e.RowIndex).ACTIVE_STATUS = "I"
                MessageBox(deleteQuery(e.RowIndex).LAST_NAME & " " & deleteQuery(e.RowIndex).FIRST_NAME & _
                           " has been trained.  Canceling DELETE and changing employee to 'inactive'.", "DELETE CANCELED")
            End If
            db.SubmitChanges()

            ' Clear the selection and the detail widgets
            ClearEmployeesFields()
            btnApplyEmployees.Text = "Add"
            e.Cancel = True
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnApplyEmployees_Click
    '
    ' Purpose: When the employee view's APPLY button is clicked,
    '          add or update the info in the database
    '
    ' ************************************************************************
    Protected Sub btnApplyEmployees_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnApplyEmployees.Click
        Try
            If ((txtEmployeeNumber.Text.Trim.Length = 0) Or _
                (txtEmployeeFirst.Text.Trim.Length = 0) Or _
                (txtEmployeeLast.Text.Trim.Length = 0)) Then
                MessageBox("You must specify an Employee Number, First Name, and Last Name before you can save.", "USER ERROR")
                Exit Sub
            End If

            If ((chbEmployeeTrainer.Checked = True) And _
                (txtEmployeePassword.Text.Trim.Length = 0)) Then
                MessageBox("You must specify the trainer password before you can save.", "USER ERROR")
                Exit Sub
            End If

            If ((chbEmployeeIsSupervisor.Checked = True) And _
                ((txtSupervisorPassword.Text.Trim.Length = 0) Or (txtEmployeeEmail.Text.Trim.Length = 0))) Then
                MessageBox("You must specify the supervisor password and e-mail before you can save.", "USER ERROR")
                Exit Sub
            End If

            If (txtEmployeeEmail.Text.Trim.Contains("@") = True) Then
                MessageBox("The supervisor e-mail should only contain the person's ID, it should not include '@haartz.com'.", "USER ERROR")
                Exit Sub
            End If

            ' Determine if the employee number is unique
            Dim newEmpNum As Integer = Integer.Parse(txtEmployeeNumber.Text)
            Dim empNumStrings As List(Of String) = _
                (From emps In db.tblEMPLOYEEs() _
                 Select emps.EMPLOYEE_NUMBER).ToList()
            Dim empNumInts As New List(Of Integer)
            For Each s As String In empNumStrings
                Dim i As Integer = Integer.Parse(s)
                empNumInts.Add(i)
            Next
            Dim numRecords As Integer = (From temp In empNumInts Where temp.Equals(newEmpNum) Select temp).Count()
            Dim empIndex As Integer = Integer.Parse(Session("SELECTED_EMPLOYEE_ID").ToString())
            If (numRecords > 0) Then
                ' If we're adding a new record, but the employee number already exists, show an error
                If (empIndex = 0) Then
                    MessageBox("The employee number '" & txtEmployeeNumber.Text & "' is already assigned to " & numRecords & " other employee.", "USER ERROR")
                    Exit Sub
                Else
                    ' If we're updating a record, and the employee number occurs more than once, show an error
                    If (numRecords > 1) Then
                        MessageBox("The employee number '" & txtEmployeeNumber.Text & "' is already assigned to " & numRecords & " other employees.", "USER ERROR")
                        Exit Sub
                    Else
                        ' If we're updating a record, and the employee number occurs on a different employee, show an error
                        Dim dupNumber As tblEMPLOYEE = _
                            (From emps In db.tblEMPLOYEEs() _
                             Select emps _
                             Where emps.EMPLOYEE_ID = empIndex).FirstOrDefault()
                        Dim empNum As Integer = Integer.Parse(dupNumber.EMPLOYEE_NUMBER)
                        If (newEmpNum <> empNum) Then
                            MessageBox("The employee number '" & txtEmployeeNumber.Text & "' is already assigned to " & numRecords & " other employee.", "USER ERROR")
                            Exit Sub
                        End If
                    End If
                End If
            End If

            ' Lookup the supervisor's employee id
            Dim supervisorID As Integer = 0
            If (ddlEmployeeSupervisor.SelectedIndex > 0) Then
                Dim pos As Integer = ddlEmployeeSupervisor.SelectedValue.IndexOf(",")
                Dim lastName As String = ddlEmployeeSupervisor.SelectedValue.Substring(0, pos)
                Dim firstName As String = ddlEmployeeSupervisor.SelectedValue.Substring(pos + 2)

                Dim empQuery As Integer = _
                    (From emps In db.tblEMPLOYEEs() _
                     Where emps.FIRST_NAME = firstName And emps.LAST_NAME = lastName _
                     Select emps.EMPLOYEE_ID).FirstOrDefault()
                supervisorID = empQuery
            End If

            ' Convert the check box to a string
            Dim activeStatus As String
            If (chbEmployeeActive.Checked = True) Then
                activeStatus = "A"
            Else
                activeStatus = "I"
            End If
            Dim isaTrainer As String
            If (chbEmployeeTrainer.Checked = True) Then
                isaTrainer = "Y"
            Else
                isaTrainer = "N"
            End If
            Dim isaSupervisor As String
            If (chbEmployeeIsSupervisor.Checked = True) Then
                isaSupervisor = "Y"
            Else
                isaSupervisor = "N"
            End If

            ' If the textfield isn't 0 it is the key to the entry we're updating
            Dim eCode As Integer = Integer.Parse(Session("SELECTED_EMPLOYEE_ID").ToString())

            ' TPL - 24-FEB-2016 - Confirm that there isn't already a person with this name
            Dim dupName As tblEMPLOYEE = _
                (From emps In db.tblEMPLOYEEs() _
                 Select emps _
                 Where emps.EMPLOYEE_ID <> eCode And _
                       emps.FIRST_NAME = txtEmployeeFirst.Text.Trim.ToUpper() And _
                       emps.LAST_NAME = txtEmployeeLast.Text.Trim.ToUpper()).FirstOrDefault()
            If (dupName IsNot Nothing) Then
                MessageBox("There is already an employee named '" & dupName.FIRST_NAME & _
                           " " & dupName.LAST_NAME & "' in the system.", "USER ERROR")
                Exit Sub
            End If

            If (eCode <> 0) Then
                Dim updateQuery As tblEMPLOYEE = _
                    (From emps In db.tblEMPLOYEEs() _
                     Select emps _
                     Where emps.EMPLOYEE_ID = eCode).FirstOrDefault()

                If updateQuery Is Nothing Then
                    MessageBox("Unable to update record #" + eCode.ToString(), "SYSTEM ERROR")
                Else
                    updateQuery.EMPLOYEE_NUMBER = txtEmployeeNumber.Text.Trim
                    updateQuery.FIRST_NAME = txtEmployeeFirst.Text.Trim.ToUpper()
                    updateQuery.LAST_NAME = txtEmployeeLast.Text.Trim.ToUpper()
                    updateQuery.ACTIVE_STATUS = activeStatus
                    updateQuery.COMMENTS = txtEmployeeComments.Text.Trim
                    updateQuery.TRAINER = isaTrainer
                    updateQuery.PASSWORD = txtEmployeePassword.Text.Trim
                    updateQuery.IS_SUPERVISOR = isaSupervisor
                    updateQuery.SUPERVISOR_PASSWORD = txtSupervisorPassword.Text.Trim
                    updateQuery.EMAIL = txtEmployeeEmail.Text.Trim
                    updateQuery.SUPERVISOR_ID = supervisorID
                    db.SubmitChanges()
                End If

                ClearEmployeesFields()
            Else
                ' Otherwise, we're adding a new record
                Dim number As Integer = GetNextEmployeeNumber()
                Dim newCode As New tblEMPLOYEE With { _
                    .EMPLOYEE_ID = number, _
                    .EMPLOYEE_NUMBER = txtEmployeeNumber.Text.Trim, _
                    .FIRST_NAME = txtEmployeeFirst.Text.Trim.ToUpper(), _
                    .LAST_NAME = txtEmployeeLast.Text.Trim.ToUpper(), _
                    .ACTIVE_STATUS = activeStatus, _
                    .COMMENTS = txtEmployeeComments.Text.Trim, _
                    .TRAINER = isaTrainer, _
                    .PASSWORD = txtEmployeePassword.Text.Trim, _
                    .IS_SUPERVISOR = isaSupervisor, _
                    .SUPERVISOR_PASSWORD = txtSupervisorPassword.Text.Trim, _
                    .EMAIL = txtEmployeeEmail.Text.Trim, _
                    .SUPERVISOR_ID = supervisorID}
                db.tblEMPLOYEEs().InsertOnSubmit(newCode)
                db.SubmitChanges()
            End If

            btnApplyEmployees.Text = "Add"
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnClearEmployees_Click
    '
    ' Purpose: When the employee view's CLEAR button is clicked,
    '          blank the detail widgets
    '
    ' ************************************************************************
    Protected Sub btnClearEmployees_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnClearEmployees.Click
        Try
            ClearEmployeesFields()
            btnApplyEmployees.Text = "Add"
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ClearEmployeesFields
    '
    ' Purpose: Helper method to blank the employee view's detail widgets
    '
    ' ************************************************************************
    Private Sub ClearEmployeesFields()
        grdEmployees.SelectedIndex = -1
        Session("SELECTED_EMPLOYEE_ID") = 0
        Session("SELECTED_EMPLOYEE") = ""
        txtEmployeeNumber.Text = ""
        txtEmployeeFirst.Text = ""
        txtEmployeeLast.Text = ""
        chbEmployeeActive.Checked = False
        txtEmployeeComments.Text = ""
        chbEmployeeTrainer.Checked = False
        txtEmployeePassword.Text = ""
        chbEmployeeIsSupervisor.Checked = False
        txtSupervisorPassword.Text = ""
        txtEmployeeEmail.Text = ""
        ddlEmployeeSupervisor.SelectedIndex = -1

        udpEmployeesBottom.Visible = False
    End Sub

    ' ************************************************************************
    '    Name: GetNextEmployeeNumber
    '
    ' Purpose: Generate a unique ID for a new entry in the EMPLOYEES table
    '
    ' ************************************************************************
    Private Function GetNextEmployeeNumber() As Integer
        Dim rtnValue As Integer = 0

        ' Lookup the last employee number, store it as the return value,
        ' increment the employee number, and update the database
        Dim controlQuery As tblOJT_CONTROL = _
            (From controls In db.tblOJT_CONTROLs() _
             Select controls _
             Where controls.RECORD_ID = "A").FirstOrDefault()

        If controlQuery Is Nothing Then
            MessageBox("Unable to retrieve Employee control number.", "SYSTEM ERROR")
        Else
            rtnValue = controlQuery.LAST_EMPLOYEE.Value
            controlQuery.LAST_EMPLOYEE = rtnValue + 1
            db.SubmitChanges()
        End If

        Return rtnValue
    End Function

#End Region

#Region "Category Maintenance Methods"

    ' ************************************************************************
    '           The methods below are for CATEGORY MAINTENANCE 
    ' ************************************************************************



    ' ************************************************************************
    '    Name: viewCategories_PreRender
    '
    ' Purpose: Update the list of categories before the view is displayed
    '
    ' ************************************************************************
    Protected Sub viewCategories_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles viewCategories.PreRender
        Try
            ' Query the database to retrieve the categories and bind them to the widget
            Dim populateQuery As List(Of String) = _
                (From cats In db.tblJOB_TITLEs() _
                 Select cats.JOB_TITLE _
                 Order By JOB_TITLE).ToList()
            lstCategories.DataSource = populateQuery
            lstCategories.DataBind()
            lstCategories.SelectedIndex = populateQuery.IndexOf(Session("SELECTED_CATEGORY").ToString())
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnNewCategories_Click
    '
    ' Purpose: When the category view's NEW button is clicked,
    '          show the detail widgets
    '
    ' ************************************************************************
    Protected Sub btnNewCategories_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnNewCategories.Click
        Try
            ClearCategoriesFields()
            btnApplyCategories.Text = "Add"
            txtCategoriesTitle.Focus()
            udpCategoriesBottom.Visible = True
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnEditCategories_Click
    '
    ' Purpose: When the category view's EDIT button is clicked, update
    '          the detail widgets with all the info for the selected entry
    '
    ' ************************************************************************
    Protected Sub btnEditCategories_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnEditCategories.Click
        Try
            If (lstCategories.SelectedIndex = -1) Then
                MessageBox("You must select an entry in the list before you can edit.", "USER ERROR")
                Exit Sub
            End If

            ' Retrieve the database record for the selected entry
            Dim editQuery As tblJOB_TITLE = _
                (From cats In db.tblJOB_TITLEs() _
                 Select cats _
                 Where cats.JOB_TITLE = lstCategories.SelectedValue).FirstOrDefault()

            ' Display the record's information in the detail widgets
            Session("SELECTED_CATEGORY_ID") = editQuery.JOB_TITLE_NUMBER
            Session("SELECTED_CATEGORY") = lstCategories.SelectedValue
            txtCategoriesTitle.Text = editQuery.JOB_TITLE
            ddlCategoriesType.SelectedValue = editQuery.JOB_TITLE_TYPE
            txtCategoriesDescription.Text = editQuery.DESCRIPTION
            txtCategoriesTitle.Focus()
            btnApplyCategories.Text = "Update"
            udpCategoriesBottom.Visible = True
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnDeleteCategories_Click
    '
    ' Purpose: When the category view's DELETE button is clicked,
    '          remove the info for the selected entry from the database
    '
    ' ************************************************************************
    Protected Sub btnDeleteCategories_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnDeleteCategories.Click
        Try
            If (lstCategories.SelectedIndex = -1) Then
                MessageBox("You must select an entry in the list before you can delete.", "USER ERROR")
                Exit Sub
            End If

            ' Retrieve the database record(s) to be deleted
            Dim delJtQuery As tblJOB_TITLE = _
                (From cats In db.tblJOB_TITLEs() _
                 Select cats _
                 Where cats.JOB_TITLE = lstCategories.SelectedValue).FirstOrDefault()

            Dim delTqQuery As List(Of tblTRAINER_QUALIFICATION) = _
                (From tq In db.tblTRAINER_QUALIFICATIONs() _
                 Select tq _
                 Where tq.JOB_TITLE_NUMBER = delJtQuery.JOB_TITLE_NUMBER).ToList()

            Dim delEqQuery As List(Of tblEMPLOYEE_QUALIFICATION) = _
                (From eq In db.tblEMPLOYEE_QUALIFICATIONs() _
                 Select eq _
                 Where eq.JOB_TITLE_NUMBER = delJtQuery.JOB_TITLE_NUMBER).ToList()

            Dim delEq2Query As List(Of tblEMPLOYEES_QUALIFIED) = _
                (From eq In db.tblEMPLOYEES_QUALIFIEDs() _
                 Select eq _
                 Where eq.JOB_TITLE_NUMBER = delJtQuery.JOB_TITLE_NUMBER).ToList()

            Dim delJmmQuery As List(Of tblJOB_MACHINE_MASTER) = _
                (From jmm In db.tblJOB_MACHINE_MASTERs() _
                 Select jmm _
                 Where jmm.JOB_TITLE_NUMBER = delJtQuery.JOB_TITLE_NUMBER).ToList()

            Dim delJtaQuery As List(Of tblJOB_TITLE_APPROVAL) = _
                (From jta In db.tblJOB_TITLE_APPROVALs() _
                 Select jta _
                 Where jta.JOB_TITLE_NUMBER = delJtQuery.JOB_TITLE_NUMBER).ToList()

            ' Delete the selected records from the database
            db.tblJOB_TITLEs().DeleteOnSubmit(delJtQuery)
            db.tblTRAINER_QUALIFICATIONs().DeleteAllOnSubmit(delTqQuery)
            db.tblEMPLOYEE_QUALIFICATIONs().DeleteAllOnSubmit(delEqQuery)
            db.tblEMPLOYEES_QUALIFIEDs().DeleteAllOnSubmit(delEq2Query)
            db.tblJOB_MACHINE_MASTERs().DeleteAllOnSubmit(delJmmQuery)
            db.tblJOB_TITLE_APPROVALs().DeleteAllOnSubmit(delJtaQuery)
            db.SubmitChanges()

            ' Clear the selection and the detail widgets
            lstCategories.ClearSelection()
            ClearCategoriesFields()
            btnApplyCategories.Text = "Add"
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnApplyCategories_Click
    '
    ' Purpose: When the category view's APPLY button is clicked,
    '          add or update the info in the database
    '
    ' ************************************************************************
    Protected Sub btnApplyCategories_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnApplyCategories.Click
        Try
            If ((txtCategoriesTitle.Text.Trim.Length = 0) Or (txtCategoriesDescription.Text.Trim.Length = 0)) Then
                MessageBox("You must specify a Title and Description before you can save.", "USER ERROR")
                Exit Sub
            End If

            ' If the ID isn't 0 then it is the key to the entry we're updating
            Dim cID As Integer = Integer.Parse(Session("SELECTED_CATEGORY_ID").ToString())
            If (cID <> 0) Then
                Dim updateQuery As tblJOB_TITLE = _
                    (From cats In db.tblJOB_TITLEs() _
                     Select cats _
                     Where cats.JOB_TITLE_NUMBER = cID).FirstOrDefault()

                If updateQuery Is Nothing Then
                    MessageBox("Unable to update record #" + cID.ToString(), "SYSTEM ERROR")
                Else
                    updateQuery.DESCRIPTION = txtCategoriesDescription.Text.Trim
                    updateQuery.JOB_TITLE = txtCategoriesTitle.Text.Trim
                    updateQuery.JOB_TITLE_TYPE = ddlCategoriesType.SelectedValue
                    db.SubmitChanges()
                End If

                ClearCategoriesFields()
            Else
                ' Otherwise, we're adding a new record
                Dim number As Integer = GetNextCategoriesNumber()
                Dim newCode As New tblJOB_TITLE With { _
                    .JOB_TITLE_NUMBER = number, _
                    .JOB_TITLE = txtCategoriesTitle.Text.Trim, _
                    .JOB_TITLE_TYPE = ddlCategoriesType.SelectedValue, _
                    .DESCRIPTION = txtCategoriesDescription.Text.Trim}
                db.tblJOB_TITLEs().InsertOnSubmit(newCode)
                db.SubmitChanges()
            End If

            btnApplyCategories.Text = "Add"
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnClearCategories_Click
    '
    ' Purpose: When the category view's CLEAR button is clicked,
    '          blank the detail widgets
    '
    ' ************************************************************************
    Protected Sub btnClearCategories_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnClearCategories.Click
        Try
            ClearCategoriesFields()
            btnApplyCategories.Text = "Add"
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ClearCategoriesFields
    '
    ' Purpose: Helper method to blank the category view's detail widgets
    '
    ' ************************************************************************
    Private Sub ClearCategoriesFields()
        lstCategories.ClearSelection()
        txtCategoriesTitle.Text = ""
        ddlCategoriesType.SelectedValue = "O"
        txtCategoriesDescription.Text = ""
        Session("SELECTED_CATEGORY_ID") = 0
        Session("SELECTED_CATEGORY") = ""
        udpCategoriesBottom.Visible = False
    End Sub

    ' ************************************************************************
    '    Name: GetNextCategoriesNumber
    '
    ' Purpose: Generate a unique ID for a new entry in the JOB_TITLES table
    '
    ' ************************************************************************
    Private Function GetNextCategoriesNumber() As Integer
        Dim rtnValue As Integer = 0

        ' Lookup the last training code number, store it as the return value,
        ' increment the training code number, and update the database
        Dim controlQuery As tblOJT_CONTROL = _
            (From controls In db.tblOJT_CONTROLs() _
             Select controls _
             Where controls.RECORD_ID = "A").FirstOrDefault()

        If controlQuery Is Nothing Then
            MessageBox("Unable to retrieve Category Code control number.", "SYSTEM ERROR")
        Else
            rtnValue = controlQuery.LAST_CATEGORY.Value
            controlQuery.LAST_CATEGORY = rtnValue + 1
            db.SubmitChanges()
        End If

        Return rtnValue
    End Function

#End Region

#Region "Approval Maintenance Methods"

    ' ************************************************************************
    '           The methods below are for APPROVAL MAINTENANCE 
    ' ************************************************************************



    ' ************************************************************************
    '    Name: viewApprovals_PreRender
    '
    ' Purpose: Update the lists of jobs, machines, and categories before the view is displayed
    '
    ' ************************************************************************
    Protected Sub viewApprovals_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles viewApprovals.PreRender
        Try
            ' Query the database to retrieve the categories and bind them to the widget
            Dim categoryQuery As List(Of String) = _
                (From cats In db.tblJOB_TITLEs() _
                 Select cats.JOB_TITLE _
                 Order By JOB_TITLE).ToList()
            categoryQuery.Insert(0, MAKE_A_SELECTION)
            ddlApprovalCategory.DataSource = categoryQuery
            ddlApprovalCategory.DataBind()
            ddlApprovalCategory.SelectedIndex = categoryQuery.IndexOf(Session("SELECTED_CATEGORY").ToString())

            ' Query the database to retrieve the job titles and bind them to the widget
            Dim jobTitleQuery As List(Of String) = _
                (From titles In db.tblMASTER_JOB_TITLEs() _
                 Select titles.MASTER_JOB_TITLE _
                 Order By MASTER_JOB_TITLE).ToList()
            jobTitleQuery.Insert(0, MAKE_A_SELECTION)
            ddlApprovalJobTitle.DataSource = jobTitleQuery
            ddlApprovalJobTitle.DataBind()
            ddlApprovalJobTitle.SelectedIndex = jobTitleQuery.IndexOf(Session("SELECTED_JOB").ToString())

            ' The list of machines depends upon which job is selected
            Dim machQuery As List(Of String) = Nothing
            Dim jNum As Integer = Integer.Parse(Session("SELECTED_JOB_NUMBER").ToString())
            If (jNum <> 0) Then
                ' Query the database to retrieve the machine codes and bind them to the widget
                machQuery = _
                    (From machines In db.tblMACHINEs() _
                     Join jmm In db.tblJOB_MACHINE_MASTERs() _
                     On machines.MACHINE_CODE Equals jmm.MACHINE_CODE _
                     Order By machines.MACHINE_CODE _
                     Where jmm.MASTER_JOB_NUMBER = jNum _
                     Select machines.MACHINE_CODE Distinct).ToList()
                machQuery.Sort()
                machQuery.Insert(0, MAKE_A_SELECTION)
                ddlApprovalMachine.DataSource = machQuery
            Else
                machQuery = New List(Of String)
                machQuery.Insert(0, MAKE_A_SELECTION)
                ddlApprovalMachine.DataSource = machQuery
            End If
            ddlApprovalMachine.DataBind()
            ddlApprovalMachine.SelectedIndex = machQuery.IndexOf(Session("SELECTED_MACHINE").ToString())
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ddlApprovalJobTitle_SelectedIndexChanged
    '
    ' Purpose: When a job is selected, store the selection - screen will
    '          update automatically to repopulate machine list for the
    '          selected job
    '
    ' ************************************************************************
    Protected Sub ddlApprovalJobTitle_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlApprovalJobTitle.SelectedIndexChanged
        Try
            ' Query to retrieve the unique index
            Dim jobTitleQuery As Integer = _
                (From titles In db.tblMASTER_JOB_TITLEs() _
                 Where titles.MASTER_JOB_TITLE = ddlApprovalJobTitle.SelectedValue() _
                 Select titles.MASTER_JOB_NUMBER).FirstOrDefault()

            Session("SELECTED_JOB_NUMBER") = jobTitleQuery
            Session("SELECTED_JOB") = ddlApprovalJobTitle.SelectedValue

            ' Reset widgets that rely on this value
            ddlApprovalMachine.SelectedIndex = 0
            Session("SELECTED_MACHINE") = ""
            gridApproval.DataSource = Nothing
            gridApproval.DataBind()
            txtApprovalQcfNumber.Text = ""
            txtApprovalReviewedBy.Text = ""
            txtApprovalReviewDate.Text = ""

            udpApprovalsBottom.Visible = False
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ddlApprovalMachine_SelectedIndexChanged
    '
    ' Purpose: When a machine is selected, store the selection
    '
    ' ************************************************************************
    Protected Sub ddlApprovalMachine_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlApprovalMachine.SelectedIndexChanged
        Try
            Session("SELECTED_MACHINE") = ddlApprovalMachine.SelectedValue
            Session("SELECTED_MACHINE_CODE") = ddlApprovalMachine.SelectedValue

            ' Reset widgets that rely on this value
            gridApproval.DataSource = Nothing
            gridApproval.DataBind()
            txtApprovalQcfNumber.Text = ""
            txtApprovalReviewedBy.Text = ""
            txtApprovalReviewDate.Text = ""

            udpApprovalsBottom.Visible = False
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ddlApprovalCategory_SelectedIndexChanged
    '
    ' Purpose: When a category is selected, store the selection
    '
    ' ************************************************************************
    Protected Sub ddlApprovalCategory_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlApprovalCategory.SelectedIndexChanged
        Try
            ' Query to retrieve the unique index
            Dim categoryQuery As Integer = _
                (From cats In db.tblJOB_TITLEs() _
                 Where cats.JOB_TITLE = ddlApprovalCategory.SelectedValue() _
                 Select cats.JOB_TITLE_NUMBER).FirstOrDefault()

            Session("SELECTED_CATEGORY_NUMBER") = categoryQuery
            Session("SELECTED_CATEGORY") = ddlApprovalCategory.SelectedValue

            ' Reset widgets that rely on this value
            gridApproval.DataSource = Nothing
            gridApproval.DataBind()
            txtApprovalQcfNumber.Text = ""
            txtApprovalReviewedBy.Text = ""
            txtApprovalReviewDate.Text = ""

            udpApprovalsBottom.Visible = False
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnApprovalLookup_Click
    '
    ' Purpose: When a the LOOKUP button is clicked fill in the details based
    '          on the selected entries
    '
    ' ************************************************************************
    Protected Sub btnApprovalLookup_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnApprovalLookup.Click
        Try
            If (ddlApprovalJobTitle.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlApprovalMachine.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlApprovalCategory.SelectedValue().Equals(MAKE_A_SELECTION)) Then
                MessageBox("You must select a job, a machine, and a category before you can proceed.", "USER ERROR")
                Exit Sub
            End If

            ' Query to find the Job information associated with the selected items and display it
            Dim jobNum As Integer = Integer.Parse(Session("SELECTED_JOB_NUMBER").ToString())
            Dim mCode As String = Session("SELECTED_MACHINE_CODE").ToString()
            Dim catNum As Integer = Integer.Parse(Session("SELECTED_CATEGORY_NUMBER").ToString())
            Dim newQuery As List(Of tblTRAINING_CODE) = _
                (From jmm In db.tblJOB_MACHINE_MASTERs() _
                 Join tCodes In db.tblTRAINING_CODEs() _
                 On tCodes.TRAINING_NUMBER Equals jmm.TRAINING_NUMBER _
                 Order By tCodes.TITLE _
                 Where jmm.MASTER_JOB_NUMBER = jobNum And _
                       jmm.MACHINE_CODE = mCode And _
                       jmm.JOB_TITLE_NUMBER = catNum _
                 Select tCodes).ToList()

            ' If there are no training codes for the selected Job, Machine, 
            ' and Category, then display a message
            If (newQuery.Count = 0) Then
                MessageBox("There is no training information available for the job, machine, and category specified.", "NO DATA")
                Exit Sub
            End If

            gridApproval.DataSource = newQuery
            gridApproval.DataBind()

            ' Query to see if this has already been approved and display the info if it exists
            Dim appQuery As tblJOB_TITLE_APPROVAL = _
                (From apps In db.tblJOB_TITLE_APPROVALs() _
                 Select apps _
                 Where apps.MASTER_JOB_NUMBER = jobNum And _
                       apps.MACHINE_CODE = mCode And _
                       apps.JOB_TITLE_NUMBER = catNum).FirstOrDefault()
            If (Not (appQuery Is Nothing)) Then
                txtApprovalQcfNumber.Text = appQuery.QCF_NUMBER
                txtApprovalReviewedBy.Text = appQuery.REVIEWED_BY
                Dim dt As DateTime
                dt = appQuery.DATE_REVIEWED.Value
                txtApprovalReviewDate.Text = dt.ToString("MM/dd/yyyy")
            End If

            udpApprovalsBottom.Visible = True
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnApprovalClear_Click
    '
    ' Purpose: When the approval view's CLEAR button is clicked,
    '          blank the detail widgets
    '
    ' ************************************************************************
    Protected Sub btnApprovalClear_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnApprovalClear.Click
        Try
            ClearApprovalFields()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnApprovalSubmit_Click
    '
    ' Purpose: When the approval view's SUBMIT button is clicked,
    '          add or update the info in the database
    '
    ' ************************************************************************
    Protected Sub btnApprovalSubmit_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnApprovalSubmit.Click
        Try
            If (ddlApprovalJobTitle.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlApprovalMachine.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlApprovalCategory.SelectedValue().Equals(MAKE_A_SELECTION)) Then
                MessageBox("You must select a job, a machine, and a category before you can proceed.", "USER ERROR")
                Exit Sub
            End If

            If (gridApproval.Rows.Count = 0) Then
                MessageBox("You must lookup the details before you can proceed.", "USER ERROR")
                Exit Sub
            End If

            If ((txtApprovalReviewedBy.Text.Trim.Length = 0) Or (txtApprovalQcfNumber.Text.Trim.Length = 0)) Then
                MessageBox("You must specify a Reviewer Name and QCF Number before you can save.", "USER ERROR")
                Exit Sub
            End If

            ' Look in the table to see if this is an UPDATE or an ADD
            Dim jobNum As Integer = Integer.Parse(Session("SELECTED_JOB_NUMBER").ToString())
            Dim mCode As String = Session("SELECTED_MACHINE_CODE").ToString()
            Dim catNum As Integer = Integer.Parse(Session("SELECTED_CATEGORY_NUMBER").ToString())
            Dim appQuery As tblJOB_TITLE_APPROVAL = _
                (From apps In db.tblJOB_TITLE_APPROVALs() _
                 Select apps _
                 Where apps.MASTER_JOB_NUMBER = jobNum And _
                       apps.MACHINE_CODE = mCode And _
                       apps.JOB_TITLE_NUMBER = catNum).FirstOrDefault()

            ' If there is no record add it
            If appQuery Is Nothing Then
                Dim newApproval As New tblJOB_TITLE_APPROVAL With { _
                    .MASTER_JOB_NUMBER = jobNum, _
                    .MACHINE_CODE = mCode, _
                    .JOB_TITLE_NUMBER = catNum, _
                    .QCF_NUMBER = txtApprovalQcfNumber.Text.Trim, _
                    .REVIEWED_BY = txtApprovalReviewedBy.Text.Trim, _
                    .DATE_REVIEWED = Now()}
                db.tblJOB_TITLE_APPROVALs().InsertOnSubmit(newApproval)
            Else
                ' If a record exists update it
                appQuery.QCF_NUMBER = txtApprovalQcfNumber.Text.Trim
                appQuery.REVIEWED_BY = txtApprovalReviewedBy.Text.Trim
                appQuery.DATE_REVIEWED = Now()
            End If
            db.SubmitChanges()

            ' Now that the database has been updated, refresh the display
            btnApprovalLookup_Click(sender, e)
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ClearApprovalFields
    '
    ' Purpose: Helper method to blank the approval view's detail widgets
    '
    ' ************************************************************************
    Private Sub ClearApprovalFields()
        txtApprovalQcfNumber.Text = ""
        txtApprovalReviewedBy.Text = ""
        txtApprovalReviewDate.Text = ""

        ddlApprovalJobTitle.SelectedIndex = -1
        ddlApprovalMachine.SelectedIndex = -1
        ddlApprovalCategory.SelectedIndex = -1

        gridApproval.DataSource = Nothing
        gridApproval.DataBind()

        Session("SELECTED_JOB_NUMBER") = 0
        Session("SELECTED_JOB") = ""
        Session("SELECTED_MACHINE") = ""
        Session("SELECTED_MACHINE_CODE") = ""
        Session("SELECTED_CATEGORY_NUMBER") = 0
        Session("SELECTED_CATEGORY") = ""

        udpApprovalsBottom.Visible = False
    End Sub

#End Region

#Region "Trainer Maintenance Methods"

    ' ************************************************************************
    '           The methods below are for TRAINER MAINTENANCE 
    ' ************************************************************************



    ' ************************************************************************
    '    Name: viewTrainer_PreRender
    '
    ' Purpose: Update the lists of jobs, machines, and categories before the view is displayed
    '
    ' ************************************************************************
    Protected Sub viewTrainer_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles viewTrainer.PreRender
        Try
            ' Query the database to retrieve the job titles and bind them to the widget
            Dim jobTitleQuery As List(Of String) = _
                (From titles In db.tblMASTER_JOB_TITLEs() _
                 Select titles.MASTER_JOB_TITLE _
                 Order By MASTER_JOB_TITLE).ToList()
            jobTitleQuery.Insert(0, MAKE_A_SELECTION)
            ddlTrainerJobTitle.DataSource = jobTitleQuery
            ddlTrainerJobTitle.DataBind()
            ddlTrainerJobTitle.SelectedIndex = jobTitleQuery.IndexOf(Session("SELECTED_JOB").ToString())

            ' Query the database to retrieve the employees and bind them to the widget
            Dim empQuery As List(Of String) = _
                (From emps In db.tblEMPLOYEEs() _
                 Where emps.TRAINER = "Y" _
                 Order By emps.LAST_NAME, emps.FIRST_NAME _
                 Select FULL_NAME = emps.LAST_NAME + ", " + emps.FIRST_NAME).ToList()
            empQuery.Insert(0, MAKE_A_SELECTION)
            ddlTrainerEmployee.DataSource = empQuery
            ddlTrainerEmployee.DataBind()
            ddlTrainerEmployee.SelectedIndex = empQuery.IndexOf(Session("SELECTED_EMPLOYEE").ToString())

            ' Query the database to retrieve the categories and bind them to the widget
            Dim categoryQuery As List(Of String) = _
                (From cats In db.tblJOB_TITLEs() _
                 Select cats.JOB_TITLE _
                 Order By JOB_TITLE).ToList()
            categoryQuery.Insert(0, MAKE_A_SELECTION)
            ddlTrainerCategory.DataSource = categoryQuery
            ddlTrainerCategory.DataBind()
            ddlTrainerCategory.SelectedIndex = categoryQuery.IndexOf(Session("SELECTED_CATEGORY").ToString())
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ddlTrainerJobTitle_SelectedIndexChanged
    '
    ' Purpose: When a job is selected, store the selection - screen will
    '          update automatically to repopulate machine list for the
    '          selected job
    '
    ' ************************************************************************
    Protected Sub ddlTrainerJobTitle_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlTrainerJobTitle.SelectedIndexChanged
        Try
            ' Query to retrieve the unique index
            Dim jobTitleQuery As Integer = _
                (From titles In db.tblMASTER_JOB_TITLEs() _
                 Where titles.MASTER_JOB_TITLE = ddlTrainerJobTitle.SelectedValue() _
                 Select titles.MASTER_JOB_NUMBER).FirstOrDefault()

            Session("SELECTED_JOB_NUMBER") = jobTitleQuery
            Session("SELECTED_JOB") = ddlTrainerJobTitle.SelectedValue

            ' Reset widgets that rely on this value
            lstTrainerSelectedMachines.DataSource = Nothing
            lstTrainerSelectedMachines.DataBind()
            lstTrainerSelectedMachines.Items.Clear()
            lstTrainerAvailableMachines.DataSource = Nothing
            lstTrainerAvailableMachines.DataBind()
            lstTrainerAvailableMachines.Items.Clear()

            udpTrainerBottom.Visible = False
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ddlTrainerEmployee_SelectedIndexChanged
    '
    ' Purpose: When a Employee is selected, store the selection
    '
    ' ************************************************************************
    Protected Sub ddlTrainerEmployee_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlTrainerEmployee.SelectedIndexChanged
        Try
            ' Query to retrieve the unique index
            If (Not ddlTrainerEmployee.SelectedValue.Equals(MAKE_A_SELECTION)) Then
                ' Break full name into its individual parts for use in the query
                Dim pos As Integer = ddlTrainerEmployee.SelectedValue.IndexOf(",")
                Dim lastName As String = ddlTrainerEmployee.SelectedValue.Substring(0, pos)
                Dim firstName As String = ddlTrainerEmployee.SelectedValue.Substring(pos + 2)

                Dim empQuery As Integer = _
                    (From emps In db.tblEMPLOYEEs() _
                     Where emps.FIRST_NAME = firstName And emps.LAST_NAME = lastName _
                     Select emps.EMPLOYEE_ID).FirstOrDefault()

                Session("SELECTED_EMPLOYEE_ID") = empQuery
                Session("SELECTED_EMPLOYEE") = ddlTrainerEmployee.SelectedValue
            Else
                Session("SELECTED_EMPLOYEE_ID") = 0
                Session("SELECTED_EMPLOYEE") = ""
            End If

            ' Reset widgets that rely on this value
            lstTrainerSelectedMachines.DataSource = Nothing
            lstTrainerSelectedMachines.DataBind()
            lstTrainerSelectedMachines.Items.Clear()
            lstTrainerAvailableMachines.DataSource = Nothing
            lstTrainerAvailableMachines.DataBind()
            lstTrainerAvailableMachines.Items.Clear()

            udpTrainerBottom.Visible = False
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ddlTrainerCategory_SelectedIndexChanged
    '
    ' Purpose: When a category is selected, store the selection
    '
    ' ************************************************************************
    Protected Sub ddlTrainerCategory_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlTrainerCategory.SelectedIndexChanged
        Try
            ' Query to retrieve the unique index
            Dim categoryQuery As Integer = _
                (From cats In db.tblJOB_TITLEs() _
                 Where cats.JOB_TITLE = ddlTrainerCategory.SelectedValue() _
                 Select cats.JOB_TITLE_NUMBER).FirstOrDefault()

            Session("SELECTED_CATEGORY_NUMBER") = categoryQuery
            Session("SELECTED_CATEGORY") = ddlTrainerCategory.SelectedValue

            ' Reset widgets that rely on this value
            lstTrainerSelectedMachines.DataSource = Nothing
            lstTrainerSelectedMachines.DataBind()
            lstTrainerSelectedMachines.Items.Clear()
            lstTrainerAvailableMachines.DataSource = Nothing
            lstTrainerAvailableMachines.DataBind()
            lstTrainerAvailableMachines.Items.Clear()

            udpTrainerBottom.Visible = False
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnTrainerLookup_Click
    '
    ' Purpose: When a the LOOKUP button is clicked fill in the details based
    '          on the selected entries
    '
    ' ************************************************************************
    Protected Sub btnTrainerLookup_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnTrainerLookup.Click
        Try
            If (ddlTrainerJobTitle.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlTrainerEmployee.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlTrainerCategory.SelectedValue().Equals(MAKE_A_SELECTION)) Then
                MessageBox("You must select an employee, a job, and a category before you can proceed.", "USER ERROR")
                Exit Sub
            End If

            PopulateTrainerDetails()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnTrainerAddSelected_Click
    '
    ' Purpose: When a the ADD SELECTED button is clicked add the selected 
    '          machine to the list of selected machines for the current
    '          employee, job, and category, and then update the lists
    '
    ' ************************************************************************
    Protected Sub btnTrainerAddSelected_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnTrainerAddSelected.Click
        Try
            If (lstTrainerAvailableMachines.SelectedIndex = -1) Then
                MessageBox("You must choose an available machine before you can proceed.", "USER ERROR")
                Exit Sub
            End If

            Dim jobNum As Integer = Integer.Parse(Session("SELECTED_JOB_NUMBER").ToString())
            Dim eCode As Integer = Integer.Parse(Session("SELECTED_EMPLOYEE_ID").ToString())
            Dim catNum As Integer = Integer.Parse(Session("SELECTED_CATEGORY_NUMBER").ToString())

            For Each aMach As ListItem In lstTrainerAvailableMachines.Items
                If (aMach.Selected = True) Then
                    Dim mCode As String = aMach.Value
                    Dim newMachine As New tblTRAINER_QUALIFICATION With { _
                            .MASTER_JOB_NUMBER = jobNum, _
                            .MACHINE_CODE = mCode, _
                            .JOB_TITLE_NUMBER = catNum, _
                            .EMPLOYEE_ID = eCode}
                    db.tblTRAINER_QUALIFICATIONs().InsertOnSubmit(newMachine)
                End If
            Next
            db.SubmitChanges()

            PopulateTrainerDetails()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnTrainerAddAll_Click
    '
    ' Purpose: When a the ADD ALL button is clicked add all available 
    '          machines to the list of selected machines for the current
    '          employee, job, and category, and then update the lists
    '
    ' ************************************************************************
    Protected Sub btnTrainerAddAll_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnTrainerAddAll.Click
        Try
            If (lstTrainerAvailableMachines.Items.Count = 0) Then
                MessageBox("There are no entries in the list of available machines.", "USER ERROR")
                Exit Sub
            End If

            Dim jobNum As Integer = Integer.Parse(Session("SELECTED_JOB_NUMBER").ToString())
            Dim eCode As Integer = Integer.Parse(Session("SELECTED_EMPLOYEE_ID").ToString())
            Dim catNum As Integer = Integer.Parse(Session("SELECTED_CATEGORY_NUMBER").ToString())

            For Each machine As ListItem In lstTrainerAvailableMachines.Items
                Dim mCode As String = machine.Value
                Dim newMachine As New tblTRAINER_QUALIFICATION With { _
                    .MASTER_JOB_NUMBER = jobNum, _
                    .MACHINE_CODE = mCode, _
                    .JOB_TITLE_NUMBER = catNum, _
                    .EMPLOYEE_ID = eCode}
                db.tblTRAINER_QUALIFICATIONs().InsertOnSubmit(newMachine)
            Next
            db.SubmitChanges()

            PopulateTrainerDetails()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnTrainerRemoveAll_Click
    '
    ' Purpose: When a the REMOVE ALL button is clicked remove all 
    '          machines from the list of selected machines for the current
    '          employee, job, and category, and then update the lists
    '
    ' ************************************************************************
    Protected Sub btnTrainerRemoveAll_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnTrainerRemoveAll.Click
        Try
            If (lstTrainerSelectedMachines.Items.Count = 0) Then
                MessageBox("There are no entries in the list of selected machines.", "USER ERROR")
                Exit Sub
            End If

            Dim jobNum As Integer = Integer.Parse(Session("SELECTED_JOB_NUMBER").ToString())
            Dim eCode As Integer = Integer.Parse(Session("SELECTED_EMPLOYEE_ID").ToString())
            Dim catNum As Integer = Integer.Parse(Session("SELECTED_CATEGORY_NUMBER").ToString())

            For Each machine As ListItem In lstTrainerSelectedMachines.Items
                Dim mCode As String = machine.Value
                Dim delQuery As tblTRAINER_QUALIFICATION = _
                    (From tq In db.tblTRAINER_QUALIFICATIONs() _
                     Select tq _
                     Where tq.EMPLOYEE_ID = eCode And _
                           tq.JOB_TITLE_NUMBER = catNum And _
                           tq.MASTER_JOB_NUMBER = jobNum And _
                           tq.MACHINE_CODE = mCode).FirstOrDefault()
                db.tblTRAINER_QUALIFICATIONs().DeleteOnSubmit(delQuery)
            Next
            db.SubmitChanges()

            PopulateTrainerDetails()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnTrainerRemoveSelected_Click
    '
    ' Purpose: When a the REMOVE SELECTED button is clicked remove the selected 
    '          machine from the list of selected machines for the current
    '          employee, job, and category, and then update the lists
    '
    ' ************************************************************************
    Protected Sub btnTrainerRemoveSelected_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnTrainerRemoveSelected.Click
        Try
            If (lstTrainerSelectedMachines.SelectedIndex = -1) Then
                MessageBox("You must choose a selected machine before you can proceed.", "USER ERROR")
                Exit Sub
            End If

            Dim jobNum As Integer = Integer.Parse(Session("SELECTED_JOB_NUMBER").ToString())
            Dim eCode As Integer = Integer.Parse(Session("SELECTED_EMPLOYEE_ID").ToString())
            Dim catNum As Integer = Integer.Parse(Session("SELECTED_CATEGORY_NUMBER").ToString())

            For Each aMach As ListItem In lstTrainerSelectedMachines.Items
                If (aMach.Selected = True) Then
                    Dim mCode As String = aMach.Value
                    Dim delQuery As tblTRAINER_QUALIFICATION = _
                        (From tq In db.tblTRAINER_QUALIFICATIONs() _
                         Select tq _
                         Where tq.EMPLOYEE_ID = eCode And _
                               tq.JOB_TITLE_NUMBER = catNum And _
                               tq.MASTER_JOB_NUMBER = jobNum And _
                               tq.MACHINE_CODE = mCode).FirstOrDefault()

                    db.tblTRAINER_QUALIFICATIONs().DeleteOnSubmit(delQuery)
                End If
            Next
            db.SubmitChanges()

            PopulateTrainerDetails()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: PopulateTrainerDetails
    '
    ' Purpose: Helper method to populate the lists of available and
    '          selected machines
    '
    ' ************************************************************************
    Private Sub PopulateTrainerDetails()
        udpTrainerBottom.Visible = True

        ' Query to find the machines that have already been selected
        Dim jobNum As Integer = Integer.Parse(Session("SELECTED_JOB_NUMBER").ToString())
        Dim eCode As Integer = Integer.Parse(Session("SELECTED_EMPLOYEE_ID").ToString())
        Dim catNum As Integer = Integer.Parse(Session("SELECTED_CATEGORY_NUMBER").ToString())
        Dim selQuery As List(Of String) = _
            (From quals In db.tblTRAINER_QUALIFICATIONs() _
             Order By quals.MACHINE_CODE _
             Where quals.EMPLOYEE_ID = eCode And _
                   quals.MASTER_JOB_NUMBER = jobNum And _
                   quals.JOB_TITLE_NUMBER = catNum _
             Select quals.MACHINE_CODE).ToList()
        lstTrainerSelectedMachines.DataSource = selQuery
        lstTrainerSelectedMachines.DataBind()

        ' Query to find the machines that have not been selected
        Dim availQuery As List(Of String) = _
            (From Jmm In db.tblJOB_MACHINE_MASTERs() _
             Order By Jmm.MACHINE_CODE _
             Where Jmm.MASTER_JOB_NUMBER = jobNum And _
                   Jmm.JOB_TITLE_NUMBER = catNum _
             Select Jmm.MACHINE_CODE).ToList()
        lstTrainerAvailableMachines.DataSource = availQuery.Except(selQuery)
        lstTrainerAvailableMachines.DataBind()
    End Sub

    ' ************************************************************************
    '    Name: ClearTrainerFields
    '
    ' Purpose: Helper method to blank the trainer view's detail widgets
    '
    ' ************************************************************************
    Private Sub ClearTrainerFields()
        ddlTrainerJobTitle.SelectedIndex = -1
        ddlTrainerEmployee.SelectedIndex = -1
        ddlTrainerCategory.SelectedIndex = -1

        Session("SELECTED_JOB_NUMBER") = 0
        Session("SELECTED_JOB") = ""
        Session("SELECTED_EMPLOYEE") = ""
        Session("SELECTED_EMPLOYEE_ID") = 0
        Session("SELECTED_CATEGORY_NUMBER") = 0
        Session("SELECTED_CATEGORY") = ""

        lstTrainerSelectedMachines.DataSource = Nothing
        lstTrainerSelectedMachines.DataBind()
        lstTrainerSelectedMachines.Items.Clear()
        lstTrainerAvailableMachines.DataSource = Nothing
        lstTrainerAvailableMachines.DataBind()
        lstTrainerAvailableMachines.Items.Clear()

        udpTrainerBottom.Visible = False
    End Sub

#End Region

#Region "Administrator Maintenance Methods"

    ' ************************************************************************
    '           The methods below are for ADMINISTRATOR MAINTENANCE 
    ' ************************************************************************



    ' ************************************************************************
    '    Name: viewAdministrators_PreRender
    '
    ' Purpose: Update the list of Administrators before the view is displayed
    '
    ' ************************************************************************
    Protected Sub viewAdministrators_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles viewAdministrators.PreRender
        Try
            ' Query the database to retrieve the Administrator names and bind them to the widget
            Dim populateQuery As List(Of String) = _
                (From emps In db.tblADMINISTRATORs() _
                 Select FULL_NAME = emps.LAST_NAME + ", " + emps.FIRST_NAME _
                 Order By FULL_NAME).ToList()
            lstAdministrators.DataSource = populateQuery
            lstAdministrators.DataBind()
            lstAdministrators.SelectedIndex = populateQuery.IndexOf(Session("SELECTED_ADMINISTRATOR").ToString())
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnNewAdministrators_Click
    '
    ' Purpose: When the Administrator view's NEW button is clicked,
    '          show the detail widgets
    '
    ' ************************************************************************
    Protected Sub btnNewAdministrators_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnNewAdministrators.Click
        Try
            ClearAdministratorsFields()
            btnApplyAdministrators.Text = "Add"
            txtAdministratorLogin.Focus()
            udpAdministratorsBottom.Visible = True
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnEditAdministrators_Click
    '
    ' Purpose: When the Administrator view's EDIT button is clicked, update
    '          the detail widgets with all the info for the selected entry
    '
    ' ************************************************************************
    Protected Sub btnEditAdministrators_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnEditAdministrators.Click
        Try
            If (lstAdministrators.SelectedIndex = -1) Then
                MessageBox("You must select an entry in the list before you can edit.", "USER ERROR")
                Exit Sub
            End If

            ' Break full name into its individual parts for use in the query
            Dim pos As Integer = lstAdministrators.SelectedValue.IndexOf(",")
            Dim lastName As String = lstAdministrators.SelectedValue.Substring(0, pos)
            Dim firstName As String = lstAdministrators.SelectedValue.Substring(pos + 2)

            ' Retrieve the database record for the selected entry
            Dim editQuery As tblADMINISTRATOR = _
                (From emps In db.tblADMINISTRATORs() _
                 Select emps _
                 Where emps.FIRST_NAME = firstName And _
                       emps.LAST_NAME = lastName).FirstOrDefault()

            ' Display the record's information in the detail widgets
            Session("SELECTED_ADMINISTRATOR_ID") = editQuery.LOGIN
            Session("SELECTED_ADMINISTRATOR") = lstAdministrators.SelectedValue
            txtAdministratorLogin.Text = editQuery.LOGIN
            txtAdministratorPassword.Text = editQuery.PASSWORD
            txtAdministratorFirst.Text = editQuery.FIRST_NAME
            txtAdministratorLast.Text = editQuery.LAST_NAME
            txtAdministratorLogin.Focus()
            btnApplyAdministrators.Text = "Update"
            udpAdministratorsBottom.Visible = True
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnDeleteAdministrators_Click
    '
    ' Purpose: When the Administrator view's DELETE button is clicked,
    '          remove the info for the selected entry from the database
    '
    ' ************************************************************************
    Protected Sub btnDeleteAdministrators_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnDeleteAdministrators.Click
        Try
            If (lstAdministrators.SelectedIndex = -1) Then
                MessageBox("You must select an entry in the list before you can delete.", "USER ERROR")
                Exit Sub
            End If

            ' Break full name into its individual parts for use in the query
            Dim pos As Integer = lstAdministrators.SelectedValue.IndexOf(",")
            Dim lastName As String = lstAdministrators.SelectedValue.Substring(0, pos)
            Dim firstName As String = lstAdministrators.SelectedValue.Substring(pos + 2)

            ' Retrieve the database record for the selected entry
            Dim delQuery As List(Of tblADMINISTRATOR) = _
                (From emps In db.tblADMINISTRATORs() _
                 Select emps _
                 Where emps.FIRST_NAME = firstName And _
                       emps.LAST_NAME = lastName).ToList()

            ' Delete the selected records from the database
            db.tblADMINISTRATORs().DeleteAllOnSubmit(delQuery)
            db.SubmitChanges()

            ' Clear the selection and the detail widgets
            lstAdministrators.ClearSelection()
            ClearAdministratorsFields()
            btnApplyAdministrators.Text = "Add"
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnApplyAdministrators_Click
    '
    ' Purpose: When the Administrator view's APPLY button is clicked,
    '          add or update the info in the database
    '
    ' ************************************************************************
    Protected Sub btnApplyAdministrators_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnApplyAdministrators.Click
        Try
            If ((txtAdministratorLogin.Text.Trim.Length = 0) Or _
                (txtAdministratorPassword.Text.Trim.Length = 0) Or _
                (txtAdministratorFirst.Text.Trim.Length = 0) Or _
                (txtAdministratorLast.Text.Trim.Length = 0)) Then
                MessageBox("You must specify all of the Administrator info before you can save.", "USER ERROR")
                Exit Sub
            End If

            ' If the ID isn't 0 then it is the key to the entry we're updating
            Dim aCode As String = Session("SELECTED_ADMINISTRATOR_ID").ToString()
            If (aCode.Length <> 0) Then
                Dim updateQuery As tblADMINISTRATOR = _
                    (From aCodes In db.tblADMINISTRATORs() _
                     Select aCodes _
                     Where aCodes.LOGIN = aCode).FirstOrDefault()

                If updateQuery Is Nothing Then
                    MessageBox("Unable to update record #" + Session("SELECTED_ADMINISTRATOR_ID").ToString(), "SYSTEM ERROR")
                Else
                    ' The index can not change so updating the administrator means delete the existing record & add a new record
                    db.tblADMINISTRATORs().DeleteOnSubmit(updateQuery)
                    Dim newAdmin As New tblADMINISTRATOR With { _
                        .LOGIN = txtAdministratorLogin.Text.Trim, _
                        .PASSWORD = txtAdministratorPassword.Text.Trim, _
                        .FIRST_NAME = txtAdministratorFirst.Text.Trim, _
                        .LAST_NAME = txtAdministratorLast.Text.Trim}
                    db.tblADMINISTRATORs().InsertOnSubmit(newAdmin)
                    db.SubmitChanges()
                End If

                ClearAdministratorsFields()
            Else
                ' Otherwise, we're adding a new record
                Dim newAdmin As New tblADMINISTRATOR With { _
                        .LOGIN = txtAdministratorLogin.Text.Trim, _
                        .PASSWORD = txtAdministratorPassword.Text.Trim, _
                        .FIRST_NAME = txtAdministratorFirst.Text.Trim, _
                        .LAST_NAME = txtAdministratorLast.Text.Trim}
                db.tblADMINISTRATORs().InsertOnSubmit(newAdmin)
                db.SubmitChanges()
            End If

            btnApplyAdministrators.Text = "Add"
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnClearAdministrators_Click
    '
    ' Purpose: When the Administrator view's CLEAR button is clicked,
    '          blank the detail widgets
    '
    ' ************************************************************************
    Protected Sub btnClearAdministrators_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnClearAdministrators.Click
        Try
            ClearAdministratorsFields()
            btnApplyAdministrators.Text = "Add"
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ClearAdministratorsFields
    '
    ' Purpose: Helper method to blank the Administrator view's detail widgets
    '
    ' ************************************************************************
    Private Sub ClearAdministratorsFields()
        lstAdministrators.ClearSelection()
        Session("SELECTED_ADMINISTRATOR_ID") = 0
        Session("SELECTED_ADMINISTRATOR") = ""
        txtAdministratorLogin.Text = ""
        txtAdministratorPassword.Text = ""
        txtAdministratorFirst.Text = ""
        txtAdministratorLast.Text = ""
        udpAdministratorsBottom.Visible = False
    End Sub

#End Region

#Region "Position Maintenance By Machine Methods"

    ' ************************************************************************
    '        The methods below are for POSITION MAINTENANCE BY MACHINE
    ' ************************************************************************



    ' ************************************************************************
    '    Name: viewByMachines_PreRender
    '
    ' Purpose: Update the lists of jobs, machines, and categories before the view is displayed
    '
    ' ************************************************************************
    Protected Sub viewByMachines_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles viewByMachine.PreRender
        Try
            ' Query the database to retrieve the categories and bind them to the widget
            Dim categoryQuery As List(Of String) = _
                (From cats In db.tblJOB_TITLEs() _
                 Select cats.JOB_TITLE _
                 Order By JOB_TITLE).ToList()
            categoryQuery.Insert(0, MAKE_A_SELECTION)
            ddlByMachineCategory.DataSource = categoryQuery
            ddlByMachineCategory.DataBind()
            ddlByMachineCategory.SelectedIndex = categoryQuery.IndexOf(Session("SELECTED_CATEGORY").ToString())

            ' Query the database to retrieve the job titles and bind them to the widget
            Dim jobTitleQuery As List(Of String) = _
                (From Titles In db.tblMASTER_JOB_TITLEs() _
                 Select Titles.MASTER_JOB_TITLE _
                 Order By MASTER_JOB_TITLE).ToList()
            jobTitleQuery.Insert(0, MAKE_A_SELECTION)
            ddlByMachineJobTitle.DataSource = jobTitleQuery
            ddlByMachineJobTitle.DataBind()
            ddlByMachineJobTitle.SelectedIndex = jobTitleQuery.IndexOf(Session("SELECTED_JOB").ToString())

            ' Query the database to retrieve the machines and bind them to the widget
            Dim machQuery As List(Of String) = _
                (From mCodes In db.tblMACHINEs() _
                 Order By mCodes.MACHINE_CODE _
                 Select mCodes.MACHINE_CODE).ToList()
            machQuery.Insert(0, MAKE_A_SELECTION)
            ddlByMachineMachine.DataSource = machQuery
            ddlByMachineMachine.DataBind()
            ddlByMachineMachine.SelectedIndex = machQuery.IndexOf(Session("SELECTED_MACHINE").ToString())

        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ddlByMachineJobTitle_SelectedIndexChanged
    '
    ' Purpose: When a job is selected, store the selection
    '
    ' ************************************************************************
    Protected Sub ddlByMachineJobTitle_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlByMachineJobTitle.SelectedIndexChanged
        Try
            ' Query to retrieve the unique index
            Dim jobTitleQuery As Integer = _
                (From Titles In db.tblMASTER_JOB_TITLEs() _
                 Where Titles.MASTER_JOB_TITLE = ddlByMachineJobTitle.SelectedValue() _
                 Select Titles.MASTER_JOB_NUMBER).FirstOrDefault()

            Session("SELECTED_JOB_NUMBER") = jobTitleQuery
            Session("SELECTED_JOB") = ddlByMachineJobTitle.SelectedValue

            ' Reset widgets that rely on this value
            lstByMachineSelectedCodes.DataSource = Nothing
            lstByMachineSelectedCodes.DataBind()
            lstByMachineSelectedCodes.Items.Clear()
            lstByMachineAvailableCodes.DataSource = Nothing
            lstByMachineAvailableCodes.DataBind()
            lstByMachineAvailableCodes.Items.Clear()

            udpByMachineBottom.Visible = False
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ddlByMachineMachine_SelectedIndexChanged
    '
    ' Purpose: When a machine is selected, store the selection
    '
    ' ************************************************************************
    Protected Sub ddlByMachineMachine_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlByMachineMachine.SelectedIndexChanged
        Try
            Session("SELECTED_MACHINE") = ddlByMachineMachine.SelectedValue
            Session("SELECTED_MACHINE_CODE") = ddlByMachineMachine.SelectedValue

            ' Reset widgets that rely on this value
            lstByMachineSelectedCodes.DataSource = Nothing
            lstByMachineSelectedCodes.DataBind()
            lstByMachineSelectedCodes.Items.Clear()
            lstByMachineAvailableCodes.DataSource = Nothing
            lstByMachineAvailableCodes.DataBind()
            lstByMachineAvailableCodes.Items.Clear()

            udpByMachineBottom.Visible = False
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ddlByMachineCategory_SelectedIndexChanged
    '
    ' Purpose: When a category is selected, store the selection
    '
    ' ************************************************************************
    Protected Sub ddlByMachineCategory_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlByMachineCategory.SelectedIndexChanged
        Try
            ' Query to retrieve the unique index
            Dim categoryQuery As Integer = _
                (From cats In db.tblJOB_TITLEs() _
                 Where cats.JOB_TITLE = ddlByMachineCategory.SelectedValue() _
                 Select cats.JOB_TITLE_NUMBER).FirstOrDefault()

            Session("SELECTED_CATEGORY_NUMBER") = categoryQuery
            Session("SELECTED_CATEGORY") = ddlByMachineCategory.SelectedValue

            ' Reset widgets that rely on this value
            lstByMachineSelectedCodes.DataSource = Nothing
            lstByMachineSelectedCodes.DataBind()
            lstByMachineSelectedCodes.Items.Clear()
            lstByMachineAvailableCodes.DataSource = Nothing
            lstByMachineAvailableCodes.DataBind()
            lstByMachineAvailableCodes.Items.Clear()

            udpByMachineBottom.Visible = False
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnByMachineLookup_Click
    '
    ' Purpose: When a the LOOKUP button is clicked fill in the details based
    '          on the selected entries
    '
    ' ************************************************************************
    Protected Sub btnByMachineLookup_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnByMachineLookup.Click
        Try
            If (ddlByMachineJobTitle.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlByMachineMachine.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlByMachineCategory.SelectedValue().Equals(MAKE_A_SELECTION)) Then
                MessageBox("You must select a job, a machine, and a category before you can proceed.", "USER ERROR")
                Exit Sub
            End If

            PopulateByMachineDetails()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnByMachineDisplayForm_Click
    '
    ' Purpose: When a the DISPLAY FORM button is clicked display the training
    '          form based on the selected entries
    '
    ' ************************************************************************
    Protected Sub btnByMachineDisplayForm_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnByMachineDisplayForm.Click
        Try
            If (ddlByMachineJobTitle.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlByMachineMachine.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlByMachineCategory.SelectedValue().Equals(MAKE_A_SELECTION)) Then
                MessageBox("You must select a job, a machine, and a category before you can proceed.", "USER ERROR")
                Exit Sub
            End If

            ' Javascript causes the Training Form to be displayed
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnByMachineAddSelected_Click
    '
    ' Purpose: When a the ADD SELECTED button is clicked add the selected 
    '          training code to the list of selected training codes for the
    '          current machine, job, and category, and then update the lists
    '
    ' ************************************************************************
    Protected Sub btnByMachineAddSelected_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnByMachineAddSelected.Click
        Try
            If (lstByMachineAvailableCodes.SelectedIndex = -1) Then
                MessageBox("You must choose an available code before you can proceed.", "USER ERROR")
                Exit Sub
            End If

            Dim jobNum As Integer = Integer.Parse(Session("SELECTED_JOB_NUMBER").ToString())
            Dim mCode As String = Session("SELECTED_MACHINE_CODE").ToString()
            Dim catNum As Integer = Integer.Parse(Session("SELECTED_CATEGORY_NUMBER").ToString())

            For Each tCode As ListItem In lstByMachineAvailableCodes.Items
                If (tCode.Selected = True) Then
                    Dim tCodeString As String = tCode.Value
                    ' Query to find the training number for the selected training code
                    Dim codeQuery As Integer = _
                        (From tc In db.tblTRAINING_CODEs _
                         Order By tc.TITLE _
                         Where tc.TITLE = tCodeString _
                         Select tc.TRAINING_NUMBER).FirstOrDefault()

                    Dim newTcode As New tblJOB_MACHINE_MASTER With { _
                            .MASTER_JOB_NUMBER = jobNum, _
                            .MACHINE_CODE = mCode, _
                            .JOB_TITLE_NUMBER = catNum, _
                            .TRAINING_NUMBER = codeQuery}
                    db.tblJOB_MACHINE_MASTERs().InsertOnSubmit(newTcode)
                End If
            Next
            db.SubmitChanges()

            PopulateByMachineDetails()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnByMachineAddAll_Click
    '
    ' Purpose: When a the ADD ALL button is clicked add all available 
    '          training codes to the list of selected training codes for the
    '          current job, machine, and category, and then update the lists
    '
    ' ************************************************************************
    Protected Sub btnByMachineAddAll_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnByMachineAddAll.Click
        Try
            If (lstByMachineAvailableCodes.Items.Count = 0) Then
                MessageBox("There are no entries in the list of available codes.", "USER ERROR")
                Exit Sub
            End If

            Dim jobNum As Integer = Integer.Parse(Session("SELECTED_JOB_NUMBER").ToString())
            Dim mCode As String = Session("SELECTED_MACHINE_CODE").ToString()
            Dim catNum As Integer = Integer.Parse(Session("SELECTED_CATEGORY_NUMBER").ToString())

            For Each tCode As ListItem In lstByMachineAvailableCodes.Items
                Dim tcodeString As String = tCode.Value
                Dim codeQuery As Integer = _
                    (From tc In db.tblTRAINING_CODEs _
                     Order By tc.TITLE _
                     Where tc.TITLE = tcodeString _
                     Select tc.TRAINING_NUMBER).FirstOrDefault()
                Dim newTcode As New tblJOB_MACHINE_MASTER With { _
                        .MASTER_JOB_NUMBER = jobNum, _
                        .MACHINE_CODE = mCode, _
                        .JOB_TITLE_NUMBER = catNum, _
                        .TRAINING_NUMBER = codeQuery}
                db.tblJOB_MACHINE_MASTERs().InsertOnSubmit(newTcode)
            Next
            db.SubmitChanges()

            PopulateByMachineDetails()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnByMachineRemoveAll_Click
    '
    ' Purpose: When a the REMOVE ALL button is clicked remove all 
    '          training codes from the list of selected training codes for the
    '          current job, machine, and category, and then update the lists
    '
    ' ************************************************************************
    Protected Sub btnByMachineRemoveAll_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnByMachineRemoveAll.Click
        Try
            If (lstByMachineSelectedCodes.Items.Count = 0) Then
                MessageBox("There are no entries in the list of selected codes.", "USER ERROR")
                Exit Sub
            End If

            Dim jobNum As Integer = Integer.Parse(Session("SELECTED_JOB_NUMBER").ToString())
            Dim mCode As String = Session("SELECTED_MACHINE_CODE").ToString()
            Dim catNum As Integer = Integer.Parse(Session("SELECTED_CATEGORY_NUMBER").ToString())

            For Each tCode As ListItem In lstByMachineSelectedCodes.Items
                Dim tcodeString As String = tCode.Value
                Dim codeQuery As Integer = _
                    (From tc In db.tblTRAINING_CODEs _
                     Order By tc.TITLE _
                     Where tc.TITLE = tcodeString _
                     Select tc.TRAINING_NUMBER).FirstOrDefault()
                Dim delQuery As tblJOB_MACHINE_MASTER = _
                    (From jmm In db.tblJOB_MACHINE_MASTERs() _
                     Select jmm _
                     Where jmm.TRAINING_NUMBER = codeQuery And _
                           jmm.JOB_TITLE_NUMBER = catNum And _
                           jmm.MASTER_JOB_NUMBER = jobNum And _
                           jmm.MACHINE_CODE = mCode).FirstOrDefault()

                Dim delEqQuery As List(Of tblEMPLOYEE_QUALIFICATION) = _
                    (From Eq In db.tblEMPLOYEE_QUALIFICATIONs() _
                     Select Eq _
                     Where Eq.TRAINING_NUMBER = codeQuery And _
                           Eq.JOB_TITLE_NUMBER = catNum And _
                           Eq.MASTER_JOB_NUMBER = jobNum And _
                           Eq.MACHINE_CODE = mCode).ToList()

                ' Delete the selected records from the database
                db.tblEMPLOYEE_QUALIFICATIONs().DeleteAllOnSubmit(delEqQuery)
                db.tblJOB_MACHINE_MASTERs().DeleteOnSubmit(delQuery)
            Next
            db.SubmitChanges()

            PopulateByMachineDetails()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnByMachineRemoveSelected_Click
    '
    ' Purpose: When a the REMOVE SELECTED button is clicked remove the selected 
    '          training code from the list of selected training codes for the 
    '          current job, machine, and category, and then update the lists
    '
    ' ************************************************************************
    Protected Sub btnByMachineRemoveSelected_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnByMachineRemoveSelected.Click
        Try
            If (lstByMachineSelectedCodes.SelectedIndex = -1) Then
                MessageBox("You must choose a selected code before you can proceed.", "USER ERROR")
                Exit Sub
            End If

            Dim jobNum As Integer = Integer.Parse(Session("SELECTED_JOB_NUMBER").ToString())
            Dim mCode As String = Session("SELECTED_MACHINE_CODE").ToString()
            Dim catNum As Integer = Integer.Parse(Session("SELECTED_CATEGORY_NUMBER").ToString())

            For Each tCode As ListItem In lstByMachineSelectedCodes.Items
                If (tCode.Selected = True) Then
                    Dim tCodeString As String = tCode.Value
                    Dim codeQuery As Integer = _
                        (From tc In db.tblTRAINING_CODEs _
                         Order By tc.TITLE _
                         Where tc.TITLE = tCodeString _
                         Select tc.TRAINING_NUMBER).FirstOrDefault()
                    Dim delQuery As tblJOB_MACHINE_MASTER = _
                        (From jmm In db.tblJOB_MACHINE_MASTERs() _
                         Select jmm _
                         Where jmm.TRAINING_NUMBER = codeQuery And _
                               jmm.JOB_TITLE_NUMBER = catNum And _
                               jmm.MASTER_JOB_NUMBER = jobNum And _
                               jmm.MACHINE_CODE = mCode).FirstOrDefault()

                    Dim delEqQuery As List(Of tblEMPLOYEE_QUALIFICATION) = _
                        (From Eq In db.tblEMPLOYEE_QUALIFICATIONs() _
                         Select Eq _
                         Where Eq.TRAINING_NUMBER = codeQuery And _
                               Eq.JOB_TITLE_NUMBER = catNum And _
                               Eq.MASTER_JOB_NUMBER = jobNum And _
                               Eq.MACHINE_CODE = mCode).ToList()

                    ' Delete the selected records from the database
                    db.tblEMPLOYEE_QUALIFICATIONs().DeleteAllOnSubmit(delEqQuery)
                    db.tblJOB_MACHINE_MASTERs().DeleteOnSubmit(delQuery)
                End If
            Next
            db.SubmitChanges()

            PopulateByMachineDetails()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: PopulateByMachineDetails
    '
    ' Purpose: Helper method to populate the lists of available and
    '          selected machines
    '
    ' ************************************************************************
    Private Sub PopulateByMachineDetails()
        ' Query to find the machines that have already been selected
        Dim jobNum As Integer = Integer.Parse(Session("SELECTED_JOB_NUMBER").ToString())
        Dim mCode As String = Session("SELECTED_MACHINE_CODE").ToString()
        Dim catNum As Integer = Integer.Parse(Session("SELECTED_CATEGORY_NUMBER").ToString())
        Dim selQuery As List(Of String) = _
            (From sc In db.tblJOB_MACHINE_MASTERs() _
             Join tc In db.tblTRAINING_CODEs _
             On sc.TRAINING_NUMBER Equals tc.TRAINING_NUMBER _
             Order By tc.TITLE _
             Where sc.MACHINE_CODE = mCode And _
                   sc.MASTER_JOB_NUMBER = jobNum And _
                   sc.JOB_TITLE_NUMBER = catNum _
             Select tc.TITLE).ToList()
        lstByMachineSelectedCodes.DataSource = selQuery
        lstByMachineSelectedCodes.DataBind()

        ' Query to find the machines that have not been selected
        Dim availQuery As List(Of String) = _
            (From tc In db.tblTRAINING_CODEs _
             Order By tc.TITLE _
             Select tc.TITLE).ToList()
        lstByMachineAvailableCodes.DataSource = availQuery.Except(selQuery)
        lstByMachineAvailableCodes.DataBind()

        udpByMachineBottom.Visible = True
    End Sub

    ' ************************************************************************
    '    Name: ClearByMachineFields
    '
    ' Purpose: Helper method to blank the ByMachine view's detail widgets
    '
    ' ************************************************************************
    Private Sub ClearByMachineFields()
        ddlByMachineJobTitle.SelectedIndex = -1
        ddlByMachineMachine.SelectedIndex = -1
        ddlByMachineCategory.SelectedIndex = -1

        Session("SELECTED_JOB_NUMBER") = 0
        Session("SELECTED_JOB") = ""
        Session("SELECTED_MACHINE") = ""
        Session("SELECTED_MACHINE_CODE") = ""
        Session("SELECTED_CATEGORY_NUMBER") = 0
        Session("SELECTED_CATEGORY") = ""

        lstByMachineSelectedCodes.DataSource = Nothing
        lstByMachineSelectedCodes.DataBind()
        lstByMachineSelectedCodes.Items.Clear()
        lstByMachineAvailableCodes.DataSource = Nothing
        lstByMachineAvailableCodes.DataBind()
        lstByMachineAvailableCodes.Items.Clear()

        udpByMachineBottom.Visible = False
    End Sub

#End Region

#Region "Position Maintenance By Training Code"

    ' ************************************************************************
    '     The methods below are for POSITION MAINTENANCE BY TRAINING CODE
    ' ************************************************************************



    ' ************************************************************************
    '    Name: viewByTrainingCodes_PreRender
    '
    ' Purpose: Update the lists of training codes, machines, and categories
    '          before the view is displayed
    '
    ' ************************************************************************
    Protected Sub viewByTrainingCodes_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles viewByTrainingCode.PreRender
        Try
            ' Query the database to retrieve the categories and bind them to the widget
            Dim categoryQuery As List(Of String) = _
                (From cats In db.tblJOB_TITLEs() _
                 Select cats.JOB_TITLE _
                 Order By JOB_TITLE).ToList()
            categoryQuery.Insert(0, MAKE_A_SELECTION)
            ddlByTrainingCodeCategory.DataSource = categoryQuery
            ddlByTrainingCodeCategory.DataBind()
            ddlByTrainingCodeCategory.SelectedIndex = categoryQuery.IndexOf(Session("SELECTED_CATEGORY").ToString())

            ' Query the database to retrieve the training codes and bind them to the widget
            Dim tCodeQuery As List(Of String) = _
                (From tCodes In db.tblTRAINING_CODEs() _
                 Select tCodes.TITLE _
                 Order By TITLE).ToList()
            tCodeQuery.Insert(0, MAKE_A_SELECTION)
            ddlByTrainingCodeTrainingCode.DataSource = tCodeQuery
            ddlByTrainingCodeTrainingCode.DataBind()
            ddlByTrainingCodeTrainingCode.SelectedIndex = tCodeQuery.IndexOf(Session("SELECTED_TRAINING_CODE").ToString())

            ' Query the database to retrieve the machines and bind them to the widget
            Dim machQuery As List(Of String) = _
                (From mCodes In db.tblMACHINEs() _
                 Order By mCodes.MACHINE_CODE _
                 Select mCodes.MACHINE_CODE).ToList()
            machQuery.Insert(0, MAKE_A_SELECTION)
            ddlByTrainingCodeMachine.DataSource = machQuery
            ddlByTrainingCodeMachine.DataBind()
            ddlByTrainingCodeMachine.SelectedIndex = machQuery.IndexOf(Session("SELECTED_MACHINE").ToString())
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ddlByTrainingCodeTrainingCode_SelectedIndexChanged
    '
    ' Purpose: When a training code is selected, store the selection
    '
    ' ************************************************************************
    Protected Sub ddlByTrainingCodeTrainingCode_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlByTrainingCodeTrainingCode.SelectedIndexChanged
        Try
            ' Query to retrieve the unique index
            Dim tCodeQuery As Integer = _
                (From tCodes In db.tblTRAINING_CODEs() _
                 Where tCodes.TITLE = ddlByTrainingCodeTrainingCode.SelectedValue() _
                 Select tCodes.TRAINING_NUMBER).FirstOrDefault()

            Session("SELECTED_TRAINING_CODE_ID") = tCodeQuery
            Session("SELECTED_TRAINING_CODE") = ddlByTrainingCodeTrainingCode.SelectedValue

            ' Reset widgets that rely on this value
            lstByTrainingCodeSelectedJobs.DataSource = Nothing
            lstByTrainingCodeSelectedJobs.DataBind()
            lstByTrainingCodeSelectedJobs.Items.Clear()
            lstByTrainingCodeAvailableJobs.DataSource = Nothing
            lstByTrainingCodeAvailableJobs.DataBind()
            lstByTrainingCodeAvailableJobs.Items.Clear()

            udpByTrainingCodeBottom.Visible = False
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ddlByTrainingCodeMachine_SelectedIndexChanged
    '
    ' Purpose: When a machine is selected, store the selection
    '
    ' ************************************************************************
    Protected Sub ddlByTrainingCodeMachine_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlByTrainingCodeMachine.SelectedIndexChanged
        Try
            Session("SELECTED_MACHINE") = ddlByTrainingCodeMachine.SelectedValue
            Session("SELECTED_MACHINE_CODE") = ddlByTrainingCodeMachine.SelectedValue

            ' Reset widgets that rely on this value
            lstByTrainingCodeSelectedJobs.DataSource = Nothing
            lstByTrainingCodeSelectedJobs.DataBind()
            lstByTrainingCodeSelectedJobs.Items.Clear()
            lstByTrainingCodeAvailableJobs.DataSource = Nothing
            lstByTrainingCodeAvailableJobs.DataBind()
            lstByTrainingCodeAvailableJobs.Items.Clear()

            udpByTrainingCodeBottom.Visible = False
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ddlByTrainingCodeCategory_SelectedIndexChanged
    '
    ' Purpose: When a category is selected, store the selection
    '
    ' ************************************************************************
    Protected Sub ddlByTrainingCodeCategory_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlByTrainingCodeCategory.SelectedIndexChanged
        Try
            ' Query to retrieve the unique index
            Dim categoryQuery As Integer = _
                (From cats In db.tblJOB_TITLEs() _
                 Where cats.JOB_TITLE = ddlByTrainingCodeCategory.SelectedValue() _
                 Select cats.JOB_TITLE_NUMBER).FirstOrDefault()

            Session("SELECTED_CATEGORY_NUMBER") = categoryQuery
            Session("SELECTED_CATEGORY") = ddlByTrainingCodeCategory.SelectedValue

            ' Reset widgets that rely on this value
            lstByTrainingCodeSelectedJobs.DataSource = Nothing
            lstByTrainingCodeSelectedJobs.DataBind()
            lstByTrainingCodeSelectedJobs.Items.Clear()
            lstByTrainingCodeAvailableJobs.DataSource = Nothing
            lstByTrainingCodeAvailableJobs.DataBind()
            lstByTrainingCodeAvailableJobs.Items.Clear()

            udpByTrainingCodeBottom.Visible = False
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnByTrainingCodeLookup_Click
    '
    ' Purpose: When a the LOOKUP button is clicked fill in the details based
    '          on the selected entries
    '
    ' ************************************************************************
    Protected Sub btnByTrainingCodeLookup_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnByTrainingCodeLookup.Click
        Try
            If (ddlByTrainingCodeTrainingCode.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlByTrainingCodeMachine.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlByTrainingCodeCategory.SelectedValue().Equals(MAKE_A_SELECTION)) Then
                MessageBox("You must select a training code, a machine, and a category before you can proceed.", "USER ERROR")
                Exit Sub
            End If

            PopulateByTrainingCodeDetails()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnByTrainingCodeAddSelected_Click
    '
    ' Purpose: When a the ADD SELECTED button is clicked add the selected 
    '          job title to the list of selected job titles for the current
    '          machine, training code, and category, and then update the lists
    '
    ' ************************************************************************
    Protected Sub btnByTrainingCodeAddSelected_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnByTrainingCodeAddSelected.Click
        Try
            If (lstByTrainingCodeAvailableJobs.SelectedIndex = -1) Then
                MessageBox("You must choose an available job before you can proceed.", "USER ERROR")
                Exit Sub
            End If

            Dim tCode As Integer = Integer.Parse(Session("SELECTED_TRAINING_CODE_ID").ToString())
            Dim mCode As String = Session("SELECTED_MACHINE_CODE").ToString()
            Dim catNum As Integer = Integer.Parse(Session("SELECTED_CATEGORY_NUMBER").ToString())

            For Each jobTitle As ListItem In lstByTrainingCodeAvailableJobs.Items
                If (jobTitle.Selected = True) Then
                    Dim jobString As String = jobTitle.Value
                    ' Query to find the job number for the selected job title
                    Dim jobQuery As Integer = _
                        (From job In db.tblMASTER_JOB_TITLEs _
                         Order By job.MASTER_JOB_TITLE _
                         Where job.MASTER_JOB_TITLE = jobString _
                         Select job.MASTER_JOB_NUMBER).FirstOrDefault()

                    Dim newTcode As New tblJOB_MACHINE_MASTER With { _
                            .MASTER_JOB_NUMBER = jobQuery, _
                            .MACHINE_CODE = mCode, _
                            .JOB_TITLE_NUMBER = catNum, _
                            .TRAINING_NUMBER = tCode}
                    db.tblJOB_MACHINE_MASTERs().InsertOnSubmit(newTcode)
                End If
            Next
            db.SubmitChanges()

            PopulateByTrainingCodeDetails()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnByTrainingCodeAddAll_Click
    '
    ' Purpose: When a the ADD ALL button is clicked add all available 
    '          job titles to the list of selected job titles for the current
    '          training code, machine, and category, and then update the lists
    '
    ' ************************************************************************
    Protected Sub btnByTrainingCodeAddAll_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnByTrainingCodeAddAll.Click
        Try
            If (lstByTrainingCodeAvailableJobs.Items.Count = 0) Then
                MessageBox("There are no entries in the list of available jobs.", "USER ERROR")
                Exit Sub
            End If

            Dim tCode As Integer = Integer.Parse(Session("SELECTED_TRAINING_CODE_ID").ToString())
            Dim mCode As String = Session("SELECTED_MACHINE_CODE").ToString()
            Dim catNum As Integer = Integer.Parse(Session("SELECTED_CATEGORY_NUMBER").ToString())

            For Each jTitle As ListItem In lstByTrainingCodeAvailableJobs.Items
                Dim jobString As String = jTitle.Value
                ' Query to find the job number for the current job title
                Dim jobQuery As Integer = _
                    (From job In db.tblMASTER_JOB_TITLEs _
                     Order By job.MASTER_JOB_TITLE _
                     Where job.MASTER_JOB_TITLE = jobString _
                     Select job.MASTER_JOB_NUMBER).FirstOrDefault()
                Dim newTcode As New tblJOB_MACHINE_MASTER With { _
                        .MASTER_JOB_NUMBER = jobQuery, _
                        .MACHINE_CODE = mCode, _
                        .JOB_TITLE_NUMBER = catNum, _
                        .TRAINING_NUMBER = tCode}
                db.tblJOB_MACHINE_MASTERs().InsertOnSubmit(newTcode)
            Next
            db.SubmitChanges()

            PopulateByTrainingCodeDetails()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnByTrainingCodeRemoveAll_Click
    '
    ' Purpose: When a the REMOVE ALL button is clicked remove all 
    '          job titles from the list of selected job titles for the current
    '          training code, machine, and category, and then update the lists
    '
    ' ************************************************************************
    Protected Sub btnByTrainingCodeRemoveAll_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnByTrainingCodeRemoveAll.Click
        Try
            If (lstByTrainingCodeSelectedJobs.Items.Count = 0) Then
                MessageBox("There are no entries in the list of selected jobs.", "USER ERROR")
                Exit Sub
            End If

            Dim tCode As Integer = Integer.Parse(Session("SELECTED_TRAINING_CODE_ID").ToString())
            Dim mCode As String = Session("SELECTED_MACHINE_CODE").ToString()
            Dim catNum As Integer = Integer.Parse(Session("SELECTED_CATEGORY_NUMBER").ToString())

            For Each jTitle As ListItem In lstByTrainingCodeSelectedJobs.Items
                Dim jobString As String = jTitle.Value
                ' Query to find the job number for the current job title
                Dim jobQuery As Integer = _
                    (From job In db.tblMASTER_JOB_TITLEs _
                     Order By job.MASTER_JOB_TITLE _
                     Where job.MASTER_JOB_TITLE = jobString _
                     Select job.MASTER_JOB_NUMBER).FirstOrDefault()
                Dim delQuery As tblJOB_MACHINE_MASTER = _
                    (From jmm In db.tblJOB_MACHINE_MASTERs() _
                     Select jmm _
                     Where jmm.TRAINING_NUMBER = tCode And _
                           jmm.JOB_TITLE_NUMBER = catNum And _
                           jmm.MASTER_JOB_NUMBER = jobQuery And _
                           jmm.MACHINE_CODE = mCode).FirstOrDefault()

                Dim delEqQuery As List(Of tblEMPLOYEE_QUALIFICATION) = _
                    (From Eq In db.tblEMPLOYEE_QUALIFICATIONs() _
                     Select Eq _
                     Where Eq.TRAINING_NUMBER = tCode And _
                           Eq.JOB_TITLE_NUMBER = catNum And _
                           Eq.MASTER_JOB_NUMBER = jobQuery And _
                           Eq.MACHINE_CODE = mCode).ToList()

                ' Delete the selected records from the database
                db.tblEMPLOYEE_QUALIFICATIONs().DeleteAllOnSubmit(delEqQuery)
                db.tblJOB_MACHINE_MASTERs().DeleteOnSubmit(delQuery)
            Next
            db.SubmitChanges()

            PopulateByTrainingCodeDetails()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnByTrainingCodeRemoveSelected_Click
    '
    ' Purpose: When a the REMOVE SELECTED button is clicked remove the selected 
    '          job title from the list of selected job titles for the current
    '          training code, machine, and category, and then update the lists
    '
    ' ************************************************************************
    Protected Sub btnByTrainingCodeRemoveSelected_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnByTrainingCodeRemoveSelected.Click
        Try
            If (lstByTrainingCodeSelectedJobs.SelectedIndex = -1) Then
                MessageBox("You must choose a selected job before you can proceed.", "USER ERROR")
                Exit Sub
            End If

            Dim tCode As Integer = Integer.Parse(Session("SELECTED_TRAINING_CODE_ID").ToString())
            Dim mCode As String = Session("SELECTED_MACHINE_CODE").ToString()
            Dim catNum As Integer = Integer.Parse(Session("SELECTED_CATEGORY_NUMBER").ToString())

            For Each jobTitle As ListItem In lstByTrainingCodeSelectedJobs.Items
                If (jobTitle.Selected = True) Then
                    Dim jobString As String = jobTitle.Value
                    ' Query to find the job number for the selected job title
                    Dim jobQuery As Integer = _
                        (From job In db.tblMASTER_JOB_TITLEs _
                         Order By job.MASTER_JOB_TITLE _
                         Where job.MASTER_JOB_TITLE = jobString _
                         Select job.MASTER_JOB_NUMBER).FirstOrDefault()
                    Dim delQuery As tblJOB_MACHINE_MASTER = _
                        (From jmm In db.tblJOB_MACHINE_MASTERs() _
                         Select jmm _
                         Where jmm.TRAINING_NUMBER = tCode And _
                               jmm.JOB_TITLE_NUMBER = catNum And _
                               jmm.MASTER_JOB_NUMBER = jobQuery And _
                               jmm.MACHINE_CODE = mCode).FirstOrDefault()

                    Dim delEqQuery As List(Of tblEMPLOYEE_QUALIFICATION) = _
                        (From Eq In db.tblEMPLOYEE_QUALIFICATIONs() _
                         Select Eq _
                         Where Eq.TRAINING_NUMBER = tCode And _
                               Eq.JOB_TITLE_NUMBER = catNum And _
                               Eq.MASTER_JOB_NUMBER = jobQuery And _
                               Eq.MACHINE_CODE = mCode).ToList()

                    ' Delete the selected records from the database
                    db.tblEMPLOYEE_QUALIFICATIONs().DeleteAllOnSubmit(delEqQuery)
                    db.tblJOB_MACHINE_MASTERs().DeleteOnSubmit(delQuery)
                End If
            Next
            db.SubmitChanges()

            PopulateByTrainingCodeDetails()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: PopulateByTrainingCodeDetails
    '
    ' Purpose: Helper method to populate the lists of available and
    '          selected machines
    '
    ' ************************************************************************
    Private Sub PopulateByTrainingCodeDetails()
        ' Query to find the machines that have already been selected
        Dim tCode As Integer = Integer.Parse(Session("SELECTED_TRAINING_CODE_ID").ToString())
        Dim mCode As String = Session("SELECTED_MACHINE_CODE").ToString()
        Dim catNum As Integer = Integer.Parse(Session("SELECTED_CATEGORY_NUMBER").ToString())
        Dim selQuery As List(Of String) = _
            (From jmm In db.tblJOB_MACHINE_MASTERs() _
             Join mjt In db.tblMASTER_JOB_TITLEs _
             On jmm.MASTER_JOB_NUMBER Equals mjt.MASTER_JOB_NUMBER _
             Order By mjt.MASTER_JOB_TITLE _
             Where jmm.MACHINE_CODE = mCode And _
                   jmm.TRAINING_NUMBER = tCode And _
                   jmm.JOB_TITLE_NUMBER = catNum _
             Select mjt.MASTER_JOB_TITLE).ToList()
        lstByTrainingCodeSelectedJobs.DataSource = selQuery
        lstByTrainingCodeSelectedJobs.DataBind()

        ' Query to find the machines that have not been selected
        Dim availQuery As List(Of String) = _
            (From mjt In db.tblMASTER_JOB_TITLEs _
             Order By mjt.MASTER_JOB_TITLE _
             Select mjt.MASTER_JOB_TITLE).ToList()
        lstByTrainingCodeAvailableJobs.DataSource = availQuery.Except(selQuery)
        lstByTrainingCodeAvailableJobs.DataBind()

        udpByTrainingCodeBottom.Visible = True
    End Sub

    ' ************************************************************************
    '    Name: ClearByTrainingCodeFields
    '
    ' Purpose: Helper method to blank the ByTrainingCode view's detail widgets
    '
    ' ************************************************************************
    Private Sub ClearByTrainingCodeFields()
        ddlByTrainingCodeTrainingCode.SelectedIndex = -1
        ddlByTrainingCodeMachine.SelectedIndex = -1
        ddlByTrainingCodeCategory.SelectedIndex = -1

        Session("SELECTED_TRAINING_CODE_ID") = 0
        Session("SELECTED_TRAINING_CODE") = ""
        Session("SELECTED_MACHINE") = ""
        Session("SELECTED_MACHINE_CODE") = ""
        Session("SELECTED_CATEGORY_NUMBER") = 0
        Session("SELECTED_CATEGORY") = ""

        lstByTrainingCodeSelectedJobs.DataSource = Nothing
        lstByTrainingCodeSelectedJobs.DataBind()
        lstByTrainingCodeSelectedJobs.Items.Clear()
        lstByTrainingCodeAvailableJobs.DataSource = Nothing
        lstByTrainingCodeAvailableJobs.DataBind()
        lstByTrainingCodeAvailableJobs.Items.Clear()

        udpByTrainingCodeBottom.Visible = False
    End Sub

#End Region

#Region "Employee Training Methods"

    ' ************************************************************************
    '           The methods below are for EMPLOYEE TRAINING 
    ' ************************************************************************



    ' ************************************************************************
    '    Name: viewEmployeeTraining_PreRender
    '
    ' Purpose: Update the lists of employees, jobs, machines, and categories 
    '          before the view is displayed
    '
    ' ************************************************************************
    Protected Sub viewEmployeeTraining_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles viewEmployeeTraining.PreRender
        Try
            Dim jNum As Integer = Integer.Parse(Session("SELECTED_JOB_NUMBER").ToString())

            ' if the person is not logged in, hide the Select All button
            If (Session("CURRENT_LOGIN_ID") Is Nothing) Then
                btnEmpTrainSelectAll.Visible = False
            Else
                btnEmpTrainSelectAll.Visible = True
            End If

            ' Query the database to retrieve the employees and bind them to the widget
            Dim empQuery As List(Of String) = Nothing
            If (CBool(Session("INCLUDE_INACTIVE_EMPLOYEES")) = False) Then
                empQuery = (From Emps In db.tblEMPLOYEEs() _
                 Order By Emps.LAST_NAME, Emps.FIRST_NAME _
                 Where Emps.ACTIVE_STATUS = "A" _
                 Select FULL_NAME = Emps.LAST_NAME + ", " + Emps.FIRST_NAME).ToList()
            Else
                empQuery = (From Emps In db.tblEMPLOYEEs() _
                 Order By Emps.LAST_NAME, Emps.FIRST_NAME _
                 Select FULL_NAME = Emps.LAST_NAME + ", " + Emps.FIRST_NAME).ToList()
            End If
            empQuery.Insert(0, MAKE_A_SELECTION)
            ddlEmpTrainEmployee.DataSource = empQuery
            ddlEmpTrainEmployee.DataBind()
            ddlEmpTrainEmployee.SelectedIndex = empQuery.IndexOf(Session("SELECTED_EMPLOYEE").ToString())

            ' Query the database to retrieve the job titles and bind them to the widget
            Dim jobTitleQuery As List(Of String) = _
                (From Titles In db.tblMASTER_JOB_TITLEs() _
                 Select Titles.MASTER_JOB_TITLE _
                 Order By MASTER_JOB_TITLE).ToList()
            jobTitleQuery.Insert(0, MAKE_A_SELECTION)
            ddlEmpTrainJobTitle.DataSource = jobTitleQuery
            ddlEmpTrainJobTitle.DataBind()
            ddlEmpTrainJobTitle.SelectedIndex = jobTitleQuery.IndexOf(Session("SELECTED_JOB").ToString())

            ' Query the database to retrieve the categories and bind them to the widget
            Dim categoryQuery As List(Of String) = _
                (From cats In db.tblJOB_TITLEs() _
                 Select cats.JOB_TITLE _
                 Order By JOB_TITLE).ToList()
            categoryQuery.Insert(0, MAKE_A_SELECTION)
            ddlEmpTrainCategory.DataSource = categoryQuery
            ddlEmpTrainCategory.DataBind()
            ddlEmpTrainCategory.SelectedIndex = categoryQuery.IndexOf(Session("SELECTED_CATEGORY").ToString())

            ' The list of machines depends on which job is selected
            Dim machQuery As List(Of String) = Nothing
            If (jNum <> 0) Then
                ' Query the database to retrieve the machine codes and bind them to the widget
                machQuery = _
                    (From mCodes In db.tblMACHINEs() _
                     Join jmm In db.tblJOB_MACHINE_MASTERs() _
                     On mCodes.MACHINE_CODE Equals jmm.MACHINE_CODE _
                     Order By mCodes.MACHINE_CODE _
                     Where jmm.MASTER_JOB_NUMBER = jNum _
                     Select mCodes.MACHINE_CODE Distinct).ToList()
                machQuery.Sort()
                machQuery.Insert(0, MAKE_A_SELECTION)
                ddlEmpTrainMachine.DataSource = machQuery
            Else
                machQuery = New List(Of String)
                machQuery.Insert(0, MAKE_A_SELECTION)
                ddlEmpTrainMachine.DataSource = machQuery
            End If
            ddlEmpTrainMachine.DataBind()
            ddlEmpTrainMachine.SelectedIndex = machQuery.IndexOf(Session("SELECTED_MACHINE").ToString())

            If (Session("OJT_BACK") IsNot Nothing) Then
                btnEmpTrainBack.Visible = True
            Else
                btnEmpTrainBack.Visible = False
            End If
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ddlEmpTrainEmployee_SelectedIndexChanged
    '
    ' Purpose: When a Employee is selected, store the selection
    '
    ' ************************************************************************
    Protected Sub ddlEmpTrainEmployee_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlEmpTrainEmployee.SelectedIndexChanged
        Try
            ' Query to retrieve the unique index
            If (Not ddlEmpTrainEmployee.SelectedValue.Equals(MAKE_A_SELECTION)) Then
                ' Break full name into its individual parts for use in the query
                Dim pos As Integer = ddlEmpTrainEmployee.SelectedValue.IndexOf(",")
                Dim lastName As String = ddlEmpTrainEmployee.SelectedValue.Substring(0, pos)
                Dim firstName As String = ddlEmpTrainEmployee.SelectedValue.Substring(pos + 2)

                Dim empQuery As Integer = _
                    (From emps In db.tblEMPLOYEEs() _
                     Where emps.FIRST_NAME = firstName And emps.LAST_NAME = lastName _
                     Select emps.EMPLOYEE_ID).FirstOrDefault()

                Session("SELECTED_EMPLOYEE_ID") = empQuery
                Session("SELECTED_EMPLOYEE") = ddlEmpTrainEmployee.SelectedValue
            Else
                Session("SELECTED_EMPLOYEE_ID") = 0
                Session("SELECTED_EMPLOYEE") = ""
            End If

            ' Reset widgets that rely on this value
            ddlEmpTrainTrainer.SelectedIndex = -1
            txtEmpTrainTrainerPassword.Text = ""
            txtEmpTrainTrainerDate.Text = ""
            gridEmployeeTraining.DataSource = Nothing
            gridEmployeeTraining.DataBind()
            ddlEmpTrainTrainer.DataSource = Nothing
            ddlEmpTrainTrainer.DataBind()
            ddlEmpTrainTrainer.Items.Clear()
            ddlEmpTrainSupervisor.SelectedIndex = -1
            txtEmpTrainSupervisorPassword.Text = ""
            txtEmpTrainSupervisorDate.Text = ""
            ddlEmpTrainSupervisor.DataSource = Nothing
            ddlEmpTrainSupervisor.DataBind()
            ddlEmpTrainSupervisor.Items.Clear()

            udpEmployeeTrainingBottom.Visible = False
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ddlEmpTrainJobTitle_SelectedIndexChanged
    '
    ' Purpose: When a Job is selected, store the selection
    '
    ' ************************************************************************
    Protected Sub ddlEmpTrainJobTitle_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlEmpTrainJobTitle.SelectedIndexChanged
        Try
            ' Query to retrieve the unique index
            Dim jobTitleQuery As Integer = _
                (From Titles In db.tblMASTER_JOB_TITLEs() _
                 Where Titles.MASTER_JOB_TITLE = ddlEmpTrainJobTitle.SelectedValue() _
                 Select Titles.MASTER_JOB_NUMBER).FirstOrDefault()

            Session("SELECTED_JOB_NUMBER") = jobTitleQuery
            Session("SELECTED_JOB") = ddlEmpTrainJobTitle.SelectedValue

            ' Reset widgets that rely on this value
            ddlEmpTrainMachine.SelectedIndex = 0
            ddlEmpTrainTrainer.SelectedIndex = -1
            txtEmpTrainTrainerPassword.Text = ""
            txtEmpTrainTrainerDate.Text = ""
            gridEmployeeTraining.DataSource = Nothing
            gridEmployeeTraining.DataBind()
            ddlEmpTrainTrainer.DataSource = Nothing
            ddlEmpTrainTrainer.DataBind()
            ddlEmpTrainTrainer.Items.Clear()
            ddlEmpTrainSupervisor.SelectedIndex = -1
            txtEmpTrainSupervisorPassword.Text = ""
            txtEmpTrainSupervisorDate.Text = ""
            ddlEmpTrainSupervisor.DataSource = Nothing
            ddlEmpTrainSupervisor.DataBind()
            ddlEmpTrainSupervisor.Items.Clear()
            Session("SELECTED_MACHINE") = ""
            Session("SELECTED_MACHINE_CODE") = ""

            udpEmployeeTrainingBottom.Visible = False
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ddlEmpTrainMachine_SelectedIndexChanged
    '
    ' Purpose: When a Machine is selected, store the selection
    '
    ' ************************************************************************
    Protected Sub ddlEmpTrainMachine_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlEmpTrainMachine.SelectedIndexChanged
        Try
            Session("SELECTED_MACHINE") = ddlEmpTrainMachine.SelectedValue
            Session("SELECTED_MACHINE_CODE") = ddlEmpTrainMachine.SelectedValue

            ' Reset widgets that rely on this value
            ddlEmpTrainTrainer.SelectedIndex = -1
            txtEmpTrainTrainerPassword.Text = ""
            txtEmpTrainTrainerDate.Text = ""
            gridEmployeeTraining.DataSource = Nothing
            gridEmployeeTraining.DataBind()
            ddlEmpTrainTrainer.DataSource = Nothing
            ddlEmpTrainTrainer.DataBind()
            ddlEmpTrainTrainer.Items.Clear()
            ddlEmpTrainSupervisor.SelectedIndex = -1
            txtEmpTrainSupervisorPassword.Text = ""
            txtEmpTrainSupervisorDate.Text = ""
            ddlEmpTrainSupervisor.DataSource = Nothing
            ddlEmpTrainSupervisor.DataBind()
            ddlEmpTrainSupervisor.Items.Clear()

            udpEmployeeTrainingBottom.Visible = False
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ddlEmpTrainCategory_SelectedIndexChanged
    '
    ' Purpose: When a Category is selected, store the selection
    '
    ' ************************************************************************
    Protected Sub ddlEmpTrainCategory_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlEmpTrainCategory.SelectedIndexChanged
        Try
            ' Query to retrieve the unique index
            Dim categoryQuery As Integer = _
                (From cats In db.tblJOB_TITLEs() _
                 Where cats.JOB_TITLE = ddlEmpTrainCategory.SelectedValue() _
                 Select cats.JOB_TITLE_NUMBER).FirstOrDefault()

            Session("SELECTED_CATEGORY_NUMBER") = categoryQuery
            Session("SELECTED_CATEGORY") = ddlEmpTrainCategory.SelectedValue

            ' Reset widgets that rely on this value
            ddlEmpTrainTrainer.SelectedIndex = -1
            txtEmpTrainTrainerPassword.Text = ""
            txtEmpTrainTrainerDate.Text = ""
            gridEmployeeTraining.DataSource = Nothing
            gridEmployeeTraining.DataBind()
            ddlEmpTrainTrainer.DataSource = Nothing
            ddlEmpTrainTrainer.DataBind()
            ddlEmpTrainTrainer.Items.Clear()
            ddlEmpTrainSupervisor.SelectedIndex = -1
            txtEmpTrainSupervisorPassword.Text = ""
            txtEmpTrainSupervisorDate.Text = ""
            ddlEmpTrainSupervisor.DataSource = Nothing
            ddlEmpTrainSupervisor.DataBind()
            ddlEmpTrainSupervisor.Items.Clear()

            udpEmployeeTrainingBottom.Visible = False
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnEmpTrainLookup_Click
    '
    ' Purpose: When a the LOOKUP button is clicked fill in the details based
    '          on the selected entries
    '
    ' ************************************************************************
    Protected Sub btnEmpTrainLookup_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnEmpTrainLookup.Click
        Try
            If (ddlEmpTrainJobTitle.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlEmpTrainMachine.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlEmpTrainEmployee.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlEmpTrainCategory.SelectedValue().Equals(MAKE_A_SELECTION)) Then
                MessageBox("You must select an employee, a job, a machine, and a category before you can proceed.", "USER ERROR")
                Exit Sub
            End If

            ' Query to find the Job information associated with the selected items and display it
            Dim jobNum As Integer = Integer.Parse(Session("SELECTED_JOB_NUMBER").ToString())
            Dim mCode As String = Session("SELECTED_MACHINE_CODE").ToString()
            Dim catNum As Integer = Integer.Parse(Session("SELECTED_CATEGORY_NUMBER").ToString())
            Dim eCode As Integer = Integer.Parse(Session("SELECTED_EMPLOYEE_ID").ToString())
            Dim stepQuery = (From Jmm In db.tblJOB_MACHINE_MASTERs() _
                             Join Codes In db.tblTRAINING_CODEs() _
                             On Codes.TRAINING_NUMBER Equals Jmm.TRAINING_NUMBER _
                             Order By Codes.TITLE _
                             Where Jmm.MASTER_JOB_NUMBER = jobNum And _
                                   Jmm.MACHINE_CODE = mCode And _
                                   Jmm.JOB_TITLE_NUMBER = catNum _
                             Select New With {.STEP_COMPLETE = False, .DESCRIPTION = Codes.DESCRIPTION, .TRAINING_NUMBER = Codes.TRAINING_NUMBER, .DATE_APPROVED = ""}).ToList()

            ' If there are no training codes for the selected Job, Machine, 
            ' and Category, then display a message
            If (stepQuery.Count = 0) Then
                MessageBox("There is no training information available for the job, machine, and category specified.", "NO DATA")
                Exit Sub
            End If

            'Lookup each step for this training to see if it has been completed
            For Each trainingStep In stepQuery
                Dim trainingNum = trainingStep.TRAINING_NUMBER
                Dim doneQuery = (From eq In db.tblEMPLOYEE_QUALIFICATIONs _
                                 Where eq.EMPLOYEE_ID = eCode And _
                                       eq.MASTER_JOB_NUMBER = jobNum And _
                                       eq.MACHINE_CODE = mCode And _
                                       eq.JOB_TITLE_NUMBER = catNum And _
                                       eq.TRAINING_NUMBER = trainingNum _
                                 Select eq).FirstOrDefault()
                If (Not (doneQuery Is Nothing)) Then
                    trainingStep.STEP_COMPLETE = True
                    trainingStep.DATE_APPROVED = doneQuery.DATE_APPROVED.Value.ToString("MM/dd/yyyy")
                End If
            Next
            gridEmployeeTraining.DataSource = stepQuery
            gridEmployeeTraining.DataBind()

            ' Query to find if a job description is available
            Dim docQuery As tblMASTER_JOB_TITLE = _
                (From docs In db.tblMASTER_JOB_TITLEs _
                 Where docs.MASTER_JOB_NUMBER = jobNum _
                 Select docs).FirstOrDefault()
            If (Not (docQuery.DOCUMENT Is Nothing) AndAlso (docQuery.DOCUMENT.Length > 0)) Then
                lnkEmpTrainDocument.Visible = True
                lblEmpTrainDocument.Visible = True
                lnkEmpTrainDocument.NavigateUrl = docQuery.DOCUMENT
                lnkEmpTrainDocument.Text = docQuery.MASTER_JOB_TITLE
                lnkEmpTrainDocument.Target = "_blank"
            Else
                lnkEmpTrainDocument.Visible = False
                lblEmpTrainDocument.Visible = False
            End If

            ' Query to find the trainers associated with the selected items and display it
            Dim trainerQuery As List(Of String) = _
                (From tq In db.tblTRAINER_QUALIFICATIONs() _
                 Join emp In db.tblEMPLOYEEs() _
                 On emp.EMPLOYEE_ID Equals tq.EMPLOYEE_ID _
                 Order By emp.LAST_NAME, emp.FIRST_NAME _
                 Where tq.MASTER_JOB_NUMBER = jobNum And _
                       tq.MACHINE_CODE = mCode And _
                       tq.JOB_TITLE_NUMBER = catNum _
                 Select FULL_NAME = emp.LAST_NAME + ", " + emp.FIRST_NAME).ToList()
            trainerQuery.Insert(0, MAKE_A_SELECTION)
            ddlEmpTrainTrainer.DataSource = trainerQuery
            ddlEmpTrainTrainer.DataBind()

            ' Query to see if this has already been completed and display the info if it exists
            Dim trainQuery = (From eq In db.tblEMPLOYEES_QUALIFIEDs() _
                              Join emp In db.tblEMPLOYEEs() _
                              On emp.EMPLOYEE_ID Equals eq.APPROVED_BY _
                              Select eq, FULL_NAME = emp.LAST_NAME + ", " + emp.FIRST_NAME _
                              Where eq.MASTER_JOB_NUMBER = jobNum And _
                                    eq.MACHINE_CODE = mCode And _
                                    eq.JOB_TITLE_NUMBER = catNum And _
                                    eq.EMPLOYEE_ID = eCode).FirstOrDefault()
            txtEmpTrainTrainerDate.Text = ""
            If (Not (trainQuery Is Nothing)) Then
                If (Not (ddlEmpTrainTrainer.Items.FindByText(trainQuery.FULL_NAME) Is Nothing)) Then
                    ddlEmpTrainTrainer.SelectedValue = trainQuery.FULL_NAME
                    Dim dt As DateTime
                    dt = trainQuery.eq.DATE_APPROVED.Value
                    txtEmpTrainTrainerDate.Text = dt.ToString("MM/dd/yyyy")
                End If
            End If

            ' Query to find the supervisors
            Dim superQuery As List(Of String) = _
                (From emp In db.tblEMPLOYEEs() _
                 Where emp.IS_SUPERVISOR = "Y" _
                 Order By emp.LAST_NAME, emp.FIRST_NAME _
                 Select FULL_NAME = emp.LAST_NAME + ", " + emp.FIRST_NAME).ToList()
            superQuery.Insert(0, MAKE_A_SELECTION)
            ddlEmpTrainSupervisor.DataSource = superQuery
            ddlEmpTrainSupervisor.DataBind()

            ' Query to see if this has already been completed and display the info if it exists
            Dim signoffQuery = (From eq In db.tblEMPLOYEES_QUALIFIEDs() _
                                Join emp In db.tblEMPLOYEEs() _
                                On emp.EMPLOYEE_ID Equals eq.SUPERVISOR _
                                Select eq, FULL_NAME = emp.LAST_NAME + ", " + emp.FIRST_NAME _
                                Where eq.MASTER_JOB_NUMBER = jobNum And _
                                      eq.MACHINE_CODE = mCode And _
                                      eq.JOB_TITLE_NUMBER = catNum And _
                                      eq.EMPLOYEE_ID = eCode).FirstOrDefault()
            txtEmpTrainSupervisorDate.Text = ""
            If (Not (signoffQuery Is Nothing)) Then
                If (Not (ddlEmpTrainSupervisor.Items.FindByText(signoffQuery.FULL_NAME) Is Nothing)) Then
                    ddlEmpTrainSupervisor.SelectedValue = signoffQuery.FULL_NAME
                    Dim dt As DateTime
                    dt = signoffQuery.eq.SUPERVISOR_APPROVED.Value
                    txtEmpTrainSupervisorDate.Text = dt.ToString("MM/dd/yyyy")
                End If
            End If

            udpEmployeeTrainingBottom.Visible = True
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: gridEmployeeTraining_RowDataBound
    '
    ' Purpose: When a row is bound to the gridview update the TRAINING_NUMBER
    '          label text and the STEP_COMPLETED checkbox state
    '
    ' ************************************************************************
    Protected Sub gridEmployeeTraining_RowDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs) Handles gridEmployeeTraining.RowDataBound
        Try
            If Not (e.Row.DataItem Is Nothing) Then
                Dim theRow = e.Row.DataItem

                Dim cb As CheckBox = CType(e.Row.FindControl("chbStepCompleted"), CheckBox)
                cb.Checked = CBool(DataBinder.GetPropertyValue(theRow, "STEP_COMPLETE"))

                Dim lbl As Label = CType(e.Row.FindControl("lblTrainingNumber"), Label)
                lbl.Text = DataBinder.GetPropertyValue(theRow, "TRAINING_NUMBER").ToString
            End If
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnEmpTrainDisplayForm_Click
    '
    ' Purpose: When a the DISPLAY FORM button is clicked display the training
    '          form based on the selected entries
    '
    ' ************************************************************************
    Protected Sub btnEmpTrainDisplayForm_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnEmpTrainDisplayForm.Click
        Try
            If (ddlEmpTrainJobTitle.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlEmpTrainMachine.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlEmpTrainCategory.SelectedValue().Equals(MAKE_A_SELECTION)) Then
                MessageBox("You must select a job, a machine, and a category before you can proceed.", "USER ERROR")
                Exit Sub
            End If

            ' Javascript causes the Training Form to be displayed
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub



    ' ************************************************************************
    '    Name: btnEmpTrainSelectAll_Click
    '
    ' Purpose: When the Employee Training view's Select All button is 
    '          clicked, select all the training code checkboxes
    '
    ' ************************************************************************
    Protected Sub btnEmpTrainSelectAll_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnEmpTrainSelectAll.Click
        ' Look at all of the checkboxes and check each one
        For Each row As GridViewRow In gridEmployeeTraining.Rows
            Dim cb As CheckBox = CType(row.FindControl("chbStepCompleted"), CheckBox)
            cb.Checked = True
        Next
    End Sub


    ' ************************************************************************
    '    Name: btnEmpTrainTrainerSignoff_Click
    '
    ' Purpose: When the Employee Training view's Trainer Signoff button is 
    '          clicked, add or update the info in the database
    '
    ' ************************************************************************
    Protected Sub btnEmpTrainTrainerSignoff_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnEmpTrainTrainerSignoff.Click
        Try
            If (ddlEmpTrainJobTitle.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlEmpTrainMachine.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlEmpTrainEmployee.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlEmpTrainCategory.SelectedValue().Equals(MAKE_A_SELECTION)) Then
                MessageBox("You must select an employee, a job, a machine, and a category before you can proceed.", "USER ERROR")
                Exit Sub
            End If
            If (gridEmployeeTraining.Rows.Count = 0) Then
                MessageBox("You must lookup the details before you can proceed.", "USER ERROR")
                Exit Sub
            End If
            If ((ddlEmpTrainTrainer.SelectedValue().Equals(MAKE_A_SELECTION)) Or _
                (txtEmpTrainTrainerPassword.Text.Trim.Length = 0)) Then
                MessageBox("You must select a trainer and specify their password before you can proceed.", "USER ERROR")
                Exit Sub
            End If

            ' Break full name into its individual parts for use in the query
            Dim pos As Integer = ddlEmpTrainTrainer.SelectedValue.IndexOf(",")
            Dim lastName As String = ddlEmpTrainTrainer.SelectedValue.Substring(0, pos)
            Dim firstName As String = ddlEmpTrainTrainer.SelectedValue.Substring(pos + 2)

            ' Lookup the trainer and verify the password is correct
            Dim trainerQuery As tblEMPLOYEE = _
                (From emps In db.tblEMPLOYEEs() _
                 Select emps _
                 Where emps.FIRST_NAME = firstName And _
                       emps.LAST_NAME = lastName).FirstOrDefault()
            If (trainerQuery.PASSWORD.Equals(txtEmpTrainTrainerPassword.Text.Trim) = False) Then
                MessageBox("The Trainer Password specified for " + firstName + " " + lastName + " is incorrect.", "USER ERROR")
                Exit Sub
            End If

            Dim trainerID As Integer = trainerQuery.EMPLOYEE_ID
            Dim jobNum As Integer = Integer.Parse(Session("SELECTED_JOB_NUMBER").ToString())
            Dim mCode As String = Session("SELECTED_MACHINE_CODE").ToString()
            Dim catNum As Integer = Integer.Parse(Session("SELECTED_CATEGORY_NUMBER").ToString())
            Dim eCode As Integer = Integer.Parse(Session("SELECTED_EMPLOYEE_ID").ToString())
            Dim allStepsChecked As Boolean = True

            ' Look at all of the checkboxes and update the database for each
            For Each row As GridViewRow In gridEmployeeTraining.Rows
                Dim cb As CheckBox = CType(row.FindControl("chbStepCompleted"), CheckBox)
                If (cb.Checked = False) Then
                    allStepsChecked = False
                End If

                ' Extract the training code number from the hidden column/label
                Dim lbl As Label = CType(row.FindControl("lblTrainingNumber"), Label)
                Dim tNum As Integer = Integer.Parse(lbl.Text)

                ' Update the qualifications table
                UpdateQualifications(jobNum, mCode, catNum, eCode, tNum, trainerID, cb.Checked)

                ' If this training code is not machine dependent, update the database
                ' for the other relavent machines too
                Dim tCodeQuery As tblTRAINING_CODE = _
                    (From tCode In db.tblTRAINING_CODEs _
                     Select tCode _
                     Where tCode.TRAINING_NUMBER = tNum).FirstOrDefault()
                If (tCodeQuery.MACHINE_DEPENDENT = "N") Then
                    ' Lookup the other machines this training code is valid for
                    Dim machQuery As List(Of String) = _
                        (From mach In db.tblJOB_MACHINE_MASTERs _
                         Where mach.TRAINING_NUMBER = tNum And _
                               mach.JOB_TITLE_NUMBER = catNum And _
                               mach.MACHINE_CODE <> mCode And _
                               mach.MASTER_JOB_NUMBER = jobNum _
                         Select mach.MACHINE_CODE Distinct).ToList()

                    ' Loop through all of the machines and update the records
                    For Each machine As String In machQuery
                        Dim theMachine As String = machine

                        ' Update the qualifications table
                        UpdateQualifications(jobNum, machine, catNum, eCode, tNum, trainerID, cb.Checked)
                    Next
                End If

                ' If this is SAFETY training, update the database for the other relavent jobs too
                If (Session("SELECTED_CATEGORY").ToString().Equals("SAFETY")) Then
                    ' Lookup the other jobs this training code is valid for
                    Dim jobQuery As List(Of Integer) = _
                        (From job In db.tblJOB_MACHINE_MASTERs _
                         Where job.TRAINING_NUMBER = tNum And _
                               job.JOB_TITLE_NUMBER = catNum And _
                               job.MACHINE_CODE = mCode And _
                               job.MASTER_JOB_NUMBER <> jobNum _
                         Select job.MASTER_JOB_NUMBER Distinct).ToList()

                    ' Loop through all of the machines and update the records
                    For Each job As Integer In jobQuery
                        Dim theJob As Integer = job

                        ' Update the qualifications table
                        UpdateQualifications(theJob, mCode, catNum, eCode, tNum, trainerID, cb.Checked)
                    Next
                End If
            Next

            ' If this is modifying a completed training record, mark it as a Form Change
            Dim formChange As Boolean = False
            If ((txtEmpTrainTrainerDate.Text.Length > 0) And (allStepsChecked = True)) Then
                Dim d As Date = Nothing
                Dim b As Boolean = Date.TryParse(txtEmpTrainTrainerDate.Text, d)
                If ((b = True) AndAlso ((Today() - d).TotalDays() > 1)) Then
                    formChange = True
                End If
            End If

            ' Record the trainer signoff
            UpdateTrainerSignoff(jobNum, mCode, catNum, eCode, trainerID, allStepsChecked)

            ' If this is SAFETY training, update the signoffs for the other relavent jobs too
            If (Session("SELECTED_CATEGORY").ToString().Equals("SAFETY")) Then
                ' Lookup the other jobs this training code is valid for
                Dim jobQuery As List(Of Integer) = _
                    (From job In db.tblJOB_MACHINE_MASTERs _
                     Where job.JOB_TITLE_NUMBER = catNum And _
                           job.MACHINE_CODE = mCode And _
                           job.MASTER_JOB_NUMBER <> jobNum _
                     Select job.MASTER_JOB_NUMBER Distinct).ToList()

                ' Loop through all of the machines and update the records
                For Each job As Integer In jobQuery
                    Dim theJob As Integer = job

                    ' Update the signoffs
                    UpdateTrainerSignoff(theJob, mCode, catNum, eCode, trainerID, allStepsChecked)
                Next
            End If

            ' E-mail everyone necessary that this person has completed training
            RaiseBpmEvent("TrainingComplete", firstName, lastName, mCode, eCode, allStepsChecked, formChange)

            ' Now that the database has been updated, refresh the display
            txtEmpTrainTrainerPassword.Text = ""
            btnEmpTrainLookup_Click(sender, e)
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnEmpTrainClear_Click
    '
    ' Purpose: When the Employee Training view's CLEAR button is clicked,
    '          blank the detail widgets
    '
    ' ************************************************************************
    Protected Sub btnEmpTrainClear_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnEmpTrainClear.Click
        Try
            ClearEmployeeTrainingFields()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub


    ' ************************************************************************
    '    Name: btnEmpTrainBack_Click
    '
    ' Purpose: When the Employee Training view's CLEAR button is clicked,
    '          blank the detail widgets
    '
    ' ************************************************************************
    Protected Sub btnEmpTrainBack_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnEmpTrainBack.Click
        Try
            If (Session("OJT_BACK") IsNot Nothing) Then
                Select Case Session("OJT_BACK").ToString()
                    Case "By Job"
                        MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(viewReportCompleteByJob)
                        PopulateReportCompleteByJobFields()

                    Case "By Employee"
                        MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(viewReportCompleteByEmployee)
                        PopulateReportCompleteByEmployeeFields()

                    Case "Waiting for Supervisor"
                        MultiView1.ActiveViewIndex = MultiView1.Views.IndexOf(viewReportWaitingForSupervisor)
                        PopulateReportWaitingForSupervisorFields()

                    Case Else
                        MessageBox("Unknown destination.", "SYSTEM ERROR")
                End Select

            End If
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnEmpTrainSupervisorSignoff_Click
    '
    ' Purpose: When the Employee Training view's Supervisor Signoff button is 
    '          clicked, add or update the info in the database
    '
    ' ************************************************************************
    Protected Sub btnEmpTrainSupervisorSignoff_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnEmpTrainSupervisorSignoff.Click
        Try
            If (ddlEmpTrainJobTitle.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlEmpTrainMachine.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlEmpTrainEmployee.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlEmpTrainCategory.SelectedValue().Equals(MAKE_A_SELECTION)) Then
                MessageBox("You must select an employee, a job, a machine, and a category before you can proceed.", "USER ERROR")
                Exit Sub
            End If
            If (gridEmployeeTraining.Rows.Count = 0) Then
                MessageBox("You must lookup the details before you can proceed.", "USER ERROR")
                Exit Sub
            End If
            If ((ddlEmpTrainTrainer.SelectedValue().Equals(MAKE_A_SELECTION)) Or _
                (txtEmpTrainTrainerDate.Text = "")) Then
                MessageBox("The trainer must signoff first.", "USER ERROR")
                Exit Sub
            End If
            If ((ddlEmpTrainSupervisor.SelectedValue().Equals(MAKE_A_SELECTION)) Or _
                (txtEmpTrainSupervisorPassword.Text.Trim.Length = 0)) Then
                MessageBox("You must select a supervisor and specify their password before you can proceed.", "USER ERROR")
                Exit Sub
            End If

            ' Look at all of the checkboxes to determine if the training is complete
            Dim allStepsChecked As Boolean = True
            For Each row As GridViewRow In gridEmployeeTraining.Rows
                Dim cb As CheckBox = CType(row.FindControl("chbStepCompleted"), CheckBox)
                If (cb.Checked = False) Then
                    allStepsChecked = False
                End If
            Next
            If (allStepsChecked = False) Then
                MessageBox("The supervisor can not sign off until training is complete.", "USER ERROR")
                Exit Sub
            End If

            ' Break full name into its individual parts for use in the query
            Dim pos As Integer = ddlEmpTrainSupervisor.SelectedValue.IndexOf(",")
            Dim lastName As String = ddlEmpTrainSupervisor.SelectedValue.Substring(0, pos)
            Dim firstName As String = ddlEmpTrainSupervisor.SelectedValue.Substring(pos + 2)

            ' Lookup the supervisor and verify the password is correct
            Dim empQuery As tblEMPLOYEE = _
                (From emps In db.tblEMPLOYEEs() _
                 Select emps _
                 Where emps.FIRST_NAME = firstName And _
                       emps.LAST_NAME = lastName).FirstOrDefault()
            If (empQuery.SUPERVISOR_PASSWORD.Equals(txtEmpTrainSupervisorPassword.Text.Trim) = False) Then
                MessageBox("The Supervisor Password specified for " + firstName + " " + lastName + " is incorrect.", "USER ERROR")
                Exit Sub
            End If

            ' If this is modifying a completed training record, mark it as a Form Change
            Dim formChange As Boolean = False
            If (txtEmpTrainSupervisorDate.Text.Length > 0) Then
                Dim d As Date = Nothing
                Dim b As Boolean = Date.TryParse(txtEmpTrainSupervisorDate.Text, d)
                If ((b = True) AndAlso ((Today() - d).TotalDays() > 1)) Then
                    formChange = True
                End If
            End If

            Dim supervisorID As Integer = empQuery.EMPLOYEE_ID
            Dim jobNum As Integer = Integer.Parse(Session("SELECTED_JOB_NUMBER").ToString())
            Dim mCode As String = Session("SELECTED_MACHINE_CODE").ToString()
            Dim catNum As Integer = Integer.Parse(Session("SELECTED_CATEGORY_NUMBER").ToString())
            Dim eCode As Integer = Integer.Parse(Session("SELECTED_EMPLOYEE_ID").ToString())

            ' Record the supervisor signoff
            UpdateSupervisorSignoff(jobNum, mCode, catNum, eCode, supervisorID, formChange)

            ' If this is SAFETY training, signoff for the other relavent jobs too
            If (Session("SELECTED_CATEGORY").ToString().Equals("SAFETY")) Then
                ' Lookup the other jobs this training code is valid for
                Dim jobQuery As List(Of Integer) = _
                    (From job In db.tblJOB_MACHINE_MASTERs _
                     Where job.JOB_TITLE_NUMBER = catNum And _
                           job.MACHINE_CODE = mCode And _
                           job.MASTER_JOB_NUMBER <> jobNum _
                     Select job.MASTER_JOB_NUMBER Distinct).ToList()

                ' Loop through all of the machines and update the records
                For Each job As Integer In jobQuery
                    Dim theJob As Integer = job

                    ' Update the signoffs
                    UpdateSupervisorSignoff(theJob, mCode, catNum, eCode, supervisorID, formChange)
                Next
            End If

            ' E-mail everyone necessary that this person has completed training
            RaiseBpmEvent("NotifyHumanResources", firstName, lastName, mCode, eCode, allStepsChecked, formChange)

            ' Now that the database has been updated, refresh the display
            txtEmpTrainSupervisorPassword.Text = ""
            btnEmpTrainLookup_Click(sender, e)
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: UpdateQualifications
    '
    ' Purpose: Helper method to update the EMPLOYEE Qualifications table
    '
    ' ************************************************************************
    Private Sub UpdateQualifications(ByVal jobNum As Integer, ByVal mCode As String, ByVal catNum As Integer, ByVal eCode As Integer, ByVal tNum As Integer, ByVal trainerId As Integer, ByVal checked As Boolean)
        Dim stepQuery As tblEMPLOYEE_QUALIFICATION = _
            (From aStep In db.tblEMPLOYEE_QUALIFICATIONs() _
             Select aStep _
             Where aStep.MASTER_JOB_NUMBER = jobNum And _
                   aStep.MACHINE_CODE = mCode And _
                   aStep.JOB_TITLE_NUMBER = catNum And _
                   aStep.EMPLOYEE_ID = eCode And _
                   aStep.TRAINING_NUMBER = tNum).FirstOrDefault()

        ' If there is no record and the checkbox is checked then add a record
        If (stepQuery Is Nothing) And (checked = True) Then
            Dim newStep As New tblEMPLOYEE_QUALIFICATION With { _
                .EMPLOYEE_ID = eCode, _
                .MASTER_JOB_NUMBER = jobNum, _
                .JOB_TITLE_NUMBER = catNum, _
                .MACHINE_CODE = mCode, _
                .TRAINING_NUMBER = tNum, _
                .APPROVED_BY = trainerId, _
                .DATE_APPROVED = Now()}
            db.tblEMPLOYEE_QUALIFICATIONs().InsertOnSubmit(newStep)
        ElseIf (Not (stepQuery Is Nothing) And (checked = True)) Then
            ' If a record exists and the checkbox is checked then do nothing
            ' because we want to maintain the previous training date
        ElseIf (Not (stepQuery Is Nothing) And (checked = False)) Then
            ' If a record exists and the checkbox is not checked then delete the record
            db.tblEMPLOYEE_QUALIFICATIONs().DeleteOnSubmit(stepQuery)
        End If

        ' TPL - 10-FEB-2016 - Commit changes
        db.SubmitChanges()
    End Sub


    ' ************************************************************************
    '    Name: UpdateTrainerSignoff
    '
    ' Purpose: Helper method to update the Signoff in the tblEMPLOYEES_QUALIFIED table
    '
    ' ************************************************************************
    Private Sub UpdateTrainerSignoff(ByVal jobNum As Integer, ByVal mCode As String, ByVal catNum As Integer, ByVal eCode As Integer, ByVal trainerID As Integer, ByVal allStepsChecked As Boolean)

        ' Look in the table to see if this is an UPDATE or an ADD
        Dim etQuery As tblEMPLOYEES_QUALIFIED = _
            (From et In db.tblEMPLOYEES_QUALIFIEDs() _
             Select et _
             Where et.MASTER_JOB_NUMBER = jobNum And _
                   et.MACHINE_CODE = mCode And _
                   et.JOB_TITLE_NUMBER = catNum And _
                   et.EMPLOYEE_ID = eCode).FirstOrDefault()

        ' If there is no record and all steps were checked then add a record
        If (etQuery Is Nothing) And (allStepsChecked = True) Then
            Dim newQual As New tblEMPLOYEES_QUALIFIED With { _
                .MASTER_JOB_NUMBER = jobNum, _
                .MACHINE_CODE = mCode, _
                .JOB_TITLE_NUMBER = catNum, _
                .EMPLOYEE_ID = eCode, _
                .APPROVED_BY = trainerID, _
                .DATE_APPROVED = Now()}
            db.tblEMPLOYEES_QUALIFIEDs().InsertOnSubmit(newQual)
        ElseIf (Not (etQuery Is Nothing)) And (allStepsChecked = True) Then
            ' If a record exists and all steps were checked then update the record
            etQuery.APPROVED_BY = trainerID
            etQuery.DATE_APPROVED = Now()
        ElseIf (Not (etQuery Is Nothing) And (allStepsChecked = False)) Then
            ' If a record exists and the checkbox is not checked then delete the record
            db.tblEMPLOYEES_QUALIFIEDs().DeleteOnSubmit(etQuery)
        End If
        db.SubmitChanges()
    End Sub


    ' ************************************************************************
    '    Name: UpdateSupervisorSignoff
    '
    ' Purpose: Helper method to update the Signoff in the tblEMPLOYEES_QUALIFIED table
    '
    ' ************************************************************************
    Private Sub UpdateSupervisorSignoff(ByVal jobNum As Integer, ByVal mCode As String, ByVal catNum As Integer, ByVal eCode As Integer, ByVal supervisorID As Integer, ByVal formChange As Boolean)

        ' Look in the table to see if this is an UPDATE or an ADD
        Dim etQuery As tblEMPLOYEES_QUALIFIED = _
            (From et In db.tblEMPLOYEES_QUALIFIEDs() _
             Select et _
             Where et.MASTER_JOB_NUMBER = jobNum And _
                   et.MACHINE_CODE = mCode And _
                   et.JOB_TITLE_NUMBER = catNum And _
                   et.EMPLOYEE_ID = eCode).FirstOrDefault()

        ' If there is no record display an error
        If (etQuery Is Nothing) Then
            MessageBox("The trainer must signoff before the supervisor.", "USER ERROR")
            Exit Sub
        Else
            ' If a record exists and all steps were checked then update the record
            etQuery.SUPERVISOR = supervisorID
            etQuery.SUPERVISOR_APPROVED = Now()
            etQuery.FORM_CHANGE = formChange
            db.SubmitChanges()
        End If
    End Sub


    ' ************************************************************************
    '    Name: ClearEmployeeTrainingFields
    '
    ' Purpose: Helper method to blank the employee training view's detail widgets
    '
    ' ************************************************************************
    Private Sub ClearEmployeeTrainingFields()
        ddlEmpTrainEmployee.SelectedIndex = -1
        ddlEmpTrainJobTitle.SelectedIndex = -1
        ddlEmpTrainMachine.SelectedIndex = -1
        ddlEmpTrainCategory.SelectedIndex = -1
        ddlEmpTrainTrainer.SelectedIndex = -1
        txtEmpTrainTrainerPassword.Text = ""
        txtEmpTrainTrainerDate.Text = ""

        gridEmployeeTraining.DataSource = Nothing
        gridEmployeeTraining.DataBind()
        ddlEmpTrainTrainer.DataSource = Nothing
        ddlEmpTrainTrainer.DataBind()

        Session("SELECTED_JOB_NUMBER") = 0
        Session("SELECTED_JOB") = ""
        Session("SELECTED_EMPLOYEE") = ""
        Session("SELECTED_EMPLOYEE_ID") = 0
        Session("SELECTED_CATEGORY_NUMBER") = 0
        Session("SELECTED_CATEGORY") = ""
        Session("SELECTED_MACHINE") = ""
        Session("SELECTED_MACHINE_CODE") = ""
        Session("INCLUDE_INACTIVE_EMPLOYEES") = False

        lnkEmpTrainDocument.Visible = False
        lblEmpTrainDocument.Visible = False

        udpEmployeeTrainingBottom.Visible = False
    End Sub


    ' ************************************************************************
    '    Name: RaiseBpmEvent
    '
    ' Purpose: Helper method to trigger the BPM event specified
    '
    ' ************************************************************************
    Private Sub RaiseBpmEvent(ByVal eventName As String, ByVal firstName As String, ByVal lastName As String, ByVal mCode As String, ByVal eCode As Integer, ByVal allStepsChecked As Boolean, ByVal formChange As Boolean)
        ' Lookup the employee data
        Dim empQuery As tblEMPLOYEE = _
            (From emps In db.tblEMPLOYEEs() _
             Select emps _
             Where emps.EMPLOYEE_ID = eCode).FirstOrDefault()
        If (empQuery Is Nothing) Then
            MessageBox("The employee data for " + firstName + " " + lastName + " could not be retrieved.", "SYSTEM ERROR")
            Exit Sub
        End If

        ' Prepend the Form Change string if necessary
        Dim formChangeString As String = "NEW:"
        If (formChange = True) Then
            formChangeString = "FORM CHANGE:"
        End If
        ' Lookup the employee's subervisor
        Dim superQuery As tblEMPLOYEE = _
            (From emps In db.tblEMPLOYEEs() _
             Select emps _
             Where emps.EMPLOYEE_ID = empQuery.SUPERVISOR_ID).FirstOrDefault()

        ' If the supervisor is valid and all the steps are checked send a notification to the supervisor
        If (Not (superQuery Is Nothing)) And (allStepsChecked = True) Then
            Dim wf As New WorkFlow.EventEngineWebServiceSoapClient
            Dim WF_Array As New WorkFlow.ArrayOfString

            WF_Array.Add("ACTON")
            WF_Array.Add(eventName)
            WF_Array.Add("TJOLY")
            WF_Array.Add("<KeyData>" & _
                         "<FORM_CHANGE>" & formChangeString & "</FORM_CHANGE>" & _
                         "<TRAINEE_NAME>" & ddlEmpTrainEmployee.SelectedValue() & "</TRAINEE_NAME>" & _
                         "<TRAINER_NAME>" & ddlEmpTrainTrainer.SelectedValue() & "</TRAINER_NAME>" & _
                         "<SUPERVISOR_EMAIL>" & superQuery.EMAIL & "</SUPERVISOR_EMAIL>" & _
                         "<JOB_NAME>" & ddlEmpTrainJobTitle.SelectedValue() & "</JOB_NAME>" & _
                         "<CATEGORY_NAME>" & ddlEmpTrainCategory.SelectedValue & "</CATEGORY_NAME>" & _
                         "<MACHINE_NAME>" & mCode & "</MACHINE_NAME>" & _
                         "<TRAINING_DATE>" & Now().ToString("MM/dd/yyyy") & "</TRAINING_DATE>" & _
                         "<DOCUMENT_LINK>http://haartznet/OJT2.0/OJT.aspx</DOCUMENT_LINK>" & _
                         "<EMPLOYEE_ID>" & eCode.ToString() & "</EMPLOYEE_ID>" & _
                         "<JOB_NUMBER>" & Session("SELECTED_JOB_NUMBER").ToString() & "</JOB_NUMBER>" & _
                         "<MACHINE_CODE>" & mCode & "</MACHINE_CODE>" & _
                         "<CATEGORY_NUMBER>" & Session("SELECTED_CATEGORY_NUMBER").ToString() & "</CATEGORY_NUMBER>" & _
                         "</KeyData>")

            wf.Open()
            wf.RaiseEventExternal2("iRen.Raise." & eventName, "ACTON", "en-US", WF_Array)
            wf.Close()
        End If
    End Sub
#End Region

#Region "Report - Training Complete by Job Methods"

    ' ************************************************************************
    '        The methods below are for REPORT - TRAINING COMPLETE BY JOB
    ' ************************************************************************



    ' ************************************************************************
    '    Name: viewReportCompleteByJob_PreRender
    '
    ' Purpose: Update the lists of jobs and machines before the view is displayed
    '
    ' ************************************************************************
    Protected Sub viewReportCompleteByJob_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles viewReportCompleteByJob.PreRender
        Try
            ' if the person is not logged in, hide the Inactive checkbox
            If (Session("CURRENT_LOGIN_ID") Is Nothing) Then
                chbReportCompleteByJobInactive.Visible = False
                lblReportCompleteInactive.Visible = False
            Else
                chbReportCompleteByJobInactive.Visible = True
                lblReportCompleteInactive.Visible = True
            End If

            ' Query the database to retrieve the job titles and bind them to the widget
            Dim jobTitleQuery As List(Of String) = _
                (From Titles In db.tblMASTER_JOB_TITLEs() _
                 Select Titles.MASTER_JOB_TITLE _
                 Order By MASTER_JOB_TITLE).ToList()
            jobTitleQuery.Insert(0, MAKE_A_SELECTION)
            ddlReportCompleteJobTitle.DataSource = jobTitleQuery
            ddlReportCompleteJobTitle.DataBind()
            ddlReportCompleteJobTitle.SelectedIndex = jobTitleQuery.IndexOf(Session("SELECTED_JOB").ToString())

            ' The list of machines depends on which job is selected
            Dim machQuery As List(Of String) = Nothing
            Dim jNum As Integer = Integer.Parse(Session("SELECTED_JOB_NUMBER").ToString())
            If (jNum <> 0) Then
                ' Query the database to retrieve the machine codes and bind them to the widget
                machQuery = _
                    (From mCodes In db.tblMACHINEs() _
                     Join jmm In db.tblJOB_MACHINE_MASTERs() _
                     On mCodes.MACHINE_CODE Equals jmm.MACHINE_CODE _
                     Order By mCodes.MACHINE_CODE _
                     Where jmm.MASTER_JOB_NUMBER = jNum _
                     Select mCodes.MACHINE_CODE Distinct).ToList()
                machQuery.Sort()
                machQuery.Insert(0, MAKE_A_SELECTION)
                ddlReportCompleteMachine.DataSource = machQuery
            Else
                machQuery = New List(Of String)
                machQuery.Insert(0, MAKE_A_SELECTION)
                ddlReportCompleteMachine.DataSource = machQuery
            End If
            ddlReportCompleteMachine.DataBind()
            ddlReportCompleteMachine.SelectedIndex = machQuery.IndexOf(Session("SELECTED_MACHINE").ToString())
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ddlReportCompleteJobTitle_SelectedIndexChanged
    '
    ' Purpose: When a job is selected, store the selection
    '
    ' ************************************************************************
    Protected Sub ddlReportCompleteJobTitle_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlReportCompleteJobTitle.SelectedIndexChanged
        Try
            ' Query to retrieve the unique index
            Dim jobTitleQuery As Integer = _
                (From Titles In db.tblMASTER_JOB_TITLEs() _
                 Where Titles.MASTER_JOB_TITLE = ddlReportCompleteJobTitle.SelectedValue() _
                 Select Titles.MASTER_JOB_NUMBER).FirstOrDefault()

            Session("SELECTED_JOB_NUMBER") = jobTitleQuery
            Session("SELECTED_JOB") = ddlReportCompleteJobTitle.SelectedValue

            ' Reset widgets that rely on this value
            gridReportCompleteByJob.DataSource = Nothing
            gridReportCompleteByJob.DataBind()
            ddlReportCompleteMachine.SelectedIndex = 0
            Session("SELECTED_MACHINE") = ""
            Session("SELECTED_MACHINE_CODE") = ""
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ddlReportCompleteMachine_SelectedIndexChanged
    '
    ' Purpose: When a machine is selected, store the selection
    '
    ' ************************************************************************
    Protected Sub ddlReportCompleteMachine_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlReportCompleteMachine.SelectedIndexChanged
        Try
            Session("SELECTED_MACHINE") = ddlReportCompleteMachine.SelectedValue
            Session("SELECTED_MACHINE_CODE") = ddlReportCompleteMachine.SelectedValue

            ' Reset widgets that rely on this value
            gridReportCompleteByJob.DataSource = Nothing
            gridReportCompleteByJob.DataBind()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnReportCompleteByJobGenerate_Click
    '
    ' Purpose: When the report complete view's GENERATE button is clicked,
    '          query the database and generate the report
    '
    ' ************************************************************************
    Protected Sub btnReportCompleteByJobGenerate_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnReportCompleteByJobGenerate.Click

        Try
            If (ddlReportCompleteJobTitle.SelectedValue().Equals(MAKE_A_SELECTION) Or _
                ddlReportCompleteMachine.SelectedValue().Equals(MAKE_A_SELECTION)) Then
                MessageBox("You must select a job and a machine before you can proceed.", "USER ERROR")
                Exit Sub
            End If

            PopulateReportCompleteByJobFields()

        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnReportCompleteByJobClear_Click
    '
    ' Purpose: When the report complete view's CLEAR button is clicked,
    '          blank the detail widgets
    '
    ' ************************************************************************
    Protected Sub btnReportCompleteByJobClear_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnReportCompleteByJobClear.Click
        Try
            ClearReportCompleteByJobFields()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub


    ' ************************************************************************
    '    Name: btnReportCompleteByJobPrint_Click
    '
    ' Purpose: When the report complete view's PRINT button is clicked,
    '          generate an Excel Sheet from the grid
    '
    ' ************************************************************************
    Protected Sub btnReportCompleteByJobPrint_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnReportCompleteByJobPrint.Click

        Try
            PrintExcelGrid(gridReportCompleteByJob, "Training Report for " & Session("SELECTED_JOB").ToString() & "/" & Session("SELECTED_MACHINE").ToString())
        Catch ex As System.Threading.ThreadAbortException
            'No Action
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ClearReportCompleteByJobFields
    '
    ' Purpose: Helper method to blank the report complete view's detail widgets
    '
    ' ************************************************************************
    Private Sub ClearReportCompleteByJobFields()
        ddlReportCompleteJobTitle.SelectedIndex = -1
        ddlReportCompleteMachine.SelectedIndex = -1

        Session("SELECTED_JOB_NUMBER") = 0
        Session("SELECTED_JOB") = ""
        Session("SELECTED_MACHINE") = ""
        Session("SELECTED_MACHINE_CODE") = ""

        gridReportCompleteByJob.DataSource = Nothing
        gridReportCompleteByJob.DataBind()

        Session("OJT_BACK") = "By Job"
    End Sub


    ' ************************************************************************
    '    Name: PopulateReportCompleteByJobFields
    '
    ' Purpose: Helper method to populate the report complete view's detail widgets
    '
    ' ************************************************************************
    Private Sub PopulateReportCompleteByJobFields()
        ' Query to find the selected job
        Dim jobNum As Integer = Integer.Parse(Session("SELECTED_JOB_NUMBER").ToString())
        Dim mCode As String = Session("SELECTED_MACHINE_CODE").ToString()
        Dim reportData As ArrayList = New ArrayList()

        ' Retrieve the categories
        Dim catQuery As List(Of tblJOB_TITLE) = _
            (From cats In db.tblJOB_TITLEs _
             Select cats _
             Order By cats.JOB_TITLE).ToList()

        ' Loop Through the categories and lookup the employees
        For Each category As tblJOB_TITLE In catQuery
            Dim theCat As tblJOB_TITLE = category

            ' Retrieve the total number of training steps for this job, machine, and category
            Dim countQuery = (From jmm In db.tblJOB_MACHINE_MASTERs _
                              Where jmm.MASTER_JOB_NUMBER = jobNum And _
                                    jmm.MACHINE_CODE = mCode And _
                                    jmm.JOB_TITLE_NUMBER = theCat.JOB_TITLE_NUMBER _
                              Group By jmm.JOB_TITLE_NUMBER _
                              Into Count()).FirstOrDefault()

            ' Retrieve the employees that have been trained for this job, machine, and category
            Dim trainQuery = (From qual In db.tblEMPLOYEE_QUALIFICATIONs _
                              Where qual.JOB_TITLE_NUMBER = theCat.JOB_TITLE_NUMBER And _
                                    qual.MACHINE_CODE = mCode And _
                                    qual.MASTER_JOB_NUMBER = jobNum _
                              Group By qual.EMPLOYEE_ID _
                              Into Count() _
                              Order By Count Descending).ToList()

            ' loop through all the employees and display if they're fully trained
            For Each train In trainQuery
                Dim theTraining = train

                'Get the current employee's name
                Dim employee = (From emp In db.tblEMPLOYEEs _
                                Where emp.EMPLOYEE_ID = theTraining.EMPLOYEE_ID _
                                Select emp).FirstOrDefault()

                ' Determine if the employee is inactive and display only if indicated
                If (employee.ACTIVE_STATUS <> "A") And (chbReportCompleteByJobInactive.Checked = False) Then
                    Continue For
                End If

                ' Determine if the training is complete
                Dim status As String = "Unknown"
                If (theTraining.Count = countQuery.Count) Then
                    ' Determine if signoffs are complete
                    Dim signoffQuery As tblEMPLOYEES_QUALIFIED = (From q In db.tblEMPLOYEES_QUALIFIEDs _
                                                                  Where q.JOB_TITLE_NUMBER = theCat.JOB_TITLE_NUMBER And _
                                                                        q.MACHINE_CODE = mCode And _
                                                                        q.MASTER_JOB_NUMBER = jobNum And _
                                                                        q.EMPLOYEE_ID = employee.EMPLOYEE_ID).FirstOrDefault()
                    If ((signoffQuery Is Nothing) OrElse _
                        (signoffQuery.SUPERVISOR_APPROVED Is Nothing) OrElse _
                        (signoffQuery.DATE_APPROVED > signoffQuery.SUPERVISOR_APPROVED)) Then
                        status = "WAITING"
                    Else
                        status = "COMPLETE"
                    End If
                ElseIf (theTraining.Count < countQuery.Count) Then
                    status = "PARTIAL"
                Else
                    status = "COMPLETE"
                    MessageBox("Too many training records for EMPLOYEE " & theTraining.EMPLOYEE_ID & ": " & _
                        jobNum & "," & _
                        mCode & "," & _
                        theCat.JOB_TITLE_NUMBER & " is invalid.", _
                        "DATABASE ERROR")
                End If
                Dim webAddress As String = Request.Url.Scheme & "://" & Request.Url.Authority & Request.Url.LocalPath
                Dim newRecord = New ReportComplete(employee.LAST_NAME + ", " + employee.FIRST_NAME, theCat.JOB_TITLE, status, _
                    webAddress & "?sc=et&ed=" + theTraining.EMPLOYEE_ID.ToString() + "&jn=" + jobNum.ToString() + "&mc=" + mCode + "&cn=" + theCat.JOB_TITLE_NUMBER.ToString())
                reportData.Add(newRecord)
            Next
        Next

        ' If there are no training codes for the selected Job and Machine, then display a message
        If (reportData.Count = 0) Then
            MessageBox("There are no employees trained for the job and machine specified.", "NO DATA")
            Exit Sub
        End If

        Dim sorter As ReportCompleteSorter = New ReportCompleteSorter()
        reportData.Sort(sorter)
        gridReportCompleteByJob.DataSource = reportData
        gridReportCompleteByJob.DataBind()

    End Sub

#End Region

#Region "Report - Training Complete by Employee Methods"

    ' ************************************************************************
    '     The methods below are for REPORT - TRAINING COMPLETE BY EMPLOYEE
    ' ************************************************************************



    ' ************************************************************************
    '    Name: viewReportCompleteByEmployee_PreRender
    '
    ' Purpose: Update the lists of employees before the view is displayed
    '
    ' ************************************************************************
    Protected Sub viewReportCompleteByEmployee_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles viewReportCompleteByEmployee.PreRender
        Try
            ' Query the database to retrieve the employees and bind them to the widget
            Dim empQuery As List(Of String) = _
                (From Emps In db.tblEMPLOYEEs() _
                 Order By Emps.LAST_NAME, Emps.FIRST_NAME _
                 Where Emps.ACTIVE_STATUS = "A" _
                 Select FULL_NAME = Emps.LAST_NAME + ", " + Emps.FIRST_NAME).ToList()
            empQuery.Insert(0, MAKE_A_SELECTION)
            ddlReportCompleteEmployee.DataSource = empQuery
            ddlReportCompleteEmployee.DataBind()
            ddlReportCompleteEmployee.SelectedIndex = empQuery.IndexOf(Session("SELECTED_EMPLOYEE").ToString())

        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ddlReportCompleteEmployee_SelectedIndexChanged
    '
    ' Purpose: When an employee is selected, store the selection
    '
    ' ************************************************************************
    Protected Sub ddlReportCompleteEmployee_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlReportCompleteEmployee.SelectedIndexChanged
        Try
            ' Query to retrieve the unique index
            If (Not ddlReportCompleteEmployee.SelectedValue.Equals(MAKE_A_SELECTION)) Then
                ' Break full name into its individual parts for use in the query
                Dim pos As Integer = ddlReportCompleteEmployee.SelectedValue.IndexOf(",")
                Dim lastName As String = ddlReportCompleteEmployee.SelectedValue.Substring(0, pos)
                Dim firstName As String = ddlReportCompleteEmployee.SelectedValue.Substring(pos + 2)

                Dim empQuery As Integer = _
                    (From emps In db.tblEMPLOYEEs() _
                     Where emps.FIRST_NAME = firstName And emps.LAST_NAME = lastName _
                     Select emps.EMPLOYEE_ID).FirstOrDefault()

                Session("SELECTED_EMPLOYEE_ID") = empQuery
                Session("SELECTED_EMPLOYEE") = ddlReportCompleteEmployee.SelectedValue
            Else
                Session("SELECTED_EMPLOYEE_ID") = 0
                Session("SELECTED_EMPLOYEE") = ""
            End If

            ' Reset widgets that rely on this value
            gridReportCompleteByEmployee.DataSource = Nothing
            gridReportCompleteByEmployee.DataBind()
            gridReportCompleteByEmployee.Visible = False
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnReportCompleteByEmployeeGenerate_Click
    '
    ' Purpose: When the report complete view's GENERATE button is clicked,
    '          query the database and generate the report
    '
    ' ************************************************************************
    Protected Sub btnReportCompleteByEmployeeGenerate_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnReportCompleteByEmployeeGenerate.Click

        Try
            If (ddlReportCompleteEmployee.SelectedValue().Equals(MAKE_A_SELECTION)) Then
                MessageBox("You must select an employee before you can proceed.", "USER ERROR")
                Exit Sub
            End If

            PopulateReportCompleteByEmployeeFields()

        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnReportCompleteByEmployeeClear_Click
    '
    ' Purpose: When the report complete view's CLEAR button is clicked,
    '          blank the detail widgets
    '
    ' ************************************************************************
    Protected Sub btnReportCompleteByEmployeeClear_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnReportCompleteByEmployeeClear.Click
        Try
            ClearReportCompleteByEmployeeFields()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub


    ' ************************************************************************
    '    Name: btnReportCompleteByEmployeePrint_Click
    '
    ' Purpose: When the report complete view's PRINT button is clicked,
    '          generate an Excel Sheet from the grid
    '
    ' ************************************************************************
    Protected Sub btnReportCompleteByEmployeePrint_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnReportCompleteByEmployeePrint.Click

        Try
            PrintExcelGrid(gridReportCompleteByEmployee, "Training Report for " & Session("SELECTED_EMPLOYEE").ToString())
        Catch ex As System.Threading.ThreadAbortException
            'No Action
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub


    ' ************************************************************************
    '    Name: ClearReportCompleteByEmployeeFields
    '
    ' Purpose: Helper method to blank the report complete view's detail widgets
    '
    ' ************************************************************************
    Private Sub ClearReportCompleteByEmployeeFields()
        ddlReportCompleteEmployee.SelectedIndex = -1

        Session("SELECTED_EMPLOYEE_ID") = 0
        Session("SELECTED_EMPLOYEE") = ""

        gridReportCompleteByEmployee.DataSource = Nothing
        gridReportCompleteByEmployee.DataBind()
        gridReportCompleteByEmployee.Visible = False

        Session("OJT_BACK") = "By Employee"
    End Sub


    ' ************************************************************************
    '    Name: PopulateReportCompleteByEmployeeFields
    '
    ' Purpose: Helper method to populate the report complete view's detail widgets
    '
    ' ************************************************************************
    Private Sub PopulateReportCompleteByEmployeeFields()
        ' Query to find the selected employee
        Dim eCode As Integer = Integer.Parse(Session("SELECTED_EMPLOYEE_ID").ToString())
        Dim reportData As ArrayList = New ArrayList()

        ' Retrieve the jobs, machines, and categories that this employee has partially completed
        Dim partialQuery = (From qual In db.tblEMPLOYEE_QUALIFICATIONs _
                            Where qual.EMPLOYEE_ID = eCode _
                            Select qual.MASTER_JOB_NUMBER, qual.MACHINE_CODE, qual.JOB_TITLE_NUMBER Distinct).ToList()

        ' loop through all the records and display that they're partially trained
        For Each part In partialQuery
            Dim theTraining = part

            'Get the Job
            Dim theJob = (From job In db.tblMASTER_JOB_TITLEs _
                          Where job.MASTER_JOB_NUMBER = theTraining.MASTER_JOB_NUMBER _
                          Select job).FirstOrDefault()
            If (theJob Is Nothing) Then
                MessageBox("MASTER JOB NUMBER '" & theTraining.MASTER_JOB_NUMBER & "' is invalid.", _
                           "DATABASE ERROR")
                Continue For
            End If

            'Get the Machine
            Dim theMachine = (From mach In db.tblMACHINEs _
                             Where mach.MACHINE_CODE = theTraining.MACHINE_CODE _
                             Select mach).FirstOrDefault()
            If (theMachine Is Nothing) Then
                MessageBox("MACHINE CODE '" & theTraining.MACHINE_CODE & "' is invalid.", _
                           "DATABASE ERROR")
                Continue For
            End If

            'Get the Category
            Dim theCat = (From cat In db.tblJOB_TITLEs _
                          Where cat.JOB_TITLE_NUMBER = theTraining.JOB_TITLE_NUMBER _
                          Select cat).FirstOrDefault()
            If (theCat Is Nothing) Then
                MessageBox("JOB TITLE NUMBER '" & theTraining.JOB_TITLE_NUMBER & "' is invalid.", _
                           "DATABASE ERROR")
                Continue For
            End If

            ' Retrieve the total number of training steps for this job, machine, and category
            Dim countQuery = (From jmm In db.tblJOB_MACHINE_MASTERs _
                              Where jmm.MASTER_JOB_NUMBER = theTraining.MASTER_JOB_NUMBER And _
                                    jmm.MACHINE_CODE = theTraining.MACHINE_CODE And _
                                    jmm.JOB_TITLE_NUMBER = theTraining.JOB_TITLE_NUMBER _
                              Group By jmm.JOB_TITLE_NUMBER _
                              Into Count()).FirstOrDefault()
            If (countQuery Is Nothing) Then
                MessageBox("JOB MACHINE MASTER for " & _
                           theTraining.MASTER_JOB_NUMBER & "," & _
                           theTraining.MACHINE_CODE & "," & _
                           theTraining.JOB_TITLE_NUMBER & " is invalid.", _
                           "DATABASE ERROR")
                Continue For
            End If

            ' Retrieve the steps this employee has completed for this job, machine, and category
            Dim trainQuery = (From qual In db.tblEMPLOYEE_QUALIFICATIONs _
                              Where qual.JOB_TITLE_NUMBER = theTraining.JOB_TITLE_NUMBER And _
                                    qual.MACHINE_CODE = theTraining.MACHINE_CODE And _
                                    qual.MASTER_JOB_NUMBER = theTraining.MASTER_JOB_NUMBER And _
                                    qual.EMPLOYEE_ID = eCode _
                              Select qual).ToList()
            If (trainQuery Is Nothing) Then
                MessageBox("EMPLOYEE QUALIFICATIONS for " & _
                           theTraining.MASTER_JOB_NUMBER & "," & _
                           theTraining.MACHINE_CODE & "," & _
                           theTraining.JOB_TITLE_NUMBER & " is invalid.", _
                           "DATABASE ERROR")
                Continue For
            End If

            ' Determine if the training is complete
            Dim status As String = "Unknown"
            If (trainQuery.Count = countQuery.Count) Then
                ' Determine if signoffs are complete
                Dim signoffQuery As tblEMPLOYEES_QUALIFIED = (From q In db.tblEMPLOYEES_QUALIFIEDs _
                                                              Where q.JOB_TITLE_NUMBER = theTraining.JOB_TITLE_NUMBER And _
                                                                    q.MACHINE_CODE = theTraining.MACHINE_CODE And _
                                                                    q.MASTER_JOB_NUMBER = theTraining.MASTER_JOB_NUMBER And _
                                                                    q.EMPLOYEE_ID = eCode).FirstOrDefault()
                If ((signoffQuery Is Nothing) OrElse _
                    (signoffQuery.SUPERVISOR_APPROVED Is Nothing) OrElse _
                    (signoffQuery.DATE_APPROVED > signoffQuery.SUPERVISOR_APPROVED)) Then
                    status = "WAITING"
                Else
                    status = "COMPLETE"
                End If
            ElseIf (trainQuery.Count < countQuery.Count) Then
                status = "PARTIAL"
            Else
                status = "COMPLETE"
                MessageBox("Too many training records for EMPLOYEE " & eCode & ": " & _
                    theTraining.MASTER_JOB_NUMBER & "," & _
                    theTraining.MACHINE_CODE & "," & _
                    theTraining.JOB_TITLE_NUMBER & " is invalid.", _
                    "DATABASE ERROR")
            End If
            Dim webAddress As String = Request.Url.Scheme & "://" & Request.Url.Authority & Request.Url.LocalPath
            Dim newRecord = New EmployeeComplete(theJob.MASTER_JOB_TITLE, theMachine.MACHINE_CODE, _
                                                 theCat.JOB_TITLE, status, _
                    webAddress & "?sc=et&ed=" + eCode.ToString() + "&jn=" + theJob.MASTER_JOB_NUMBER.ToString() + "&mc=" + theMachine.MACHINE_CODE + "&cn=" + theCat.JOB_TITLE_NUMBER.ToString())
            reportData.Add(newRecord)
        Next

        Dim sorter As EmployeeCompleteSorter = New EmployeeCompleteSorter()
        reportData.Sort(sorter)
        gridReportCompleteByEmployee.DataSource = reportData
        gridReportCompleteByEmployee.DataBind()
        gridReportCompleteByEmployee.Visible = True
    End Sub

#End Region

#Region "Report - Incomplete Training"

    ' ************************************************************************
    '          The methods below are for REPORT - INCOMPLETE TRAINING
    ' ************************************************************************

    ' ************************************************************************
    '    Name: viewReportIncompleteTraining_PreRender
    '
    ' Purpose: Update the list of supervisors before the view is displayed
    '
    ' ************************************************************************
    Protected Sub viewReportIncompleteTraining_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles viewReportIncompleteTraining.PreRender
        Try
            ' Query the database to retrieve the supervisors and bind them to the widget
            Dim superQuery As List(Of String) = _
                (From supers In db.tblEMPLOYEEs() _
                 Where supers.IS_SUPERVISOR = "Y" _
                 Order By supers.LAST_NAME, supers.FIRST_NAME _
                 Select FULL_NAME = supers.LAST_NAME + ", " + supers.FIRST_NAME).ToList()
            superQuery.Insert(0, MAKE_A_SELECTION)
            superQuery.Insert(1, "All Supervisors")
            ddlReportIncompleteSupervisor.DataSource = superQuery
            ddlReportIncompleteSupervisor.DataBind()
            ddlReportIncompleteSupervisor.SelectedIndex = superQuery.IndexOf(Session("SELECTED_SUPERVISOR").ToString())

            ddlReportIncompleteReport.SelectedIndex = Integer.Parse(Session("SELECTED_REPORT").ToString)

        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ddlReportIncompleteSupervisor_SelectedIndexChanged
    '
    ' Purpose: When a supervisor is selected, store the selection
    '
    ' ************************************************************************
    Protected Sub ddlReportIncompleteSupervisor_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlReportIncompleteSupervisor.SelectedIndexChanged
        Try
            ' Query to retrieve the unique index
            If (Not ddlReportIncompleteSupervisor.SelectedValue.Equals(MAKE_A_SELECTION)) Then

                If (Not ddlReportIncompleteSupervisor.SelectedValue.Equals("All Supervisors")) Then
                    ' Break full name into its individual parts for use in the query
                    Dim pos As Integer = ddlReportIncompleteSupervisor.SelectedValue.IndexOf(",")
                    Dim lastName As String = ddlReportIncompleteSupervisor.SelectedValue.Substring(0, pos)
                    Dim firstName As String = ddlReportIncompleteSupervisor.SelectedValue.Substring(pos + 2)

                    ' Lookup the supervisor's ID
                    Dim superQuery As Integer = _
                        (From supers In db.tblEMPLOYEEs() _
                         Where supers.FIRST_NAME = firstName And supers.LAST_NAME = lastName _
                         Select supers.EMPLOYEE_ID).FirstOrDefault()
                    Session("SELECTED_SUPERVISOR_ID") = superQuery
                Else
                    Session("SELECTED_SUPERVISOR_ID") = ALL_SUPERVISORS_ID
                End If
                Session("SELECTED_SUPERVISOR") = ddlReportIncompleteSupervisor.SelectedValue
            Else
                Session("SELECTED_SUPERVISOR_ID") = 0
                Session("SELECTED_SUPERVISOR") = ""
            End If

            ' Reset widgets that rely on this value
            gridReportIncompleteTraining.DataSource = Nothing
            gridReportIncompleteTraining.DataBind()
            lblReportIncompleteTrainingNoEmp.Visible = False
            lblReportIncompleteTrainingHeading.Visible = False
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ddlReportIncompleteReport_SelectedIndexChanged
    '
    ' Purpose: When an report is selected, store the selection
    '
    ' ************************************************************************
    Protected Sub ddlReportIncompleteReport_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlReportIncompleteReport.SelectedIndexChanged
        Try
            ' Query to retrieve the unique index
            If (Not ddlReportIncompleteReport.SelectedValue.Equals(MAKE_A_SELECTION)) Then
                Session("SELECTED_REPORT") = ddlReportIncompleteReport.SelectedIndex
            Else
                Session("SELECTED_REPORT") = -1
            End If

            ' Reset widgets that rely on this value
            gridReportIncompleteTraining.DataSource = Nothing
            gridReportIncompleteTraining.DataBind()
            lblReportIncompleteTrainingNoEmp.Visible = False
            lblReportIncompleteTrainingHeading.Visible = False
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub


    ' ************************************************************************
    '    Name: btnReportIncompleteTrainingGenerate_Click
    '
    ' Purpose: When the incomplete training view's GENERATE button is clicked,
    '          query the database and generate the report
    '
    ' ************************************************************************
    Protected Sub btnReportIncompleteTrainingGenerate_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnReportIncompleteTrainingGenerate.Click
        Try
            If (ddlReportIncompleteSupervisor.SelectedValue.Equals(MAKE_A_SELECTION)) Then
                MessageBox("You must select a supervisor before you can proceed.", "USER ERROR")
                Exit Sub
            End If

            If (ddlReportIncompleteReport.SelectedValue.Equals(MAKE_A_SELECTION)) Then
                MessageBox("You must select a report before you can proceed.", "USER ERROR")
                Exit Sub
            End If

            Dim superId As Integer = Integer.Parse(Session("SELECTED_SUPERVISOR_ID").ToString())

            Dim empQuery As List(Of tblEMPLOYEE) = Nothing
            If (superId <> ALL_SUPERVISORS_ID) Then
                ' Query for the specified supervisor's employees
                empQuery = (From emps In db.tblEMPLOYEEs() _
                            Select emps _
                            Where emps.ACTIVE_STATUS = "A" And _
                                  emps.SUPERVISOR_ID = superId _
                            Order By emps.LAST_NAME, emps.FIRST_NAME).ToList()
            Else
                ' Query for all supervisors' employees
                empQuery = (From emps In db.tblEMPLOYEEs() _
                            Select emps _
                            Where emps.ACTIVE_STATUS = "A" And _
                                  emps.SUPERVISOR_ID <> 0 _
                            Order By emps.SUPERVISOR_ID, emps.LAST_NAME, emps.FIRST_NAME).ToList()
            End If

            ' Get the report data based on the selection
            Dim headingLabel As String = ""
            Dim output As List(Of Incomplete) = Nothing
            If (ddlReportIncompleteReport.SelectedValue.Equals("No Training")) Then
                output = RetrieveNoTraining(empQuery)
                headingLabel = "Employees With No Training Started"
            ElseIf (ddlReportIncompleteReport.SelectedValue.Equals("Safety Incomplete")) Then
                output = RetrieveSafetyIncomplete(empQuery)
                headingLabel = "Employees With No Safety Training Complete"
            ElseIf (ddlReportIncompleteReport.SelectedValue.Equals("Operational Incomplete")) Then
                output = RetrieveOperationalIncomplete(empQuery)
                headingLabel = "Employees With No Operational Training Complete"
            End If

            ' Update the display
            Dim sorter As New IncompleteSorter
            output.Sort(sorter)

            gridReportIncompleteTraining.DataSource = output
            gridReportIncompleteTraining.DataBind()
            lblReportIncompleteTrainingNoEmp.Visible = (output.Count = 0)
            lblReportIncompleteTrainingHeading.Visible = True
            lblReportIncompleteTrainingHeading.Text = headingLabel

        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnReportIncompleteTrainingClear_Click
    '
    ' Purpose: When the incomplete training view's CLEAR button is clicked,
    '          blank the widgets
    '
    ' ************************************************************************
    Protected Sub btnReportIncompleteTrainingClear_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnReportIncompleteTrainingClear.Click
        Try
            ClearReportIncompleteTrainingFields()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ClearReportIncompleteTrainingFields
    '
    ' Purpose: Helper method to blank the incomplete training view's widgets
    '
    ' ************************************************************************
    Private Sub ClearReportIncompleteTrainingFields()
        Session("SELECTED_SUPERVISOR_ID") = 0
        Session("SELECTED_SUPERVISOR") = ""
        Session("SELECTED_REPORT") = -1

        gridReportIncompleteTraining.DataSource = Nothing
        gridReportIncompleteTraining.DataBind()

        lblReportIncompleteTrainingNoEmp.Visible = False
        lblReportIncompleteTrainingHeading.Visible = False
        ddlReportIncompleteSupervisor.SelectedIndex = -1
    End Sub


    ' ************************************************************************
    '    Name: btnReportIncompleteTrainingPrint_Click
    '
    ' Purpose: When the incomplete training view's PRINT button is clicked,
    '          generate an Excel Sheet from the grid
    '
    ' ************************************************************************
    Protected Sub btnReportIncompleteTrainingPrint_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnReportIncompleteTrainingPrint.Click

        Try
            PrintExcelGrid(gridReportIncompleteTraining, lblReportIncompleteTrainingHeading.Text)
        Catch ex As System.Threading.ThreadAbortException
            'No Action
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub


    ' ************************************************************************
    '    Name: RetrieveNoTraining
    '
    ' Purpose: Helper method to generate a list of employees who have 
    '          no training at all
    '
    ' ************************************************************************
    Private Function RetrieveNoTraining(ByVal empQuery As List(Of tblEMPLOYEE)) As List(Of Incomplete)
        Dim output As New List(Of Incomplete)

        ' Loop throught the list of employees
        For Each em As tblEMPLOYEE In empQuery

            Dim emp As tblEMPLOYEE = em

            ' Determine how many training steps have been completed
            Dim qualCount As Integer = _
                (From qual In db.tblEMPLOYEE_QUALIFICATIONs() _
                 Select qual _
                 Where qual.EMPLOYEE_ID = emp.EMPLOYEE_ID).Count()

            ' If no training steps have been completed add this employee to the display list
            If (qualCount = 0) Then
                ' Lookup the name of this employee's supervisor
                Dim superQuery As tblEMPLOYEE = _
                    (From sup In db.tblEMPLOYEEs() _
                     Select sup _
                     Where sup.EMPLOYEE_ID = emp.SUPERVISOR_ID).FirstOrDefault()

                ' Create the record and add it to the display list
                Dim record As New Incomplete(emp.LAST_NAME & ", " & emp.FIRST_NAME, _
                                             superQuery.LAST_NAME & ", " & superQuery.FIRST_NAME)
                output.Add(record)
            End If
        Next

        Return output
    End Function


    ' ************************************************************************
    '    Name: RetrieveSafetyIncomplete
    '
    ' Purpose: Helper method to generate a list of employees who have 
    '          incomplete SAFETY training
    '
    ' ************************************************************************
    Private Function RetrieveSafetyIncomplete(ByVal empQuery As List(Of tblEMPLOYEE)) As List(Of Incomplete)
        Dim output As New List(Of Incomplete)

        ' Lookup "SAFETY"
        Dim safety As Integer = _
            (From j In db.tblJOB_TITLEs() _
             Where j.JOB_TITLE_TYPE = "S" _
             Select j.JOB_TITLE_NUMBER).FirstOrDefault()

        ' Loop throught the list of employees
        For Each em As tblEMPLOYEE In empQuery

            Dim emp As tblEMPLOYEE = em

            ' Determine jobs with safety steps completed
            Dim safetyCount As Integer = _
                (From qual In db.tblEMPLOYEES_QUALIFIEDs() _
                 Where qual.EMPLOYEE_ID = emp.EMPLOYEE_ID And _
                       qual.JOB_TITLE_NUMBER = safety _
                 Select qual).Count()

            ' If safety steps have not been completed add this employee to the display list
            If (safetyCount = 0) Then
                ' Lookup the name of this employee's supervisor
                Dim superQuery As tblEMPLOYEE = _
                    (From sup In db.tblEMPLOYEEs() _
                     Select sup _
                     Where sup.EMPLOYEE_ID = emp.SUPERVISOR_ID).FirstOrDefault()

                ' Create the record and add it to the display list
                Dim record As New Incomplete(emp.LAST_NAME & ", " & emp.FIRST_NAME, _
                                             superQuery.LAST_NAME & ", " & superQuery.FIRST_NAME)

                output.Add(record)
            End If
        Next

        Return output
    End Function


    ' ************************************************************************
    '    Name: RetrieveOperationalIncomplete
    '
    ' Purpose: Helper method to generate a list of employees who have 
    '          completed SAFETY training, but have not completed the 
    '          associated OPERATIONAL training
    '
    ' ************************************************************************
    Private Function RetrieveOperationalIncomplete(ByVal empQuery As List(Of tblEMPLOYEE)) As List(Of Incomplete)
        Dim output As New List(Of Incomplete)

        ' Lookup "OPERATIONAL"
        Dim operational As Integer = _
            (From j In db.tblJOB_TITLEs() _
             Where j.JOB_TITLE_TYPE = "O" _
             Select j.JOB_TITLE_NUMBER).FirstOrDefault()

        ' Loop throught the list of employees
        For Each em As tblEMPLOYEE In empQuery

            Dim emp As tblEMPLOYEE = em

            ' Determine jobs with safety completed
            Dim operationalCount As Integer = _
                (From qual In db.tblEMPLOYEES_QUALIFIEDs() _
                 Where qual.EMPLOYEE_ID = emp.EMPLOYEE_ID And _
                       qual.JOB_TITLE_NUMBER = operational _
                 Select qual).Count()

            ' If operational jobs have not been completed add this employee to the display list
            If (operationalCount = 0) Then
                ' Lookup the name of this employee's supervisor
                Dim superQuery As tblEMPLOYEE = _
                    (From sup In db.tblEMPLOYEEs() _
                     Select sup _
                     Where sup.EMPLOYEE_ID = emp.SUPERVISOR_ID).FirstOrDefault()

                ' Create the record and add it to the display list
                Dim record As New Incomplete(emp.LAST_NAME & ", " & emp.FIRST_NAME, _
                                             superQuery.LAST_NAME & ", " & superQuery.FIRST_NAME)
                output.Add(record)
            End If
        Next

        Return output
    End Function

#End Region

#Region "Report - Training Waiting For Supervisor"


    ' ************************************************************************
    '        The methods below are for REPORT - WAITING FOR SUPERVISOR
    ' ************************************************************************

    ' ************************************************************************
    '    Name: viewReportWaitingForSupervisor_PreRender
    '
    ' Purpose: Update the list of supervisors before the view is displayed
    '
    ' ************************************************************************
    Protected Sub viewReportWaitingForSupervisor_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles viewReportWaitingForSupervisor.PreRender
        Try
            ' Query the database to retrieve the supervisors and bind them to the widget
            Dim superQuery As List(Of String) = _
                (From supers In db.tblEMPLOYEEs() _
                 Where supers.IS_SUPERVISOR = "Y" _
                 Order By supers.LAST_NAME, supers.FIRST_NAME _
                 Select FULL_NAME = supers.LAST_NAME + ", " + supers.FIRST_NAME).ToList()
            superQuery.Insert(0, MAKE_A_SELECTION)
            superQuery.Insert(1, "All Supervisors")
            ddlReportWaitingForSupervisor.DataSource = superQuery
            ddlReportWaitingForSupervisor.DataBind()
            ddlReportWaitingForSupervisor.SelectedIndex = superQuery.IndexOf(Session("SELECTED_SUPERVISOR").ToString())

        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ddlReportWaitingForSupervisor_SelectedIndexChanged
    '
    ' Purpose: When a supervisor is selected, store the selection
    '
    ' ************************************************************************
    Protected Sub ddlReportWaitingForSupervisor_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlReportWaitingForSupervisor.SelectedIndexChanged
        Try
            ' Query to retrieve the unique index
            If (Not ddlReportWaitingForSupervisor.SelectedValue.Equals(MAKE_A_SELECTION)) Then

                If (Not ddlReportWaitingForSupervisor.SelectedValue.Equals("All Supervisors")) Then
                    ' Break full name into its individual parts for use in the query
                    Dim pos As Integer = ddlReportWaitingForSupervisor.SelectedValue.IndexOf(",")
                    Dim lastName As String = ddlReportWaitingForSupervisor.SelectedValue.Substring(0, pos)
                    Dim firstName As String = ddlReportWaitingForSupervisor.SelectedValue.Substring(pos + 2)

                    ' Lookup the supervisor's ID
                    Dim superQuery As Integer = _
                        (From supers In db.tblEMPLOYEEs() _
                         Where supers.FIRST_NAME = firstName And supers.LAST_NAME = lastName _
                         Select supers.EMPLOYEE_ID).FirstOrDefault()
                    Session("SELECTED_SUPERVISOR_ID") = superQuery
                Else
                    Session("SELECTED_SUPERVISOR_ID") = ALL_SUPERVISORS_ID
                End If
                Session("SELECTED_SUPERVISOR") = ddlReportWaitingForSupervisor.SelectedValue
            Else
                Session("SELECTED_SUPERVISOR_ID") = 0
                Session("SELECTED_SUPERVISOR") = ""
            End If

            ' Reset widgets that rely on this value
            gridReportWaitingForSupervisor.DataSource = Nothing
            gridReportWaitingForSupervisor.DataBind()
            lblReportWaitingForSupervisorNoEmp.Visible = False
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnReportWaitingForSupervisorGenerate_Click
    '
    ' Purpose: When the waiting for supervisor view's GENERATE button is clicked,
    '          query the database and generate the report
    '
    ' ************************************************************************
    Protected Sub btnReportWaitingForSupervisorGenerate_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnReportWaitingForSupervisorGenerate.Click
        Try
            If (ddlReportWaitingForSupervisor.SelectedValue.Equals(MAKE_A_SELECTION)) Then
                MessageBox("You must select a supervisor before you can proceed.", "USER ERROR")
                Exit Sub
            End If

            PopulateReportWaitingForSupervisorFields()

        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnReportWaitingForSupervisorClear_Click
    '
    ' Purpose: When the waiting for supervisor view's CLEAR button is clicked,
    '          blank the widgets
    '
    ' ************************************************************************
    Protected Sub btnReportWaitingForSupervisorClear_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnReportWaitingForSupervisorClear.Click
        Try
            ClearReportWaitingForSupervisorFields()
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: btnReportWaitingForSupervisorPrint_Click
    '
    ' Purpose: When the waiting for supervisor view's PRINT button is clicked,
    '          generate an Excel Sheet from the grid
    '
    ' ************************************************************************
    Protected Sub btnReportWaitingForSupervisorPrint_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnReportWaitingForSupervisorPrint.Click

        Try
            PrintExcelGrid(gridReportWaitingForSupervisor, ttlReportWaitingForSupervisor.Text)
        Catch ex As System.Threading.ThreadAbortException
            'No Action
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    ' ************************************************************************
    '    Name: ClearReportWaitingForSupervisorFields
    '
    ' Purpose: Helper method to blank the waiting for supervisor view's widgets
    '
    ' ************************************************************************
    Private Sub ClearReportWaitingForSupervisorFields()
        Session("SELECTED_SUPERVISOR_ID") = 0
        Session("SELECTED_SUPERVISOR") = ""

        gridReportWaitingForSupervisor.DataSource = Nothing
        gridReportWaitingForSupervisor.DataBind()
        lblReportWaitingForSupervisorNoEmp.Visible = False

        ddlReportWaitingForSupervisor.SelectedIndex = -1

        Session("OJT_BACK") = "Waiting for Supervisor"
    End Sub

    ' ************************************************************************
    '    Name: PopulateReportWaitingForSupervisorFields
    '
    ' Purpose: Helper method to populate the waiting for supervisor view's widgets
    '
    ' ************************************************************************
    Private Sub PopulateReportWaitingForSupervisorFields()

        Dim superId As Integer = Integer.Parse(Session("SELECTED_SUPERVISOR_ID").ToString())

        Dim empQuery As List(Of tblEMPLOYEE) = Nothing
        If (superId <> ALL_SUPERVISORS_ID) Then
            ' Query for the specified supervisor's employees
            empQuery = (From emps In db.tblEMPLOYEEs() _
                        Select emps _
                        Where emps.ACTIVE_STATUS = "A" And _
                              emps.SUPERVISOR_ID = superId _
                        Order By emps.LAST_NAME, emps.FIRST_NAME).ToList()
        Else
            ' Query for all supervisors' employees
            empQuery = (From emps In db.tblEMPLOYEEs() _
                        Select emps _
                        Where emps.ACTIVE_STATUS = "A" And _
                              emps.SUPERVISOR_ID <> 0 _
                        Order By emps.SUPERVISOR_ID, emps.LAST_NAME, emps.FIRST_NAME).ToList()
        End If

        ' Loop throught the list of employees
        Dim output As New List(Of Waiting)
        For Each em As tblEMPLOYEE In empQuery

            Dim emp As tblEMPLOYEE = em

            ' Retrieve the jobs, machines, and categories that this employee has partially completed
            Dim partialQuery = (From qual In db.tblEMPLOYEE_QUALIFICATIONs _
                                Where qual.EMPLOYEE_ID = emp.EMPLOYEE_ID _
                                Select qual.MASTER_JOB_NUMBER, qual.MACHINE_CODE, qual.JOB_TITLE_NUMBER Distinct).ToList()

            ' loop through all the records and display that they're partially trained
            For Each part In partialQuery
                Dim theTraining = part

                'Get the Job
                Dim theJob = (From job In db.tblMASTER_JOB_TITLEs _
                              Where job.MASTER_JOB_NUMBER = theTraining.MASTER_JOB_NUMBER _
                              Select job).FirstOrDefault()
                If (theJob Is Nothing) Then
                    MessageBox("MASTER JOB NUMBER '" & theTraining.MASTER_JOB_NUMBER & "' is invalid.", _
                               "DATABASE ERROR")
                    Continue For
                End If

                'Get the Machine
                Dim theMachine = (From mach In db.tblMACHINEs _
                                 Where mach.MACHINE_CODE = theTraining.MACHINE_CODE _
                                 Select mach).FirstOrDefault()
                If (theMachine Is Nothing) Then
                    MessageBox("MACHINE CODE '" & theTraining.MACHINE_CODE & "' is invalid.", _
                               "DATABASE ERROR")
                    Continue For
                End If

                'Get the Category
                Dim theCat = (From cat In db.tblJOB_TITLEs _
                              Where cat.JOB_TITLE_NUMBER = theTraining.JOB_TITLE_NUMBER _
                              Select cat).FirstOrDefault()
                If (theCat Is Nothing) Then
                    MessageBox("JOB TITLE NUMBER '" & theTraining.JOB_TITLE_NUMBER & "' is invalid.", _
                               "DATABASE ERROR")
                    Continue For
                End If

                ' Retrieve the total number of training steps for this job, machine, and category
                Dim countQuery = (From jmm In db.tblJOB_MACHINE_MASTERs _
                                  Where jmm.MASTER_JOB_NUMBER = theTraining.MASTER_JOB_NUMBER And _
                                        jmm.MACHINE_CODE = theTraining.MACHINE_CODE And _
                                        jmm.JOB_TITLE_NUMBER = theTraining.JOB_TITLE_NUMBER _
                                  Group By jmm.JOB_TITLE_NUMBER _
                                  Into Count()).FirstOrDefault()
                If (countQuery Is Nothing) Then
                    MessageBox("JOB MACHINE MASTER for " & _
                               theTraining.MASTER_JOB_NUMBER & "," & _
                               theTraining.MACHINE_CODE & "," & _
                               theTraining.JOB_TITLE_NUMBER & " is invalid.", _
                               "DATABASE ERROR")
                    Continue For
                End If

                ' Retrieve the steps this employee has completed for this job, machine, and category
                Dim trainQuery = (From qual In db.tblEMPLOYEE_QUALIFICATIONs _
                                  Where qual.JOB_TITLE_NUMBER = theTraining.JOB_TITLE_NUMBER And _
                                        qual.MACHINE_CODE = theTraining.MACHINE_CODE And _
                                        qual.MASTER_JOB_NUMBER = theTraining.MASTER_JOB_NUMBER And _
                                        qual.EMPLOYEE_ID = emp.EMPLOYEE_ID _
                                  Select qual).ToList()
                If (trainQuery Is Nothing) Then
                    MessageBox("EMPLOYEE QUALIFICATIONS for " & _
                               theTraining.MASTER_JOB_NUMBER & "," & _
                               theTraining.MACHINE_CODE & "," & _
                               theTraining.JOB_TITLE_NUMBER & " is invalid.", _
                               "DATABASE ERROR")
                    Continue For
                End If

                ' Determine if the training is complete
                If (trainQuery.Count = countQuery.Count) Then
                    ' Determine if signoffs are complete
                    Dim signoffQuery As tblEMPLOYEES_QUALIFIED = (From q In db.tblEMPLOYEES_QUALIFIEDs _
                                                                  Where q.JOB_TITLE_NUMBER = theTraining.JOB_TITLE_NUMBER And _
                                                                        q.MACHINE_CODE = theTraining.MACHINE_CODE And _
                                                                        q.MASTER_JOB_NUMBER = theTraining.MASTER_JOB_NUMBER And _
                                                                        q.EMPLOYEE_ID = emp.EMPLOYEE_ID).FirstOrDefault()
                    If ((signoffQuery Is Nothing) OrElse _
                        (signoffQuery.SUPERVISOR_APPROVED Is Nothing) OrElse _
                        (signoffQuery.DATE_APPROVED > signoffQuery.SUPERVISOR_APPROVED)) Then

                        ' Lookup the name of this employee's supervisor
                        Dim superQuery As tblEMPLOYEE = _
                            (From sup In db.tblEMPLOYEEs() _
                             Select sup _
                             Where sup.EMPLOYEE_ID = emp.SUPERVISOR_ID).FirstOrDefault()

                        ' Create the record and add it to the display list
                        Dim webAddress As String = Request.Url.Scheme & "://" & Request.Url.Authority & Request.Url.LocalPath
                        Dim record As New Waiting(emp.LAST_NAME & ", " & emp.FIRST_NAME, _
                                                  superQuery.LAST_NAME & ", " & superQuery.FIRST_NAME, _
                                                  theJob.MASTER_JOB_TITLE, _
                                                  theMachine.MACHINE_CODE, _
                                                  theCat.JOB_TITLE, _
                                                  webAddress & "?sc=et&ed=" + emp.EMPLOYEE_ID.ToString() + _
                                                  "&jn=" + theJob.MASTER_JOB_NUMBER.ToString() + _
                                                  "&mc=" + theMachine.MACHINE_CODE + _
                                                  "&cn=" + theCat.JOB_TITLE_NUMBER.ToString())
                        output.Add(record)
                    End If
                End If
            Next
        Next

        ' Update the display
        Dim sorter As New WaitingSorter
        output.Sort(sorter)

        lblReportWaitingForSupervisorNoEmp.Visible = (output.Count() = 0)
        gridReportWaitingForSupervisor.DataSource = output
        gridReportWaitingForSupervisor.DataBind()

    End Sub
#End Region

#Region "Report - Trainer Report"

    'generate content for Trainer name dropdownlist
    Private Sub TrainerReportSetUp()
        Try
            'Get the content of ddlTRAINER_REPORT

            Dim trainerQuery As List(Of String) = _
                    (From trainers In db.tblEMPLOYEEs() _
                     Where trainers.ACTIVE_STATUS = "A" And _
                            trainers.TRAINER = "Y" _
                     Order By trainers.LAST_NAME, trainers.FIRST_NAME _
                     Select FULL_NAME = trainers.LAST_NAME + ", " + trainers.FIRST_NAME).ToList()

            trainerQuery.Insert(0, "All")
            ddlTRAINER_REPORT.DataSource = trainerQuery
            ddlTRAINER_REPORT.DataBind()

            processTrainerReport("All")

        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION bind")
        End Try

    End Sub

    Private Sub processTrainerReport(ByVal SELECTED_TRAINER As String)
        'Try
        '    'define the datatable to combine information
        '    Dim dt As New DataTable
        '    dt.Columns.AddRange(New DataColumn() {New DataColumn("Trainer Name", GetType(String)), _
        '                                          New DataColumn("Machine Code", GetType(String)), _
        '                                          New DataColumn("Master Job Title", GetType(String)), _
        '                                          New DataColumn("Job Title", GetType(String))})
        '    Dim ds As DataView = CType(SqlDataSourcetblTrainerQualifications.Select(DataSourceSelectArguments.Empty), DataView)
        '    'ds.Sort = "LAST_NAME ASC, FIRST_NAME ASC, MACHINE_CODE ASC"
        '    For Each dr As DataRowView In ds
        '        Dim trainer As String = dr.Item(3).ToString + ", " + dr.Item(2).ToString

        '        dt.Rows.Add(trainer, dr.Item(1), dr.Item(4), dr.Item(5))

        '    Next

        '    gridTrainerReport.DataSource = dt
        '    gridTrainerReport.DataBind()

        'Catch ex As Exception
        '    MessageBox(ex.ToString(), "EXCEPTION bind")
        'End Try

        gridTrainerReport.DataSource = Nothing

        Dim output As New List(Of Trainer)
        Dim trainerName As String = SELECTED_TRAINER

        Try
            ' Select trainer IDs from tblEMPLOYEE 
            Dim empQuery As List(Of tblEMPLOYEE)

            If (Not trainerName = "All") Then
                ' Break full name into its individual parts for use in the query
                Dim pos As Integer = ddlTRAINER_REPORT.SelectedValue.IndexOf(",")
                Dim lastName As String = ddlTRAINER_REPORT.SelectedValue.Substring(0, pos)
                Dim firstName As String = ddlTRAINER_REPORT.SelectedValue.Substring(pos + 2)

                Session("FIRST_NAME") = firstName
                Session("LAST_NAME") = lastName
                empQuery = (From emps In db.tblEMPLOYEEs() _
                            Select emps _
                            Where emps.FIRST_NAME.Equals(Session("FIRST_NAME")) And _
                            emps.LAST_NAME.Equals(Session("LAST_NAME")) _
                            Order By emps.LAST_NAME, emps.FIRST_NAME).ToList
            Else
                empQuery = (From emps In db.tblEMPLOYEEs() _
                            Where emps.ACTIVE_STATUS = "A" And _
                            emps.TRAINER = "Y" _
                            Select emps).ToList()

            End If


            ' Loop throught the list of employees
            For Each em As tblEMPLOYEE In empQuery
                Dim emp As tblEMPLOYEE = em

                ' Retrieve the machine code, job title numbers and master job title numbers from tblEMPLOYEE_QUALILFICATIONS
                Dim partiaQual = (From qual In db.tblEMPLOYEES_QUALIFIEDs _
                                  Where qual.EMPLOYEE_ID = emp.EMPLOYEE_ID _
                                  Select qual.MASTER_JOB_NUMBER, qual.JOB_TITLE_NUMBER, qual.MACHINE_CODE).ToList()

                ' Loop through all the partia records in tblEMPLOYEE_QUALIFICATIONS and display their detailed information
                For Each part In partiaQual
                    Dim theTraining = part
                    'get job
                    Dim theJob = (From job In db.tblJOB_TITLEs _
                                  Where job.JOB_TITLE_NUMBER = theTraining.JOB_TITLE_NUMBER _
                                  Select job).FirstOrDefault()
                    If (theJob Is Nothing) Then
                        MessageBox("JOB NUMBER '" & theTraining.JOB_TITLE_NUMBER & "' is invalid.", _
                                   "DATABASE ERROR")
                        Continue For
                    End If

                    'get master job
                    Dim theMaster = (From master In db.tblMASTER_JOB_TITLEs _
                                     Where master.MASTER_JOB_NUMBER = part.MASTER_JOB_NUMBER _
                                     Select master).FirstOrDefault()
                    If (theMaster Is Nothing) Then
                        MessageBox("MASTSER JOB NUMBER '" & part.MASTER_JOB_NUMBER & "' is invalid.", _
                                   "DATABASE ERROR")
                        Continue For
                    End If



                    ' Create the record and add it to the display list
                    Dim record As New Trainer(emp.LAST_NAME & ", " & emp.FIRST_NAME, _
                                              theTraining.MACHINE_CODE, _
                                              theMaster.MASTER_JOB_TITLE, _
                                              theJob.JOB_TITLE
                                              )
                    output.Add(record)
                Next
            Next

            ' Update the display
            Dim sorter As New TrainerSorter
            output.Sort(sorter)

            gridTrainerReport.DataSource = output
            gridTrainerReport.DataBind()
            'UpdatePanel1.Update()

        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION bind")
        End Try
    End Sub

    Protected Sub ddlTRAINER_REPORT_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlTRAINER_REPORT.SelectedIndexChanged
        Try
            processTrainerReport(ddlTRAINER_REPORT.SelectedValue)

            '' Reset widgets that rely on this value
            'gridTrainerReport.DataSource = Nothing
            'gridTrainerReport.DataBind()

        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

    'print the grid
    Protected Sub btnTrainerReportPrint_Click(sender As Object, e As EventArgs) Handles btnTrainerReportPrint.Click
        Try
            PrintExcelGrid(gridTrainerReport, "Trainer Report")
        Catch ex As System.Threading.ThreadAbortException
            'No Action
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub
#End Region

#Region "EXCEL"

    ' ************************************************************************
    ' Name: preprocessTrainerReport
    '
    ' Purpose: Produce a DataTable to combine all the data from different tables and bind it to gridview as datasource
    '
    ' ************************************************************************
    

    Private Sub PrintExcelGrid(ByVal grid As GridView, ByVal title As String)
        Try
            ' If there are no records to print display a message
            If ((grid.HeaderRow Is Nothing) Or _
                (grid.Rows.Count = 0)) Then
                MessageBox("There are no records to print.", "USER ERROR")
                Exit Sub
            End If

            ' Create the Excel File
            Dim excel As New ExcelPackage()
            Dim wkbk As ExcelWorkbook = excel.Workbook
            Dim wkst As ExcelWorksheet = wkbk.Worksheets.Add("Sheet 1")

            ' Write the titles to the spreadsheet
            Dim row As Integer = 1
            Dim column As Integer = 1
            wkst.Cells(row, column).Value = title
            wkst.Cells(row, column).Style.Font.Bold = True

            ' Merge the title columns
            wkst.Cells(1, 1, 1, grid.HeaderRow.Cells.Count).Merge = True
            row += 1

            wkst.Cells(row, column).Value = Now.ToString("dd-MMM-yyyy")
            wkst.Cells(row, column).Style.Font.Bold = True

            ' Merge the title columns
            wkst.Cells(2, 1, 2, grid.HeaderRow.Cells.Count).Merge = True

            row += 1
            row += 1

            ' Copy the grid header to the spreadsheet
            Dim headingRow As Integer = row
            column = 1
            For i As Integer = 0 To (grid.HeaderRow.Cells.Count - 1)
                Dim strValue As String = grid.HeaderRow.Cells(i).Text.Trim
                If (strValue <> "&nbsp;") Then
                    wkst.Cells(row, column).Value = strValue
                    wkst.Cells(row, column).Style.Border.BorderAround(Style.ExcelBorderStyle.Thin)
                    column += 1
                End If
            Next
            Dim header As ExcelRange = wkst.Cells(headingRow, 1, headingRow, column - 1)
            header.Style.Font.Bold = True
            header.Style.Fill.PatternType = Style.ExcelFillStyle.Solid
            header.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray)

            ' Copy the grid data to the spreadsheet
            For i As Integer = 0 To (grid.Rows.Count - 1)
                row += 1
                column = 1
                For j As Integer = 0 To (grid.Columns.Count - 1)

                    ' get the cell text
                    Dim strValue As String = grid.Rows(i).Cells(j).Text.Trim

                    ' Extract the vell text from the child widgets
                    If (grid.Rows(i).Cells(j).Controls.Count > 0) Then
                        Dim ctrl As Control = grid.Rows(i).Cells(j).Controls(0)
                        If (Not ctrl Is Nothing) Then
                            strValue = CType(ctrl, HyperLink).Text.Trim
                        Else
                            strValue = ""
                        End If
                    End If

                    ' Replace NBSP with a space
                    If (strValue = "&nbsp;") Then
                        strValue = ""
                    End If

                    ' Write the value to the spreadsheet cell
                    If ((strValue.Length <> 0) Or (j <> 0)) Then
                        If (IsNumeric(strValue) = True) Then
                            wkst.Cells(row, column).Value = Double.Parse(strValue)
                        Else
                            wkst.Cells(row, column).Value = strValue
                        End If
                        wkst.Cells(row, column).Style.Border.BorderAround(Style.ExcelBorderStyle.Thin)
                        column += 1
                    End If
                Next
            Next

            'Resize the columns so everything fits
            Dim rpt As ExcelRange = wkst.Cells(1, 1, row, column - 1)
            rpt.Style.HorizontalAlignment = Style.ExcelHorizontalAlignment.Center
            rpt.AutoFitColumns()

            ' Download the Excel File
            Dim filename As String = Now.ToString("yyyy_MM_dd_HHmmss") & ".xlsx"
            Dim Response As HttpResponse = System.Web.HttpContext.Current.Response
            Response.Clear()

            Response.AddHeader("content-disposition", "attachment;  filename=" & filename)

            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            Response.BinaryWrite(excel.GetAsByteArray())
            Response.End()
        Catch ex As System.Threading.ThreadAbortException
            'No Action
        Catch ex As Exception
            MessageBox(ex.ToString(), "EXCEPTION")
        End Try
    End Sub

#End Region

End Class