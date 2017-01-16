Option Strict On
Option Explicit On
Option Infer Off

Public Class Waiting

    Private employee_ As String
    Private supervisor_ As String
    Private job_ As String
    Private machine_ As String
    Private category_ As String
    Private address_ As String

    Public Sub New(ByVal newEmp As String, ByVal newSuper As String, ByVal newJob As String, ByVal newMach As String, ByVal newCat As String, ByVal newAddress As String)
        Me.employee_ = newEmp
        Me.supervisor_ = newSuper
        Me.job_ = newJob
        Me.machine_ = newMach
        Me.category_ = newCat
        Me.address_ = newAddress
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


    ' ************************************************************************
    '    Name: Job accessor
    '
    ' Purpose: Return the job attribute of the current record
    '
    ' ************************************************************************
    Public ReadOnly Property Job() As String
        Get
            Return job_
        End Get
    End Property

    ' ************************************************************************
    '    Name: Machine accessor
    '
    ' Purpose: Return the machine attribute of the current record
    '
    ' ************************************************************************
    Public ReadOnly Property Machine() As String
        Get
            Return machine_
        End Get
    End Property

    ' ************************************************************************
    '    Name: Category accessor
    '
    ' Purpose: Return the category attribute of the current record
    '
    ' ************************************************************************
    Public ReadOnly Property Category() As String
        Get
            Return category_
        End Get
    End Property

    ' ************************************************************************
    '    Name: Address accessor
    '
    ' Purpose: Return the address attribute of the current record
    '
    ' ************************************************************************
    Public ReadOnly Property Address() As String
        Get
            Return address_
        End Get
    End Property

End Class

        ' ****************************************************************************
        '    Name: WaitingSorter class
        '
        ' Purpose: Sort logic for the Waiting class
        '
        ' ****************************************************************************
Public Class WaitingSorter
        Implements IComparer(Of Waiting)

        ' ************************************************************************
        '    Name: Compare
        '
        ' Purpose: Sort on the employee, job, machine
        '
        ' ************************************************************************
        Public Function Compare(ByVal left As Waiting, ByVal right As Waiting) As Integer _
            Implements IComparer(Of Waiting).Compare

        If (left.SUPERVISOR.Equals(right.SUPERVISOR)) Then
            If (left.EMPLOYEE.Equals(right.EMPLOYEE)) Then
                If (left.Category.Equals(right.Category)) Then
                    If (left.Machine.Equals(right.Machine)) Then
                        Return New CaseInsensitiveComparer().Compare(left.Job, right.Job)
                    Else
                        Return New CaseInsensitiveComparer().Compare(left.Machine, right.Machine)
                    End If

                Else
                    Return New CaseInsensitiveComparer().Compare(left.Category, right.Category)
                End If
            Else
                Return New CaseInsensitiveComparer().Compare(left.EMPLOYEE, right.EMPLOYEE)
            End If
        Else
            Return New CaseInsensitiveComparer().Compare(left.SUPERVISOR, right.SUPERVISOR)
        End If

        End Function

    End Class