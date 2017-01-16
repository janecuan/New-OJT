Option Strict On
Option Explicit On
Option Infer Off

' ****************************************************************************
'    Name: EmployeeComplete class
'
' Purpose: Storage class for the info in the Employee Training Completed report
'
' ****************************************************************************
Public Class EmployeeComplete

    Private job_ As String
    Private machine_ As String
    Private category_ As String
    Private status_ As String
    Private address_ As String

    ' ************************************************************************
    '    Name: EmployeeComplete constructor
    '
    ' Purpose: Initialize the class attributes of a new instance
    '
    ' ************************************************************************
    Public Sub New(ByVal newJob As String, ByVal newMach As String, ByVal newCat As String, ByVal newStatus As String, ByVal newAddress As String)
        Me.job_ = newJob
        Me.machine_ = newMach
        Me.category_ = newCat
        Me.status_ = newStatus
        Me.address_ = newAddress
    End Sub

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
    '    Name: Status accessor
    '
    ' Purpose: Return the status attribute of the current record
    '
    ' ************************************************************************
    Public ReadOnly Property Status() As String
        Get
            Return status_
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
'    Name: EmployeeCompleteSorter class
'
' Purpose: Sort logic for the EmployeeComplete class
'
' ****************************************************************************
Public Class EmployeeCompleteSorter
    Implements IComparer

    ' ************************************************************************
    '    Name: Compare
    '
    ' Purpose: Sort on the job, machine, category, and status
    '
    ' ************************************************************************
    Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements IComparer.Compare
        Dim left As EmployeeComplete = CType(x, EmployeeComplete)
        Dim right As EmployeeComplete = CType(y, EmployeeComplete)

        If (left.Status.Equals(right.Status)) Then
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
            Return New CaseInsensitiveComparer().Compare(left.Status, right.Status)
        End If

    End Function 'IComparer.Compare

End Class
