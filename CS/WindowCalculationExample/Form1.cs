using DevExpress.DashboardCommon;
using DevExpress.DashboardWin;
using System;
using System.IO;
using System.Windows.Forms;

namespace WindowCalculationExample
{
    public partial class Form1 : DevExpress.XtraEditors.XtraForm
    {
        private string timedTabContainerName;
        private Timer tabTimer = new Timer();
        public Form1()
        {
            InitializeComponent();
            dashboardViewer1.ConfigureDataConnection += DashboardViewer1_ConfigureDataConnection;
            dashboardViewer1.DashboardItemControlCreated += DashboardViewer1_DashboardItemControlCreated;
            dashboardViewer1.DashboardItemControlUpdated += DashboardViewer1_DashboardItemControlUpdated;
            dashboardViewer1.DashboardItemClick += DashboardViewer1_DashboardItemClick;

            TabPageLoadHelper helper = new TabPageLoadHelper("CalculationProductSalesSource.xml");
            helper.PrepareDashboard();
            dashboardViewer1.Dashboard = helper.Dashboard;
            

            ApplyScatterChartWindowCalculations(helper.ScatterChartItemName);
            ApplyPivotWindowCalculations(helper.PivotItemName);

            timedTabContainerName = helper.TabContainerName;

            StartTimer();
        }

        private void ApplyPivotWindowCalculations(string pivotItemName)
        {
            PivotDashboardItem pivotItem = dashboardViewer1.Dashboard.Items[pivotItemName] as PivotDashboardItem;
            if (pivotItem != null)
            {
                Measure extendedPrice = new Measure("Extended Price")
                {
                    Name = "Diff",
                    ShowGrandTotals = false
                };
                PivotWindowDefinition pivotWindowDefinition = new PivotWindowDefinition();
                pivotWindowDefinition.DefinitionMode = PivotWindowDefinitionMode.Columns;
                extendedPrice.WindowDefinition = pivotWindowDefinition;
                extendedPrice.Calculation = new DifferenceCalculation() { DifferenceType = DifferenceType.Absolute };
                pivotItem.Values.Add(extendedPrice);
            }
        }

        private void ApplyScatterChartWindowCalculations(string scatterChartName)
        {
            ScatterChartDashboardItem scatterChart = dashboardViewer1.Dashboard.Items[scatterChartName] as ScatterChartDashboardItem;
            if (scatterChart != null)
            {
                scatterChart.AxisXMeasure.Calculation = new DifferenceCalculation() { DifferenceType = DifferenceType.Percentage };
                scatterChart.AxisXMeasure.WindowDefinition = new ScatterWindowDefinition();
                scatterChart.AxisYMeasure.Expression = "WindowMedian(ToDouble(Sum([Extended Price]) - Lookup(Sum([Extended Price]), -1)) / Lookup(Sum([Extended Price]), -1), 0, 1)";
                scatterChart.AxisYMeasure.WindowDefinition = new ScatterWindowDefinition();
            }
        }

        private void DashboardViewer1_ConfigureDataConnection(object sender, DashboardConfigureDataConnectionEventArgs e)
        {
            ExtractDataSourceConnectionParameters parameters = e.ConnectionParameters as ExtractDataSourceConnectionParameters;
            if (parameters != null)
                parameters.FileName = Path.GetFileName(parameters.FileName);
        }
        private void DashboardViewer1_DashboardItemControlUpdated(object sender, DashboardItemControlEventArgs e)
        {
            if (e.PivotGridControl != null)
                e.PivotGridControl.BestFit();
        }

        private void DashboardViewer1_DashboardItemControlCreated(object sender, DashboardItemControlEventArgs e)
        {
            if (e.PivotGridControl != null)
                e.PivotGridControl.BestFit();
        }


        private void StartTimer()
        {
            tabTimer.Interval = 2000;
            tabTimer.Tick += TabTimer_Tick;
            tabTimer.Start();
        }

        private void TabTimer_Tick(object sender, EventArgs e)
        {
            if (timedTabContainerName != null)
            {
                int selectedIndex = dashboardViewer1.GetSelectedTabPageIndex(timedTabContainerName);
                int pageCount = ((TabContainerDashboardItem)dashboardViewer1.Dashboard.Items[timedTabContainerName]).TabPages.Count;
                dashboardViewer1.SetSelectedTabPage(timedTabContainerName, ++selectedIndex % pageCount);
            }
        }

        private void DashboardViewer1_DashboardItemClick(object sender, DevExpress.DashboardWin.DashboardItemMouseActionEventArgs e)
        {
            tabTimer.Enabled = !tabTimer.Enabled;
        }

    }
}
