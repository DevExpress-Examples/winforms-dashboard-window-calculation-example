Imports DevExpress.DashboardCommon
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq

Namespace WindowCalculationExample

    Friend Class TabPageLoadHelper

        Private _TabContainerName As String, _ScatterChartItemName As String, _PivotItemName As String

        Private originalDashboard As Dashboard = New Dashboard()

        Private destinationParentContainer As TabContainerDashboardItem

        Public Property Dashboard As Dashboard

        Public Property TabContainerName As String
            Get
                Return _TabContainerName
            End Get

            Private Set(ByVal value As String)
                _TabContainerName = value
            End Set
        End Property

        Public Property ScatterChartItemName As String
            Get
                Return _ScatterChartItemName
            End Get

            Private Set(ByVal value As String)
                _ScatterChartItemName = value
            End Set
        End Property

        Public Property PivotItemName As String
            Get
                Return _PivotItemName
            End Get

            Private Set(ByVal value As String)
                _PivotItemName = value
            End Set
        End Property

        Public Sub New(ByVal xmldashboardFile As String)
            originalDashboard.LoadFromXml(Path.Combine(Environment.CurrentDirectory, xmldashboardFile))
        End Sub

        Public Sub PrepareDashboard()
            Dim targetDashboard As Dashboard = New Dashboard()
            destinationParentContainer = New TabContainerDashboardItem()
            targetDashboard.Items.Add(destinationParentContainer)
            TabContainerName = destinationParentContainer.ComponentName
            Dim tabPage As DashboardTabPage = CopyDashboardToTabPage(targetDashboard, originalDashboard)
            Dashboard = targetDashboard
            Dim tabPage1 As DashboardTabPage = destinationParentContainer.CreateTabPage()
            targetDashboard.RebuildLayout()
            CopyTabPage(tabPage, tabPage1)
            ScatterChartItemName = Enumerable.Single(targetDashboard.Items, Function(item)(TypeOf item Is ScatterChartDashboardItem) AndAlso item.ParentContainer Is tabPage1).ComponentName
            PivotItemName = Enumerable.Single(targetDashboard.Items, Function(item) TypeOf item Is PivotDashboardItem AndAlso item.ParentContainer Is tabPage1).ComponentName
        End Sub

        Private Function CopyDashboardToTabPage(ByVal targetDashboard As Dashboard, ByVal originalDashboard As Dashboard) As DashboardTabPage
            Dim tabPage As DashboardTabPage = destinationParentContainer.CreateTabPage()
            Dim layoutPage As DashboardLayoutTabPage = New DashboardLayoutTabPage(tabPage)
            Dim layoutTabContainer As DashboardLayoutTabContainer = New DashboardLayoutTabContainer(destinationParentContainer)
            layoutTabContainer.ChildNodes.Add(layoutPage)
            Dim itemsAndGroups As IEnumerable(Of DashboardItem) = originalDashboard.Items.Union(originalDashboard.Groups).Where(Function(item) Not(TypeOf item Is TabContainerDashboardItem))
            For Each item As DashboardItem In itemsAndGroups
                If item.ParentContainer Is Nothing Then
                    If Not(TypeOf item Is TabContainerDashboardItem) Then
                        targetDashboard.Items.Add(item)
                        item.ParentContainer = tabPage
                    End If
                End If
            Next

            Dim layoutRoot As DashboardLayoutGroup = originalDashboard.LayoutRoot
            layoutPage.ChildNodes.Add(layoutRoot)
            targetDashboard.LayoutRoot = New DashboardLayoutGroup()
            targetDashboard.LayoutRoot.ChildNodes.Add(layoutTabContainer)
            Return tabPage
        End Function

        Public Sub CopyTabPage(ByVal tabPageSource As DashboardTabPage, ByVal tabPageDestination As DashboardTabPage)
            Dim targetDashboard As Dashboard = Dashboard
            Dim tabPageSourceLayout As DashboardLayoutTabPage = tabPageSource.Dashboard.LayoutRoot.FindRecursive(tabPageSource)
            Dim tabPageDestinationLayout As DashboardLayoutTabPage = tabPageSource.Dashboard.LayoutRoot.FindRecursive(tabPageDestination)
            LayoutCopy(tabPageSourceLayout, tabPageDestinationLayout)
        End Sub

        Private Sub LayoutCopy(ByVal copyFrom As DashboardLayoutNode, ByVal copyTo As DashboardLayoutNode)
            copyTo.Weight = copyFrom.Weight
            If(TypeOf copyTo Is DashboardLayoutGroup) AndAlso (TypeOf copyFrom Is DashboardLayoutGroup) Then CType(copyTo, DashboardLayoutGroup).Orientation = CType(copyFrom, DashboardLayoutGroup).Orientation
            If TypeOf copyFrom Is DashboardLayoutGroup Then
                For Each currentNode As DashboardLayoutNode In CType(copyFrom, DashboardLayoutGroup).ChildNodes
                    Dim node As DashboardLayoutNode = New DashboardLayoutItem()
                    Dim item As DashboardItem = currentNode.DashboardItem
                    If item IsNot Nothing Then
                        node.DashboardItem = CopyItemFromPageToPage(item, destinationParentContainer.TabPages(1))
                        node.Weight = currentNode.Weight
                    End If

                    If TypeOf currentNode Is DashboardLayoutGroup Then
                        node = CopyTree(currentNode)
                    End If

                    CType(copyTo, DashboardLayoutGroup).ChildNodes.Add(node)
                Next
            End If
        End Sub

        Private Function CopyItemFromPageToPage(ByVal item As DashboardItem, ByVal itemContainer As IDashboardItemContainer) As DashboardItem
            Dim itemCopy As DashboardItem = item.CreateCopy()
            itemCopy.ComponentName = GetNameOnNextPage(item.ComponentName)
            itemCopy.Name = GetNameOnNextPage(item.Name)
            itemCopy.ParentContainer = itemContainer
            Dashboard.Items.Add(itemCopy)
            Return itemCopy
        End Function

        Private Function GetNameOnNextPage(ByVal name As String) As String
            Return name & "_1"
        End Function

        Private Function CopyTree(ByVal currentNode As DashboardLayoutNode) As DashboardLayoutNode
            Dim dashboardLayoutNode As DashboardLayoutGroup = New DashboardLayoutGroup()
            LayoutCopy(currentNode, dashboardLayoutNode)
            Return dashboardLayoutNode
        End Function
    End Class
End Namespace
