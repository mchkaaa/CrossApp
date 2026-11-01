using System.Collections.Generic;
using CrossApp.Core.Domain;

namespace CrossApp.Core;

public static class SampleData
{
    public static List<Order> Generate()
    {
        var list = new List<Order>
        {
            Order.Create("ORD-1001", "CUST-001", 1500m),
            Order.Create("ORD-1002", "CUST-002", 450m),
            Order.Create("ORD-1003", "CUST-001", 2300m),
            Order.Create("ORD-1004", "CUST-003", 150m),
            Order.Create("ORD-1005", "CUST-002", 900m),
            Order.Create("ORD-1006", "CUST-004", 1200m),
            Order.Create("ORD-1007", "CUST-005", 300m),
            Order.Create("ORD-1008", "CUST-003", 50m),
            Order.Create("ORD-1009", "CUST-001", 5000m),
            Order.Create("ORD-1010", "CUST-006", 1100m),
            Order.Create("ORD-1011", "CUST-002", 800m),
            Order.Create("ORD-1012", "CUST-004", 250m),
            Order.Create("ORD-1013", "CUST-005", 950m),
            Order.Create("ORD-1014", "CUST-006", 1750m),
            Order.Create("ORD-1015", "CUST-003", 400m),
            Order.Create("ORD-1016", "CUST-007", 200m),
            Order.Create("ORD-1017", "CUST-001", 60m),
            Order.Create("ORD-1018", "CUST-002", 3400m),
            Order.Create("ORD-1019", "CUST-005", 70m),
            Order.Create("ORD-1020", "CUST-007", 890m)
        };

        // Змінюємо статуси, щоб звіт report-groups мав різні групи
        list[0].ChangeStatus(OrderStatus.Confirmed);
        list[2].ChangeStatus(OrderStatus.Confirmed);
        list[4].ChangeStatus(OrderStatus.Cancelled);
        list[8].ChangeStatus(OrderStatus.Confirmed);
        list[11].ChangeStatus(OrderStatus.Cancelled);
        list[13].ChangeStatus(OrderStatus.Confirmed);
        list[17].ChangeStatus(OrderStatus.Confirmed);

        return list;
    }
}