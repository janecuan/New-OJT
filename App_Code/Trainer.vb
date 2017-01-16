Option Strict On
Option Explicit On
Option Infer Off

Public Class Trainer

    Private employee_ As String
    Private machine_ As String
    Private master_ As String
    Private job_ As String


    Public Sub New(ByVal newEmp As String, ByVal newMachine As String, ByVal newMaster As String, ByVal newJob As String)
        Me.employee_ = newEmp
        Me.machine_ = newMachine
        Me.master_ = newMaster
        Me.job_ = newJob
    End Sub

    Public ReadOnly Property EMPLOYEE() As String
        Get
            Return employee_
        End Get
    End Property

    Public ReadOnly Property MASTER() As String
        Get
            Return master_
        End Get
    End Property

    Public ReadOnly Property Job() As String
        Get
            Return job_
        End Get
    End Property

    Public ReadOnly Property Machine() As String
        Get
            Return machine_
        End Get
    End Property

End Class

        ' ****************************************************************************
'    Name: TrainerSorter class
        '
        ' Purpose: Sort logic for the Waiting class
        '
        ' ****************************************************************************
Public Class TrainerSorter
    Implements IComparer(Of Trainer)

    ' ************************************************************************
    '    Name: Compare
    '
    ' Purpose: Sort on the employee, job, machine
    '
    ' ************************************************************************
    Public Function Compare(ByVal left As Trainer, ByVal right As Trainer) As Integer _
        Implements IComparer(Of Trainer).Compare

            If (left.EMPLOYEE.Equals(right.EMPLOYEE)) Then
                    If (left.Machine.Equals(right.Machine)) Then
                        Return New CaseInsensitiveComparer().Compare(left.Job, right.Job)
                    Else
                        Return New CaseInsensitiveComparer().Compare(left.Machine, right.Machine)
                    End If
            Else
                Return New CaseInsensitiveComparer().Compare(left.EMPLOYEE, right.EMPLOYEE)
            End If


    End Function

End Class