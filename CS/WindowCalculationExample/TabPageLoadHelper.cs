using DevExpress.DashboardCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WindowCalculationExample
{
    class TabPageLoadHelper
    {
        private Dashboard originalDashboard = new Dashboard();
        private TabContainerDashboardItem destinationParentContainer;
        public Dashboard Dashboard { get; set; }
        public string TabContainerName { get; private set; }
        public string ScatterChartItemName { get; private set; }
        public string PivotItemName { get; private set; }

        public TabPageLoadHelper(string xmldashboardFile)
        {
            originalDashboard.LoadFromXml(Path.Combine(Environment.CurrentDirectory, xmldashboardFile));
        }

        public void PrepareDashboard()
        {
            Dashboard targetDashboard = new Dashboard();
            destinationParentContainer = new TabContainerDashboardItem();
            targetDashboard.Items.Add(destinationParentContainer);
            TabContainerName = destinationParentContainer.ComponentName;

            DashboardTabPage tabPage = CopyDashboardToTabPage(targetDashboard, originalDashboard);

            this.Dashboard = targetDashboard;
            DashboardTabPage tabPage1 = destinationParentContainer.CreateTabPage();
            targetDashboard.RebuildLayout();

            CopyTabPage(tabPage, tabPage1);

            ScatterChartItemName = targetDashboard.Items.Single(item => (item is ScatterChartDashboardItem) && (item.ParentContainer == tabPage1)).ComponentName;
            PivotItemName = targetDashboard.Items.Single(item => item is PivotDashboardItem && (item.ParentContainer == tabPage1)).ComponentName;

        }

        private DashboardTabPage CopyDashboardToTabPage(Dashboard targetDashboard, Dashboard originalDashboard)
        {
            DashboardTabPage tabPage = destinationParentContainer.CreateTabPage();
            DashboardLayoutTabPage layoutPage = new DashboardLayoutTabPage(tabPage);
            DashboardLayoutTabContainer layoutTabContainer = new DashboardLayoutTabContainer(destinationParentContainer);
            layoutTabContainer.ChildNodes.Add(layoutPage);

            IEnumerable<DashboardItem> itemsAndGroups = originalDashboard.Items.Union(originalDashboard.Groups).Where(item => !(item is TabContainerDashboardItem));
            foreach (DashboardItem item in itemsAndGroups)
            {
                if (item.ParentContainer == null)
                {
                    if (!(item is TabContainerDashboardItem))
                    {
                        targetDashboard.Items.Add(item);
                        item.ParentContainer = tabPage;
                    }
                }
            }

            DashboardLayoutGroup layoutRoot = originalDashboard.LayoutRoot;
            layoutPage.ChildNodes.Add(layoutRoot);
            targetDashboard.LayoutRoot = new DashboardLayoutGroup();
            targetDashboard.LayoutRoot.ChildNodes.Add(layoutTabContainer);
            return tabPage;
        }

        public void CopyTabPage(DashboardTabPage tabPageSource, DashboardTabPage tabPageDestination)
        {
            Dashboard targetDashboard = this.Dashboard;
            DashboardLayoutTabPage tabPageSourceLayout = tabPageSource.Dashboard.LayoutRoot.FindRecursive(tabPageSource);
            DashboardLayoutTabPage tabPageDestinationLayout = tabPageSource.Dashboard.LayoutRoot.FindRecursive(tabPageDestination);
            LayoutCopy(tabPageSourceLayout, tabPageDestinationLayout);
        }

        void LayoutCopy(DashboardLayoutNode copyFrom, DashboardLayoutNode copyTo)
        {
            copyTo.Weight = copyFrom.Weight;
            if ((copyTo is DashboardLayoutGroup) && (copyFrom is DashboardLayoutGroup))
                ((DashboardLayoutGroup)copyTo).Orientation = ((DashboardLayoutGroup)copyFrom).Orientation;
            if (copyFrom is DashboardLayoutGroup)
            {
                foreach (DashboardLayoutNode currentNode in ((DashboardLayoutGroup)copyFrom).ChildNodes)
                {
                    DashboardLayoutNode node = new DashboardLayoutItem();
                    DashboardItem item = currentNode.DashboardItem;
                    if (item != null) {
                        node.DashboardItem = CopyItemFromPageToPage(item, destinationParentContainer.TabPages[1]);
                        node.Weight = currentNode.Weight;
                    }
                    if (currentNode is DashboardLayoutGroup)
                    {
                        node = CopyTree(currentNode);
                    }
                    ((DashboardLayoutGroup)copyTo).ChildNodes.Add(node);
                }
            }
        }

        private DashboardItem CopyItemFromPageToPage(DashboardItem item, IDashboardItemContainer itemContainer)
        {
            DashboardItem itemCopy = item.CreateCopy();
            itemCopy.ComponentName = GetNameOnNextPage(item.ComponentName);
            itemCopy.Name = GetNameOnNextPage(item.Name);
            itemCopy.ParentContainer = itemContainer;
            Dashboard.Items.Add(itemCopy);
            return itemCopy;
        }

        private string GetNameOnNextPage(string name)
        {
            return name + "_1";
        }

        private DashboardLayoutNode CopyTree(DashboardLayoutNode currentNode)
        {
            DashboardLayoutGroup dashboardLayoutNode = new DashboardLayoutGroup();
            LayoutCopy(currentNode, dashboardLayoutNode);
            return dashboardLayoutNode;
        }
    }
}
