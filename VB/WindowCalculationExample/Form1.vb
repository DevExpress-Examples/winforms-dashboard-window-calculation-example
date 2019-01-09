Imports DevExpress.DashboardCommon
Imports DevExpress.DashboardWin
Imports System
Imports System.IO
Imports System.Windows.Forms

Namespace WindowCalculationExample
    Partial Public Class Form1
        Inherits Form

        Private timedTabContainerName As String
        Private tabTimer As New Timer()
        Public Sub New()
            InitializeComponent()
            AddHandler dashboardViewer1.ConfigureDataConnection, AddressOf DashboardViewer1_ConfigureDataConnection
            AddHandler dashboardViewer1.DashboardItemControlCreated, AddressOf DashboardViewer1_DashboardItemControlCreated
            AddHandler dashboardViewer1.DashboardItemControlUpdated, AddressOf DashboardViewer1_DashboardItemControlUpdated
            AddHandler dashboardViewer1.DashboardItemClick, AddressOf DashboardViewer1_DashboardItemClick

            Dim helper As New TabPageLoadHelper("CalculationProductSalesSource.xml")
            helper.PrepareDashboard()
            dashboardViewer1.Dashboard = helper.Dashboard


            ApplyScatterChartWindowCalculations(helper.ScatterChartItemName)
            ApplyPivotWindowCalculations(helper.PivotItemName)

            timedTabContainerName = helper.TabContainerName

            StartTimer()
        End Sub

        Private Sub ApplyPivotWindowCalculations(ByVal pivotItemName As String)
            Dim pivotItem As PivotDashboardItem = TryCast(dashboardViewer1.Dashboard.Items(pivotItemName), PivotDashboardItem)
            If pivotItem IsNot Nothing Then
                Dim extendedPrice As New Measure("Extended Price") With { _
                    .Name = "Diff", _
                    .ShowGrandTotals = False _
                }
                Dim pivotWindowDefinition As New PivotWindowDefinition()
                pivotWindowDefinition.DefinitionMode = PivotWindowDefinitionMode.Columns
                extendedPrice.WindowDefinition = pivotWindowDefinition
                extendedPrice.Calculation = New DifferenceCalculation() With {.DifferenceType = DifferenceType.Absolute}
                pivotItem.Values.Add(extendedPrice)
            End If
        End Sub

        Private Sub ApplyScatterChartWindowCalculations(ByVal scatterChartName As String)
            Dim scatterChart As ScatterChartDashboardItem = TryCast(dashboardViewer1.Dashboard.Items(scatterChartName), ScatterChartDashboardItem)
            If scatterChart IsNot Nothing Then
                scatterChart.AxisXMeasure.Calculation = New DifferenceCalculation() With {.DifferenceType = DifferenceType.Percentage}
                scatterChart.AxisXMeasure.WindowDefinition = New ScatterWindowDefinition()
                scatterChart.AxisYMeasure.Expression = "WindowMedian(ToDouble(Sum([Extended Price]) - Lookup(Sum([Extended Price]), -1)) / Lookup(Sum([Extended Price]), -1), 0, 1)"
                scatterChart.AxisYMeasure.WindowDefinition = New ScatterWindowDefinition()
            End If
        End Sub

        Private Sub DashboardViewer1_ConfigureDataConnection(ByVal sender As Object, ByVal e As DashboardConfigureDataConnectionEventArgs)
            Dim parameters As ExtractDataSourceConnectionParameters = TryCast(e.ConnectionParameters, ExtractDataSourceConnectionParameters)
            If parameters IsNot Nothing Then
                parameters.FileName = Path.GetFileName(parameters.FileName)
            End If
        End Sub
        Private Sub DashboardViewer1_DashboardItemControlUpdated(ByVal sender As Object, ByVal e As DashboardItemControlEventArgs)
            If e.PivotGridControl IsNot Nothing Then
                e.PivotGridControl.BestFit()
            End If
        End Sub

        Private Sub DashboardViewer1_DashboardItemControlCreated(ByVal sender As Object, ByVal e As DashboardItemControlEventArgs)
            If e.PivotGridControl IsNot Nothing Then
                e.PivotGridControl.BestFit()
            End If
        End Sub


        Private Sub StartTimer()
            tabTimer.Interval = 2000
            AddHandler tabTimer.Tick, AddressOf TabTimer_Tick
            tabTimer.Start()
        End Sub

        Private Sub TabTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)
            If timedTabContainerName IsNot Nothing Then
                Dim selectedIndex As Integer = dashboardViewer1.GetSelectedTabPageIndex(timedTabContainerName)
                Dim pageCount As Integer = CType(dashboardViewer1.Dashboard.Items(timedTabContainerName), TabContainerDashboardItem).TabPages.Count
                selectedIndex += 1
                dashboardViewer1.SetSelectedTabPage(timedTabContainerName, selectedIndex Mod pageCount)
            End If
        End Sub

        Private Sub DashboardViewer1_DashboardItemClick(ByVal sender As Object, ByVal e As DevExpress.DashboardWin.DashboardItemMouseActionEventArgs)
            tabTimer.Enabled = Not tabTimer.Enabled
        End Sub

    End Class
End Namespace
