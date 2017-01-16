Option Strict On
Option Explicit On
Option Infer Off

Public Class Incomplete

    Private employee_ As String
    Private supervisor_ As String

    Public Sub New(ByVal newEmp As String, ByVal newSuper As String)
        Me.employee_ = newEmp
        Me.supervisor_ = newSuper
    End Sub

    Public ReadOnly Property EMPLOYEE() As String
        Get
            Return employee_
        End Get
    End Property

    Public ReadOnly Property SUPERVISOR() As String
        Get
            Return supervisor_
        End Get
    End Property

End Class

' ****************************************************************************
'    Name: EmployeeCompleteSorter class
'
' Purpose: Sort logic for the EmployeeComplete class
'
' ****************************************************************************
Public Class IncompleteSorter
    Implements IComparer(Of Incomplete)

    ' ************************************************************************
    '    Name: Compare
    '
    ' Purpose: Sort on the employee, job, machine
    '
    ' ************************************************************************
    Public Function Compare(ByVal left As Incomplete, ByVal right As Incomplete) As Integer _
        Implements IComparer(Of Incomplete).Compare

        If (left.SUPERVISOR.Equals(right.SUPERVISOR)) Then
            Return New CaseInsensitiveComparer().Compare(left.EMPLOYEE, right.EMPLOYEE)
        Else
            Return New CaseInsensitiveComparer().Compare(left.SUPERVISOR, right.SUPERVISOR)
        End If

    End Function

End Class