Option Strict On
Option Explicit On

Imports System.Data.Linq

' ****************************************************************************
'    Name: TrainingForm class
'
' Purpose: Display the training info for a particular job, machine, and 
'          category as a printer friendly form
'
' ****************************************************************************
Partial Class TrainingForm
    Inherits System.Web.UI.Page

    Private db As OJT_DB_ClassesDataContext      ' Reference to the database


    ' ************************************************************************
    '    Name: TrainingForm_Load
    '
    ' Purpose: When the form loads, open a connection to the database
    '
    ' ************************************************************************
    Protected Sub TrainingForm_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles imgHaartz.Load
        Try
            db = New OJT_DB_ClassesDataContext(ConfigurationManager.ConnectionStrings.Item("OJTConnectionString").ConnectionString)
        Catch ex As Exception
            HandleException(ex)
        End Try
    End Sub

    ' ************************************************************************
    '    Name: HandleException
    '
    ' Purpose: When an exception occurs, display it
    '
    ' ************************************************************************
    Private Sub HandleException(ByVal ex As Exception)
        lblError.Text = "EXCEPTION: " & ex.ToString()
        lblError.Visible = True
    End Sub

    ' ************************************************************************
    '    Name: TrainingForm_PreRender
    '
    ' Purpose: Update the widgets before the form is displayed
    '
    ' ************************************************************************
    Protected Sub TrainingForm_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles imgHaartz.PreRender
        Try
            Dim jobNum As Integer = Integer.Parse(Session("SELECTED_JOB_NUMBER").ToString())
            Dim mCode As String = Session("SELECTED_MACHINE_CODE").ToString()
            Dim catNum As Integer = Integer.Parse(Session("SELECTED_CATEGORY_NUMBER").ToString())
            Dim eCode As Integer = 0
            If ((Not Session("SELECTED_EMPLOYEE_ID") Is Nothing) AndAlso _
                (Session("SELECTED_EMPLOYEE_ID").ToString().Length <> 0)) Then
                eCode = Integer.Parse(Session("SELECTED_EMPLOYEE_ID").ToString())
            End If

            If (jobNum <= 0) Or (mCode.Length = 0) Or (catNum <= 0) Then
                Exit Sub
            End If

            ' Query to find the Job information associated with the selected job number
            Dim jobTitleQuery As String = _
                (From titles In db.tblMASTER_JOB_TITLEs() _
                 Where titles.MASTER_JOB_NUMBER = jobNum _
                 Select titles.MASTER_JOB_TITLE).FirstOrDefault()
            lblHeaderJob2.Text = jobTitleQuery

            ' Query to find the Machine information associated with the selected machine code
            Dim machineQuery As String = _
                (From mach In db.tblMACHINEs() _
                 Where mach.MACHINE_CODE = mCode _
                 Select mach.DESCRIPTION).FirstOrDefault()
            lblHeaderMachine2.Text = machineQuery + "(" + mCode + ")"

            ' Query to find the Category information associated with the selected category
            Dim categoryQuery As String = _
                (From cats In db.tblJOB_TITLEs() _
                 Where cats.JOB_TITLE_NUMBER = catNum _
                 Select cats.JOB_TITLE).FirstOrDefault()
            lblHeaderCategory2.Text = categoryQuery

            ' Query to find the training codes for this job, machine, and category
            Dim stepQuery = _
                (From Jmm In db.tblJOB_MACHINE_MASTERs() _
                 Join Codes In db.tblTRAINING_CODEs() _
                 On Codes.TRAINING_NUMBER Equals Jmm.TRAINING_NUMBER _
                 Order By Codes.TITLE _
                 Where Jmm.MASTER_JOB_NUMBER = jobNum And _
                     Jmm.MACHINE_CODE = mCode And _
                     Jmm.JOB_TITLE_NUMBER = catNum _
                 Select New With {.STEP_COMPLETE = "", Codes.MACHINE_DEPENDENT, Codes.TITLE, Codes.DESCRIPTION, Codes.TRAINING_NUMBER}).ToList()

            'Lookup each step for this training to see if it has been completed
            For Each trainingStep In stepQuery
                Dim trainingNum = trainingStep.TRAINING_NUMBER
                Dim doneQuery = (From eq In db.tblEMPLOYEE_QUALIFICATIONs _
                                 Where eq.EMPLOYEE_ID = eCode And _
                                       eq.MASTER_JOB_NUMBER = jobNum And _
                                       eq.MACHINE_CODE = mCode And _
                                       eq.JOB_TITLE_NUMBER = catNum And _
                                       eq.TRAINING_NUMBER = trainingNum).ToList()
                If (doneQuery.Count <> 0) Then
                    trainingStep.STEP_COMPLETE = "X"
                End If
            Next
            gridTraining.DataSource = stepQuery
            gridTraining.DataBind()

            ' Query to find the qualification information
            Dim qualificationQuery As tblEMPLOYEES_QUALIFIED = ( _
                From eq In db.tblEMPLOYEES_QUALIFIEDs _
                Where eq.EMPLOYEE_ID = eCode And _
                      eq.MASTER_JOB_NUMBER = jobNum And _
                      eq.MACHINE_CODE = mCode And _
                      eq.JOB_TITLE_NUMBER = catNum _
                Select eq).FirstOrDefault()

            ' Update the Form Change label
            lblFormChange.Text = ""
            If (Not qualificationQuery Is Nothing) AndAlso (qualificationQuery.FORM_CHANGE = True) Then
                lblFormChange.Text = "FORM CHANGE"
            End If
            ' Query to find the trainee information
            Dim traineeQuery As tblEMPLOYEE = ( _
                From emps In db.tblEMPLOYEEs() _
                Where emps.EMPLOYEE_ID = eCode _
                Select emps).FirstOrDefault()
            If (traineeQuery Is Nothing) Then
                lblEmployeeSignedSpace.Text = "_______________________________________________"
                lblEmployeeDateSpace.Text = "______________"
                lblHeaderEmp1.Text = ""
                lblHeaderEmp2.Text = ""
            Else
                lblEmployeeSignedSpace.Text = traineeQuery.FIRST_NAME + " " + traineeQuery.LAST_NAME
                lblHeaderEmp2.Text = lblEmployeeSignedSpace.Text
                If (qualificationQuery Is Nothing) Then
                    lblEmployeeDateSpace.Text = "______________"
                Else
                    lblEmployeeDateSpace.Text = qualificationQuery.DATE_APPROVED.Value.ToString("MM/dd/yyyy")
                End If
            End If

            ' Query to find the operator/trainer information
            Dim trainerQuery As tblEMPLOYEE = ( _
                From eq In db.tblEMPLOYEES_QUALIFIEDs _
                Join emp In db.tblEMPLOYEEs _
                On eq.APPROVED_BY Equals emp.EMPLOYEE_ID _
                Where eq.EMPLOYEE_ID = eCode And _
                      eq.MASTER_JOB_NUMBER = jobNum And _
                      eq.MACHINE_CODE = mCode And _
                      eq.JOB_TITLE_NUMBER = catNum _
                Select emp).FirstOrDefault()
            If (trainerQuery Is Nothing) Then
                lblTrainerSignedSpace.Text = "_______________________________________________"
                lblTrainerDateSpace.Text = "______________"
            Else
                lblTrainerSignedSpace.Text = trainerQuery.FIRST_NAME + " " + trainerQuery.LAST_NAME
                lblTrainerDateSpace.Text = qualificationQuery.DATE_APPROVED.Value.ToString("MM/dd/yyyy")
            End If

            ' Query to find the supervisor information
            Dim superQuery As tblEMPLOYEE = ( _
                From eq In db.tblEMPLOYEES_QUALIFIEDs _
                Join emp In db.tblEMPLOYEEs _
                On eq.SUPERVISOR Equals emp.EMPLOYEE_ID _
                Where eq.EMPLOYEE_ID = eCode And _
                      eq.MASTER_JOB_NUMBER = jobNum And _
                      eq.MACHINE_CODE = mCode And _
                      eq.JOB_TITLE_NUMBER = catNum _
                Select emp).FirstOrDefault()
            If (superQuery Is Nothing) Then
                lblSupervisorSignedSpace.Text = "_______________________________________________"
                lblSupervisorDateSpace.Text = "______________"
            Else
                lblSupervisorSignedSpace.Text = superQuery.FIRST_NAME + " " + superQuery.LAST_NAME
                lblSupervisorDateSpace.Text = qualificationQuery.SUPERVISOR_APPROVED.Value.ToString("MM/dd/yyyy")
            End If

            ' Populate the signoff fields based on the category and if we have a known trainee
            If (categoryQuery.StartsWith("S", StringComparison.OrdinalIgnoreCase)) Then
                If (traineeQuery Is Nothing) Then
                    lblCompletionType.Text = "I have completed the above safety training."
                    lblCompletionTrainer.Text = "The above employee has completed the above safety training."
                    lblCompletionSupervisor.Text = "The above employee has completed the above safety training."
                Else
                    lblCompletionType.Text = "I have completed the above safety training."
                    lblCompletionTrainer.Text = traineeQuery.FIRST_NAME + " " + traineeQuery.LAST_NAME + " has completed the above safety training."
                    lblCompletionSupervisor.Text = traineeQuery.FIRST_NAME + " " + traineeQuery.LAST_NAME + " has completed the above safety training."
                End If
            Else
                If (traineeQuery Is Nothing) Then
                    lblCompletionType.Text = "I have completed the above training and am ready to be qualified as a " + _
                        jobTitleQuery + " at " + machineQuery + "."
                    lblCompletionTrainer.Text = "I consider the above employee qualified to assume the duties and responsibilties of a " + jobTitleQuery + " at " + machineQuery + "."
                    lblCompletionSupervisor.Text = "I consider the above employee qualified to assume the duties and responsibilties of a " + jobTitleQuery + " at " + machineQuery + "."
                Else
                    lblCompletionType.Text = "I have completed the above training and am ready to be qualified as a " + _
                        jobTitleQuery + " at " + machineQuery + "."
                    lblCompletionTrainer.Text = "I consider " + traineeQuery.FIRST_NAME + " " + traineeQuery.LAST_NAME + _
                        " qualified to assume the duties and responsibilties of a " + jobTitleQuery + " at " + machineQuery + "."
                    lblCompletionSupervisor.Text = "I consider " + traineeQuery.FIRST_NAME + " " + traineeQuery.LAST_NAME + _
                        " qualified to assume the duties and responsibilties of a " + jobTitleQuery + " at " + machineQuery + "."
                End If
            End If
        Catch ex As Exception
            HandleException(ex)
        End Try
    End Sub
End Class
