Option Strict On
Option Explicit On
Option Infer Off

' ****************************************************************************
'    Name: ReportComplete class
'
' Purpose: Storage class for the info in the Training Completed report
'
' ****************************************************************************
Public Class ReportComplete

    Private employee_ As String
    Private category_ As String
    Private status_ As String
    Private address_ As String

    ' ************************************************************************
    '    Name: ReportComplete constructor
    '
    ' Purpose: Initialize the class attributes of a new instance
    '
    ' ************************************************************************
    Public Sub New(ByVal newEmp As String, ByVal newCat As String, ByVal newStatus As String, ByVal newAddress As String)
        Me.employee_ = newEmp
        Me.category_ = newCat
        Me.status_ = newStatus
        Me.address_ = newAddress
    End Sub

    ' ************************************************************************
    '    Name: Employee accessor
    '
    ' Purpose: Return the employee attribute of the current record
    '
    ' ************************************************************************
    Public ReadOnly Property Employee() As String
        Get
            Return employee_
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
'    Name: ReportCompleteSorter class
'
' Purpose: Sort logic for the ReportComplete class
'
' ****************************************************************************
Public Class ReportCompleteSorter
    Implements IComparer

    ' ************************************************************************
    '    Name: Compare
    '
    ' Purpose: Sort on the status
    '
    ' ************************************************************************
    Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements IComparer.Compare
        Dim left As ReportComplete = CType(x, ReportComplete)
        Dim right As ReportComplete = CType(y, ReportComplete)

        If (left.Status.Equals(right.Status)) Then
            If (left.Category.Equals(right.Category)) Then
                Return New CaseInsensitiveComparer().Compare(left.Employee, right.Employee)
            Else
                Return New CaseInsensitiveComparer().Compare(left.Category, right.Category)
            End If
        Else
            Return New CaseInsensitiveComparer().Compare(left.Status, right.Status)
        End If
    End Function 'IComparer.Compare

End Class