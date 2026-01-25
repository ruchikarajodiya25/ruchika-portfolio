# Missing Backend Endpoints for Work Order Items

## Current Status

**Backend Entity Exists:** ✅ `WorkOrderItem` entity exists in `ServiceHubPro.Domain.Entities`

**Backend API Endpoints:** ❌ **MISSING**

## What's Missing

### 1. **API Endpoints** (in `WorkOrdersController.cs`):
   - `POST /api/WorkOrders/{id}/items` - Add item to work order
   - `PUT /api/WorkOrders/{id}/items/{itemId}` - Update work order item
   - `DELETE /api/WorkOrders/{id}/items/{itemId}` - Remove item from work order
   - `GET /api/WorkOrders/{id}/items` - Get all items for a work order (optional, can be included in WorkOrderDto)

### 2. **DTOs** (in `ServiceHubPro.Application/DTOs/`):
   - `CreateWorkOrderItemDto.cs`:
     ```csharp
     public class CreateWorkOrderItemDto
     {
         public string ItemType { get; set; } // "Service", "Product", "Labor"
         public Guid? ServiceId { get; set; }
         public Guid? ProductId { get; set; }
         public string Description { get; set; }
         public decimal Quantity { get; set; }
         public decimal UnitPrice { get; set; }
         public decimal TaxRate { get; set; }
     }
     ```
   - `UpdateWorkOrderItemDto.cs` (similar structure)

### 3. **Commands/Queries** (in `ServiceHubPro.Application/Features/WorkOrders/`):
   - `Commands/AddWorkOrderItemCommand.cs`
   - `Commands/UpdateWorkOrderItemCommand.cs`
   - `Commands/RemoveWorkOrderItemCommand.cs`

### 4. **WorkOrderDto Enhancement**:
   - Add `List<WorkOrderItemDto> Items { get; set; }` to include items in responses
   - Update `GetWorkOrdersQuery` to include `.Include(w => w.Items)` when loading work orders

### 5. **Total Calculation**:
   - Backend should recalculate `WorkOrder.TotalAmount` when items are added/updated/removed
   - Formula: `TotalAmount = Sum(Items.Select(i => i.TotalAmount))` where `item.TotalAmount = (Quantity * UnitPrice) * (1 + TaxRate)`

## Temporary Frontend Solution

A **client-side only** solution has been implemented that:
- Stores items in React component state (`workOrderItems` Map)
- Calculates totals client-side
- Shows items in an expandable row
- **Note:** Items are lost on page refresh (not persisted to backend)

## Implementation Notes

The frontend currently:
1. ✅ Fetches services using `useServices()` hook
2. ✅ Shows "+ Item" button for each work order
3. ✅ Opens modal to add service items (service dropdown, quantity, unit price, tax rate)
4. ✅ Calculates totals: `(quantity * unitPrice) * (1 + taxRate)`
5. ✅ Shows computed total in the table (with original total in parentheses if different)
6. ✅ Expandable row to view all items
7. ✅ Remove item functionality
8. ⚠️ **Items are NOT saved to backend** - they exist only in browser memory

## Next Steps

To make this production-ready:
1. Implement backend endpoints listed above
2. Update frontend to call API endpoints instead of using local state
3. Remove client-side storage code
4. Backend will handle total calculation and persistence
